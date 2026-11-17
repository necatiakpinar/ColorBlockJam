# Development Assumptions

> **Binding agent rules.** A player-visible decision the [design](../Design/README.md) does not settle is registered here
> before it ships, and its id (`DA-n`) is cited in the code comment and the commit footer.

Each entry: the decision, why it was needed, what would change if it is decided otherwise, and
its status (`Open`, `Confirmed <date>`, `Withdrawn <date>`).

| Id | Decision | Why | If decided otherwise | Status |
| --- | --- | --- | --- | --- |
| DA-1 | Portrait orientation, 1080x1920 canvas reference | Mobile casual default | Change Player orientation and every CanvasScaler | Confirmed 2026-11-16 (R-2) |
| DA-2 | Level numbers are 1-based and persist locally (PlayerPrefs) | Needed to show "Level N" before level progression exists | Swap `IKeyValueStorage` implementation; `PlayerProgressService` unchanged | Open |
| DA-3 | Splash stays up at least 1.5 s | Avoids a flash on fast devices | Tune `SplashConfig.MinimumDisplaySeconds` | Open |
| DA-4 | Controls: one finger drags one block; it follows the finger freely like a solid object (diagonals included), its drawing gliding after the finger over about 0.12 s instead of jumping, slides along walls and blocks instead of passing through them, slips into a one-cell gap when slightly off its line, leans a little toward the finger when it has no room, shows an outline while held, and on release settles on the nearest cell with a short tween | The design names the genre but not its rules | `BlockDragController` and `BoardState.TryStep` change; model tests rewritten | Open |
| DA-5 | A block exits when it is pushed against a door of its colour whose span covers the block's whole span on that wall and nothing stands between the block and that wall; the level is won when every block has exited | Exit and win rules are not written in the design | `DoorResolver` and `BoardState.IsCleared` change | Open |
| DA-6 | The timer starts when the level appears, freezes while paused or the app is backgrounded, and fails the level at 0 | R-18 says only "counts down and stops the level at zero" | `LevelTimer` start trigger changes (e.g. first move) | Open |
| DA-7 | Coins are earned only by winning: a fixed `GameplayConfig.CoinsPerWin` per win, credited before the win popup; the starting balance is 0 | R-19 defines no economy | `LevelRewardService` and config change; wallet unchanged | Open |
| DA-8 | After the last catalog level, levels loop from level 1 while the shown level number keeps rising | Only 5 levels exist | `LevelProvider` mapping changes | Open |
| DA-9 | Lives are a static placeholder: the heart shows a full count and never changes | R-23: the life system is a placeholder | Add a lives service; HUD and Home bind it | Open |
| DA-10 | Settings toggles persist locally and only change their own visual state; nothing plays audio or vibrates | R-14: settings buttons are placeholders; the art set has no audio | Audio and haptic services read `ISettingsService` | Open |
| DA-11 | Tapping a tab gives press feedback only; the Home tab stays active and no content changes | R-8: the tabs are placeholders | Tab bar moves the `ActiveTab` highlight | Open |
| DA-12 | Winning shows a win popup with the reward and one Continue button that loads the next level | R-24 defines no screen for continuing | Add a Home button to the win popup | Open |
| DA-13 | Levels are `LevelDefinition` ScriptableObject assets listed in one `LevelCatalog`; the editor writes them and the game reads them | R-29: one saved format; designers work in Unity | Swap to JSON files behind a loader; editor save path changes | Open |
| DA-14 | Leaving a level through Home discards the attempt with no penalty; coins and level stay as before the attempt | The design does not define abandoning | Add a life cost or a confirmation dialog | Open |
| DA-15 | Restart (from pause or fail) costs nothing and resets the board and timer | Lives are a placeholder | Restart consumes a life once DA-9 is replaced | Open |
| DA-16 | Boosters show four buttons (hammer, vacuum, rocket, freeze) with press feedback and no effect | "Boosters do not need a function" | T-36 makes one functional | Open |
| DA-17 | Doors sit on the outer wall ring only, never on corners; one colour per door; length 1 to the wall length | Door rules are not written | Editor door tool and `DoorData` change | Open |
| DA-18 | A block is one colour and any edge-connected set of cells (polyomino) | Block shapes are not listed | Editor block tool and validator change | Open |
| DA-19 | The board camera looks down steeply (72° tilt, 30° field of view) so blocks read mostly from the top, and the held block's outline traces its on-screen silhouette (bottom edge on the sides the camera sees, top rim on the others) | The design does not set the camera angle or the outline's look | Retune the Gameplay scene camera; `BoardFraming` refits any tilt | Confirmed 2026-11-17 (developer) |
| DA-20 | A block leaves as soon as it is pushed 0.15 cell into its own door; it settles at the door's mouth (0.08 s) and only then slides into it at full size at a steady 6 cells per second, cut away at the board's edge, while cubes of its colour spray out of the door | The design says blocks leave through doors but not how it looks | Tune `ExitSpeed`, `ExitDebrisRate` and `BlockExitBurst.prefab` | Confirmed 2026-11-17 (developer) |
