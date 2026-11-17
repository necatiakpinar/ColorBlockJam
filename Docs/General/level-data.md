# Level Data

> **Binding agent rules.** Read this page before touching `Scripts/Game/Levels/`, a level asset, the
> level catalog or the level editor.

## 1. Format

One format, written by the level editor and read by the game (R-29, DA-13).

| Type | Holds |
| --- | --- |
| `LevelDefinition` (asset, `Assets/_Project/Levels/`) | one `LevelData`; `SetData` replaces it (the editor records undo first) |
| `LevelData` | `Width`, `Height` (cells), `TimerSeconds`, `Difficulty` (`Normal`, `Hard`, `VeryHard`; sets the win reward, [economy.md](economy.md)), `Blocks`, `Doors` |
| `BlockData` | `Color` and the absolute start `Cells` it covers (DA-18) |
| `DoorData` | `Side` (`Bottom`, `Top`, `Left`, `Right`), `Start` cell along that wall, `Length`, `Color` (DA-17) |
| `LevelCatalog` (asset, `Assets/_Project/Levels/LevelCatalog.asset`) | the ordered level list; level N is entry N − 1 |
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

Rule 9 is a necessary condition only; whether the level can actually be solved is T-33.

## 3. Runtime use

The game never mutates `LevelData`. The catalog is registered by the root scope; `LevelProvider` maps a
level number to its entry and loops after the last one (DA-8). Each attempt builds a new `BoardState`
([gameplay.md](gameplay.md) §3, §4), which is what makes a restart start clean.

The catalog ships five levels, `Level_001` to `Level_005`, made in code until the level editor
exists (T-32 remakes them with it). They ramp up by board size, block count and how many rounds of exits
the blocks need (a block can only leave once the ones in its way have):

| Level | Board | Blocks | Rounds | Timer | Difficulty |
| --- | --- | --- | --- | --- | --- |
| 1 | 4x4 | 2 | 1 | 60 s | Normal |
| 2 | 5x5 | 3 | 2 | 60 s | Normal |
| 3 | 5x6 | 5 | 4 | 90 s | Hard |
| 4 | 6x7 | 8 | 4 | 120 s | Hard |
| 5 | 6x7 | 8 | 6 | 150 s | Very Hard |

`ShippedLevelsTests` keeps every catalog level free of validator issues and proves it solvable with the
real `BoardState` rules: it takes blocks out one at a time, each searching every cell it can slide to.
