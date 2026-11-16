# Documentation Index

`CLAUDE.md` / `AGENTS.md` at the repository root hold the always-on rules and point here. Each page below
owns one area; when a fact changes, it changes in the page that owns it.

## Case

| Page | Owns |
| --- | --- |
| [Case/README.md](Case/README.md) | the case brief, requirements, scope, open questions, task status — the design authority |
| [Case/tasks.md](Case/tasks.md) | per-task subtasks, acceptance criteria, tests and docs |
| [Case/asset-feedback.md](Case/asset-feedback.md) | problems found in the supplied assets |
| [Case/brief/brief.md](Case/brief/brief.md) | verbatim text of the brief PDF |

## Plans

Dated planning artifacts. They schedule work; they never specify behavior.

| Page | Owns |
| --- | --- |
| [Plans/pre-case-readiness.md](Plans/pre-case-readiness.md) | gaps and foundations to build before the brief arrives |

## General (binding pages)

Pages marked **binding** carry rules agents must follow before touching the paths they name.

| Page | Owns | Binding |
| --- | --- | --- |
| [General/architecture.md](General/architecture.md) | assemblies, DI scopes, installers, signals, flows | yes |
| [General/splash-boot.md](General/splash-boot.md) | Splash scene, boot steps, scene handover | yes |
| [General/testing.md](General/testing.md) | test layout, naming, fakes, how to run | yes |
| [General/contributing.md](General/contributing.md) | branches, commit convention, commit planning | yes |
| [General/doc-authoring-rules.md](General/doc-authoring-rules.md) | how docs are written and kept true | yes |
| [General/getting-started.md](General/getting-started.md) | setup, packages, editor tooling | no |
| [General/development-assumptions.md](General/development-assumptions.md) | `DA-n` registry of unconfirmed product decisions | yes |
| [General/agent-capabilities.md](General/agent-capabilities.md) | what agents verified tools can and cannot do | no |

## QA checklists

| Page | Verifies |
| --- | --- |
| [testing/splash-gameplay-qa.md](testing/splash-gameplay-qa.md) | the boot from Splash to Gameplay |
| [testing/ui-qa.md](testing/ui-qa.md) | safe area, button feedback, popups (stub) |
| [testing/home-qa.md](testing/home-qa.md) | the Home screen (stub) |
| [testing/settings-qa.md](testing/settings-qa.md) | the Settings popup (stub) |
| [testing/gameplay-qa.md](testing/gameplay-qa.md) | gameplay, timer, coins, popups, levels 1–5 (stub) |
| [testing/level-editor-qa.md](testing/level-editor-qa.md) | the level editor (stub) |
