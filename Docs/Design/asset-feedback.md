# Art Asset Feedback

Problems found in the art assets (R-40). The project README (T-40) summarises this page. Add an entry the
moment a problem is found.

| # | Asset | Problem | Workaround |
| --- | --- | --- | --- |
| A-1 | `UI/btn_toggle_on.png` | Only the "on" state exists; settings toggles need an "off" state (R-15) | Off state built from the same sprite: grey tint and mirrored knob (T-25) |
| A-2 | `UI/Blockjam_Settings_*.png` | File names carry another product's name; inconsistent prefix casing across the set (`Btn_` / `btn_`, `bg_`, `ic_`) | Kept as imported so names map 1:1 to the source art folder |
| A-3 | `Models/WallAndDoor.fbx` | Contains helper nodes (`Key 1`, several `Scene.*` objects) beside the meshes; corner node names mix `Corner_1` and `corner_3/4/5` | Prefabs reference only the needed meshes (T-03) |
| A-4 | `Models/*.fbx` | No unit scale is documented; files use a 2-unit cell while `GroundGrid` and `Arrows` also carry a 0.01 file scale | Import scale 0.5 on every model, one cell = 1 unit (`art-pipeline.md` §3) |
| A-5 | `Models/WallAndDoor.fbx` | Unity import warning: "A polygon of Mesh 'corner_3.002' … is self-intersecting and has been discarded" | Check the corner visually in T-03/T-10; use another corner mesh if a hole shows |
| A-6 | `BlockParts.fbx`, `Arrows.fbx` | Authored Z-up while `WallAndDoor.fbx` and `GroundGrid.fbx` are Y-up | Part prefabs rotate the mesh child +90° on X (`art-pipeline.md` §3) |
| A-7 | `UI/Btn_levelBg_normal.png` | Circle and pattern baked into one sprite, so it cannot be nine-sliced for other widths | Nine-sliced with the whole circle in the left slice and drawn at its own aspect ratio, so nothing stretches (`art-pipeline.md` §2) |
| A-8 | `Models/WallAndDoor.fbx` | Exported upside down: walls, corners and doors have no top face, their only cap faces down at y = -0.31, so they render as open shells from above | Prefabs turn the mesh 180° on X (`art-pipeline.md` §3) |
| A-9 | `Models/WallAndDoor.fbx` | Wall pieces are half a cell long with rounded ends, so a straight wall shows a small notch every half cell | Kept: reads as a segmented rim; a cell-long wall mesh would remove it |
