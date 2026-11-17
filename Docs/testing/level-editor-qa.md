# Level editor QA

Stub created by the task breakdown; filled by tasks T-26–T-31. `⚙` = automated (test named), `👁` = manual.
Every case is checked at 1080x1920 and 1080x2400 (20:9) unless it says otherwise.

## Cases

### Validation (T-08)

- [ ] ⚙ Every structural rule reports a readable message — `LevelDefinitionValidatorTests` (one test per rule in level-data.md §2)
- [ ] ⚙ The saved format keeps every field — `LevelDefinitionTests.LevelData_ShouldSurviveSerialization_WhenCopiedThroughTheAsset`

Editor window cases arrive with T-26 to T-31.
