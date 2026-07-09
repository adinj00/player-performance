# Unit 05: Common UI Primitives Foundation

## Goal

Create a small set of reusable app-level UI primitives for repeated page structure, empty/loading/error states, and neutral workflow surfaces. This unit should reduce repeated placeholder markup across the routed frontend screens without introducing real domain features, API calls, authentication, backend logic, or mock business data.

## Design

The UI must stay aligned with the light-only FK Velež Mostar visual system documented in `context/ui-context.md`.

Use semantic Tailwind utilities backed by `frontend/src/index.css` tokens only. Do not introduce raw Tailwind palette classes such as `bg-red-*`, `text-yellow-*`, `border-gray-*`, `bg-zinc-*`, or hardcoded color values in component files.

All visible fallback and placeholder copy must be Bosnian Latin by default. Keep copy short, operational, and reusable. Avoid mixing English and Bosnian in the same visible screen unless the text is a technical/domain term or internal identifier.

This unit should create app-level primitives outside generated shadcn/ui files. Generated shadcn/ui primitive files in `frontend/src/components/ui/*` must not be modified unless a required primitive was already generated incorrectly and the active implementation cannot compile without the correction.

The resulting components should feel like building blocks for a professional sports performance dashboard, but they must remain generic. Do not add real matches, players, reports, imports, users, teams, statistics, charts, or fake business datasets.

The existing app shell and route placeholders from earlier units should be updated only enough to use these shared primitives consistently.

## Implementation

### Required reading

Before implementing, read:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/ui-context.md`
5. `context/code-standards.md`
6. `context/ai-workflow-rules.md`
7. `context/progress-tracker.md`
8. `context/feature-specs/05-common-ui-primitives-foundation.md`

Implement only this unit.

### Common component folder

Create or use the existing app-level common component folder:

```txt
frontend/src/components/common/
```

Keep generated shadcn/ui primitives in:

```txt
frontend/src/components/ui/
```

Use `@/` imports for source imports. Avoid deep relative imports such as `../../../`.

### Page header component

Create:

```txt
frontend/src/components/common/page-header.tsx
```

The component should support:

- required `title`
- optional `description`
- optional `eyebrow`
- optional `actions` slot

Behavior and styling:

- Use semantic layout spacing suitable for dashboard pages.
- Keep text hierarchy clear and compact.
- Do not hardcode colors.
- Do not include route-specific or domain-specific copy inside the component.
- Allow route pages to pass Bosnian Latin text as props.

### Empty state component

Create:

```txt
frontend/src/components/common/empty-state.tsx
```

The component should support:

- optional icon slot
- required `title`
- optional `description`
- optional action slot

Behavior and styling:

- Center content inside a contained area.
- Use card/surface styling through semantic tokens.
- Keep spacing readable on desktop, tablet, and mobile.
- Do not assume a specific module such as matches, players, imports, or users.

Recommended default copy may be generic Bosnian Latin when the caller does not provide text:

- title: `Nema podataka`
- description: `Sadržaj će biti prikazan kada bude dostupan.`

Only add defaults if the component API remains explicit and easy to override.

### Loading state component

Create:

```txt
frontend/src/components/common/loading-state.tsx
```

The component should support:

- optional `label`
- compact centered loading presentation

Behavior and styling:

- Use a lightweight loading indicator or simple text state.
- Default visible label, if used, should be Bosnian Latin: `Učitavanje...`
- Do not introduce a new spinner package.
- Keep the component accessible with readable loading text.

### Error state component

Create:

```txt
frontend/src/components/common/error-state.tsx
```

The component should support:

- required or default `title`
- optional `description`
- optional action slot

Behavior and styling:

- Use destructive/error semantic tokens only where supported by the theme.
- Default visible title, if used, should be Bosnian Latin: `Došlo je do greške`
- Do not leak technical exception details.
- Do not implement API retry behavior yet; only provide an action slot for future usage.

### Content section component

Create:

```txt
frontend/src/components/common/content-section.tsx
```

The component should support:

- optional `title`
- optional `description`
- optional `actions` slot
- children content

Behavior and styling:

- Provide a consistent bordered/card-like section for future dense dashboard content.
- Use semantic tokens and shadcn-compatible layout.
- Do not hardcode visual values.
- Keep the component generic and domain-agnostic.

### Route placeholder cleanup

Update existing routed placeholder pages from Unit 03 to use the new common components where it reduces duplication.

Requirements:

- Keep route placeholder copy minimal and Bosnian Latin.
- Do not introduce real module content.
- Do not add fake data rows, fake statistics, fake players, fake matches, fake imports, fake users, or dashboard metrics.
- Do not add API calls, TanStack Query, forms, validation schemas, auth checks, or backend integration in this unit.
- Preserve the existing app shell and routing behavior.

### Barrel exports

If the project already uses component barrel exports, add exports in the same style.

If the project does not use barrel exports yet, do not introduce a broad export pattern only for this unit unless it clearly improves imports without creating circular dependency risk.

### Progress tracker

During implementation, update `context/progress-tracker.md` to reflect that Unit 05 is in progress and then completed after verification passes.

Do not change architecture, UI system, or code standards unless implementation reveals a real mismatch in the context files.

## Dependencies

None.

Use existing React, Vite, Tailwind CSS v4, shadcn/ui setup, Lucide icons if already available, and the formatting tooling introduced in Unit 04.

Do not install new npm packages for this unit.

## Verification checklist

- [ ] `AGENTS.md` and all required context files were read before implementation.
- [ ] `frontend/src/components/common/page-header.tsx` exists and is generic.
- [ ] `frontend/src/components/common/empty-state.tsx` exists and is generic.
- [ ] `frontend/src/components/common/loading-state.tsx` exists and is generic.
- [ ] `frontend/src/components/common/error-state.tsx` exists and is generic.
- [ ] `frontend/src/components/common/content-section.tsx` exists and is generic.
- [ ] Existing route placeholder pages use the shared primitives where appropriate.
- [ ] No real domain features, API calls, auth logic, backend logic, fake business datasets, or production workflows were added.
- [ ] User-facing placeholder/fallback copy is Bosnian Latin by default.
- [ ] No generated shadcn/ui primitive files were modified unnecessarily.
- [ ] Frontend source imports use the `@/` alias instead of deep relative imports.
- [ ] No raw Tailwind palette classes or hardcoded colors were introduced in app UI components.
- [ ] From `frontend/`, `npm run format` completes successfully.
- [ ] From `frontend/`, `npm run format:check` passes.
- [ ] From `frontend/`, `npm run lint` passes if the script exists.
- [ ] From `frontend/`, `npm run build` passes.
- [ ] Backend files were not changed. If backend files were changed unexpectedly, run the backend build and document why the backend was touched.
- [ ] `context/progress-tracker.md` reflects the completed Unit 05 implementation after verification.
