# Testing

> **Binding agent rules.** Read this page before adding or changing a test or a test assembly.

## 1. Layout

| Assembly | Folder | Covers |
| --- | --- | --- |
| `RollicCase.Tests.EditMode` | `Assets/_Project/Tests/EditMode/` | pure logic, DI wiring, build settings |
| `RollicCase.Tests.PlayMode` | `Assets/_Project/Tests/PlayMode/` | real scenes end to end |

Shared fakes live in `Tests/EditMode/Helpers/`: `InMemoryKeyValueStorage`, `RecordingLogger`,
`FakeBootStep`, `RecordingProgress`. Reuse them before writing a new fake.

## 2. Current suite

| Fixture | Pins |
| --- | --- |
| `SignalBusTests` | delivery, order, type isolation, duplicate guard, (un)subscribe during fire |
| `BootSequenceTests` | step order, weighted progress, empty sequence, cancellation, weight validation |
| `PlayerProgressServiceTests` | defaults, read-before-load, persist + flush, corrupt and invalid data |
| `InstallerResolutionTests` | every installer builds; one SignalBus; boot step order |
| `GameplayFlowTests` | the level announced matches saved progress |
| `BuildSettingsTests` | Splash first; every `SceneId` is a build scene |
| `SplashToGameplayTests` (PlayMode) | real boot reaches Gameplay and the HUD shows the level |

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
