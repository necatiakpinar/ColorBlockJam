# Color Block Jam

A Unity 2022.3 LTS mobile puzzle game for portrait phones. The player drags coloured blocks across a
walled board and out through doors of their colour before the timer runs out. A level editor lets
designers make, check and play levels without writing code. The full design is in
[Docs/Design/requirements.md](Docs/Design/requirements.md).

## 1. Highlights

- **Full level flow**: Splash → Home → Gameplay → win or fail → next level, with a timer, coins, pause,
  restart and popups.
- **Twenty levels on a saw-tooth curve**: gentle levels 1–8, then groups of six that each end on a harder
  peak ([level-data.md](Docs/General/level-data.md) §3).
- **Level editor**: a Unity window with a visual board, block and door tools, live checks, a shortcut for
  every tool, a generator and a hardener ([level-editor.md](Docs/General/level-editor.md)).
- **Fast bit-mask solver**: proves every level can be finished, with the game's own rules, and shows the
  solution step by step. It runs on a worker thread, so checks never hold up an edit.
- **Level hardener**: **Make harder** climbs a level toward more room-making moves, with arrows and ice,
  and keeps it valid and solvable.
- **Level packs as JSON, ready for remote config**: the game plays `levels.json`; a remote pack is checked
  and plays from the next launch ([level-packs.md](Docs/General/level-packs.md)).
- **Blockers**: ice (a block thaws after N others leave) and arrows (a block moves along one axis only)
  ([blockers.md](Docs/General/blockers.md)).
- **Boosters**: freeze, rocket, hammer and vacuum, each with a saved stock and a coin price
  ([boosters.md](Docs/General/boosters.md)).
- **Lives**: five lives that refill on trusted internet time, or at once for coins
  ([lives.md](Docs/General/lives.md)).
- **Dependency injection with VContainer**: a session-wide root scope plus one scope per scene; installers
  are unit-tested by building them into a real container.
- **Weighted boot pipeline**: Splash runs `IBootStep`s with a progress bar, then hands over to Home.
- **Typed signal bus**: `readonly struct` signals, subscription at construction, re-entrancy safe.
- **Separated animation layer**: stateless DOTween recipes (`IAnimation<TConfig>`), awaited with UniTask.
- **Data-driven tuning**: every tunable lives in a ScriptableObject config.
- **Tests**: Edit Mode (rules, solver, generator, DI wiring, build settings) and Play Mode (real boot
  through Splash, Home and Gameplay). Architecture tests enforce the rules: no per-frame code, Core never
  references Game, no raw logs, every scene reference set. A Play Mode test proves the drag allocates 0 B
  ([performance.md](Docs/General/performance.md)).
- **Documented rules**: [CLAUDE.md](CLAUDE.md) (also [AGENTS.md](AGENTS.md)) and [Docs/](Docs/index.md),
  plus repository-local agent workflows in `.claude/skills/`.

## 2. Tech

Unity 2022.3.62f2 · URP 14 (mobile) · VContainer 1.19 · UniTask 2.5 · DOTween 1.3 · Input System ·
Unity Test Framework

## 3. Open and run

1. Install Unity **2022.3.62f2** and Git LFS. Run `git lfs install` once per machine, then clone.
2. Open the repository root in Unity Hub. Packages resolve by themselves.
3. Press Play from any scene: the editor always boots through **Splash**.
   **ColorBlockJam > Play From Splash** turns that off and on.
4. Run the tests from **Window > General > Test Runner** (EditMode and PlayMode tabs), or headless
   (close the editor holding the project first):

```bash
"<Unity>" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml
"<Unity>" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults Logs/playmode.xml
```

Packages, the Android build and troubleshooting: [getting-started.md](Docs/General/getting-started.md).
Tests: [testing.md](Docs/General/testing.md). Checking by hand without finishing a level:
**ColorBlockJam > Debug Window** ([debug-tools.md](Docs/General/debug-tools.md)).

## 4. Make a level

Open **ColorBlockJam > Level Editor** (`Ctrl+Shift+L`). The left panel lists the levels in play order,
the middle shows the board from above, the right panel holds the tools, checks and generator.
Press **+ New** (`N`) under the list: it adds a small starter level at the end and opens it.

1. **Board.** Set Width, Height, Timer and Difficulty under *Level*.
2. **Doors.** Door tool (`3`): pick a colour, drag along a wall.
3. **Blocks.** Block tool (`2`): pick a colour and a shape, click to stamp it. Select a block to add
   **Ice** (`I`) or an **Arrow** (`A`).
