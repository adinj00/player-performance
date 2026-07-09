# Unit 11: Frontend Form and Validation Foundation

## Goal

Add the frontend form and validation foundation used by future data-entry workflows. This unit introduces React Hook Form + Zod conventions, shared form error helpers, and reusable non-domain form UI patterns without implementing any real product form, auth form, settings form, player form, match form, or API mutation.

## Design

The form foundation must follow the existing light-only FK Velež UI direction and shadcn/ui component rules.

User-facing helper copy, validation examples, fallback labels, and inline messages must use Bosnian Latin by default. Keep any visible or example copy minimal and operational.

Forms in future units should be built from these conventions:

- Use React Hook Form for form state.
- Use Zod for frontend schemas.
- Use `@hookform/resolvers/zod` to connect Zod schemas to React Hook Form.
- Keep schemas close to the feature that owns the form when the form belongs to a real feature.
- Use shared helpers only for cross-feature patterns such as error extraction, form-level error summaries, and field message rendering.
- Backend validation remains authoritative. Frontend validation improves UX only.
- Do not duplicate server-owned data in local form state longer than needed.
- Use existing shadcn/ui primitives and variants; do not apply ad-hoc visual color overrides.
- Use semantic tokens through existing Tailwind utilities; do not add raw Tailwind palette classes or hardcoded colors.
- Keep the foundation compatible with future localization. Do not create a full i18n system in this unit.

Scope limits:

- Do not implement real domain forms.
- Do not implement authentication forms.
- Do not add API mutations.
- Do not add backend validation.
- Do not add localization infrastructure.
- Do not add form persistence.
- Do not add optimistic updates.
- Do not add route-specific business behavior.
- Do not create fake players, matches, users, seasons, teams, imports, reports, or other domain data.

## Implementation

### Required reading

Before implementation, read:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/ui-context.md`
5. `context/code-standards.md`
6. `context/ai-workflow-rules.md`
7. `context/progress-tracker.md`
8. `context/feature-specs/11-frontend-form-validation-foundation.md`

### Package setup

In the frontend project, install the form validation dependencies if they are not already installed:

- `react-hook-form`
- `zod`
- `@hookform/resolvers`

Do not install additional form libraries, UI kits, state libraries, localization packages, or validation packages in this unit.

If the shadcn/ui form primitive is not already present and the project is using shadcn form conventions, add it through the shadcn CLI. Do not manually rewrite generated shadcn/ui primitive files after installation.

### Shared form utility helpers

Create shared form helpers under `frontend/src/lib` or another existing shared frontend location that matches the current project structure.

Add utilities for:

- extracting readable field error text from React Hook Form field errors;
- normalizing a form-level error string from unknown caught values when future mutations fail;
- providing a safe default Bosnian Latin fallback message such as `Došlo je do greške. Pokušajte ponovo.`;
- keeping helper input types explicit and avoiding `any`.

These helpers must remain generic. They must not reference project domain entities such as players, matches, reports, imports, medical records, teams, or users.

### Shared form UI components

Create reusable app-level form UI components under `frontend/src/components/common` or the existing shared component folder used by previous units.

At minimum, add:

#### `FormErrorSummary`

A compact component for form-level errors.

Requirements:

- Accepts a string or list of strings.
- Renders nothing when there are no errors.
- Uses semantic token utilities and existing spacing/radius conventions.
- Uses Bosnian Latin fallback copy if a generic message is needed.
- Does not use raw palette classes or hardcoded colors.
- Does not depend on a domain feature.

#### `FormActions`

A small layout component for form action buttons.

Requirements:

- Supports primary and secondary action placement through children.
- Keeps button alignment consistent across dialogs and pages.
- Works in narrow and desktop layouts.
- Does not create custom button colors or visual variants.
- Uses existing shadcn/ui button variants through child buttons.

#### `RequiredFieldIndicator` or equivalent pattern

Add a small, accessible required-field indicator pattern if the current shadcn form setup does not already provide a clear convention.

Requirements:

- Does not rely only on color.
- Keeps required indication minimal.
- Does not hardcode raw colors.
- Can be reused in future forms.

### Non-domain validation example

Add a small non-domain validation example only if it is useful to verify the convention and TypeScript integration.

If added, it must:

- live in a clearly internal/example location such as `frontend/src/components/common/form-validation-example.tsx` or another non-routed location;
- not be linked from sidebar navigation;
- not introduce business entities, fake project data, players, matches, users, teams, reports, imports, medical records, or settings;
- use a generic field such as `Naziv` and a generic submit action that does not call an API;
- demonstrate Zod schema typing, `zodResolver`, field-level error display, and form-level error summary;
- remain small enough that future feature forms can copy the convention without copying product behavior.

If the convention can be verified without an example component, do not add a visible or exported example.

### Form conventions documentation

Add a short frontend form convention note in the most appropriate existing documentation location if one already exists for frontend conventions. If no appropriate local file exists, keep the convention documented in code comments near the shared helpers and do not create broad new documentation.

The convention must state:

- feature-owned schemas live near the feature form;
- shared helpers stay generic;
- backend validation remains authoritative;
- user-facing validation messages are Bosnian Latin by default until localization infrastructure exists;
- future localized strings should move into translation resources after the localization foundation is implemented.

### Integration with existing app

Do not change routing, sidebar navigation, topbar behavior, auth behavior, backend behavior, API client behavior, or existing placeholder page content unless a minimal import/path update is required by the new shared components.

The app should continue to build and behave the same from a user perspective after this unit, except that the shared form foundation exists for future feature units.

### Progress tracker update

Update `context/progress-tracker.md` after implementation to reflect that Unit 11 was implemented.

The update should include:

- Unit 11 added to completed work;
- the form/validation dependencies added;
- any relevant note about shared form helpers or UI components;
- any verification command that failed and why, if applicable.

## Dependencies

Install in `frontend/` if not already present:

- `react-hook-form` — frontend form state management.
- `zod` — frontend schema validation and typed form values.
- `@hookform/resolvers` — connects Zod schemas to React Hook Form.

Add the shadcn/ui `form` primitive through the shadcn CLI only if the project does not already have it and the implementation uses shadcn form conventions.

No backend dependencies are required.

## Verification checklist

- [ ] `react-hook-form`, `zod`, and `@hookform/resolvers` are installed in the frontend project if they were not already present.
- [ ] No extra speculative frontend or backend packages were installed.
- [ ] Shared form helpers exist and are generic.
- [ ] Shared form UI components exist and do not depend on domain features.
- [ ] Any non-domain validation example is not routed, not linked in navigation, and does not create fake domain data.
- [ ] User-facing helper/example text is Bosnian Latin by default.
- [ ] No real auth, user, player, team, match, report, import, medical, dashboard, or settings form was implemented.
- [ ] No API mutations or backend endpoints were added.
- [ ] Generated shadcn/ui primitives were not manually modified after installation.
- [ ] No raw Tailwind palette classes or hardcoded colors were introduced in app UI.
- [ ] Frontend source imports use the configured `@/` alias where appropriate.
- [ ] `context/progress-tracker.md` reflects the completed unit after implementation.
- [ ] Frontend formatting was applied where needed.
- [ ] `npm run format:check` passes in the frontend project.
- [ ] `npm run lint` passes in the frontend project if configured.
- [ ] `npm run build` passes in the frontend project.
- [ ] `dotnet build` passes for the backend solution if backend files are affected.
- [ ] `dotnet test` passes for the backend solution if backend tests exist and backend files are affected.
