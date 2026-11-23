# Unit 36: Manual Match Statistics UI

## Goal

Build the real manual player and goalkeeper statistics interface inside the match detail `Statistika` tab using the atomic Unit 33 statistics contract. Support dense desktop/tablet data entry, tracking-level-driven fields, draft report initialization, completeness feedback, workflow-aware read-only behavior, and safe atomic saves without adding report transition actions, GPS, media, imports, audit history, or new backend behavior.

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

Use relevant project-local skills from `.agents/skills/` when applicable.

Before creating custom UI primitives or interaction patterns:

- check `context/references/shadcn-components.md`;
- prefer suitable shadcn/ui components;
- install required shadcn components just in time through the approved workflow;
- compose app-level components outside `frontend/src/components/ui`;
- do not manually modify generated shadcn/ui primitive files;
- do not hand-build equivalents of existing shadcn primitives without a documented reason.

This unit is frontend-only.

Do not change backend domain models, API contracts, validation rules, migrations, authorization behavior, tracking-level behavior, or report workflow transitions.

### Scope

This unit introduces:

- real content for the existing match detail `Statistika` tab;
- match-report lookup by match;
- minimal draft report initialization through the existing Unit 32 create-report endpoint;
- player statistics read mode;
- goalkeeper statistics read mode;
- player statistics edit mode;
- goalkeeper statistics edit mode;
- tracking-level-driven dynamic fields;
- atomic statistics save through Unit 33;
- row and overall completeness feedback;
- backend validation and workflow-conflict feedback;
- loading, empty, unavailable, read-only, error, and save states;
- dense desktop/tablet keyboard-friendly entry.

This unit does not introduce:

- submit for review;
- request correction;
- verify;
- report archive;
- report restore;
- report review history;
- report comments;
- report workflow management UI;
- lineup editing changes;
- tactical positions;
- player-position modeling;
- automatic goalkeeper detection;
- team-level match statistics;
- GPS or physical metrics;
- media;
- imports;
- audit history;
- charts;
- dashboards;
- bulk import or clipboard-paste grid behavior.

Report transition actions remain owned by Unit 37.

### Existing backend contracts remain authoritative

Use the existing endpoints:

```txt
GET  /api/matches/{matchId}/report
POST /api/matches/{matchId}/report

GET /api/match-reports/{reportId}/statistics
PUT /api/match-reports/{reportId}/statistics
```

Do not add:

- per-player statistics mutation endpoints;
- per-cell mutation endpoints;
- separate goalkeeper save endpoints;
- frontend-generated statistics record IDs;
- frontend-generated appearance IDs;
- a generic report-status mutation.

The statistics save must remain one atomic snapshot.

### Statistics availability by match state

Statistics are report-owned data for a played match.

#### `PLAYED` and not archived

The statistics tab may:

- create a draft report when no visible report exists and the backend authorizes the caller;
- load statistics when a report exists;
- allow editing only when the statistics response says the caller may edit.

#### `SCHEDULED`

Show a concise unavailable state:

```txt
Statistika se unosi nakon što je utakmica označena kao odigrana.
```

Do not offer report creation or statistics editing.

#### `POSTPONED`

Show a concise unavailable state explaining that statistics cannot be entered until the match is played.

Do not clear any unrelated match or lineup data.

#### `CANCELLED`

Show a read-only unavailable state.

Do not offer report creation or statistics editing.

#### Archived match

If an accessible report/statistics snapshot exists, show it read-only.

If no accessible report exists, show an unavailable state.

Do not offer draft report creation or statistics editing for an archived match.

The backend remains authoritative for all status and archive checks.

### Minimal draft report initialization

The Unit 33 statistics API requires a `reportId`, while no previous frontend unit creates a report.

Unit 36 must therefore expose only the minimal prerequisite action for authorized data-entry users:

```txt
Započni izvještaj
```

Behavior:

- show only for a `PLAYED`, non-archived match when no visible report exists;
- show only when the current session clearly indicates `ADMIN` or in-scope `DATA_OPERATOR`;
- call the existing `POST /api/matches/{matchId}/report`;
- do not expose report status choices;
- the backend creates the report as `DRAFT`;
- on success, refetch the match report and statistics;
- on duplicate/conflict caused by concurrent creation, refetch the report and continue when it is now accessible;
- show a clear safe error when creation is rejected;
- do not automatically submit the report;
- do not automatically create a report merely by opening the tab.

