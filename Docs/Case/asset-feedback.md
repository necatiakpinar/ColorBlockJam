# Supplied Asset Feedback

Problems found in the supplied assets (brief §8: "If you find a problem in the supplied assets, tell us").
The submission README (T-40) summarises this page. Add an entry the moment a problem is found.

| # | Asset | Problem | Workaround |
| --- | --- | --- | --- |
| A-1 | `UI/btn_toggle_on.png` | Only the "on" state is supplied; settings toggles need an "off" state (R-15) | Off state built from the same sprite: grey tint and mirrored knob (T-25) |
| A-2 | `UI/Blockjam_Settings_*.png` | File names carry another product's name; inconsistent prefix casing across the set (`Btn_` / `btn_`, `bg_`, `ic_`) | Kept as supplied so the delivery maps 1:1 to the drive folder |
| A-3 | `Models/WallAndDoor.fbx` | Contains helper nodes (`Key 1`, several `Scene.*` objects) beside the meshes; corner node names mix `Corner_1` and `corner_3/4/5` | Prefabs reference only the needed meshes (T-03) |
| A-4 | `Models/*.fbx` | No unit scale or pivot convention is documented; to be measured during import | Measured scale recorded in `art-pipeline.md` (T-03) |
| A-5 | `Models/WallAndDoor.fbx` | Unity import warning: "A polygon of Mesh 'corner_3.002' … is self-intersecting and has been discarded" | Check the corner visually in T-03/T-10; use another corner mesh if a hole shows |
