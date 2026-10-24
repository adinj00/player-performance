# Unit 26: Settings UI Foundation

## Goal

Replace the `/settings` placeholder with a complete administrator-facing frontend module for seasons, competitions, teams/selections, tracking-level assignment, venues, and opponents using the real backend contracts from Units 21, 22, and 25. The completed unit must support safe create, edit, lifecycle, filtering, and team-ordering workflows without adding backend changes, new settings resources, fake data, localization infrastructure, or unrelated club features.

## Design

This is a frontend-only feature unit. It provides the first real settings workspace inside the authenticated application shell and consumes existing administrator-protected backend APIs.

The UI must preserve the established Player Performance Data System direction:

- light-only FK Velež Mostar identity;
- semantic tokens from `frontend/src/index.css` only;
- Bosnian Latin as the default visible UI language;
- desktop-first settings workflows with clean tablet and mobile behavior;
- shadcn/ui primitives composed through app-level and feature-level components;
- TanStack Query for server state and mutations;
- React Hook Form with Zod for create and edit forms;
- React Router for settings sub-routes;
- `nuqs` only where URL-backed settings state provides real value;
- no raw Tailwind palette classes, hardcoded colors, dark mode, or ad-hoc visual overrides;
- no fake seasons, competitions, teams, venues, opponents, counts, or API responses.

Internal route names, filenames, TypeScript identifiers, query keys, API contracts, enum values, and code-level constants remain in English. Backend enum values must not be rendered directly to users.

### Scope boundary

This unit includes:

- administrator-only settings route behavior;
- settings navigation and sub-routes;
- seasons list, create, edit, archive, and restore UI;
- competitions list, create, edit, archive, and restore UI;
- teams/selections list, create, edit, tracking-level assignment, activation, deactivation, archive, restore, and reorder UI;
- venues list, create, edit, archive, and restore UI;
- opponents list, create, edit, archive, and restore UI;
- centralized Bosnian Latin labels for tracking levels and team statuses;
- archived-record visibility controls;
- loading, empty, error, validation, conflict, and mutation states;
- query invalidation after successful mutations;
- responsive and accessible interaction patterns;
- progress-tracker updates after implementation.

This unit does not include:

- backend, database, migration, API contract, or authorization changes;
- a separately managed tracking-level resource or tracking-level CRUD;
- editing the fixed backend enum values `BASIC`, `STANDARD`, or `FULL`;
- public or non-admin settings access;
- global selected-season or selected-team context in the top bar;
- match, player, report, import, media, medical, dashboard, audit, or training behavior;
- seed management or reset-to-default actions;
- hard deletion of any settings record;
- bulk archive, bulk restore, or bulk edit operations;
- drag-and-drop dependencies for team ordering;
- pagination for expected-small settings lists;
- optimistic updates for lifecycle or ordering mutations;
- English translations, language switching, or localization infrastructure;
- arbitrary settings key/value storage;
- logos, addresses, coordinates, governing-body metadata, external IDs, or other unconfirmed fields.

### Access and authorization behavior

All settings screens must require:

- a resolved authenticated session;
- completion of any required password-change flow from Unit 19;
- `primaryRole === ADMIN` before rendering settings data or issuing settings requests.

Frontend access behavior:

- keep `/settings` and all settings sub-routes behind the existing protected-route composition;
- show the existing session loading state while authentication is unresolved;
- show a clear access-denied state or safely redirect to `/` for authenticated non-admin users;
- show the `Postavke` navigation item only to administrators;
- guard direct navigation to every settings sub-route even when the sidebar item is hidden;
- never treat frontend checks as authorization; backend `AdminOnly` policies remain authoritative;
- handle backend `401` and `403` responses through the established session/error behavior without exposing protected data.

### Route structure

Use nested or equivalent React Router routes:

```txt
/settings
/settings/seasons
/settings/competitions
/settings/teams
/settings/venues
/settings/opponents
```

Required behavior:

- `/settings` redirects to `/settings/seasons` or renders the same seasons screen through a clear index route;
- settings sub-navigation uses links with route-aware active state;
- unknown `/settings/*` paths use the existing not-found behavior rather than silently showing a different resource;
- the application top bar may continue to show `Postavke`, while the page-level heading and active settings navigation identify the current resource;
- preserve stable English URL segments and Bosnian Latin visible labels.

