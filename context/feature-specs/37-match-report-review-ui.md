# Unit 37: Match Report Review UI

## Goal

Build the complete visible match-report workflow using the Unit 32 backend-owned lifecycle and `allowedActions`, the Unit 31 lineup snapshot, and the Unit 33 statistics completeness contract. Add a central team-scope-aware report queue and replace the match detail `Revizija` placeholder with submit, review, correction, verification, and archive interactions without adding report restore, audit history, comments, exports, GPS, media, imports, or backend workflow changes.

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
10. `context/feature-specs/31-match-lineup-appearance-backend.md`
11. `context/feature-specs/32-match-report-workflow-backend.md`
12. `context/feature-specs/33-manual-match-statistics-backend.md`
13. `context/feature-specs/34-matches-ui-foundation.md`
14. `context/feature-specs/35-lineup-appearances-ui.md`
15. `context/feature-specs/36-manual-match-statistics-ui.md`
16. `context/feature-specs/37-match-report-review-ui.md`

Use relevant project-local skills from `.agents/skills/` when applicable.

Before creating custom UI primitives or interaction patterns:

- check `context/references/shadcn-components.md`;
- prefer suitable shadcn/ui components;
- install shadcn components just in time through the approved workflow;
- compose app-level components outside `frontend/src/components/ui`;
- do not manually modify generated shadcn/ui primitive files;
- do not hand-build equivalents of existing shadcn primitives without a documented reason.

This unit is frontend-only except for required context-documentation synchronization.

Do not change backend domain models, API contracts, transition rules, authorization behavior, migrations, or endpoint implementations.

### Required context synchronization: archived report restore

The current backend workflow implemented by Unit 32 defines:

- `ARCHIVED` as terminal;
- no report restore endpoint;
- no approved restore target.

An older illustrative line in `context/architecture.md` may still mention:

```txt
ARCHIVED: view, restore if admin
```

Before implementing Unit 37, synchronize that context example with the implemented workflow:

```txt
ARCHIVED: view
```

Also ensure the documented transition list remains identical to the Unit 32 transitions.

Do not add:

- a restore button;
- a restore API call;
- a client-side status change;
- a speculative `ARCHIVED -> ...` transition.

A future report-restore feature requires a separate confirmed backend/domain decision defining the target status, permissions, audit behavior, and data-lock implications.

### Scope

This unit introduces:

- `/match-reports` workflow queue;
- role- and team-scope-aware report navigation;
- URL-driven report filters and pagination;
- report workflow status tabs;
- report list rows using the Unit 32 list endpoint;
- navigation from report rows to the related match `Revizija` tab;
- real `Revizija` tab content in the existing match detail shell;
- report workflow metadata display;
- backend-provided `allowedActions` rendering;
- report readiness summary;
- links to inspect the existing `Sastav` and `Statistika` tabs;
- submit for review;
- request correction with required reason;
- verify;
- archive;
- workflow mutation feedback and focused cache invalidation;
- read-only verified/archived report visibility for authorized roles;
- loading, empty, inaccessible, conflict, and error states.

This unit does not introduce:

- report restore;
- report delete;
- report comments;
- threaded reviewer discussion;
- multiple correction-history entries in the UI;
- audit-log history;
- report versions;
- report exports;
- PDF generation;
- coach notes;
- tactical notes;
- team statistics;
- GPS;
- media;
- imports;
- dashboard KPIs;
- email or in-app notifications;
- backend workflow or readiness changes.

### Existing backend contracts remain authoritative

Use the existing Unit 32 endpoints:

```txt
GET  /api/match-reports
GET  /api/matches/{matchId}/report

POST /api/matches/{matchId}/report
POST /api/match-reports/{reportId}/submit
POST /api/match-reports/{reportId}/verify
POST /api/match-reports/{reportId}/request-correction
POST /api/match-reports/{reportId}/archive
```

Use existing supporting reads where needed:

```txt
GET /api/matches/{matchId}
GET /api/matches/{matchId}/lineup
GET /api/match-reports/{reportId}/statistics
```

Do not add:

- a generic status mutation;
- a report patch endpoint;
- a restore endpoint;
- per-action frontend status assignment;
- a second readiness API;
- a second report-detail backend contract.

The frontend invokes explicit action endpoints and then refetches authoritative report state.

### Backend-provided `allowedActions` are the action source of truth

Every action rendered by Unit 37 must be derived from the report response's `allowedActions`.

Known semantic actions include:

```txt
VIEW
EDIT
SUBMIT_FOR_REVIEW
VERIFY
REQUEST_CORRECTION
ARCHIVE
```

Rules:

