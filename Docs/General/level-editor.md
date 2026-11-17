# Level Editor

> **Binding agent rules.** Read this page before touching `Scripts/Editor/LevelEditing/`,
> `Scripts/Game/Levels/Solving/` or `Scripts/Game/Levels/Generation/`.

Open it with **ColorBlockJam > Level Editor** (`Ctrl+Shift+L`). It edits the levels the game plays, in the
game's own format ([level-data.md](level-data.md)), and checks them with the game's own rules.

## 1. Designer guide

### The window

| Area | What it holds |
| --- | --- |
| Left | the catalog in play order: level number, size, blocks, difficulty; New, Copy, Delete, ▲ Up, ▼ Down |
| Middle | the level seen from above: floor cells inside the dark wall ring, doors as coloured strips with an arrow out, blocks as joined coloured cells numbered 1, 2, 3… |
| Right | help, tools, colours, shapes, level settings, the selection, checks with the solution, the generator |
| Top | level name (`*` when unsaved), Save, Revert, Check, ▶ Play, Help |

### Make a level in five steps

1. **Board.** Set Width, Height, Timer and Difficulty under *Level*; the win reward shows under them.
2. **Doors.** Door tool (`3`): pick a colour, drag along a wall. A door can be 1 cell up to the whole wall;
   corners are not wall cells. Click a door to select it and change its colour, start or length.
3. **Blocks.** Block tool (`2`): pick a colour and a shape, click to stamp it (`R` turns the stamp), or pick
   **Free** and drag over cells to paint any connected shape. A red preview means it cannot go there.
   Select a block to see its **Blockers** panel: set **Ice** (or press `I` / `Shift+I`) to freeze it until
   that many other blocks have left ([blockers.md](blockers.md)). Frozen blocks are drawn pale with "ice N" on them.
4. **Checks.** Fix every red line. The level must say **Solvable**; it also says how it plays
   (Normal, Hard, Very Hard) so you can match the Difficulty you set.
5. **Save and play.** `Ctrl+S`, then ▶ Play (`P`): the game starts straight in this level.

Or press **Generate** (`G`) and edit what it builds.

### Shortcuts

| Key | Does |
| --- | --- |
| `1` `2` `3` `4` | Select, Block, Door, Erase tool |
| `Q` / `E` | previous / next colour (recolours the selection) |
| `Tab` / `Shift+Tab` | next / previous shape (Free, 1x1, 1x2, 1x3, 2x2, L, T, S) |
| `R` | turn the selected block, or the stamp |
| `I` / `Shift+I` | one more / one less ice on the selected block |
| arrows | move the selected block one cell |
| `Delete` | remove the selected block or door |
| `Esc` | clear the selection |
| `Ctrl+Z` / `Ctrl+Y` | undo / redo any edit |
| `Ctrl+S` | save |
| `N` | new level at the end |
| `Shift+D` | copy this level after itself |
| `[` / `]` | previous / next level |
| `C` | check now |
| `Space` | step through the solution on the canvas |
| `G` | generate into this level |
| `P` | save and play |
| `F1` | show or hide the help |

Every control also has a tooltip.

### Checks

- **Structure** (`LevelDefinitionValidator`, level-data.md §2): board and timer limits, blocks inside the
  board and not overlapping, one piece each, a door of their colour at least as wide as the block, doors
  on their wall and apart, a difficulty. Errors block saving; warnings do not.
- **Can it be finished?** (`LevelSolver`): **Solvable** with the number of exit rounds and room-making moves
  and the difficulty it plays like; **Unsolvable** when some block can never reach its door; **Unknown**
  when the search gave up (very large levels). Saving an unsolvable level asks first.
- **The solution**: the steps one by one ("Block 3 leaves ▶ right", "Block 2 makes room"). Drag *Show
  step* or press `Space` to replay it on the canvas; the moving block is outlined and gone blocks fade.

### Generator

Set width, height, colours, blocks, difficulty and seed, then **Generate** (`G`). It builds a level that
passes every check and plays at the chosen difficulty, puts it on the canvas (undo brings the old one
back) and moves to the next seed, so pressing again gives another level. The same seed with the same
settings always gives the same level. When no candidate hits the difficulty exactly it gives the closest
one and says so; when the settings cannot work (too many blocks for the board) it says why.

## 2. Play-test

▶ Play saves the level (it refuses while there are errors), makes it the player's current level, enters play
mode through Splash as always, and opens Gameplay as soon as Home is up (`LevelPlayTest`, editor-only). It
changes the saved progress to that level; the debug window's *Clear player saves* resets it.

## 3. How it works

| Piece | Assembly | Role |
| --- | --- | --- |
| `LevelEditorWindow` | Editor | the window: panels, mouse, shortcuts, checks |
| `LevelCanvas` | Editor | draws a draft top-down and maps the mouse to board and wall cells |
| `LevelDraft` | Editor | the level being edited; every change keeps the board buildable and answers false otherwise |
| `LevelEditorState` | Editor | holds the draft for Unity's undo (`Undo.RecordObject`) |
| `LevelCatalogEditing` | Editor | create, copy, delete, reorder and save level assets in the catalog, straight to disk without undo (git is the history; an undo record on an asset lets the test runner's rollback revert a save) |
| `LevelPlayTest` | Editor | play-test from the editor |
| `LevelSolver` | Game | finishes a level with the game's rules (below) |
| `LevelGenerator` | Game | builds levels from a `LevelGenerationRequest` (below) |
| `BlockShape`, `BlockShapeLibrary` | Game | the preset shapes the editor stamps and the generator uses |

**Solver (T-33, DA-32).** It lays board states out on a real `BoardState` (`Arrange`, `MoveBlockTo`), so the
rules are never copied. A move takes one block along any path of free cells, as the drag allows. Leaving
never hurts, so after every move all blocks that can reach their door leave, in waves (each wave is every
block that can leave at that moment). The search branches only over room-making moves, breadth first, and
stops at `LevelRulesConfig.SolverStateCap` (20 000 states) with *Unknown*.

Ice needs no extra state in the search: every exit counts every frozen block down by one, so a block's ice
is its starting ice minus the blocks already gone (`BoardState.Arrange`).

**Difficulty (DA-33). A solution's score is its exit waves plus its room-making moves: Normal 1–2, Hard 3–4,
Very Hard 5 or more. The shipped levels score 1, 2, 4, 4 and 8, matching their labels
(`ShippedLevelsTests`).

**Generator (T-47, DA-33).** With a seeded `System.Random`, each candidate gets one door per colour (1–3 cells,
not overlapping) and preset blocks in those colours, each turned so it fits a door of its colour. A
candidate is kept when the validator has no error and the solver finds it solvable within 2 000 states;
the first whose score is in the difficulty's band wins. The timer is 20 s plus 10 s a block, plus 30 % for
Hard and 60 % for Very Hard, rounded to 5 s.

## 4. Tests

`LevelDraftTests`, `LevelCatalogEditingTests`, `LevelSolverTests` (each solution is replayed on a real board),
`LevelGeneratorTests`, `ShippedLevelsTests`. The window itself is checked by hand
([level-editor-qa.md](../testing/level-editor-qa.md)).