Creating the draft report is a prerequisite for statistics entry, not the Unit 37 report workflow interface.

Users who cannot create a report receive a neutral unavailable state without disclosure of hidden draft/review data.

Suggested copy:

```txt
Izvještaj utakmice još nije dostupan.
```

### Permission and workflow-aware behavior

Use backend response data as the source of truth.

The Unit 33 statistics response exposes whether the current caller may edit statistics.

Use that `mayEdit` value rather than reproducing all role, team-scope, report-status, match-status, and archive rules in the frontend.

Frontend role/session information may be used only to avoid clearly impossible actions such as showing `Započni izvještaj` to a read-only role.

Expected states:

#### Editable

Statistics may be edited when the backend response permits it, normally for:

- `DRAFT`;
- `NEEDS_CORRECTION`;
- authorized `ADMIN`;
- authorized in-scope `DATA_OPERATOR`.

#### Read-only

Statistics are read-only when:

- report is `READY_FOR_REVIEW`;
- report is `VERIFIED`;
- report is `ARCHIVED`;
- caller lacks mutation permission;
- match is archived;
- backend otherwise returns `mayEdit = false`.

Do not add an admin bypass.

Display a concise reason when it can be derived safely from visible report/match data.

Suggested locked message:

```txt
Statistika je zaključana zbog trenutnog statusa izvještaja utakmice.
```

If a save returns a stale workflow-lock `409`:

- preserve unsaved values until the user acknowledges the conflict;
- show a clear Bosnian Latin message;
- exit or disable stale edit mode;
- refetch report and statistics data;
- do not force the rejected values into the query cache;
- do not expose request-correction actions in this unit.

### `Statistika` tab integration

Replace the Unit 34 `Statistika` placeholder inside the existing match detail shell.

Do not create a second match detail page or duplicate:

- match header;
- breadcrumbs;
- top-level match tabs;
- match metadata actions.

Keep the other future tabs unchanged.

The statistics tab should contain:

```txt
Statistics Header
Completeness Summary
Player / Goalkeeper sub-tabs
Statistics Content
Edit Actions
```

Recommended header content:

- title;
- applied/resolved tracking-level badge;
- report status badge when safely visible;
- overall statistics completeness state;
- edit action when `mayEdit = true`.

Suggested title:

```txt
Statistika igrača
```

Do not show submit, verify, correction, or archive actions.

### Tracking level is backend-owned

The backend response provides:

- resolved/applied tracking level;
- enabled player field codes;
- enabled goalkeeper field codes.

These values are the sole source of truth for which statistics inputs are visible and accepted.

Do not hardcode frontend logic such as:

```txt
if BASIC show ...
if STANDARD show ...
if FULL show ...
```

Do not duplicate the Unit 33 tracking-level matrix in:

- React components;
- Zod schemas;
- table definitions;
- route logic;
- role logic.

The frontend may maintain one central metadata registry for known stable field codes.

The registry may define only presentation metadata such as:

- Bosnian label;
- short table label;
- description/tooltip;
- display group;
- input type;
- deterministic display order.

Example concept:

```ts
interface StatisticFieldDefinition {
  code: StatisticFieldCode;
  label: string;
  shortLabel: string;
  group: StatisticFieldGroup;
  inputType: "nonNegativeInteger" | "nullableBoolean";
}
```

The backend-enabled field arrays determine which registry entries are rendered.

If the backend returns an unknown field code:

- do not silently submit or discard it;
- show a safe unsupported-field error state;
- prevent editing/saving until frontend support is added;
- document the mismatch in `context/progress-tracker.md`.

### Tracking-level labels

Localize the visible tracking level:

```txt
BASIC -> Osnovni
STANDARD -> Standardni
FULL -> Puni
```

Internal API values remain English.

The tracking-level badge is informational.

Do not allow changing the tracking level from the statistics tab.

Do not imply that changing the team's current tracking level will change the already applied historical report level.

### Statistics field presentation registry

Create a centralized Matches-feature statistics field registry.

Suggested Bosnian labels:

#### Player fields

