# Doc Authoring Rules

> **Binding agent rules.** Read this page before writing or editing `CLAUDE.md`, `AGENTS.md`, a page
> under `Docs/`, a `README.md` or a skill.

## 1. What each kind of doc holds

| Doc | Holds | Never holds |
| --- | --- | --- |
| `CLAUDE.md` / `AGENTS.md` | rules that bind every change; "Read X before touching Y" triggers | subsystem detail |
| `Docs/General/*.md` | how an area works and its binding rules | task status |
| `Docs/Design/*.md` | requirements, tasks, scope decisions, progress | implementation detail |
| `Docs/testing/*-qa.md` | observable test cases for an area | code paths |
| `.claude/skills/*/SKILL.md` | a repeatable workflow | project facts (link to the page instead) |

## 2. Writing facts that stay true

- State the current truth in the present tense. History belongs in git, not in docs.
- Name a type, file or test only when the reader needs it to act; a renamed type must be renamed in
  every doc in the same change.
- One fact lives in one place. Elsewhere, link to it.
- Prefer a table or a numbered rule over prose.

## 3. The blast-radius sweep

Before committing a change to a documented fact, search for every mention and update it:

```bash
git grep -n "<TypeName>\|<file-name>\|<old wording>" -- '*.md'
```

Also check `Docs/index.md` (new page listed?), `CLAUDE.md` (trigger still right?) and the area's QA
checklist.

## 4. Style

- English, short sentences, lines near 105 characters.
- Headings numbered in binding pages so rules can be cited (`architecture.md §2`).
- No emojis except the QA markers `⚙` (automated) and `👁` (manual).
