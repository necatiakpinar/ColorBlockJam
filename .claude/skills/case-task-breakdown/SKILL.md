---
name: case-task-breakdown
description: Turn the case brief (or any new feature request) into a scoped, ordered task list in Docs/Case/README.md with stable ids, acceptance criteria, dependencies, open questions and registered assumptions. Use when the case study brief arrives, when the brief changes, or when the user asks to plan, split or estimate work.
---

# Case Task Breakdown

Converts a brief into work a reviewer can follow commit by commit. The output is documentation only:
no code changes in this skill.

## Inputs

- The brief: a file the user gives (PDF, Markdown, text) or pasted text. Store the original, unedited,
  under `Docs/Case/brief/`.
- `CLAUDE.md`, `Docs/General/architecture.md`, `Docs/Case/README.md` (current tasks),
  `Docs/General/development-assumptions.md`.

## Workflow

1. **Read the whole brief twice.** First for the goal, second for every requirement, constraint,
   deliverable and deadline. Quote requirements exactly; never paraphrase a number or a rule.
2. **Extract requirements** into a numbered list `R-1..R-n`, each one testable sentence. Mark each as
   *must* (explicit in the brief), *should* (strongly implied) or *could* (polish).
3. **Find the gaps.** For every requirement ask: what happens on win, fail, restart, empty input, edge
   values, interruption (app background), first launch? Each unanswered point becomes either:
   - an **open question** (blocks implementation or changes architecture), or
   - a **`DA-n` assumption** (has a sensible default; record decision, reason, impact if reversed).
4. **Design the slices.** Split into tasks `T-01..T-n` that each:
   - deliver one observable outcome, small enough for one branch and 1–5 commits;
   - name their acceptance criteria as observable outcomes (what a player sees or a test asserts);
   - list the tests they add (Edit Mode for rules and data, Play Mode for scene flow);
   - name the docs they touch (new page under `Docs/General/` for a new subsystem).
   Order: core gameplay loop first (playable end to end, even ugly), then win/fail, then progression,
   then juice (animation, feedback, audio), then polish. Every task leaves `main` playable.
5. **Map to the architecture.** For each task, note the scope (Root / Gameplay), new services, signals,
   configs and views, following `architecture.md`. Flag anything that breaks a rule in `CLAUDE.md`.
6. **Write `Docs/Case/README.md`**: brief summary (§1), scope decisions (§2), open questions (§3),
   tasks table (§4). Register assumptions in `development-assumptions.md`. Add a QA checklist stub
   `Docs/testing/<area>-qa.md` per gameplay area.
7. **Report** to the user: the requirement list, the open questions (ask the blocking ones now), the
   task order with a one-line rationale, and the proposed first task. Do not start implementing until
   the user picks a task.

## Rules

- Every requirement maps to at least one task; every task maps back to at least one requirement.
- Never invent scope the brief does not ask for; extras go under *could* and are clearly optional.
- Task ids never change once written; a dropped task is marked `Dropped`, not deleted.
