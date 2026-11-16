---
name: simplify
description: Review the changed Unity C# code in this repository for reuse, rule compliance, efficiency and doc sync, then fix what is found. Run at the end of every implementation or refactor, or when the user asks to clean up, simplify or refactor recent changes.
---

# Simplify

A cleanup pass over the current change set. It fixes problems; it does not just list them.

## 1. Read the rules

`CLAUDE.md`, plus every page it links with a "Read X before touching Y" trigger for the touched paths
(`architecture.md`, `splash-boot.md`, `testing.md`, `doc-authoring-rules.md` when docs changed).

## 2. Collect the change set

```bash
git status --porcelain              # new files, missing .meta files
git diff --stat
git diff -- '*.cs'
git diff --cached -- '*.cs'
```

Review C# in full. For `.unity` / `.prefab` / `.asset` review the file list and only targeted
serialized values (a config number, a reference). Never line-review YAML.

## 3. Passes

### Pass 1 — Reuse

- A new helper, fake or animation that duplicates an existing one (`Tests/EditMode/Helpers`,
  `Common/Animations`, `SignalBus`, `IKeyValueStorage`).
- A literal tunable that belongs in a `<Area>Config` asset.
- A new signal that duplicates an existing one, or a signal where a direct call to the one owner is
  simpler.

### Pass 2 — Rules (each is a bug to fix now)

- Per-frame code (`Update`, `LateUpdate`, `FixedUpdate`, `ITickable`) without a justified comment.
- Inline `DO*` / `DOTween.*` outside an `IAnimation<T>` struct; data writes gated on a tween or flow.
- Subscriptions outside a constructor / `[Inject] Construct`; missing unsubscribe.
- Null checks on serialized references in components; `Camera.main`; `Find*`; static runtime logic.
- Flows (`*Flow`) mutating data or deciding domain rules.
- Entry points registered inside an `IInstaller` (they belong in the scope).
- Raw `Debug.Log`; magic numbers; strings used as state; enums without explicit spaced values.
- UI built in code instead of authored.
- Naming: `RollicCase.*` namespaces matching folders, `_camelCase` fields, test names
  `Subject_ShouldOutcome_WhenContext`.

### Pass 3 — Efficiency

- Hot-path allocations: LINQ, `foreach` over `List`/`Dictionary`, string building, closures, `new`.
- `GetComponent` in callbacks; `Instantiate`/`Destroy` churn instead of pooling.
- Async without a `CancellationToken`.

### Pass 4 — Docs and tests

- Every behavior change has a test; every fixed bug has a reproducing test.
- Docs that mention a changed fact are updated (`doc-authoring-rules.md` §3 sweep).
- `AGENTS.md` is byte-identical to `CLAUDE.md` (`cmp CLAUDE.md AGENTS.md`).

## 4. Fix and verify

Apply the fixes, then compile and run the Edit Mode suite (Unity MCP `run_tests` or the batch command
in `testing.md`). Report what was fixed, what was skipped and why, and the test totals.
