# Gameplay

> **Binding agent rules.** Read this page before touching `Scripts/Game/Gameplay/` or the Gameplay
> scene. Sections are added as the case tasks land (rules, view, flow, timer, economy).

## 1. Board space

1. **Level data, rules and the level editor work in cells only.** Cell `(0, 0)` is the bottom-left cell;
   `x` grows right, `y` grows up the screen.
2. **`BoardSpace` is the only cell ↔ world conversion** (Gameplay scope). The board lies on the XZ plane
   at Y = 0, centred on the world origin; cell `y` maps to world +Z. It also projects touch rays onto
   that plane (`TryProjectToBoard`). The level loader (T-10) calls `SetSize` and then
   `BoardInputView.FitToBoard`; until it exists the hit area is zero-sized and only tests size it.
3. The world size of a cell is `BoardViewConfig.CellSize` (1, matching the 0.5 model import scale in
   [art-pipeline.md](art-pipeline.md) §3). No code assumes a cell is 1 unit: no `* 2`, `0.5f` or
   cell-size literal outside `BoardSpace` (`BoardSpaceTests` run at cell size 1 and 2).

## 2. Input

1. The board camera is registered with the Gameplay scope (`RegisterComponent`) and injected as `Camera`;
   `Camera.main` is never used.
2. `BoardInputView` sits on a `BoxCollider` hit area sized by `FitToBoard()` after `BoardSpace.SetSize`.
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

## 3. Navigation

`GameplayHudView` has a Home button that routes back to Home through `GameplayFlow` and
`INavigationService`. It is a placeholder until the pause popup (T-19) owns the route.
