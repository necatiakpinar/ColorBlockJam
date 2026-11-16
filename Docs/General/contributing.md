# Contributing

> **Binding agent rules.** Read this page before proposing or making a commit.

## 1. Who commits

The developer commits. Agents prepare changes and propose a commit plan (`commit-plan` skill); they run
`git commit` / `git push` only when the developer explicitly asks in the current message.

## 2. Branches

- `main` is always green: compiles, all tests pass.
- Work happens on `feature/<task-id>-<slug>`, `fix/<slug>` or `chore/<slug>`, merged back to `main`
  with a merge commit (no squash) so the history shows each step.

## 3. Commit messages — Conventional Commits

```
<type>(<scope>): <imperative summary, lower case, no period, ≤ 72 chars>

<optional body: why, not what; wrapped at 72>

<optional footer: Refs: T-03, DA-2>
```

| Type | Use for |
| --- | --- |
| `feat` | player-visible or new capability |
| `fix` | a bug fix (comes with the test that reproduces it) |
| `refactor` | behavior-preserving restructuring |
| `perf` | measurable performance improvement |
| `test` | tests only |
| `docs` | documentation only |
| `build` | packages, assembly definitions, project settings |
| `ci` | CI configuration |
| `chore` | tooling, repository housekeeping |

Scopes in use: `core`, `boot`, `splash`, `gameplay`, `ui`, `di`, `signals`, `persistence`, `editor`,
`tests`, `agents`, `docs`, `deps`, `project`.

Examples:

```
build(deps): add VContainer and UniTask from OpenUPM
feat(splash): boot through weighted steps and hand over to gameplay
test(core): cover signal bus re-entrancy
docs(architecture): describe scope hierarchy and registration rules
```

## 4. What a commit contains

- One logical change. If the summary needs "and", split it.
- Code + its tests + the doc edits it makes necessary, together.
- Every `.meta` file for every added asset or folder.
- The project compiles and the suite passes at every commit.
- No generated files (`Library/`, `*.csproj`, `*.sln`, `Logs/`).

## 5. Never

- Squash, fixup or force-push published history.
- Mix formatting-only changes with behavior changes.
- Commit secrets, keystores or personal data.
