# Level Packs

> **Binding agent rules.** Read this page before touching `Scripts/Game/Levels/Packs/`, `levels.json`,
> `LevelPackConfig`, `LoadLevelsStep` or anything that changes where the game's levels come from.

The game plays a **level pack**: the whole ordered level list as one JSON document (T-50, R-52, DA-41).
- **Shipped.** One pack ships inside the app.
- **Remote.** A newer one can arrive from remote config without an app update. It plays from the next launch,
  only after it passes the same checks as a level saved in the editor.
- **Authoring.** The level editor stays where levels are made. The pack is what it publishes.

## 1. Where levels live

| Place | What it is | Who writes it |
| --- | --- | --- |
| `Assets/_Project/Levels/LevelCatalog.asset` + `Level_NNN.asset` | the levels as authored (`LevelCatalog`, [level-data.md](level-data.md)) | the level editor |
| `Assets/_Project/Levels/levels.json` | the **bundled pack**, built into the app | `LevelCatalogEditing.WritePack`, on every catalog change |
| `IKeyValueStorage` key `level_pack_remote_v1` | the **stored remote pack**, waiting for the next launch | `LevelPackService.TryStoreRemote` |
| `LevelPackConfig` (`Assets/_Project/Configs/`, in the `ConfigProvider`) | the bundled pack's `TextAsset`; packs are checked against `LevelRulesConfig` | by hand |

Rules:
1. **The catalog and `levels.json` never drift.** Every editor change to the catalog (save, add, copy,
   delete, reorder) rewrites `levels.json`. **ColorBlockJam ▸ Write Level Pack** rewrites it by hand.
   `ShippedLevelsTests.BundledPack_ShouldHoldTheCatalogsLevels_WhenTheGameShips` fails if they differ.
2. **Never edit `levels.json` by hand** in the repository: change the catalog in the editor instead.
3. **The game reads only packs.** Runtime code asks `ILevelSource` (`LevelPackService`), never the catalog.

## 2. The JSON format

Readable names, no Unity field names or enum numbers, so a pack can be read and edited in a remote config
console. Format `1`:

```json
{
  "format": 1,
  "revision": 12,
  "levels": [
    {
      "width": 6, "height": 7, "timer": 220, "difficulty": "VeryHard",
      "doors":  [ { "side": "Left", "start": 1, "length": 2, "color": "Red" } ],
      "blocks": [ { "color": "Red", "cells": [ { "x": 0, "y": 1 }, { "x": 1, "y": 1 } ],
                    "ice": 0, "arrow": "Horizontal" } ]
    }
  ]
}
```

| Field | Meaning |
| --- | --- |
| `format` | the layout of this document; this build reads `1` and refuses any other (`LevelPackJson.Format`) |
| `revision` | grows by one with every content change; a stored pack older than the bundled one is ignored |
| `levels` | in play order; level N is entry N − 1; after the last the game loops (DA-8) |
| `width`, `height`, `timer` | board cells and seconds, within `LevelRulesConfig` |
| `difficulty` | `Normal`, `Hard` or `VeryHard`; sets the win reward ([economy.md](economy.md)) |
| `doors[]` | `side` (`Bottom`, `Top`, `Left`, `Right`), `start` cell along that wall, `length`, `color` |
| `blocks[]` | `color`, absolute start `cells`, `ice` (other blocks to leave first, 0 = none), `arrow` (`None`, `Horizontal`, `Vertical`; absent = `None`) |

Colours are the `BlockColor` names: `Red`, `Orange`, `Yellow`, `Green`, `Cyan`, `Blue`, `Purple`, `Pink`.
Coordinates are cells from the bottom-left ([gameplay.md](gameplay.md) §1.1).

Reading (`LevelPackJson.TryRead`) is strict about what it understands and tolerant of what it does not:
- **Strict.** An unknown `format`, a misspelt or numeric name, or an empty pack is refused. The message
  names the level and the item, e.g. `Level 3: block 2 has an unknown colour 'Teal'.`
- **Tolerant.** Fields it does not know are ignored, so a later format can add optional fields without
  breaking older builds. A change older builds must not read raises `format`.
- **Output.** The editor writes the repository file indented. **ColorBlockJam ▸ Copy Level Pack for Remote
  Config** puts the same pack, compact, on the clipboard.

## 3. Which pack a launch plays

`LoadLevelsStep` (Splash, before `LoadPlayerDataStep`, [splash-boot.md](splash-boot.md)) calls
`LevelPackService.Load()` once:

1. Read the bundled pack and check every level with `LevelDefinitionValidator`. A broken bundled pack throws:
   it is a build bug and must fail loudly.
2. If a remote pack is stored, it **reads**, **passes the validator** (errors; warnings pass) and its
   `revision` is **not lower** than the bundled one, then this launch plays it (`IsRemote` is true).
3. Otherwise the bundled pack plays. A stored pack that failed is deleted, with a warning that says why.
   An app update that ships newer levels therefore replaces an older remote pack.

Reading levels before `Load` throws, as for the player's saves ([persistence.md](persistence.md) §2).
`LevelProvider` maps a level number to the pack through `ILevelSource`.

## 4. Changing levels from remote config

Nothing fetches remote config yet. The service is the seam a fetcher calls:

```csharp
// After the remote config fetch, with the value of the "level_pack" parameter:
if (!levelPacks.TryStoreRemote(json, out string error))
    logger.LogWarning($"Remote level pack ignored: {error}");
```

`TryStoreRemote` runs the same checks as `Load` (format, names, validator, revision) and stores the JSON
only if all pass. It never changes the levels of the running session: the player's current level never
changes under them, and the new pack plays from the next launch.

Publishing a new pack:
1. Make the levels in the level editor. The solver must say **Solvable** for each one ([level-editor.md](level-editor.md)).
   The game checks structure at runtime, not solvability.
2. Save. `levels.json` gets the next `revision`.
3. **ColorBlockJam ▸ Copy Level Pack for Remote Config**, then paste the value into the `level_pack` parameter.
4. Ship the same `levels.json` with the next app update, so new installs start on it too.

Rolling back: publish the previous pack with a **higher** revision. A lower one is ignored on purpose.

Player progress is a level number (`player_progress_v1`). A pack with fewer levels than a player has
reached still works: the number loops over the pack (DA-8).

## 5. Tests

| Test | Covers |
| --- | --- |
| `LevelPackJsonTests` | round trip of every field, names not numbers, compact output, absent arrow, unknown fields ignored, refusals worded with level and item |
| `LevelPackServiceTests` | bundled by default; stored remote pack when valid and not older; stale or broken stored packs dropped with a warning; `TryStoreRemote` stores only good packs and only for the next launch; reading before `Load` throws |
| `LevelCatalogEditingTests` | catalog changes rewrite the pack; the revision rises only when the levels changed |
| `ShippedLevelsTests` | the bundled pack holds exactly the catalog's levels |
| `InstallerResolutionTests` | `ILevelSource` is the `LevelPackService`; `LoadLevelsStep` runs before `LoadPlayerDataStep` |
