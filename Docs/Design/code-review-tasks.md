# Code Review Tasks

Improvement tasks found by a full read of the runtime and editor scripts (tests excluded), starting
from `RootLifetimeScope` and following every scope, installer, flow, service, view and level tool.
Each task has a stable id (`CR-n`), a priority, the evidence, a fix, and its acceptance, tests and docs.
The summary table owns each task's status; a finished task keeps its block as the record of what changed.

**Evidence level.** Static reading only: every finding was re-checked against the code, and the
booster-button and arrow-material findings against `Gameplay.unity` and the prefabs. No test run,
profiler capture or device run backs this page; items marked *needs profiling* or *needs device* must
be measured before they are fixed.

## 1. Priorities

| Priority | Meaning |
| --- | --- |
| P0 | A player can lose or gain currency or lives against the rules, or sees wrong state |
| P1 | A latent bug with a concrete trigger, or a hole that must close before a planned feature ships |
| P2 | Architecture or maintainability cost with a named failure it will cause |
| P3 | Cleanup |

## 2. Summary

| Id | P | Area | Task | Depends | Status |
| --- | --- | --- | --- | --- | --- |
| [CR-02](#cr-02-home-coin-counter-ignores-spends) | P0 | Home | Home coin counter ignores spends | — | Done |
| [CR-03](#cr-03-booster-is-applied-even-when-no-stock-was-taken) | P0 | Boosters | Booster is applied even when no stock was taken | — | Done |
| [CR-04](#cr-04-a-failed-continue-skips-the-life-cost) | P0 | Lives | A failed continue skips the life cost | — | Superseded |
| [CR-05](#cr-05-two-record-economy-operations-are-not-atomic) | P1 | Economy | Two-record economy operations are not atomic | — | Todo |
| [CR-06](#cr-06-win-reward-is-computed-twice-and-depends-on-handler-order) | P1 | Economy | Win reward is computed twice and depends on handler order | — | Todo |
| [CR-07](#cr-07-booster-target-tap-relies-on-subscription-order) | P1 | Input | Booster target tap relies on subscription order | — | Todo |
| [CR-08](#cr-08-board-input-can-stay-pressed-across-a-background) | P1 | Input | Board input can stay pressed across a background | — | Todo |
| [CR-09](#cr-09-connectivity-loop-dies-on-an-unexpected-error) | P1 | Network | Connectivity loop dies on an unexpected error | — | Todo |
| [CR-10](#cr-10-probe-and-trusted-clock-accept-wrong-answers) | P1 | Network | Probe and trusted clock accept wrong answers | — | Todo |
| [CR-11](#cr-11-remote-level-pack-gate-before-a-fetcher-ships) | P1 | Levels | Remote level pack gate before a fetcher ships | — | Todo |
| [CR-12](#cr-12-pack-revision-can-go-backwards) | P1 | Levels | Pack revision can go backwards | — | Todo |
| [CR-13](#cr-13-arrow-overlay-is-not-clipped-at-the-door) | P1 | Views | Arrow overlay is not clipped at the door | — | Todo |
| [CR-14](#cr-14-booster-button-scale-has-two-owners) | P1 | Views | Booster button scale has two owners | — | Todo |
| [CR-15](#cr-15-level-editor-writes-edits-into-the-wrong-level) | P1 | Editor | Level editor writes edits into the wrong level | — | Todo |
| [CR-16](#cr-16-solver-and-generator-fail-past-their-limits) | P1 | Levels | Solver and generator fail past their limits | — | Todo |
| [CR-17](#cr-17-saves-flush-playerprefs-on-every-change) | P2 | Persistence | Saves flush PlayerPrefs on every change | — | Todo |
| [CR-18](#cr-18-split-leveltimer-countdown-and-freeze) | P2 | Session | Split `LevelTimer` countdown and freeze | — | Todo |
| [CR-19](#cr-19-split-blocklayerview-and-boardviewconfig) | P2 | Views | Split `BlockLayerView` and `BoardViewConfig` | — | Todo |
| [CR-20](#cr-20-split-homeflow-lives-out) | P2 | Home | Split `HomeFlow` lives out | CR-02 | Todo |
| [CR-21](#cr-21-split-leveleditorwindow) | P2 | Editor | Split `LevelEditorWindow` | CR-15 | Todo |
| [CR-22](#cr-22-one-home-for-each-level-rule) | P2 | Levels | One home for each level rule | — | Todo |
| [CR-23](#cr-23-generation-tunables-into-config) | P2 | Levels | Generation tunables into config | — | Todo |
| [CR-24](#cr-24-editor-tools-bypass-the-save-services) | P2 | Editor | Editor tools bypass the save services | — | Todo |
| [CR-25](#cr-25-restart-leaves-debris-and-overlapping-tweens) | P3 | Views | Restart leaves debris and overlapping tweens | — | Todo |
| [CR-26](#cr-26-small-cleanups) | P3 | Mixed | Small cleanups | — | Todo |

## 3. Open product questions

These need a decision before the linked task, or a new `DA-n` in
[development-assumptions.md](../General/development-assumptions.md).

| Id | Question | Why it matters | Recommendation |
| --- | --- | --- | --- |
| Q-2 | Should Freeze be usable before the first grab? | boosters.md §3 counts the freeze "touched or not", so a freeze used before the first grab runs out while the timer stands still anyway; the player pays a booster for nothing | Refuse Freeze until the timer runs (`FreezeBoosterCommand.CanExecute`), or hold the freeze until the first grab |
| Q-3 | Should choosing a target stop the timer? | Activating a target booster pauses the level (DA-36), so the player can study a visible board with no clock, then tap the booster again to put it back for free | Keep the timer running during targeting, or accept it and record it in DA-36 |
| Q-4 | When does the vibration toggle start gating haptics? | DA-10 is open: the toggle saves but haptics still fire; T-25 is Done, so players see a switch that does nothing | Gate `GameplayHaptics` on `ISettingsService` now (one dependency, one test) |

## 4. Tasks

### CR-02 Home coin counter ignores spends

- **Evidence:** `HomeFlow.Start` calls `HomeView.ShowCoins` once (`Game/Home/HomeFlow.cs:50`); only
  `GameplayHudView` subscribes to `CoinsChangedSignal`.
- **Scenario:** on Home with 0 lives, open the lives popup and tap Refill.
- **Effect:** coins are spent, the counter still shows the old balance until the scene reloads.
- **Fix:** `HomeView` subscribes to `CoinsChangedSignal` in `Construct` and unsubscribes in `OnDestroy`,
  the same way `GameplayHudView` does.
- **Accept:** the Home counter drops by the refill price the moment Refill is tapped.
- **Tests:** `HomeFlowTests.Refill_ShouldShowTheBalanceLeft_WhenLivesAreBought`.
- **Docs:** home-qa.md case.

### CR-03 Booster is applied even when no stock was taken

- **Evidence:** `BoosterService.Use` ignores `_inventory.TryUse`'s result and executes the command
  anyway (`Gameplay/Boosters/BoosterService.cs:80`). Stock is checked in `Activate`, but a targeting
  booster spends at the tap, later.
- **Scenario:** a stock change between `Activate` and the target tap (the debug "clear booster" today; a
  second spender such as a sync or a tutorial grant tomorrow).
- **Effect:** the effect is applied for free, breaking DA-36 "a booster is spent only when it works".
  The order also commits the stock before the effect: an exception in `Execute` loses the booster.
- **Fix:** re-check at the operation boundary: `Use` executes the command only when
  `_inventory.TryUse` succeeds; targeting has already ended, so the level plays on unchanged.
- **Accept:** with stock forced to 0 while targeting, the tap changes nothing on the board.
- **Tests:** `BoosterServiceTests.Tap_ShouldLeaveTheBoardAlone_WhenTheStockWentWhileTargeting`.
- **Docs:** boosters.md §2.

### CR-04 A failed continue skips the life cost

- **Evidence:** `FailFlow.OnContinueClicked` opens "Level Failed!" when `TryContinue` fails
  (`Gameplay/FailFlow.cs:54`) without calling `_attempt.Abandon()`; `OnGiveUp` does call it.
- **Scenario:** the continue button is enabled from a stale `CanAfford`, the spend fails, the player taps
  Retry: the level restarts with no life taken.
- **Effect:** small today (the button is disabled when short of coins), but the invariant "every route to
  Level Failed takes the life" lives in two handlers and one forgot it.
- **Fix:** one private `ShowLevelFailed()` that abandons then opens, used by both handlers.
- **Superseded:** the paid continue and the out-of-time popup are gone (DA-30 withdrawn, DA-31): a
  failed level is final and `LevelAttemptLives` takes the life on `LevelFailedSignal`, so no route to the
  level-failed popup can skip it.
- **Accept:** every path that shows "Level Failed!" has taken the attempt's life.
- **Tests:** `LevelAttemptLivesTests.Fail_ShouldCostOneLifeOnce_WhenAStartedAttemptRunsOutOfTime`,
  `LevelResultFlowTests.Fail_ShouldTakeTheAttemptsLife_WhenTheLevelFails`.
- **Docs:** gameplay.md `FailFlow`, lives.md, lives-qa.md, DA-30, DA-31.

### CR-05 Two-record economy operations are not atomic

- **Evidence:** `BoosterShop.TryBuy` saves the wallet then the inventory (`Game/Boosters/BoosterShop.cs:25-27`);
  `LivesService.TryRefill` saves the wallet then lives (`Game/Lives/LivesService.cs:83`); a win advances
  progress then pays coins in two handlers. Each `JsonRecord.Save` flushes on its own.
- **Scenario:** the process dies between the two flushes (OOM kill on low-end Android, battery).
- **Effect:** coins spent with no booster or lives; or a level advanced with no reward.
- **Fix:** `IKeyValueStorage` gains a write-then-flush-once scope (`using (storage.Batch())`); the economy
  operations write both records inside one scope so `PlayerPrefs.Save` runs once. PlayerPrefs gives no true
  transaction, so also order the writes so the player-favourable record lands first (grant before spend)
  and document that choice.
- **Accept:** a fake storage that throws on the second flush leaves both records unchanged, or both changed.
- **Tests:** `BoosterShopTests`, `LivesServiceTests` with a failing fake storage.
- **Docs:** persistence.md (transaction rule), economy.md.

### CR-06 Win reward is computed twice and depends on handler order

- **Evidence:** `LevelRewardService` pays `CoinsFor(level)`; `WinFlow` computes `CoinsFor(level)` again and
  holds the counter at `_wallet.Coins - _reward` (`Gameplay/WinFlow.cs:55`). That is right only because the
  build callback resolves `LevelRewardService` before `WinFlow` (`Scopes/GameplayLifetimeScope.cs:88-89`).
- **Scenario:** the resolve order changes, or a reward rule changes in one place (a bonus, a first-clear
  multiplier).
- **Effect:** the counter starts from the wrong balance, or the popup shows a reward the wallet never got.
- **Fix:** `LevelRewardService` is the single authority: after paying, it fires
  `LevelRewardedSignal(coins, balanceBefore)`; `WinFlow` reads both from it and never calls the provider.
- **Accept:** reordering the two resolves in the build callback changes nothing on screen.
- **Tests:** `LevelResultFlowTests` asserts the popup reward equals the paid amount.
- **Docs:** economy.md, architecture.md (flows read results, never recompute them).

### CR-07 Booster target tap relies on subscription order

- **Evidence:** the same `BoardPointerSignal(Pressed)` reaches `BlockDragController` and `BoosterService`.
  `BoosterService` resumes the session inside the dispatch. The controller ignores the press only because it
  subscribed first (`Scopes/GameplayLifetimeScope.cs` build callback).
- **Scenario:** the resolve order changes.
- **Effect:** the hammer tap on an ice block also grabs it, outlines it and starts the timer (DA-6).
- **Fix:** `BoosterService` consumes the press: the controller only accepts a `Pressed` that began after
  the last `LevelResumedSignal`.
- **Accept:** a target tap never grabs, in either subscription order.
- **Tests:** Edit Mode test subscribing the two in reverse order.
- **Docs:** boosters.md §2, gameplay.md input section.

### CR-08 Board input can stay pressed across a background

- **Evidence:** `BoardInputView._isPressed` clears only in `OnPointerUp` / `OnDisable`
  (`Gameplay/Input/BoardInputView.cs:43-77`); nothing in input reacts to `ApplicationPausedSignal`.
- **Scenario (needs device):** drag a block, take a call or press home; the OS cancels the touch without a
  pointer-up.
- **Effect:** every later press is ignored for the rest of the attempt; at best the block stays held and
  outlined across the background.
- **Fix:** the view subscribes to `ApplicationPausedSignal` in `Construct`; on pause it resets the press and
  fires `Released`.
- **Accept:** background mid-drag, return: the block has settled and the next press works.
- **Tests:** Edit Mode test firing the signal mid-drag; device case in gameplay-qa.md.
- **Docs:** gameplay.md input section.

### CR-09 Connectivity loop dies on an unexpected error

- **Evidence:** `ConnectivityService.RunAsync` catches only cancellation (`Core/Network/ConnectivityService.cs:54-69`);
  `ProbeNowAsync`'s completion source is only completed by a probe result.
- **Scenario:** the probe throws anything but `UnityWebRequestException` (an insecure `http` URL, a
  malformed URL from config).
- **Effect:** `IsOnline` freezes for the session and any caller awaiting `ProbeNowAsync` (T-45 Retry) hangs
  forever. A call after `Dispose` hangs too.
- **Fix:** catch per probe, log through `IGameLogger`, treat as offline, keep looping; return a cancelled
  task from `ProbeNowAsync` after `Dispose`.
- **Accept:** a probe that throws once is followed by a normal probe on the next interval.
- **Tests:** `ConnectivityServiceTests` with a throwing fake probe and a post-dispose call.
- **Docs:** network.md.

### CR-10 Probe and trusted clock accept wrong answers

- **Evidence:** `UnityWebRequestProbe` accepts any 2xx (`Core/Network/UnityWebRequestProbe.cs:23`) while
  DA-25 expects 204; `TrustedClock.TryRecord` takes any `Date`, backwards or far ahead.
- **Scenario:** a captive portal answers 200 with its own page and `Date`; a proxy shifts `Date` forward.
- **Effect:** the game claims to be online behind a portal; lives refill faster against a moved clock.
- **Fix:** require `204`; in `TryRecord`, ignore a jump against the monotonic extrapolation beyond a
  tolerance in `NetworkConfig`, and log it. Check on device that `Time.realtimeSinceStartup` keeps counting
  in the background; if it does not, the extrapolation lags until the resume probe lands.
- **Accept:** a 200 probe reads offline; a `Date` 1 h behind the extrapolation is ignored.
- **Tests:** `TrustedClockTests`, probe result tests.
- **Docs:** network.md, DA-25.

### CR-11 Remote level pack gate before a fetcher ships

- **Evidence:** `LevelPackService.TryStoreRemote` (`Game/Levels/Packs/LevelPackService.cs:58-73`) runs only
  the structural validator and compares the revision only with the bundled pack. No runtime code calls it
  yet, so this is a gate for the fetcher, not a live bug.
- **Scenario:** two blocks with ice 1 each pass the validator (1 ≤ other blocks) and deadlock; a CDN serves
  rev 14 after rev 15 was stored. The editor's "Copy Level Pack for Remote Config" copies without solving,
  and Save skips the "Save anyway?" prompt while the solver is still running.
- **Effect:** once a fetcher exists: players stuck on an unwinnable level, burning lives; or a silent
  rollback to older levels.
- **Fix:** reject `revision <` the stored remote pack's; make the copy command and Save require a finished
  solve that is not `Unsolvable`; optionally solve in `TryStoreRemote` on a worker with a cap
  (`Unknown` accepted).
- **Accept:** the stale and the deadlocked packs are both refused with a message naming the reason.
- **Tests:** `LevelPackServiceTests` for both; an editor test for the copy gate.
- **Docs:** level-packs.md, DA-41.

### CR-12 Pack revision can go backwards

- **Evidence:** `LevelCatalogEditing.WritePack` restarts at revision 1 when the existing `levels.json`
  cannot be read (`Editor/LevelEditing/LevelCatalogEditing.cs:33-38`).
- **Scenario:** a merge-conflict marker in `levels.json`, then any editor save.
- **Effect:** the build ships rev 1; players holding a stored remote pack keep the old remote levels.
- **Fix:** throw on an unreadable existing file; never write a revision lower than the last one.
- **Accept:** a corrupt `levels.json` stops the save with an error.
- **Tests:** `LevelCatalogEditingTests`.
- **Docs:** level-packs.md.

### CR-13 Arrow overlay is not clipped at the door

- **Evidence:** `BlockLayerView` swaps only the block's own parts to the clipping exit material
  (`Gameplay/View/BlockLayerView.cs:150-155`, `:239-259`); the child `ArrowBlockerView` keeps `M_Arrow`
  and is removed only in `Recycle`.
- **Effect:** the block vanishes into the door while its arrow slides on over the wall, then pops.
- **Fix:** undress blocker overlays when the exit starts (`OnBlockExited`), before `PlayExit`.
- **Accept:** no overlay is visible past the door plane.
- **Tests:** `GameplayBoardTests` asserts no blocker view stays attached once an exit starts.
- **Docs:** blockers.md views section.

### CR-14 Booster button scale has two owners

- **Evidence:** on all four booster buttons in `Gameplay.unity`, `ButtonPressFeedback._target` is the same
  `RectTransform` that `BoosterButtonView.SetSelected` scales (`UI/ButtonPressFeedback.cs:31-56`,
  `Gameplay/Hud/BoosterButtonView.cs:45-50`).
- **Scenario:** press the selected booster and slide the finger off.
- **Effect:** the button springs to 1.0 while targeting is still on. Selection only looks right because its
  tween (0.2 s) outlasts the release tween (`UiConfig`, 0.18 s); tuning either breaks it.
- **Fix:** press feedback on a child visual, selection scale on the root.
- **Accept:** press-and-slide-off keeps the selected scale.
- **Tests:** Play Mode press, slide off, check scale.
- **Docs:** ui.md button feedback rule (one scale owner per transform).

### CR-15 Level editor writes edits into the wrong level

- **Evidence:** `LevelEditorWindow.ConfirmLeave` returns true even when `Save()` refused (`:853-862`);
  `Load` records undo on the draft but not the level index (`:841-849`); Generate / Make harder apply their
  result to whatever level is open when they finish (`:978-1035`); `HandleShortcuts` ignores
  `EditorGUIUtility.editingTextField` (`:667-701`).
- **Effect:** lost drafts; Ctrl+Z then Ctrl+S overwrites another level; typing a seed switches tools or
  deletes the selected block.
- **Fix:** `Save()` returns success and leaving waits for it; clear draft undo on switch; capture the target
  level and drop a late result; skip shortcuts while a text field has focus.
- **Accept:** each scenario above leaves the other levels untouched.
- **Tests:** level-editor-qa.md cases; Edit Mode tests where the logic moves out in CR-21.
- **Docs:** level-editor.md.

### CR-16 Solver and generator fail past their limits

- **Evidence:** `SearchBoard` caps boards at 128 cells (`Game/Levels/Solving/SearchBoard.cs:12, 28-29`)
  while `LevelRulesConfig._maxBoardSize` has no matching clamp; `LevelChecker.SolveAsync` catches only
  cancellation; a mid-run cancel of `LevelGenerator` escapes `Parallel.For` as `AggregateException`
  (`Game/Levels/Generation/LevelGenerator.cs:72-89`), and the test only cancels before the start.
- **Effect:** a 12×12 level validates, then the checker shows "Solving…" forever; a cancel can surface as
  an error.
- **Fix:** clamp the board size so width × height ≤ `SearchBoard.MaxCells` (or validate it); clear
  `IsSolving` and show other exceptions; throw cancellation on the options token inside the loop body.
- **Accept:** a too-big board is a validator error; a mid-run cancel ends quietly.
- **Tests:** a mid-run cancel test; a validator case for the cell cap.
- **Docs:** level-editor.md, level-data.md.

### CR-17 Saves flush PlayerPrefs on every change

- **Evidence:** `JsonRecord.Save` calls `Flush` (`PlayerPrefs.Save`) on every change; a win saves progress
  and wallet in one frame; the stored remote pack would sit in the same file (`LevelPackService.cs:70`).
- **Effect (needs profiling):** on Android `PlayerPrefs.Save` rewrites the whole prefs XML synchronously;
  with a remote pack of hundreds of KB that is a frame hitch on every coin or booster change.
- **Fix:** measure first (T-38). Then: the remote pack goes to a file in `persistentDataPath`, written via
  temp-and-rename, behind its own store; the batch scope from CR-05 cuts flushes to one per operation.
- **Accept:** a profiler capture of a win and a booster use on a mid-range device, before and after.
- **Tests:** storage fake counts flushes per operation.
- **Docs:** persistence.md, performance.md (measured numbers).

### CR-18 Split `LevelTimer` countdown and freeze

- **Evidence:** `LevelTimer` (249 lines, nine signals) owns the countdown, the freeze countdown and
  app-pause handling, with two parallel `CancellationTokenSource` state machines.
- **Cost:** the freeze rules (Q-2) and a future time booster each change this one class; its `Refresh`
  decides both runs at once.
- **Fix:** a `FreezeClock` owns the freeze seconds and announces `TimerFreezeChangedSignal`; `LevelTimer`
  asks it `IsFrozen` and listens for its end. Behaviour unchanged.
- **Accept:** `LevelTimerFreezeTests` and `LevelTimerTests` pass unchanged.
- **Tests:** `FreezeClockTests` moved out of `LevelTimerFreezeTests`.
- **Docs:** boosters.md §3.

### CR-19 Split `BlockLayerView` and `BoardViewConfig`

- **Evidence:** `BlockLayerView` (390 lines, 11 signals, 5 pools) assembles blocks, dresses blockers, plays
  the drag, the exits, the debris, the thaws and the targeting outline. `BoardViewConfig` mixes geometry,
  prefabs, FX and drag tuning, so `BlockDragController` depends on a view config.
- **Cost:** a new blocker kind (blockers.md) edits `Build`, `Undress`, a pool and a dictionary in this class.
- **Fix:** extract `BlockerOverlayPresenter` and `BoardDebrisView`; split `BlockDragConfig` and
  `BoardFxConfig` out of `BoardViewConfig` with `FormerlySerializedAs` where fields move.
- **Accept:** no visible change; `DragAllocationTests` still 0 B.
- **Tests:** existing board view tests; installer resolution test for the new configs.
- **Docs:** blockers.md "how to add a kind", gameplay.md, art-pipeline.md if prefab fields move.

### CR-20 Split `HomeFlow` lives out

- **Evidence:** `HomeFlow` routes the level button, the settings popup and the lives popup with ten
  dependencies; Gameplay already has `GameplayLivesFlow`.
- **Fix:** `HomeLivesFlow` owns the lives bar "+", the lives popup and Refill.
- **Accept:** Home behaviour unchanged.
- **Tests:** `HomeFlowTests` split along the same line.
- **Docs:** lives.md §2.

### CR-21 Split `LevelEditorWindow`

- **Evidence:** 1,077 lines holding menus, asset paths, shortcuts, tool state, undo, five panels, async
  generate/harden and catalog navigation; CR-15's bugs come from that mix.
- **Fix:** extract `LevelEditorShortcuts`, `DraftCommands` (change, undo, level index), `ChecksPanel`,
  `GeneratorPanel` (owns its async job and target level), `LevelPackMenu`, `LevelEditorAssets`.
- **Accept:** level-editor-qa.md passes unchanged.
- **Tests:** Edit Mode tests for `DraftCommands` and the shortcut map.
- **Docs:** level-editor.md.

### CR-22 One home for each level rule

- **Evidence:** door overlap is written four times (`LevelDefinitionValidator.cs:126-133`,
  `Editor/LevelEditing/LevelDraft.cs:180-188`, `LevelGenerator.cs:172-178`, `LevelHardener.cs:168-174`);
  edge connectivity twice; `LevelDefinitionValidator.ArrowAllows` re-implements `ArrowBlocker.AllowsStep`;
  wall length about six times.
- **Cost:** a rule change in one copy lets the editor, the generator and the game disagree on a level.
- **Fix:** `DoorData.Overlaps`, a shared cell-connectivity check, `WallSide.IsHorizontal`,
  `ArrowAxis.Reaches(WallSide)` in Game, used everywhere. Instance methods, not static helpers.
- **Accept:** each rule has one implementation (a grep finds one).
- **Tests:** the existing validator, generator and draft tests pass unchanged.
- **Docs:** level-data.md.

### CR-23 Generation tunables into config

- **Evidence:** the DA-33 difficulty bands and timer formula live in `LevelGenerator.ScoreFor` / `TimerFor`
  (`:29-46`); hardener mix, state caps, attempt counts and new-level defaults are constants in
  `LevelHardener` and `LevelEditorWindow`. The difficulty decides the coin reward (economy.md), so the bands
  are an economy tunable. Make harder also leaves the label and timer as they were (`LevelHardener.cs:117`).
- **Fix:** a `LevelGenerationConfig` registered in `ConfigProvider`; after hardening, offer the suggested
  difficulty and timer, or warn in Checks when they disagree.
- **Accept:** no magic numbers left in the generator and hardener.
- **Tests:** `ShippedLevelsTests` reads the bands from config.
- **Docs:** level-editor.md, economy.md, DA-33.

### CR-24 Editor tools bypass the save services

- **Evidence:** `LevelPlayTest` writes `player_progress_v1` to PlayerPrefs itself
  (`Editor/LevelEditing/LevelPlayTest.cs:23-30`); `GameDebugWindow`'s clear list misses `lives_v1` and the
  remote pack key (`Editor/Debugging/GameDebugWindow.cs:17-21`); a stored remote pack overrides the level
  just edited during a play-test.
- **Fix:** play-test goes through `PlayerProgressService` and ignores the remote pack; clear-saves reads one
  list of record keys owned next to the records.
- **Accept:** after "Clear player saves" every record is fresh; play-test plays the edited level.
- **Tests:** an Edit Mode test that the key list covers every `StorageKey`.
- **Docs:** debug-tools.md, level-editor.md.

### CR-25 Restart leaves debris and overlapping tweens

- **Evidence:** `BlockLayerView.ReleaseAll` does not stop active bursts or clear `_lastBursts` (`:363-371`);
  `BlockView.Leave` does not kill `_thaw`, and `PlayExit` duplicates `Leave` (`BlockView.cs:114-121, 149-154`);
  `CoinFlightView` never turns its input blocker off or hides unlanded coins after a cancel (`:87-93`).
- **Effect:** debris spraying from an empty door after a restart; two tweens on one block's scale when the
  vacuum takes a block that just thawed; a stuck blocker the day a "next level" route exists.
- **Fix:** release bursts in `ReleaseAll`; `Leave` kills `_thaw` and `PlayExit` calls `Leave`; a
  `try/finally` in the coin flight.
- **Accept:** restart mid-exit shows a clean board.
- **Tests:** `GameplayBoardTests` restart-mid-exit case.
- **Docs:** none.

### CR-26 Small cleanups

| Item | Where | Fix |
| --- | --- | --- |
| `LevelSession.Next` has no runtime caller (the win goes Home) | `Gameplay/Session/LevelSession.cs:61` | Remove with its test, or keep it for a "next level" route and say so in gameplay.md |
| `mm:ss` written three times, two behaviours (wrap at 100 vs clamp at 99) | `GameplayHudView.cs:94-101`, `LivesBarView.cs:56-63`, `OutOfLivesPopupView.cs:88-96` | One allocation-free clock-digits writer with tests |
| Exit timing computed twice with different formulas | `BoardView.cs:183`, `BlockView.cs:161`, `BlockLayerView.cs:247` | `ExitPath.SlideSeconds(config, alreadyIn)` |
| Player-facing text hard-coded in views; blocks DA-29 | `OutOfLivesPopupView.cs:35-38`, `LivesBarView.cs:13`, `HomeView.cs:12`, `OutOfTimePopupView.cs:12-13` | Text in prefabs or a config |
| Dead field `_level` | `BoardView.cs:32,64` | Remove |
| `foreach` over `IReadOnlyList` boxes an enumerator on every exit | `Gameplay/Board/ActiveBoard.cs:97` | Index loop |
| Block build allocates two `HashSet`s and a `List`, plus `GetComponentInChildren` per part (needs profiling) | `BlockTileResolver.cs:12-14`, `BlockLayerView.cs:144`, `BoardView.cs:117` | Reuse collections; cache renderers in `PartPool` |
| Comma-joined enum names parse as a third value (`"Red, Orange"` → `Yellow`) | `Game/Levels/Packs/LevelPackJson.cs:129-134` | Reject names containing `,` |
| Static methods hold runtime logic, against CLAUDE.md | `LevelPackJson.cs`, `LevelGenerator.Fits`, `LevelHardener.Better/Overlaps`, `LevelPackService.FirstError`, `SearchBoard.FitsAt/FreeCellOutside` | Instance methods |
| Wallet add is unchecked; past `int.MaxValue` it goes negative and the next load resets to 0 | `Core/Persistence/WalletService.cs:48-52` | Clamp at a configured cap |
| A corrupt save is overwritten and lost | `Core/Persistence/JsonRecord.cs:30-39` | Keep the raw string under `<key>_corrupt` before starting fresh |
| A second navigation request returns a completed task | `Game/Navigation/NavigationService.cs:25-26` | Return the in-flight task |

## 5. What holds up

- The board rules are pure and authoritative (`BoardState`), and the solver asks a real `BoardState` to
  build its tables, so the solver cannot drift from the movement rules.
- `LevelSession` refuses out-of-state commands, which already absorbs most double taps and late events
  (the win-delay restart, a pause during a win, a booster tap after a fail).
- Persistence is disciplined: versioned keys, read-before-load throws, save before announce, validity on load.
- Views subscribe in `Construct` and unsubscribe on destroy; tweens are unscaled, linked and killed; the drag
  path is measured at 0 B.
- Boot steps are typed, ordered and pinned by `InstallerResolutionTests`.