```txt
goals -> Golovi
assists -> Asistencije
yellowCards -> Žuti kartoni
redCards -> Crveni kartoni
shots -> Šutevi
shotsOnTarget -> Šutevi u okvir
passesAttempted -> Pokušaji dodavanja
passesCompleted -> Tačna dodavanja
keyPasses -> Ključna dodavanja
duelsAttempted -> Pokušani dueli
duelsWon -> Dobijeni dueli
foulsCommitted -> Napravljeni prekršaji
foulsWon -> Iznuđeni prekršaji
offsides -> Ofsajdi
ballRecoveries -> Osvojene lopte
possessionLosses -> Izgubljene lopte
```

#### Goalkeeper fields

```txt
saves -> Odbrane
goalsConceded -> Primljeni golovi
cleanSheet -> Sačuvana mreža
penaltySaves -> Odbranjeni penali
```

Use concise short labels in dense headers only when needed.

Every abbreviated header must provide its full label through visible supporting text or a shadcn `Tooltip`.

Do not display raw field codes.

### Statistics grouping

The field registry may group enabled player columns for readability.

Suggested presentation groups:

```txt
Učinak
Disciplina
Šutevi
Dodavanja
Dueli
Prekršaji i ostalo
```

Goalkeeper fields may use:

```txt
Golmanski učinak
```

Grouping is presentational only.

Do not use groups to determine whether a field is enabled or required.

### Player statistics table

Use TanStack Table for table structure/column generation and shadcn table primitives for rendering.

The player statistics snapshot must contain exactly one row for every appearance returned by the Unit 33 statistics response.

Do not use:

- the current team player list;
- the eligible-player list;
- lineup players without appearances;
- unused substitutes;
- manually created rows.

The backend appearance summaries are authoritative.

Recommended fixed columns:

- player;
- starter/substitute origin when available;
- minutes played when provided by the appearance summary;
- row completeness status.

Dynamic columns come only from `enabledPlayerFields`.

Table behavior:

- preserve the backend appearance order unless the contract explicitly supplies another deterministic order;
- keep the player column sticky on desktop/tablet when practical;
- allow horizontal scrolling inside a contained area;
- keep grouped/table headers visible when practical;
- use compact density appropriate for data entry;
- do not add user sorting that would make row-to-row entry less predictable unless an existing shared table pattern requires it;
- do not paginate the appearance grid;
- do not hide enabled columns through a generic column-visibility control.

The entire current appearance set should be visible in one statistics snapshot.

### Player statistics read mode

In read mode:

- show non-null values as numbers;
- preserve and display zero as `0`;
- show `null` as a clear missing value, such as an em dash with an accessible `Nije uneseno` label/tooltip;
- show row completeness with text and a badge/icon;
- do not rely only on color;
- keep player identity visible during horizontal scrolling.

Suggested row status labels:

```txt
Kompletno
Nedostaju podaci
```

Do not show editable inputs when `mayEdit = false`.

### Player statistics edit mode

Use React Hook Form with Zod.

Each enabled numeric cell uses an approved shadcn `Input`.

Requirements:

- empty input maps to `null`;
- entered `0` maps to numeric zero;
- positive integers remain numbers;
- decimal values are invalid;
- negative values are invalid;
- use `inputMode="numeric"` or another accessible numeric-entry approach;
- do not automatically replace empty fields with zero;
- do not auto-save per cell;
- tab order should move predictably through the grid;
- preserve values while switching between player and goalkeeper sub-tabs;
- keep one form source of truth.

Do not create one React Query mutation per row or cell.

### Player relationship validation

Provide immediate frontend feedback for the unambiguous Unit 33 relationships when both fields are enabled and non-null:

```txt
shotsOnTarget <= shots
passesCompleted <= passesAttempted
duelsWon <= duelsAttempted
```

Validation must be generated from enabled fields and the central field registry/support utilities.

Do not invent additional relationships.

Show errors:

- on the relevant cells;
- in the player row;
- in a form-level summary when multiple rows fail.

Backend validation remains authoritative.

### Goalkeeper statistics section

Use a separate shadcn `Tabs` sub-tab or clearly separated section:

```txt
Igrači
Golmani
```

The goalkeeper snapshot contains zero or more appearance-linked rows.

Because the project does not yet model player positions, the UI must not guess the goalkeeper from:

