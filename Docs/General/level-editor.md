# Level Editor

> **Binding agent rules.** Read this page before touching `Scripts/Editor/LevelEditing/`,
> `Scripts/Game/Levels/Solving/` or `Scripts/Game/Levels/Generation/`.

Open it with **ColorBlockJam > Level Editor** (`Ctrl+Shift+L`). It edits the levels the game plays, in the
game's own format ([level-data.md](level-data.md)), and checks them with the game's own rules.

## 1. Designer guide

### The window

| Area | What it holds |
| --- | --- |
| Left | the catalog in play order: level number, size, blocks, difficulty, with a colour strip per difficulty (green Normal, amber Hard, red Very Hard); + New, Copy, Delete, ▲ Earlier, ▼ Later |
| Middle | the level seen from above: rounded floor cells inside the dark wall ring, doors as rounded coloured tiles with an arrow out, blocks as joined coloured cells numbered 1, 2, 3… |
| Right | one card each: help, tools (with icons), colours, shapes, level settings, the selection, checks as coloured chips (green, amber, red) with the solution, the generator |
| Top | a coloured header: level name ("unsaved" when changed) and its size, blocks, doors and difficulty; Save (green when there is something to save), Revert, Check, ▶ Play, Help |

The look comes from `LevelEditorSkin` (editor assembly): rounded buttons in five kinds (neutral, primary,
success, danger, accent), cards, list rows and chips, with colours for the dark and the light editor theme.
It only draws; every tool, shortcut (DA-34) and check works as before (T-55).

### Make a level in five steps

Press **+ New** (`N`) for a new level at the end of the list. It starts as a 5 x 5 board with one red block
and its red door, so the game can always load the pack; change it from there.

1. **Board.** Set Width, Height, Timer and Difficulty under *Level*; the win reward shows under them.
2. **Doors.** Door tool (`3`): pick a colour, drag along a wall. A door can be 1 cell up to the whole wall;
   corners are not wall cells. Click a door to select it and change its colour, start or length.
3. **Blocks.** Block tool (`2`): pick a colour and a shape, click to stamp it (`R` turns the stamp), or pick
   **Free** and drag over cells to paint any connected shape. A red preview means it cannot go there.
   Select a block to see its **Blockers** panel: set **Ice** (or press `I` / `Shift+I`) to freeze it until
   that many other blocks have left, and **Arrow** (or press `A`) to let it move only horizontally or only
   vertically; the canvas shows ↔ or ↕ next to its number ([blockers.md](blockers.md)). Frozen blocks are drawn pale with "ice N" on them.
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
| `A` | the selected block's arrow: none → horizontal → vertical |
| arrows | move the selected block one cell |
| `Delete` | remove the selected block or door |
| `Esc` | clear the selection |
| `Ctrl+Z` / `Ctrl+Y` | undo / redo any edit to the open level; opening another level clears the undo history |
| `Ctrl+S` | save |
| `N` | new level at the end |
| `Shift+D` | copy this level after itself |
| `[` / `]` | previous / next level |
| `C` | check now |
| `Space` | step through the solution on the canvas |
| `G` | generate into this level |
| `H` | make the level harder |
| `P` | save and play |
| `F1` | show or hide the help |

Every control also has a tooltip. Shortcuts do nothing while a text or number field has the keyboard, so
typing a seed or a timer never switches a tool or deletes the selection.

### Checks

- **Structure** (`LevelDefinitionValidator`, level-data.md §2): board and timer limits, blocks inside the
  board and not overlapping, one piece each, a door of their colour at least as wide as the block, doors
  on their wall and apart, a difficulty. Errors block saving; warnings do not.
- **Can it be finished?** (`LevelSolver`): **Solvable** with the number of exit rounds and room-making moves
  and the difficulty it plays like; **Unsolvable** when some block can never reach its door; **Unknown**
  when the search gave up (very large levels). Saving a level that is unsolvable, unknown or still being
  solved asks first.
- **The solution**: the steps one by one ("Block 3 leaves ▶ right", "Block 2 makes room"). Drag *Show
  step* or press `Space` to replay it on the canvas; the moving block is outlined and gone blocks fade.

The checks never hold up an edit (`LevelChecker`):
- **Structure** answers at once.
- **The solver** starts on a worker thread once the edits pause for 250 ms; meanwhile Checks says *Solving…*.
- **A newer edit** cancels the search still running for the old draft. `C` or Check solves at once.
- **Save** does not wait for a running solve; it asks first, as for an unsolvable level.
- **Generate** also runs on a worker thread; its button reads *Generating…* until the level lands. Opening
  another level stops Generate and Make harder, so a result never lands in a level it was not made for.

Everything the window draws from the draft (the unsaved mark, block labels, the level list) is worked out once
per change, not on every GUI event.

### Generator

Set width, height, colours, blocks, difficulty and seed, then **Generate** (`G`). It builds a level that
passes every check and plays at the chosen difficulty, puts it on the canvas (undo brings the old one
back) and moves to the next seed, so pressing again gives another level. The same seed with the same
settings always gives the same level. When no candidate hits the difficulty exactly it gives the closest
one and says so; when the settings cannot work (too many blocks for the board) it says why.

