# Art Pipeline

> **Binding agent rules.** Read this page before adding, reimporting or wiring an asset under
> `Assets/_Project/Art/` or a part prefab under `Assets/_Project/Prefabs/Board/`.

The art set is used as imported (R-3). `AssetImportTests` enforces §2 and §3.

## 1. Folders

| Folder | Holds |
| --- | --- |
| `Art/UI/` | UI sprites and `UiAtlas.spriteatlas` |
| `Art/Models/` | FBX files: `Arrows`, `BlockParts`, `GroundGrid`, `WallAndDoor` |
| `Art/Textures/` | textures (`icetexture.jpg`) |
| `Art/Materials/` | project materials (URP Simple Lit, GPU instancing on) |
| `Prefabs/Board/` | one prefab per mesh part, used by the board and block views |
| `Audio/` | sound effects (`sfx_*.wav`, made by `tools/synth_sfx.py`); forced to mono, Decompress On Load, ADPCM ([audio.md](audio.md)) |

## 2. UI sprites

1. Texture Type `Sprite (2D and UI)`, single, mipmaps off, clamp, alpha is transparency.
2. Max size: the smallest power of two at least 1.5 times the largest size the UI draws it at (on the
   1080x1920 reference canvas; phones scale it up to about 1.2 times), never above the source. So the
   booster, clock and heart icons are 256, `ic_time` and `ic_restart` 128. Nine-sliced sprites keep their
   source size, since their borders are drawn at it.
3. Every sprite the UI uses, except the full-screen background `bg_home_screen`, is packed into `UiAtlas`
   (build-time packing, padding 4, no rotation, no tight packing). The atlas compresses its page (Android
   ASTC 6x6), so a packed sprite's own import is **uncompressed with no Android override**: a compressed
   source would be compressed twice and lose detail. The atlas must fit on **one 2048 page**: a second
   page splits the UI's draw calls and doubles its memory. A new sprite that does not fit means shrinking
   one by rule 2 first.
   The atlas is **late-bound**: "Include in Build" is off and it is addressable as `UiAtlas` (`UI` group).
   UI keeps referencing the sprites; when Unity first asks for the atlas, `SpriteAtlasBinder` loads it
   through Addressables ([addressables.md](addressables.md) §7).
4. Sprites outside the atlas (`bg_home_screen`, and the unused `bg_block_texture`, `Btn_levelBg_normal`,
   `Lego_Brickv1`, `leaderboard`, `bg_popup_info_holder_2`) keep an Android ASTC 6x6 override. An unused
   sprite is not built into the player; it joins the atlas when a screen starts using it.
5. Nine-slice borders are set on panels and stretchable buttons: `bg_popup`, `bg_popup_header`,
   `bg_popup_info_holder`, `bg_popup_info_holder_2`, `bg_menu`, `Btn_popup_green`, `Btn_popup_blue`,
   `bg_gameplay_time_holder`, `bg_home_coin_holder`, `bg_home_resource_holder`, `SquareRoundedMask`.
   Borders sit 6 px inside the opaque corner radius. Sprites with a vertical gradient (`Btn_popup_blue`,
   `Btn_popup_green`) are drawn at their own vertical scale (multiplier = sprite height / rect height): only
   their middle stretches, sideways, so the face never splits into a light and a dark band. A sprite with a
   gloss mark near a corner keeps the whole mark inside its corner slice, or slicing stretches it into a
   streak: `Btn_popup_blue` is sliced at left/right 74, top 86, bottom 50 and `Btn_popup_green` at 110, 125,
   125 (`PopupPrefabTests.PopupButtonSprites_ShouldKeepTheirHighlightInsideTheCornerSlice_WhenNineSliced`).
   `Btn_levelBg_normal` is not sliceable (circle and pattern); use it as `Simple` with `Preserve Aspect`.
   Borders are edited in the Sprite Editor, which comes with the `com.unity.2d.sprite` package.

## 3. Models

1. Scale factor **0.5**: the source files use a 2-unit cell, so after import **one board cell is 1 world
   unit**. Materials, animation, cameras, lights, blend shapes and colliders are not imported, nor are
   tangents (no material uses a normal map); meshes are not readable, so no CPU copy stays in memory.
2. `BlockParts` and `Arrows` are authored Z-up; their part prefabs rotate the `Mesh` child +90° on X so
   the part's top faces +Y. `GroundGrid` is Y-up. `WallAndDoor` is exported upside down (its cap faces -y
   and the top is open), so wall, corner and door prefabs rotate the `Mesh` child 180° on X; their top then
   sits 0.31 cells above the floor.
