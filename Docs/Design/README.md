# Design

The design authority for Color Block Jam. Every gameplay behavior traces to a requirement `R-n` below, to
a task `T-n`, or to a registered assumption `DA-n` in
[development-assumptions.md](../General/development-assumptions.md). Per-task subtasks, tests and docs
live in [tasks.md](tasks.md).

## 1. Game

**Goal.** A mobile puzzle game with a complete level flow (Home → Gameplay → win or fail → next level)
and a level editor that designers use without writing code. Architecture, code quality and editor
usability come before art.

**Core loop.** A grid board surrounded by walls. Coloured multi-cell blocks sit on the board. The player
drags a block; it slides with the finger until a wall or another block stops it. Wall segments carry
coloured doors; a block leaves the board through a door of its own colour that is wide enough for it. The
level is won when every block has left, and failed when the timer reaches zero. The exact rules are
DA-4 to DA-6.

### 1.1 Requirements

Priority: **must** (core product), **should** (quality bar), **could** (optional scope). A dropped
requirement keeps its id and is marked `Dropped`.

| Id | Requirement | Prio | Tasks |
| --- | --- | --- | --- |
| R-1 | The project uses Unity 2022.3.62f2 | must | T-01 |
| R-2 | Reference resolution 1080 x 1920, portrait | must | T-02, T-05 |
| R-3 | The game is built from the project's own art set; new art is added only when a feature needs it | must | T-03 |
| R-4 | _Dropped_ | — | — |
| R-5 | The repository tracks only Assets, Packages, ProjectSettings, docs and tooling; never Library, Temp or Build | must | T-42 |
| R-6 | `README.md` explains how to open and run the project, how to make a level, the architecture decisions with reasons, and known problems | must | T-40 |
| R-7 | An Android build (.apk) plays the full flow on a device | must | T-41 |
| R-8 | Home shows a tab navigation bar; the tabs are placeholders | must | T-23 |
| R-9 | Home shows the level button and the top UI | must | T-23 |
| R-10 | The level button starts the current level | must | T-24 |
| R-11 | The settings button opens the settings screen | must | T-24 |
| R-12 | Other Home buttons give visual feedback on touch; they need no function | must | T-05, T-23 |
| R-13 | The layout is correct at 1080 x 1920 and on a 20:9 device | must | T-05, T-23, T-39 |
| R-14 | Settings buttons are placeholders that give visual feedback | must | T-25 |
| R-15 | The vibration, sound and music toggles each show a different visual state for on and off | must | T-25 |
| R-16 | The player can close Settings and return to Home | must | T-25 |
| R-17 | The first 5 levels are playable and built from the project's art | must | T-08–T-13, T-32 |
| R-18 | The timer counts down and stops the level at zero | must | T-15 |
| R-19 | The coin inventory works and stays correct after the player leaves a level and returns | must | T-07, T-16 |
| R-20 | The fail popup opens when the level fails and offers restart and return to Home | must | T-20 |
| R-21 | Pause, restart and Home navigation work | must | T-19, T-22 |
| R-22 | Boosters are shown; they need no function | must | T-18 |
| R-23 | The life system is a placeholder | must | T-18, T-23 |
| R-24 | The player can complete a level and continue to the next level | must | T-13, T-17, T-21 |
| R-25 | Restart sets the level back to its start state; no data stays from the previous attempt | must | T-09, T-14, T-22 |
| R-26 | The level editor controls board width and height, door placement, block placement and timer value | must | T-27–T-29 |
| R-27 | A designer makes a new level, saves it and plays it without writing code or editing a file by hand | must | T-26, T-30 |
| R-28 | The editor shows the level visually, not only as numeric fields | must | T-27 |
| R-29 | Saved levels have one format, and the game reads that same format | must | T-08, T-30 |
| R-30 | The 5 shipped levels are made with the editor | must | T-32 |
| R-31 | The editor warns before saving an unsolvable level | could | T-33 |
| R-32 | Functional boosters | could | T-36 |
| R-33 | Ice blocker | could | T-34 |
| R-34 | Arrow block | could | T-35 |
| R-35 | Separation of data, logic and view; low coupling; easy to extend | should | all; T-37 |
| R-36 | A designer can use the editor without help | should | T-31 |
| R-37 | Clear names, no dead code, consistent style | should | all; T-37 |
| R-38 | No unnecessary allocation in the update loop; stable frame rate on a mid-range device | should | T-38 |
| R-39 | A small and complete result before a large and incomplete one | should | §2 below |
| R-40 | Problems found in the art set are logged in [asset-feedback.md](asset-feedback.md) | should | T-03, T-40 |
| R-41 | An unclear requirement gets a decision, registered as `DA-n`, and work continues | must | DA registry, T-40 |
| R-42 | Touch and door feedback feel forgiving and clear: near-miss presses pick a block, wrong doors answer a push, doors open for a leaving block | should | T-43 |
| R-43 | The game needs a working internet connection: without one, a full-screen "No internet connection" popup with a Retry button blocks the game at boot and during play, and closes itself once the connection is back | must | T-44, T-45, T-46 |
| R-44 | Wall-clock game features (lives refill, daily rewards) read a trusted time taken from the internet, never the device clock | should | T-44 |
| R-45 | Changing the device clock cannot change the level timer | must | T-15 |

