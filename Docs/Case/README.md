# Case

The design authority for gameplay. Every gameplay behavior traces to a requirement `R-n` below, to a task
`T-n`, or to a registered assumption `DA-n` in
[development-assumptions.md](../General/development-assumptions.md). Per-task subtasks, tests and docs
live in [tasks.md](tasks.md).

## 1. Brief

Original: [brief/game-developer-case-2026.pdf](brief/game-developer-case-2026.pdf) · text transcript:
[brief/brief.md](brief/brief.md). Received 2026-11-16; **deadline 2026-11-23** ("You have 7 days!").

**Goal.** "Build a small vertical slice of Color Block Jam in Unity. The slice must contain a playable level
flow and a level editor." Code quality, architecture and editor usability are evaluated; art is not.

**Genre summary (our reading).** A grid board surrounded by walls. Coloured multi-cell blocks sit on the
board. The player drags a block; it slides cell by cell until a wall or another block stops it. Wall
segments carry coloured doors; a block leaves the board through a door of its own colour that is wide
enough for it. The level is won when every block has left, and failed when the timer reaches zero. The
exact rules we implement are DA-4 to DA-6.

### 1.1 Requirements

Priority: **must** (explicit in the brief), **should** (evaluation criterion or strongly implied),
**could** (optional scope). Quotes are verbatim.

| Id | Requirement | Brief | Prio | Tasks |
| --- | --- | --- | --- | --- |
| R-1 | The project uses "Unity version: 2022.3.62f2" | §2 | must | T-01 |
| R-2 | "Reference resolution: 1080 x 1920 (portrait)" | §2 | must | T-02, T-05 |
| R-3 | "Use the assets that we supply. Do not buy or import other art assets unless you think is absolutely necessary." | §2 | must | T-03 |
| R-4 | The README says "which tools you used and for which parts" (LLM tools) | §2, §7 | must | T-40 |
| R-5 | One link with the Unity project: "Assets, Packages, and ProjectSettings … Do not include Library, Temp, or Build." | §3 | must | T-42 |
| R-6 | A `README.md` with the six §7 points: open and run, open the editor and make a level, architecture decisions with reasons, known problems, LLM tools, approximate work time | §3, §7 | must | T-40 |
| R-7 | "A build for Android (.apk) or a short screen recording (maximum 3 minutes) that shows the full flow." | §3 | must | T-41 |
| R-8 | Home shows "a tab navigation bar. The tabs are placeholders." | §4.1 | must | T-23 |
| R-9 | Home shows "the level button and the top UI" | §4.1 | must | T-23 |
| R-10 | "The level button starts the current level." | §4.1 | must | T-24 |
| R-11 | "The settings button opens the settings screen." | §4.1 | must | T-24 |
| R-12 | Other Home buttons "must give visual feedback on touch. They do not need a function." | §4.1 | must | T-05, T-23 |
| R-13 | "The layout is correct at 1080 x 1920 and stays correct on a 20:9 device." | §4.1 | must | T-05, T-23, T-39 |
| R-14 | Settings: "All buttons are placeholders, but each button must give visual feedback." | §4.2 | must | T-25 |
| R-15 | "Each toggle shows a different visual state for on and for off" (vibration, sound, music) | §4.2 | must | T-25 |
| R-16 | "The player can close the screen and return to the home screen." | §4.2 | must | T-25 |
| R-17 | "Implement the first 5 levels. Build the levels from the supplied assets." | §4.3 | must | T-08–T-13, T-32 |
| R-18 | "The timer counts down and stops the level at zero." | §4.3 | must | T-15 |
| R-19 | "The coin inventory must function." "The coin count stays correct after the player leaves the level and returns to it." | §4.3 | must | T-07, T-16 |
| R-20 | "The fail popup opens when the level fails. The popup gives a way to restart the level and a way to return to the home screen." | §4.3 | must | T-20 |
| R-21 | "Pause, restart, and home navigation must function." | §4.3 | must | T-19, T-22 |
| R-22 | "Boosters do not need a function." (they are shown) | §4.3 | must | T-18 |
| R-23 | "The life system can be a placeholder." | §4.3 | must | T-18, T-23 |
| R-24 | "The player can complete a level and continue to the next level." | §4.3 | must | T-13, T-17, T-21 |
| R-25 | "Restart sets the level back to its start state. No data stays from the previous attempt." | §4.3 | must | T-09, T-14, T-22 |
| R-26 | The editor controls "Board width and height", "Door placement", "Block placement", "Timer value" | §4.4 | must | T-27–T-29 |
| R-27 | "A designer can make a new level, save it, and play it. The designer does not write code and does not edit a file by hand." | §4.4 | must | T-26, T-30 |
| R-28 | "The editor shows the level in a visual form. It does not show only numeric fields." | §4.4 | must | T-27 |
| R-29 | "The saved data has one format. The game reads the same format." | §4.4 | must | T-08, T-30 |
| R-30 | "The 5 levels from section 4.3 come from this editor." | §4.4 | must | T-32 |
| R-31 | "Level validation. The editor gives a warning before it saves an unsolvable level." | §5 | could | T-33 |
| R-32 | "Functional boosters" | §5 | could | T-36 |
| R-33 | "Ice blocker" | §5 | could | T-34 |
| R-34 | "Arrow block" | §5 | could | T-35 |
| R-35 | Architecture: "Separation of data, logic, and view. Low coupling. The code is easy to extend." | §6 | should | all; T-37 |
| R-36 | "A designer can use the editor without help." | §6 | should | T-31 |
| R-37 | "Clear names, no dead code, consistent style." | §6 | should | all; T-37 |
| R-38 | "No unnecessary allocation in the update loop. Stable frame rate on a mid-range device." | §6 | should | T-38 |
| R-39 | "A small and complete result is better than a large and incomplete result." | §6 | should | §2 below |
| R-40 | "If you find a problem in the supplied assets, tell us." | §8 | should | T-03, T-40 |
| R-41 | "If a requirement is not clear, make a decision, continue, and write the decision in the README." | §8 | must | DA registry, T-40 |