Visible settings labels:

| Route | Visible label |
| --- | --- |
| `/settings/seasons` | Sezone |
| `/settings/competitions` | Takmičenja |
| `/settings/teams` | Timovi / Selekcije |
| `/settings/venues` | Stadioni / Lokacije |
| `/settings/opponents` | Protivnici |

Do not add `/settings/tracking-levels`. Tracking levels are fixed backend enum values assigned to a selection and are managed inside the teams/selections screen.

### Settings layout

Create one reusable settings page shell outside generated shadcn/ui primitives.

The shell should provide:

- `PageHeader` with title and concise description;
- compact settings sub-navigation;
- an optional primary action slot;
- the current resource content region;
- consistent loading, empty, error, and mutation presentation;
- responsive behavior that does not require horizontal page scrolling.

Recommended page heading:

```txt
Postavke
Upravljajte osnovnim podacima koji se koriste kroz sistem.
```

The sub-navigation may use shadcn/ui tabs styled as links or a compact route-aware navigation list. URL navigation must remain the source of truth; do not keep a competing selected-tab state in local component state.

### Lifecycle and archived-record behavior

Seasons, competitions, venues, and opponents use the backend archive/restore lifecycle from Units 21 and 25.

Teams use the backend status and lifecycle behavior from Unit 22:

- `ACTIVE`;
- `INACTIVE`;
- `ARCHIVED`.

Each resource screen must provide an archived visibility control:

- default: active/non-archived records only;
- optional: include archived records;
- persist the control in the URL with `nuqs` when the package and project pattern are already available from Unit 24;
- use a stable parameter such as `archived=include` or an existing approved convention;
- convert the UI state to the backend `includeArchived=true` query parameter;
- do not create separate duplicate pages for archived records.

Archived resources:

- remain visible only when archived records are included;
- show a readable archived status badge;
- expose `Vrati` instead of normal edit/archive actions;
- remain non-editable until restored;
- never expose physical delete actions.

Do not optimistically remove, restore, or reorder records. Wait for backend success, then invalidate/refetch the affected query.

### Centralized visible labels

Create centralized frontend mappings instead of duplicating enum switches across components.

Tracking levels:

| Backend value | Visible label | Helper meaning |
| --- | --- | --- |
| `BASIC` | Osnovni | Osnovni obim praćenja i unosa podataka. |
| `STANDARD` | Standardni | Prošireni obim praćenja za selekciju. |
| `FULL` | Potpuni | Puni raspoloživi obim praćenja i izvještavanja. |

Team statuses:

| Backend value | Visible label |
| --- | --- |
| `ACTIVE` | Aktivna |
| `INACTIVE` | Neaktivna |
| `ARCHIVED` | Arhivirana |

Status and tracking-level presentation must include text and must not rely only on color. Do not invent behavior differences beyond the backend contracts and documented tracking-level direction.

### Date presentation

Season `startDate` and `endDate` are date-only values.

Requirements:

- use native date inputs or the existing approved date input pattern;
- submit backend-compatible date-only values without timezone conversion;
- display dates in a clear Bosnian/BiH format such as `dd.MM.yyyy.`;
- use platform APIs such as `Intl.DateTimeFormat` where practical;
- do not install a date library only for this unit;
- prevent accidental UTC shifts caused by converting date-only values through local midnight `Date` objects.

## Implementation

### 1. Required reading and existing-pattern review

Before changing code:

1. Read root `AGENTS.md`.
2. Read the six context files in the required order.
3. Read `context/feature-specs/00-build-plan.md`.
4. Read this feature spec completely.
5. Review the existing app shell, route configuration, protected-route behavior, session provider, API client, TanStack Query provider, form helpers, and common UI primitives.
6. Review Unit 21, Unit 22, and Unit 25 backend contracts and Unit 24 administrator UI patterns.
7. Use relevant project-local skills from `.agents/skills/` when applicable without allowing them to override project context, the active feature spec, or established code standards.

Implement only this unit. Do not refactor unrelated auth, users, shell, API, or backend code unless a small change is required to register the settings routes or administrator navigation correctly.

