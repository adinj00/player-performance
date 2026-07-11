# Unit 34: Matches UI Foundation

## Goal

Build the first real frontend Matches module using the existing match metadata APIs, authentication/session context, settings data, and team-scope authorization behavior. Provide a responsive match list, URL-driven filters, authorized create/edit and lifecycle actions, and a match detail shell without implementing lineup, appearances, report workflow, statistics, GPS, media, or audit UI.

## Design

### Required reading and implementation boundaries

Before implementation, read:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/ui-context.md`
5. `context/code-standards.md`
6. `context/ai-workflow-rules.md`
7. `context/progress-tracker.md`
8. `context/references/shadcn-components.md`
9. `context/feature-specs/00-build-plan.md`
10. `context/feature-specs/30-matches-backend-foundation.md`
11. `context/feature-specs/31-match-lineup-appearance-backend.md`
12. `context/feature-specs/32-match-report-workflow-backend.md`
13. `context/feature-specs/33-manual-match-statistics-backend.md`
14. `context/feature-specs/34-matches-ui-foundation.md`

Use relevant frontend Codex skills from `frontend/.agents/` when applicable.

Before creating custom UI primitives or interaction patterns:

- check `context/references/shadcn-components.md`;
- prefer suitable shadcn/ui components;
- install shadcn components just in time through the approved workflow;
- compose app-level components outside `frontend/src/components/ui`;
- do not hand-build equivalents of existing shadcn primitives without a documented reason.

This unit is frontend-only.

Do not change backend domain models, API contracts, authorization rules, migrations, or endpoint behavior in this unit.

### Scope

This unit introduces:

- `/matches` list page;
- `/matches/:matchId` detail shell;
- match navigation entry;
- team-scope-aware match queries;
- server-side filtering and pagination through the Unit 30 API;
- match creation;
- match metadata editing;
- valid match-status transitions exposed through the edit flow;
- final score entry/correction for `PLAYED` matches;
- admin-only archive/restore actions;
- loading, empty, error, unauthorized, and mutation feedback states.

This unit does not introduce:

- lineup editor;
- starting XI or substitutes UI;
- appearances or minutes UI;
- substitution UI;
- report workflow actions;
- report review/verification UI;
- player or goalkeeper statistics UI;
- GPS/physical data UI;
- media UI;
- import UI;
- audit-log UI;
- dashboard summaries.

Those remain owned by Units 35–37 and later units.

### Routes and navigation

Add or replace the existing Matches placeholder route with:

```txt
/matches
```

Add a match detail route:

```txt
/matches/:matchId
```

Use the existing routing architecture and protected app shell.

The sidebar Matches navigation item must:

- use Bosnian Latin visible copy;
- remain visible only to authenticated users who can access the Matches area according to the current role/navigation conventions;
- show the active state on both `/matches` and `/matches/:matchId`;
- use a suitable Lucide icon;
- not expose mutation capability merely through navigation visibility.

Suggested Bosnian label:

```txt
Utakmice
```

Do not create separate routes for create or edit. Use focused dialogs for these actions.

### Permission-aware UI behavior

Backend authorization remains authoritative.

Use session role, permission, and team-scope data only to:

- hide or disable actions the user clearly cannot perform;
- choose sensible filter defaults;
- avoid presenting impossible team choices.

Expected frontend behavior:

#### `ADMIN`

May see:

- create match;
- edit match;
- archive match;
- restore match;
- all accessible teams.

#### `DATA_OPERATOR`

May see:

- create match for teams in their authorized scope;
- edit accessible non-archived matches when the backend permits it;
- no archive/restore actions.

#### Other authenticated roles

May:

- view matches in their authorized scope;
- open match detail;
- not see create, edit, archive, or restore controls in this unit.

The UI must still handle backend `403`, `404`, and `409` responses because permissions and workflow state may change after rendering.

Do not infer report workflow permissions or show report actions in this unit.

### Match list page

The `/matches` page should follow the established page pattern:

```txt
PageHeader
Filter controls
Status tabs
Match data table
Pagination
```

Use the existing app-level `PageHeader`, loading, empty, and error components when appropriate.

The page header should include:

- title in Bosnian Latin;
- concise operational description;
- primary `Nova utakmica` action only when the current user may create matches.

Do not wrap the entire page in an unnecessary decorative card.

### URL-driven filters

Use `nuqs` for shareable list state.

Support at minimum:

- season;
- team/selection;
- competition;
- opponent;
- match status;
- date from;
- date to;
- archive visibility;
- page.

Use URL parameter names consistent with frontend conventions and backend query semantics.

Rules:

- discrete filters update without debounce;
- changing any filter resets pagination to the first page;
- URL values must be validated and normalized before use;
- invalid URL values fall back safely;
- do not duplicate filter state in Zustand or React Context;
- do not fetch all matches and filter client-side;
- use the server-side Unit 30 query API.

Default behavior:

- archived matches are hidden;
- the page uses the backend’s deterministic sort order;
- when a selected-team preference already exists in the application shell and is authorized, it may initialize the team filter without creating a second source of truth;
- `SELECTED_TEAMS` users must not be offered teams outside their authorized scope.

### Status tabs

Use shadcn `Tabs` or the approved existing tabs primitive for the primary match-status filter.

Provide Bosnian Latin labels for:

```txt
Sve
Zakazane
Odigrane
Odgođene
Otkazane
```

Internal values remain:

```txt
SCHEDULED
PLAYED
POSTPONED
CANCELLED
```

The active tab must be represented in URL state.

Archive visibility is separate from football match status and must not be represented as a fifth match status.

If archived visibility is enabled, use a separate explicit filter/control such as:

```txt
Prikaži arhivirane
```

Do not mix report workflow statuses into these tabs.

### Filter controls

Use appropriate shadcn components, favoring:

- `Select` or `Combobox` for season, team, competition, and opponent;
- `Date Picker` or an approved `Popover` + `Calendar` composition for date range;
- `Switch`, `Checkbox`, or `Select` for archive visibility;
- `Button Group` only if it improves compact filter actions without introducing custom behavior.

Use actual settings API data introduced by Units 21, 22, and 25.

Do not hardcode seasons, teams, competitions, opponents, or venues.

Filtering option rules:

- active/non-archived options are used for normal selection;
- existing archived references may still display correctly in rows/detail through backend summaries;
- do not silently replace a selected URL filter when its referenced value is unavailable;
- show a clear fallback label when an archived historical reference is returned by the backend.

Add a compact `Očisti filtere` action when any non-default filter is active.

### Match data table

Use TanStack Table for table behavior and shadcn table primitives for rendering.

Recommended columns:

- date and kickoff time;
- FK Velež selection;
- opponent;
- competition;
- round;
- home/away/neutral context;
- result/status;
- venue;
- row actions.

Do not add lineup count, report status, statistics completeness, GPS, media, or audit columns in this unit.

Use:

- localized status badges;
- localized location labels;
- 24-hour time;
- clear day-month-year formatting for Bosnian UI;
- JetBrains Mono only where useful for aligned score/numeric display.

Suggested location labels:

```txt
HOME -> Domaćin
AWAY -> Gost
NEUTRAL -> Neutralni teren
```

Suggested status labels:

```txt
SCHEDULED -> Zakazana
PLAYED -> Odigrana
POSTPONED -> Odgođena
CANCELLED -> Otkazana
```

Status badges must include readable text and must not rely only on color.

Row interaction:

- the primary row link opens `/matches/:matchId`;
- row actions use shadcn `Dropdown Menu`;
- actions are permission-aware;
- keyboard navigation must remain usable;
- action menu clicks must not accidentally trigger row navigation.

### Empty, loading, and error states

Use shadcn-first components according to the new component-selection guidance.

Prefer:

- `Skeleton` for initial/loading table structure;
- `Empty` for no matches;
- `Alert` for non-destructive query errors;
- `Sonner` for succinct mutation feedback;
- existing app-level wrappers when they already compose these primitives correctly.

Required states:

#### No matches exist

Show a Bosnian Latin empty state explaining that no match records exist.

When the user may create matches, include a `Nova utakmica` action.

#### Filters return no results

Show a distinct empty state that explains no matches satisfy the current filters and offers `Očisti filtere`.

#### Query error

Show a clear error message and retry action.

#### Unauthorized/inaccessible detail

Use the existing protected/unauthorized pattern without exposing match metadata.

### Pagination

Use backend pagination metadata.

Provide:

- previous;
- next;
- current page;
- total page or total item context when supplied by the API.

Keep page in URL state.

Do not implement client-side slicing of server data.

Use the existing common pagination component if one exists and fits the API contract. Otherwise compose shadcn `Pagination` at app level rather than creating a custom low-level pagination primitive.

### Match create dialog

Use shadcn `Dialog`.

The create form must use React Hook Form with Zod.

Required fields:

- season;
- competition;
- team/selection;
- opponent;
- optional venue;
- kickoff date;
- kickoff time;
- optional round/phase;
- location type.

New matches are always created as:

```txt
SCHEDULED
```

Do not show a status selector or score fields in the create form.

Form behavior:

- season, competition, team, opponent, and location are required;
- venue remains optional;
- date and time are combined and converted to the backend UTC contract;
- visible inputs display local/Bosnian-friendly date and 24-hour time;
- the resulting timestamp conversion must be deterministic;
- `DATA_OPERATOR` team choices are limited to authorized teams;
- `ADMIN` may choose any active team;
- only active/non-archived references are selectable for new records;
- submit button shows loading state;
- duplicate/conflict responses are shown clearly;
- field-level backend validation errors map to fields where possible;
- successful creation closes the dialog, invalidates match queries, shows success feedback, and navigates to the new match detail when the API response provides the identifier.

Do not invent default season, competition, venue, or opponent values unless an existing authorized selected context provides a valid explicit default.

### Match edit dialog

Use shadcn `Dialog`.

The edit form uses the current match metadata.

Editable fields:

- season;
- competition;
- opponent;
- optional venue;
- kickoff date;
- kickoff time;
- optional round/phase;
- location type;
- match status according to allowed Unit 30 transitions;
- final score when status is `PLAYED`.

`TeamId` must be displayed as immutable context and must not be editable.

Status options must be constrained by the current status:

#### `SCHEDULED`

Offer:

- `SCHEDULED`;
- `PLAYED`;
- `POSTPONED`;
- `CANCELLED`.

#### `POSTPONED`

Offer:

- `POSTPONED`;
- `SCHEDULED`;
- `CANCELLED`.

#### `PLAYED`

Keep:

- `PLAYED` only.

Allow score correction and other backend-permitted metadata correction.

#### `CANCELLED`

Keep:

- `CANCELLED` only.

Do not offer unsupported reopening transitions.

Score behavior:

- show `TeamScore` and `OpponentScore` only when the selected/current status is `PLAYED`;
- both values are required together;
- use integer inputs with zero allowed;
- do not label scores as home/away scores;
- visible labels must make clear that one value is FK Velež and one is the opponent;
- clear score values when changing from an editable non-played state to another non-played state;
- rely on backend validation as authoritative.

If the backend returns a workflow lock conflict because a report is `READY_FOR_REVIEW`, `VERIFIED`, or `ARCHIVED`, show a clear Bosnian message and refresh relevant match/report data.

Do not add report correction actions to work around the lock.

### Archive and restore actions

Use shadcn `Alert Dialog` for archive confirmation.

Archive:

- visible to `ADMIN` only;
- explains that the match record will be hidden from default lists and not physically deleted;
- does not imply cancellation;
- handles backend conflict when an active report workflow blocks archive;
- invalidates list/detail queries on success.

Restore:

- visible to `ADMIN` only for archived matches;
- may use a confirmation dialog when consistent with existing lifecycle patterns;
- preserves match status and metadata;
- invalidates relevant queries on success.

Do not expose hard delete.

### Match detail shell

The `/matches/:matchId` route should provide a stable shell for Units 35–37.

The page header should show:

- FK Velež selection;
- opponent;
- formatted kickoff date/time;
- competition;
- round when present;
- status badge;
- score when `PLAYED`;
- location context;
- venue when present;
- archive indicator when archived;
- permission-aware edit/archive/restore actions.

Use breadcrumbs where useful:

```txt
Utakmice / [Velež selection] – [Opponent]
```

Add shadcn `Tabs` for future match sections.

Create these tab labels/routes or in-page tab states as appropriate to the existing router architecture:

```txt
Pregled
Sastav
Statistika
GPS / Fizički podaci
Video
Revizija
```

Only `Pregled` is implemented in this unit.

Other tabs must show concise Bosnian placeholder/empty-state content indicating that the section is not yet available.

Do not:

- fetch or render lineup data;
- fetch or render report workflow data;
- fetch or render statistics;
- fetch or render GPS;
- fetch or render media;
- fetch or render audit records.

The detail shell must be structured so later units can replace each placeholder without rewriting the page header or route foundation.

### Overview tab

The implemented `Pregled` tab should show match metadata only.

Recommended sections:

- fixture summary;
- competition and season;
- team/selection;
- opponent;
- kickoff;
- venue;
- location type;
- round;
- match status;
- final score when present;
- record timestamps if useful and already returned.

Use shadcn `Card`, `Item`, `Separator`, or another suitable existing primitive instead of custom low-level layout when appropriate.

Keep the layout operational and compact. Avoid oversized sports hero layouts, gradients, decorative scoreboards, or marketing-style visual treatments.

### Data access and frontend organization

Keep code inside the Matches feature folder.

Suggested organization:

```txt
frontend/src/features/matches/
├── api/
├── components/
├── hooks/
├── schemas/
├── types/
├── utils/
└── index.ts
```

Use:

- TanStack Query for match/settings server state;
- `nuqs` for list filters and pagination;
- React Hook Form + Zod for create/edit forms;
- session provider/context only for stable current-user auth/access context;
- existing generic API client and ProblemDetails handling.

Do not store match server data in Zustand or React Context.

Use narrow, explicit TypeScript interfaces aligned with backend contracts.

Avoid `any`.

### Query and mutation behavior

Define stable query keys for:

- match list;
- match detail;
- seasons;
- competitions;
- teams;
- opponents;
- venues.

Reuse existing settings feature query helpers where available rather than duplicating API wrappers.

Mutations must invalidate the minimum relevant keys:

- create: match lists and new detail if needed;
- edit: matching detail and affected lists;
- archive/restore: matching detail and lists.

Avoid broad cache clearing.

Handle stale authorization/workflow conflicts by:

- showing clear feedback;
- invalidating/refetching affected data;
- not locally forcing a state the backend rejected.

### Localization and display rules

Visible copy is Bosnian Latin with proper characters:

- use `č`, `ć`, `š`, `ž`, and `đ`;
- do not use ASCII fallbacks;
- do not mix Bosnian and English labels on the same operational screen;
- internal enum and API values remain English.

Use a centralized mapping utility for match status and location labels.

Do not display raw enum names.

Dates/times:

- display local time in 24-hour format;
- use clear day-month-year formatting;
- convert API UTC timestamps consistently;
- do not scatter ad-hoc date formatting across components.

### Accessibility and responsive behavior

Desktop/tablet are primary for match operations, but mobile must remain usable.

Requirements:

- dialogs remain keyboard accessible;
- every input has an associated label;
- validation messages are associated with fields;
- status tabs are keyboard navigable;
- table retains semantic markup;
- horizontal overflow is contained;
- mobile may show a simplified column set with detail navigation;
- actions have accessible names;
- icon-only buttons use tooltips or accessible labels;
- focus returns appropriately after dialogs close;
- destructive archive action uses explicit confirmation.

Do not build a separate mobile-only product flow.

### Tests

Add frontend tests only if the repository already has an approved frontend testing foundation by implementation time or if the existing feature patterns require them.

At minimum, verification must manually cover:

- role-aware action visibility;
- URL filter behavior;
- server-side pagination;
- create flow;
- edit/status flow;
- played score entry/correction;
- archive/restore;
- workflow-lock conflict display;
- inaccessible match detail;
- responsive behavior;
- loading/empty/error states.

Do not introduce a new testing framework solely for this unit.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

If implementation reveals that match API contracts, role visibility, route structure, status behavior, or component conventions differ from project context, update the relevant context file before continuing.

Do not silently expand Unit 34 into lineup, report, statistics, GPS, media, import, or audit UI.

## Implementation

### 1. Add the Matches feature structure and routes

Create the Matches feature folder organization.

Wire:

- `/matches`;
- `/matches/:matchId`;
- sidebar navigation;
- protected route behavior.

Replace old placeholder route content rather than leaving duplicate screens.

### 2. Add match API contracts and hooks

Create typed frontend contracts aligned with Unit 30.

Implement TanStack Query hooks for:

- list matches;
- get match detail;
- create match;
- update match;
- archive match;
- restore match.

Reuse settings API/query helpers for selector options.

### 3. Build URL-driven filters and status tabs

Use `nuqs`.

Implement:

- season filter;
- team filter;
- competition filter;
- opponent filter;
- date range;
- match status tabs;
- archived visibility;
- pagination;
- clear filters.

Keep server queries authoritative.

### 4. Build the match data table

Use TanStack Table and shadcn table primitives.

Add:

- localized cells;
- badges;
- row navigation;
- permission-aware dropdown actions;
- responsive handling;
- loading, empty, error, and pagination states.

### 5. Build create and edit dialogs

Use shadcn `Dialog`, React Hook Form, and Zod.

Implement:

- settings selectors;
- local date/time input and UTC conversion;
- immutable team display in edit mode;
- valid status options;
- played score fields;
- backend validation/error mapping;
- success feedback and query invalidation.

### 6. Build archive and restore actions

Use shadcn `Alert Dialog` for archive confirmation.

Handle:

- admin-only visibility;
- report-workflow archive conflicts;
- success/error feedback;
- query invalidation.

### 7. Build the match detail shell

Create:

- stable page header;
- breadcrumbs;
- metadata overview;
- future section tabs;
- placeholder states for later units.

Do not fetch future feature data.

### 8. Verify shadcn-first implementation

Review all new UI primitives and interaction patterns against:

```txt
context/references/shadcn-components.md
```

Replace unnecessary hand-built primitives with suitable shadcn components.

Do not modify generated `frontend/src/components/ui/*` files.

### 9. Update progress documentation

Update `context/progress-tracker.md` with actual implementation and verification results.

Do not mark Unit 34 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use the frontend packages and infrastructure already introduced by earlier units:

- TanStack Query;
- nuqs;
- React Hook Form;
- Zod;
- TanStack Table;
- shadcn/ui;
- Lucide React.

Add required shadcn components just in time through the approved shadcn CLI workflow. Do not install the full component catalog.

Do not add another data-fetching, form, table, date, toast, or state-management package unless the existing approved stack cannot satisfy a documented requirement.

## Verification checklist

- [ ] `/matches` renders the real match list instead of placeholder or mock data.
- [ ] `/matches/:matchId` renders a stable real match detail shell.
- [ ] Matches navigation uses Bosnian Latin copy and correct active state.
- [ ] Match queries use TanStack Query and the existing API client.
- [ ] Filters and pagination use `nuqs` URL state.
- [ ] Filtering is performed by the backend, not by loading all records client-side.
- [ ] Season, team, competition, opponent, status, date, and archive filters work.
- [ ] Changing filters resets pagination.
- [ ] Match status tabs use localized labels and URL state.
- [ ] Report workflow statuses are not mixed into match-status tabs.
- [ ] The table uses TanStack Table and shadcn table primitives.
- [ ] Match statuses and location types display localized labels instead of raw enums.
- [ ] Dates use clear Bosnian day-month-year formatting and 24-hour time.
- [ ] Loading uses an approved skeleton/loading pattern.
- [ ] Empty states distinguish no data from no filter results.
- [ ] Query errors provide readable feedback and retry behavior.
- [ ] `ADMIN` sees create, edit, archive, and restore actions where applicable.
- [ ] `DATA_OPERATOR` sees create/edit actions only for authorized teams.
- [ ] Other roles do not see match mutation actions.
- [ ] Backend `403`, `404`, and `409` responses are still handled safely.
- [ ] Create form uses React Hook Form and Zod.
- [ ] Create form uses real settings data and contains no hardcoded teams or competitions.
- [ ] New matches are created only as `SCHEDULED`.
- [ ] Create flow converts local date/time consistently to the backend UTC contract.
- [ ] Edit form does not allow changing `TeamId`.
- [ ] Edit status options follow Unit 30 transition rules.
- [ ] `PLAYED` requires both FK Velež and opponent score fields.
- [ ] Played score correction is supported.
- [ ] Unsupported status reopening is not offered.
- [ ] Workflow-lock conflicts are displayed clearly and trigger relevant refetching.
- [ ] Archive uses explicit confirmation and is not presented as cancellation.
- [ ] Restore preserves match metadata/status behavior.
- [ ] No hard-delete action exists.
- [ ] Detail overview shows only match metadata in this unit.
- [ ] Sastav, Statistika, GPS/Fizički podaci, Video, and Revizija remain placeholders without future data fetching.
- [ ] New UI follows shadcn-first component selection guidance.
- [ ] Generated `frontend/src/components/ui/*` files are not manually modified.
- [ ] No raw Tailwind palette classes or hardcoded component colors are introduced.
- [ ] No ad-hoc visual overrides are applied to shadcn components.
- [ ] Frontend imports use the `@/` alias instead of deep relative paths.
- [ ] Visible Bosnian copy uses proper Bosnian Latin characters.
- [ ] Keyboard navigation, labels, focus behavior, and semantic table markup are preserved.
- [ ] Mobile/tablet layouts remain usable without a separate product flow.
- [ ] `npm run format` completes successfully for the frontend.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] Frontend typecheck/build passes using the configured project command.
- [ ] Backend files and contracts are not changed.
- [ ] `context/progress-tracker.md` reflects the actual Unit 34 implementation and verification state.