- lineup order;
- name;
- shirt number;
- statistics;
- team assignment.

Authorized editors explicitly select appearances that should have goalkeeper statistics.

### Adding a goalkeeper row

In edit mode, provide:

```txt
Dodaj golmana
```

Use a shadcn `Combobox`, `Command` + `Popover`, or another approved searchable selection primitive.

Candidate choices:

- come only from current appearance summaries returned by the statistics response;
- exclude appearances already used in a goalkeeper row;
- include starters and substitute appearances;
- do not include unused substitutes or non-appearance players.

Adding a goalkeeper row:

- references the existing `PlayerMatchAppearanceId`;
- initializes enabled goalkeeper fields as `null`;
- does not create a new player or appearance;
- does not assign a persistent player position.

Multiple goalkeeper rows are allowed.

### Removing a goalkeeper row

Authorized editors may remove a goalkeeper row.

Because omission from the atomic save removes the persisted goalkeeper record:

- require explicit confirmation when the row contains any entered or persisted value;
- explain that the goalkeeper-specific statistics will be removed;
- do not remove the ordinary player statistics row;
- do not remove the appearance;
- do not change lineup data.

Use shadcn `Alert Dialog` for destructive confirmation.

### Goalkeeper inputs

Render only `enabledGoalkeeperFields`.

Numeric fields:

- empty maps to `null`;
- zero is valid;
- non-negative integer validation applies.

`cleanSheet` is a nullable boolean.

Do not use a simple two-state checkbox that collapses `null` and `false`.

Use an accessible tri-state representation such as shadcn `Select` with:

```txt
Nije uneseno
Da
Ne
```

Map to:

```txt
null
true
false
```

Do not derive `cleanSheet` from `goalsConceded`.

### Goalkeeper completeness

While editing, zero goalkeeper rows are allowed by the save API.

However, report submission later requires:

- at least one goalkeeper row;
- every enabled goalkeeper field completed on each persisted goalkeeper row.

Unit 36 should therefore show a non-blocking completeness warning when:

- no goalkeeper row exists;
- a goalkeeper row is incomplete.

Do not block saving an incomplete editable snapshot.

Do not add a submit-for-review action.

Suggested warning:

```txt
Za slanje izvještaja na pregled potrebno je unijeti najmanje jednu kompletnu golmansku statistiku.
```

### Completeness summary

Use the response's row completeness and overall statistics completeness as authoritative after load/save.

While the user edits, calculate a local preview using:

- enabled player fields;
- enabled goalkeeper fields;
- current form values;
- the same explicit completeness concepts documented by Unit 33.

The local preview is UX feedback only.

Do not use the local preview to bypass the backend.

Recommended summary content:

- overall complete/incomplete state;
- completed player rows out of total appearances;
- completed goalkeeper rows;
- warning when no goalkeeper row exists;
- tracking level.

A shadcn `Progress` component may be used for a calculated entry-progress indicator.

Label it clearly as statistics completeness, not overall report readiness.

Suggested copy:

```txt
Kompletnost statistike
```

Do not imply that GPS, media, or other future modules are required.

### Save behavior

Use one mutation:

```txt
PUT /api/match-reports/{reportId}/statistics
```

Construct the snapshot from current form state.

`PlayerStatistics`:

- contains every current appearance exactly once;
- identifies each row by backend-provided `PlayerMatchAppearanceId`;
- contains only fields enabled by the backend response;
- preserves `null` and zero distinctly.

`GoalkeeperStatistics`:

- contains the complete current goalkeeper row set;
- identifies each row by backend-provided `PlayerMatchAppearanceId`;
- contains only fields enabled by the backend response;
- omission intentionally removes a previously persisted goalkeeper record after confirmation.

Do not send unknown or disabled field values.

Do not send display labels, local row status, or UI-only metadata.

### Save lifecycle

Use explicit edit mode.

Suggested actions:

```txt
Uredi statistiku
Sačuvaj statistiku
Odustani
```

On edit start:

- initialize the form from the latest statistics response;
- do not mutate the TanStack Query cache.

On successful save:

- keep the report in its existing workflow status;
- exit edit mode;
- show Bosnian success feedback through the approved toast system;
- replace/refetch the statistics query with the authoritative response;
- invalidate report detail/readiness data when necessary;
- do not navigate away automatically;
- do not submit the report.