### 2. Feature organization

Create or extend a feature-owned settings structure following the existing frontend conventions. A suitable structure is:

```txt
frontend/src/features/settings/
├── api/
│   ├── settings-api.ts
│   └── settings-query-keys.ts
├── components/
│   ├── settings-layout.tsx
│   ├── settings-navigation.tsx
│   ├── archived-filter.tsx
│   ├── settings-actions-menu.tsx
│   ├── season-form-dialog.tsx
│   ├── name-setting-form-dialog.tsx
│   ├── team-form-dialog.tsx
│   ├── team-order-controls.tsx
│   └── tracking-level-help.tsx
├── hooks/
│   ├── use-seasons.ts
│   ├── use-competitions.ts
│   ├── use-teams.ts
│   ├── use-venues.ts
│   └── use-opponents.ts
├── pages/
│   ├── seasons-settings-page.tsx
│   ├── competitions-settings-page.tsx
│   ├── teams-settings-page.tsx
│   ├── venues-settings-page.tsx
│   └── opponents-settings-page.tsx
├── schemas/
│   ├── season-schema.ts
│   ├── name-setting-schema.ts
│   └── team-schema.ts
├── types/
│   └── settings-types.ts
├── utils/
│   ├── settings-labels.ts
│   └── date-only.ts
└── index.ts
```

Equivalent organization is acceptable when it follows an already established feature convention and keeps resource concerns clear.

Rules:

- keep API calls out of presentational components;
- keep server data in TanStack Query, not Zustand or React Context;
- keep form state in React Hook Form;
- keep route/filter state in the URL where specified;
- use `@/` imports for non-adjacent frontend source imports;
- avoid a broad generic CRUD framework;
- reuse a focused name-only dialog/hook pattern for competitions, venues, and opponents only when it remains clear and strongly typed;
- keep season and team workflows resource-specific because their validation and lifecycle behavior differ.

### 3. API contracts and client functions

Use the existing typed API client and CSRF behavior. Do not duplicate fetch setup or bypass the shared ProblemDetails error model.

Implement typed client functions for the established endpoints.

Seasons:

```txt
GET    /api/settings/seasons
POST   /api/settings/seasons
PATCH  /api/settings/seasons/{seasonId}
POST   /api/settings/seasons/{seasonId}/archive
POST   /api/settings/seasons/{seasonId}/restore
```

Competitions:

```txt
GET    /api/settings/competitions
POST   /api/settings/competitions
PATCH  /api/settings/competitions/{competitionId}
POST   /api/settings/competitions/{competitionId}/archive
POST   /api/settings/competitions/{competitionId}/restore
```

Teams/selections:

```txt
GET    /api/settings/teams
POST   /api/settings/teams
PATCH  /api/settings/teams/{teamId}
PUT    /api/settings/teams/order
POST   /api/settings/teams/{teamId}/activate
POST   /api/settings/teams/{teamId}/deactivate
POST   /api/settings/teams/{teamId}/archive
POST   /api/settings/teams/{teamId}/restore
```

Venues:

```txt
GET    /api/settings/venues
POST   /api/settings/venues
PATCH  /api/settings/venues/{venueId}
POST   /api/settings/venues/{venueId}/archive
POST   /api/settings/venues/{venueId}/restore
```

Opponents:

```txt
GET    /api/settings/opponents
POST   /api/settings/opponents
PATCH  /api/settings/opponents/{opponentId}
POST   /api/settings/opponents/{opponentId}/archive
POST   /api/settings/opponents/{opponentId}/restore
```

Requirements:

- append `?includeArchived=true` only when the archived filter requests it;
- preserve backend request and response field names through typed interfaces;
- do not expose normalized names or persistence details in frontend types;
- use stable resource-specific query keys that include the archived-filter value;
- include CSRF headers/tokens for unsafe requests through the existing approved client mechanism;
- let the shared API client handle credentials and ProblemDetails parsing;
- do not call single-resource GET endpoints unless a real UI flow requires them;
- do not add fake fallbacks when the backend is unavailable.

### 4. Query and mutation hooks

Create resource-owned TanStack Query hooks.

Query requirements:

