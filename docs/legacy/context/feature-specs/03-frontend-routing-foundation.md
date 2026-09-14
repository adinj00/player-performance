# Unit 03: Frontend Routing Foundation

## Goal

Introduce the frontend routing foundation for the Player Performance Data System so the app shell can host stable route-level placeholder pages. This unit wires navigation structure only and must not implement authentication, protected routes, backend data fetching, domain workflows, fake data, or role-aware behavior.

## Design

The routing foundation must preserve the light-only FK Velež Mostar visual identity established by the app shell. All routed screens must use the existing shell and semantic design tokens from `frontend/src/index.css`. Do not add dark mode, theme switching, raw Tailwind palette classes, hardcoded colors, or one-off visual overrides.

Visible UI copy must be Bosnian Latin by default. Keep route page text minimal, operational, and easy to replace once localization infrastructure exists. Internal route constants, component names, file names, TypeScript identifiers, and translation-key-like names remain in English.

This unit should make navigation structurally real without implying completed product behavior. Sidebar items should become links to route-level placeholders, but every routed page must clearly remain a placeholder. Do not add mock matches, fake players, sample GPS numbers, demo users, fake reports, fake tables, fake charts, or simulated backend responses.

The app should support a small set of initial authenticated-area route placeholders even though authentication is not implemented yet. Route protection belongs in a later authentication feature spec. This unit only prepares the frontend route map and empty page surfaces.

The routing behavior should be desktop-first and remain usable on tablet and mobile. Navigating through the sidebar should update the active route state and render the matching placeholder inside the existing main content area.

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

This is a frontend-only routing foundation unit.

Do not implement:

- authentication, login, logout, sessions, route guards, or permission checks;
- backend API calls;
- TanStack Query server-state setup;
- Zustand stores;
- localization providers or translation resource files;
- forms, schemas, validation, tables, charts, dashboard metrics, imports, match workflows, player workflows, medical workflows, audit workflows, media workflows, settings forms, or real module behavior;
- fake domain data or placeholder statistics;
- role-aware or team-scope-aware navigation;
- persistent user preferences;
- backend code changes.

If the project already has a routing library and route foundation, extend the existing pattern minimally instead of introducing a competing router setup.

### Routing dependency

Use `react-router-dom` for the frontend routing foundation unless the project already has an explicitly established routing dependency.

If `react-router-dom` is not installed, add it in this unit because route structure is the purpose of this spec. Do not add TanStack Router or another router unless the existing project already uses it and replacing it would create unnecessary churn.

### Frontend file structure

Add or update frontend files using the documented frontend boundaries.

Preferred structure:

```txt
frontend/src/
├── app/
│   ├── app.tsx
│   ├── app-routes.tsx
│   └── route-paths.ts
├── components/
│   ├── common/
│   │   ├── module-placeholder.tsx
│   │   └── not-found-page.tsx
│   └── layout/
│       ├── app-shell.tsx
│       ├── app-sidebar.tsx
│       └── app-topbar.tsx
└── pages/
    ├── dashboard-page.tsx
    ├── matches-page.tsx
    ├── players-page.tsx
    ├── training-gps-page.tsx
    ├── imports-page.tsx
    ├── teams-page.tsx
    ├── medical-page.tsx
    ├── media-page.tsx
    ├── users-page.tsx
    └── settings-page.tsx
```

Use this structure unless the existing project already has an equivalent convention. Keep same-folder or immediately adjacent relative imports when clearer. Use `@/` imports for non-adjacent frontend source imports and avoid deep relative imports such as `../../../`.

### Route constants

Create `frontend/src/app/route-paths.ts` or an equivalent route constants file.

Define route paths for the initial app navigation:

```txt
/
/matches
/players
/training-gps
/imports
/teams
/medical
/media
/users
/settings
```

Use English route path segments for stable URLs and code consistency. Bosnian Latin remains the visible UI language for labels and page copy.

Do not add dynamic entity routes yet, such as match detail, player detail, report detail, import detail, media detail, user detail, or settings sub-routes. These belong in later module-specific feature specs.

### App routes

Create `frontend/src/app/app-routes.tsx` or an equivalent route composition file.

Requirements:

- wrap the routed app with `BrowserRouter` at the top of the frontend app composition;
- render the existing app shell around route content;
- use `Routes` and `Route` entries for every initial route placeholder;
- render a not-found page for unmatched routes;
- keep route components small and static;
- do not add loaders, actions, route-level data fetching, route guards, or async route behavior in this unit.

If `frontend/src/app/app.tsx` already owns app composition from Unit 02, update it to delegate routing to `AppRoutes` or wrap routing cleanly without duplicating shell state.

### Sidebar navigation wiring

Update `frontend/src/components/layout/app-sidebar.tsx` so navigation items render as router links instead of inert placeholders.

Requirements:

- use `NavLink` or equivalent active-route support from the router;
- preserve the existing grouped navigation labels;
- set the active state based on the current route, not hardcoded `Kontrolna ploča` state;
- clicking a link on mobile should close the sidebar if the shell supports mobile open/close state;
- keep labels Bosnian Latin;
- keep navigation items as links to placeholder pages only.

Recommended visible labels remain:

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

Do not implement role-aware filtering or permission behavior. The visible navigation can remain static until backend auth and authorization are implemented.

### Top bar page context

Update `frontend/src/components/layout/app-topbar.tsx` so the displayed page title reflects the current route.

Requirements:

- show `Kontrolna ploča` on `/`;
- show the matching Bosnian Latin title on each placeholder route;
- preserve the mobile sidebar toggle;
- keep optional context placeholders minimal if they already exist, such as `Sezona nije odabrana` and `Selekcija nije odabrana`;
- do not add real selectors, dropdown menus, query parameters, season/team state, or persistence.

