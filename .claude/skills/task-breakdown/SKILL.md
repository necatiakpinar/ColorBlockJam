---
name: task-breakdown
description: Turn a feature request or design change into a scoped, ordered task list in Docs/Design/requirements.md with stable ids, acceptance criteria, dependencies, open questions and registered assumptions. Use when a new feature or design change arrives, or when the user asks to plan, split or estimate work.
---

# Task Breakdown

Converts a feature request into work that can be followed commit by commit. The output is documentation
only: no code changes in this skill.

## Inputs

- The request: a file the user gives (Markdown, text) or pasted text.
- `CLAUDE.md`, `Docs/General/architecture.md`, `Docs/Design/requirements.md` (current requirements and tasks),
  `Docs/General/development-assumptions.md`.

## Workflow

1. **Read the whole request twice.** First for the goal, second for every requirement and constraint.
   Keep numbers and rules exact; never round or reinterpret them.
2. **Extract requirements** into the next free ids `R-n`, each one testable sentence. Mark each as
   *must* (explicitly requested), *should* (strongly implied) or *could* (polish).
3. **Find the gaps.** For every requirement ask: what happens on win, fail, restart, empty input, edge
   values, interruption (app background), first launch? Each unanswered point becomes either:
   - an **open question** (blocks implementation or changes architecture), or
   - a **`DA-n` assumption** (has a sensible default; record decision, reason, impact if reversed).
4. **Design the slices.** Split into tasks with the next free ids `T-n` that each:
   - deliver one observable outcome, small enough for one branch and 1–5 commits;
   - name their acceptance criteria as observable outcomes (what a player sees or a test asserts);
   - list the tests they add (Edit Mode for rules and data, Play Mode for scene flow);
   - name the docs they touch (new page under `Docs/General/` for a new subsystem).
   Order: core gameplay loop first (playable end to end, even ugly), then win/fail, then progression,
   then juice (animation, feedback, audio), then polish. Every task leaves `main` playable.
5. **Map to the architecture.** For each task, note the scope (Root / Gameplay), new services, signals,
   configs and views, following `architecture.md`. Flag anything that breaks a rule in `CLAUDE.md`.
6. **Update `Docs/Design/requirements.md`**: requirements (§1), scope decisions (§2), open questions (§3),
   tasks table (§4), and the task detail in `Docs/Design/tasks.md`. Register assumptions in
   `development-assumptions.md`. Add a QA checklist stub `Docs/testing/<area>-qa.md` per new area.
7. **Report** to the user: the requirement list, the open questions (ask the blocking ones now), the
   task order with a one-line rationale, and the proposed first task. Do not start implementing until
   the user picks a task.

## Rules

- Every requirement maps to at least one task; every task maps back to at least one requirement.
- Never invent scope the request does not ask for; extras go under *could* and are clearly optional.
- Requirement and task ids never change once written; a dropped one is marked `Dropped`, not deleted.
