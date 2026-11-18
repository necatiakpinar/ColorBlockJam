# Splash and Boot

> **Binding agent rules.** Read this page before touching `Scripts/Game/Splash/`, `Scripts/Game/Boot/`,
> `Scripts/Core/Boot/`, `SceneLoader`, `SceneId` or the build scene list.

## 1. Guarantees

1. The app always starts in **Splash** (build index 0) — pinned by
   `BuildSettingsTests.BuildSettings_ShouldStartWithSplash_WhenAppLaunches`.
2. Every `SceneId` maps to an enabled build scene — `BuildSettings_ShouldContainEverySceneId_WhenLoaderNamesIt`.
3. **Splash always reaches Home**: no boot step may wait on the network or on user input without a
   bounded timeout.
4. All boot work is done by `IBootStep`s; `SplashFlow` only presents and routes.

## 2. Sequence

```
VContainer creates RootLifetimeScope (prefab)          TweeningInitializer: DOTween.Init + capacities
Splash.unity loads → SplashLifetimeScope builds        SplashView injected (SplashConfig)
SplashFlow.StartAsync
  ├─ SplashView.PlayIntro()                            logo fade + scale (FadeScaleInAnimation)
  ├─ start minimum-display timer                       SplashConfig.MinimumDisplaySeconds, realtime
  ├─ BootSequence.RunAsync(progress → SplashView)      steps in SplashInstaller order
  │    InitializeAssetsStep          (weight 1)        Addressables catalog (addressables.md §3)
  │    PreloadUiAtlasStep            (weight 1)        UI atlas, so Home draws at once (addressables.md §7)
  │    ApplyApplicationSettingsStep  (weight 1)        Application.targetFrameRate
  │    LoadLevelsStep                (weight 1)        level pack: stored remote or bundled (level-packs.md)
  │    LoadPlayerDataStep            (weight 2)        progress, wallet, settings .Load()
  │    LoadAudioStep                 (weight 1)        every cue's clip, addressable (audio.md)
  ├─ await minimum-display timer
  ├─ await SplashView.WaitForPresentationAsync         intro + last bar fill finished
  └─ INavigationService.GoToHomeAsync()                fade to black, single mode load, fade in; Splash scope is destroyed
Home.unity → HomeLifetimeScope builds                  HomeView wires its level button
HomeFlow.Start → HomeView.ShowLevel(level)             level button → GoToGameplayAsync
Gameplay.unity → GameplayLifetimeScope builds          GameplayHudView subscribes in Construct
GameplayFlow.Start → LevelStartedSignal(level)         HUD shows "Level N" with a reveal
```

## 3. Adding a boot step

1. Implement `IBootStep` in `Scripts/Game/Boot/` with a positive `Weight` (its share of the bar).
2. Register it in `SplashInstaller` **at the position it must run**.
3. Update `InstallerResolutionTests.SplashInstaller_ShouldRegisterBootStepsInExecutionOrder_WhenBuilt`
   and the table in section 2.
4. A step that can block (network, SDK init) wraps its wait in a timeout and continues on failure.

## 4. Scene handover detail

Scenes are loaded by `INavigationService`; why it ignores the calling scope's token is in
[architecture.md](architecture.md) §2.

## 5. Splash screen

The splash is kept plain (`SplashLayoutTests`):

| Part | Look |
| --- | --- |
| `Background` | `bg_home_screen`, envelope-fitted to the screen, the same art as Home |
| `SafeArea/Logo/Title` | "COLOR BLOCK" in white over a larger yellow "JAM", Outlined style; fades and scales in |
| `SafeArea/ProgressBar` | navy rounded track, 620x56, low on the screen |
| `SafeArea/ProgressBar/Fill` | green rounded fill pinned to the track's left edge; `BarFillAnimation` grows its right anchor from 0 to the boot progress, so the bar fills left to right |
| `SafeArea/LoadingLabel` | "Loading..." under the bar, Outlined style |

The splash draws only sprites outside the UI atlas (`bg_home_screen` and Unity's built-in `UISprite`): the
atlas is late-bound and loaded by a boot step, so it is not there yet ([addressables.md](addressables.md) §7).