The title may be passed from the shell, derived from route metadata, or read from a small route config object. Prefer a simple route config if it avoids duplicating route labels across sidebar and topbar.

### Route placeholder pages

Create one small placeholder page component per route or use a shared `ModulePlaceholder` component with route-specific props.

Each placeholder page must include:

- a short Bosnian Latin page title;
- a one-sentence description that clearly states the module will be added later;
- no domain data, fake counts, fake tables, fake cards, charts, forms, or interactive workflow controls.

Suggested placeholder copy:

```txt
Kontrolna ploča
Pregled sistema će biti dodan u kasnijem feature specu.

Utakmice
Modul za utakmice će biti dodan u kasnijem feature specu.

Igrači
Modul za igrače će biti dodan u kasnijem feature specu.

Trening GPS
Modul za GPS i fizičko opterećenje će biti dodan u kasnijem feature specu.

Importi
Modul za import podataka će biti dodan u kasnijem feature specu.

Timovi / Selekcije
Modul za timove i selekcije će biti dodan u kasnijem feature specu.

Medicinski status
Modul za medicinski status i dostupnost igrača će biti dodan u kasnijem feature specu.

Medijska biblioteka
Modul za mediju i vanjske reference će biti dodan u kasnijem feature specu.

Korisnici i uloge
Modul za korisnike, uloge i pristup će biti dodan u kasnijem feature specu.

Postavke
Modul za sistemske postavke će biti dodan u kasnijem feature specu.
```

Keep placeholders visually consistent and restrained. A simple card, panel, or empty-state block is acceptable if it uses semantic tokens and existing shadcn/ui primitives without ad-hoc visual overrides.

### Not found page

Create `frontend/src/components/common/not-found-page.tsx` or an equivalent component.

Requirements:

- show a clear Bosnian Latin message for unknown frontend routes;
- include a link or button back to `/` labeled `Nazad na kontrolnu ploču`;
- no error stack, debug details, or technical diagnostics in visible UI;
- use semantic token utilities and existing shadcn/ui primitives where appropriate.

### Accessibility and responsiveness

Routing must preserve the accessibility and responsive behavior from Unit 02:

- active navigation state must not rely only on color; use text weight, border, indicator, aria-current, or equivalent semantic state where practical;
- links and buttons must have accessible names;
- focus states must remain visible;
- mobile sidebar navigation should be usable with keyboard and touch;
- route content should not overflow horizontally on mobile;
- the app should remain readable on desktop, tablet, and mobile widths.

### Documentation update

After implementation, update `context/progress-tracker.md` to reflect the actual Unit 03 implementation result.

At minimum, record:

- Unit 02 completed if the tracker does not already show it;
- Unit 03 implemented or in progress, depending on actual verification state;
- the routing dependency added, if `react-router-dom` was installed;
- verification commands that passed or any blocker that prevented verification.

Do not update architecture, UI, or code-standard context files unless implementation reveals that the documented context is inaccurate.

Do not add Git workflow instructions, branch names, commit commands, push commands, or local terminal instructions inside this spec file.

## Dependencies

- `react-router-dom` — install only if no frontend routing library is already present; required for the route foundation in this unit.

Do not add TanStack Query, Zustand, i18next, React Hook Form, Zod, TanStack Table, Recharts, auth libraries, backend packages, or other dependencies in this unit.

## Verification checklist

- [ ] `frontend/src/app/route-paths.ts` or an equivalent route constants file exists.
- [ ] `frontend/src/app/app-routes.tsx` or an equivalent route composition file exists.
- [ ] The app is wrapped with `BrowserRouter` or the existing approved router provider.
- [ ] The existing app shell wraps all route content.
- [ ] Initial routes exist for `/`, `/matches`, `/players`, `/training-gps`, `/imports`, `/teams`, `/medical`, `/media`, `/users`, and `/settings`.
- [ ] A not-found route renders for unmatched frontend paths.
- [ ] Sidebar navigation items use router links instead of inert placeholders.
- [ ] Sidebar active state follows the current route.
- [ ] Top bar page title reflects the current route.
- [ ] Mobile sidebar closes after navigation if mobile sidebar state exists.
- [ ] Visible UI copy is Bosnian Latin by default.
- [ ] Route path segments and code identifiers remain English.
- [ ] Placeholder pages clearly state that real modules will be added later.
- [ ] No fake match, player, GPS, import, media, medical, report, dashboard, user, or settings data is introduced.
- [ ] No authentication, route guards, sessions, permissions, or role-aware navigation are introduced.
- [ ] No backend API calls or server-state fetching are introduced.
- [ ] No TanStack Query, Zustand, localization, form, table, chart, or auth providers are introduced prematurely.
- [ ] No backend files are changed.
- [ ] Generated shadcn/ui primitive files are not modified for app-specific styling.
- [ ] App UI uses semantic token utilities from `frontend/src/index.css`.
- [ ] No raw Tailwind palette classes or hardcoded colors are added in application UI components.
- [ ] No dark mode classes, `.dark` selectors, theme toggles, or dark-specific behavior are added.
- [ ] Non-adjacent frontend imports use the `@/` alias.
- [ ] The routed app remains usable on desktop, tablet, and mobile viewport widths.
- [ ] Interactive route links and controls have accessible names and visible focus states.
- [ ] Frontend build passes from `frontend/` using the configured build command.
- [ ] Frontend lint/typecheck passes if configured.
- [ ] `context/progress-tracker.md` is updated with the actual Unit 03 implementation result after implementation.