## 2. Scope decisions

1. **Required scope first, end to end, before any optional item** (R-39). The order in
   [tasks.md](tasks.md) keeps `main` playable after every chapter.
2. **Optional scope (T-33–T-36) starts only when chapters 0–9 are Done.** Ranked by value: solvability
   validation (editor usability) → arrow block → ice blocker → one functional booster. The art set
   includes arrow meshes (`Arrows.fbx`, `Door_Arrow`) and `icetexture.jpg`, so the two blockers need no
   new art.
3. **No audio, haptics, shop, leaderboard, lives logic or ads** for now. Settings toggles only store and
   show state (DA-10).
4. **One scene per screen:** Splash → Home → Gameplay. Settings and every gameplay popup are popups in their
   scene, not scenes.
5. **The level editor is a Unity `EditorWindow`**, not an in-game editor: designers already work in Unity,
   and the editor writes the exact asset the game loads (R-29, DA-13).
6. **Pure-C# board model** (no `MonoBehaviour`, no Unity physics) holds every rule, so rules are Edit Mode
   tested and the editor's validation reuses the same code.
7. **Foundations on demand:** safe area, popups, input, persistence and architecture tests are built only
   when a task needs them.
8. **Cells, not world units, everywhere but the view.** The art set's models share a 2-unit grid cell
   (door 2 units wide, floor tile 1.95, block parts a quarter cell). They are imported at scale 0.5 so one
   cell is one world unit ([art-pipeline.md](../General/art-pipeline.md) §3). Level data, board model,
   rules and the level editor work in cell coordinates only. The cell-to-world conversion lives in one
   class, `BoardSpace`, and reads `BoardViewConfig.CellSize`; no code assumes a cell is 1 unit. **Why:**
   the art convention stays an import detail, the world size is one tunable, and changing either never
   touches data or rules. The project README states this decision (T-40).
9. **Online only** (R-43, developer decision 2026-11-17). A real request to a probe URL decides whether
   the game is online; Unity's `Application.internetReachability` only reports a network interface, so
   Wi-Fi without internet would pass it. The probe's response also carries the server's time (HTTP `Date`
   header), kept as the trusted wall clock (R-44). The level timer never needed it: it counts monotonic
   real time, which the device clock does not move (R-45, T-15). The no-internet popup sits above every
   scene and every other popup, so it lives on the Root scope, outside the scenes' one-popup
   `IPopupService`. This replaces the rule that Splash never waits on the network (splash-boot.md §1).

## 3. Open questions

| # | Question | Blocking | Answer |
| --- | --- | --- | --- |
| Q-1 | Which Unity version? | yes | 2022.3.62f2 (T-01) |
| Q-2 | _Dropped_ | — | — |
| Q-3 | _Dropped_ | — | — |
| Q-4 | _Dropped_ | — | — |

## 4. Tasks

Ids are stable (`T-01`, `T-02`, …); they appear in branch names and commit footers (`Refs: T-03`). A dropped
task is marked `Dropped`, never deleted. Detail per task: [tasks.md](tasks.md).

