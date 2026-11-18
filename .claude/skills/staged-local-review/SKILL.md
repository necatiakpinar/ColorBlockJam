---
name: staged-local-review
description: Run a bounded local review of the current diff with independent read-only reviewer subagents, fix every actionable finding, and re-review until clean. Run after simplify on every implementation or refactor, and whenever the user asks for a review before committing. Never commits or pushes.
---

# Staged Local Review

## Inputs

- **Target diff:** the working tree against `HEAD` by default; a branch against its base when asked.
- **Rules:** `CLAUDE.md`, the binding pages for the touched paths, the task in `Docs/Design/requirements.md`.

## Workflow

1. **Checks first.** Compile and run the Edit Mode suite (and Play Mode when scenes or flows changed).
   A failing check is a finding before any reviewer runs.
2. **Round 1 — two reviewers in parallel.** Spawn two read-only subagents. Give each the diff (C# in
   full, assets as a file list), the rules list and the rubric below. Tell them explicitly not to edit
   anything. Do not share your own conclusions with them.
3. **Consolidate.** Wait for both. Deduplicate. Classify each finding: *fix*, *false positive (reason)*,
   or *accepted risk* (only with the user's explicit approval). Verify no reviewer changed the tree
   (`git status` before and after).
4. **Fix** every *fix* finding in the main session, add or update tests, rerun the checks.
5. **Round 2 — one reviewer**, only if round 1 produced fixes, against the fixed state.
6. **Stop** when a round reports nothing actionable. Report findings, fixes and test totals, then hand
   over to `commit-plan`.

## Rubric

| Severity | Examples |
| --- | --- |
| **Blocking** | does not compile; failing test; data written inside a flow or after an animation; per-frame code; inline tween in a view or service; subscription outside construction; missing `.meta` |
| **Major** | behavior without a test; doc now wrong; magic number; missing cancellation; untraced gameplay decision (no task, no `DA-n`) |
| **Minor** | naming, a comment left in code, small duplication |

Every finding names `file:line`, the rule it breaks (`CLAUDE.md` section or page §) and a concrete fix.
