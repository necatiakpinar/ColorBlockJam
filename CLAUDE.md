# Repository Guidelines

Guidance for coding agents (Claude Code, Codex, Cursor) and humans working in this repository.
This file is the **always-on rule set and an index**. `AGENTS.md` is a byte-identical copy; keep the two
in sync (`cp CLAUDE.md AGENTS.md`).

Subsystem detail lives in pages under `Docs/`. Wherever this file says **"Read X before touching Y"**,
reading X is mandatory before changing Y. A rule in a linked page binds exactly like one written here.
The index of every page is [Docs/README.md](Docs/README.md).

## Project Overview

Color Block Jam, a Unity 2022.3 LTS mobile puzzle game. Its foundation is a production-shaped
skeleton: dependency-injected boot from **Splash** to **Home** and **Gameplay**,
a typed signal bus, data-driven configs, separated animations, and Edit Mode + Play Mode tests.

- **Target:** mobile, portrait, URP (Mobile renderer). 60 FPS target via `ApplicationConfig`.
- **Design authority:** the requirements and the task list derived from them, under
  [Docs/Design/](Docs/Design/README.md). Every gameplay behavior must trace to a task there or to a
  registered assumption in [development-assumptions.md](Docs/General/development-assumptions.md).

## Repository-Local Skills

Skills live in `.claude/skills/<name>/SKILL.md` and are binding workflows for every agent:

| Skill | Run it |
| --- | --- |
| `task-breakdown` | when a feature request or design change arrives: turn it into tasks in `Docs/Design/` |
| `simplify` | at the end of every implementation or refactor |
| `staged-local-review` | after `simplify`, before proposing commits |
| `commit-plan` | at the end of every task: group the diff into atomic commits and propose messages |
| `record-agent-capabilities` | at the end of every task: record what tools could or could not do |

Validate skill frontmatter with `python tools/validate_skills.py`.

## Hard Rules For Agents

- **Never commit, push, amend, rebase or stash unless the developer explicitly asks in the current
  message.** Prepare the change, then run `commit-plan` and present the proposed commits. Approval for
  one commit never extends to the next.
- **Every change ships with its tests and its doc updates** in the same commit set.
- Never edit `Library/`, `Temp/`, `Logs/`, `UserSettings/` or generated `*.csproj` / `*.sln`.
- Never hand-edit a GUID in a `.meta` file; every new asset or folder under `Assets/` needs its `.meta`
  (Unity generates it; commit them together).

## Environment Setup

- Unity **2022.3.62f2** (R-1). Open the repository root as the Unity project.
- Packages resolve from Unity's registry and OpenUPM (scoped registry in `Packages/manifest.json`):
  VContainer, UniTask. DOTween (free) is vendored under `Assets/Plugins/Demigiant/DOTween` with a
  hand-authored `DOTween.Modules` assembly definition.
- Git LFS is required (`git lfs install`); binary art, audio, models and DLLs are LFS-tracked by
  `.gitattributes`.
- Detail and troubleshooting: [getting-started.md](Docs/General/getting-started.md).

## Build, Test & Development Commands

```bash
# Edit Mode + Play Mode tests, headless (close the editor holding the project first)
"<Unity>" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml
"<Unity>" -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults Logs/playmode.xml
```

In the editor: **Window > General > Test Runner**. Pressing Play always boots through Splash
(**ColorBlockJam > Play From Splash** toggles it). Detail: [testing.md](Docs/General/testing.md).

## Architecture

Handbook: [architecture.md](Docs/General/architecture.md). **Read it before touching a LifetimeScope,
an installer, the signal bus or scene loading.**

### Assemblies

| Assembly | Holds | May reference |
| --- | --- | --- |
| `ColorBlockJam.Core` | game-agnostic infrastructure: signals, boot sequence, scene loading, persistence, logging, `IAnimation<T>` | UniTask, VContainer, DOTween |
| `ColorBlockJam.Game` | scopes, installers, configs, boot steps, splash, gameplay, views, animations | Core + packages |
| `ColorBlockJam.Editor` | editor tooling | Core, Game |
| `ColorBlockJam.Tests.EditMode` / `.PlayMode` | tests | everything above |