4. **Checks.** Fix every red line. The level must say **Solvable**.
5. **Save and play.** `Ctrl+S`, then ▶ Play (`P`): the game boots and opens this level.

| Tool | Key | Does |
| --- | --- | --- |
| Generate | `G` | builds a valid, solvable level from size, colours, blocks, difficulty and seed |
| Make harder | `H` | runs the hardener: more room-making moves, still valid and solvable |
| Check | `C` | validates at once and runs the solver; `Space` steps through the solution |
| Save | `Ctrl+S` | writes the level asset and rewrites `levels.json`, the pack the game plays |
| Play | `P` | saves and starts the game in this level |

Saving an unsolvable level asks first. Every shortcut, the checks and the generator:
[level-editor.md](Docs/General/level-editor.md). Publishing a pack to remote config:
[level-packs.md](Docs/General/level-packs.md) §4.

Screenshots of the editor are pending.

## 5. Architecture decisions

The handbook is [architecture.md](Docs/General/architecture.md).

| Decision | Reason |
| --- | --- |
| **DI scopes.** A VContainer root scope lives for the session; Splash, Home and Gameplay each have a scope that dies with the scene. Installers hold plain registrations. | Every dependency is explicit and swappable in tests; no singletons or scene searches. Installers build in an Edit Mode test without scenes. ([architecture.md](Docs/General/architecture.md) §2) |
| **Typed signal bus.** Events are `readonly struct` signals; listeners subscribe at construction only. | Systems react to facts without knowing each other. Subscribing at construction means no signal is missed by entry-point start order. ([architecture.md](Docs/General/architecture.md) §3) |
| **Flows orchestrate only.** A flow sequences presentation and routing; the owning service commits data first. | Game state is correct even if every animation is skipped or cut short. ([architecture.md](Docs/General/architecture.md) §4) |
| **Pure board model.** `BoardState` is plain C# that holds every rule of one attempt and is built fresh on each start and restart. | Rules are Edit Mode tested, a restart cannot leak state, and the editor's solver reuses the same code. ([gameplay.md](Docs/General/gameplay.md) §3) |
| **Cells only; `BoardSpace` converts.** Level data, rules and the editor work in cells; `BoardSpace` is the only cell ↔ world conversion. | The world size is one tunable (`BoardViewConfig.CellSize`); changing it never touches data or rules. ([gameplay.md](Docs/General/gameplay.md) §1) |
| **The 2-unit art cell is normalized at import.** The models share a 2-unit cell and import at scale 0.5, so one cell is one world unit. | The art convention stays an import detail. ([Design requirements](Docs/Design/requirements.md) §2.8, [art-pipeline.md](Docs/General/art-pipeline.md) §3) |
| **ScriptableObject levels, JSON packs at runtime.** The editor authors `LevelDefinition` assets in a `LevelCatalog` and publishes them as `levels.json`; the game reads only packs. | Designers work in Unity with undo and git history, while levels can change from remote config without an app update. ([level-data.md](Docs/General/level-data.md), [level-packs.md](Docs/General/level-packs.md)) |
| **The solver reads rules from `BoardState`.** It asks a real board once per level, records the answers as 128-bit cell masks and searches with bit operations. | The rules are never copied, so the solver cannot drift from the game, and the search stays fast. ([level-editor.md](Docs/General/level-editor.md) §3) |
| **Separated animations.** Motion is a stateless `readonly struct` implementing `IAnimation<TConfig>`; no inline DOTween in views, flows or services. | Motion is reusable and tuned from configs; views only own the running sequence. ([architecture.md](Docs/General/architecture.md) §6) |
| **No per-frame code.** No `Update`, `LateUpdate`, `FixedUpdate` or `ITickable`; input comes from EventSystem handlers, state from signals, motion from tweens. | Nothing runs when nothing happens, which keeps the frame cheap and free of GC allocation. ([CLAUDE.md](CLAUDE.md)) |
| **Data-driven configs.** Every tunable is an `<Area>Config` ScriptableObject; all of them sit in one `ConfigProvider`, which registers each by its type. | Designers tune without code changes; logic has no magic numbers. ([architecture.md](Docs/General/architecture.md) §5) |

## 6. Decisions for unclear requirements

Each open product question got a decision, registered as `DA-n` and cited in the docs and commits. Why,
alternatives and status: [development-assumptions.md](Docs/General/development-assumptions.md).
Withdrawn entries (DA-9, DA-15, DA-16, DA-23, DA-26, DA-30) are left out below.

