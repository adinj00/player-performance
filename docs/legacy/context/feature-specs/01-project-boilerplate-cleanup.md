# Unit 01: Project Boilerplate Cleanup

## Goal

Clean the initial frontend and backend starter boilerplate so the repository has a minimal, buildable baseline for future feature implementation. Preserve the approved frontend foundation, including shadcn/ui setup, Tailwind CSS v4 theme tokens, the final `index.css` design system, and the frontend `@/` import alias.

## Design

This unit is a cleanup and baseline-preparation task only. It must not introduce real Player Performance Data System domain features, authentication screens, dashboards, navigation shells, API modules, database models, import flows, match workflows, player workflows, or role/permission behavior.

The frontend should remain visually minimal after cleanup. If any visible placeholder UI remains, it must use Bosnian Latin as the default visible language and stay neutral, for example a compact baseline message such as `Osnovna aplikacija je spremna.` Avoid adding long-lived product copy before localization infrastructure exists.

The application is light-only. Do not add dark mode classes, dark theme tokens, theme toggles, or dark-specific behavior. Do not replace the FK Velež red, white, and gold token direction with Vite, React, or generic starter styling.

Preserve the existing shadcn/ui foundation:

- Keep `frontend/components.json` or the project’s actual shadcn configuration file if it already exists.
- Keep generated shadcn/ui primitives in `frontend/src/components/ui/*` unchanged.
- Keep `frontend/src/lib/utils.ts` and its `cn()` helper if already present.
- Keep the final Tailwind CSS v4 and shadcn imports in `frontend/src/index.css` or the actual project global CSS entry file.
- Keep all approved CSS custom properties and semantic theme tokens in the global CSS token file.

All cleanup should reduce starter noise without changing architecture. The result should be a clean foundation that future specs can build on.

## Implementation

### Required reading

