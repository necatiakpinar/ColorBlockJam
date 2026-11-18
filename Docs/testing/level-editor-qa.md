# Level editor QA

Checks for [level-editor.md](../General/level-editor.md). `⚙` = automated (test named), `👁` = manual.

## Cases

### Validation (T-08)

- [x] ⚙ Every structural rule reports a readable message — `LevelDefinitionValidatorTests` (one test per rule in level-data.md §2)
- [x] ⚙ The saved format keeps every field — `LevelDefinitionTests.LevelData_ShouldSurviveSerialization_WhenCopiedThroughTheAsset`

### Catalog and editing (T-26 – T-29)

- [x] ⚙ New, Copy, Delete, Up/Down and Save change the catalog and the assets — `LevelCatalogEditingTests`
- [x] ⚙ New adds a level the game can load; a level with errors never reaches `levels.json` — `LevelCatalogEditingTests.AppendStarter_ShouldAddALevelTheGameCanLoad_WhenANewLevelIsMade`, `Append_ShouldThrowAndChangeNothing_WhenTheLevelHasErrors`, `Save_ShouldThrowAndKeepTheAsset_WhenTheDraftHasErrors`, `WritePack_ShouldThrowAndKeepThePack_WhenACatalogLevelHasErrors`
- [x] ⚙ Blocks and doors never overlap, leave the board or break into pieces; shrinking reports what goes — `LevelDraftTests`
- [ ] 👁 Every tool works with the mouse and its shortcut (table in level-editor.md §1); Ctrl+Z undoes each edit.
- [ ] 👁 The canvas redraws at once and scales with the window; the hover cell and the red/green previews show.
- [ ] 👁 Switching level or closing with unsaved changes asks Save / Cancel / Discard.
- [ ] 👁 With an error on the open level, switching level → Save → OK keeps the open level and its changes.
- [ ] 👁 Open level 3, open level 5, press Ctrl+Z: nothing of level 3 comes back; level 5 is unchanged.
- [ ] 👁 With a block selected, typing in the Seed or Timer field (Backspace, digits, `P`) only edits the field.
- [ ] 👁 Press Make harder, open another level before it finishes: the other level is unchanged and the generator says it stopped.
- [ ] 👁 Open level 12, press ▶ Play, stop: the window shows level 12 again.

### Checks, solution and generator (T-30, T-33, T-47)

- [x] ⚙ The solver finishes, refuses and gives up correctly, and its solutions replay on a real board — `LevelSolverTests`
- [x] ⚙ Shipped levels play as hard as their label says — `ShippedLevelsTests.ShippedLevels_ShouldPlayAsHardAsTheirDifficultySays_WhenSolved`
- [x] ⚙ Generated levels are valid, solvable, in their difficulty band and repeat with their seed — `LevelGeneratorTests`
- [ ] 👁 Space steps the solution on the canvas; the moving block is outlined.
- [ ] 👁 Save is refused while there are errors and asks before saving an unsolvable level, or one still being solved (Ctrl+S right after an edit).

### Ice (T-34)

- [x] ⚙ The draft keeps a block's ice; the validator refuses more ice than other blocks — `IceBlockerTests`
- [ ] 👁 Ice slider and `I` / `Shift+I` set the selected block's ice; frozen blocks draw pale with "ice N".

### Play-test (T-30)

- [ ] 👁 ▶ Play opens Gameplay straight on the edited level (through Splash), with the saved content.

### Usability (T-31, T-55)

- [x] 👁 Hallway test (developer, 2026-11-17): someone new builds and plays a 2-block level using only the in-editor help.
- [ ] 👁 The styled window reads at a glance in the dark and the light editor theme: header, cards, rounded buttons, difficulty strips, check chips; every button and shortcut still works.