| Id | Decision |
| --- | --- |
| DA-1 | Portrait, 1080x1920 canvas reference |
| DA-2 | Level numbers are 1-based and saved locally |
| DA-3 | Splash stays up at least 1.5 s |
| DA-4 | One finger drags one block freely; it slides along walls and settles on the nearest cell |
| DA-5 | A block exits through a door of its colour that covers its whole span; all blocks out wins |
| DA-6 | The timer starts at the first grab and freezes while paused or in the background |
| DA-7 | Coins come only from wins: Normal 10, Hard 25, Very Hard 50 |
| DA-8 | After the last level, levels loop while the shown number keeps rising |
| DA-10 | Settings toggles save and show their state; sound mutes the effects, vibration the haptics, music has nothing to control yet |
| DA-11 | Home tabs give press feedback only |
| DA-12 | The win popup shows the reward and one Continue button |
| DA-13 | Levels are `LevelDefinition` assets in one `LevelCatalog` |
| DA-14 | Leaving a level through Home keeps coins and level as before; a started attempt costs a life, after a confirmation |
| DA-17 | Doors sit on the outer wall ring, never on corners, one colour each |
| DA-18 | A block is one colour and any edge-connected shape |
| DA-19 | The camera looks down steeply (72° tilt, 30° field of view) |
| DA-20 | How a block leaves: settle at the door, slide in, debris burst |
| DA-21 | Forgiving touch, a wrong door acts as a wall, doors sink while a block passes |
| DA-22 | Pause, win or fail drops a held block; no restart after a win |
| DA-24 | One arrow on each door, pointing out of the board |
| DA-25 | Online is decided by a probe request with a `Date` header |
| DA-27 | Trusted time is the last probe's `Date` plus monotonic time since |
| DA-28 | The gameplay HUD layout follows the reference; no lives in gameplay |
| DA-29 | Pause opens the settings popup; closing it resumes |
| DA-31 | Running out of time shows one "Level Failed!" popup: Retry (or a refill with no lives) and Return to Menu |
| DA-32 | Solvable means some order of moves clears the level; a move follows any free path |
| DA-33 | Difficulty is exit waves plus room-making moves; the generator's timer formula |
| DA-34 | The level editor's keyboard shortcuts |
| DA-36 | Boosters: 20 of each to start, coin prices, freeze for 10 s, spent only when they work |
| DA-37 | Ice counts down with every block that leaves; the hammer breaks it |
| DA-38 | Five lives, one back every 30 minutes of trusted time, a refill for 100 coins |
| DA-39 | An arrow block moves and leaves only along its axis |
| DA-40 | The 20-level curve and how its hard levels and timers are made |
| DA-41 | A remote level pack plays from the next launch, only if valid and not older |
| DA-42 | The last 10 s flash the timer red and tick |
| DA-43 | One short synthesized sound per action; the sound toggle mutes them all |
| DA-44 | Haptics on grab, exit, win and fail; the vibration toggle silences them |
| DA-45 | The current level's tile on Home breathes |
| DA-46 | Every scene change fades through black |
| DA-47 | Home's level path stretches like a scroll view |
| DA-48 | A strip at the bottom of Gameplay stays free for an ad banner |

## 7. Known problems and incomplete parts

Open tasks from the [task table](Docs/Design/requirements.md) §4:

| Task | Status | What is missing |
| --- | --- | --- |
| T-02 | In progress | Player, Android and quality settings |
| T-38 | In progress | Profiling a full level on an Android device; the editor checks are done ([performance.md](Docs/General/performance.md)) |
| T-39 | In progress | The manual (👁) QA cases at 1080x1920 and 20:9, in the Game view and on a device |
| T-41 | Todo | Release APK installed and smoke-tested on a device |

Other known gaps:

- Nothing fetches remote config yet; `LevelPackService.TryStoreRemote` is the seam a fetcher calls
  ([level-packs.md](Docs/General/level-packs.md) §4).
- The level editor window is checked by hand only ([level-editor-qa.md](Docs/testing/level-editor-qa.md)).
- Several QA checklists are still stubs ([Docs/index.md](Docs/index.md)).
- Editor screenshots are pending (§4).

### Found in the QA pass

Automated part of T-39, last run 2026-11-18 on a fresh clone of `origin/main`:
- **Suites.** Edit Mode 643/643 and Play Mode 49/49 pass. Every ⚙ case on the QA pages is ticked.
- **Fixed: a test that depended on the save.** `BoosterFlowTests` hammered block 0 of whatever level the
  save held. On a level where that block is frozen, the hammer only breaks the ice (DA-37), so the test
  failed. It now picks a block without ice.