**Make harder** (`H`) runs `LevelHardener` on the level on the canvas:
- **What it changes.** For 25 rounds it tries 16 small changes at a time (move a block, slide a door, set an
  arrow or ice) and keeps the one that needs the most room-making moves.
- **What it keeps.** The level stays valid and solvable.
- **Result.** It says how many moves the level now needs; undo brings the old one back, and pressing again
  pushes further. The seed field seeds it too, so the same level and seed give the same result.

### Leaving a level

Switching level, New, Copy and moving a level ask **Save / Cancel / Discard** when the open level has unsaved
changes. When Save is refused (errors, or Cancel on *Save anyway?*) the open level stays open with its
changes. The window reopens the level it had open after play mode or a script reload.

## 2. Play-test

▶ Play saves unsaved changes first (it stops if Save is refused), makes the level the player's current
level, enters play mode through Splash as always, and opens Gameplay as soon as Home is up (`LevelPlayTest`, editor-only). It
changes the saved progress to that level; the debug window's *Clear player saves* resets it.

## 3. How it works

| Piece | Assembly | Role |
| --- | --- | --- |
| `LevelEditorWindow` | Editor | the window: panels, mouse, shortcuts, checks |
| `LevelCanvas` | Editor | draws a draft top-down and maps the mouse to board and wall cells |
| `LevelChecker` | Editor | validates a draft at once and solves it on a worker thread, cancelling a stale search |
| `LevelDraft` | Editor | the level being edited; every change keeps the board buildable and answers false otherwise |
| `LevelEditorState` | Editor | holds the draft for Unity's undo (`Undo.RecordObject`) |
| `LevelCatalogEditing` | Editor | create, copy, delete, reorder and save level assets in the catalog, straight to disk without undo (git is the history; an undo record on an asset lets the test runner's rollback revert a save); every change rewrites `levels.json` ([level-packs.md](level-packs.md)) and throws, writing nothing, when a level has validator errors |
| `LevelPlayTest` | Editor | play-test from the editor |
| `LevelSolver` | Game | finishes a level with the game's rules (below) |
| `LevelGenerator` | Game | builds levels from a `LevelGenerationRequest` (below) |
| `BlockShape`, `BlockShapeLibrary` | Game | the preset shapes the editor stamps and the generator uses |

**Solver (T-33, DA-32).**
- **Rules from the game.** `SearchBoard` asks a real `BoardState` once per level and records, as 128-bit
  cell masks, every block's positions, its one-cell moves, the spots it can leave from and the cells its way
  out needs free (found by probing with a one-cell block), plus what its blockers allow for each number of
  blocks gone. The search then runs on bit operations; the rules are never copied.
- **Board size.** It handles boards up to 128 cells.
- **Same answers.** Before the old board-moving solver was removed, it was checked to give the same status,
  score, states searched and steps on every shipped level and on generated ones with arrows and ice.

A move takes one block along any path of free cells, as the drag allows. Leaving
never hurts, so after every move all blocks that can reach their door leave, in waves (each wave is every
block that can leave at that moment). The search branches only over room-making moves, breadth first, and
stops at `LevelRulesConfig.SolverStateCap` (20 000 states) with *Unknown*. `Solve(level, token)` stops with
`OperationCanceledException` once the token is cancelled; `Solve(level, maxMoves, token)` tries only
solutions with at most that many room-making moves and says *Unknown* past them.

It keeps the search cheap without changing what it finds:
- A board reached right after a move is played out once, however many moves reach it.
- States are keyed by one character per block, hashed with a mixing comparer (the runtime's string hash
  spreads such keys badly).
- The reach search reuses its buffers.

Because of those buffers, use one solver per thread.

Ice needs no extra state in the search: every exit counts every frozen block down by one, so a block's ice
is its starting ice minus the blocks already gone (`BoardState.Arrange`).

**Difficulty (DA-33).** A solution's score is its exit waves plus its room-making moves: Normal 1–2, Hard 3–4,
Very Hard 5 or more. The shipped levels score 1, 2, 4, 4, 8, 9, 10 and 11, matching their labels
(`ShippedLevelsTests`).

**Generator (T-47, DA-33).** With a seeded `System.Random`, each candidate gets one door per colour (1–3 cells,
not overlapping) and preset blocks in those colours, each turned so it fits a door of its colour. A
candidate is kept when the validator has no error and the solver finds it solvable within 2 000 states;
the first whose score is in the difficulty's band wins.
- **Parallel checks.** Candidates are built one after another from the seed, then checked in parallel batches,
  one per core. They are read in order, so a seed gives the same level as one at a time would.
- **Early stop.** Once a candidate fits, the later ones in its batch stop.
- **Move limit.** For Normal and Hard the solver is not asked for solutions with more moves than the band's
  top score allows, since every solution has at least one exit wave.
- **Cancellation.** `Generate(request, token)` stops when the token is cancelled.

The timer is 20 s plus 10 s a block, plus 30 % for
Hard and 60 % for Very Hard, rounded to 5 s.

## 4. Tests

`LevelDraftTests`, `LevelCatalogEditingTests`, `LevelCheckerTests`, `LevelSolverTests` (each solution is replayed on a real board),
`LevelGeneratorTests`, `ShippedLevelsTests`. The window itself is checked by hand
([level-editor-qa.md](../testing/level-editor-qa.md)).
