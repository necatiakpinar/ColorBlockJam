# Level Data

> **Binding agent rules.** Read this page before touching `Scripts/Game/Levels/`, a level asset, the
> level catalog or the level editor.

## 1. Format

One format, written by the level editor and read by the game (R-29, DA-13).

| Type | Holds |
| --- | --- |
| `LevelDefinition` (asset, `Assets/_Project/Levels/`) | one `LevelData`; `SetData` replaces it (the editor records undo first) |
| `LevelData` | `Width`, `Height` (cells), `TimerSeconds`, `Difficulty` (`Normal`, `Hard`, `VeryHard`; sets the win reward, [economy.md](economy.md)), `Blocks`, `Doors` |
| `BlockData` | `Color`, the absolute start `Cells` it covers (DA-18), and `Blockers`: a `BlockerSet` with one entry per blocker kind, e.g. `Ice.Exits`, the other blocks that must leave before it thaws (0 = no ice, T-34, DA-37; [blockers.md](blockers.md)) |
| `DoorData` | `Side` (`Bottom`, `Top`, `Left`, `Right`), `Start` cell along that wall, `Length`, `Color` (DA-17) |
| `LevelCatalog` (asset, `Assets/_Project/Levels/LevelCatalog.asset`) | the ordered level list as authored; level N is entry N − 1; the editor publishes it as `levels.json`, the pack the game plays ([level-packs.md](level-packs.md)) |
| `LevelRulesConfig` (asset, `Assets/_Project/Configs/`) | board size and timer limits |

Coordinates are cells ([gameplay.md](gameplay.md) §1.1). A door's `Start` counts columns on the
Bottom/Top walls and rows on the Left/Right walls. `BlockColor` and `WallSide` use spaced enum values;
never renumber them, they are stored in the assets. The format grows additively: a new field gets a
default that keeps old levels valid, and a renamed field keeps `[FormerlySerializedAs]`.

## 2. Invariants

`LevelDefinitionValidator` checks every rule below and words each problem for a designer
(`LevelDefinitionValidatorTests`). **Errors** make a level unplayable; **warnings** flag a probable
mistake. The level editor (T-30) refuses to save on errors and asks the designer to confirm warnings.

| # | Rule | Severity |
| --- | --- | --- |
| 1 | Width and height lie within `LevelRulesConfig` (default 3–10) | error |
| 2 | The timer lies within `LevelRulesConfig` (default 10–600 s) | error |
| 3 | The level has at least one block | error |
| 4 | Every block has a colour and cells, all inside the board, none repeated | error |
| 5 | No two blocks share a cell | error |
| 6 | Every block is one edge-connected piece | error |
| 7 | Every door has a colour, a wall, length ≥ 1 and lies within its wall | error |
| 8 | Doors on the same wall do not overlap | error |
| 9 | Every block has a door of its colour at least as wide as the block along that door's wall | error |
| 10 | Every door has at least one block of its colour | warning |
| 11 | The level has a difficulty (old levels read as `Normal`) | error |
| 12 | A block's ice is 0 or more and at most the number of other blocks (or it could never thaw) | error |

Rule 9 is a necessary condition only; whether the level can actually be finished is the solver's answer
(`LevelSolver`, [level-editor.md](level-editor.md) §3), shown live in the level editor.

## 3. Runtime use

The game never mutates `LevelData`. It plays the level pack loaded at boot (`LevelPackService`, an `ILevelSource`,
[level-packs.md](level-packs.md)), not the catalog; `LevelProvider` maps a
level number to its entry and loops after the last one (DA-8). Each attempt builds a new `BoardState`
([gameplay.md](gameplay.md) §3, §4), which is what makes a restart start clean.