On normal validation error:

- keep edit mode open;
- preserve entered values;
- map backend field/appearance errors to rows/cells where possible;
- show a form-level summary.

On authorization/workflow conflict:

- preserve a recoverable copy of unsaved values until the user acknowledges the conflict;
- show clear feedback;
- refetch report/statistics data;
- disable stale saving;
- do not overwrite authoritative server data.

On appearance-snapshot mismatch caused by concurrent lineup changes:

- explain that the lineup/appearance set changed;
- do not silently merge rows;
- offer an explicit reload action;
- warn that removed-appearance unsaved values cannot be applied to the new snapshot;
- refetch lineup-related statistics data after confirmation.

### Null versus zero

This distinction is non-negotiable.

The UI must preserve:

```txt
empty input -> null -> not entered
0 -> zero -> deliberately entered
```

Do not use truthiness checks such as:

```ts
value || null
```

Use explicit parsing and formatting helpers.

Tests or verification should cover:

- empty;
- zero;
- positive integer;
- invalid decimal;
- invalid negative value.

### Unsaved-change protection

Statistics editing can involve many cells.

When the form is dirty and the user attempts to:

- leave edit mode;
- change the top-level match tab;
- navigate away;
- close a confirmation surface;
- start a conflicting reload;

show an explicit discard confirmation where supported by the existing router/application patterns.

Use an approved shadcn confirmation component.

Suggested copy:

```txt
Imate nesačuvane promjene statistike. Želite li ih odbaciti?
```

Do not introduce a broad new routing framework solely for this guard.

If full browser-navigation interception is not supported by the current router, protect in-app edit cancellation/tab changes and document the remaining limitation in `context/progress-tracker.md`.

### Loading, empty, unavailable, and error states

Use shadcn-first patterns and existing app-level wrappers.

Prefer:

- `Skeleton` for initial grid loading;
- `Empty` for no report/no appearances/unavailable states;
- `Alert` for query and validation problems;
- `Sonner` for succinct mutation feedback;
- `Badge` for tracking, workflow, and completeness states;
- `Progress` for local completeness visualization;
- `Tooltip` for dense field labels;
- `Alert Dialog` for destructive/discard confirmations.

Required states:

#### No visible report

- authorized data-entry user on a played non-archived match: show `Započni izvještaj`;
- other users: show neutral unavailable message.

#### Report exists but no appearances

Show:

```txt
Prije unosa statistike potrebno je evidentirati nastupe igrača u tabu Sastav.
```

Do not render an empty editable statistics grid.

Do not offer fake player rows.

#### Query error

Show a readable error and retry action.

#### Unsupported field code

Show a blocking compatibility error.

Do not allow save.

#### Locked report

Show statistics read-only and explain the lock when safe.

### Responsive data-entry behavior

Desktop and tablet are primary.

Requirements:

- player identity column remains visible when practical;
- dynamic metric columns use contained horizontal scrolling;
- headers remain readable;
- table density supports many appearances and up to the FULL field set;
- save/edit actions remain reachable;
- no page-wide uncontrolled horizontal overflow;
- inputs have adequate touch targets on tablet;
- mobile supports clean read mode and limited editing through contained horizontal scroll;
- do not build a separate mobile statistics product;
- do not hide required enabled metrics on mobile;
- do not convert the entire FULL grid into dozens of disconnected modal screens.

### Accessibility and keyboard behavior

Requirements:

- semantic table markup is preserved;
- every input has an accessible name containing player and metric;
- abbreviated headers have full tooltip/accessible text;
- keyboard tab order is predictable;
- focus indicators remain visible;
- numeric errors are associated with their cell inputs;
- row completeness does not rely only on color;
- add/remove goalkeeper actions have accessible names;
- tri-state clean-sheet control is keyboard accessible;
- destructive/discard confirmations return focus appropriately;
- locked/read-only controls are not presented as editable;
- horizontal scrolling does not trap keyboard focus.

### Frontend architecture and organization

Keep statistics UI in the Matches feature.

Suggested organization:

```txt
frontend/src/features/matches/
├── api/
├── components/
│   └── statistics/
├── hooks/
├── schemas/
├── types/
├── utils/
│   └── statistics/
└── index.ts
```

