# Level editor QA

Checks for [level-editor.md](../General/level-editor.md). `⚙` = automated (test named), `👁` = manual.

## Cases

### Validation (T-08)

- [ ] ⚙ Every structural rule reports a readable message — `LevelDefinitionValidatorTests` (one test per rule in level-data.md §2)
- [ ] ⚙ The saved format keeps every field — `LevelDefinitionTests.LevelData_ShouldSurviveSerialization_WhenCopiedThroughTheAsset`

### Catalog and editing (T-26 – T-29)

- [ ] ⚙ New, Copy, Delete, Up/Down and Save change the catalog and the assets — `LevelCatalogEditingTests`
- [ ] ⚙ Blocks and doors never overlap, leave the board or break into pieces; shrinking reports what goes — `LevelDraftTests`
- [ ] 👁 Every tool works with the mouse and its shortcut (table in level-editor.md §1); Ctrl+Z undoes each edit.
- [ ] 👁 The canvas redraws at once and scales with the window; the hover cell and the red/green previews show.
- [ ] 👁 Switching level or closing with unsaved changes asks Save / Cancel / Discard.

### Checks, solution and generator (T-30, T-33, T-47)

- [ ] ⚙ The solver finishes, refuses and gives up correctly, and its solutions replay on a real board — `LevelSolverTests`
- [ ] ⚙ Shipped levels play as hard as their label says — `ShippedLevelsTests.ShippedLevels_ShouldPlayAsHardAsTheirDifficultySays_WhenSolved`
- [ ] ⚙ Generated levels are valid, solvable, in their difficulty band and repeat with their seed — `LevelGeneratorTests`
- [ ] 👁 Space steps the solution on the canvas; the moving block is outlined.
- [ ] 👁 Save is refused while there are errors and asks before saving an unsolvable level.

### Ice (T-34)

- [ ] ⚙ The draft keeps a block's ice; the validator refuses more ice than other blocks — `IceBlockerTests`
- [ ] 👁 Ice slider and `I` / `Shift+I` set the selected block's ice; frozen blocks draw pale with "ice N".

### Play-test (T-30)

- [ ] 👁 ▶ Play opens Gameplay straight on the edited level (through Splash), with the saved content.

### Usability (T-31)

- [x] 👁 Hallway test (developer, 2026-11-17): someone new builds and plays a 2-block level using only the in-editor help.
