# Architecture

> **Binding agent rules.** Read this page before touching a `LifetimeScope`, an installer, `SignalBus`,
> `SceneLoader` or any flow class.

## 1. Layers and assemblies

```
Assets/_Project/
├── Scripts/Core/      RollicCase.Core     game-agnostic infrastructure
│   ├── Signals/         SignalBus
│   ├── Boot/            IBootStep, BootSequence
│   ├── Scenes/          SceneId, ISceneLoader, SceneLoader
│   ├── Persistence/     IKeyValueStorage, PlayerPrefsStorage, IPlayerProgressService
│   ├── Logging/         IGameLogger, UnityGameLogger
│   └── Animation/       IAnimation<TConfig>
├── Scripts/Game/      RollicCase.Game     this game
│   ├── Scopes/          Root / Splash / Gameplay LifetimeScopes
│   ├── Installers/      CoreServicesInstaller, SplashInstaller
│   ├── Configs/         ApplicationConfig, SplashConfig, GameplayConfig (ScriptableObjects)
│   ├── Boot/            boot steps, TweeningInitializer
│   ├── Splash/          SplashFlow, SplashView
│   ├── Gameplay/        GameplayFlow, signals, HUD
│   └── Common/Animations/  reusable IAnimation structs
├── Scripts/Editor/    RollicCase.Editor   editor tooling
├── Tests/EditMode/    RollicCase.Tests.EditMode
├── Tests/PlayMode/    RollicCase.Tests.PlayMode
├── Scenes/            Splash (build index 0), Gameplay
├── Prefabs/           RootLifetimeScope
└── Configs/           config assets
```

`Core` never references `Game`. Code moves into `Core` only when it is reusable by any game and has no
knowledge of this game's scenes, configs or rules.

## 2. Dependency injection (VContainer)

```
RootLifetimeScope (prefab, VContainerSettings, DontDestroyOnLoad)
├── SplashLifetimeScope   (Splash.unity)
└── GameplayLifetimeScope (Gameplay.unity)
```

- **Root** is the prefab referenced by `Assets/_Project/Configs/VContainerSettings.asset`. VContainer
  instantiates it before the first scene loads, so any scene (including one opened directly in the
  editor) gets it as parent without a `parentReference`.
- **Scene scopes** live in their scene, own that scene's services and die with it.

### Registration rules

1. **Installers register plain services; scopes register entry points and scene components.**
   `CoreServicesInstaller` and `SplashInstaller` are `IInstaller`s so `InstallerResolutionTests` can build
   them into a bare `ContainerBuilder` without scenes or the player loop. A scope's `Configure` runs its
   installer, then `RegisterComponent` for serialized scene objects, then `RegisterEntryPoint` for flows.
2. **Constructor injection** for plain classes. **`[Inject] public void Construct(...)`** for
   MonoBehaviours. No field or property injection.
3. **Entry points** implement a VContainer lifecycle interface (`IInitializable`, `IStartable`,
   `IAsyncStartable`). A class that only listens is **not** an entry point: register it as a singleton
   and resolve it in the scope's `RegisterBuildCallback` so it exists before any entry point runs.
4. Collections: register several implementations of one interface (`IBootStep`) and inject
   `IReadOnlyList<T>`; registration order is resolution order.
5. Never: a service locator, `static` singletons, `FindObjectOfType`/`FindFirstObjectByType`,
   `Camera.main`, `Resources.Load` for configs.

## 3. Signals

`SignalBus` is registered once in Root. Signals are `readonly struct`s named `<Event>Signal`
(`LevelStartedSignal`).

- **Subscribe at construction**: in the constructor (plain classes) or `[Inject] Construct`
  (components). Unsubscribe in `Dispose` or `OnDestroy`.
- Components injected by `RegisterComponent` are constructed while the container builds, i.e. **before
  any entry point's `Start`**. That is what lets `GameplayHudView` receive the `LevelStartedSignal` fired
  by `GameplayFlow.Start`.
- Handlers run synchronously in subscription order. Subscribing during a fire takes effect on the next
  fire; unsubscribing during a fire is safe (`SignalBusTests`).
- Fire a signal for a fact other systems react to. Call a method when one owner needs one thing done.

## 4. Flows, services and views

| Kind | Owns | Must not |
| --- | --- | --- |
| **Service** (`PlayerProgressService`, boot steps) | state, persistence, rules | touch views or tweens |
| **Flow** (`SplashFlow`, `GameplayFlow`) | ordering of presentation and routing | mutate data or decide domain rules |
| **View** (`SplashView`, `GameplayHudView`) | binding authored UI, owning running sequences | contain motion code, call services that mutate data |
| **Animation** (`FadeScaleInAnimation`) | building one DOTween sequence from a config | hold state, be a MonoBehaviour |

Data is committed by its service **before** anything presents it; a game state must be correct even if
every animation were skipped.

## 5. Configs

Tunables are `ScriptableObject`s named `<Area>Config` with `[CreateAssetMenu(menuName =
"RollicCase/Configs/...")]`, stored in `Assets/_Project/Configs/`, referenced by the owning scope's
serialized field and registered with `RegisterInstance`. Consumers inject the config type.

## 6. Animations (DOTween)

- `IAnimation<TConfig>` (Core) — `Sequence Play(in TConfig config)`.
- Implementations are `readonly struct`s with a nested `readonly struct Config` carrying the target(s)
  and a serializable settings struct (`FadeScaleInSettings`) coming from a config asset.
- Every sequence is `SetUpdate(true)` (unscaled) and `SetLink(target)`. The view stores the returned
  sequence, kills it before replaying and in `OnDestroy`.
- `TweeningInitializer` (Root entry point) calls `DOTween.Init` with capacities from
  `ApplicationConfig` before any scene can start a tween.
- Awaiting: UniTask's DOTween bridge (`UNITASK_DOTWEEN_SUPPORT` define, `UniTask.DOTween` assembly) —
  `await sequence.ToUniTask(cancellationToken: token)`.
