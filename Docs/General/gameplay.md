# Gameplay

> **Binding agent rules.** Read this page before touching `Scripts/Game/Gameplay/` or the Gameplay
> scene. Sections are added as the design tasks land (rules, view, flow, timer, economy).

## 1. Board space

1. **Level data, rules and the level editor work in cells only.** Cell `(0, 0)` is the bottom-left cell;
   `x` grows right, `y` grows up the screen.
2. **`BoardSpace` is the only conversion between cells and world units** (Gameplay scope). The board lies
   on the XZ plane at Y = 0, centred on the world origin; cell `y` maps to world +Z. It also projects
   touch rays onto that plane (`TryProjectToBoard`). `ActiveBoard.Load` calls `SetSize` for every level.
3. The world size of a cell is `BoardViewConfig.CellSize` (1, matching the 0.5 model import scale in
   [art-pipeline.md](art-pipeline.md) §3). No code assumes a cell is 1 world unit (`BoardSpaceTests` run
   at cell size 1 and 2). Views lay their parts out in cell units under a root that `BoardSpace` places
   and scales (§5.1), so cell-unit constants such as a cell centre's `0.5` are fine there.

## 2. Input

1. The board camera is registered with the Gameplay scope (`RegisterComponent`) and injected as `Camera`;
   `Camera.main` is never used.
2. `BoardInputView` sits on a `BoxCollider` hit area it resizes to the board on every
   `BoardLoadedSignal`.
   It implements `IPointerDownHandler`, `IDragHandler` and `IPointerUpHandler`; the camera's
   `PhysicsRaycaster` delivers the events. No polling, no `Update`.
3. Each event is projected onto the board plane through `BoardSpace` (not the collider, so a drag past
   the edge still reports a cell) and fired as **`BoardPointerSignal`** (`Pressed`, `Dragged`,
   `Released`) with the cell and world point. The cell may lie outside the board while dragging;
   consumers check `BoardSpace.Contains`.
4. **One finger at a time** (DA-4): events from other pointers are ignored until the first one is
   released. A release whose ray misses the board plane still ends the touch at the last cell. Disabling
   the view drops a stale press **without** firing `Released`, so a consumer that tracks a drag resets
   it when it disables the board (`BoardInputTests`).
5. UI above the board wins the EventSystem raycast, so touches on HUD buttons never reach the board.

## 3. Rules

