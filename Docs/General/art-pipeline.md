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

## 2. UI sprites

1. Texture Type `Sprite (2D and UI)`, single, mipmaps off, clamp, alpha is transparency.
2. Max size: the smallest power of two that holds the source; the 1024² booster icons are capped at 512.
3. Android override ASTC 6x6.
4. Every sprite except the full-screen backgrounds (`bg_home_screen`, `bg_block_texture`) is packed into
   `UiAtlas` (build-time packing, padding 4, no rotation, no tight packing).
5. Nine-slice borders are set on panels and stretchable buttons: `bg_popup`, `bg_popup_header`,
   `bg_popup_info_holder`, `bg_popup_info_holder_2`, `bg_menu`, `Btn_popup_green`, `Btn_popup_blue`,
   `bg_gameplay_time_holder`, `bg_home_coin_holder`, `bg_home_resource_holder`, `SquareRoundedMask`.
   Borders sit 6 px inside the opaque corner radius. `Btn_levelBg_normal` is not sliceable (circle and
   pattern); use it as `Simple` with `Preserve Aspect`.

## 3. Models

1. Scale factor **0.5**: the source files use a 2-unit cell, so after import **one board cell is 1 world
   unit**. Materials, animation, cameras, lights, blend shapes and colliders are not imported.
2. `BlockParts` and `Arrows` are authored Z-up; their part prefabs rotate the `Mesh` child +90° on X so
   the part's top faces +Y. `GroundGrid` is Y-up. `WallAndDoor` is exported upside down (its cap faces -y
   and the top is open), so wall, corner and door prefabs rotate the `Mesh` child 180° on X; their top then
   sits 0.31 cells above the floor.
3. Board plane: X right, Z up the screen, Y towards the camera. The floor top is at Y = 0.
4. **Code never assumes the cell size.** Grid ↔ world conversion goes through `BoardSpace`, which reads
   `BoardViewConfig.CellSize`; level data and rules stay in cell coordinates
   ([Design README §2.8](../Design/README.md#2-scope-decisions)).

## 4. Part prefabs

Each prefab is an axis-aligned root with one `Mesh` child (`MeshFilter` + `MeshRenderer`). Views place
and rotate the root only.

| Prefab | Mesh | Footprint (cells) | Notes |
| --- | --- | --- | --- |
| `Floor_Tile` | `Grid` | 0.98 × 0.98 | centred on its cell; the gap shows the grid |
| `Board_Base` | `Grid` | stretched to the board | darker base under the tiles; the gaps read as grid lines |
| `Wall_Segment` | `Wall` | 0.5 × 0.5 | two segments per cell side |
| `Wall_Corner1`, `Wall_Corner5` | `Corner_1`, `corner_5` | 0.5 × 0.5 | outer ring corners, rounded toward +x +z; `Wall_Corner1` is used |
| `Wall_Corner3` | `corner_3` | 1 × 1 | L-shaped corner |
| `Door_Segment` | `Door_1x1` | 1 × 0.5, along x | one per door cell, tinted by colour |
| `Door_Arrow` | `Door_Arrow` | 0.2 | marker for arrow doors |
| `BlockPart_Center`, `_Edge`, `_OuterCorner` | `ModularBlock_*` | 0.5 × 0.5 quarter cell | pivot at the quarter's corner; four quarters make one cell |
| `BlockPart_InnerCorner` | `ModularBlock_InnerCorner` | 1 × 1 centred | fills a concave corner between cells |
| `Arrow_1`, `Arrow_2`, `Arrow_3` | `Arrow_*` | 0.45 × 0.8 / 1.75 / 2.71 | arrow over a 1-, 2- or 3-cell block |

Block height is 0.8 world units (the top face rim sits 0.33 above the floor, studs reach 0.48; corners,
outer and inner, are rounded with radius 0.27, `BoardViewConfig.BlockCornerRadius`); the `_Center` part is the top cap only. Each quarter piece is authored in
the +x +z quarter of its cell and is mirrored into the other quarters with a negative scale (the Edge
wall's other orientation adds a 90° turn); `BlockTileResolver` picks the piece and its transform.
`BlockView.prefab` is the empty pooled root a block's pieces hang under. `BlockExitBurst.prefab` is the
pooled spray of debris out of a door: a particle system of small cubes (built-in cube mesh,
`M_ExitDebris`, colour set per exit, world space, unscaled time, stop action Callback) with
`ExitBurstView`.

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