Use:

- TanStack Query for report/statistics server state;
- React Hook Form for editable snapshot state;
- Zod for dynamic statistics validation;
- TanStack Table for the player statistics grid;
- shadcn/ui primitives for rendering and interaction;
- the existing API client and ProblemDetails handling.

Do not store statistics server data in Zustand or React Context.

Do not create a global statistics store.

Use explicit TypeScript contracts aligned with backend DTOs.

Avoid `any`.

### Query keys and invalidation

Define or reuse stable query keys for:

- match report by match ID;
- statistics by report ID;
- match detail when needed for status/archive state.

Draft report creation should invalidate/refetch:

- report by match;
- statistics after the report ID is available.

Statistics save should update/refetch:

- statistics by report;
- report detail/readiness data when applicable.

Do not broadly clear all match queries.

Do not optimistically mark incomplete rows complete before backend success.

### Localization

Visible copy is Bosnian Latin with proper characters:

- `č`;
- `ć`;
- `š`;
- `ž`;
- `đ`.

Do not mix English and Bosnian visible labels on the operational screen.

Internal codes remain English.

Suggested UI copy includes:

```txt
Statistika
Statistika igrača
Igrači
Golmani
Uredi statistiku
Sačuvaj statistiku
Odustani
Započni izvještaj
Nivo praćenja
Kompletnost statistike
Kompletno
Nedostaju podaci
Nije uneseno
Dodaj golmana
Ukloni golmansku statistiku
Sačuvana mreža
Nema evidentiranih nastupa
Statistika je zaključana
```

Use centralized label utilities.

Do not scatter raw field-code translations across cells/components.

### Shadcn-first component selection

Review all new interactions against:

```txt
context/references/shadcn-components.md
```

Prefer suitable components such as:

- `Tabs`;
- `Table`;
- `Input`;
- `Select`;
- `Combobox`;
- `Command`;
- `Popover`;
- `Card`;
- `Badge`;
- `Progress`;
- `Alert`;
- `Alert Dialog`;
- `Empty`;
- `Skeleton`;
- `Scroll Area`;
- `Tooltip`;
- `Sonner`;
- `Button`;
- `Button Group` where it improves the action layout.

Do not install or use components only because they are available.

Do not modify generated shadcn primitives.

Do not add ad-hoc visual overrides.

### Tests and verification approach

Add frontend tests only if the repository already has an approved frontend testing foundation by implementation time.

If tests exist, prioritize:

- null versus zero parser/serializer;
- dynamic enabled-field rendering;
- relationship validation;
- atomic payload construction;
- nullable clean-sheet mapping;
- goalkeeper row add/remove behavior;
- unsupported field handling;
- local completeness calculation.

Do not introduce a new testing framework solely for Unit 36.

Manual verification must cover:

- BASIC field rendering;
- STANDARD field rendering;
- FULL field rendering;
- field rendering driven by backend arrays rather than tracking-level conditionals;
- draft report creation;
- no-report read-only user behavior;
- no-appearance behavior;
- editable `DRAFT`;
- editable `NEEDS_CORRECTION`;
- read-only `READY_FOR_REVIEW`;
- read-only `VERIFIED`;
- read-only `ARCHIVED`;
- player grid entry;
- zero preservation;
- null preservation;
- relationship validation;
- adding multiple goalkeeper rows;
- nullable clean-sheet entry;
- incomplete save;
- complete save;
- goalkeeper row removal confirmation;
- workflow-lock conflict;
- concurrent lineup/appearance mismatch;
- unsaved-change confirmation;
- desktop/tablet horizontal grid behavior;
- mobile read usability;
- keyboard navigation.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

If implementation reveals that Unit 33 response contracts, enabled-field behavior, completeness behavior, report creation prerequisites, or workflow locks differ from this spec, update the relevant context/spec before continuing.

Do not silently add report transition actions, new statistics fields, team statistics, automatic goalkeeper detection, or backend changes.

## Implementation

### 1. Add report and statistics frontend contracts

Create typed contracts for:

- report lookup by match;
- draft report creation response;
- statistics response;
- player statistics rows;
- goalkeeper statistics rows;
- enabled field codes;
- completeness metadata;
- `mayEdit`;
- atomic statistics save request.

