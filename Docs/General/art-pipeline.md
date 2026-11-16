# Art Pipeline

> **Binding agent rules.** Read this page before adding, reimporting or wiring an asset under
> `Assets/_Project/Art/` or a part prefab under `Assets/_Project/Prefabs/Board/`.

The supplied art is used as delivered (case brief §2). `AssetImportTests` enforces §2 and §3.

## 1. Folders

| Folder | Holds |
| --- | --- |
| `Art/UI/` | supplied UI sprites and `UiAtlas.spriteatlas` |
| `Art/Models/` | supplied FBX files: `Arrows`, `BlockParts`, `GroundGrid`, `WallAndDoor` |
| `Art/Textures/` | supplied textures (`icetexture.jpg`) |
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
   the part's top faces +Y. `GroundGrid` and `WallAndDoor` are already Y-up.
3. Board plane: X right, Z up the screen, Y towards the camera. The floor top is at Y = 0.
4. **Code never assumes the cell size.** Grid ↔ world conversion goes through `BoardSpace`, which reads
   `BoardViewConfig.CellSize`; level data and rules stay in cell coordinates
   ([Case README §2.8](../Case/README.md#2-scope-decisions)).

## 4. Part prefabs

Each prefab is an axis-aligned root with one `Mesh` child (`MeshFilter` + `MeshRenderer`). Views place
and rotate the root only.

| Prefab | Mesh | Footprint (cells) | Notes |
| --- | --- | --- | --- |
| `Floor_Tile` | `Grid` | 0.98 × 0.98 | centred on its cell; the gap shows the grid |
| `Wall_Segment` | `Wall` | 0.5 × 0.5, 1.79 high | two segments per cell side |
| `Wall_Corner1`, `Wall_Corner5` | `Corner_1`, `corner_5` | 0.5 × 0.5 | outer ring corners |
| `Wall_Corner3` | `corner_3` | 1 × 1 | L-shaped corner |
| `Door_Segment` | `Door_1x1` | 1 × 0.5 | one per door cell, tinted by colour |
| `Door_Arrow` | `Door_Arrow` | 0.2 | marker for arrow doors |
| `BlockPart_Center`, `_Edge`, `_OuterCorner` | `ModularBlock_*` | 0.5 × 0.5 quarter cell | pivot at the quarter's corner; four quarters make one cell |
| `BlockPart_InnerCorner` | `ModularBlock_InnerCorner` | 1 × 1 centred | fills a concave corner between cells |
| `Arrow_1`, `Arrow_2`, `Arrow_3` | `Arrow_*` | 0.45 × 0.8 / 1.75 / 2.71 | arrow over a 1-, 2- or 3-cell block |

Block height is 0.8 world units; the `_Center` part is the top cap only.

## 5. Materials

| Material | Use |
| --- | --- |
| `M_Block` | all block parts; per-block colour comes from the block palette (T-11) |
| `M_Floor`, `M_Wall` | floor tiles; wall segments and corners |
| `M_Door`, `M_Arrow` | door segments (tinted per colour); arrows and door markers |
| `M_Ice` | ice blocker overlay, base map `icetexture.jpg` |