- show a workflow mutation only when its corresponding action is present;
- do not recreate role/status permission matrices in components;
- do not infer verify permission from the role alone;
- do not infer team access from the current route alone;
- do not show an admin override when the action is absent;
- always handle backend rejection because permissions or state may change after rendering;
- unknown action codes must be ignored safely for mutation rendering and documented if they indicate a frontend/backend contract mismatch.

`EDIT` is not a generic report-edit modal in Unit 37.

When `EDIT` is present:

- show navigation affordances to the existing `Sastav` and `Statistika` tabs;
- those tabs remain responsible for their own mutation controls and backend locks;
- do not duplicate lineup or statistics forms inside `Revizija`.

`VIEW` grants no mutation button.

### Report status labels

Use a centralized report-workflow display mapping.

Internal values:

```txt
DRAFT
READY_FOR_REVIEW
VERIFIED
NEEDS_CORRECTION
ARCHIVED
```

Bosnian Latin labels:

```txt
DRAFT -> Nacrt
READY_FOR_REVIEW -> Spremno za pregled
VERIFIED -> Verificirano
NEEDS_CORRECTION -> Potrebna korekcija
ARCHIVED -> Arhivirano
```

Suggested descriptions:

```txt
Nacrt
Podaci se još unose i mogu se mijenjati.

Spremno za pregled
Unos je zaključan dok ovlašteni korisnik pregleda izvještaj.

Verificirano
Izvještaj je pregledan i potvrđen.

Potrebna korekcija
Izvještaj je vraćen na doradu.

Arhivirano
Izvještaj je trajno zaključan za redovno uređivanje.
```

Do not display raw enum values.

Status display must include readable text and must not rely only on color.

### Route and navigation model

Add a protected route:

```txt
/match-reports
```

Suggested Bosnian navigation label:

```txt
Izvještaji utakmica
```

Place it in the existing Performance navigation group near `Utakmice`.

Navigation visibility:

- `ADMIN`, `DATA_OPERATOR`, and `ANALYST` may see the report queue;
- `COACH`, `MEDICAL_STAFF`, and `VIEWER` may also see it when the existing navigation/access conventions allow them to browse verified reports;
- backend list visibility remains authoritative;
- do not show hidden statuses or mutation capability merely because the navigation item is visible.

The report queue must not replace `/matches`.

Use `/matches` for football fixture status and match management.

Use `/match-reports` for report workflow status and review operations.

Do not mix report workflow tabs back into the Unit 34 match-status tabs.

### Navigation to the match review tab

Each report row opens the existing match detail page with the `Revizija` tab selected.

Use the tab-routing/search-parameter convention established by Unit 34.

Conceptual target:

```txt
/matches/{matchId}?tab=review
```

Use the actual existing tab key and routing convention rather than introducing a second source of truth.

The selected `Revizija` tab must be shareable through the URL when the current match-detail architecture already stores tab state in the URL.

Do not create a separate `/match-reports/{reportId}` detail shell unless the current router already requires it. The match remains the primary page context.

### Report queue page

The `/match-reports` page follows:

```txt
PageHeader
Filter controls
Workflow status tabs
Report table
Pagination
```

Suggested page title:

```txt
Izvještaji utakmica
```

Suggested description:

```txt
Pratite unos, pregled, korekcije i verifikaciju izvještaja utakmica.
```

Do not add a `Novi izvještaj` button.

Draft reports are initialized from a played match through Unit 36's existing prerequisite action.

The queue is for finding and processing reports that already exist.

### URL-driven report filters

Use `nuqs`.

Support at minimum:

- season;
- team/selection;
- competition;
- report status;
- date from;
- date to;
- page.

Use the Unit 32 list endpoint.

Rules:

- changing a filter resets page to the first page;
- discrete filters update without debounce;
- invalid URL values normalize safely;
- do not duplicate filter state in Zustand or React Context;
- do not load all reports and filter client-side;
- explicit out-of-scope team selection must surface the backend `403` safely;
- selected-team users must not be offered teams outside their authorized scope;
- use real seasons, teams, and competitions from existing settings queries;
- provide `Očisti filtere` when non-default filters are active.

### Workflow status tabs

Use shadcn `Tabs` or the approved app-level tabs composition.

Full workflow tabs:

```txt
Sve
Nacrti
Spremno za pregled
Verificirano
Potrebna korekcija
Arhivirano
```

Internal filter values:

```txt
all
DRAFT
READY_FOR_REVIEW
VERIFIED
NEEDS_CORRECTION
ARCHIVED
```

The active status is URL-driven.

Status visibility should follow known session capability only as a UX optimization:

- `ADMIN`, `DATA_OPERATOR`, and `ANALYST` may be shown all workflow tabs;
- roles that can read only verified/archived reports may be shown only `Sve`, `Verificirano`, and `Arhivirano`.

The backend remains authoritative and may return fewer results.

Do not reveal counts or labels for inaccessible report states through speculative client logic.

### Report queue table

Use TanStack Table and shadcn table primitives.

Recommended columns:

- match kickoff date/time;
- FK Velež selection;
- opponent;
- competition;
- report status;
- latest submission;
- verifier/verification time;
- latest correction state;
- actions.

Do not add:

- player statistics columns;
- lineup player columns;
- GPS completeness;
- media status;
- audit counts;
- generated report files.

Use deterministic backend ordering and server pagination.

Row behavior:

- primary row link opens the related match `Revizija` tab;
- status uses a localized badge;
- latest correction reason may be truncated in the table with a tooltip or preview;
- action menu uses shadcn `Dropdown Menu`;
- queue row actions should prioritize safe navigation.

High-impact workflow actions should normally be completed inside the `Revizija` tab after the user can see readiness and report context.

The queue may show a direct `Otvori` action and status-specific hints.

Do not add one-click verify or archive to a dense table row without full report context.

### Queue role-focused empty states

Required empty states:

#### No reports exist

Explain that no match reports are available.

Do not offer a generic create button.

For authorized data operators, suggest opening a played match and starting a report from its `Statistika` tab.

#### No reports match filters

Explain that no reports match the selected filters and offer `Očisti filtere`.

#### No reports ready for review

For the `READY_FOR_REVIEW` tab, show a specific neutral state:

```txt
Trenutno nema izvještaja spremnih za pregled.
```

#### Restricted read roles

Do not mention hidden drafts or review states.

Show only that no accessible reports are available.

### Report queue loading, error, and pagination

Prefer:

- `Skeleton` for loading;
- `Empty` for no data;
- `Alert` for query error;
- existing common pagination or shadcn `Pagination`;
- `Sonner` only for succinct mutation feedback, not initial query errors.

Pagination:

- uses backend metadata;
- stores page in URL;
- does not slice client-side.

Query errors include a retry action.

### `Revizija` tab integration

Replace the Unit 34 `Revizija` placeholder inside the existing match detail shell.

Do not duplicate:

- match header;
- breadcrumbs;
- match metadata;
- top-level tabs;
- existing `Sastav` or `Statistika` forms.

Keep `GPS / Fizički podaci` and `Video` placeholders unchanged.

The `Revizija` tab should contain:

```txt
Workflow Header
Current Status
Readiness Summary
Workflow Metadata
Latest Correction
Allowed Actions
```

The tab works for:

- data entry users;
- analysts;
- administrators;
- read-only users who may see verified/archived reports.

### No visible report state

If `GET /api/matches/{matchId}/report` returns the project's normal no-report result:

#### Authorized data-entry user

For a played, non-archived match, show:

- neutral explanation;
- link to the `Statistika` tab;
- note that `Započni izvještaj` is available there.

Do not duplicate Unit 36's report-creation button in `Revizija`.

Suggested copy:

```txt
Izvještaj još nije započet. Otvorite tab Statistika kako biste započeli unos.
```

#### Other users

Show a neutral unavailable state:

```txt
Izvještaj utakmice još nije dostupan.
```

Do not reveal whether an inaccessible draft exists.

For scheduled, postponed, cancelled, or archived matches without a visible report, show a match-state-appropriate unavailable message.

### Workflow header

Show:

- localized report status;
- concise status explanation;
- applied tracking level when available through the statistics response;
- match identity context already present in the page header;
- primary allowed action when one exists;
- secondary allowed actions in a clear action group or dropdown.

Do not show action buttons based only on role.

Primary action priority:

1. `SUBMIT_FOR_REVIEW`;
2. `VERIFY`;
3. `REQUEST_CORRECTION`;
4. `ARCHIVE`.

When both `VERIFY` and `REQUEST_CORRECTION` are allowed:

- `VERIFY` is the primary action;
- `REQUEST_CORRECTION` is a clearly distinct secondary action.

When `ARCHIVE` is allowed:

- keep it visually separated as a destructive/lifecycle action;
- do not present it as ordinary completion.

### Readiness summary

Use existing backend data without creating a second source of truth.

Fetch or reuse:

- report detail;
- lineup snapshot;
- statistics response.

Show a compact review-readiness summary containing at minimum:

#### Match state

- match is `PLAYED`;
- match is not archived.

#### Lineup and appearances