- **Fixed: QA cases that named missing tests.** Two navigation cases named tests that no longer exist.
  They now point to the tests that cover them.
- **Open: the manual cases.** The 57 👁 cases (looks, feel, device) are not ticked; they need a person
  with a device.
- **Open: draw calls.** Gameplay draws 173 batches with 8 SetPass calls. That is fine for a mid-range phone,
  but worth a check on a device ([performance.md](Docs/General/performance.md) §4).

## 8. Asset feedback

Problems found in the art set, each with its workaround:

- **Scale and axes.** The models use an undocumented 2-unit cell (some also a 0.01 file scale), mix Z-up
  and Y-up, and `WallAndDoor.fbx` is exported upside down. Import settings and part prefabs correct all
  three.
- **Missing states.** The settings toggle has only an "on" sprite; the "off" state is built from it.
- **Mesh issues.** A self-intersecting corner polygon, helper nodes beside the meshes, and half-cell
  wall pieces that show a small notch every half cell.
- **Naming and slicing.** Inconsistent file-name prefixes, and a level button sprite that cannot be
  nine-sliced.

Everything on screen comes from the given art set, plus two additions it does not cover:

| Added | Why | Source |
| --- | --- | --- |
| Lilita One font (`Assets/_Project/Art/Fonts/LilitaOne`) | the set has no font; every text needs the rounded, bold look of the reference | Google Fonts, SIL Open Font License (`OFL.txt` beside it) |
| Seven sound effects (`Assets/_Project/Audio/sfx_*.wav`) | the set has no audio (DA-43) | synthesized by `tools/synth_sfx.py` in this repository; no third-party audio |

## 9. Documentation map

| Page | Owns |
| --- | --- |
| [Docs/index.md](Docs/index.md) | the index of every page |
| [Design/requirements.md](Docs/Design/requirements.md) | requirements, scope decisions, task status |
| [Design/tasks.md](Docs/Design/tasks.md) | subtasks and acceptance criteria per task |
| [getting-started.md](Docs/General/getting-started.md) | setup, packages, Android build |
| [architecture.md](Docs/General/architecture.md) | assemblies, DI scopes, signals, flows, animations |
| [gameplay.md](Docs/General/gameplay.md) | board space, input, rules, session, timer |
| [level-data.md](Docs/General/level-data.md) | level format, validation, the shipped levels |
| [level-packs.md](Docs/General/level-packs.md) | the JSON pack the game plays, remote config |
| [level-editor.md](Docs/General/level-editor.md) | the editor, solver, generator and hardener |
| [blockers.md](Docs/General/blockers.md), [boosters.md](Docs/General/boosters.md), [lives.md](Docs/General/lives.md) | ice and arrows; the four boosters; lives |
| [development-assumptions.md](Docs/General/development-assumptions.md) | the `DA-n` registry |
| [testing.md](Docs/General/testing.md) | test layout and how to run |
| [contributing.md](Docs/General/contributing.md) | commit convention |

## 10. Project layout

```
Assets/_Project/
  Scripts/Core      game-agnostic infrastructure (ColorBlockJam.Core)
  Scripts/Game      scopes, boot, gameplay, levels, solver, generator (ColorBlockJam.Game)
  Scripts/Editor    level editor, debug window, editor tooling
  Tests/EditMode    unit and wiring tests
  Tests/PlayMode    scene flow tests
  Levels            level assets, LevelCatalog and levels.json
  Scenes, Prefabs, Configs
Docs/               design, architecture, rules, tasks, QA checklists
.claude/skills/     agent workflows (task breakdown, simplify, review, commit plan)
```

## 11. AI tools and time

The game was built in about **16 hours** with **Claude** (Claude Code) as the coding assistant.

| Used for | How |
| --- | --- |
| Code and refactors | Claude wrote and changed the C# under the rules in [CLAUDE.md](CLAUDE.md); the developer set the design, reviewed every change and approved each commit. |
| Tests | Claude wrote the Edit Mode and Play Mode tests that travel with each change and ran them headless. |
| Docs | Claude kept the pages under [Docs/](Docs/index.md), the task list and the `DA-n` registry in step with the code. |
| Reviews | Claude reviewed the code before commits, using the workflows in `.claude/skills/`. |
| Sound effects | Claude synthesised the sound effects with `tools/synth_sfx.py`. |