Reuse existing Unit 32 report types where already present.

Do not duplicate conflicting report DTO definitions.

### 2. Add the statistics field registry

Create one central presentation registry for all known Unit 33 player and goalkeeper field codes.

Include:

- Bosnian label;
- short label;
- tooltip/description where useful;
- display group;
- input type;
- display order.

Do not encode BASIC/STANDARD/FULL membership.

### 3. Add TanStack Query hooks

Implement/reuse hooks for:

- report by match;
- create draft report;
- statistics by report;
- save statistics.

Use focused query keys and invalidation.

### 4. Replace the `Statistika` placeholder

Integrate real content into the existing Unit 34 match detail shell.

Handle:

- match states;
- no report;
- draft report creation;
- no appearances;
- loading;
- errors;
- read-only statistics;
- edit action.

Do not duplicate the page shell.

### 5. Build the completeness header

Show:

- tracking level;
- report status when visible;
- overall statistics completeness;
- player row progress;
- goalkeeper completeness warning;
- edit action when allowed.

Use backend completeness after load/save and local preview while editing.

### 6. Build the player statistics grid

Use TanStack Table and shadcn table primitives.

Generate dynamic metric columns from `enabledPlayerFields`.

Implement:

- sticky player identity where practical;
- grouped headers;
- read mode;
- edit inputs;
- row completeness;
- horizontal scrolling;
- accessible labels;
- deterministic row order.

### 7. Build dynamic validation and payload helpers

Use React Hook Form and Zod.

Implement:

- empty-to-null parsing;
- zero preservation;
- non-negative integer validation;
- enabled-field-only schemas;
- explicit relationship validation;
- atomic payload serialization;
- unsupported field protection.

### 8. Build goalkeeper statistics management

Implement:

- goalkeeper sub-tab/section;
- add goalkeeper from current appearances;
- multiple goalkeeper rows;
- dynamic enabled fields;
- nullable clean-sheet control;
- completeness feedback;
- removal confirmation;
- complete snapshot serialization.

Do not infer player positions.

### 9. Wire draft report initialization

Show `Započni izvještaj` only in the supported prerequisite state.

On success or duplicate conflict:

- refetch report;
- load statistics;
- do not perform any report transition beyond `DRAFT` creation.

### 10. Wire atomic save and conflict handling

Submit one Unit 33 statistics snapshot.

Handle:

- success;
- backend validation;
- workflow lock;
- authorization changes;
- appearance-set mismatch;
- unsupported field mismatch;
- authoritative refetch.

Do not auto-submit the report.

### 11. Add unsaved-change protection

Protect:

- edit cancellation;
- match-tab changes where supported;
- in-app navigation where supported;
- destructive goalkeeper-row removal;
- reload after appearance mismatch.

Use approved confirmation primitives.

### 12. Verify shadcn-first implementation

Review all new controls against the component reference.

Remove unnecessary hand-built primitives.

Confirm generated `frontend/src/components/ui/*` files remain unmodified.

### 13. Update progress documentation

Update `context/progress-tracker.md` with actual implementation and verification results.

Do not mark Unit 36 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use the packages already introduced by previous frontend units:

- TanStack Query;
- React Hook Form;
- Zod;
- TanStack Table;
- shadcn/ui;
- Lucide React.

Add required shadcn components just in time through the approved CLI workflow.

Do not add:

- a spreadsheet/grid package;
- a second table library;
- a second form library;
- a second query/state library;
- a clipboard-paste dependency;
- a position-detection library;
- a new frontend testing framework solely for this unit.

## Verification checklist

