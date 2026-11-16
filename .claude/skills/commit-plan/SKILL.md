---
name: commit-plan
description: Split the current uncommitted work into atomic, reviewable commits with Conventional Commit messages and present the plan for approval. Run at the end of every task, or when the user asks how to commit, to write commit messages, or to commit. Executes git commit only after the user explicitly approves in the current message; never pushes unless asked.
---

# Commit Plan

Reviewers read the history; each commit should tell one small, true story.

## 1. Read

`Docs/General/contributing.md` (types, scopes, format) and `git log --oneline -20` to match the style
already in the history.

## 2. Inventory

```bash
git status --porcelain
git diff --stat
git diff --cached --stat
```

List every changed path, including `.meta` files and untracked files.

## 3. Group

- One logical change per commit, ordered so **each commit compiles and passes tests on its own**:
  build/deps → core infrastructure → features (code + tests + docs together) → docs-only → tooling.
- A `.meta` file always travels with its asset; a folder `.meta` with the first commit that adds a file
  in it.
- Tests go with the code they test, unless the commit is a pure `test:` addition.
- Never mix reformatting with behavior.

## 4. Write messages

`type(scope): imperative summary` — lower case, no period, at most 72 characters. Add a body when the
*why* is not obvious. Footer `Refs: T-xx` / `DA-x` when a task or assumption applies.

## 5. Present

Show a numbered table: message, files (or globs), and why it is separate. Ask for approval. Point out
anything that should not be committed (generated files, secrets, stray assets).

## 6. Execute — only on explicit approval

For each approved commit, stage exactly its paths (`git add -- <paths>`), verify with
`git diff --cached --stat`, then commit. Never use `git add -A` blindly, `--amend` on pushed commits,
`--no-verify`, squash or force push. After the last commit, show `git log --oneline` for the new range.