- queries run only after session resolution and administrator authorization;
- each list query includes its archived-filter state in the query key;
- use existing project defaults for retry and stale behavior;
- show meaningful loading and error states;
- never copy query results into Zustand or long-lived local state.

Mutation requirements:

- create, update, lifecycle, and reorder mutations expose pending state;
- prevent accidental duplicate submissions while pending;
- invalidate the affected resource list after success;
- close dialogs only after confirmed success;
- reset form state after successful create where appropriate;
- keep dialogs open and show safe errors after validation or conflict failure;
- do not apply optimistic updates to archive, restore, activate, deactivate, or reorder operations;
- map backend validation fields to form errors where the established ProblemDetails shape allows it;
- show a safe form-level or page-level error for non-field failures.

### 5. Settings routes and navigation

Replace the existing settings placeholder route with the settings layout and sub-routes.

Requirements:

- register every route listed in the Design section;
- keep the app shell mounted around settings pages;
- preserve the existing administrator navigation visibility rules;
- update route metadata/topbar behavior only as needed to identify settings consistently;
- do not add dynamic settings detail routes;
- close the mobile sidebar after navigating to settings according to existing shell behavior;
- ensure browser back/forward navigation correctly updates the active settings sub-navigation.

### 6. Seasons screen

Create the real seasons settings screen.

List presentation must show:

- season name;
- start date;
- end date;
- lifecycle status;
- actions available for the current lifecycle state.

Primary action:

```txt
Nova sezona
```

Create/edit form fields:

- `Naziv`;
- `Datum početka`;
- `Datum završetka`.

Validation:

- trim the name;
- require all three fields;
- enforce the frontend counterpart of the documented maximum length;
- require start date not to be later than end date;
- keep backend validation authoritative;
- do not enforce a specific season-name format such as `2026/27`.

Lifecycle behavior:

- active record: `Uredi` and `Arhiviraj`;
- archived record: `Vrati`;
- archive requires a confirmation dialog naming the season;
- restore may require a compact confirmation when consistent with existing destructive/lifecycle patterns;
- do not expose delete.

Default list ordering must follow the backend response. Do not re-sort into a conflicting order on the client.

### 7. Competitions screen

Create the real competitions settings screen.

List presentation must show:

- competition name;
- lifecycle status;
- actions.

Primary action:

```txt
Novo takmičenje
```

Create/edit form:

- field label: `Naziv`;
- trim and require the value;
- apply the documented frontend maximum length;
- use the shared name-only settings schema/dialog only if it keeps resource-specific labels and errors clear.

Lifecycle behavior:

- active record: `Uredi` and `Arhiviraj`;
- archived record: `Vrati`;
- archive confirmation includes the competition name;
- no delete or season-assignment behavior.

### 8. Teams / selections screen

Create the real teams/selections settings screen.

List presentation must show:

- administrator-controlled order;
- stored team/selection name;
- tracking-level label;
- status label;
- lifecycle and edit actions;
- accessible move controls for non-archived records.

Primary action:

```txt
Nova selekcija
```

Create/edit form fields:

- `Naziv`;
- `Nivo praćenja`.

Tracking-level control:

- options map exactly to `BASIC`, `STANDARD`, and `FULL`;
- show Bosnian Latin labels from the centralized mapping;
- include concise helper text explaining that the level controls the future available data/workflow depth;
- do not allow administrators to create, rename, reorder, or delete tracking-level definitions;
- do not imply that all tracking-level-driven workflows already exist.

Status actions:

- `ACTIVE`: allow `Deaktiviraj`, `Uredi`, and `Arhiviraj`;
- `INACTIVE`: allow `Aktiviraj`, `Uredi`, and `Arhiviraj`;
- `ARCHIVED`: allow `Vrati` only;
- state changes use explicit confirmation when they may affect workflow availability;
- do not expose direct status editing inside the form.

Reorder behavior:

- do not install a drag-and-drop package;
- provide keyboard-accessible `Pomjeri gore` and `Pomjeri dolje` controls or an equivalent accessible ordering pattern;
- reorder only the complete current non-archived list;
- after one move, submit the complete ordered non-archived ID array required by Unit 22;
- disable the top item's upward control and the bottom item's downward control;
- disable reorder controls while a reorder mutation is pending;
- do not allow archived records to participate in ordering;
- wait for backend success and then invalidate/refetch;
- if reorder fails, retain/refetch the last server-confirmed order and show a safe error;
- avoid repeated rapid requests and partial local order persistence.

