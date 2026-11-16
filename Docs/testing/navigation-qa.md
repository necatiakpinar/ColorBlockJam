# Splash → Gameplay QA

Verifies [splash-boot.md](../General/splash-boot.md) §1–§2. `⚙` = automated (test named), `👁` = manual.

## Test harness

- Reset progress: delete the `player_progress_v1` PlayerPrefs key (Edit > Clear All PlayerPrefs).
- Force a level: set `player_progress_v1` to `{"_currentLevel":7}`.
- Slow boot: raise `SplashConfig.MinimumDisplaySeconds`.

## Verified configuration

| Asset | Field | Value | Reference |
| --- | --- | --- | --- |
| `ApplicationConfig` | Target Frame Rate | 60 | CLAUDE.md *Project Overview* |
| `SplashConfig` | Minimum Display Seconds | 1.5 | DA-3 |
| Build Settings | scene 0 | Splash | splash-boot.md §1.1 |

## §1 Guarantees

- [ ] ⚙ Build starts in Splash — `BuildSettingsTests.BuildSettings_ShouldStartWithSplash_WhenAppLaunches`
- [ ] ⚙ Every scene id is a build scene — `BuildSettingsTests.BuildSettings_ShouldContainEverySceneId_WhenLoaderNamesIt`
- [ ] ⚙ Splash reaches Gameplay — `SplashToGameplayTests.Splash_ShouldReachGameplayAndShowLevel_WhenAppBoots`

## §2 Sequence

- [ ] 👁 The logo fades and scales in on launch; no pop or flicker.
- [ ] 👁 The loading bar fills smoothly and is full before the scene changes.
- [ ] 👁 Splash stays at least `MinimumDisplaySeconds` on a fast device.
- [ ] ⚙ Gameplay HUD shows "Level N" for the saved level — `GameplayFlowTests.Start_ShouldFireLevelStartedWithCurrentLevel_WhenSceneStarts`
- [ ] 👁 The HUD reveals with a short fade and scale.
- [ ] 👁 Pressing Play with Gameplay open in the editor still boots through Splash.

## Residual gaps

- Device runs (Android/iOS) not yet performed.