- lineup exists;
- number of starters;
- number of substitutes;
- number of concrete appearances;
- captain presence where applicable;
- substitution count.

Do not enforce new readiness rules in the UI.

Use the Unit 31 snapshot only to summarize and link to `Sastav`.

#### Statistics

- applied tracking level;
- completed player rows out of total appearance rows;
- goalkeeper row count;
- goalkeeper completeness;
- overall statistics completeness.

Use Unit 33 response completeness as authoritative.

Do not recompute the tracking matrix.

#### Workflow

- current report status;
- latest submission metadata;
- latest verification metadata;
- latest correction state.

### Readiness display semantics

The readiness summary is advisory before mutation.

Use labels such as:

```txt
Spremno
Nedostaju podaci
Nije dostupno
Zaključano
```

Do not state that the report is submit-ready solely from frontend calculations.

Recommended wording:

```txt
Provjera spremnosti
```

The submit endpoint remains authoritative.

When statistics are incomplete:

- show the backend-provided completeness state;
- link to `Statistika`;
- do not offer client-side workarounds.

When appearances are missing:

- link to `Sastav`.

When an unknown or unsupported statistics field prevents Unit 36 editing:

- show a compatibility warning;
- do not allow frontend assumptions to bypass it.

### Review navigation cards

Provide clear navigation to existing data sections:

```txt
Pregledaj sastav
Pregledaj statistiku
```

When `EDIT` is allowed, labels may become:

```txt
Uredi sastav
Uredi statistiku
```

The target tabs still decide whether an edit control is actually available based on their own backend response and lock state.

Do not embed duplicate editable grids in `Revizija`.

Use `Card`, `Item`, `Button`, `Badge`, and `Separator` or equivalent approved components.

### Workflow metadata

Display current persisted metadata available from Unit 32:

- report creator and creation time;
- latest submitter and submission time;
- verifier and verification time;
- latest correction requester and request time;
- current latest correction reason;
- archive metadata when returned.

Use existing user summaries from the backend response.

Do not attempt to construct a full transition history from current metadata.

Clearly label this section as current workflow information, not audit history.

Suggested heading:

```txt
Podaci o toku izvještaja
```

Unit 38 will add real audit history later.

### Latest correction reason

When a correction reason exists:

- show it prominently for `NEEDS_CORRECTION`;
- keep it visible as contextual metadata in later states if the backend still returns it;
- preserve line breaks safely if supported;
- do not render HTML;
- do not treat it as a threaded comment;
- show requester and time.

Use shadcn `Alert` or a compact `Card`.

Suggested heading:

```txt
Zahtjev za korekciju
```

### Submit for review

Render only when `SUBMIT_FOR_REVIEW` is present in `allowedActions`.

Suggested button:

```txt
Pošalji na pregled
```

Use shadcn `Alert Dialog` or a focused confirmation `Dialog`.

The confirmation should explain:

- the report will become read-only for normal data entry;
- lineup and statistics editing will be locked;
- an authorized reviewer can verify it or request correction.

Show the current readiness summary in or immediately before the confirmation.

On confirm:

```txt
POST /api/match-reports/{reportId}/submit
```

Do not send a target status from the frontend.

On success:

- show success feedback;
- refetch report detail;
- refetch report list queries;
- refetch statistics so `mayEdit` updates;
- refetch lineup/report-lock-related data where required;
- update visible actions from the new backend response;
- remain on the `Revizija` tab.

On semantic readiness failure:

- keep the report in its current status;
- show a clear error summary;
- map missing appearance/field requirements when the backend response provides structured details;
- link users to `Sastav` or `Statistika` as appropriate;
- do not guess missing requirements.

On `409` stale transition:

- refetch authoritative report state;
- show safe conflict feedback.

### Verify report

Render only when `VERIFY` is present.

Suggested button:

```txt
Verificiraj izvještaj
```

Use shadcn `Alert Dialog`.

Explain that:

- the report will be marked as verified;
- report-owned data remains locked;
- later correction requires an explicit correction request.

On confirm:

```txt
POST /api/match-reports/{reportId}/verify
```

Do not expose or toggle `canVerifyReports` in this UI.

On success:

- show success feedback;
- invalidate/refetch report detail and list;
- refetch statistics/lineup lock-aware data;
- keep the user on the review tab.

On `403` or `409`:

- show safe feedback;
- refetch report state;
- do not retain a stale verify button.

### Request correction

Render only when `REQUEST_CORRECTION` is present.

Suggested button:

```txt
Zatraži korekciju
```

Use shadcn `Dialog` with:

- React Hook Form;
- Zod;
- shadcn `Textarea`;
- field-level and form-level validation;
- submit/cancel actions.

