# Architecture

> **Binding agent rules.** Read this page before touching a `LifetimeScope`, an installer, `SignalBus`,
> `SceneLoader` or any flow class.

## 1. Layers and assemblies

```
Assets/_Project/
├── Scripts/Core/      ColorBlockJam.Core     game-agnostic infrastructure
│   ├── Signals/         SignalBus
│   ├── Boot/            IBootStep, BootSequence
│   ├── Scenes/          SceneId, ISceneLoader, SceneLoader
│   ├── Persistence/     IKeyValueStorage, PlayerPrefsStorage, JsonRecord, progress, wallet, settings
│   ├── Logging/         IGameLogger, UnityGameLogger
│   ├── Clock/           IGameClock, RealtimeGameClock
│   ├── Lifecycle/       ApplicationPausedSignal, ApplicationPauseReporter (Root prefab component)
│   ├── Network/         IConnectivityService, ConnectivityService, ITrustedClock, TrustedClock, probe
│   └── Animation/       IAnimation<TConfig>
├── Scripts/Game/      ColorBlockJam.Game     this game
│   ├── Scopes/          Root / Splash / Home / Gameplay LifetimeScopes
│   ├── Installers/      CoreServicesInstaller, SplashInstaller, HomeInstaller, GameplayInstaller
│   ├── Configs/         Application, Splash, Ui, Gameplay, BoardView configs (ScriptableObjects)
│   ├── Navigation/      INavigationService, NavigationService
│   ├── Boot/            boot steps, TweeningInitializer
│   ├── Splash/          SplashFlow, SplashView
│   ├── Home/            HomeFlow, HomeView
│   ├── Levels/          LevelData, LevelDefinition, LevelCatalog, LevelProvider, validator
│   ├── UI/              safe area, button press feedback, popup service
│   ├── Gameplay/        GameplayFlow, signals, HUD, Board/ (rules, ActiveBoard), Input/ (touch, drag),
│   │                    View/ (board, blocks, tiling, camera), Animations/
│   └── Common/Animations/  reusable IAnimation structs
├── Scripts/Editor/    ColorBlockJam.Editor   editor tooling
├── Tests/EditMode/    ColorBlockJam.Tests.EditMode
├── Tests/PlayMode/    ColorBlockJam.Tests.PlayMode
├── Scenes/            Splash (build index 0), Home, Gameplay
├── Prefabs/           RootLifetimeScope, Board/ part prefabs
├── Levels/            LevelCatalog and level assets
└── Configs/           config assets
```

`Core` never references `Game`. Code moves into `Core` only when it is reusable by any game and has no
knowledge of this game's scenes, configs or rules.

## 2. Dependency injection (VContainer)

```
RootLifetimeScope (prefab, VContainerSettings, DontDestroyOnLoad)
├── SplashLifetimeScope   (Splash.unity)
├── HomeLifetimeScope     (Home.unity)
└── GameplayLifetimeScope (Gameplay.unity)
```

- **Root** is the prefab referenced by `Assets/_Project/Configs/VContainerSettings.asset`. VContainer
  instantiates it before the first scene loads, so any scene (including one opened directly in the
  editor) gets it as parent without a `parentReference`.
- **Scene scopes** live in their scene, own that scene's services and die with it.
- **Navigation**: flows change scenes only through `INavigationService` (Root). It loads with
  `CancellationToken.None`, because the load destroys the calling scope and cancels its token; a load
  listening to that token would report its own success as a cancellation. It ignores a request while a
  load is running (a double tap) (`NavigationServiceTests`).

### Registration rules

1. **Installers register plain services; scopes register entry points and scene components.**
   `CoreServicesInstaller`, `SplashInstaller`, `HomeInstaller` and `GameplayInstaller` are `IInstaller`s so
   `InstallerResolutionTests` can build
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
"ColorBlockJam/Configs/...")]`, stored in `Assets/_Project/Configs/`, referenced by the owning scope's
serialized field and registered with `RegisterInstance`. Consumers inject the config type. The one exception,
`ButtonPressFeedback` reading `UiConfig` directly, is explained in [ui.md](ui.md) §2.2.

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