3. Board plane: X right, Z up the screen, Y towards the camera. The floor top is at Y = 0.
4. **Code never assumes the cell size.** Grid ↔ world conversion goes through `BoardSpace`, which reads
   `BoardViewConfig.CellSize`; level data and rules stay in cell coordinates
   ([Design README §2.8](../Design/README.md#2-scope-decisions)).

## 4. Part prefabs

Each prefab is an axis-aligned root with one `Mesh` child (`MeshFilter` + `MeshRenderer`). Views place,
rotate and stretch the root only. The `Wall`, `Door_1x1` and `Corner_1` meshes are a hair off centre and
off size in the source file (up to 0.002 cell); their `Mesh` child is offset and scaled so the footprint
is exact (the `Door_Arrow` child is turned like the other `WallAndDoor` parts and lifted onto the door's
top), otherwise a wall stretched over many cells opens a visible seam at the corner.

| Prefab | Mesh | Footprint (cells) | Notes |
| --- | --- | --- | --- |
| `Floor_Tile` | `Grid` | 0.98 × 0.98 | centred on its cell; the gap shows the grid |
| `Board_Base` | `Grid` | stretched to the board | darker base under the tiles; the gaps read as grid lines |
| `Wall_Segment` | `Wall` | 0.5 × 0.5, along z | one per stretch of wall, stretched along it |
| `Wall_Corner1`, `Wall_Corner5` | `Corner_1`, `corner_5` | 0.5 × 0.5 | outer ring corners, rounded toward +x +z; `Wall_Corner1` is used |
| `Wall_Corner3` | `corner_3` | 1 × 1 | L-shaped corner |
| `Door_Segment` | `Door_1x1` | 1 × 0.5, along x | one per door, stretched over its cells, tinted by colour |
| `Door_Arrow` | `Door_Arrow` | 0.2 | one per door, centred on its top, tip toward local -z (out of the board) |
| UI `Btn_level_green` | — | 420 x 418 | nine-sliced (border 120) so the Home "Level N" button can be wide; square tiles use it as is |
| `BlockPart_Center`, `_Edge`, `_OuterCorner` | `ModularBlock_*` | 0.5 × 0.5 quarter cell | pivot at the quarter's corner; four quarters make one cell |
| `BlockPart_InnerCorner` | `ModularBlock_InnerCorner` | 1 × 1 centred | fills a concave corner between cells |
| `Arrow_1`, `Arrow_2`, `Arrow_3` | `Arrow_*` | 0.45 × 0.8 / 1.75 / 2.71 | arrow over a 1-, 2- or 3-cell block |

Block height is 0.8 world units (the top face rim sits 0.33 above the floor, studs reach 0.48; corners,
outer and inner, are rounded with radius 0.27, `BoardViewConfig.BlockCornerRadius`); the `_Center` part is the top cap only. Each quarter piece is authored in
the +x +z quarter of its cell and is mirrored into the other quarters with a negative scale (the Edge
wall's other orientation adds a 90° turn); `BlockTileResolver` picks the piece and its transform.
`BlockView.prefab` is the empty pooled root a block's pieces hang under. `BlockExitBurst.prefab` is the
pooled spray of debris out of a door: a particle system of small cubes (built-in cube mesh,
`M_ExitDebris`, colour set per exit, world space, unscaled time, stop action Callback) shot from
a 30° cone pointing out of the door, with `ExitBurstView`.

Blocker views live in `Prefabs/Board/Blockers/` ([blockers.md](blockers.md)): `ArrowBlocker.prefab` (the
`Arrow_1/2/3` prefabs as children, half a cell up so they lie on the studs, one shown by the block's length),
`IceBlocker.prefab` (the
frozen block's count, a flat TextMeshPro over the block) and `IceShatter.prefab` (the thaw's ice-textured
shards, `M_IceShard`).

## 5. Materials

| Material | Use |
| --- | --- |
| `M_Block` | base of every block colour; `ColorMaterialCache` makes one copy per palette colour |
| `M_Floor`, `M_BoardBase`, `M_Wall` | floor tiles; the grid base; wall segments and corners |
| `M_Door`, `M_Arrow` | base of every door colour; arrows and door markers |
| `M_SelectionOutline` | outline of a held block (`Art/Shaders/SelectionOutline.shader`, drawn over everything) |
| `M_Ice` | ice blocker overlay, base map `icetexture.jpg` |
| `M_BlockExit` | base of an exiting block (`Art/Shaders/BlockExit.shader`): lit like `M_Block`, cut at the board's edge; each block view makes its own copy |
| `M_ExitDebris` | debris cubes sprayed out of a door (URP Particles/Simple Lit, tinted by the particle colour) |