Required field:

```txt
Razlog korekcije
```

Rules:

- trim whitespace;
- non-empty;
- use the backend maximum length if represented in shared/frontend contract metadata;
- otherwise mirror the explicit stable Unit 32 limit already implemented in the frontend API schema;
- show remaining/maximum character context when useful;
- do not allow HTML;
- do not create multiple comment fields;
- do not auto-populate with previous reason.

On submit:

```txt
POST /api/match-reports/{reportId}/request-correction
```

On success:

- status becomes `NEEDS_CORRECTION`;
- close the dialog;
- show success feedback;
- refetch report detail/list;
- refetch statistics and lineup so edit availability updates;
- display the returned/latest correction reason.

On validation error:

- keep the dialog open;
- preserve the reason;
- map backend errors.

On stale-state conflict:

- close or disable stale mutation controls after acknowledgement;
- refetch authoritative report state.

### Archive report

Render only when `ARCHIVE` is present.

Suggested button:

```txt
Arhiviraj izvještaj
```

Use shadcn `Alert Dialog`.

Explain that:

- archive is a report lifecycle action;
- it is not match cancellation;
- the report becomes terminal and read-only under the current V1 workflow;
- Unit 37 provides no restore.

On confirm:

```txt
POST /api/match-reports/{reportId}/archive
```

On success:

- show feedback;
- refetch report detail/list;
- refetch lock-aware statistics and lineup data;
- keep the report visible in read-only archived state.

Do not hide the archived report immediately from its detail page.

Do not add restore.

### `EDIT` action behavior

When `EDIT` is present:

- show `Uredi sastav` and `Uredi statistiku` navigation;
- do not create a generic report editor;
- do not make match metadata editable inside `Revizija`;
- preserve the actual edit permissions and locks of Units 34–36.

If the report is `NEEDS_CORRECTION`, show the latest correction reason near these navigation actions.

### Report queue and detail query behavior

Use TanStack Query.

Define or reuse stable query keys for:

- report list/filter snapshot;
- report by match;
- statistics by report;
- lineup by match;
- match detail.

Do not duplicate report query implementations already introduced by Unit 36.

Centralize report API contracts and hooks in the Matches/report workflow feature area.

Avoid circular imports between Matches submodules.

### Mutation invalidation

After every workflow mutation, invalidate/refetch the minimum relevant data:

- report detail by match;
- report list queries;
- statistics by report;
- lineup by match when edit-lock UI depends on report state;
- match detail only when it contains report-derived state or action surfaces.

Do not clear the entire TanStack Query cache.

Do not optimistically assign a report status before backend success.

### Concurrent and stale-state behavior

Workflow actions are sensitive to concurrent changes.

Required handling:

- disable repeated submission while a mutation is pending;
- prevent double-click duplicate requests;
- preserve correction text during normal validation errors;
- on `409`, refetch and display the authoritative status;
- on `403`, remove stale actions after refetch;
- on `404`, use safe inaccessible/missing behavior;
- on network error, retain the current loaded status and offer retry where appropriate;
- do not silently retry a high-impact mutation automatically.

### Loading and error states in `Revizija`

Use:

- `Skeleton` for initial workflow/readiness loading;
- `Empty` for no visible report;
- `Alert` for readiness/query/action errors;
- `Sonner` for succinct success feedback;
- `Badge` for report status;
- `Alert Dialog` for submit, verify, and archive confirmation;
- `Dialog` + `Textarea` for correction reason.

If report detail loads but lineup/statistics readiness queries fail:

- keep workflow metadata visible;
- show the failed readiness section separately;
- do not falsely show a complete state;
- do not block a backend-allowed action solely because a preview query failed;
- warn that final validation will occur on the server.

### Report visibility by role

Follow Unit 32 backend visibility.

Expected frontend outcomes:

#### `ADMIN`

May see all accessible report states and only backend-provided actions.

#### `DATA_OPERATOR`

May see all report states for accessible teams.

Normally receives:

- `EDIT`;
- `SUBMIT_FOR_REVIEW`;

only in allowed statuses.

#### `ANALYST`

May see all report states for accessible teams.

May receive:

- `REQUEST_CORRECTION`;
- `VERIFY` only when the backend grants it.

Do not infer verification permission from `ANALYST` alone.

#### `COACH`, `MEDICAL_STAFF`, `VIEWER`

May see only backend-visible verified/archived reports.

No workflow mutation controls.

The UI must not reveal inaccessible report existence through:

- queue status counts;
- no-report copy;
- action placeholders;
- error details.

### Responsive behavior

