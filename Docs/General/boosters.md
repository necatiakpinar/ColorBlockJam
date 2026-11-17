# Boosters

> **Binding agent rules.** Read this page before touching `Scripts/Game/Boosters/`,
> `Scripts/Game/Gameplay/Boosters/`, the booster bar or the booster popup.

Four boosters sit at the bottom of the gameplay HUD, left to right (T-36, DA-36):

| Booster | Use | What it does |
| --- | --- | --- |
| Freeze | at once | the level timer stands still for `BoosterConfig.FreezeSeconds` (10 s) of play |
| Rocket | tap a block | blasts the tapped cell out of the block; if the rest falls apart, every piece but the biggest becomes a block of its own |
| Hammer | tap a block | smashes the whole block; on a frozen block it breaks the ice instead (DA-37) |
| Vacuum | tap a block | clears every block of the tapped block's colour |

A block a booster takes counts as gone, like one that left through its door: taking the last one wins the
level.

## 1. Stock and shop

- **`BoosterInventory`** (Root scope, `boosters_v1`, [persistence.md](persistence.md)) holds how many of
  each the player owns. A booster missing from the save starts at its `StartingCount` (1 each). `TryUse`
  takes one; every change is saved at once and fires `BoosterCountChangedSignal`.
- **`BoosterShop`** (Gameplay scope) sells one for its `Price` in coins: the wallet pays first
  (`IWalletService.TrySpend`), then the stock grows ([economy.md](economy.md) §3).
- **`BoosterConfig`** (Root scope, `Configs/BoosterConfig.asset`) holds per booster the starting stock, the
  price, the title, the hint shown while choosing a target, the description and the icon of the purchase
  popup, and the freeze length.

## 2. Using one

Each booster's effect is a command (`Boosters/Commands/`): an **`IBoosterCommand`** carries its `Type`,
whether it `NeedsTarget`, whether it `CanExecute` now, and `Execute(in BoosterTarget)`. `GameplayInstaller`
registers one per booster (`FreezeBoosterCommand`, `RocketBoosterCommand`, `HammerBoosterCommand`,
`VacuumBoosterCommand`). **Never branch on `BoosterType` in the service: a new booster is a new command
and one registration.** Two commands for one booster fail the build; a booster without one throws on use.

**`BoosterService`** (Gameplay scope, built by the scope's build callback) owns the rules every booster
shares, and looks the command up by type:

1. `Activate(type)` answers `NeedsPurchase` when the player has none, `Refused` when the level is not
   playing, a target is being chosen, or the command cannot execute (a freeze is already on), else:
   - **No target (Freeze)**: takes one, executes the command (`LevelTimer.FreezeFor`, §3) and fires
     `BoosterUsedSignal`.
   - **Needs a target (Rocket, Hammer, Vacuum)**: pauses the level through `LevelSession.Pause` (no timer,
     no dragging) and fires `BoosterTargetingChangedSignal(type, true)`. Nothing is spent yet.
2. While choosing, a press on a board cell that holds a block (`BoardPointerSignal`) picks the target:
   targeting ends, the level resumes, one booster is taken, then the command changes the board
   (`ActiveBoard.RemoveCell`, `RemoveBlock`/`Thaw` or `RemoveColor`), and `BoosterUsedSignal` fires. A press
   on an empty cell does nothing.
3. `CancelTargeting` gives up: the booster is kept and the level resumes. A restart drops the choice.

`ActiveBoard` announces the change: `BlockRemovedSignal` (with `BlockRemovalKind` Smashed, Blasted or
Vacuumed and an order for the vacuum's stagger), `BlockCellRemovedSignal` when a rocket leaves the block
with cells, and `BoardClearedSignal` once when nothing is left. When the blasted block falls apart, the
biggest piece (the first on a tie) stays the block and every other piece becomes a new block of the same
colour and ice, with the next free id, where it lies (`BoardState.JustSplitOff`); `BlockSplitOffSignal`
announces each one and `BlockLayerView` gives it a view of its own. Each piece then moves and leaves by
itself. `BoardBlock.StartCells` holds the cells a block still has; the views read exit paths from it, not
from the level data.

## 3. The freeze

`LevelTimer.FreezeFor(seconds)` stops the count; the freeze's own seconds pass only while the level plays
and the app is in front, touched or not, so a pause or the background holds both. It does not stack. When
it runs out the count picks up where it stopped; a restart ends it. `TimerFreezeChangedSignal` carries the
seconds left, the total and whether it is counting.

## 4. On screen

| Piece | Where | Shows |
| --- | --- | --- |
| `BoosterBarView` + four `BoosterButtonView` | `HudCanvas/SafeArea/BoosterBar` | the stock in a blue badge, or a green "+" when none are left; the booster choosing a target grows, the others fade; the freeze fades while it runs |
| `BoosterTargetingView` | `SafeArea/BoosterBanner` + `BoosterBackdrop` (scene root) | while choosing: the title and hint, the top bar parts faded, and a dark quad under the board that shades everything around it (`BoosterDimAnimation`) |
| `FreezeView` | `HudCanvas/IceFrame` + `TopBar/TimePanel/FreezeMeter` | an ice frame (`icetexture`) around the screen and a meter under the timer that empties with the freeze |
| `BlockLayerView` | `Blocks` | every block outlined while choosing; smash (`BlockSmashAnimation`) and a burst of cubes; the vacuum lifts blocks away in turn (`BlockVacuumAnimation`); a blasted cell rebuilds the block from the cells left |
| `BoosterPurchasePopupView` | `BoosterPurchasePopup.prefab` (`GameplayPopupConfig`) | the booster's title, icon, "x1", description and a Buy button with the price, disabled when the coins are short |

Timings and fades are tunables: `GameplayConfig` (Boosters header) for the HUD, `BoardViewConfig` (Booster
removal) for the blocks.

## 5. Routing

**`BoosterFlow`** routes the bar: a tap activates; tapping the booster that waits for a target puts it back,
tapping another switches to it. `NeedsPurchase` pauses the level and opens the purchase popup; Buy goes
through `BoosterShop`, and both Buy and close resume. Buying does not use the booster. `GameplayFlow` puts a
waiting booster back before the pause button pauses.

## 6. Tests

`BoardRemovalTests`, `BoosterInventoryTests`, `BoosterServiceTests`, `InstallerResolutionTests` (one command per
booster), `LevelTimerFreezeTests`,
`GameplayDebugCommandsTests` (Edit Mode); `BoosterFlowTests` (Play Mode, through the real HUD and input).
Manual checks: [boosters-qa.md](../testing/boosters-qa.md).