## 2. Scope decisions

1. **Required scope first, end to end, before any optional item** (R-39). The order in
   [tasks.md](tasks.md) keeps `main` playable after every chapter.
2. **Optional scope (T-33–T-36) starts only when chapters 0–9 are Done.** Ranked by evaluation value:
   solvability validation (editor usability) → arrow block → ice blocker → one functional booster. The
   supplied assets include arrow meshes (`Arrows.fbx`, `Door_Arrow`) and `icetexture.jpg`, so the two
   blockers need no new art.
3. **No audio, haptics, shop, leaderboard, lives logic or ads.** The brief makes them placeholders. Settings
   toggles only store and show state (DA-10).
4. **One scene per screen:** Splash → Home → Gameplay. Settings and every gameplay popup are popups in their
   scene, not scenes.
5. **The level editor is a Unity `EditorWindow`**, not an in-game editor: designers already work in Unity,
   and the editor writes the exact asset the game loads (R-29, DA-13).
6. **Pure-C# board model** (no `MonoBehaviour`, no Unity physics) holds every rule, so rules are Edit Mode
   tested and the editor's validation reuses the same code.
7. Items from [Plans/pre-case-readiness.md](../Plans/pre-case-readiness.md) are absorbed only where a task
   needs them (safe area, popups, input, persistence, architecture tests); the rest is out.
8. **Cells, not world units, everywhere but the view.** The supplied models share a 2-unit grid cell
   (door 2 units wide, floor tile 1.95, block parts a quarter cell). They are imported at scale 0.5 so one
   cell is one world unit ([art-pipeline.md](../General/art-pipeline.md) §3). Level data, board model,
   rules and the level editor work in cell coordinates only. The cell-to-world conversion lives in one
   class, `BoardSpace`, and reads `BoardViewConfig.CellSize`; no code assumes a cell is 1 unit. **Why:**
   the art convention stays an import detail, the world size is one tunable, and changing either never
   touches data or rules. The submission README states this decision (T-40).

## 3. Open questions for the reviewer

| # | Question | Blocking | Answer |
| --- | --- | --- | --- |
| Q-1 | Which Unity version? | yes | 2022.3.62f2, as the brief requires (T-01) |
| Q-2 | Deliverable R-7: Android APK, screen recording, or both? Plan: both, APK first | no | _pending_ |
| Q-3 | Submission as a GitHub link or an archive? Decides whether CI is worth adding | no | _pending_ |
| Q-4 | May the brief PDF stay in the repository, or is it confidential? | no | _pending_ |

## 4. Tasks

Ids are stable (`T-01`, `T-02`, …); they appear in branch names and commit footers (`Refs: T-03`). A dropped
task is marked `Dropped`, never deleted. Detail per task: [tasks.md](tasks.md).