Desktop/tablet are primary for review work.

Requirements:

- report queue table uses contained horizontal overflow;
- key identity/status columns remain readable;
- review summary uses a responsive grid;
- action buttons remain reachable;
- dialogs fit tablet/mobile viewports;
- correction textarea remains usable on mobile;
- the match review tab remains readable without duplicating data-heavy tables;
- mobile may stack readiness cards and workflow metadata;
- do not create a separate mobile report workflow.

### Accessibility

Requirements:

- workflow status has readable text;
- confirmation dialogs describe the action and consequence;
- destructive archive action is clearly labeled;
- correction field has associated label/error/help text;
- action buttons have accessible names;
- pending actions expose disabled/loading state;
- focus returns appropriately after dialogs close;
- row actions do not interfere with row navigation;
- table markup remains semantic;
- tabs are keyboard navigable;
- correction reason is readable by assistive technology;
- readiness states do not rely only on color;
- live toast feedback is accessible through the approved toast system.

### Localization

Visible copy is Bosnian Latin with proper characters:

- `č`;
- `ć`;
- `š`;
- `ž`;
- `đ`.

Do not mix English and Bosnian labels on the same operational surface.

Suggested copy includes:

```txt
Izvještaji utakmica
Revizija
Nacrt
Spremno za pregled
Verificirano
Potrebna korekcija
Arhivirano
Provjera spremnosti
Pregledaj sastav
Pregledaj statistiku
Uredi sastav
Uredi statistiku
Pošalji na pregled
Verificiraj izvještaj
Zatraži korekciju
Razlog korekcije
Arhiviraj izvještaj
Podaci o toku izvještaja
Zahtjev za korekciju
Izvještaj utakmice još nije dostupan
Trenutno nema izvještaja spremnih za pregled
```

Internal API/status/action codes remain English.

Use centralized mapping utilities.

Do not scatter raw action/status labels across components.

### Shadcn-first component selection

Review all new interactions against:

```txt
context/references/shadcn-components.md
```

Prefer suitable components such as:

- `Tabs`;
- `Table`;
- `Badge`;
- `Card`;
- `Item`;
- `Alert`;
- `Alert Dialog`;
- `Dialog`;
- `Textarea`;
- `Field`;
- `Select`;
- `Combobox`;
- `Popover`;
- `Dropdown Menu`;
- `Skeleton`;
- `Empty`;
- `Separator`;
- `Tooltip`;
- `Pagination`;
- `Sonner`;
- `Button`;
- `Button Group` where it improves action grouping.

Do not install or use a component only because it exists.

Do not use conversation-oriented components such as `Message`, `Bubble`, or `Message Scroller` for a workflow that is not a chat.

Do not manually modify generated primitives.

Do not apply ad-hoc visual overrides.

### Frontend organization

Keep report workflow UI inside the Matches feature unless an existing feature structure already has a dedicated report submodule.

Suggested organization:

```txt
frontend/src/features/matches/
├── api/
├── components/
│   └── reports/
├── hooks/
├── schemas/
├── types/
├── utils/
│   └── reports/
└── index.ts
```

Potential page components may live under the existing route/page convention.

Reuse Unit 36 report query/types rather than creating duplicates.

Use:

- TanStack Query for server state;
- `nuqs` for queue filters and pagination;
- React Hook Form + Zod for correction reason;
- TanStack Table for the queue;
- shadcn/ui primitives for presentation and interactions;
- existing ProblemDetails mapping.

Do not store report server data in Zustand or React Context.

Avoid `any`.

### Tests and verification approach

Add frontend tests only when the repository already has an approved frontend testing foundation.

When tests exist, prioritize:

- report status/action mapping;
- allowed-action rendering;
- queue URL-state parsing;
- correction schema;
- focused mutation invalidation helpers;
- no-restore rendering;
- status-aware navigation labels.

Do not introduce a new test framework solely for this unit.

Manual verification must cover:

- queue access for every role;
- selected-team scope filtering;
- report status tabs;
- queue pagination;
- row navigation to `Revizija`;
- no-report states;
- draft readiness view;
- submit success;
- incomplete submit failure;
- ready-for-review analyst view;
- analyst without verify permission;
- analyst with verify permission;
- request correction from ready-for-review;
- request correction from verified;
- data-operator correction flow;
- resubmit after correction;
- verify confirmation;
- verified read-only behavior;
- archive confirmation;
- archived terminal behavior;
- no restore action;
- stale `403`/`409` handling;
- separate readiness-query failure;
- responsive behavior;
- keyboard and focus behavior.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

Before implementation, synchronize the outdated archived-report restore example in `context/architecture.md` with the implemented Unit 32 workflow.

