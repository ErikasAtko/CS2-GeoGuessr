# Contributing

How the team works on CS2-GeoGuessr. If something here changes, change it in a PR like any other code.

## Workflow

1. Pick up (or create) a GitHub issue for the task.
2. Create a branch off `main` (see [Branch naming](#branch-naming)).
3. Format your code before committing (see [Coding style](#coding-style)).
4. Open a pull request into `main` and request a review from at least one teammate.
5. The **author** merges, and only after every requested reviewer has submitted their review and the comments are resolved.

Nothing goes to `main` directly, not even a typo fix.

## Branch naming

Pattern: `[initials]/[issue-number]-[short-title]`

Example - Miron picks up issue #3, "Add scoring logic":

`mb/3-add-scoring-logic`

- Initials first (lowercase), separated from the rest by `/`.
- Issue number next, then a short kebab-case description of the task.

## Pull requests

- Keep PRs small and about one thing. Formatting sweeps, refactors and features go in separate PRs.
- The description says **what** changed, **why**, and **how to test it**.
- Reviewers comment on the actual code - logic, naming, edge cases - not just "LGTM". If a comment needs a change, the author pushes a fix to the same PR.

## Coding style

The rules are written down in config files, and tools apply them - not memory.

| Part | Rules live in | Fix everything | Check only (what CI runs) |
|------|---------------|----------------|-------------------------------|
| Backend (C#) | [`.editorconfig`](./.editorconfig) | `dotnet format` (repo root) | `dotnet format --verify-no-changes` |
| Frontend (TS/React/CSS) | [`frontend/.prettierrc.json`](./frontend/.prettierrc.json) | `npm run format` (in `frontend/`) | `npm run format:check` |
| Frontend lint | [`frontend/.oxlintrc.json`](./frontend/.oxlintrc.json) | - | `npm run lint` |

The [Code style](./.github/workflows/code-style.yml) GitHub Actions check runs the "check only" commands on every pull request and fails if anything is unformatted - fix it by running the formatter and pushing again. Line endings are normalized by [`.gitattributes`](./.gitattributes), so they don't depend on your git settings.

Run both formatters before every commit. Turn on format-on-save so you rarely have to:

- **Rider**: picks up `.editorconfig` automatically. For the frontend, enable Settings → Languages & Frameworks → JavaScript → Prettier → "Automatic Prettier configuration" + "Run on save".
- **VS Code**: install the *EditorConfig* and *Prettier* extensions and set `"editor.formatOnSave": true`.

### C# conventions

`.editorconfig` enforces these, and the IDE warns when they are broken:

- `PascalCase` for types, methods, properties, constants and public fields.
- `IPascalCase` for interfaces.
- `_camelCase` for private fields.
- `camelCase` for parameters and local variables.
- File-scoped namespaces (`namespace CS2_GEO.Game;`), `using` directives at the top.
- Braces on their own line (Allman style), 4-space indentation.
- One type per file, with the file named after the type (small related DTO records may share a file).
- `var` when the type is obvious; `int`/`string` rather than `Int32`/`String`.

### Frontend conventions

Prettier owns formatting (2-space indent, single quotes, no semicolons, 120-character lines). Beyond that:

- Components are `PascalCase` and live one per file in `src/components/` (`GameScreen.tsx`).
- Functions, variables and props are `camelCase`; types and interfaces are `PascalCase`.
- Types that mirror backend DTOs live in `src/types.ts` and keep the backend's names (`RoundDto`).
- All backend calls go through `src/api.ts`.

### General

- English everywhere: code, comments, commit messages.
- No commented-out code or unused `using`s / imports - git history keeps the old version.
- Commit messages are short imperative sentences: "Add 5-round game summary", not "added stuff".
