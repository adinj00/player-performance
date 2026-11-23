# Unit 04: Code Formatting Foundation

## Goal

Add a consistent frontend formatting foundation using Prettier, `prettier-plugin-tailwindcss`, and `eslint-config-prettier` so future feature work produces smaller, cleaner, and more reviewable diffs. This unit configures code formatting and lint compatibility only; it must not implement product features, visual redesigns, backend behavior, or broad lint rule changes.

## Design

This is a tooling-only foundation unit. The visible application UI should not change except for formatting-related whitespace or import/class ordering that does not affect behavior.

Prettier should become the source of truth for formatting frontend TypeScript, React, CSS, JSON, and Markdown files where appropriate. ESLint should remain responsible for code quality rules, while `eslint-config-prettier` disables ESLint rules that conflict with Prettier formatting.

Tailwind utility ordering should be handled by `prettier-plugin-tailwindcss` so class lists stay consistent as the light-only FK Velež UI grows. This supports the existing requirement to use semantic token utilities and avoid raw Tailwind palette classes, but it does not replace those design-system rules.

Keep formatting configuration boring and conventional. Do not introduce custom style debates or large preference changes unless they are already established in the project. Do not add Biome, Rome, dprint, Stylelint, Husky, lint-staged, commit hooks, or CI automation in this unit.

Visible UI copy remains Bosnian Latin by default. Internal package scripts, config names, filenames, and technical identifiers remain English.

## Implementation

### Required reading