| Id | Ch | Task | Acceptance criteria (summary) | Depends on | Status |
| --- | --- | --- | --- | --- | --- |
| T-00 | — | Skeleton: DI, boot Splash → Gameplay, tests, docs, agent rules | Play from any scene reaches Gameplay; all tests green | — | Done |
| T-01 | 0 | Pin the project to Unity 2022.3.62f2 | Project opens in 2022.3.62f2 with no errors; all tests green | — | Done |
| T-02 | 0 | Player, Android and quality settings | Portrait-locked, IL2CPP ARM64 APK builds; mobile URP only | T-01 | In progress |
| T-03 | 0 | Import and configure the art assets | UI as sprites, models and materials ready; asset issues logged | T-01 | Done |
| T-04 | 1 | Home scene and scene routing | Splash → Home → Gameplay → Home works; tests green | T-01 | Done |
| T-05 | 1 | UI kit: safe area, button feedback, popup service | Every button scales on press; one popup at a time; layout fits 20:9 | T-04 | Done |
| T-06 | 1 | Board input and board space | Press, drag and release on board cells arrive as events, no polling | T-04 | Done |
| T-07 | 1 | Persistence: wallet, settings, progress | Coins, settings and level survive restart and app background | T-01 | Done |
| T-08 | 2 | Level data format and catalog | `LevelDefinition` asset holds width, height, timer, blocks, doors | T-01 | Done |
| T-09 | 2 | Board model (pure rules) | Move, block, exit and win rules pass Edit Mode tests | T-08 | Done |
| T-10 | 3 | Board view: floor, walls, doors, camera fit | Any size up to the max shows framed at 1080x1920 and 20:9 | T-08, T-03 | Done |
| T-11 | 3 | Block view: modular block meshes and colours | Any polyomino shows with correct corners and colour | T-10 | Done |
| T-12 | 3 | Drag and slide | A held block follows the finger smoothly, stops at obstacles, snaps to its cell on release | T-06, T-09, T-11 | Done |
| T-13 | 3 | Exit through door and level complete | A block leaves through a matching door; last exit completes the level | T-12 | Done |
| T-14 | 4 | Level session state machine | Loading → Playing ⇄ Paused → Won / Failed; restart rebuilds from data | T-09 | Done |
| T-15 | 4 | Countdown timer | Counts down, pauses, fails the level at 0 | T-14 | Done |
| T-16 | 4 | Coin rewards | Win adds the reward once, before the popup; leaving never changes coins | T-07, T-14 | Done |
| T-17 | 4 | Level progression and looping | Win advances the level; after level 5 the catalog loops | T-07, T-14 | Done |
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
| T-32 | 9 | Author levels 1–5 in the editor | Five levels, rising difficulty, each solvable within its timer | T-31, T-22 | In progress |
| T-33 | 10 | (could) Solvability validation | Saving an unsolvable level shows a warning | T-30 | Todo |
| T-34 | 10 | (could) Ice blocker | Frozen block thaws after N exits; editor places it | T-32 | Todo |
| T-35 | 10 | (could) Arrow block | Block moves only along its arrow; editor places it | T-32 | Todo |
| T-36 | 10 | (could) One functional booster | Freeze or hammer works with a count | T-32 | Todo |
| T-37 | 11 | Architecture tests | Tests fail on `Update`, Core → Game references, raw `Debug.Log` | T-04 | Todo |
| T-38 | 11 | Performance pass | 0 B GC per gameplay frame measured; 60 FPS on a mid-range device | T-22 | Todo |
| T-39 | 11 | Full QA pass on device and aspect ratios | Every QA checklist ticked at 1080x1920 and 20:9 | T-32 | Todo |
| T-40 | 12 | Project README | Open and run, level editor how-to, architecture decisions, known problems, asset feedback | T-39 | Todo |
| T-41 | 12 | Android APK | APK installs and plays the full flow on a device | T-39 | Todo |
| T-42 | 12 | Repository hygiene | Clean clone opens and runs; no Library, Temp or Build tracked | T-40, T-41 | Todo |
| T-44 | 4 | Connectivity probe and trusted time | Online state follows a real request; server time is readable once a probe succeeds | T-04 | Done |
| T-45 | 5 | No-internet popup over every scene | Going offline anywhere shows the blocking popup; in gameplay the timer and input stop; back online closes it | T-44, T-05, T-15 | Todo |
| T-46 | 5 | Boot waits for a connection | Splash does not reach Home offline; the popup's Retry or a returning connection continues the boot | T-44, T-45 | Todo |
| T-43 | 3 | Touch and door feel | Near-miss presses pick a block; wrong doors stop a block like a wall; doors open and close; close exits share one spray | T-12, T-13 | Done |

Status values: `Todo`, `In progress`, `Blocked`, `Done`, `Dropped`.
