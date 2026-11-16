# Rollic Case Study — Necati Akpınar

A Unity 2022.3 LTS mobile game built for the Rollic case study. The project starts from a production-shaped
skeleton so the case itself can be built on a clean, tested foundation.

## Highlights

- **Dependency injection with VContainer**: a session-wide root scope plus one scope per scene;
  installers are unit-tested by building them into a real container.
- **Weighted boot pipeline**: Splash runs `IBootStep`s with a progress bar, then hands over to Home.
- **Typed signal bus**: `readonly struct` signals, subscription at construction, re-entrancy safe.
- **Separated animation layer**: stateless DOTween recipes (`IAnimation<TConfig>`), awaited with UniTask.
- **Data-driven tuning**: every tunable lives in a ScriptableObject config.
- **Tests**: Edit Mode (logic, DI wiring, build settings) and Play Mode (real boot through Splash, Home and Gameplay).
- **Documented rules**: [CLAUDE.md](CLAUDE.md) (also [AGENTS.md](AGENTS.md)) and [Docs/](Docs/README.md),
  plus repository-local agent workflows in `.claude/skills/`.

## Tech

Unity 2022.3.62f2 · URP 14 (mobile) · VContainer 1.19 · UniTask 2.5 · DOTween 1.3 · Input System ·
Unity Test Framework

## Getting started

1. Install Git LFS, clone, open the folder in Unity Hub with **2022.3.62f2**.
2. Press Play from any scene: the editor always boots through **Splash**.
3. Run tests from **Window > General > Test Runner**.

More: [Docs/General/getting-started.md](Docs/General/getting-started.md) ·
[architecture](Docs/General/architecture.md) · [boot flow](Docs/General/splash-boot.md) ·
[testing](Docs/General/testing.md) · [case tasks](Docs/Case/README.md)

## Project layout

```
Assets/_Project/
  Scripts/Core      game-agnostic infrastructure (RollicCase.Core)
  Scripts/Game      scopes, boot, splash, gameplay (RollicCase.Game)
  Scripts/Editor    editor tooling
  Tests/EditMode    unit and wiring tests
  Tests/PlayMode    scene flow tests
  Scenes, Prefabs, Configs
Docs/               architecture, rules, case tasks, QA checklists
.claude/skills/     agent workflows (task breakdown, simplify, review, commit plan)
```