Before making changes, read the project entry instructions and context files in the required order:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/ui-context.md`
5. `context/code-standards.md`
6. `context/ai-workflow-rules.md`
7. `context/progress-tracker.md`

Use those files as the source of truth for project architecture, UI rules, code standards, workflow, and current progress.

### Frontend boilerplate cleanup

Inspect the `frontend/` project for default Vite and React starter files and remove only files that are unused starter artifacts.

Remove starter/demo UI such as:

- Vite logo usage.
- React logo usage.
- Counter/demo state examples.
- Starter links to Vite or React documentation.
- Default demo text such as “Click on the Vite and React logos” or “count is”.
- Starter CSS that exists only to style the default demo screen.

Remove unused starter assets when they are no longer imported, such as:

- `frontend/public/vite.svg`
- `frontend/src/assets/react.svg`
- other Vite/React demo images or starter-only files

Do not remove project foundation files that are still required by Vite, React, Tailwind, shadcn/ui, or the app bootstrap.

### Frontend baseline UI

After removing the demo UI, leave the app in a minimal buildable state.

If `frontend/src/App.tsx` or the current root component still renders visible content, replace the demo UI with a compact placeholder only. The placeholder must:

- use Bosnian Latin by default;
- avoid real domain workflows;
- avoid fake data;
- avoid navigation, dashboard cards, tables, forms, charts, auth screens, or feature shells;
- use semantic token utilities from the approved theme instead of raw Tailwind palette classes or hardcoded colors;
- remain simple enough to be replaced by the first real UI shell spec.

Acceptable placeholder direction:

- short title: `FK Velež Mostar`
- short supporting text: `Osnovna aplikacija je spremna.`

Do not add English placeholder copy unless language selection or localization infrastructure is implemented in a later spec.

### Preserve theme and shadcn/ui foundation

Do not regenerate or simplify `frontend/src/index.css` if it already contains the approved final theme tokens.

Confirm that the global CSS entry still includes the approved Tailwind CSS v4 and shadcn/ui setup, including any project-approved imports and CSS custom properties. Preserve the light-only FK Velež theme tokens, semantic color variables, typography variables, radius tokens, shadows, and chart tokens.

Do not introduce:

- raw palette utilities such as `bg-red-700`, `text-slate-500`, `border-gray-200`, or similar in application UI;
- hardcoded hex, RGB, HSL, or OKLCH values inside component files;
- `.dark` selectors or dark-mode token branches;
- ad-hoc visual overrides on generated shadcn/ui primitives.

Generated shadcn/ui primitive files in `frontend/src/components/ui/*` must remain unchanged unless the existing project is broken and the fix is explicitly limited to restoring the generated primitive state.

### Confirm frontend `@/` alias

Confirm the frontend source alias is configured and usable.

The expected alias behavior is:

- `@/*` resolves to `frontend/src/*`.
- TypeScript configuration supports the alias.
- Vite configuration supports the alias.
- Source imports should use `@/` for non-adjacent frontend imports instead of deep relative paths such as `../../../`.

If the alias is missing or incomplete, add the smallest configuration change needed to make it work in both TypeScript and Vite. Do not reorganize folders beyond what is required for the alias check.

### Backend starter cleanup

Inspect the `backend/` project only if it exists.

If backend starter/template code is present, remove only generic sample code that is not part of the approved architecture, such as:

- `WeatherForecast` demo endpoints, records, controllers, or generated sample responses;
- template “Hello World” endpoints that are not needed for health or baseline verification;
- unused sample classes created by the template;
- default demo Swagger/OpenAPI examples if they reference removed sample endpoints.

Keep the backend solution structure and project boundaries intact. Do not create domain entities, database models, authentication behavior, authorization policies, import modules, match modules, player modules, audit modules, or file-storage implementations in this unit.

If no backend starter/template code exists, leave the backend unchanged and note that there was nothing to clean.

### Documentation and scope control

Do not update architecture, UI, or code-standard context files unless cleanup reveals that the documented project state is inaccurate.

Do not add Git workflow instructions, branch names, commit commands, push commands, or implementation-session commands to this spec file.

After implementation, update `context/progress-tracker.md` only to reflect the actual Unit 01 implementation state and any verification result or blocker.

## Dependencies

None.

Do not install new npm, pnpm, yarn, NuGet, or tooling packages in this unit unless the existing project cannot build because a previously intended dependency is missing from lockfile/package references. If such a fix is required, keep it limited to restoring the existing baseline and document it in `context/progress-tracker.md`.

## Verification checklist

- [ ] Default Vite/React demo UI is removed.
- [ ] Unused starter assets such as Vite and React logos are removed when no longer imported.
- [ ] Frontend still has a minimal buildable root UI.
- [ ] Any remaining visible placeholder copy is minimal and Bosnian Latin by default.
- [ ] No real domain features, fake match/player data, auth flows, dashboards, API integrations, database work, imports, or report workflows are introduced.
- [ ] shadcn/ui configuration is preserved.
- [ ] Generated shadcn/ui primitives in `frontend/src/components/ui/*` are not modified for app-specific styling.
- [ ] `frontend/src/lib/utils.ts` and the `cn()` helper are preserved if already present.
- [ ] Final `frontend/src/index.css` theme tokens and Tailwind CSS v4/shadcn setup are preserved.
- [ ] No dark mode classes, `.dark` selectors, dark token branches, or theme toggles are introduced.
- [ ] No raw Tailwind palette classes or hardcoded colors are introduced in application UI components.
- [ ] Frontend `@/` alias is configured for both TypeScript and Vite.
- [ ] Non-adjacent frontend source imports use `@/` instead of deep relative paths where applicable.
- [ ] Backend starter/template code is cleaned only if present.
- [ ] Backend architecture boundaries remain unchanged.
- [ ] No new dependencies are installed for speculative future work.
- [ ] Frontend install state is valid using the project’s existing package manager and lockfile.
- [ ] Frontend build passes from `frontend/` using the configured build command.
- [ ] Frontend lint/typecheck passes if configured.
- [ ] Backend build passes from `backend/` using `dotnet build` if the backend project exists.
- [ ] Backend tests pass if test projects already exist and can be run without adding new test infrastructure.
- [ ] `context/progress-tracker.md` reflects the actual Unit 01 implementation result after implementation.
