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
- [x] [T-07 Persistence: wallet, settings, progress](#t-07-persistence-wallet-settings-progress)
- [x] [T-08 Level data format and catalog](#t-08-level-data-format-and-catalog)
- [x] [T-09 Board model (pure rules)](#t-09-board-model-pure-rules)
- [x] [T-10 Board view: floor, walls, doors, camera fit](#t-10-board-view-floor-walls-doors-camera-fit)
- [x] [T-11 Block view: modular block meshes and colours](#t-11-block-view-modular-block-meshes-and-colours)
- [x] [T-12 Drag and slide](#t-12-drag-and-slide)
- [x] [T-13 Exit through door and level complete](#t-13-exit-through-door-and-level-complete)
- [x] [T-14 Level session state machine](#t-14-level-session-state-machine)
- [x] [T-15 Countdown timer](#t-15-countdown-timer)
- [x] [T-16 Coin rewards](#t-16-coin-rewards)
- [x] [T-17 Level progression and looping](#t-17-level-progression-and-looping)
- [x] [T-18 Gameplay HUD](#t-18-gameplay-hud)
- [x] [T-19 Pause popup](#t-19-pause-popup)
- [x] [T-20 Fail popup](#t-20-fail-popup)
- [x] [T-21 Win popup](#t-21-win-popup)
- [x] [T-22 Restart and home correctness](#t-22-restart-and-home-correctness)
- [x] [T-23 Home screen layout](#t-23-home-screen-layout)
- [x] [T-24 Home behaviors](#t-24-home-behaviors)
- [x] [T-25 Settings popup](#t-25-settings-popup)
- [x] [T-26 Level editor window and level list](#t-26-level-editor-window-and-level-list)
- [x] [T-27 Visual board canvas, size and timer](#t-27-visual-board-canvas-size-and-timer)
- [x] [T-28 Block placement tools](#t-28-block-placement-tools)
- [x] [T-29 Door placement tools](#t-29-door-placement-tools)
- [x] [T-30 Save, load, play-test and structural validation](#t-30-save-load-play-test-and-structural-validation)
- [x] [T-31 Editor usability pass and designer guide](#t-31-editor-usability-pass-and-designer-guide)
- [x] [T-32 Author levels 1–5 in the editor](#t-32-author-levels-15-in-the-editor)
- [x] [T-33 Solvability validation](#t-33-could-solvability-validation)
- [x] [T-34 Ice blocker](#t-34-ice-blocker)
- [x] [T-35 Arrow block](#t-35-arrow-block)
- [x] [T-36 Four functional boosters](#t-36-four-functional-boosters)
- [x] [T-37 Architecture tests](#t-37-architecture-tests)
- [ ] [T-38 Performance pass](#t-38-performance-pass)
- [ ] [T-39 Full QA pass on device and aspect ratios](#t-39-full-qa-pass-on-device-and-aspect-ratios)
- [x] [T-40 Project README](#t-40-project-readme)
- [ ] [T-41 Android APK](#t-41-android-apk)
- [ ] [T-42 Repository hygiene](#t-42-repository-hygiene)
- [x] [T-43 Touch and door feel](#t-43-touch-and-door-feel)
- [x] [T-44 Connectivity probe and trusted time](#t-44-connectivity-probe-and-trusted-time)
- [ ] [T-45 No-internet popup over every scene](#t-45-no-internet-popup-over-every-scene)
- [ ] [T-46 Boot waits for a connection](#t-46-boot-waits-for-a-connection)
- [x] [T-47 Level generator](#t-47-level-generator)
- [x] [T-48 Lives](#t-48-lives)
- [x] [T-49 Twenty levels and the level hardener](#t-49-twenty-levels-and-the-level-hardener)
- [x] [T-50 Level packs (JSON, remote-ready)](#t-50-level-packs-json-remote-ready)
- [x] [T-51 Coin flight on a win](#t-51-coin-flight-on-a-win)
- [x] [T-52 Whole lives bar opens the lives popup](#t-52-whole-lives-bar-opens-the-lives-popup)
- [x] [T-53 Sound effects](#t-53-sound-effects)
- [x] [T-54 Low-time warning](#t-54-low-time-warning)
- [x] [T-55 Level editor look](#t-55-level-editor-look)

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
  - [x] `WalletService`, `SettingsService` and `PlayerProgressService` share `JsonRecord<T>`; all three load
    in `LoadPlayerDataStep`.
  - [x] ~~`AppLifecycleObserver`~~: dropped, every change saves and flushes at once, so there is nothing
    left to save on pause (persistence.md §2).
- **Accept:** coins, toggles and level survive app kill and restart; background/foreground keeps them.
- **Tests:** `EM WalletServiceTests` (add, negative rejected, persists, signal fired once),
  `EM SettingsServiceTests` (each change saves at once, so no lifecycle save test).
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

## Online requirement

### T-44 Connectivity probe and trusted time

- **Req:** R-43, R-44. Implements DA-25, DA-27.
- **Arch:** Root scope. `IConnectivityService` (Core, `Network/`) with `UnityWebRequestConnectivityService`:
  a `GET` to `NetworkConfig.ProbeUrl` with `ProbeTimeout`; success is any 2xx answer. It keeps `IsOnline`
  and fires `ConnectivityChangedSignal(isOnline)` only on a change. It probes at start, every
  `OnlineProbeInterval` while online, every `OfflineProbeInterval` while offline, at once when the app
  comes back from the background (`ApplicationPausedSignal`), and on `ProbeNowAsync` (Retry). Waits use
  `IGameClock` delays, not `Update`. `ITrustedClock` (Core): `TryGetUtcNow(out DateTime)` = the last
  successful probe's `Date` header plus the monotonic time since it; false before the first success.
  The HTTP call sits behind a small `IProbeRequest` seam so Edit Mode tests answer it by hand.
- **Subtasks:**
  - [x] `NetworkConfig` (Root) with probe URL, timeout and intervals.
  - [x] Probe loop, change-only signal, resume probe, `ProbeNowAsync`.
  - [x] Trusted clock from the `Date` header (RFC 1123); missing or unparsable header keeps the last value.
- **Accept:** with the network cut, `IsOnline` turns false within one probe interval plus the timeout;
  reconnecting turns it true within one offline interval; the trusted time stays right after the device
  clock is moved.
- **Tests:** `EM ConnectivityServiceTests` (fake request and `FakeGameClock`: start, change-only signal,
  intervals, resume, retry, cancel on dispose), `EM TrustedClockTests` (date parse, monotonic advance,
  unavailable before the first success).
- **Docs:** new `network.md` (probe, intervals, trusted time, editor bypass); `architecture.md` §1 tree;
  `network-qa.md`.

### T-45 No-internet popup over every scene

- **Req:** R-43. Implements DA-26.
- **Arch:** an authored overlay canvas on the `RootLifetimeScope` prefab (sort order above every scene
  canvas, `SafeArea`, full-screen blocker), `NoInternetPopupView` bound by `[Inject] Construct`, shown on
  `ConnectivityChangedSignal(false)` and hidden on `(true)`. Retry calls `IConnectivityService.ProbeNowAsync`
  and shows a short busy state. Gameplay: `LevelTimer` and `BlockDragController` treat offline like the
  app being in the background (timer freezes, a held block is dropped, the finger is ignored); the level is
  not paused, so no pause popup opens under it.
- **Subtasks:**
  - [ ] Author the overlay (title, message, Retry with `ButtonPressFeedback`) from the UI art set.
  - [ ] Show / hide with the popup animation used by the other popups (T-19 / T-25 shared view).
  - [ ] Timer and drag listen to the connectivity signal.
- **Accept:** cutting the network on Home or mid-level shows the popup within one probe interval; a
  mid-level timer stands still under it and the board ignores touches; Retry with the network back
  closes it and play continues where it stopped.
- **Tests:** `EM LevelTimerTests` (offline freezes, online resumes), `EM BlockDragControllerTests`
  (offline drops and ignores), `PM NoInternetPopupTests` (fake connectivity: shows over Gameplay, Retry
  probes, back online hides).
- **Docs:** `network.md`, `ui.md` §3 (the Root overlay is the one popup outside `IPopupService`),
  `gameplay.md` §6 Timer, `network-qa.md`.

### T-46 Boot waits for a connection

- **Req:** R-43. Implements DA-25.
- **Arch:** `ConnectivityBootStep` (first in `SplashInstaller`) awaits the first successful probe; while
  offline the Root popup is up and the step keeps waiting (no timeout: the game cannot run offline).
  `splash-boot.md` §1 guarantee 3 changes to "Splash reaches Home once online". In the editor,
  `NetworkConfig.BypassInEditor` skips the gate so play mode and Play Mode tests do not need a network;
  tests of the gate use a fake service.
- **Subtasks:**
  - [ ] Boot step and its weight on the loading bar.
  - [ ] Editor bypass; Play Mode tests run with it on.
- **Accept:** a device in airplane mode stays on Splash with the popup; turning the network on continues
  to Home without a restart; the editor plays without a network when the bypass is on.
- **Tests:** `EM ConnectivityBootStepTests` (waits while offline, completes on the first success),
  `EM InstallerResolutionTests` (step order), `PM NavigationFlowTests` stay green with the bypass.
- **Docs:** `splash-boot.md` §1–§3, `network.md`, `navigation-qa.md`.

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
  - [x] `LevelTimer`, `IGameClock` / `RealtimeGameClock`, `TimerChangedSignal`; it counts from the first
    grab of a block (DA-6); the background comes from
    `ApplicationPauseReporter` on the Root prefab (`ApplicationPausedSignal`).
  - [x] `mm:ss` label in the placeholder top bar; T-18 moves it into the timer holder art.
  - [ ] The fail popup opening at zero lands with T-20 (input already stops).
- **Accept:** HUD counts down `mm:ss`; pause and background freeze it; at 0 input stops and the fail popup
  opens; restart restores the full time.
- **Tests:** `EM LevelTimerTests` with a fake clock (countdown, pause, background, expiry once, win,
  restart), `EM GameplayHudViewTests` (`mm:ss`), `PM GameplayTimerTests` (real scene, background event).
- **Docs:** `gameplay.md` §Timer.

### T-16 Coin rewards

- **Req:** R-19. Implements DA-7, DA-14.
- **Arch:** on `Complete`, `LevelRewardService` adds `GameplayConfig.CoinsPerWin` to `IWalletService`
  before the win popup appears; idempotent per attempt. Fail, restart and Home never touch the wallet.
  - [x] Rewards by level difficulty instead of one amount (developer, DA-7): `LevelData.Difficulty`,
    `LevelRewardConfig` read through `LevelRewardProvider`; `LevelRewardService` pays on
    `LevelCompletedSignal` (economy.md).
- **Accept:** coins rise by the reward once per win; leaving a level mid-play and returning shows the same
  balance as before entering; app restart keeps the balance.
- **Tests:** `EM LevelRewardTests`; `PM CoinPersistenceTests` (enter, leave, return, compare).
- **Docs:** `economy.md`.

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
  - [x] Laid out after the reference screen (DA-28); lives are not shown in gameplay.
- **Tests:** `EM GameplayHudViewTests` (level, timer and coin text after signals), `PM GameplayPopupTests`.
- **Docs:** `ui.md` §Gameplay HUD; `gameplay-qa.md`.

### T-19 Pause popup

- **Req:** R-21. Implements DA-15.
- **Arch:** `PausePopupView` (authored, `bg_popup`, `bg_popup_header`, `Popup_Close_Button`): Resume, Restart
  (`ic_restart`), Home (`home_icon`). Calls the session / navigation; owns no rules.
  - [x] Built as the reference settings popup instead (DA-29): working switches, placeholders, Home; close
    resumes, the HUD's restart button restarts.
- **Accept:** opening pauses timer and board input; Resume continues from the same second; Restart and Home
  work from the popup.
- **Tests:** `EM GameplayFlowTests` (pause, close, switches, Home), `PM GameplayPopupTests`.
- **Docs:** `ui.md` §Popups; `gameplay-qa.md`.

### T-20 Fail popup

- **Req:** R-20. Implements DA-15.
- **Arch:** `FailPopupView`: "Time's up" header, Restart (`Btn_square_retry`), Home. Opens on
  `LevelFailedSignal`.
  - [x] One popup after the reference failed screen (DA-31): "Level Failed!" opens at timer zero with
    Retry (restarts, or offers a refill with no lives left) and a close that goes Home (`FailFlow`).
- **Accept:** opens at timer zero; Restart replays the same level from its start state; Home returns to Home
  with the same level number.
- **Tests:** `EM LevelResultFlowTests`, `PM GameplayPopupTests`.
- **Docs:** `ui.md`; `gameplay-qa.md`.

### T-21 Win popup

- **Req:** R-24, R-19. Implements DA-12.
- **Arch:** `WinPopupView`: level cleared header, reward amount (`Coin_Single`), Continue
  (`Btn_popup_green`). Opens on `LevelCompletedSignal` after the last exit animation.
  - [x] `WinFlow` opens it `WinPopupDelay` after the win; Continue replaces the timed next level (DA-23
    withdrawn).
- **Accept:** shows the reward already credited; Continue moves on to the next level number (through Home since T-51).
- **Tests:** `EM LevelResultFlowTests`, `PM LevelProgressionFlowTests`, `PM CoinPersistenceTests`.
- **Docs:** `ui.md`; `gameplay-qa.md`.

### T-22 Restart and home correctness

- **Req:** R-21, R-25.
- **Subtasks:**
  - [x] Kill every board tween on restart (a reload kills the block, door and spray tweens)
  - [x] Return all pooled views (`BoardView` and `BlockLayerView` release every part on a load)
  - [x] Reset drag state (`BlockDragController` drops its block on `BoardLoadedSignal`)
  - [x] Clear popups (Retry closes its popup before restarting; restart is only reachable with none open)
  - [x] No signal subscriber survives a scene unload (`RestartResetsStateTests` fires every gameplay signal
    on Home).
- **Accept:** ten restarts in a row leave identical board, timer and coin state; no warnings, no leaked
  objects in the hierarchy.
- **Tests:** `PM RestartResetsStateTests`: ten restarts compared with a fresh start (board, views, timer,
  coins, state, object count); Home round-trip after a loss; no gameplay listener left after going Home.
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
  - [x] Laid out after the reference screen with the project's art (ui.md §8); the portrait and the
    hard-level skull have no art.
- **Tests:** `EM HomeFlowTests`, `PM HomeScreenTests`, `EM SceneSetupTests` (press feedback on every button).
- **Docs:** `ui.md` §Home; `Docs/testing/home-qa.md`.

### T-24 Home behaviors

- **Req:** R-10, R-11, R-19.
- **Arch:** `HomeFlow` wires the level button to `INavigationService.GoToGameplayAsync()` and the settings
  button to `IPopupService.Open<SettingsPopup>()`.
- **Accept:** level button plays the current level; settings opens Settings; coin and level values are
  current on every return.
- **Tests:** `EM HomeFlowTests` (level path, balance, play, settings), `PM HomeScreenTests`.
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
  - [x] One base prefab (`SettingsPopup`) with a Home and a Gameplay variant (ui.md §7); the gameplay
    variant is the pause popup.
- **Accept:** each toggle visibly differs on vs off; state survives closing, reopening and app restart;
  close returns to Home with nothing else changed.
- **Tests:** `EM HomeFlowTests` (switch → stored state and look), `EM PopupPrefabTests` (both variants of the
  base), `PM HomeScreenTests` (open and close).
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
  - [x] Built (level-editor.md); edits are undoable through `LevelEditorState`; Copy and Delete confirm.
- **Tests:** `EM LevelCatalogEditingTests` (append, duplicate, move, delete, save on a temp catalog).
- **Docs:** `Docs/General/level-editor.md` (designer guide + internals); `Docs/testing/level-editor-qa.md`.

### T-27 Visual board canvas, size and timer

- **Req:** R-26, R-28, R-49.
- **Arch:** the inspector also sets the difficulty (Normal, Hard, Very Hard) and shows its reward.
  Centre panel draws the board as a scaled grid (walls, cells, blocks in their colours, doors on
  the wall ring); inspector panel: Width and Height steppers (bounded by `LevelRulesConfig`), Timer field in
  seconds with an `mm:ss` preview. Shrinking that would cut blocks or doors asks to confirm and lists what
  gets removed.
- **Accept:** changing size redraws immediately; the canvas scales to the window; no numeric-only view.
- **Tests:** `EM LevelDraftTests` (resize keeps what fits and reports what goes).
- **Docs:** `level-editor.md`.

### T-28 Block placement tools

- **Req:** R-26.
- **Arch:** tool bar: Select/Move, Paint block, Erase. Colour swatches from the `BoardViewConfig` palette. Shape
  presets (1x1, 1x2, 1x3, L, T, S, 2x2) plus free paint (drag over cells to build a connected shape).
  Rotate 90° (R key / button). Overlaps and disconnected shapes are prevented live with a red preview.
- **Accept:** a designer places, recolours, moves, rotates and deletes blocks with the mouse only.
- **Tests:** `EM LevelDraftTests` (add, move, rotate, overlap and connectivity rejection).
- **Docs:** `level-editor.md`.

### T-29 Door placement tools

- **Req:** R-26. Implements DA-17.
- **Arch:** Door tool: click a wall cell to add a 1-cell door in the selected colour; drag along the wall to
  set length; click a door to select, change colour, resize or delete. Corners are not valid door cells.
- **Accept:** doors appear on the canvas in their colour and length; invalid spots show why.
- **Tests:** `EM LevelDraftTests` (doors stay on their wall and apart).
- **Docs:** `level-editor.md`.

### T-30 Save, load, play-test and structural validation

- **Req:** R-27, R-29, R-31 (partial), R-47.
- Built with a simpler play-test than first planned: `LevelPlayTest` makes the level the saved current
  level and opens Gameplay once Home is up, so the game needs no play-test code (level-editor.md §2).
- **Arch:** Save writes the `LevelDefinition` asset (`EditorUtility.SetDirty` + `AssetDatabase.SaveAssets`);
  a dirty marker and "unsaved changes" prompt on switching levels or closing. **Play** saves, stores the
  chosen level in a `PlayTestRequest` (EditorPrefs, editor-only) and enters Play mode through Splash;
  Gameplay starts that level instead of the progress level and Home is skipped. Before save, the
  validator's issues show in a list; errors block save, warnings ask to confirm.
- **Accept:** a designer makes a level, saves, presses Play and plays it with no code or file edits; the game
  loads the exact asset the editor saved.
- **Tests:** `EM NavigationServiceTests.IsLoading_*`; play-test checked by hand (`level-editor-qa.md`).
- **Docs:** `level-editor.md`; `splash-boot.md` (play-test override).

### T-31 Editor usability pass and designer guide

- **Req:** R-36, R-48. Implements DA-34.
- **Subtasks:**
  - [x] Tooltips on every control
  - [x] A collapsible help box with the 5-step "make a level" guide
  - [x] Shortcuts (DA-34), hover highlight, empty-catalog message
  - [x] Hallway test (`level-editor-qa.md`), accepted by the developer 2026-11-17
- **Accept:** someone who has not seen the editor builds and plays a 2-block level following only the
  in-editor help (hallway test, recorded in the QA page).
- **Tests:** manual, `level-editor-qa.md`.
- **Docs:** `level-editor.md` designer section.

---

## Chapter 9 — Content

### T-32 Author levels 1–5 in the editor

- **Req:** R-17, R-30.
- **Subtasks:** design a difficulty ramp — L1 tutorial-sized (≈ 4x4, 2–3 blocks), L2 more colours, L3 blocks
  that must move out of the way first, L4 larger board and narrow doors, L5 combination; set timers with
  margin; play each to completion; record solution notes in the QA page.
  - [x] Five levels with a rising ramp, made in code and proved solvable (level-data.md §3).
  - [x] Remake them with the level editor (R-30): generated, timed and saved from the editor; settings, seeds
    and solutions in level-data.md §3 and `gameplay-qa.md`.
  - [x] Play each to completion by hand (`gameplay-qa.md`), accepted by the developer 2026-11-17.
- **Accept:** all five come from the editor (no hand-edited YAML); each solvable within its timer by a
  first-time player.
- **Tests:** `EM ShippedLevelsTests`: five levels, no validator issues, solved by `LevelSolver` as hard as
  their label; block count and score never fall; from level 3 on, room must be made first.
- **Docs:** `gameplay-qa.md` (per-level checks).

---

## Chapter 10 — Optional scope (only after chapters 0–9 are Done)

### T-33 Solvability validation (pulled into the editor chain, README §2 item 10)

- **Req:** R-31, R-47. Implements DA-32.
- **Arch:** `LevelSolver` (pure, Game assembly): breadth-first search over board states with a visited hash
  and a state cap from `LevelRulesConfig`; returns Solvable, Unsolvable or Unknown (cap hit). The editor runs
  it before save and warns.
- **Tests:** `EM LevelSolverTests` (solvable, unsolvable, cap, the returned solution clears the board).
- The editor shows the solution as a list of moves and steps the canvas through it.

### T-47 Level generator

- **Req:** R-46. Implements DA-33.
- **Arch:** `LevelGenerator` (pure, Game assembly, seeded `System.Random`): places one door per colour on
  the wall ring, then blocks from the shape presets in those colours, each fitting a door of its colour;
  keeps a candidate only when the validator reports no errors and `LevelSolver` proves it solvable, and
  rates it by the solution's score (exit rounds plus room-making moves, DA-33) to match the difficulty. Suggests a timer. The editor's Generate panel
  sets width, height, colours, blocks, difficulty and seed, and loads the result into the canvas to edit
  further.
- **Accept:** Generate gives a valid, solvable level with the chosen settings; the same seed gives the same
  level; a level that cannot be made within the attempts says so instead of returning a broken one.
- **Tests:** `EM LevelGeneratorTests` (valid and solvable, seed repeats, settings respected, impossible
  settings report failure).
- **Docs:** `level-editor.md`.

### T-34 Ice blocker

- **Req:** R-33. Implements DA-37.
- **Arch:** as built: `BlockData.Ice` (exits to thaw) instead of a modifier type, since ice is one number;
  `BoardBlock.Ice` counts down in `BoardState.MarkGone` on every exit or booster removal; a frozen block's
  steps are `Blocked` and the drag ignores it; `ActiveBoard` fires `IceCountedDownSignal` and
  `BlockThawedSignal`; the hammer thaws. View: `M_Ice` shows the `icetexture` as it is, `IceLabel` count,
  thaw swell and `IceShatter` (ice-textured shards, `M_IceShard`). Editor: Ice slider and `I` / `Shift+I` on a selected block; the validator and the solver know ice.
- **Accept:** a frozen block cannot move until N other blocks have left, shows its count going down, and
  thaws; the editor places it and the solver finishes levels with it.
- **Tests:** `EM IceBlockerTests`; `PM IceBlockerViewTests`.
- **Docs:** level-data.md, gameplay.md §3, level-editor.md, boosters.md.

### T-35 Arrow block

- **Req:** R-34. Implements DA-39.
- **Arch:** as built: a blocker kind ([blockers.md](../General/blockers.md)): `ArrowBlockerData` (`ArrowAxis`),
  `ArrowBlocker : IBlocker` (`AllowsStep` only along the axis), `ArrowBlockerView` on `ArrowBlocker.prefab` with
  the `Arrow_1/2/3` meshes; the drag neither moves nor leans across the axis; the validator checks a door lies
  along it. Editor: the Arrow row of the Blockers panel and `A`.
- **Tests:** `EM ArrowBlockerTests`; `PM IceBlockerViewTests` (arrow and ice on one block).

### T-36 Four functional boosters

- **Req:** R-32. Implements DA-36; replaces DA-16.
- **Arch:** as built in [boosters.md](../General/boosters.md): `BoosterInventory` (Root, saved stock),
  `BoosterShop`, `BoosterService` (freeze at once; rocket, hammer and vacuum pause the level and wait for a
  tapped block), `ActiveBoard.RemoveBlock` / `RemoveCell` / `RemoveColor`, `LevelTimer.FreezeFor`;
  `BoosterFlow` routes the bar and the purchase popup. No `IBooster` interface: the four differ only in what
  they do to the board, which one switch in the service says plainly.
- **Subtasks:**
  - [x] Stock, prices and words in `BoosterConfig`; saved stock; shop
  - [x] Freeze, rocket, hammer, vacuum on the board and the timer
  - [x] Bar counts and "+", targeting banner and backdrop, ice frame and meter, smash and vacuum animations
  - [x] Purchase popup prefab; debug window booster shortcuts
- **Accept:** each booster works from the HUD as boosters.md describes; with none left a tap offers one for
  coins; a booster can win the level.
- **Tests:** `EM BoardRemovalTests`, `BoosterInventoryTests`, `BoosterServiceTests`, `LevelTimerFreezeTests`;
  `PM BoosterFlowTests`.
- **Docs:** `boosters.md`, `boosters-qa.md`.

---

## Chapter 11 — Quality

### T-37 Architecture tests

- **Req:** R-35, R-37, R-38.
- **Arch:** reflection tests: no `Update`/`LateUpdate`/`FixedUpdate`/`ITickable` in `ColorBlockJam.*` types
  (allow-list with reason); `ColorBlockJam.Core` references no `ColorBlockJam.Game`; no `UnityEngine.Debug.Log`
  calls outside `UnityGameLogger` (scan IL or source); every scene's serialized references are set.
  As built: `EM ArchitectureTests`. It runs the four checks above, plus a source scan for `Camera.main` and
  `Find*` lookups. The scene check reads the project's own components in the three build scenes (135 references).
- **Docs:** `testing.md`.

### T-38 Performance pass

- **Req:** R-38.
- **Subtasks:**
  - [ ] Profile a full level on device (needs T-41's build and a device)
  - [x] GC alloc 0 B per frame while dragging (`PM DragAllocationTests`) and while idle (no per-frame code,
    `ArchitectureTests`)
  - [x] Batch
  count; `ProfilerMarker` on drag and move; fix findings; record numbers.
- **Docs:** `Docs/General/performance.md` (method, device, numbers).

### T-39 Full QA pass on device and aspect ratios

- **Req:** R-13, all acceptance criteria.
- **Subtasks:**
  - [x] Full Edit Mode (455) and Play Mode (44) suites green; every ⚙ case on the QA pages ticked
  - [ ] Run every 👁 case at 1080x1920 and 1080x2400 in the Game view and on an Android device
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

### T-48 Lives

- **Req:** R-23, R-44, R-50. Implements DA-38; withdraws DA-9 and DA-15.
- **Arch:** as built in [lives.md](../General/lives.md): `LivesService` (Root, saved, trusted time),
  `LivesClock` (Root entry point, one tick a second), `LevelAttemptLives` and `GameplayLivesFlow` (Gameplay),
  `LivesBarView` and `HomeFlow` (Home), `OutOfLivesPopup.prefab` shared by both scenes, `LivesConfig`.
- **Accept:** abandoning a started attempt costs one life, a win none; lives come back every 30 minutes of
  trusted time, also while closed; with none, playing opens the popup, whose Refill fills them for coins.
- **Tests:** `EM LivesServiceTests`, `LevelAttemptLivesTests`, `HomeFlowTests`, `GameplayFlowTests`,
  `LevelResultFlowTests`.
- **Docs:** `lives.md`, `lives-qa.md`, persistence.md, economy.md, ui.md, gameplay.md.

### T-49 Twenty levels and the level hardener

- **Req:** R-51. Implements DA-40.
- **Arch:** `LevelHardener` (Game assembly, seeded): from a generated level it climbs toward more room-making
  moves. Each round it draws 16 changes from the seed (move a block, slide a door, set an arrow or ice), checks
  them in parallel and keeps the one that needs the most moves while the level stays valid and solvable.
  The editor's Generate panel has **Make harder** (`H`). Levels 9–20 come from the hardener and are
  arranged by moves into the DA-40 curve.
- **Accept:** the catalog has 20 valid, solvable levels. From level 9 each group of six has a breather and
  ends on a peak no easier than the last group's, and the last peaks need about 10 room-making moves. Make
  harder never makes a level easier, and the same seed gives the same level.
- **Tests:** `EM LevelHardenerTests`, `ShippedLevelsTests`.
- **Docs:** level-data.md, level-editor.md.

### T-50 Level packs (JSON, remote-ready)

- **Req:** R-52. Implements DA-41.
- **Arch:** as built ([level-packs.md](../General/level-packs.md)):
  - `LevelPackJson` reads and writes a versioned, human-readable pack.
  - `LevelPackService` (Root, an `ILevelSource`) loads the stored remote pack or the bundled `levels.json`
    in `LoadLevelsStep`, and checks and stores remote packs (`TryStoreRemote`).
  - `LevelProvider` reads `ILevelSource`.
  - `LevelCatalogEditing` rewrites `levels.json` on every change; editor menus write it and copy it compact.
- **Accept:**
  - The game plays `levels.json`, which always matches the catalog.
  - A valid remote pack plays from the next launch.
  - A broken or older one is dropped with a warning and the bundled levels play.
- **Tests:** `EM LevelPackJsonTests`, `LevelPackServiceTests`, `LevelCatalogEditingTests`,
  `ShippedLevelsTests`, `InstallerResolutionTests`.
- **Docs:** level-packs.md, level-data.md, persistence.md, splash-boot.md, architecture.md.

### T-51 Coin flight on a win

- **Req:** R-53. Implements DA-12 (as confirmed).
- **Arch:**
  - `WinFlow` holds the HUD counter at the win. On Continue it closes the popup, awaits `ICoinFlight.FlyAsync`,
    then goes Home through `INavigationService`.
  - `CoinFlightView` sits on the top layer of the HUD canvas. It pools `FlyingCoin.prefab` and
    `CoinSparkle.prefab` and plays `CoinFlightAnimation`, `SparkleBurstAnimation` and `PunchScaleAnimation`;
    tunables are in `GameplayConfig.CoinFlight`.
  - The sparkle and glow sprites (`fx_sparkle`, `fx_glow`) are drawn by the project.
- **Accept:**
  - The counter does not jump at the win.
  - Every pile coin flies to the counter and its landing counts in, swells the icon and sparkles.
  - Home then shows the next level, and its button plays it.
- **Tests:** `EM LevelResultFlowTests`, `GameplayHudViewTests`, `ArchitectureTests` (scene wiring);
  `PM LevelProgressionFlowTests`, `CoinPersistenceTests`.
- **Docs:** gameplay.md §7, ui.md, DA-12, gameplay-qa.md.

### T-52 Whole lives bar opens the lives popup

- **Req:** R-54.
- **Arch:** `LivesBarView` raises `Clicked` from one `Button` that covers the whole `TopBar/Lives` area;
  the "+" stays as art inside it. `HomeFlow` opens the lives popup on it as before.
- **Accept:** a tap on the heart, the count, the countdown or the "+" opens the lives popup.
- **Tests:** `EM HomeFlowTests`; `PM HomeScreenTests` taps the bar.
- **Docs:** lives.md §3, lives-qa.md.

### T-53 Sound effects

- **Req:** R-55. Implements DA-43.
- **Arch:**
  - `IAudioService` (`Play(AudioCue)`) on the Root scope, reading `ISettingsService` so the sound toggle
    mutes it; it plays through `IAudioOutput`, the `SfxPlayer` on the Root prefab (one 2D source, the
    listener).
  - `AudioConfig` (Root, in `ConfigProvider`) maps each cue to a clip and a volume.
  - Gameplay `GameplaySounds` (plain class, signals): grab, exit, booster used, win, fail. Taps:
    `ButtonTapSounds` on each scene's canvas adds the tap cue to every button under it.
  - Clips under `Assets/_Project/Audio/`, synthesized by `tools/synth_sfx.py`, mono, short, ADPCM.
- **Accept:** each event plays its clip once; with sound off nothing plays; no allocation per play.
- **Tests:** `EM` audio service with a fake output (mute, cue mapping), `GameplaySoundsTests`;
  `ArchitectureTests` (config listed, no `Update`).
- **Docs:** new `Docs/General/audio.md`, CLAUDE.md trigger, art-pipeline.md (audio import), DA-10, DA-43,
  new `Docs/testing/audio-qa.md`.

### T-54 Low-time warning

- **Req:** R-56. Implements DA-42.
- **Arch:** `LevelTimer` already announces each second (`TimerChangedSignal`). On each second in
  `GameplayConfig`'s low-time range (10 s to 1 s) `GameplayHudView` flashes the timer red and swells it
  (`TimerWarningAnimation`) and `GameplaySounds` plays the tick; no signal while paused or frozen means no
  flash or tick; a level start resets the label.
- **Accept:** at 10 s left the timer turns red and pulses and a tick plays every second; a pause, the
  freeze, a win, a fail or a restart stops both; a restart shows the normal timer.
- **Tests:** `EM GameplayHudViewTests`, `GameplaySoundsTests`.
- **Docs:** gameplay.md HUD section, DA-42, gameplay-qa.md, audio-qa.md.

### T-55 Level editor look

- **Req:** R-57.
- **Arch:** one `LevelEditorSkin` (editor assembly) builds the skin once: rounded, coloured buttons from
  generated textures, a header toolbar, card panels, a selected state for the active tool and colour.
  `LevelEditorWindow` and `LevelCanvas` draw with it; behaviour, shortcuts (DA-34) and data stay as they are.
- **Accept:** every tool, shortcut and panel works as before; the window reads at a glance in the
  light and dark editor themes.
- **Tests:** `EM` level editor tests stay green; level-editor-qa.md manual look check.
- **Docs:** level-editor.md (screens), level-editor-qa.md.