Core never references Game. A new feature goes in Game unless it is reusable infrastructure.

### Dependency Injection (VContainer) — scope hierarchy

- **RootLifetimeScope** — prefab assigned in `VContainerSettings`, spawned before the first scene and
  kept for the session. Registers app-wide services through `CoreServicesInstaller`.
- **SplashLifetimeScope**, **HomeLifetimeScope**, **GameplayLifetimeScope** — one per scene; the root
  becomes their parent automatically. Scenes change only through `INavigationService`.
- **Installers (`IInstaller`) hold plain service registrations; entry points are registered by the
  scope.** That keeps every installer buildable in an Edit Mode test (`InstallerResolutionTests`).
- Constructor injection for plain classes; `[Inject] public void Construct(...)` for MonoBehaviours.
  Scene components are registered with `RegisterComponent` from serialized scope fields.
- Never use a service locator, a static singleton, `FindObjectOfType`/`FindFirstObjectByType`, or
  `Camera.main`. Resolve through DI.

### Signals (decoupled events)

`SignalBus` (Root scope) carries `readonly struct` signals. **Subscribe at construction only**: in the
constructor (plain classes) or `[Inject] Construct` (components). Unsubscribe in `Dispose` /
`OnDestroy`. Never subscribe in `Awake`, `Start`, `OnEnable`, `IStartable.Start` or
`IInitializable.Initialize`: they run after the scope builds, in an order nothing pins, so a signal fired
by an earlier entry point would be missed. Handlers run in subscription order.

### Flows are orchestration only

A *flow* (`SplashFlow`, `GameplayFlow`, and every future command-like orchestrator) sequences
presentation and routing. **It never owns data or domain decisions**: state is committed by the service
that owns it (boot steps, `IPlayerProgressService`, future level services) *before* the flow presents
the result. Never gate a data write on an animation or a flow finishing.

### Boot

Splash runs a `BootSequence` of `IBootStep`s (registered in `SplashInstaller`, executed in registration
order, weighted for the loading bar), waits for the minimum display time and the bar, then loads
Home. **Read [splash-boot.md](Docs/General/splash-boot.md) before touching `Splash/`, `Boot/`,
`SceneLoader` or the build scene list.**

### Gameplay

**Read [persistence.md](Docs/General/persistence.md) before touching `Scripts/Core/Persistence/` or saving anything
for the player. Read [economy.md](Docs/General/economy.md) before touching `Scripts/Game/Economy/`, rewards or a
level's difficulty.**

**Read [network.md](Docs/General/network.md) before touching `Scripts/Core/Network/`, `NetworkConfig` or anything
that reads the time of day.**

**Read [gameplay.md](Docs/General/gameplay.md) before touching `Scripts/Game/Gameplay/` or the Gameplay
scene.** Level data and rules work in cells; `BoardSpace` is the only cell ↔ world conversion.
**Read [level-data.md](Docs/General/level-data.md) before touching `Scripts/Game/Levels/`, a level asset or
the level editor.**

## Coding Standards

### Simplicity first

- No abstraction "just in case": an interface needs two implementations or a real seam (tests count).
- One responsibility per class; methods fit on one screen.
- No static classes or methods holding runtime logic (editor tooling excepted). Instance + DI.
- Tunables live in a ScriptableObject config (`<Area>Config`, `[CreateAssetMenu]`) registered in its
  scope. No magic numbers in logic.

### Naming

- Namespace `ColorBlockJam.<Assembly>.<Area>`; folders mirror namespaces.
- PascalCase types, members and constants; camelCase locals; `_camelCase` private and serialized fields.
- Enum members carry explicit, spaced values (`1000`, `2000`, …) so entries can be inserted without
  renumbering serialized data; `[Flags]` enums use powers of two.
