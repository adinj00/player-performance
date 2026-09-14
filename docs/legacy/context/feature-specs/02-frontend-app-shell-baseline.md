# Unit 02: Frontend App Shell Baseline

## Goal

Build the first reusable frontend application shell for the Player Performance Data System: a light-only FK Velež dashboard frame with a sidebar, top bar, and minimal placeholder main content. This unit creates layout structure only and must not introduce authentication, backend data, routing flows, domain screens, fake statistics, or real feature behavior.

## Design

The shell must follow the approved light-only FK Velež Mostar visual identity: red, white, and gold through semantic theme tokens, clean dashboard structure, dense-but-readable spacing, and no dark mode behavior.

Visible UI copy must be Bosnian Latin by default. Keep copy short, operational, and easy to replace later when localization infrastructure is implemented. Do not mix Bosnian and English in visible UI text except for accepted domain acronyms such as GPS.

The shell should provide the first visual frame for future staff workflows:

- left sidebar for primary navigation;
- top bar for page context and future high-level actions;
- main content area with a minimal placeholder state;
- responsive behavior that works on desktop, tablet, and mobile.

This unit is intentionally static. Navigation items may be displayed, but they must not imply implemented workflows. If routing is not already configured, do not add a routing package in this unit. Keep navigation items as non-destructive static UI controls/placeholders and leave real route behavior for a later dedicated routing or feature-screen spec.

Use semantic token utilities from `frontend/src/index.css`. Do not use raw Tailwind palette classes such as `bg-red-700`, `text-slate-500`, `border-gray-200`, or hardcoded hex/RGB/HSL/OKLCH colors in component files. Do not add `.dark` selectors, theme toggles, dark token branches, or dark-specific classes.

Use shadcn/ui primitives only through their existing generated files and documented variants. Do not modify generated files in `frontend/src/components/ui/*` for app-specific styling. Layout-only utility classes are allowed for spacing, width, grid, flex, alignment, overflow, and responsive behavior.

The shell should be professional and restrained. Do not create KPI cards, charts, tables, forms, dashboards, match lists, player lists, import previews, medical panels, audit views, or fake sample data in this unit.

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

This is a frontend-only layout foundation unit.

Do not implement:

- authentication, login, logout, user sessions, or permission checks;
- backend API calls;
- TanStack Query server-state setup;
- React Router or another routing dependency unless routing already exists in the project;
- dashboard metrics, tables, charts, forms, imports, match workflows, player workflows, medical workflows, audit workflows, or settings behavior;
- fake player, match, team, GPS, medical, or report data;
- role-aware navigation filtering;
- language switching or translation resources;
- persistent sidebar preferences;
- Zustand state stores;
- backend code changes.

If an existing project file already has a compatible app-shell or routing foundation, extend it minimally to match this spec instead of duplicating competing shell structures.

### Frontend file structure

Add or update frontend files using the documented frontend boundaries.

Preferred structure:

```txt
frontend/src/
├── app/
│   └── app.tsx
├── components/
│   ├── layout/
│   │   ├── app-shell.tsx
│   │   ├── app-sidebar.tsx
│   │   └── app-topbar.tsx
│   └── common/
│       └── app-empty-state.tsx
└── App.tsx
```

Use this structure unless the existing project has an equivalent established pattern. If `frontend/src/App.tsx` is still the Vite root component, keep it minimal and have it render the app composition from `@/app/app`.

Use `@/` imports for non-adjacent frontend source imports. Avoid deep relative imports such as `../../../`.

Keep component names, filenames, route-like identifiers, and TypeScript identifiers in English. Keep user-facing copy in Bosnian Latin.

### App composition

Create `frontend/src/app/app.tsx` as the top-level frontend composition for this unit.

It should render the app shell and pass the minimal page content into it.

Do not create provider abstractions yet unless the existing app already has a provider composition file. TanStack Query, auth/session providers, localization providers, and router providers should be introduced in the unit that actually needs them.

### App shell component

Create `frontend/src/components/layout/app-shell.tsx`.

Requirements:

- full viewport minimum height;
- light-only background using semantic tokens;
- left sidebar on desktop;
- top bar above the main content area;
- main content region that can host future pages;
- responsive layout for smaller screens;
- local component state only for mobile sidebar open/close behavior if needed;
- no global store for sidebar state in this unit.

The app shell should accept children or a named content slot so future page components can render inside the main content area.

Do not hardcode domain data into the shell. The shell may define static navigation metadata for future modules, but those items are labels only and should not carry fake counts, fake statuses, or real data.

### Sidebar component

Create `frontend/src/components/layout/app-sidebar.tsx`.

The sidebar should include:

- compact FK Velež Mostar app identity area;
- grouped navigation placeholders based on the approved UI direction;
- active state for `Kontrolna ploča` only;
- sidebar footer area reserved for a future user/account action, using minimal placeholder copy only if needed.

Recommended Bosnian Latin visible labels:

```txt
Pregled
- Kontrolna ploča

Performanse
- Utakmice
- Igrači
- Trening GPS
- Importi

Klub
- Timovi / Selekcije
- Medicinski status
- Medijska biblioteka

Administracija
- Korisnici i uloge
- Postavke
```

These labels are navigation placeholders only. Do not wire them to real pages unless the project already has a route system and matching placeholder routes are explicitly present.

The sidebar should be responsive:

- desktop: visible as a persistent layout region;
- mobile/tablet narrow width: hidden by default and opened through the top bar menu button;
- mobile open state: include a simple backdrop/scrim and close behavior.