| Id | Ch | Task | Acceptance criteria (summary) | Depends on | Status |
| --- | --- | --- | --- | --- | --- |
| T-00 | — | Skeleton: DI, boot Splash → Gameplay, tests, docs, agent rules | Play from any scene reaches Gameplay; all tests green | — | Done |
| T-01 | 0 | Pin the project to Unity 2022.3.62f2 | Project opens in 2022.3.62f2 with no errors; all tests green | — | Done |
| T-02 | 0 | Player, Android and quality settings | Portrait-locked, IL2CPP ARM64 APK builds; mobile URP only | T-01 | In progress |
| T-03 | 0 | Import and configure the supplied assets | UI as sprites, models and materials ready; asset issues logged | T-01 | Done |
| T-04 | 1 | Home scene and scene routing | Splash → Home → Gameplay → Home works; tests green | T-01 | Done |
| T-05 | 1 | UI kit: safe area, button feedback, popup service | Every button scales on press; one popup at a time; layout fits 20:9 | T-04 | Done |
| T-06 | 1 | Board input and board space | Press, drag and release on board cells arrive as events, no polling | T-04 | Done |
| T-07 | 1 | Persistence: wallet, settings, progress | Coins, settings and level survive restart and app background | T-01 | Todo |
| T-08 | 2 | Level data format and catalog | `LevelDefinition` asset holds width, height, timer, blocks, doors | T-01 | Todo |
| T-09 | 2 | Board model (pure rules) | Move, block, exit and win rules pass Edit Mode tests | T-08 | Todo |
| T-10 | 3 | Board view: floor, walls, doors, camera fit | Any size up to the max shows framed at 1080x1920 and 20:9 | T-08, T-03 | Todo |
| T-11 | 3 | Block view: modular block meshes and colours | Any polyomino shows with correct corners and colour | T-10 | Todo |
| T-12 | 3 | Drag and slide | A dragged block follows the finger cell by cell and stops at obstacles | T-06, T-09, T-11 | Todo |
| T-13 | 3 | Exit through door and level complete | A block leaves through a matching door; last exit completes the level | T-12 | Todo |
| T-14 | 4 | Level session state machine | Loading → Playing ⇄ Paused → Won / Failed; restart rebuilds from data | T-09 | Todo |
| T-15 | 4 | Countdown timer | Counts down, pauses, fails the level at 0 | T-14 | Todo |
| T-16 | 4 | Coin rewards | Win adds the reward once, before the popup; leaving never changes coins | T-07, T-14 | Todo |
| T-17 | 4 | Level progression and looping | Win advances the level; after level 5 the catalog loops | T-07, T-14 | Todo |
| T-18 | 5 | Gameplay HUD | Level, timer, coins, pause, boosters and lives shown and updating | T-05, T-15, T-16 | Todo |
| T-19 | 5 | Pause popup | Pause freezes timer and input; resume, restart, home work | T-18 | Todo |
| T-20 | 5 | Fail popup | Opens on timeout; restart and home work | T-18 | Todo |
| T-21 | 5 | Win popup | Shows the reward; Continue loads the next level | T-18, T-17 | Todo |
| T-22 | 5 | Restart and home correctness | No state leaks across attempts; Play Mode tests prove it | T-19–T-21 | Todo |
| T-23 | 6 | Home screen layout | Top UI, level button, tab bar match the assets at 1080x1920 and 20:9 | T-05 | Todo |
| T-24 | 6 | Home behaviors | Level button plays the current level; settings opens Settings | T-23, T-04 | Todo |
| T-25 | 7 | Settings popup | Three toggles show on/off; buttons give feedback; close returns Home | T-05, T-07 | Todo |
| T-26 | 8 | Level editor window and level list | Designer opens the editor, creates, duplicates, deletes, reorders levels | T-08 | Todo |
| T-27 | 8 | Visual board canvas, size and timer | Board drawn as a grid; width, height and timer editable | T-26 | Todo |
| T-28 | 8 | Block placement tools | Paint, colour, move, rotate, delete blocks on the canvas | T-27 | Todo |
| T-29 | 8 | Door placement tools | Place, recolour, resize, delete doors on wall edges | T-27 | Todo |
| T-30 | 8 | Save, load, play-test and structural validation | Save writes the game's format; Play opens the level in the game | T-28, T-29, T-14 | Todo |
| T-31 | 8 | Editor usability pass and designer guide | A new user makes and plays a level with only the in-editor help | T-30 | Todo |
| T-32 | 9 | Author levels 1–5 in the editor | Five levels, rising difficulty, each solvable within its timer | T-31, T-22 | Todo |
| T-33 | 10 | (could) Solvability validation | Saving an unsolvable level shows a warning | T-30 | Todo |
| T-34 | 10 | (could) Ice blocker | Frozen block thaws after N exits; editor places it | T-32 | Todo |
| T-35 | 10 | (could) Arrow block | Block moves only along its arrow; editor places it | T-32 | Todo |
| T-36 | 10 | (could) One functional booster | Freeze or hammer works with a count | T-32 | Todo |
| T-37 | 11 | Architecture tests | Tests fail on `Update`, Core → Game references, raw `Debug.Log` | T-04 | Todo |
| T-38 | 11 | Performance pass | 0 B GC per gameplay frame measured; 60 FPS on a mid-range device | T-22 | Todo |
| T-39 | 11 | Full QA pass on device and aspect ratios | Every QA checklist ticked at 1080x1920 and 20:9 | T-32 | Todo |
| T-40 | 12 | Submission README | Covers every §7 point, decisions, asset feedback, LLM use, time | T-39 | Todo |
| T-41 | 12 | Android APK and flow recording | APK installs and plays; recording ≤ 3 min shows the full flow | T-39 | Todo |
| T-42 | 12 | Repository hygiene and hand-off | Clean clone opens and runs; no Library, Temp or Build; link sent | T-40, T-41 | Todo |

Status values: `Todo`, `In progress`, `Blocked`, `Done`, `Dropped`.

### 4.1 Suggested schedule (7 days)

| Day | Chapters |
| --- | --- |
| 1 (11-16) | 0 baseline, 1 foundations |
| 2 | 2 data and model, 3 board and blocks |
| 3 | 3 drag and exit, 4 session rules |
| 4 | 5 gameplay UI, 6 Home, 7 Settings |
| 5 | 8 level editor |
| 6 | 9 levels, 11 quality, optional items if time remains |
| 7 (11-23) | 12 delivery, buffer |