The catalog ships twenty levels, `Level_001` to `Level_020`. Levels 1–8 were made with the level editor (T-32): each was
generated with the settings and seed below, its timer set by hand, and saved from the editor. They ramp up
by board size, block count, exit rounds (a block can only leave once the ones in its way have) and, from
level 3, blocks that must be moved out of the way first. Level 6 adds the ice blocker (T-34): its blue
block waits for 2 others to leave and its cyan block for 4, which turns the plain layout's score 7 into 9. Levels 7 and 8 add the arrow blocker (T-35): each arrow lies on
a block the plain layout's solution moves along one axis only. Level 7's two arrows (cyan horizontal, red vertical)
keep its score; level 8's three (purple and orange vertical, cyan horizontal) cut the shortcuts and turn 9 into 11. Score is the solver's difficulty measure
([level-editor.md](level-editor.md) §3).

| Level | Board | Colours | Blocks | Seed | Rounds | Moves | Score | Timer | Difficulty |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 4x4 | 2 | 2 | 1 | 1 | 0 | 1 | 60 s | Normal |
| 2 | 5x5 | 3 | 3 | 4 | 2 | 0 | 2 | 60 s | Normal |
| 3 | 5x6 | 4 | 5 | 24 | 3 | 1 | 4 | 90 s | Hard |
| 4 | 6x7 | 5 | 7 | 47 | 3 | 1 | 4 | 120 s | Hard |
| 5 | 6x7 | 6 | 8 | 2 | 6 | 2 | 8 | 160 s | Very Hard |
| 6 | 6x7 | 6 | 9 | 5, ice 2 and 4 | 7 | 2 | 9 | 190 s | Very Hard |
| 7 | 6x7 | 6 | 9 | 12, 2 arrows | 7 | 3 | 10 | 200 s | Very Hard |
| 8 | 6x8 | 6 | 10 | 2, 3 arrows | 9 | 2 | 11 | 220 s | Very Hard |

Levels 9–20 follow the DA-40 curve in two groups of six. Each group's third level is a breather: a Hard
level that needs no room made. Each group ends on its peak, the level that needs the most room-making moves.
- **How they were made.** Each hard level was generated at Very Hard, then pushed by the level hardener
  (`LevelHardener`, T-49) toward more room-making moves, with arrows and ice where it chose them.
  The levels were picked from a pool of about 70 by their moves.
- **Timer.** The generator's suggestion plus 15 s per room-making move, rounded to 5 s.

| Level | Board | Blocks | Arrows | Ice | Rounds | Moves | Score | Timer | Difficulty |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 9 | 6x7 | 9 | 0 | 0 | 9 | 3 | 12 | 220 s | Very Hard |
| 10 | 6x7 | 10 | 0 | 0 | 10 | 3 | 13 | 235 s | Very Hard |
| 11 | 6x7 | 9 | 0 | 0 | 4 | 0 | 4 | 145 s | Hard (breather) |
| 12 | 6x7 | 10 | 1 | 0 | 10 | 4 | 14 | 250 s | Very Hard |
| 13 | 6x8 | 11 | 1 | 3 | 7 | 4 | 11 | 270 s | Very Hard |
| 14 | 6x8 | 11 | 0 | 7 | 7 | 6 | 13 | 300 s | Very Hard (peak) |
| 15 | 7x8 | 12 | 1 | 6 | 10 | 4 | 14 | 285 s | Very Hard |
| 16 | 7x8 | 12 | 1 | 8 | 9 | 5 | 14 | 300 s | Very Hard |
| 17 | 7x8 | 11 | 0 | 0 | 4 | 0 | 4 | 170 s | Hard (breather) |
| 18 | 7x8 | 12 | 1 | 8 | 9 | 5 | 14 | 300 s | Very Hard |
| 19 | 6x8 | 11 | 0 | 9 | 10 | 6 | 16 | 300 s | Very Hard |
| 20 | 7x8 | 12 | 4 | 6 | 11 | 10 | 21 | 375 s | Very Hard (peak) |

`ShippedLevelsTests` checks:
- **Every level** is free of validator issues, solvable by `LevelSolver` and as hard as its label, and the
  last level is Very Hard.
- **Levels 1–8** ramp up: blocks, difficulty and score never fall, and room-making is needed from level 3.
- **Level 6** keeps its two frozen blocks; levels 7 and 8 keep their arrow blocks.
- **From level 9** every group of six has a breather. No level outdoes its group's peak, each peak needs more
  room-making moves than the last, and the final peak needs at least 8.