`BoardState` (pure C#) holds the rules of one attempt. It is built fresh from `LevelData` on every start
and restart, so no state survives an attempt. `BoardStateTests` pins every rule below.

1. **Moves are one cell at a time.** `TryStep(blockId, direction)` takes exactly one cell right, left,
   up or down and returns `Moved`, `Blocked` or `Exited` (DA-4). A block moves when every cell it enters
   is free or its own.
2. **Exit** (DA-5): when a step would take a block past a wall, it leaves only if
   - a door on that wall has the block's colour,
   - the door spans every cell the block covers along that wall (a wider door is fine), and
   - nothing stands between any of the block's cells and that wall (it slides out in a straight line).
   Otherwise the step is `Blocked` and nothing changes. `DoorResolver` answers the first two checks.
3. **Win**: `IsCleared` turns true when the last block exits. Losing is the timer's job (T-15).
4. A step allocates nothing; invalid input (a diagonal step, moving an exited block, overlapping start
   cells) throws.

## 4. Loading and controls

1. `GameplayFlow.Start` starts the attempt through `LevelSession.Start` (§6), which asks `LevelProvider`
   for the level (the catalog loops after its last level, DA-8), loads it into `ActiveBoard` and then
   fires `LevelStartedSignal`. `ActiveBoard.Load` builds a new
   `BoardState`, sizes `BoardSpace` and fires **`BoardLoadedSignal`**; every board view rebuilds from it.
2. **`ActiveBoard.TryStep`** is the only way a block moves during play. It calls `BoardState.TryStep` and
   announces the outcome: `BlockMovedSignal` per cell, `BlockExitedSignal` on an exit and
   **`BoardClearedSignal`** once, after the exit of the last block. Anything that moves blocks (the drag)
   goes through it; boosters take blocks away through its `RemoveBlock`, `RemoveCell` and `RemoveColor`,
   which fire `BlockRemovedSignal` / `BlockCellRemovedSignal` and the same `BoardClearedSignal`
   ([boosters.md](boosters.md)).
3. **`BlockDragController`** (`Input/`, plain class, built by the scope's build callback) lets the finger
   slide the touched block like a solid object (DA-4):
   - a press fires **`BlockGrabbedSignal`**; a press that misses every block still picks the block whose
     cell is nearest the finger when it is at most `BoardViewConfig.TouchTolerance` (0.35 cell) away. The
     block aims at the finger plus the offset it was grabbed at, which never changes, so it cannot drift away from the finger. It follows freely in fractional
     cells, diagonals included, firing **`BlockDraggedSignal`** on every drag event;
   - collisions work like tile physics: each pass moves it along the axis the finger is furthest along,
     then the other, as far as every cell its footprint sweeps through is free (`BoardState.IsCellFree`);
     a block between rows needs both rows free. It slides along an obstacle instead of sticking, and a
     fast drag crosses any number of free cells in one event but never passes through a block;
   - pushed into a one-cell gap from at most `BoardViewConfig.CornerTolerance` off the cell line, the
     stopped part of the push slips it sideways onto that line (only if the line opens the way), and the
     finger cannot pull it back out during that event. The slip helps only the way the finger is mostly
     going (the stopped push is at least the finger's sideways offset), so a wobbling finger never slips
     the block along its main axis. While the finger mostly pushes along one axis and that push is free
     on the block's cell line but stopped off it, the block does not leave the line sideways; a pull
     within the tolerance that only undoes the last slip's offset to the finger neither counts as such a
     push nor sets off a slip of its own. Nothing is locked from one event to the next, so a slow push
     enters a gap without jitter and a block can always turn into any other gap;
   - the drawn position may lean toward the finger past an obstacle by `BoardViewConfig.HoldGive`, so a
     held block with no room still gives a little; the lean is never committed;
   - whenever its nearest cell changes the move is committed (`ActiveBoard.TryStep`), so the board always
     holds the cell it settles on;
   - lined up with a fitting door (`BoardState.CanStep` says `Exited`), the wall stops being an obstacle
     and the block exits as soon as it is pushed `BoardViewConfig.ExitPush` (0.15 cell) into the door;
   - a door the block cannot use (another colour, or too narrow) is a wall to it: no feedback (DA-21);
   - release fires **`BlockDroppedSignal`** with the cell it settles on; nothing snaps before release;
   - input counts only while the level plays: `LevelPausedSignal`, `LevelCompletedSignal` and
     `LevelFailedSignal` drop the held block onto its cell (`BlockDroppedSignal`) and the finger is ignored
     until `LevelStartedSignal` or `LevelResumedSignal` (DA-22).
   A drag allocates nothing (`BlockDragControllerTests`).

## 5. View

All board views listen to signals only; the model has committed every change before a view sees it.
`BoardLoadedSignal` carries the level as authored, never the live `BoardState`, so views cannot change it.

1. Board views parent their parts to a root at `BoardSpace.Origin` scaled by `CellSize`, so parts are laid
   out in cell units: cell `(x, y)` is centred at `(x + 0.5, 0, y + 0.5)`.
2. **`BoardView`** builds, per load: a base under the floor tiles (the gaps read as grid lines), one tile per
   cell, the wall ring (`BoardViewConfig.WallThickness` outside the board) and four rounded corners. The
   wall and door meshes are straight extrusions, so each stretch of wall between doors or corners is one
   segment stretched along it and each door is one segment stretched over its cells, in the door's
   colour, with one `Door_Arrow` centred on top pointing out of the board (a child of the door, so it
   sinks with it, DA-24): the ring reads as continuous bars on any board size
   (`GameplayBoardTests.Board_ShouldCloseTheWallRingWithoutSeams_WhenWallsAreStretchedBetweenDoors`).
3. **`BlockLayerView`** builds every block from the modular pieces chosen by **`BlockTileResolver`**
   (art-pipeline.md §4) in its palette colour. While a block is held it shows an outline band that hugs
   its footprint (`OutlineMeshBuilder`): a closed loop `OutlineGap` outside the block, its corners
   rounded like the block models (`BlockCornerRadius`), so it follows any polyomino, notches and holes
   included. It traces the block's on-screen silhouette: shader `ColorBlockJam/Selection Outline` keeps a
   side the camera sees outlined along its bottom edge (`OutlineBaseHeight`) and lifts a side turned away
   to the rim of the top face (`BlockTopHeight`). A block high on screen shows more of its front face, so
   the band runs further below its top face there than near the bottom of the screen. The shader draws
   over everything, so the outline stays visible where the block goes under another object. On every
   `BlockDraggedSignal` the view glides the block toward where the finger has it with the follow tween
   (`FollowDuration`, `FollowEase`), retargeted each event, so the drawn block trails the finger slightly
   and never jumps on a fast flick (the board model follows the finger at once; only the drawing trails);
   on `BlockDroppedSignal` it settles it onto its cell with the snap tween (`SnapDuration`, `SnapEase`);
   on `BlockExitedSignal` (which carries the cell offset the block left from) it plays
   `BlockExitAnimation`: from wherever the block is drawn it first settles lined up at the mouth of its
   door (`ExitSnapDuration`, `ExitSnapEase`; never backing away from it), then slides through the door at
   `ExitSpeed` cells per second, its own length plus `ExitCutOffset` and `ExitClearance`, at full size.
   Its pieces switch to the view's copy of `BlockExitMaterial` (`ColorBlockJam/Block Exit`: lit like
   Simple Lit, cut by a world plane `ExitCutOffset` (0.15 cell) past the board's edge, inside the door,
   so the block covers the floor's edge the sinking door uncovers; its back faces are drawn too, so the
   cut reads as a solid face where it faces the camera, at the bottom wall), so it disappears into the
   door; meanwhile an `ExitBurstView` (pooled `BlockExitBurst.prefab`) sprays cubes of its colour out of
   the door, outside the wall ring, for as long as the slide lasts, starting when it begins
   (`ExitDebrisBurst` cubes per cell of width at once, then `ExitDebrisRate` per cell of width per
   second, in a 30° cone pointing out of the door). The view then returns to its pool, and the burst once
   its last cube is gone (DA-20). A block view's pivot is the block's centre. The follow and the snap are
   one paused tween each per block, retargeted on every drag and drop (`BlockMoveAnimation`), so
   following and snapping allocate nothing
   (`GameplayBoardTests.BlockView_ShouldNotAllocate_WhenFollowingAFingerAndSnapping`). A board reload
   also recycles blocks that are still sliding out
   (`GameplayBoardTests.Reload_ShouldRebuildCleanBlocks_WhenABlockIsStillSlidingOut`).
   Blocks leaving the same door within `ExitBurstCoalesceWindow` (0.15 s) share one debris spray:
   the later ones add their cubes to it. The exit's geometry (wall, length, door span) comes from one pure
   `ExitPath`, shared by the block view and the board view.
   On `BlockExitedSignal` the **`BoardView`** opens the door: the whole door, however narrow the block, sinks
   `DoorGateDepth` into the floor while the block settles, stays down while it slides through, then rises
   back (`DoorGateAnimation`, `DoorGateCloseDuration`) (DA-21).
   **`GameplayHaptics`** (plain class, Gameplay scope, built by the scope's build callback) taps the
   device when a block leaves: `GameplayConfig.ExitHaptic` (Light) on `BlockExitedSignal`. The taps go
   through the root scope's `IHapticService` (`Core/Haptics/MobileHapticService`): Android
   `VibrationEffect` predefined effects (one-shot pulses before API 29; the VIBRATE permission comes from
   `Plugins/Android/ColorBlockJamHaptics.androidlib`), iOS `UIImpactFeedbackGenerator`
   (`Plugins/iOS/ColorBlockJamHaptics.mm`), silent in the editor. No third-party haptics package. The
   Settings vibration toggle does not gate it yet (T-25).
4. Every part, block view and piece comes from a pool (`PartPool`, `ObjectPool`), so a restart or the next
   level reuses objects. Colours are one shared material per colour (`ColorMaterialCache`) to keep the SRP
   batcher batching.
5. **`BoardCameraView`** keeps the authored tilt (72°) and field of view (30°) and moves the camera so the
   board and its walls fit between `BoardViewConfig.Framing` margins (HUD on top, boosters below), centred
   in that band, for the screen's aspect ratio (`BoardFraming`, `BoardFramingTests`). It frames on every
   board load; the device aspect never changes (portrait only), so there is no re-fit on resize.

## 6. Level session

**`LevelSession`** (plain class, Gameplay scope) owns the attempt at the current level: its `LevelState`
and `LevelNumber`. It is the only place the state changes; every transition commits the state first and
then fires its signal. Anything not in the table is refused (the method returns false, nothing fires),
so a double tap or a late timer cannot corrupt an attempt (`LevelSessionTests`).

```text
Loading --Start(n)--> Playing --Pause--> Paused --Resume--> Playing
Playing --(BoardClearedSignal)--> Won          Playing --Fail--> Failed
Playing | Paused | Failed --Restart--> Playing
Won --Next(n)--> Playing
Failed --Continue(s)--> Playing
```

| Transition | Signals |
| --- | --- |
| `Start(n)`, `Restart`, `Next(n)` | `BoardLoadedSignal` (a new `BoardState` from the level data), then `LevelStartedSignal` |
| `Pause` / `Resume` | `LevelPausedSignal` / `LevelResumedSignal` |
| board cleared while playing | `LevelCompletedSignal` (once) |
| `Fail` (the timer, T-15) | `LevelFailedSignal` (once) |
| `Continue(s)` (after a loss, paid first) | `LevelContinuedSignal(s)`: the timer gets `s` more seconds, input comes back |

- Restart rebuilds everything from the level's data, so nothing of the previous attempt stays (DA-15); a
  won level is not restarted, it moves on (T-17, T-21). A paused level cannot fail: its timer stands still.
- **Progression:** `LevelProgression` (plain class, built by the scope's build callback) advances
  `IPlayerProgressService` on `LevelCompletedSignal`, before anything presents the win (T-17). The
  catalog loops (DA-8): after the last level the next number plays the first layout again.
- The flows only route (§7): the win popup's Continue plays the next level through `LevelSession.Next`
  with the already advanced level number.

### Timer

**`LevelTimer`** (plain class, Gameplay scope, built by the scope's build callback) counts the level's
`TimerSeconds` down in real time (T-15, DA-6):

- It shows the full time on `LevelStartedSignal` and starts counting at the attempt's first
  `BlockGrabbedSignal` (DA-6), so a restart or the next level always gets the full time and waits for the
  player again; later grabs change nothing.
- It stands still while the level is paused, won or lost, and while the app is in the background
  (`ApplicationPausedSignal`, fired by `ApplicationPauseReporter` on the Root scope object from Unity's
  `OnApplicationPause`). It picks up where it stopped.
- It ticks with `IGameClock` delays (`RealtimeGameClock`: unscaled real time), one delay per shown second,
  never per frame. `TimerChangedSignal` carries the shown seconds (rounded up) when a level starts and
  each time the shown value drops.
- At zero it calls `LevelSession.Fail` once; the session fires `LevelFailedSignal` and input stops.
- `FreezeFor` (the freeze booster) holds it still for a while; see [boosters.md](boosters.md) §3.
- `SetSecondsLeft` puts the timer at any value and keeps it counting if it was; the debug window uses it
  ([debug-tools.md](debug-tools.md)).
- `GameplayHudView` shows it as `mm:ss` in `TimerLabel`, written into a char buffer
  (`TMP_Text.SetCharArray`) so a tick allocates nothing on device. The label sits in the placeholder top
  bar until the HUD layout (T-18) puts it in the timer holder art.

## 7. Popups and navigation

Four flows route the HUD and the popups ([ui.md](ui.md) §5); the session, the wallet and the settings
own every change:

- **`GameplayFlow`** (entry point) starts the level. HUD restart calls `LevelSession.Restart`. HUD pause
  calls `LevelSession.Pause` and, if the level was playing, opens the pause popup: its switches flip
  `ISettingsService`, Home leaves for Home, close resumes (T-19).
- **`WinFlow`** opens the win popup `GameplayConfig.WinPopupDelay` (1 s, through `IGameClock`) after
  `LevelCompletedSignal`, so the last block is seen leaving, with the reward the win already paid.
  Continue plays the next level with `LevelSession.Next` (T-21, DA-12).
- **`BoosterFlow`** routes the booster bar and the booster purchase popup ([boosters.md](boosters.md) §5);
  `GameplayFlow` puts a booster that waits for a target back before pausing.
- **`FailFlow`** opens the out-of-time popup on `LevelFailedSignal`. Continue buys
  `GameplayConfig.ContinueSeconds` (20) for `ContinueCost` (50) coins through `LevelContinueService`:
  the wallet pays first (`IWalletService.TrySpend`), then `LevelSession.Continue` plays on with the same
  board and `LevelContinuedSignal` adds the seconds to the timer and gives input back. The button cannot
  be pressed without the coins. Closing gives up and opens the level-failed popup: Retry restarts, close
  goes Home (T-20, DA-30, DA-31).