Before implementation, read the project entry instructions and context files in the required order:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/ui-context.md`
5. `context/code-standards.md`
6. `context/ai-workflow-rules.md`
7. `context/progress-tracker.md`

Then read this feature spec and implement only what is defined here.

### Scope boundaries

This is a frontend tooling and repository hygiene unit.

Do not implement:

- authentication, authorization, sessions, or user flows;
- backend API endpoints or backend architecture changes;
- database setup, migrations, seed data, or domain models;
- TanStack Query, Zustand, nuqs, i18next, forms, tables, charts, imports, dashboards, players, matches, reports, media, medical, audit, settings, or business workflows;
- new UI screens, visual redesigns, theme changes, dark mode, or new design tokens;
- fake data, demo data, or placeholder domain behavior;
- CI pipelines, pre-commit hooks, lint-staged, Husky, or Git workflow automation;
- broad ESLint rule redesigns unrelated to Prettier compatibility.

If the project already has partial Prettier or formatting configuration, extend and normalize the existing setup instead of creating competing duplicate configuration files.

### Frontend dependency installation

Install the requested packages as frontend development dependencies in `frontend/package.json`:

```txt
prettier
prettier-plugin-tailwindcss
eslint-config-prettier
```

Use the package manager already established by the frontend project. Do not switch package managers or create a second lockfile.

Do not install additional formatter, linter, hook, or CI packages in this unit.

### Prettier configuration

Add a single Prettier configuration for the frontend unless one already exists.

Preferred location:

```txt
frontend/.prettierrc
```

Requirements:

- enable `prettier-plugin-tailwindcss` through the Prettier config;
- keep configuration minimal and conventional;
- do not add project-specific formatting opinions that are not required for consistency;
- ensure Tailwind classes are sorted automatically by Prettier;
- do not hardcode or change design tokens in the Prettier config.

A minimal configuration is acceptable. The implementation agent may choose JSON, JavaScript, or TypeScript config format based on what is most compatible with the existing frontend toolchain, but there must be only one active Prettier config for the frontend.

### Prettier ignore file

Add or update:

```txt
frontend/.prettierignore
```

The ignore file should exclude generated outputs, installed dependencies, local environment files, coverage output, and build artifacts.

At minimum, ignore:

```txt
node_modules
dist
build
coverage
.env
.env.*
```

Do not ignore normal source files that should be formatted, including app components, feature components, shared utilities, CSS token files, Markdown specs, and JSON config files.

Do not ignore generated shadcn/ui primitive files solely to avoid formatting them. They may be formatted by Prettier, but do not manually modify their behavior or styling.

### Frontend package scripts

Update `frontend/package.json` scripts without removing existing scripts.

Add scripts equivalent to:

```json
{
  "format": "prettier . --write",
  "format:check": "prettier . --check"
}
```

Keep existing `build`, `lint`, `dev`, `preview`, or test scripts intact.

Do not make `build` depend on `format` in this unit. Formatting should be runnable as an explicit check without changing the build pipeline.

### ESLint compatibility

Update the existing frontend ESLint configuration so Prettier and ESLint do not fight each other.

Requirements:

- use `eslint-config-prettier` according to the existing ESLint config style;
- preserve existing project ESLint rules and plugins unless they directly conflict with Prettier;
- do not introduce new style rules that duplicate Prettier responsibilities;
- do not switch ESLint major configuration style unless the existing setup already requires it;
- do not add TypeScript, React, accessibility, import, or testing rules unrelated to this Prettier compatibility unit.

If the project uses ESLint flat config, add the Prettier config in the recommended flat-config position. If it uses legacy `.eslintrc` style, extend `prettier` in the compatible legacy way.

If the frontend currently has no ESLint setup at all, do not create a full new ESLint stack in this unit. Add the Prettier setup and document in `context/progress-tracker.md` that ESLint compatibility was skipped because no ESLint config exists yet.

### Formatting pass

Run the formatter once after configuration is added.

Requirements:

- format frontend source and config files according to the new Prettier setup;
- accept class ordering changes produced by `prettier-plugin-tailwindcss`;
- review generated diffs to ensure there are no behavior changes;
- do not manually rewrite components beyond formatting changes required by the tool.

If formatting reveals existing syntax or parser errors, fix the root cause only if it is clearly part of starter/tooling cleanup. Do not use formatting errors as a reason to refactor product code beyond this spec.

### Documentation and progress tracker

Update `context/progress-tracker.md` after implementation to record that Unit 04 configured frontend formatting.

The progress note should mention:

- Prettier was added for frontend formatting;
- Tailwind class sorting was added through `prettier-plugin-tailwindcss`;
- ESLint compatibility was configured with `eslint-config-prettier`, or explicitly note if skipped because no ESLint config exists;
- verification commands run and whether they passed.

Do not change architecture, UI, or code-standards context files unless implementation reveals a real context-level rule that must be updated. Do not add Git commands or branch workflow notes to any spec or context file.

## Dependencies

Frontend development dependencies:

- `prettier` — code formatter for frontend source, config, CSS, JSON, and Markdown files.
- `prettier-plugin-tailwindcss` — automatic Tailwind utility class sorting, including semantic token utilities.
- `eslint-config-prettier` — disables ESLint formatting rules that conflict with Prettier.

No backend dependencies are required in this unit.

## Verification checklist

- [ ] `frontend/package.json` includes `prettier`, `prettier-plugin-tailwindcss`, and `eslint-config-prettier` as development dependencies.
- [ ] The frontend uses one active Prettier config.
- [ ] Prettier config enables `prettier-plugin-tailwindcss`.
- [ ] `frontend/.prettierignore` excludes dependencies, build outputs, coverage output, and local environment files.
- [ ] `frontend/package.json` includes `format` and `format:check` scripts without removing existing scripts.
- [ ] Existing frontend ESLint configuration is compatible with Prettier through `eslint-config-prettier`, or `context/progress-tracker.md` clearly documents why this was skipped.
- [ ] Running `npm run format` from `frontend/` completes successfully.
- [ ] Running `npm run format:check` from `frontend/` completes successfully after formatting.
- [ ] Running the existing frontend lint command passes if a lint script exists.
- [ ] Running the existing frontend build command passes.
- [ ] Running the existing backend build command passes if a backend solution exists.
- [ ] Formatting changes do not introduce product behavior changes, route changes, UI redesigns, dark mode, fake data, or new domain features.
- [ ] No generated shadcn/ui primitive behavior or styling is manually modified.
- [ ] No speculative dependencies, hook tooling, CI workflow, or package manager changes are introduced.
- [ ] `context/progress-tracker.md` is updated with actual implementation and verification results.
