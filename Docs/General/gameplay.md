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

1. `GameplayFlow.Start` asks `LevelProvider` for the current level (the catalog loops after its last level,
   DA-8), loads it into `ActiveBoard` and then fires `LevelStartedSignal`. `ActiveBoard.Load` builds a new
   `BoardState`, sizes `BoardSpace` and fires **`BoardLoadedSignal`**; every board view rebuilds from it.
2. **`ActiveBoard.TryStep`** is the only way a block moves during play. It calls `BoardState.TryStep` and
   announces the outcome: `BlockMovedSignal` per cell, `BlockExitedSignal` on an exit and
   **`BoardClearedSignal`** once, after the exit of the last block. Anything that moves blocks (the drag
   today, a booster later) goes through it.
3. **`BlockDragController`** (`Input/`, plain class, built by the scope's build callback) lets the finger
   slide the touched block like a solid object (DA-4):
   - a press fires **`BlockGrabbedSignal`**; the block aims at the finger plus the offset it was grabbed
     at, which never changes, so it cannot drift away from the finger. It follows freely in fractional
     cells, diagonals included, firing **`BlockDraggedSignal`** on every drag event;
   - collisions work like tile physics: each pass moves it along the axis the finger is furthest along,
     then the other, as far as every cell its footprint sweeps through is free (`BoardState.IsCellFree`);
     a block between rows needs both rows free. It slides along an obstacle instead of sticking, and a
     fast drag crosses any number of free cells in one event but never passes through a block;
   - pushed into a one-cell gap from at most `BoardViewConfig.CornerTolerance` off the cell line, the
     stopped part of the push slips it sideways onto that line (only if the line opens the way), and the
     finger cannot pull it back out during that event. Later events keep the slip until pulling back
     has room and leaves the push no more stopped than on the line (through the gap, or the finger turned
     back), or the finger is past the tolerance; otherwise a slow push would be pulled back and slipped
     again every event and the block would jitter;
   - the drawn position may lean toward the finger past an obstacle by `BoardViewConfig.HoldGive`, so a
     held block with no room still gives a little; the lean is never committed;
   - whenever its nearest cell changes the move is committed (`ActiveBoard.TryStep`), so the board always
     holds the cell it settles on;
   - lined up with a fitting door (`BoardState.CanStep` says `Exited`), the wall stops being an obstacle
     and the block exits once it is halfway through;
   - release fires **`BlockDroppedSignal`** with the cell it settles on; nothing snaps before release.
   A drag allocates nothing (`BlockDragControllerTests`).

## 5. View

All board views listen to signals only; the model has committed every change before a view sees it.
`BoardLoadedSignal` carries the level as authored, never the live `BoardState`, so views cannot change it.

1. Board views parent their parts to a root at `BoardSpace.Origin` scaled by `CellSize`, so parts are laid
   out in cell units: cell `(x, y)` is centred at `(x + 0.5, 0, y + 0.5)`.
2. **`BoardView`** builds, per load: a base under the floor tiles (the gaps read as grid lines), one tile per
   cell, the wall ring (two half-cell segments per cell, `BoardViewConfig.WallThickness` outside the
   board), one door segment per door cell in the door's colour, and four rounded corners.
3. **`BlockLayerView`** builds every block from the modular pieces chosen by **`BlockTileResolver`**
   (art-pipeline.md §4) in its palette colour. While a block is held it shows an outline band that hugs
   its footprint (`OutlineMeshBuilder`): a closed loop `OutlineGap` outside the block, its corners rounded
   like the block models (`BlockCornerRadius`), so it follows any polyomino, notches and holes included.
   It traces the block's on-screen silhouette: shader `ColorBlockJam/Selection Outline` keeps a side the
   camera sees outlined along its bottom edge (`OutlineBaseHeight`) and lifts a side turned away to the
   rim of the top face (`BlockTopHeight`). A block high on screen shows more of its front face, so the
   band runs further below its top face there than near the bottom of the screen. The shader draws over
   everything, so the outline stays visible where the block goes under another object. The view also draws the block exactly where the finger has it
   (`BlockDraggedSignal`, no tween); on `BlockDroppedSignal` it settles it onto its cell with the
   snap tween (`SnapDuration`, `SnapEase`); on `BlockExitedSignal` it plays `BlockExitAnimation` from
   wherever the block is: it slides `ExitDistance` cells through the door while shrinking, then returns to
   its pool. A block view's pivot is the block's centre. The snap is one paused tween per block, retargeted
   on every drop (`BlockMoveAnimation`), so following and snapping allocate nothing
   (`GameplayBoardTests.BlockView_ShouldNotAllocate_WhenFollowingAFingerAndSnapping`). A board reload also
   recycles blocks that are still sliding out
   (`GameplayBoardTests.Reload_ShouldRebuildCleanBlocks_WhenABlockIsStillSlidingOut`).
4. Every part, block view and piece comes from a pool (`PartPool`, `ObjectPool`), so a restart or the next
   level reuses objects. Colours are one shared material per colour (`ColorMaterialCache`) to keep the SRP
   batcher batching.
5. **`BoardCameraView`** keeps the authored tilt (60°) and field of view (40°) and moves the camera so the
   board and its walls fit between `BoardViewConfig.Framing` margins (HUD on top, boosters below), centred
   in that band, for the screen's aspect ratio (`BoardFraming`, `BoardFramingTests`). It frames on every
   board load; the device aspect never changes (portrait only), so there is no re-fit on resize.

## 6. Navigation

`GameplayHudView` has a Home button that routes back to Home through `GameplayFlow` and
`INavigationService`. It is a placeholder until the pause popup (T-19) owns the route.