If implementation reveals that the backend list/detail/action contracts differ from Units 32–33, update the relevant context/spec before continuing.

Do not silently add restore, comments, audit history, exports, notifications, or backend changes.

## Implementation

### 1. Synchronize the archived-report context rule

Update the outdated illustrative `ARCHIVED` action in `context/architecture.md` so it reflects:

```txt
ARCHIVED: view
```

Do not change backend code or add a transition.

Document the synchronization in `context/progress-tracker.md`.

### 2. Consolidate report frontend contracts and hooks

Reuse and centralize the report contracts/hooks introduced by Unit 36.

Ensure typed support for:

- report list;
- report detail by match;
- `allowedActions`;
- report workflow metadata;
- submit;
- verify;
- request correction;
- archive.

Avoid duplicate report DTO definitions.

### 3. Add the report queue route and navigation

Create:

```txt
/match-reports
```

Add role-aware navigation using the existing app-shell conventions.

Do not remove or replace `/matches`.

### 4. Build URL-driven report filters and status tabs

Use `nuqs`.

Implement:

- season;
- team;
- competition;
- status;
- date range;
- page;
- clear filters.

Use server-side Unit 32 queries.

### 5. Build the report queue table

Use TanStack Table and shadcn table primitives.

Implement:

- localized workflow status;
- match identity summaries;
- workflow metadata columns;
- safe row/action navigation;
- loading;
- empty states;
- errors;
- pagination.

Keep high-impact mutations in the review detail tab.

### 6. Replace the `Revizija` placeholder

Integrate the real report workflow UI into the existing match detail shell.

Handle:

- no report;
- current status;
- `allowedActions`;
- readiness summary;
- workflow metadata;
- latest correction reason;
- navigation to existing lineup/statistics tabs.

### 7. Build the readiness summary

Reuse:

- lineup snapshot;
- statistics response;
- report detail;
- match detail.

Show advisory completeness and links.

Do not duplicate the backend tracking-level matrix or claim final submit readiness.

### 8. Implement submit for review

Use the explicit submit endpoint and a confirmation dialog.

Handle:

- success;
- structured incomplete-data response;
- `403`;
- `404`;
- `409`;
- focused refetch/invalidation.

Do not assign status locally.

### 9. Implement request correction

Use React Hook Form, Zod, shadcn `Dialog`, and `Textarea`.

Require a valid reason.

Handle both:

- `READY_FOR_REVIEW`;
- `VERIFIED`;

only when the action is backend-provided.

### 10. Implement verify

Use the explicit verify endpoint and confirmation.

Render only from `allowedActions`.

Do not infer `canVerifyReports`.

### 11. Implement archive

Use the explicit archive endpoint and destructive confirmation.

Explain terminal read-only behavior.

Do not implement restore.

### 12. Wire focused query invalidation

After transitions, refresh:

- report detail;
- report lists;
- statistics editability/completeness;
- lineup lock-aware UI where necessary.

Do not clear unrelated cache.

### 13. Add conflict, loading, and accessibility behavior

Handle:

- pending mutations;
- stale actions;
- partial readiness failures;
- focus;
- keyboard use;
- responsive queue/review surfaces;
- safe inaccessible states.

### 14. Verify shadcn-first implementation

Review all new primitives against the component reference.

Remove unnecessary custom low-level controls.

Confirm generated UI primitives remain unmodified.

### 15. Update progress documentation

Update `context/progress-tracker.md` with:

- Unit 37 implementation status;
- context synchronization;
- verification outcomes;
- any documented limitations.

Do not mark Unit 37 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use the existing frontend stack:

- TanStack Query;
- `nuqs`;
- React Hook Form;
- Zod;
- TanStack Table;
- shadcn/ui;
- Lucide React.

Add required shadcn components just in time through the approved CLI workflow.

Do not add:

- another workflow/state-machine library;
- another query/state library;
- another table library;
- a comment/chat library;
- a timeline package;
- a notification service;
- an export/PDF package;
- a new testing framework solely for this unit.

## Verification checklist