- Tests: `Subject_ShouldOutcome_WhenContext`.

### Safety & error handling

- Let unexpected exceptions surface. No silent `catch`, no fallback defaults except true edge cases.
- **No null checks on serialized references in components.** A missing reference is a setup bug that
  must fail loudly in testing. Null-propagation on a component's own optional runtime state (e.g. a
  tween that may not have started) is fine.
- Async work uses UniTask and takes a `CancellationToken`; a scope's token is the default.

### Update loop

**Per-frame code is banned**: no `Update`, `LateUpdate`, `FixedUpdate` or `ITickable` unless a code
comment names the event that was unavailable. Use EventSystem handlers for input, signals for state
changes, tweens for motion with an end, `UniTask.Delay` for deadlines.

### GC allocations

Target 0 B GC alloc per gameplay frame. In hot paths: no LINQ, no `foreach` over `List<>`/`Dictionary<>`,
no string building (use `TMP_Text.SetText(format, value)`), no per-frame `new`, no capturing lambdas, no
`GetComponent`. Pool recyclable objects instead of `Instantiate`/`Destroy` churn.

### UI

**Read [ui.md](Docs/General/ui.md) before adding or changing a canvas, a button or a popup.**

- **UI is authored, never built in code**: hierarchies live in scenes or prefabs; scripts bind serialized
  references, text, sprites and `SetActive`.
- Canvases use **1080x1920** reference resolution, `Scale With Screen Size`, match 0.5, and respect the
  safe area.

### Animation & visual separation

- Motion lives in a **stateless `readonly struct` implementing `IAnimation<TConfig>`** whose `Play(in
  config)` builds a DOTween `Sequence` purely from the config (which carries the target). Animations are
  never MonoBehaviours.
- **No inline `DOTween`/`DO*` calls in views, flows, services or signal handlers.** Views own visual
  state and the returned `Sequence` (to kill it on destroy); timing values come from config assets.
- Tweens run on unscaled time and `SetLink` to their target so a destroyed view kills them.

### Logging

Use the injected `IGameLogger`, never raw `Debug.Log`. No logs on per-frame paths.

### Comments

Comment only non-obvious intent (why, not what). XML summaries on public types whose role is not obvious
from the name.

## Testing Guidelines

- **Every implementation or refactor adds or updates tests.** If automated coverage is impossible, say
  why and record the manual check in the area's QA checklist.
- Edit Mode tests cover pure logic and DI wiring; Play Mode tests cover scene flow end to end.
- Keep `Core` logic Unity-light so it is testable without scenes; use the fakes in
  `Tests/EditMode/Helpers` instead of PlayerPrefs, scenes or timing.
- QA checklists live in `Docs/testing/<area>-qa.md`; a case automated by a test names that test.
- Detail: [testing.md](Docs/General/testing.md).

## Documentation Maintenance

- **Docs are part of the change.** A commit that changes a documented fact carries the doc edit.
- This file is an index: add only rules that bind every change, or a one-line "Read X before touching
  Y" trigger. Subsystem detail goes in its own page under `Docs/General/`, listed in `Docs/README.md`.
- **Read [doc-authoring-rules.md](Docs/General/doc-authoring-rules.md) before writing any `.md`.**
- A player-visible decision the design does not answer is never a silent default: register it in
  [development-assumptions.md](Docs/General/development-assumptions.md) as `DA-n` and cite the id in
  code and commit message.

## Commit Guidelines

**Read [contributing.md](Docs/General/contributing.md) before proposing a commit.** In short:

- [Conventional Commits](https://www.conventionalcommits.org/): `type(scope): imperative summary`,
  under ~72 characters, types `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `build`, `ci`, `perf`.
- One logical change per commit; tests travel with the code they test; the tree compiles and tests pass
  at every commit.
- Never squash or rewrite published history. A review fix is a new commit.
