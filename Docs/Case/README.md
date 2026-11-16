# Case

The design authority for gameplay. Filled by the `case-task-breakdown` skill when the brief arrives.

## 1. Brief

_Not received yet._ When it arrives, store the original under `Docs/Case/brief/` (PDF or Markdown)
unchanged and summarize it here: goal, required features, constraints, deliverables, deadline.

## 2. Scope decisions

What is in, what is out, and why. Every unclear point becomes a question below or a `DA-n` in
[development-assumptions.md](../General/development-assumptions.md).

## 3. Open questions for the reviewer

| # | Question | Blocking | Answer |
| --- | --- | --- | --- |

## 4. Tasks

Ids are stable (`T-01`, `T-02`, …); they appear in branch names and commit footers (`Refs: T-03`).

| Id | Task | Acceptance criteria | Depends on | Status |
| --- | --- | --- | --- | --- |
| T-00 | Skeleton: DI, boot Splash → Gameplay, tests, docs, agent rules | Play from any scene reaches Gameplay showing "Level N"; all tests green | — | Done |

Status values: `Todo`, `In progress`, `Blocked`, `Done`.