- [ ] `context/architecture.md` no longer claims that archived reports can be restored in the current V1 workflow.
- [ ] No report restore endpoint, mutation, button, or status transition is added.
- [ ] `/match-reports` exists as a protected workflow queue.
- [ ] `/matches` remains the fixture-management page.
- [ ] Report workflow statuses are not mixed into match-status tabs.
- [ ] Report queue navigation follows existing role/access conventions.
- [ ] Report list data comes from `GET /api/match-reports`.
- [ ] Queue filters and pagination use `nuqs`.
- [ ] Queue filtering and pagination remain server-side.
- [ ] Season, team, competition, status, date, and page filters work.
- [ ] Selected-team users are not offered unauthorized team filters.
- [ ] Explicit out-of-scope team responses are handled safely.
- [ ] Workflow tabs use localized labels.
- [ ] Read-only roles do not see inaccessible workflow-status disclosures.
- [ ] The queue uses TanStack Table and shadcn table primitives.
- [ ] Queue rows navigate to the related match `Revizija` tab.
- [ ] High-impact workflow actions are not exposed as unsafe one-click table actions.
- [ ] Loading, no-data, no-filter-results, ready-for-review-empty, error, and pagination states are implemented.
- [ ] The Unit 34 `Revizija` placeholder is replaced without duplicating the match detail shell.
- [ ] No-report behavior is safe for data-entry and read-only users.
- [ ] Unit 36 remains the only place that initializes a draft report.
- [ ] Report status uses localized readable text.
- [ ] Report status display does not rely only on color.
- [ ] Every workflow mutation button is rendered from backend `allowedActions`.
- [ ] The frontend does not reconstruct role/status action matrices.
- [ ] Unknown action codes do not create unsafe mutation buttons.
- [ ] `EDIT` links to existing `Sastav` and `Statistika` tabs rather than creating a duplicate report editor.
- [ ] Readiness summary uses existing match, lineup, statistics, and report data.
- [ ] Statistics completeness comes from Unit 33 response data.
- [ ] The frontend does not duplicate the tracking-level matrix.
- [ ] Readiness preview is labeled as advisory/server-validated.
- [ ] Missing appearances link users to `Sastav`.
- [ ] Incomplete statistics link users to `Statistika`.
- [ ] Workflow metadata shows available creator, submitter, verifier, correction, and archive information.
- [ ] Workflow metadata is not presented as complete audit history.
- [ ] Latest correction reason is shown safely with requester/time.
- [ ] `SUBMIT_FOR_REVIEW` renders only when backend-provided.
- [ ] Submit uses the explicit submit endpoint.
- [ ] Submit confirmation explains that normal data editing will lock.
- [ ] Incomplete submit responses preserve status and provide actionable feedback.
- [ ] Submit success refreshes report, queue, statistics, and lock-aware UI.
- [ ] `VERIFY` renders only when backend-provided.
- [ ] Analyst verification permission is not inferred client-side.
- [ ] Verify uses explicit confirmation and endpoint.
- [ ] `REQUEST_CORRECTION` renders only when backend-provided.
- [ ] Correction uses React Hook Form, Zod, and a required trimmed reason.
- [ ] Correction success displays the latest reason and re-enables backend-permitted editing.
- [ ] `ARCHIVE` renders only when backend-provided.
- [ ] Archive confirmation explains terminal read-only behavior and distinguishes report archive from match cancellation.
- [ ] Archived report remains visible read-only.
- [ ] No generic report status selector exists.
- [ ] No optimistic local status assignment occurs before backend success.
- [ ] Pending actions prevent duplicate requests.
- [ ] `403`, `404`, `409`, validation, and network failures are handled safely.
- [ ] Stale actions disappear after authoritative refetch.
- [ ] A readiness subquery failure does not falsely show complete readiness.
- [ ] Query invalidation is focused rather than clearing the full cache.
- [ ] Report contracts/hooks introduced in Unit 36 are reused rather than duplicated.
- [ ] Report server data is not stored in Zustand or React Context.
- [ ] New UI follows shadcn-first component-selection guidance.
- [ ] Generated `frontend/src/components/ui/*` files are not manually modified.
- [ ] No conversation/chat component is misused for correction workflow.
- [ ] No raw Tailwind palette classes or hardcoded component colors are introduced.
- [ ] No ad-hoc visual overrides are applied to shadcn components.
- [ ] Frontend imports use `@/` instead of deep relative paths.
- [ ] Visible Bosnian copy uses proper Bosnian Latin characters.
- [ ] Queue and review UI remain usable on desktop, tablet, and mobile.
- [ ] Semantic tables, labels, focus, keyboard navigation, and accessible feedback are preserved.
- [ ] No backend code, migrations, workflow transitions, authorization rules, or readiness rules are changed.
- [ ] No comments, audit history, notifications, exports, GPS, media, imports, charts, or dashboard behavior is added.
- [ ] `npm run format` completes successfully.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] Frontend typecheck/build passes using the configured project command.
- [ ] Existing approved frontend tests pass when present.
- [ ] `context/progress-tracker.md` reflects the actual Unit 37 implementation, context synchronization, and verification state.
