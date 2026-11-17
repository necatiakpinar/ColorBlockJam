# Design Tasks — Detail

Subtasks, acceptance criteria, tests and docs for every task in [README.md §4](README.md#4-tasks). The
README table owns status; this page owns scope. Each task block lists:

- **Req** — requirements it satisfies (`R-n`) and assumptions it implements (`DA-n`).
- **Arch** — scope, services, signals, configs and views it adds, following
  [architecture.md](../General/architecture.md).
- **Subtasks** — the checklist a branch works through.
- **Accept** — observable outcomes a player sees or a test asserts.
- **Tests** — Edit Mode (`EM`) and Play Mode (`PM`) tests it adds. Manual checks go in the named QA page.
- **Docs** — pages it creates or edits.

Art assets referenced below live in `Assets/_Project/Art/` (`UI/`, `Models/`, `Textures/`).

## Progress

Tick a task when its acceptance criteria hold and its tests are green; tick subtasks as they land.

- [x] [T-01 Pin the project to Unity 2022.3.62f2](#t-01-pin-the-project-to-unity-2022362f2)
- [x] [T-02 Player, Android and quality settings](#t-02-player-android-and-quality-settings)
- [x] [T-03 Import and configure the art assets](#t-03-import-and-configure-the-art-assets)
- [x] [T-04 Home scene and scene routing](#t-04-home-scene-and-scene-routing)
- [x] [T-05 UI kit: safe area, button feedback, popup service](#t-05-ui-kit-safe-area-button-feedback-popup-service)
- [x] [T-06 Board input and board space](#t-06-board-input-and-board-space)
- [ ] [T-07 Persistence: wallet, settings, progress](#t-07-persistence-wallet-settings-progress)
- [x] [T-08 Level data format and catalog](#t-08-level-data-format-and-catalog)
- [x] [T-09 Board model (pure rules)](#t-09-board-model-pure-rules)
- [x] [T-10 Board view: floor, walls, doors, camera fit](#t-10-board-view-floor-walls-doors-camera-fit)
- [x] [T-11 Block view: modular block meshes and colours](#t-11-block-view-modular-block-meshes-and-colours)
- [x] [T-12 Drag and slide](#t-12-drag-and-slide)
- [x] [T-13 Exit through door and level complete](#t-13-exit-through-door-and-level-complete)
- [ ] [T-14 Level session state machine](#t-14-level-session-state-machine)
- [ ] [T-15 Countdown timer](#t-15-countdown-timer)
- [ ] [T-16 Coin rewards](#t-16-coin-rewards)
- [ ] [T-17 Level progression and looping](#t-17-level-progression-and-looping)
- [ ] [T-18 Gameplay HUD](#t-18-gameplay-hud)
- [ ] [T-19 Pause popup](#t-19-pause-popup)
- [ ] [T-20 Fail popup](#t-20-fail-popup)
- [ ] [T-21 Win popup](#t-21-win-popup)
- [ ] [T-22 Restart and home correctness](#t-22-restart-and-home-correctness)
- [ ] [T-23 Home screen layout](#t-23-home-screen-layout)
- [ ] [T-24 Home behaviors](#t-24-home-behaviors)
- [ ] [T-25 Settings popup](#t-25-settings-popup)
- [ ] [T-26 Level editor window and level list](#t-26-level-editor-window-and-level-list)
- [ ] [T-27 Visual board canvas, size and timer](#t-27-visual-board-canvas-size-and-timer)
- [ ] [T-28 Block placement tools](#t-28-block-placement-tools)
- [ ] [T-29 Door placement tools](#t-29-door-placement-tools)
- [ ] [T-30 Save, load, play-test and structural validation](#t-30-save-load-play-test-and-structural-validation)
- [ ] [T-31 Editor usability pass and designer guide](#t-31-editor-usability-pass-and-designer-guide)
- [ ] [T-32 Author levels 1–5 in the editor](#t-32-author-levels-15-in-the-editor)
- [ ] [T-33 (could) Solvability validation](#t-33-could-solvability-validation)
- [ ] [T-34 (could) Ice blocker](#t-34-could-ice-blocker)
- [ ] [T-35 (could) Arrow block](#t-35-could-arrow-block)
- [ ] [T-36 (could) One functional booster](#t-36-could-one-functional-booster)
- [ ] [T-37 Architecture tests](#t-37-architecture-tests)
- [ ] [T-38 Performance pass](#t-38-performance-pass)
- [ ] [T-39 Full QA pass on device and aspect ratios](#t-39-full-qa-pass-on-device-and-aspect-ratios)
- [ ] [T-40 Project README](#t-40-project-readme)
- [ ] [T-41 Android APK](#t-41-android-apk)
- [ ] [T-42 Repository hygiene](#t-42-repository-hygiene)

---

## Chapter 0 — Project baseline

### T-01 Pin the project to Unity 2022.3.62f2

- **Req:** R-1.
- **Subtasks:**
  - [x] Package set for 2022.3: URP 14, Input System 1.14, Test Framework 1.1, TextMesh Pro 3.0, uGUI 1.0.
  - [x] Every asset serialized by 2022.3.62f2.
  - [x] Play From Splash exempts test runs and batch mode.
- **Accept:** project opens in 2022.3.62f2 with an empty console; Play from any scene reaches Gameplay;
  all tests green.
- **Tests:** existing suites pass.
- **Docs:** `CLAUDE.md` + `AGENTS.md`, `README.md`, `getting-started.md`.

### T-02 Player, Android and quality settings

- **Req:** R-2, R-7, R-38. Implements DA-1.
- **Subtasks:**
  - [x] Company, product name, bundle id `com.necatiakpinar.colorblockjam`, version `0.1.0`, bundle code 1.
  - [x] Default orientation Portrait; disable auto-rotation to landscape.
  - [x] Android: IL2CPP, ARM64 only, min API 24, target API = highest installed, `Optimized Frame Pacing`.
  - [x] Quality: one Mobile level pointing at the mobile URP asset; delete `PC_RPAsset` / `PC_Renderer`.
  - [x] Delete the unused template `InputSystem_Actions.inputactions` and its project-wide actions entry.
  - [x] Delete the unused `DefaultVolumeProfile`.
  - [x] DOTween safe mode off in editor and development builds (`TweeningInitializer`).
  - [ ] Build a development APK and launch it on a device (first build happens with T-41 if no device
    is at hand earlier).
- **Accept:** a development APK builds and launches portrait on a device.
- **Tests:** `EM BuildSettingsTests.PlayerSettings_ShouldLockPortrait_WhenBuilding`,
  `EM BuildSettingsTests.PlayerSettings_ShouldBuildIl2CppArm64_WhenTargetingAndroid`.
- **Docs:** `getting-started.md` (build steps).

### T-03 Import and configure the art assets

- **Req:** R-3, R-17, R-40.
- **Subtasks:**
  - [x] `UI/*.png`: Texture Type `Sprite (2D and UI)`, single sprite, mipmaps off, `Clamp`, compression
    ASTC for Android, max size fitted to the source.
  - [x] Nine-slice borders for stretchable sprites (list in `art-pipeline.md` §2); `Btn_levelBg_normal`
    is not sliceable and stays `Simple`.
  - [x] A Sprite Atlas for the UI folder (one draw-call group for HUD and popups).
  - [x] `Models/*.fbx`: scale 0.5 so one cell is 1 unit; no materials, animation, cameras, lights or
    colliders; read/write off.
  - [x] Part prefabs under `Assets/_Project/Prefabs/Board/` for floor, wall, corners, door, door arrow,
    block quarter parts and arrows (table in `art-pipeline.md` §4).
  - [x] URP Simple Lit materials: block, floor, wall, door, arrow, ice (`icetexture.jpg`).
  - [x] Log asset problems for the README (R-40) in [asset-feedback.md](asset-feedback.md).
- **Accept:** every UI sprite drops onto an `Image` correctly; every mesh prefab sits on a 1-unit grid.
- **Tests:** `EM AssetImportTests`: UI textures are sprites without mipmaps; models import at cell scale
  without materials; the floor tile spans one cell.
- **Docs:** `Docs/General/art-pipeline.md` (folders, import rules, scale, palette); `Docs/README.md`.

---

## Chapter 1 — Foundations

### T-04 Home scene and scene routing

- **Req:** R-10, R-16, R-20, R-21.
- **Arch:** new `Home.unity` with `HomeLifetimeScope` (child of Root), `HomeInstaller`, `HomeFlow` entry
  point. `SceneId` gains `Home`. `SplashFlow` loads Home instead of Gameplay. `INavigationService` (Root)
  exposes `GoToHomeAsync()`, `GoToGameplayAsync()` over `ISceneLoader`, so views never know scene names.
- **Subtasks:**
  - [x] Create the scene, scope, installer and `HomeFlow` (shows the level, routes the level button).
  - [x] Add `Home` to build settings (index 1) and `SceneId`.
  - [x] Route Splash → Home; Gameplay placeholder "Home" button → Home.
  - [x] `PlayModeStartScene` needs no change (it always uses build scene 0).
- **Accept:** Play boots Splash → Home; Home → Gameplay → Home round-trips without errors or duplicate
  Root scopes.
- **Tests:** `EM InstallerResolutionTests` covers `HomeInstaller` and `GameplayInstaller`;
  `EM BuildSettingsTests` covers the new id; `EM HomeFlowTests`, `EM NavigationServiceTests`,
  `EM GameplayFlowTests`; `PM NavigationFlowTests` (boot to Home, Home ⇄ Gameplay).
- **Docs:** `architecture.md` §1–§2, `splash-boot.md` (handover target), `navigation-qa.md`.

### T-05 UI kit: safe area, button feedback, popup service

- **Req:** R-2, R-12, R-13, R-14.
- **Arch:**
  - `SafeAreaFitter` component on each canvas content root; re-applies on rect change
    (`OnRectTransformDimensionsChange`), never per frame.
  - `ButtonPressFeedback` component on every `Button`: `IPointerDown/Up` handlers run a
    `ScaleToAnimation`, timings from `UiConfig.PressFeedback`. Disabled buttons give no feedback.
  - `IPopupService` per scene scope: opens an authored popup view (already in the scene, disabled),
    one at a time, a full-screen blocker behind it.
  - Canvases stay Screen Space – Overlay with `CanvasScaler` 1080x1920, match 0.5; `SceneSetupTests`
    enforces the scaler instead of a template prefab.
- **Subtasks:**
  - [x] `SafeArea` + `SafeAreaFitter`; a `SafeArea` root in every canvas.
  - [x] `ButtonPressFeedback` + `ScaleToAnimation`, timings in `UiConfig` (Root).
  - [x] `IPopupService` / `PopupService` per scene; the shared popup view lands with the first popup
    (T-19, T-25).
  - [x] `SceneSetupTests` enforce scaler, safe area and button feedback in every build scene.
- **Accept:** any button scales down on press and back on release; opening a second popup while one is open
  is refused; UI stays inside the safe area at 1080x1920, 1080x2400 (20:9) and an iPhone notch preset.
- **Tests:** `EM SafeAreaTests`, `EM PopupServiceTests`, `EM SceneSetupTests`,
  `PM ButtonPressFeedbackTests`.
- **Docs:** `Docs/General/ui.md` (canvas rules, feedback, popups, safe area); `Docs/testing/ui-qa.md`.

### T-06 Board input and board space

- **Req:** R-17, R-38.
- **Arch:** the scene camera is registered with the scope and injected as `Camera` (replaces
  `Camera.main`; a wrapper service would add nothing). `BoardInputView` on a board-sized collider receives
  `IPointerDownHandler`, `IDragHandler`, `IPointerUpHandler` through a `PhysicsRaycaster`, projects the
  pointer onto the board plane and fires `BoardPointerSignal`s. Pointers over UI never reach the board.
  Single touch only (DA-4). `BoardSpace` is the only cell ↔ world conversion; it reads
  `BoardViewConfig.CellSize` (1 with the current import scale) ([Design README §2.8](README.md#2-scope-decisions)).
- **Subtasks:**
  - [x] `BoardViewConfig`, `BoardSpace` registered by `GameplayInstaller`.
  - [x] `BoardInputView` + `BoardPointerSignal`; camera `PhysicsRaycaster`; Gameplay scope wiring.
  - [x] Edit Mode `BoardSpaceTests`, Play Mode `BoardInputTests`.
- **Accept:** pressing, dragging and releasing over board cells reports correct cell coordinates at any
  aspect ratio; touches on HUD buttons never move blocks.
- **Tests:** `EM BoardSpaceTests` (world ↔ cell round-trip at cell size 1 and 2, out-of-board),
  `PM BoardInputTests` (cells in order, second finger ignored, disable mid-touch, EventSystem raycast
  hits the board and the HUD wins over it).
- **Docs:** `Docs/General/gameplay.md` §1–§2 (new page, grows in chapters 2–5).

### T-07 Persistence: wallet, settings, progress

- **Req:** R-19, R-15, R-24. Implements DA-7, DA-10.
- **Arch:** Root-scoped `IWalletService` (coins: `Balance`, `Add`, `CoinsChangedSignal`),
  `ISettingsService` (music, sound, vibration booleans, `SettingChangedSignal`), existing
  `IPlayerProgressService` (current level). All through `IKeyValueStorage`, versioned keys. An
  `AppLifecycleObserver` saves on `OnApplicationPause(true)` / focus loss.
- **Accept:** coins, toggles and level survive app kill and restart; background/foreground keeps them.
- **Tests:** `EM WalletServiceTests` (add, negative rejected, persists, signal fired once),
  `EM SettingsServiceTests`, `EM` lifecycle save with a fake storage.
- **Docs:** `Docs/General/persistence.md` (keys, versions, what is saved when).

---

## Chapter 2 — Level data and board model

### T-08 Level data format and catalog

- **Req:** R-17, R-26, R-29. Implements DA-13, DA-17, DA-18.
- **Arch:** `LevelDefinition : ScriptableObject` wraps a plain serializable `LevelData` — `Width`,
  `Height` (bounds in `LevelRulesConfig`, 3–10), `TimerSeconds`, `BlockData` (`BlockColor`, absolute start
  `Cells`), `DoorData` (`WallSide`, `Start`, `Length`, `BlockColor`). Ice/arrow modifiers are added by
  T-34/T-35, not reserved now. `LevelCatalog : ScriptableObject` — ordered list, registered in Root. `LevelDefinitionValidator` (pure) returns `LevelIssue`s (error / warning).
- **Subtasks:**
  - [x] Data types, enums with spaced values, `LevelDefinition`, `LevelCatalog`, `LevelRulesConfig`.
  - [x] Validator with the ten rules of level-data.md §2.
  - [x] `LevelCatalog.asset` and `LevelRulesConfig.asset`.
- **Accept:** level data round-trips through Unity serialization of the asset type; the validator reports
  each broken invariant with a readable message.
- **Tests:** `EM LevelDefinitionValidatorTests` (one test per rule), `EM LevelDefinitionTests`
  (serialization round trip); the catalog is covered through `LevelProviderTests`.
- **Docs:** `Docs/General/level-data.md` (format, invariants); `level-editor-qa.md`.

### T-09 Board model (pure rules)

- **Req:** R-17, R-24, R-25, R-35. Implements DA-4, DA-5.
- **Arch:** plain C# in `ColorBlockJam.Game.Gameplay.Board`: `BoardState` built fresh from `LevelData` per
  attempt (occupancy grid `int[]`, `BoardBlock` list); `TryStep(blockId, direction)` moves one cell and
  returns `Moved` / `Blocked` / `Exited` (the drag controller asks `CanStep` and commits one cell at a time, so no separate
  `MaxSteps`); `DoorResolver` answers colour and span fit; the exit also needs a clear straight path to the
  wall (DA-5). `BoardState.IsCleared`. No allocation per step.
- **Subtasks:**
  - [x] `BoardState`, `BoardBlock`, `DoorResolver`, `StepResult`.
  - [x] Exit path check for shapes whose rows do not all touch the wall (L, T, S).
- **Accept:** every rule below is a passing test:
  slide stops at walls; slide stops at other blocks; block exits through a matching door wide enough;
  wrong colour does not exit; too-narrow door does not exit; removing the last block clears the board;
  building a second `BoardState` from the same definition shares no state with the first.
- **Tests:** `EM BoardStateTests` (covers the door fit through `TryStep` on every wall), including a
  zero-allocation test over 5 000 steps covering door and path checks (`Is.Not.AllocatingGCMemory`).
- **Docs:** `gameplay.md` §3; DA-5 gains the clear-path condition.

---

## Chapter 3 — Core gameplay (playable, ugly allowed)

### T-10 Board view: floor, walls, doors, camera fit

- **Req:** R-17, R-13.
- **Arch:** `BoardView` (Gameplay scene component) builds the board from pooled part prefabs: base, floor
  tiles, wall segments (one per stretch between doors) and corners, doors (one per door, tinted by colour). `BoardCameraView` +
  pure `BoardFraming` frame the board for its size and the aspect, with margins from `BoardViewConfig`.
  Parts hang under a root at `BoardSpace.Origin` scaled by the cell size, so no cell-size literal in view
  code. `LevelProvider` + `ActiveBoard` load the current level and announce it (`BoardLoadedSignal`,
  which carries the level, not the live board).
- **Subtasks:**
  - [x] Wall/door prefabs flipped (exported upside down, A-8); `Board_Base` prefab and material.
  - [x] `BoardView`, `PartPool`, `ColorMaterialCache`, `BoardCameraView`, `BoardFraming`.
  - [x] `LevelProvider`, `ActiveBoard`, catalog in the root scope, development level `Level_001`.
- **Accept:** boards from 3x3 to the max size are fully visible and centred at 1080x1920 and 20:9; doors sit
  on the right wall cells in the right colour.
- **Tests:** `EM BoardFramingTests`, `EM LevelProviderTests`, `EM ActiveBoardTests`,
  `PM GameplayBoardTests.Board_ShouldDrawEveryPartAndBlock_WhenGameplayStarts` (part counts).
- **Docs:** `gameplay.md` §4–§5, `art-pipeline.md` §3–§5, `level-data.md` §3, asset feedback A-8, A-9.

### T-11 Block view: modular block meshes and colours

- **Req:** R-17.
- **Arch:** `BlockLayerView` assembles each block under a pooled `BlockView` from `ModularBlock_*` parts.
  A pure `BlockTileResolver` picks, per cell quarter, Center / Edge / OuterCorner from the two neighbours
  toward that corner, and one InnerCorner where three cells meet at a vertex; mirroring by negative scale.
  Colour from the `BoardViewConfig` palette through one shared material per colour (SRP batcher friendly,
  unlike property blocks). Block views and parts are pooled.
- **Accept:** I, L, T, S, square and 1x1 shapes render with seamless edges and correct corners in every
  colour.
- **Tests:** `EM BlockTileResolverTests` (pieces for each shape, notch direction, full coverage).
- **Docs:** `gameplay.md` §5, `art-pipeline.md` §4.

### T-12 Drag and slide

- **Req:** R-17, R-38. Implements DA-4.
- **Arch:** `BlockDragController` (`Input/`, plain class, Gameplay scope) listens to board input: press
  selects the block under the finger; the block follows the finger smoothly in fractional cells, asking
  `BoardState.CanStep` before entering each cell and committing through `ActiveBoard.TryStep` past the
  middle of it; release snaps it to its cell (`BlockDraggedSignal`, `BlockDroppedSignal`, one retargeted
  `BlockMoveAnimation` snap tween per block).
  Release ends the drag. Signal `BlockMovedSignal`. Ignoring input outside `Playing` lands with the
  session state machine (T-14).
- **Accept:** a held block follows the finger smoothly, also when dragged fast across free space, never
  overlaps or crosses walls, changes axis only through free cells; releasing snaps it to the nearest cell
  with a tween; no GC per drag frame.
- **Tests:** `EM BlockDragControllerTests` with signals and a real board (including zero allocation),
  `PM GameplayBoardTests.BlockView_ShouldNotAllocate_WhenFollowingAFingerAndSnapping` (one retargeted tween).
- **Docs:** `gameplay.md` §4; `gameplay-qa.md` cases.

### T-13 Exit through door and level complete

- **Req:** R-17, R-24.
- **Arch:** when `BoardState.TryStep` returns `Exited` (a push past the wall through a fitting door with a
  clear path, DA-5), `ActiveBoard` fires `BlockExitedSignal`; `BlockView` plays
  `BlockExitAnimation` (slide through the door at full size, cut at the board's edge, debris spray), then
  returns to the pool. The last exit fires
  `BoardClearedSignal` once; the session (T-14) turns it into level complete.
- **Accept:** a block leaves through its door; a wrong-colour or narrow door blocks it; the last exit
  triggers level complete exactly once.
- **Tests:** `EM` model exit tests (T-09), `EM BlockDragControllerTests` (exit, clear once),
  `PM GameplayBoardTests` (swipes on a two-block level in the real scene).
- **Docs:** `gameplay.md` §3–§5.

### T-43 Touch and door feel

- **Req:** R-42. Implements DA-21.
- **Arch:** `BlockDragController` picks the nearest block within `TouchTolerance` on a missed press; a
  door the block cannot use stays a plain wall (a shake and haptic there were tried and dropped, DA-21).
  `BoardView` opens the door segments a
  leaving block passes (`DoorGateAnimation`). `BlockLayerView` folds exits through one door within
  `ExitBurstCoalesceWindow` into one `ExitBurstView`. `ExitPath` is the shared exit geometry.
- **Subtasks:**
  - [x] Near-miss press.
  - [x] ~~Rejection signal, shake and haptic at a wrong door~~ (dropped: a wrong door is a wall).
  - [x] Door gate, spray coalescing, `ExitPath`.
  - [x] Haptics: own `IHapticService` (Android `VibrationEffect`, iOS `UIImpactFeedbackGenerator`), light
    tap on exit (`GameplayHaptics`). Settings toggle gating lands with T-25.
- **Accept:** a press 0.25 cell next to a block picks it; a wrong door stops a block like a wall; doors
  sink open and close for a leaving block; two blocks leaving one door together make one spray.
- **Tests:** `EM BlockDragControllerTests` (near miss, wrong door is a wall), `EM ExitPathTests`,
  `EM GameplayHapticsTests`, `PM GameplayBoardTests` (door gate, shared spray).
- **Docs:** `gameplay.md` §4–§5, `art-pipeline.md` §4, `testing.md`, `gameplay-qa.md`.

---

## Chapter 4 — Level session rules

### T-14 Level session state machine

- **Req:** R-18, R-20, R-21, R-24, R-25.
- **Arch:** `LevelSession` (Gameplay scope; a concrete class: one implementation, pure and tested
  directly, so no interface) owns `LevelState` (`Loading`, `Playing`, `Paused`, `Won`, `Failed`) and
  `LevelNumber`, with `Start(n)`, `Pause`, `Resume`, `Restart`, `Fail`; it turns `BoardClearedSignal` into
  `Won` itself. Each attempt gets a new `BoardState`; the drag controller drops its block and ignores input
  outside `Playing` (DA-22). Signals: `LevelStartedSignal` (exists), `LevelPausedSignal`,
  `LevelResumedSignal`, `LevelCompletedSignal`, `LevelFailedSignal`. `GameplayFlow` only starts the session
  and routes, per *Flows are orchestration only*.
- **Subtasks:**
  - [x] `LevelSession`, `LevelState`, the four signals; `GameplayFlow` starts the session.
  - [x] Drag input gated on the level state.
- **Accept:** illegal transitions are rejected (e.g. Pause after Won); restart yields a state equal to a
  fresh start.
- **Tests:** `EM LevelSessionTests` (transition table, refusals, signal order, restart equality),
  `EM GameplayFlowTests` (starts the session), `EM BlockDragControllerTests` (drop on pause, ignore input).
- **Docs:** `gameplay.md` §6 (state diagram).

### T-15 Countdown timer

- **Req:** R-18. Implements DA-6.
- **Arch:** `LevelTimer` (plain, Gameplay scope) with `IGameClock` seam (fake in tests). Remaining time from
  `LevelDefinition.TimerSeconds`; ticks via `UniTask.Delay` on unscaled time with the session token, not
  `Update`. Pause/resume and app background (`OnApplicationPause`) freeze it. At zero: `Fail` once.
  `TimerChangedSignal` once per displayed second.
- **Accept:** HUD counts down `mm:ss`; pause and background freeze it; at 0 input stops and the fail popup
  opens; restart restores the full time.
- **Tests:** `EM LevelTimerTests` with a fake clock (countdown, pause, expiry once, restart).
- **Docs:** `gameplay.md` §Timer.

### T-16 Coin rewards

- **Req:** R-19. Implements DA-7, DA-14.
- **Arch:** on `Complete`, `LevelRewardService` adds `GameplayConfig.CoinsPerWin` to `IWalletService`
  before the win popup appears; idempotent per attempt. Fail, restart and Home never touch the wallet.
- **Accept:** coins rise by the reward once per win; leaving a level mid-play and returning shows the same
  balance as before entering; app restart keeps the balance.
- **Tests:** `EM LevelRewardServiceTests`; `PM CoinPersistenceTests` (enter, leave, return, compare).
- **Docs:** `gameplay.md` §Economy.

### T-17 Level progression and looping

- **Req:** R-10, R-24. Implements DA-2, DA-8.
- **Arch:** on `Complete`, `IPlayerProgressService` advances the level number before any presentation.
  `LevelProvider` maps level number → `LevelData` (`(n - 1) % catalog.Count`) — done with T-10.
- **Subtasks:**
  - [x] `LevelProvider` with looping (`LevelProviderTests`).
  - [x] Advance the level on win before the win popup (`LevelProgression`; `GameplayFlow` starts the
    next level after `NextLevelDelay` until T-21, DA-23).
- **Accept:** winning level 3 makes Home show "Level 4"; after level 5, level 6 plays level 1's layout.
- **Tests:** `EM LevelProviderTests` (mapping, loop), `EM LevelProgressionTests`,
  `EM LevelSessionTests.Next_*`, `PM LevelProgressionFlowTests` (a full lap of the catalog).
- **Docs:** `gameplay.md` §Progression.

---

## Chapter 5 — Gameplay UI

### T-18 Gameplay HUD

- **Req:** R-19, R-22, R-23, R-18. Implements DA-9, DA-16.
- **Arch:** authored HUD in the Gameplay canvas: level label, timer holder (`bg_gameplay_time_holder`,
  `ic_time`), coin counter (`Coin_Single`), pause button (`ic_pause`), booster bar
  (`Btn_booster_normal` with `Hammer`, `Vacuum`, `Rocket_Purple_Icon`, `Clock_Freeze`), lives placeholder
  (`Heart`). `GameplayHudView` binds signals; text via `TMP_Text.SetText(format, value)`.
- **Accept:** HUD values update live without per-frame code; booster buttons give press feedback and do
  nothing else; layout fits 20:9 and the safe area.
- **Tests:** `PM GameplayHudTests` (timer and coin text after signals).
- **Docs:** `ui.md` §Gameplay HUD; `gameplay-qa.md`.

### T-19 Pause popup

- **Req:** R-21. Implements DA-15.
- **Arch:** `PausePopupView` (authored, `bg_popup`, `bg_popup_header`, `Popup_Close_Button`): Resume, Restart
  (`ic_restart`), Home (`home_icon`). Calls the session / navigation; owns no rules.
- **Accept:** opening pauses timer and board input; Resume continues from the same second; Restart and Home
  work from the popup.
- **Tests:** `PM PauseFlowTests`.
- **Docs:** `ui.md` §Popups; `gameplay-qa.md`.

### T-20 Fail popup

- **Req:** R-20. Implements DA-15.
- **Arch:** `FailPopupView`: "Time's up" header, Restart (`Btn_square_retry`), Home. Opens on
  `LevelFailedSignal`.
- **Accept:** opens at timer zero; Restart replays the same level from its start state; Home returns to Home
  with the same level number.
- **Tests:** `PM FailFlowTests` (fake clock forces expiry).
- **Docs:** `ui.md`; `gameplay-qa.md`.

### T-21 Win popup

- **Req:** R-24, R-19. Implements DA-12.
- **Arch:** `WinPopupView`: level cleared header, reward amount (`Coin_Single`), Continue
  (`Btn_popup_green`). Opens on `LevelCompletedSignal` after the last exit animation.
- **Accept:** shows the reward already credited; Continue loads the next level number.
- **Tests:** `PM WinFlowTests`.
- **Docs:** `ui.md`; `gameplay-qa.md`.

### T-22 Restart and home correctness

- **Req:** R-21, R-25.
- **Subtasks:**
  - [ ] Kill every board tween on restart
  - [ ] Return all pooled views
  - [ ] Reset drag state
  - [ ] Clear popups
  verify no signal subscriber survives a scene unload.
- **Accept:** ten restarts in a row leave identical board, timer and coin state; no warnings, no leaked
  objects in the hierarchy.
- **Tests:** `PM RestartResetsStateTests` (move blocks, restart, compare to fresh), `PM` Home round-trip
  after a fail.
- **Docs:** `gameplay-qa.md`.

---

## Chapter 6 — Home

### T-23 Home screen layout

- **Req:** R-8, R-9, R-12, R-13, R-23. Implements DA-9, DA-11.
- **Arch:** authored Home canvas: background `bg_home_screen`; top UI — lives (`bg_home_resource_holder`
  + `Heart`), coins (`bg_home_coin_holder` + `Coin_Single` + `ic_plus`), settings (`Btn_settings_normal`
  + `Settings_Icon`), profile (`profile_box_blue`); centre — level button (`Btn_levelBg_normal` /
  `Btn_level_green`, "Level N"); bottom — tab bar (`bg_menu`, `bg_home_line` separators, `ActiveTab`
  highlight, `shop`, `home_icon`, `leaderboard`, locked tabs with `lock`). `HomeView` binds level number
  and coins.
- **Accept:** layout matches at 1080x1920 and 1080x2400 with nothing clipped or overlapping; every button
  gives press feedback; Home tab stays highlighted.
- **Tests:** `PM HomeLayoutTests` (top UI and tab bar rects inside the safe area at both resolutions).
- **Docs:** `ui.md` §Home; `Docs/testing/home-qa.md`.

### T-24 Home behaviors

- **Req:** R-10, R-11, R-19.
- **Arch:** `HomeFlow` wires the level button to `INavigationService.GoToGameplayAsync()` and the settings
  button to `IPopupService.Open<SettingsPopup>()`.
- **Accept:** level button plays the current level; settings opens Settings; coin and level values are
  current on every return.
- **Tests:** `PM HomeNavigationTests`.
- **Docs:** `home-qa.md`.

---

## Chapter 7 — Settings

### T-25 Settings popup

- **Req:** R-14, R-15, R-16. Implements DA-10.
- **Arch:** `SettingsPopupView` in the Home scene: `bg_popup`, header, three rows with
  `Blockjam_Settings_Music/Sound/Vibration` icons and a `ToggleView` each. On: `btn_toggle_on`. Off: the
  same sprite greyed and knob position flipped (the art set has no off sprite — asset feedback). Placeholder
  buttons (e.g. Support with `Support_icon`) give feedback only. Close (`Popup_Close_Button`) returns Home.
  State read from and written to `ISettingsService`.
- **Accept:** each toggle visibly differs on vs off; state survives closing, reopening and app restart;
  close returns to Home with nothing else changed.
- **Tests:** `EM ToggleViewModel` tests if a model exists, `PM SettingsPopupTests` (toggle → visual and
  stored state).
- **Docs:** `ui.md` §Settings; `Docs/testing/settings-qa.md`.

---

## Chapter 8 — Level editor

All editor code lives in `ColorBlockJam.Editor`; it reuses `LevelDefinition`, `LevelDefinitionValidator`
and (T-33) the solver from `ColorBlockJam.Game`, so the editor never has its own copy of the rules.

### T-26 Level editor window and level list

- **Req:** R-27, R-36.
- **Arch:** `LevelEditorWindow : EditorWindow` (menu **ColorBlockJam > Level Editor**), left panel: catalog
  list with New, Duplicate, Delete (confirm dialog), drag to reorder, "Level N" labels. New levels are
  created as assets in `Assets/_Project/Levels/` and appended to the catalog automatically. All edits go
  through `Undo.RecordObject`.
- **Accept:** a designer creates, duplicates, deletes and reorders levels without touching the Project
  window; Ctrl+Z undoes each action.
- **Tests:** `EM LevelCatalogEditingTests` (create/append/reorder on a temp catalog).
- **Docs:** `Docs/General/level-editor.md` (designer guide + internals); `Docs/testing/level-editor-qa.md`.

### T-27 Visual board canvas, size and timer

- **Req:** R-26, R-28.
- **Arch:** centre panel draws the board as a scaled grid (walls, cells, blocks in their colours, doors on
  the wall ring); inspector panel: Width and Height steppers (bounded by `LevelRulesConfig`), Timer field in
  seconds with an `mm:ss` preview. Shrinking that would cut blocks or doors asks to confirm and lists what
  gets removed.
- **Accept:** changing size redraws immediately; the canvas scales to the window; no numeric-only view.
- **Tests:** `EM LevelResizeTests` (content kept, out-of-bounds items reported).
- **Docs:** `level-editor.md`.

### T-28 Block placement tools

- **Req:** R-26.
- **Arch:** tool bar: Select/Move, Paint block, Erase. Colour swatches from the `BoardViewConfig` palette. Shape
  presets (1x1, 1x2, 1x3, L, T, S, 2x2) plus free paint (drag over cells to build a connected shape).
  Rotate 90° (R key / button). Overlaps and disconnected shapes are prevented live with a red preview.
- **Accept:** a designer places, recolours, moves, rotates and deletes blocks with the mouse only.
- **Tests:** `EM BlockEditingTests` (rotate, move, overlap rejection) on the pure editing model.
- **Docs:** `level-editor.md`.

### T-29 Door placement tools

- **Req:** R-26. Implements DA-17.
- **Arch:** Door tool: click a wall cell to add a 1-cell door in the selected colour; drag along the wall to
  set length; click a door to select, change colour, resize or delete. Corners are not valid door cells.
- **Accept:** doors appear on the canvas in their colour and length; invalid spots show why.
- **Tests:** `EM DoorEditingTests`.
- **Docs:** `level-editor.md`.

### T-30 Save, load, play-test and structural validation

- **Req:** R-27, R-29, R-31 (partial).
- **Arch:** Save writes the `LevelDefinition` asset (`EditorUtility.SetDirty` + `AssetDatabase.SaveAssets`);
  a dirty marker and "unsaved changes" prompt on switching levels or closing. **Play** saves, stores the
  chosen level in a `PlayTestRequest` (EditorPrefs, editor-only) and enters Play mode through Splash;
  Gameplay starts that level instead of the progress level and Home is skipped. Before save, the
  validator's issues show in a list; errors block save, warnings ask to confirm.
- **Accept:** a designer makes a level, saves, presses Play and plays it with no code or file edits; the game
  loads the exact asset the editor saved.
- **Tests:** `EM PlayTestRequestTests`; `PM PlayTestLevelTests` (requested level is the one loaded).
- **Docs:** `level-editor.md`; `splash-boot.md` (play-test override).

### T-31 Editor usability pass and designer guide

- **Req:** R-36.
- **Subtasks:**
  - [ ] Tooltips on every control
  - [ ] A collapsible help box with the 5-step "make a level" guide
  keyboard shortcuts (1–4 tools, R rotate, Del delete, Ctrl+S save); hover highlight of the cell under the
  mouse; empty-state message when the catalog is empty; consistent colours and spacing.
- **Accept:** someone who has not seen the editor builds and plays a 2-block level following only the
  in-editor help (hallway test, recorded in the QA page).
- **Tests:** manual, `level-editor-qa.md`.
- **Docs:** `level-editor.md` designer section; screenshots for the README.

---

## Chapter 9 — Content

### T-32 Author levels 1–5 in the editor

- **Req:** R-17, R-30.
- **Subtasks:** design a difficulty ramp — L1 tutorial-sized (≈ 4x4, 2–3 blocks), L2 more colours, L3 blocks
  that must move out of the way first, L4 larger board and narrow doors, L5 combination; set timers with
  margin; play each to completion; record solution notes in the QA page.
  - [x] Five levels with a rising ramp, made in code and proved solvable (level-data.md §3).
  - [ ] Remake them with the level editor once it exists (R-30).
- **Accept:** all five come from the editor (no hand-edited YAML); each solvable within its timer by a
  first-time player.
- **Tests:** `EM ShippedLevelsTests`: five levels, no validator issues, solvable block by block with the
  real board rules, block count never falls from one level to the next.
- **Docs:** `gameplay-qa.md` (per-level checks).

---

## Chapter 10 — Optional scope (only after chapters 0–9 are Done)

### T-33 (could) Solvability validation

- **Req:** R-31.
- **Arch:** `LevelSolver` (pure, Game assembly): breadth-first search over board states with a visited hash
  and a state cap from `LevelRulesConfig`; returns Solvable, Unsolvable or Unknown (cap hit). The editor runs
  it before save and warns.
- **Tests:** `EM LevelSolverTests` (solvable, unsolvable, cap).

### T-34 (could) Ice blocker

- **Req:** R-33.
- **Arch:** `BlockModifier.Ice` with `ThawAfterExits`; frozen blocks cannot move; counter on the block; ice
  material from `icetexture.jpg`. Editor: ice toggle and count on a selected block.
- **Tests:** `EM` frozen-move and thaw tests.

### T-35 (could) Arrow block

- **Req:** R-34.
- **Arch:** `BlockModifier.Axis` (horizontal / vertical) shown with `Arrow_*` meshes; `BoardState.TryStep` limits
  directions. Editor: axis selector on a selected block.
- **Tests:** `EM` axis-constrained move tests.

### T-36 (could) One functional booster

- **Req:** R-32.
- **Arch:** `IBooster` with one implementation (timer freeze for N seconds or hammer removing one block);
  count stored in the wallet; HUD shows count.
- **Tests:** `EM` booster tests.

---

## Chapter 11 — Quality

### T-37 Architecture tests

- **Req:** R-35, R-37, R-38.
- **Arch:** reflection tests: no `Update`/`LateUpdate`/`FixedUpdate`/`ITickable` in `ColorBlockJam.*` types
  (allow-list with reason); `ColorBlockJam.Core` references no `ColorBlockJam.Game`; no `UnityEngine.Debug.Log`
  calls outside `UnityGameLogger` (scan IL or source); every scene's serialized references are set.
- **Docs:** `testing.md`.

### T-38 Performance pass

- **Req:** R-38.
- **Subtasks:**
  - [ ] Profile a full level on device
  - [ ] GC alloc 0 B per frame while dragging and while idle
  - [ ] Batch
  count; `ProfilerMarker` on drag and move; fix findings; record numbers.
- **Docs:** `Docs/General/performance.md` (method, device, numbers).

### T-39 Full QA pass on device and aspect ratios

- **Req:** R-13, all acceptance criteria.
- **Subtasks:**
  - [ ] Run every QA page at 1080x1920 and 1080x2400 in the Game view and on an Android device
  fix or log every failure as a known problem.
- **Docs:** tick QA pages; known problems list for the README.

---

## Chapter 12 — Release

### T-40 Project README

- **Req:** R-6, R-40, R-41.
- **Sections:** how to open and run (Unity 2022.3.62f2, LFS, Play); how to open the editor and make a level
  (with screenshots); architecture decisions and the reason for each (DI scopes, signal bus, pure board
  model, ScriptableObject level format, animation separation, no per-frame code, cells-only data with the
  2-unit art cell normalized at import — Design README §2.8); decisions made for unclear
  requirements (DA list summary); known problems and incomplete parts; asset feedback.
- **Docs:** root `README.md`.

### T-41 Android APK

- **Req:** R-7.
- **Subtasks:**
  - [ ] Release-config APK (IL2CPP, ARM64)
  - [ ] Install and smoke test the full flow: Splash → Home → Settings toggles → Level 1 win → Level 2 →
    pause/restart → timeout fail → Home.
- **Docs:** `getting-started.md` (build steps).

### T-42 Repository hygiene

- **Req:** R-5.
- **Subtasks:**
  - [ ] `.gitignore` excludes Library, Temp, Logs, Build, UserSettings, generated `.csproj`/`.sln`
  every LFS file is a pointer; fresh clone into an empty folder opens and plays; all tests green.