- [ ] The existing match detail `Statistika` placeholder is replaced with real statistics content.
- [ ] No second match detail shell or duplicate top-level match tabs are created.
- [ ] `SCHEDULED`, `POSTPONED`, and `CANCELLED` matches show correct unavailable/read-only states.
- [ ] Archived matches do not offer report creation or statistics editing.
- [ ] A played non-archived match with no report can show `Započni izvještaj` to authorized data-entry users.
- [ ] Draft report initialization uses the existing Unit 32 endpoint.
- [ ] Draft report creation does not submit, verify, request correction, or archive the report.
- [ ] Read-only users receive a neutral no-report/unavailable state.
- [ ] Statistics load through `GET /api/match-reports/{reportId}/statistics`.
- [ ] Statistics save through one atomic `PUT /api/match-reports/{reportId}/statistics`.
- [ ] No per-row or per-cell mutation endpoints are added.
- [ ] `mayEdit` from the backend response controls edit availability.
- [ ] `DRAFT` and `NEEDS_CORRECTION` are editable only when the backend permits it.
- [ ] `READY_FOR_REVIEW`, `VERIFIED`, and `ARCHIVED` are read-only.
- [ ] No admin workflow-lock bypass is introduced.
- [ ] Enabled player fields come from the backend response.
- [ ] Enabled goalkeeper fields come from the backend response.
- [ ] The frontend does not duplicate the BASIC/STANDARD/FULL field matrix.
- [ ] The central field registry contains presentation metadata only.
- [ ] Unknown backend field codes block editing safely instead of being silently ignored.
- [ ] Tracking-level labels are localized.
- [ ] The player grid uses current appearance summaries only.
- [ ] Unused substitutes and unrelated players do not receive player-stat rows.
- [ ] Every atomic player snapshot contains each current appearance exactly once.
- [ ] Dynamic columns use TanStack Table and shadcn table primitives.
- [ ] Player identity remains readable during horizontal scrolling.
- [ ] Empty numeric inputs serialize to `null`.
- [ ] Entered zero serializes and displays as `0`.
- [ ] Decimal and negative values are rejected.
- [ ] `shotsOnTarget <= shots` is validated when applicable.
- [ ] `passesCompleted <= passesAttempted` is validated when applicable.
- [ ] `duelsWon <= duelsAttempted` is validated when applicable.
- [ ] No speculative statistics relationship is added.
- [ ] Goalkeeper rows reference existing appearances only.
- [ ] Multiple goalkeeper rows are supported.
- [ ] Goalkeeper selection does not infer player position.
- [ ] Duplicate goalkeeper rows for the same appearance are prevented.
- [ ] `cleanSheet` preserves `null`, `true`, and `false` distinctly.
- [ ] Removing a populated goalkeeper row requires explicit confirmation.
- [ ] Saving with zero goalkeeper rows is allowed while editing.
- [ ] Missing/incomplete goalkeeper data displays a report-submission readiness warning.
- [ ] Incomplete statistics may still be saved in editable workflow states.
- [ ] Overall and row completeness feedback is displayed.
- [ ] Completeness UI is labeled as statistics completeness, not total report readiness.
- [ ] No report transition actions are shown in Unit 36.
- [ ] Successful save preserves the current report workflow status.
- [ ] Backend validation errors map to cells, rows, or a form summary where possible.
- [ ] Workflow-lock `409` responses disable stale editing and refetch authoritative data.
- [ ] Appearance-set mismatch caused by lineup changes is handled without silent merging.
- [ ] Dirty statistics edits require discard confirmation for supported in-app exits.
- [ ] Loading, no-report, no-appearance, unavailable, error, unsupported-field, locked, and success states are implemented.
- [ ] Desktop/tablet statistics entry remains usable with the FULL field set.
- [ ] Mobile read mode remains clean and the entry grid has contained horizontal scrolling.
- [ ] Every input has an accessible player-and-metric name.
- [ ] Abbreviated headers expose full labels.
- [ ] Row completeness does not rely only on color.
- [ ] Keyboard navigation and focus indicators are preserved.
- [ ] New UI follows shadcn-first component-selection guidance.
- [ ] Generated `frontend/src/components/ui/*` files are not manually modified.
- [ ] No raw Tailwind palette classes or hardcoded component colors are introduced.
- [ ] No ad-hoc visual overrides are applied to shadcn components.
- [ ] Frontend imports use `@/` instead of deep relative paths.
- [ ] Visible Bosnian copy uses proper Bosnian Latin characters.
- [ ] No backend implementation files, contracts, migrations, or authorization rules are changed.
- [ ] No new statistics fields, team statistics, GPS, media, import, audit, chart, or dashboard behavior is added.
- [ ] `npm run format` completes successfully.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] Frontend typecheck/build passes using the configured project command.
- [ ] Existing approved frontend tests pass when present.
- [ ] `context/progress-tracker.md` reflects the actual Unit 36 implementation and verification state.