Use semantic tokens for active states, borders, muted text, surfaces, and focus states.

### Top bar component

Create `frontend/src/components/layout/app-topbar.tsx`.

The top bar should include:

- mobile sidebar toggle button;
- current page title: `Kontrolna ploča`;
- optional compact context placeholders such as `Sezona nije odabrana` and `Selekcija nije odabrana` if they fit cleanly;
- right-side placeholder for future user menu or account actions, without implementing auth.

Do not add real season/team selectors in this unit. Do not add dropdown behavior unless the required shadcn/ui primitive already exists and the behavior is purely visual. Real season and team selection belongs in a later data-aware feature spec.

### Empty main content state

Create `frontend/src/components/common/app-empty-state.tsx` or an equivalent small reusable component.

Render a minimal placeholder in the main content area. Suggested Bosnian Latin copy:

```txt
Sistem je spreman za sljedeći korak.
Osnovni okvir aplikacije je postavljen. Naredni feature specovi će dodati stvarne module i podatke.
```

Keep the empty state simple. Do not wrap it in a complex dashboard, chart, table, or domain-specific card layout.

### Icons

Use Lucide React icons for sidebar and top bar icons if `lucide-react` is already installed. If it is not installed, install only `lucide-react` because it is the approved icon library in `context/ui-context.md`.

Recommended icon usage:

- app/sidebar identity or dashboard: `LayoutDashboard`;
- matches: `Trophy` or `CalendarDays`;
- players: `Users`;
- GPS/training: `Activity`;
- imports: `Upload`;
- medical: `HeartPulse`;
- media: `Film`;
- admin/settings: `Settings`;
- mobile menu: `Menu`;
- close: `X`.

Use the approved icon sizes from `context/ui-context.md`. Do not mix in another icon library.

### shadcn/ui usage

Use the existing shadcn/ui `Button` primitive if it exists and fits the needed interactions.

If `Button` is missing but shadcn/ui is already configured, add the `Button` primitive through the shadcn CLI instead of hand-writing a generated primitive. Do not add broad shadcn components that are not used by this unit.

Do not modify generated shadcn/ui primitive files for app-specific visuals.

### Accessibility and responsiveness

The shell must remain keyboard-accessible and readable:

- menu and close controls must have accessible names;
- interactive placeholder navigation items should be buttons or links with clear labels;
- focus states should remain visible through the theme ring token;
- mobile sidebar backdrop should close the sidebar;
- content should not overflow horizontally on mobile;
- main landmark or equivalent semantic structure should be present where practical.

### Documentation update

After implementation, update `context/progress-tracker.md` to reflect the actual Unit 02 implementation result.

At minimum, record:

- Unit 01 completed if the tracker does not already show it;
- Unit 02 implemented or in progress, depending on actual verification state;
- verification commands that passed or any blocker that prevented verification;
- any new dependency added, such as `lucide-react`, if it was missing and installed.

Do not update architecture, UI, or code-standard context files unless implementation reveals that the documented context is inaccurate.

Do not add Git workflow instructions, branch names, commit commands, push commands, or local terminal instructions inside this spec file.

## Dependencies

- `lucide-react` — install only if it is not already present; required for approved app shell icons.

Do not add React Router, TanStack Query, Zustand, i18next, React Hook Form, Zod, TanStack Table, Recharts, auth libraries, backend packages, or any other dependency in this unit unless it already exists and is required by the current project bootstrap.

## Verification checklist

- [ ] `frontend/src/app/app.tsx` or an equivalent top-level app composition exists.
- [ ] `frontend/src/components/layout/app-shell.tsx` exists and renders the shell layout.
- [ ] `frontend/src/components/layout/app-sidebar.tsx` exists and renders grouped navigation placeholders.
- [ ] `frontend/src/components/layout/app-topbar.tsx` exists and renders page context plus a mobile sidebar toggle.
- [ ] `frontend/src/components/common/app-empty-state.tsx` or an equivalent minimal empty state exists.
- [ ] The app renders a light-only FK Velež dashboard frame.
- [ ] Visible UI copy is Bosnian Latin by default.
- [ ] Navigation labels are placeholders only and do not implement real workflows.
- [ ] No fake match, player, GPS, medical, report, dashboard, or user data is introduced.
- [ ] No authentication, backend API calls, route protection, or permission logic is introduced.
- [ ] No React Router or other routing dependency is added unless it already existed and was already part of the app bootstrap.
- [ ] No TanStack Query, Zustand, localization, form, table, chart, or auth providers are introduced prematurely.
- [ ] No backend files are changed.
- [ ] Generated shadcn/ui primitive files are not modified for app-specific styling.
- [ ] `lucide-react` is used for icons if icons are included, and no other icon library is introduced.
- [ ] App UI uses semantic token utilities from `frontend/src/index.css`.
- [ ] No raw Tailwind palette classes or hardcoded colors are added in application UI components.
- [ ] No dark mode classes, `.dark` selectors, theme toggles, or dark-specific behavior are added.
- [ ] Non-adjacent frontend imports use the `@/` alias.
- [ ] The shell is usable on desktop, tablet, and mobile viewport widths.
- [ ] Mobile sidebar can open and close if mobile sidebar behavior is implemented.
- [ ] Interactive controls have accessible names and visible focus states.
- [ ] Frontend build passes from `frontend/` using the configured build command.
- [ ] Frontend lint/typecheck passes if configured.
- [ ] `context/progress-tracker.md` is updated with the actual Unit 02 implementation result after implementation.
