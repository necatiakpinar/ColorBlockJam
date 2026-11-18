# Reference Deconstruction

What the reference game shows on screen, measured from a 42.8 s screen recording of one level (level 26,
30 fps, portrait) and from screenshots. Everything here is observed behaviour: timings come from counting
frames, sizes from comparing against the board's cells. Nothing comes from the reference's files or
code. Each row says whether this project matches it and where the matching value lives.

Use this page when tuning feel; record a decision that follows from it in the
[design](README.md) or as a `DA-n` in [development-assumptions.md](../General/development-assumptions.md).

## 1. Camera and board

| Observation | Measured | Here |
| --- | --- | --- |
| Steep perspective look down at the board; front faces of blocks show a little, more for far rows | about 70° to 75° tilt, narrow field of view (near and far rows nearly the same width) | 72° tilt, 30° FOV in the Gameplay scene (DA-19) |
| Board fills the width between the HUD and the booster bar | about 92% of the screen width | `BoardViewConfig.Framing` margins |
| No shadows on blocks or walls | none visible | shadows off in `Mobile_RPAsset` and the scene light |
| Blocks are glossy studded bricks, one flat colour each, rounded corners | — | `M_Block` (Simple Lit), `BlockCornerRadius` 0.27 |
| Doors are colour strips on the wall ring with an arrow pointing out | — | `BoardView` door segments with one centred `Door_Arrow` (DA-24) |

## 2. Holding and dragging

| Observation | Measured | Here |
| --- | --- | --- |
| Touching a block shows a cream outline around it at once; no lift, no scale | outline on the frame of the touch | `OutlineMeshBuilder`, `SelectionOutline.shader` |
| The outline follows the block's silhouette: below the front face for far rows, tight for near rows | — | outline lifted by side visibility (gameplay.md §5) |
| The outline stays visible where the block goes under another object | — | outline drawn over everything |
| The block trails the finger slightly and never jumps | about 0.1 s behind on fast moves | `FollowDuration` 0.089 s (35% faster than the first 0.12 s, developer), `FollowEase` OutQuad |
| A block lined up roughly with a gap or a door slips into it | — | `CornerTolerance` 0.4 cell (DA-4) |

## 3. Leaving through a door

Frames 184–214 (yellow L, left door), 482–524 (blue, bottom door) and 933–968 (red, top door).

| Observation | Measured | Here |
| --- | --- | --- |
| The exit starts as soon as the held block is pushed into its own door; the finger need not let go | same frame the block reaches the door line | exit once pushed `ExitPush` (0.15 cell) into the door (gameplay.md §4) |
| The outline disappears when the exit starts | — | `BlockView.PlayExit` hides it |
| The block settles lined up at the door's mouth, then slides straight through at full size | about 5 to 6 cells per second, steady speed (yellow 3 cells in 0.6 s, blue 2 cells in 0.4 s) | settle `ExitSnapDuration` 0.08 s, then `ExitSpeed` 6 cells/s, `ExitEase` Linear |
| It is cut away at the board's edge, as if sliding into a slot; the door strip stays drawn | — | `BlockExit.shader` clip plane at the board edge |
| Small cubes of the block's colour spray out of the door while it passes, spread outward, then shrink away | 30–60 cubes for a 2-cell block; each lives 0.6–1.0 s; cube size about 0.1–0.15 cell | `BlockExitBurst.prefab`, `ExitDebrisRate` |
| The last exit ends the level | — | `BoardClearedSignal` |

## 4. Not built yet

| Observation | Notes |
| --- | --- |
| Wooden crates with a number cover blocks; every exit lowers the number, at 0 the crate breaks in a burst of wood chips and frees the blocks under it | a blocker like ice (T-34); needs level data and editor support |
| Frozen blocks show a number of exits left before they thaw | ice blocker, T-34 |
| Timer panel flashes red in the last seconds (seen at 0:29 and 0:19) | countdown, T-15 / T-18 |
| HUD: level badge, countdown, restart and pause buttons, coin counter | T-18 |
| Booster bar: freeze, rocket, hammer, vacuum, each with a count | placeholders, T-18; one functional booster is T-36 |
