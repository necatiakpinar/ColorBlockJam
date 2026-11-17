# Blockers

> **Binding agent rules.** Read this page before touching a blocker (ice, and every kind that follows), its data,
> its view or its row in the level editor.

A **blocker** is a rule a block carries on top of its colour and cells: ice keeps it frozen until enough other blocks
have left (T-34). Every kind lives in the same four places, so a new kind (the arrow block next) is added the same
way and nothing else in the game learns its name.

## 1. Where a blocker lives

| Layer | Folder | Ice |
| --- | --- | --- |
| Data, as authored | `Scripts/Game/Levels/Blockers/` | `IceBlockerData` (exits to thaw; 0 = none), one field of `BlockerSet` |
| Rule, during an attempt | `Scripts/Game/Gameplay/Board/Blockers/` | `IceBlocker : IBlocker`, made by `BlockerFactory` |
| View | `Scripts/Game/Gameplay/View/Blockers/` + `Prefabs/Board/Blockers/` | `IceBlockerView` on `IceBlocker.prefab` (the count); `IceShatter.prefab` (the thaw's shards) |
| Level editor | `LevelEditorWindow.DrawBlockers` | the Ice row of the selected block's Blockers panel (`I` / `Shift+I`) |

`BlockData.Blockers` (a `BlockerSet`) holds one entry per kind, inactive by default, so a plain block saves the same
shape as any other. `LevelDraft` copies it with `BlockerSet.Clone`.

## 2. The rule interface

`BoardState` asks every `IBlocker` of a block and never names a kind:

| Member | Asked when | Ice |
| --- | --- | --- |
| `AllowsGrab` | the drag picks a block up (`BoardBlock.CanBeGrabbed`) | not while frozen |
| `AllowsStep(direction)` | every one-cell step, out through a door included | not while frozen |
| `OnOtherBlockGone()` | another block leaves (door or booster); true when the hold is released | counts down, true at 0 |
| `ResetAfter(blocksGone)` | the solver lays a board out at once | starting exits minus the blocks gone |
| `CloneForPiece()` | the rocket splits the block; each piece carries the blocker as it stands | the exits left |

A kind-specific action stays on the kind and is reached with `BoardBlock.TryGetBlocker<T>`: the hammer calls
`IceBlocker.Thaw`. Blocks released by an exit are listed in `BoardState.JustThawed`; `ActiveBoard` announces them.

## 3. Adding a blocker kind

1. Data: a `<Kind>BlockerData` class in `Levels/Blockers/` and its field in `BlockerSet` (and in `Clone`, `IsEmpty`).
2. Rule: `<Kind>Blocker : IBlocker` in `Gameplay/Board/Blockers/`, created in `BlockerFactory`.
3. Validation: its limits in `LevelDefinitionValidator`, worded for a designer.
4. View: `<Kind>BlockerView` and `<Kind>Blocker.prefab`, dressed onto the block by `BlockLayerView` (pooled, attached
   with `BlockView.AttachBlockerView`, taken back on release); its prefab on `BoardViewConfig`.
5. Editor: a row in `DrawBlockers`, and how the canvas draws it.
6. Tests: the rule in Edit Mode, the view in Play Mode; a DA entry for every player-visible decision.
