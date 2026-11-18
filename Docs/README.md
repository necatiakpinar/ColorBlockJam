# Documentation Index

`CLAUDE.md` / `AGENTS.md` at the repository root hold the always-on rules and point here. Each page below
owns one area; when a fact changes, it changes in the page that owns it.

## Design

| Page | Owns |
| --- | --- |
| [Design/README.md](Design/README.md) | the game, requirements, scope, open questions, task status — the design authority |
| [Design/tasks.md](Design/tasks.md) | per-task subtasks, acceptance criteria, tests and docs |
| [Design/code-review-tasks.md](Design/code-review-tasks.md) | improvement tasks (`CR-n`) and open questions from the full code review |
| [Design/asset-feedback.md](Design/asset-feedback.md) | problems found in the art assets |
| [Design/deconstruction.md](Design/deconstruction.md) | what the reference game shows on screen, measured from recordings, and how this project matches it |

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
| [General/ui.md](General/ui.md) | canvases, safe area, button feedback, popups | yes |
| [General/gameplay.md](General/gameplay.md) | board space, input, movement and exit rules | yes |
| [General/level-data.md](General/level-data.md) | level format, catalog, validation rules | yes |
| [General/performance.md](General/performance.md) | per-frame and allocation checks, profiler markers, measured numbers | yes |
| [General/level-packs.md](General/level-packs.md) | the level pack JSON the game plays, bundled or from remote config | yes |
| [General/persistence.md](General/persistence.md) | saved player data: progress, wallet, settings | yes |
| [General/economy.md](General/economy.md) | coins: win rewards by level difficulty | yes |
| [General/addressables.md](General/addressables.md) | runtime asset loading: Addressables, `AssetLoader`, scopes, settings | yes |
| [General/network.md](General/network.md) | online requirement: connectivity probe, trusted time | yes |
| [General/art-pipeline.md](General/art-pipeline.md) | art folders, import rules, cell scale, part prefabs, materials | yes |
| [General/development-assumptions.md](General/development-assumptions.md) | `DA-n` registry of unconfirmed product decisions | yes |
| [General/lives.md](General/lives.md) | lives: the rules, trusted-time refill, the coin refill, the Home bar and popup | yes |
| [General/blockers.md](General/blockers.md) | blockers (ice, ...): data, rule interface, views, editor row; how to add a kind | yes |
| [General/boosters.md](General/boosters.md) | the four boosters: stock, shop, use, freeze, looks, routing | yes |
| [General/level-editor.md](General/level-editor.md) | the level editor: designer guide, shortcuts, solver, generator | yes |
| [General/audio.md](General/audio.md) | sound effects: cues, the audio service, the clips and how they are made | yes |
| [General/debug-tools.md](General/debug-tools.md) | the debug window: win, fail, timer, coins, levels, saves | yes |
| [General/agent-capabilities.md](General/agent-capabilities.md) | what agents verified tools can and cannot do | no |

## QA checklists

| Page | Verifies |
| --- | --- |
| [testing/navigation-qa.md](testing/navigation-qa.md) | the boot from Splash to Home and the Home ⇄ Gameplay routes |
| [testing/ui-qa.md](testing/ui-qa.md) | safe area, button feedback, popups |
| [testing/home-qa.md](testing/home-qa.md) | the Home screen (stub) |
| [testing/settings-qa.md](testing/settings-qa.md) | the Settings popup (stub) |
| [testing/gameplay-qa.md](testing/gameplay-qa.md) | gameplay, timer, coins, popups, levels 1–5 (stub) |
| [testing/level-editor-qa.md](testing/level-editor-qa.md) | the level editor (stub) |
| [testing/lives-qa.md](testing/lives-qa.md) | lives, their refill and the lives popup |
| [testing/boosters-qa.md](testing/boosters-qa.md) | the four boosters, their stock, popup and looks |
| [testing/audio-qa.md](testing/audio-qa.md) | sound effects, the low-time tick and flash, the sound toggle |
| [testing/addressables-qa.md](testing/addressables-qa.md) | runtime asset loading through Addressables in the editor and on a device |
| [testing/network-qa.md](testing/network-qa.md) | the online requirement: connectivity, trusted time, no-internet popup (stub) |
