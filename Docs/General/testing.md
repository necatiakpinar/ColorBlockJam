# Testing

> **Binding agent rules.** Read this page before adding or changing a test or a test assembly.

## 1. Layout

| Assembly | Folder | Covers |
| --- | --- | --- |
| `ColorBlockJam.Tests.EditMode` | `Assets/_Project/Tests/EditMode/` | pure logic, DI wiring, build settings |
| `ColorBlockJam.Tests.PlayMode` | `Assets/_Project/Tests/PlayMode/` | real scenes end to end |

Shared fakes and builders live in `Tests/EditMode/Helpers/`: `InMemoryKeyValueStorage`,
`RecordingLogger`, `FakeBootStep`, `RecordingProgress`, `RecordingNavigation`, `ControllableSceneLoader`,
`RecordingPopup`, `LevelDataBuilder` (levels in one line), `TestCatalog` (in-memory catalogs) and
`TestViews` (views with their serialized references wired). Play Mode tests drive the app through
`SceneTestUtility` (`BootToHome`, `BootToGameplay`, `TapButton`). The Game assembly exposes a few
internal view counters to the test assemblies (`Properties/AssemblyInfo.cs`). Reuse them before writing a
new fake.

## 2. Current suite

| Fixture | Pins |
| --- | --- |
| `SignalBusTests` | delivery, order, type isolation, duplicate guard, (un)subscribe during fire |
| `BootSequenceTests` | step order, weighted progress, empty sequence, cancellation, weight validation |
| `PlayerProgressServiceTests` | defaults, read-before-load, persist + flush, corrupt and invalid data, setting a level |
| `InstallerResolutionTests` | every installer builds; one SignalBus; boot step order; board space cell size |
| `HomeFlowTests` | Home shows the level path and the balance; the level button routes to Gameplay; settings open showing the saved switches, a switch flips its setting, close closes |
| `NavigationServiceTests` | each route loads its scene; a request during a load is ignored; a failed load frees the guard |
| `PopupServiceTests` | one popup at a time; close allows the next |
| `SafeAreaTests` | safe area to anchors, full screen and notched |
| `LevelDefinitionValidatorTests` | one test per level rule in level-data.md §2 |
| `LevelDefinitionTests` | level data survives asset serialization; wall lengths |
| `BoardStateTests` | moves, walls, blocks, door fit on every wall, clear path out, win, fresh restart, `CanStep`, zero allocation |
| `LevelProviderTests` | level number to catalog entry; looping; empty catalog |
| `LevelProgressionTests` | a win advances the level once; a loss or a disposed listener does not |
| `ShippedLevelsTests` | the catalog's five levels: no validator issues, solvable block by block, block counts that never fall |
| `ConnectivityServiceTests` | first answer announced even offline, only changes after, online and offline intervals, probe on app resume, Retry probes at once and returns the answer, server time handed to the trusted clock, stops when disposed (`FakeProbeRequest`, `FakeGameClock`) |
| `TrustedClockTests` | no time before a server time; HTTP date read as UTC; moves on with monotonic time; a bad header keeps the last time |
| `WalletServiceTests` | starts empty, throws before load, adds and saves at once with one signal, rejects zero or less, corrupt or negative saves start fresh |
| `SettingsServiceTests` | all on for a new player, throws before load, a change saves and announces only that toggle, an unchanged value does nothing, a corrupt save starts fresh |
| `LevelRewardTests` | coins by difficulty, a missing entry throws, the shipped table rises with difficulty, a win pays its layout's reward (also when looped), a loss or a disposed service pays nothing |
| `GameplayFlowTests` | start, HUD restart, pause opens the settings popup only while playing, close resumes, switches flip settings, Home, nothing after dispose (`RecordingPopupService`) |
| `LevelResultFlowTests` | win popup after the delay with the reward, Continue plays the next level; out-of-time offer, buying time, giving up, Retry, Home |
| `PopupPrefabTests` | both settings popups are variants of the base prefab; the popup configs list every prefab; prefabs start hidden and full-screen; every popup button gives press feedback; every popup text auto-sizes |
| `LevelContinueTests` | spending coins, buying more time on the same board, failing again when it runs out, nothing without coins or outside a loss |
| `LevelTimerTests` | full time on start, counting only from the first grab, one signal per second, stands still while paused or in the background, fails once at zero, stops on a win, full again on restart, stops when disposed (`FakeGameClock`) |
| `GameplayHudViewTests` | the timer label reads `mm:ss` |
| `LevelSessionTests` | the attempt's transition table, refused transitions, signal order, restart equal to a fresh start |
| `GameplayHapticsTests` | an exit taps light, nothing after dispose (`RecordingHaptics`) |
| `ExitPathTests` | a leaving block's wall, length, door span, edge and door centre |
| `ActiveBoardTests` | a load sizes the board space and starts fresh; steps announce move, exit (with the cell left from) and clear |
| `BlockDragControllerTests` | free follow and diagonals, nearest-cell commit, fast drags, sliding, slipping into gaps without drift, jitter on a slow push, or locking out of a side corridor or a wobbled-at gap, exit on a small push into the door, picking a block a press narrowly missed, dropping the held block on pause and ignoring input until the level resumes, lean, grab, snap, exit, zero allocation |
| `OutlineMeshBuilderTests` | the outline is one rounded loop per boundary (1x1, L, ring with a hole), hugs a bar a gap outside it along its bottom edge, rounds its corners, and gives each vertex its outward normal and the rim lift |
| `BlockTileResolverTests` | pieces for 1x1, bars, squares, L, T; inner corner notch; every quarter covered once |
| `BoardFramingTests` | 3x3 to 10x10 boards fit inside the margins at 9:16 and 20:9, centred, as close as possible |
| `BoardSpaceTests` | cell ↔ world at cell size 1 and 2; edges; outside cells |
| `BuildSettingsTests` | Splash first; every `SceneId` is a build scene; portrait; IL2CPP ARM64 |
| `AssetImportTests` | UI sprites, model scale, floor tile size |
| `SceneSetupTests` | every build scene: canvas scaler, safe area root, button feedback |
| `NavigationFlowTests` (PlayMode) | real boot reaches Home; Home ⇄ Gameplay round trip |
| `ButtonPressFeedbackTests` (PlayMode) | press shrinks, release restores; non-interactable and disabled cases |
| `GameplayBoardTests` (PlayMode) | every part and block drawn; the wall ring closes corner to corner without seams; every door has one centred arrow pointing out; swipes take blocks out and clear once; a wrong door holds; outline while held; reload mid-exit; a held block glides after a sudden finger move instead of jumping; the door opens and closes for a leaving block, the whole door for a narrower block; two blocks leaving one door together share one spray; an exiting block settles at its door before sliding in, and is cut at the door at full size while debris sprays; following and snapping allocate nothing |
| `LevelProgressionFlowTests` (PlayMode) | a full lap of wins: each starts the next level and the last loops to the first layout (restores saved progress) |
| `CoinPersistenceTests` (PlayMode) | a win pays its reward once; the balance survives going Home and back, with or without a win (restores the saves) |
| `HomeScreenTests` (PlayMode) | Home shows the current level on the path and the button; the settings popup opens and closes |
| `GameplayPopupTests` (PlayMode) | pause shows the settings popup and close resumes; a loss goes out of time, level failed, Retry |
| `GameplayTimerTests` (PlayMode) | the real scene waits for the first grab, then counts down on the HUD; Unity's background event freezes the timer |
| `BoardInputTests` (PlayMode) | pointer events become board cells; one finger; release off the plane; disable mid-touch; UI wins the raycast |

## 3. Rules

- Name tests `Subject_ShouldOutcome_WhenContext`.
- One behavior per test; arrange / act / assert separated by a blank line.
- Async tests: `[UnityTest] public IEnumerator X() => UniTask.ToCoroutine(async () => { ... });`
- No real time waits in Edit Mode; drive async steps with `UniTaskCompletionSource`.
- Never touch real `PlayerPrefs` in a test; inject `InMemoryKeyValueStorage`.
- Every bug fix starts with a failing test that reproduces it.
- A QA checklist case that a test automates names the test.

## 4. Running

- Editor: **Window > General > Test Runner** (EditMode / PlayMode tabs).
- Headless (close the editor holding the project first):

```bash
"<Unity>" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml
"<Unity>" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults Logs/playmode.xml
```

Results land in `Logs/` (git-ignored).
