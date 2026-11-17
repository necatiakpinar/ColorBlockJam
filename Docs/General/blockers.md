# Blockers

> **Binding agent rules.** Read this page before touching a blocker (ice, and every kind that follows), its data,
> its view or its row in the level editor.

A **blocker** is a rule a block carries on top of its colour and cells: ice keeps it frozen until enough other blocks
have left (T-34); an arrow lets it move only along one axis (T-35). Every kind lives in the same four places, so a
new kind is added the same way and nothing else in the game learns its name. A block may carry several kinds.

## 1. Where a blocker lives

| Layer | Folder | Ice | Arrow |
| --- | --- | --- | --- |
| Data, as authored | `Scripts/Game/Levels/Blockers/` | `IceBlockerData` (exits to thaw; 0 = none) | `ArrowBlockerData` (`ArrowAxis` None, Horizontal, Vertical) |
| Rule, during an attempt | `Scripts/Game/Gameplay/Board/Blockers/` | `IceBlocker : IBlocker` | `ArrowBlocker : IBlocker` |
| View | `Scripts/Game/Gameplay/View/Blockers/` + `Prefabs/Board/Blockers/` | `IceBlockerView` on `IceBlocker.prefab` (the count); `IceShatter.prefab` (the thaw's shards) | `ArrowBlockerView` on `ArrowBlocker.prefab`: the `Arrow_1/2/3` double-headed arrow for a block 1, 2 or 3+ cells long, lying on it along its axis |
| Level editor | `LevelEditorWindow.DrawBlockers` | the Ice row (`I` / `Shift+I`) | the Arrow row (`A` cycles None → Horizontal → Vertical); the canvas adds ↔ or ↕ to the block's label |

Both are fields of `BlockerSet` and made by `BlockerFactory`.

`BlockData.Blockers` (a `BlockerSet`) holds one entry per kind, inactive by default, so a plain block saves the same
shape as any other. `LevelDraft` copies it with `BlockerSet.Clone`.

## 2. The rule interface

`BoardState` asks every `IBlocker` of a block and never names a kind:

| Member | Asked when | Ice | Arrow |
| --- | --- | --- | --- |
| `AllowsGrab` | the drag picks a block up (`BoardBlock.CanBeGrabbed`) | not while frozen | always |
| `AllowsStep(direction)` | every one-cell step, out through a door included; the drag's free distance, slips and lean too | not while frozen | only along its axis |
| `OnOtherBlockGone()` | another block leaves (door or booster); true when the hold is released | counts down, true at 0 | never |
| `ResetAfter(blocksGone)` | the solver lays a board out at once | starting exits minus the blocks gone | nothing to reset |
| `CloneForPiece()` | the rocket splits the block; each piece carries the blocker as it stands | the exits left | the same axis |

The validator knows an arrow block reaches only the walls across its axis: a horizontal block needs a fitting door on
the left or right wall, a vertical one on the bottom or top.

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
