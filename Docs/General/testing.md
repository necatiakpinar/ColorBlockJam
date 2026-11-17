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
| `PlayerProgressServiceTests` | defaults, read-before-load, persist + flush, corrupt and invalid data |
| `InstallerResolutionTests` | every installer builds; one SignalBus; boot step order; board space cell size |
| `GameplayFlowTests` | the level announced matches saved progress; Home button routes home |
| `HomeFlowTests` | Home shows the saved level; the level button routes to Gameplay |
| `NavigationServiceTests` | each route loads its scene; a request during a load is ignored; a failed load frees the guard |
| `PopupServiceTests` | one popup at a time; close allows the next |
| `SafeAreaTests` | safe area to anchors, full screen and notched |
| `LevelDefinitionValidatorTests` | one test per level rule in level-data.md §2 |
| `LevelDefinitionTests` | level data survives asset serialization; wall lengths |
| `BoardStateTests` | moves, walls, blocks, door fit on every wall, clear path out, win, fresh restart, `CanStep`, zero allocation |
| `LevelProviderTests` | level number to catalog entry; looping; empty catalog |
| `ActiveBoardTests` | a load sizes the board space and starts fresh; steps announce move, exit (with the cell left from) and clear |
| `BlockDragControllerTests` | free follow and diagonals, nearest-cell commit, fast drags, sliding, slipping into gaps without drift, jitter on a slow push, or locking out of a side corridor or a wobbled-at gap, exit on a small push into the door, lean, grab, snap, exit, zero allocation |
| `OutlineMeshBuilderTests` | the outline is one rounded loop per boundary (1x1, L, ring with a hole), hugs a bar a gap outside it along its bottom edge, rounds its corners, and gives each vertex its outward normal and the rim lift |
| `BlockTileResolverTests` | pieces for 1x1, bars, squares, L, T; inner corner notch; every quarter covered once |
| `BoardFramingTests` | 3x3 to 10x10 boards fit inside the margins at 9:16 and 20:9, centred, as close as possible |
| `BoardSpaceTests` | cell ↔ world at cell size 1 and 2; edges; outside cells |
| `BuildSettingsTests` | Splash first; every `SceneId` is a build scene; portrait; IL2CPP ARM64 |
| `AssetImportTests` | UI sprites, model scale, floor tile size |
| `SceneSetupTests` | every build scene: canvas scaler, safe area root, button feedback |
| `NavigationFlowTests` (PlayMode) | real boot reaches Home; Home ⇄ Gameplay round trip |
| `ButtonPressFeedbackTests` (PlayMode) | press shrinks, release restores; non-interactable and disabled cases |
| `GameplayBoardTests` (PlayMode) | every part and block drawn; swipes take blocks out and clear once; a wrong door holds; outline while held; reload mid-exit; a held block glides after a sudden finger move instead of jumping; an exiting block settles at its door before sliding in, and is cut at the door at full size while debris sprays; following and snapping allocate nothing |
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