Archived-inclusive mode may render archived records in a separate clearly labeled section or after non-archived records. It must not make archived rows appear reorderable.

### 9. Venues screen

Create the real venues settings screen.

Visible page label:

```txt
Stadioni / Lokacije
```

List presentation must show:

- venue name;
- lifecycle status;
- actions.

Primary action:

```txt
Nova lokacija
```

Form field:

- `Naziv`.

Lifecycle behavior matches the backend archive/restore contract. Do not add address, city, country, capacity, pitch type, coordinates, or default-venue fields.

### 10. Opponents screen

Create the real opponents settings screen.

List presentation must show:

- opponent name;
- lifecycle status;
- actions.

Primary action:

```txt
Novi protivnik
```

Form field:

- `Naziv`.

Lifecycle behavior matches the backend archive/restore contract. Do not add abbreviation, logo, country, competition, external provider, or scouting fields.

### 11. Forms, dialogs, and confirmations

Use React Hook Form with Zod and the shared form foundation from Unit 11.

Requirements:

- prefill edit dialogs from the selected resource;
- reset stale values when dialogs close or targets change;
- focus the first editable field when a dialog opens where supported by the existing dialog primitive;
- disable submit while pending;
- support Enter submission where it does not conflict with multiline or destructive confirmation behavior;
- use `FormErrorSummary` or the established form-level error pattern;
- connect backend field errors to matching fields where possible;
- show duplicate-name `409` conflicts with clear Bosnian Latin feedback;
- never show raw ProblemDetails internals, trace IDs, database constraint names, or stack details in normal form messages;
- use destructive variants only for archive confirmations where consistent with existing component semantics;
- restore, activate, and deactivate actions must use semantic non-destructive variants unless the established design system specifies otherwise;
- do not manually modify generated shadcn/ui primitives.

Recommended reusable visible messages:

```txt
Zapis je uspješno sačuvan.
Zapis je arhiviran.
Zapis je vraćen.
Redoslijed selekcija je ažuriran.
Zapis sa ovim nazivom već postoji.
Promjene nije moguće sačuvati. Provjerite unesene podatke.
```

Use the project's existing toast/feedback mechanism if one already exists. Do not install a notification package only for this unit; inline feedback is acceptable.

### 12. Tables, lists, and responsive behavior

Use semantic table markup and existing shadcn/ui table primitives where the established frontend already supports them. TanStack Table may be reused if it was installed by Unit 24 and materially improves rendering, but do not introduce complex client sorting/filtering for these expected-small server-ordered lists.

Requirements:

- preserve backend-defined ordering;
- keep row actions accessible by keyboard;
- use readable text labels or accessible names for icon-only controls;
- wrap wide tables safely on smaller screens;
- allow a compact stacked-card presentation on narrow mobile screens only when needed to avoid unusable horizontal scrolling;
- do not hide important status or lifecycle information on mobile;
- empty states should explain that no records exist and show the create action only when allowed;
- archived-filter state must remain reachable on mobile.

### 13. Error and session behavior

Handle errors through the established API/session conventions.

Requirements:

- `401`: allow the existing auth/session mechanism to move the user to sign-in safely;
- `403`: show access denied and do not retain protected settings content;
- required-password-change response: follow Unit 19 routing behavior;
- validation response: show field/form feedback;
- `404` during edit/lifecycle action: close stale target state, invalidate/refetch, and show a safe message;
- `409`: show duplicate-name or lifecycle conflict feedback without guessing server state;
- unexpected error: show `ErrorState` or a safe inline error with retry/refetch action;
- never replace an API failure with mock data.

### 14. Documentation update

During implementation:

- mark Unit 26 in progress in `context/progress-tracker.md`;
- record meaningful implementation decisions only when they are not already defined by context/spec;
- after all verification passes, mark Unit 26 complete and Unit 27 as next;
- document any verification failure honestly instead of marking the unit complete;
- update another context file only if implementation reveals a real architecture, UI, or standards mismatch.

The implementation report should state:

- routes added or replaced;
- settings resources wired;
- new reusable feature components introduced;
- any shadcn primitives added through the CLI;
- whether any dependency was unexpectedly required and why;
- all verification commands and their results.

## Dependencies

No new npm or NuGet packages are required.

Use the dependencies already introduced by earlier units:

- React Router;
- TanStack Query;
- React Hook Form;
- Zod;
- `@hookform/resolvers`;
- `nuqs` if already installed by Unit 24;
- TanStack Table if already installed and useful;
- shadcn/ui primitives;
- Lucide React;
- the existing typed API client and auth/session infrastructure.

Add missing shadcn/ui primitives through the existing shadcn CLI only when the unit directly uses them. Do not add a date library, drag-and-drop library, notification package, alternate table package, state-management package, localization package, or another API client.

## Verification checklist

- [ ] `AGENTS.md`, all required context files, the current build plan, Units 19, 21, 22, 24, and 25 specs, and this feature spec were read before implementation.
- [ ] Relevant project-local skills from `.agents/skills/` were used when applicable without overriding project context or this spec.
- [ ] `/settings` is protected and administrator-only in the frontend.
- [ ] Direct access to every settings sub-route is guarded.
- [ ] `/settings` resolves to a stable settings index screen or redirects to `/settings/seasons`.
- [ ] Routes exist for seasons, competitions, teams/selections, venues, and opponents.
- [ ] No separate tracking-level CRUD route or resource was invented.
- [ ] The settings sub-navigation uses route state as the source of truth.
- [ ] The sidebar `Postavke` item is visible only to administrators.
- [ ] Seasons use real Unit 21 API data and support create, edit, archive, restore, and archived visibility.
- [ ] Season forms preserve date-only values without timezone shifts.
- [ ] Competitions use real Unit 21 API data and support create, edit, archive, restore, and archived visibility.
- [ ] Teams/selections use real Unit 22 API data and support create, edit, tracking-level assignment, activate, deactivate, archive, restore, reorder, and archived visibility.
- [ ] Team reorder submits the complete non-archived ordered ID list atomically and does not use a new drag-and-drop dependency.
- [ ] Archived teams are not reorderable or editable until restored.
- [ ] Tracking-level and team-status labels are centralized and displayed in Bosnian Latin.
- [ ] Venues use real Unit 25 API data and support create, edit, archive, restore, and archived visibility.
- [ ] Opponents use real Unit 25 API data and support create, edit, archive, restore, and archived visibility.
- [ ] No hard-delete action exists for any settings resource.
- [ ] TanStack Query owns settings server state and mutation invalidation.
- [ ] Server data is not duplicated in Zustand or React Context.
- [ ] Forms use React Hook Form with Zod and shared form-error patterns.
- [ ] Unsafe requests use the established CSRF-capable API client.
- [ ] Loading, empty, error, validation, duplicate-conflict, not-found, and pending states are handled safely.
- [ ] Backend enum values are not rendered directly to users.
- [ ] Visible long-lived copy is Bosnian Latin by default.
- [ ] No fake settings data or fallback API responses were added.
- [ ] No backend, database, migration, or API contract files were changed.
- [ ] No player, match, report, import, media, medical, dashboard, audit, or training behavior was added.
- [ ] No localization infrastructure, English resources, or language switcher was added.
- [ ] Generated shadcn/ui primitive files were not manually modified.
- [ ] No raw Tailwind palette classes, hardcoded colors, dark-mode behavior, or ad-hoc shadcn visual overrides were introduced.
- [ ] Frontend source imports use the `@/` alias where appropriate and avoid deep relative paths.
- [ ] Settings screens remain usable on desktop, tablet, and mobile widths.
- [ ] Interactive controls have accessible names, visible focus states, and keyboard support.
- [ ] `context/progress-tracker.md` reflects the actual Unit 26 state after implementation.
- [ ] From `frontend/`, `npm run format` completes successfully.
- [ ] From `frontend/`, `npm run format:check` passes.
- [ ] From `frontend/`, `npm run lint` passes if configured.
- [ ] From `frontend/`, `npm run build` passes.
- [ ] Backend files remain unchanged; if they were changed unexpectedly, the reason is documented and `dotnet build` plus `dotnet test` pass.
