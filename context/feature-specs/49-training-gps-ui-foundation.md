# Unit 49: Training GPS UI Foundation

## Goal

Build the complete first frontend experience for training sessions and canonical physical workload data using Unit 48. Add a protected `Treninzi i GPS` module, training-session list/detail and lifecycle flows, date-eligible participant management, contextual `TRAINING_GPS` import creation, session import history, comparison-safe workload tables, controlled physical trend visualization, match GPS/physical tab integration, and a real player physical-history section.

Add only two minimal backend read-query extensions needed to keep eligibility and contextual import filtering authoritative:

```txt
GET /api/training-sessions/{trainingSessionId}/participant-candidates
GET /api/imports?trainingSessionId={trainingSessionId}
```

Do not add vendor-specific mappings, manual metric entry, fake Gpexe/Zone14 confirmation actions, client-side workload aggregation across incompatible contexts, new persistence tables, or an EF Core migration.

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
10. `context/feature-specs/28-player-team-assignment-backend.md`
11. `context/feature-specs/29-players-ui-foundation.md`
12. `context/feature-specs/31-match-lineup-appearance-backend.md`
13. `context/feature-specs/34-matches-ui-foundation.md`
14. `context/feature-specs/39-audit-ui-foundation.md`
15. `context/feature-specs/44-import-ui-foundation.md`
16. `context/feature-specs/46-gpexe-sample-review-mapping-gate.md`
17. `context/feature-specs/48-training-gps-backend-foundation.md`
18. `context/feature-specs/49-training-gps-ui-foundation.md`

Use relevant frontend skills from `frontend/.agents/` and installed backend Codex skills/plugins when applicable.

Before creating custom UI primitives or interaction patterns:

- check `context/references/shadcn-components.md`;
- prefer suitable shadcn/ui components;
- install required shadcn components just in time through the approved workflow;
- compose app-level components outside `frontend/src/components/ui`;
- do not manually modify generated shadcn primitive files;
- do not hand-build equivalents of existing shadcn primitives without a documented reason.

This unit is primarily frontend work with two narrowly scoped backend read-query additions.

Do not change:

- training-session persistence or lifecycle;
- participant mutation semantics;
- canonical metric codes or units;
- threshold/method comparability rules;
- workload revision behavior;
- import confirmation rules;
- permissions;
- audit codes;
- vendor processor registrations;
- storage behavior;
- EF Core schema.

No migration is expected in Unit 49.

### Scope

This unit introduces:

- protected `/training-sessions` route;
- protected `/training-sessions/:trainingSessionId` route;
- `Treninzi i GPS` Performance navigation;
- URL-driven training filters and pagination;
- training-session table/list;
- create and edit training dialogs;
- explicit complete and cancel actions;
- participant list;
- date-eligible participant candidate search;
- participant add/remove flows;
- session overview and lifecycle summaries;
- session-linked import-job history;
- contextual `TRAINING_GPS` source upload;
- canonical metric catalogue loading;
- workload presentation registry;
- current training workload display;
- exact comparability-group selection;
- threshold and method context display;
- safe ranking/sorting only within one comparability group;
- current revision and import provenance display;
- training-session audit history;
- a real match `GPS / Fizički podaci` tab;
- contextual `MATCH_GPS` import creation from match detail;
- a real player `Fizički podaci` section;
- player workload history filters;
- a trend chart restricted to one metric and one exact comparability key;
- loading, empty, unsupported-vendor, incompatible-comparison, locked, conflict, and terminal states.

This unit does not introduce:

- Gpexe exact preview, validation, or confirmation;
- Zone14 exact preview, validation, or confirmation;
- guessed vendor headers or fields;
- manual physical metric entry;
- public workload mutation endpoints;
- automatic participant creation from player names;
- client-side CSV/XLSX parsing;
- client-side confirmation;
- automatic import confirmation;
- workload corrections outside approved import revisions;
- cross-threshold or cross-method averaging;
- team/session aggregate persistence;
- dashboard read models;
- medical data;
- wellness, RPE, injury, attendance reasons, drills, or coaching plans;
- training media attachments;
- training-session deletion or reopening;
- import rollback;
- new database tables or columns;
- a new charting library.

### Required UI-context synchronization

Update `context/ui-context.md` so visible default-language labels reflect the implemented module.

Change the Performance navigation example from:

```txt
Training GPS
```

to:

```txt
Treninzi i GPS
```

Update the match detail visible label to the established Bosnian form:

```txt
GPS / Fizički podaci
```

Update the player-profile example so `Fizički podaci` is no longer a future placeholder after Unit 49.

Keep internal route keys and API codes in English.

Do not rewrite unrelated page patterns.

### Minimal participant-candidate endpoint

Add:

```txt
GET /api/training-sessions/{trainingSessionId}/participant-candidates
```

Supported query parameters:

```txt
search
page
pageSize
```

Purpose:

- provide authoritative players eligible on the session's immutable/current `SessionDate`;
- avoid using Unit 28's current-assignment-only list filter;
- exclude players already actively linked to the session;
- provide safe player summaries for the add-participant dialog.

Authorization:

- authenticated active user;
- session must be accessible;
- caller must currently be able to add participants:
  - `ADMIN`; or
  - in-scope `DATA_OPERATOR`;
- cancelled session returns a workflow conflict or no mutation capability according to existing conventions;
- read-only roles receive `403`;
- inaccessible session uses established safe behavior.

Candidate rules:

- player exists;
- player is not archived;
- one assignment to the session team covers `SessionDate`;
- historical, current, or future assignment records are evaluated relative to `SessionDate`;
- current-day assignment state is not used as a substitute;
- active participants are excluded;
- removed historical participant links do not permanently exclude re-adding;
- database pagination and normalized search;
- deterministic ordering by display name then player ID;
- no N+1 assignment/player queries.

Return at minimum:

```txt
id
displayName
preferredName
dateOfBirth
eligibleAssignment:
  id
  teamId
  startDate
  endDate
previouslyParticipated
```

`previouslyParticipated` may indicate removed history but must not change eligibility.

The existing participant POST endpoint remains authoritative.

A player may become ineligible between candidate read and mutation; the POST must still revalidate.

Add focused backend tests.

### Training-session import filter

Extend the existing Unit 43 list query:

```txt
GET /api/imports
```

with optional:

```txt
trainingSessionId
```

Rules:

- only valid for the explicit Unit 48 relation;
- target session visibility/team scope is validated;
- the import job's team must match the session team by existing invariants;
- combines with existing status/type/source/file/date filters through normal AND semantics;
- database-backed;
- deterministic pagination/order remains unchanged;
- inaccessible target does not leak import existence;
- no new endpoint;
- no mutation change;
- no migration.

Update typed backend/frontend query contracts and add focused tests.

### Routes and navigation

Add:

```txt
/training-sessions
/training-sessions/:trainingSessionId
```

Visible navigation label:

```txt
Treninzi i GPS
```

Place it in the Performance group near Matches, Players, and Imports.

Navigation visibility:

- all authenticated active staff who may read at least one team-scoped session/workload area may see it;
- mutation permission is not required to see the module;
- selected-team users see only accessible teams and sessions;
- active state applies to list and detail routes;
- collapsed sidebar retains accessible label/tooltip.

Use a suitable Lucide training/activity icon.

Do not create separate top-level routes for:

- GPS metrics;
- workloads;
- Gpexe;
- Zone14.

### Training-session list page

Recommended layout:

```txt
PageHeader
Status tabs
Filter bar
Training-session table
Pagination
```

Suggested title:

```txt
Treninzi i GPS
```

Suggested description:

```txt
Vodite treninge, učesnike i potvrđene fizičke podatke po selekcijama.
```

Primary action for users who may create:

```txt
Novi trening
```

Action visibility:

- `ADMIN`;
- in-scope `DATA_OPERATOR`.

Other roles receive a read-only page.

### Training status labels

Centralize:

```txt
PLANNED -> Planiran
COMPLETED -> Završen
CANCELLED -> Otkazan
```

Do not expose raw codes.

Status badges must not rely only on color.

### Training status tabs

Use shadcn `Tabs` or the established app-level status tabs.

Suggested values:

```txt
Sve
Planirani
Završeni
Otkazani
```

Store the selected status in URL state.

### URL-driven training filters

Use `nuqs`.

Support:

```txt
search
teamId
status
dateFrom
dateTo
page
```

Rules:

- search uses the established debounce pattern;
- discrete filters update immediately;
- filter change resets page to 1;
- invalid values normalize safely;
- selected-team users are offered only authorized teams;
- filtering/pagination remains server-side;
- provide `Očisti filtere` when active;
- no client-side complete-list filtering;
- no season filter unless the backend later adds one.

### Training-session table

Use TanStack Table and shadcn table primitives.

Recommended columns:

- session date/time;
- title;
- team/selection;
- location;
- status;
- participant count;
- confirmed workload count;
- creator;
- actions.

Rules:

- date uses Bosnian date format;
- exact time is shown only when available;
- a session without exact time displays a clear fallback such as `Vrijeme nije uneseno`;
- participant and workload counts come from the compact backend response;
- no N+1 detail requests;
- row opens the detail route;
- action menu uses `Dropdown Menu`;
- high-impact complete/cancel actions may be available from the menu only when sufficient context and confirmation are provided; detail remains the primary workflow surface.

Do not show raw GPS metric values in the list.

### Training list states

#### No sessions

Show:

```txt
Još nema treninga.
```

Users with create permission receive `Novi trening`.

#### No filter matches

Show filter-specific empty state with `Očisti filtere`.

#### Loading

Use `Skeleton`.

#### Error

Use `Alert` with retry.

#### Inaccessible result/detail

Use established safe behavior without leaking hidden team/session existence.

### Create-training dialog

Use shadcn `Dialog`, React Hook Form, and Zod.

Fields:

```txt
team/selection
session date
exact-time toggle
start time when known
end time when known
title
location
description
```

Rules:

- team required and limited to mutation scope;
- session date required;
- exact start/end times optional;
- end requires start and must be later;
- no status selector;
- new session always `PLANNED`;
- no participant selection in the create form;
- no GPS file upload in the create transaction;
- no vendor selection;
- no hidden automatic completion;
- no hard-delete semantics.

Time handling:

- user enters local Sarajevo/application-local date/time;
- convert deterministically to backend UTC according to existing date utility conventions;
- preserve `SessionDate` separately;
- do not derive `SessionDate` from UTC conversion.

On success:

- close dialog;
- show success feedback;
- invalidate session lists;
- navigate to the created detail route.

### Edit-training dialog

Show according to backend `EDIT`.

Editable:

- planned:
  - session date;
  - optional start/end;
  - title;
  - location;
  - description;
- completed:
  - title;
  - location;
  - description;
- cancelled:
  - none.

Immutable:

```txt
id
team
creator
created time
status through generic edit
```

Rules:

- no direct status select;
- session-date change may fail because participants become ineligible;
- map backend conflict to a clear participant eligibility message;
- preserve form values after failure;
- no-op submission should be disabled or produce no misleading success.

### Complete-training action

Render only from backend `allowedActions`.

Use shadcn `Alert Dialog`.

Explain:

- session becomes terminal `Završen`;
- participants may still be corrected according to backend rules;
- official workload revisions require a completed session;
- session cannot be reopened.

Call:

```txt
POST /api/training-sessions/{id}/complete
```

On success:

- refetch detail/list/participants/workloads/audit;
- show success feedback;
- reveal contextual GPS import action when import permission exists.

Do not automatically create an import job.

### Cancel-training action

Render only from backend `allowedActions`.

Use shadcn `Alert Dialog`.

Explain:

- session becomes terminal `Otkazan`;
- source/import/audit history remains;
- no new participants or workload revisions may be added;
- no restore/reopen action exists.

Call:

```txt
POST /api/training-sessions/{id}/cancel
```

On success:

- refetch detail/list/participants/workloads/audit/import list;
- remove mutation actions.

Do not use HTTP delete.

### Training detail route

Recommended layout:

```txt
Training header
Context/status summary
Tabs
```

Tabs:

```txt
Pregled
Učesnici
GPS / Fizički podaci
Historija promjena
```

Use URL-backed tab state with the existing route/`nuqs` convention.

Suggested query:

```txt
tab=overview
tab=participants
tab=physical
tab=audit
```

Unknown tab values normalize safely.

### Training detail header

Show:

- title;
- date/time;
- team;
- status;
- location;
- participant count;
- workload count;
- creator;
- backend-provided actions.

Use a dropdown/action group for:

- edit;
- complete;
- cancel.

Do not infer actions solely from role/status.

### Overview tab

Show:

- session metadata;
- lifecycle actor/time information;
- description;
- participant summary;
- workload/import readiness summary.

Readiness summary may state:

- session planned/completed/cancelled;
- participants present/absent;
- confirmed workloads count;
- linked source import jobs count;
- active processor support state from import capabilities.

This summary is advisory.

Backend remains authoritative.

Do not claim that a generic raw preview means confirmed physical data exists.

### Participants tab

Load active participants from Unit 48.

Use a compact TanStack Table or shadcn `Item` list.

Recommended columns/content:

- player;
- current visible assignment context;
- added by/date;
- workload state;
- actions.

Show:

```txt
Dodaj učesnika
```

only when `ADD_PARTICIPANT` is present.

Show remove only when `REMOVE_PARTICIPANT` is present and the participant is removable according to returned state.

Do not use player-name text entry.

### Add-participant dialog

Use:

```txt
GET /api/training-sessions/{id}/participant-candidates
```

Provide:

- debounced search;
- backend pagination;
- loading/empty/error states;
- one explicit player selection;
- assignment eligibility summary;
- `Ranije bio uklonjen` indicator when returned.

Then call the existing Unit 48 participant POST endpoint with:

```txt
playerId
```

Rules:

- no current-team-only player query;
- no fuzzy matching;
- no bulk add in this unit;
- no auto-create assignment;
- backend POST revalidation remains authoritative.

On success:

- invalidate participants/session detail/list/candidates;
- show feedback;
- keep the user in the Participants tab.

### Remove-participant action

Use shadcn `Alert Dialog`.

Explain:

- the participant relation is removed, not hard-deleted;
- historical add/remove metadata remains;
- a participant with confirmed workload cannot be removed.

On success:

- invalidate participants/detail/list/candidates/audit.

On `409` workload conflict:

- refetch participants/workloads;
- show clear feedback;
- remove stale action if needed.

### Physical metric presentation registry

Create one frontend registry for visible presentation of the canonical codes.

At minimum map:

```txt
TOTAL_DISTANCE_METERS -> Ukupna udaljenost
HIGH_SPEED_RUNNING_DISTANCE_METERS -> Udaljenost visokog intenziteta
SPRINT_DISTANCE_METERS -> Sprint udaljenost
SPRINT_COUNT -> Broj sprinteva
MAX_SPEED_METERS_PER_SECOND -> Maksimalna brzina
ACCELERATION_COUNT -> Broj ubrzanja
DECELERATION_COUNT -> Broj usporavanja
PLAYER_LOAD_ARBITRARY_UNITS -> Player load
SESSION_DURATION_SECONDS -> Trajanje sesije
```

Registry metadata may include:

- full label;
- short label;
- description/tooltip;
- display unit;
- precision;
- formatting function;
- category/group;
- chart eligibility.

Backend metric catalogue remains authoritative for:

- value kind;
- canonical unit;
- threshold requirement;
- method requirement;
- aggregation hint.

Do not duplicate or override backend semantic rules.

Unknown future metric codes:

- remain visible;
- use a safe technical fallback label;
- show backend unit/value;
- are not silently ranked/charted until presentation support is reviewed.

### Unit formatting

Centralize:

```txt
METERS
METERS_PER_SECOND
METERS_PER_SECOND_SQUARED
COUNT
SECONDS
ARBITRARY_UNITS
```

Recommended visible formatting:

- meters:
  - use `m`;
  - optionally show `km` for large values only through one deterministic formatter;
- meters per second:
  - display canonical `m/s`;
  - an optional secondary `km/h` conversion may be shown only as a clearly labeled presentation conversion;
  - canonical stored/API value remains `m/s`;
- count:
  - integer;
- seconds:
  - format as duration such as `01:32:15`;
  - exact seconds may be available in a tooltip;
- arbitrary units:
  - `AU` or `a.u.` with methodology visible.

Do not change canonical units in API data.

Do not use locale-dependent arithmetic.

Preserve zero.

Absent metric displays:

```txt
Nije dostupno
```

not `0`.

### Threshold context display

Threshold-dependent values must visibly show their threshold context.

Example:

```txt
HSR ≥ 5,5 m/s · Individualni prag
```

Map:

```txt
ABOVE_OR_EQUAL -> ≥
BELOW_OR_EQUAL -> ≤

TEAM -> Timski prag
PLAYER -> Individualni prag
SOURCE_DEFINED -> Prag izvora
```

Rules:

- threshold value and unit are visible in headers, tooltips, or context badges;
- do not hide threshold context behind only a technical tooltip when comparing players;
- values with different threshold values, units, directions, or scopes are separate groups;
- do not merge them into one HSR/sprint/acceleration column.

### Method context display

For method-dependent metrics such as player load, show:

```txt
MethodKey
MethodVersion
```

through a readable context label.

Do not present two player-load values as directly comparable when method keys or versions differ.

Technical keys may use a compact badge/tooltip.

Do not translate a vendor/method key into a claim not supplied by the backend.

### Comparability groups

Build a stable frontend helper that groups metric values by exact backend comparability key.

A group contains:

```txt
metricCode
comparabilityKey
unit
threshold context
method context
```

Rules:

- exact key equality is required for comparison;
- do not derive a weaker key;
- do not group by metric code alone;
- do not normalize away threshold precision;
- do not normalize away method version;
- unknown/missing key prevents comparison mode and falls back to per-player detail display;
- no client-generated hash replaces the backend key.

### Training workload tab

Load:

```txt
GET /api/training-sessions/{trainingSessionId}/workloads
GET /api/physical-metrics/catalog
GET /api/imports?trainingSessionId={trainingSessionId}
GET /api/imports/capabilities
```

Use pagination/filter contracts from Unit 48.

Recommended layout:

```txt
Confirmed workload summary
Metric/comparability selector
Comparison table
Per-player workload details
Source import jobs
```

### Confirmed workload summary

Show:

- active participant count;
- participants with current workload;
- participants without workload;
- current revision count;
- latest recorded time;
- source systems represented;
- incompatible comparability-group count where relevant.

Do not compute team totals or averages in this unit.

Do not label missing workloads as zero workload.

### Metric and context selector

Provide two-step selection:

1. metric code;
2. exact comparability context when more than one exists.

Use shadcn `Select`/`Combobox`.

Rules:

- metric options come from actual returned workload values and catalogue labels;
- context selector is required when the selected metric has multiple comparability keys;
- context label includes threshold/method information;
- selected state is URL-backed within the detail page where practical:

```txt
physicalMetric
physicalComparison
```

- invalid values normalize to the first valid group or no selection;
- no default selection implies comparison across incompatible groups.

### Comparison table

Use TanStack Table.

Rows:

```txt
players/participants with one value in the selected exact comparability group
```

Recommended columns:

- player;
- value;
- unit/context;
- revision;
- source;
- recorded time;
- import job link.

Rules:

- sorting by value is allowed only inside the selected exact group;
- missing values are shown separately and not sorted as zero;
- no average, total, percentile, rank, or “top performer” label is added;
- no cross-context comparison;
- context is visible in the table heading;
- table remains useful with one player;
- pagination remains backend-backed where the API paginates.

### Per-player workload detail

Allow opening a player workload detail Sheet/Collapsible from a row.

Show:

- every current metric;
- exact value/unit;
- threshold context;
- method context;
- comparability key as technical detail;
- current revision number;
- revision count;
- source system;
- processor key/version;
- recorded by/time;
- import job link;
- training or match context.

Do not show previous revision metric arrays unless Unit 48 later exposes an approved revision-history endpoint.

Do not fabricate a revision diff.

### Source import jobs

Show import jobs linked through:

```txt
trainingSessionId
```

Display:

- source filename;
- source system;
- format;
- status;
- created time;
- processor capability;
- open job action.

Open the existing Unit 44 import detail:

```txt
/imports?importJobId={id}
```

Do not duplicate preview/validation/confirmation UI inside training detail.

The import workspace remains the workflow authority.

### Contextual training GPS import action

Show:

```txt
Učitaj GPS podatke
```

only when:

- session is `COMPLETED`;
- current user is:
  - `ADMIN`; or
  - in-scope `DATA_OPERATOR` with `canImportData = true`;
- session is not cancelled.

Use the shared Unit 44 `CreateImportDialog`.

Prefill and lock:

```txt
importType = TRAINING_GPS
teamId = session.TeamId
trainingSessionId = session.Id
matchId = null
```

The user still chooses:

- file;
- source system;
- source label for `OTHER`;
- description.

Rules:

- do not allow target switch inside contextual mode;
- use runtime import capabilities;
- show actual processor stages;
- blocked Gpexe/Zone14 remain generic-preview-only;
- unsupported validation/confirmation is not an upload blocker;
- no fake confirmation button;
- no automatic preview, validation, or confirmation.

On upload success:

- invalidate session import list;
- navigate/open:
  - `/imports?importJobId={createdId}`;
- preserve a clear route back to the training session where practical.

### Unsupported-vendor state

When no exact processor can validate/confirm the selected source:

Show:

```txt
Izvorni fajl se može sačuvati i pregledati, ali ova kombinacija još nema potvrđen processor za validaciju i import službenih GPS podataka.
```

For Gpexe/Zone14, mention the evidence gate without implying failure:

```txt
Vendor mapping ostaje blokiran dok stvarni export format, jedinice i identifikatori nisu potvrđeni.
```

Do not show:

- “biće dostupno u sljedećem ažuriranju” action toast;
- guessed supported metrics;
- a fake mapping form;
- active validate/confirm controls.

### Planned-session physical state

A `PLANNED` session cannot have official workloads.

Show:

```txt
Službeni fizički podaci mogu se potvrditi tek nakon završetka treninga.
```

Allow source upload only if the backend/import rules and product decision permit retention before completion.

For Unit 49 contextual action, keep the primary upload action hidden until `COMPLETED` to reduce target-state conflicts.

Do not claim that no source file may exist for the session if one was uploaded through the global import workflow.

### Cancelled-session physical state

Show existing retained source/import history and any historical data readable according to backend behavior.

Do not show:

- new participant action;
- new contextual import action;
- workload mutation action.

No manual correction is introduced.

### Training audit history

Use:

```txt
GET /api/training-sessions/{id}/audit
```

Reuse Unit 39 shared audit components.

Add labels:

```txt
TRAINING_SESSION_CREATED -> Kreiran trening
TRAINING_SESSION_UPDATED -> Ažuriran trening
TRAINING_SESSION_COMPLETED -> Trening završen
TRAINING_SESSION_CANCELLED -> Trening otkazan
TRAINING_SESSION_PARTICIPANT_ADDED -> Igrač dodat na trening
TRAINING_SESSION_PARTICIPANT_REMOVED -> Igrač uklonjen sa treninga
```

Use scoped action/date/page URL state.

Do not create a global training audit page.

Do not duplicate complete workload metric arrays into the session audit display.

### Match detail integration

Replace the existing match GPS/physical placeholder with real data.

Keep the existing match detail tab route/key.

Visible label:

```txt
GPS / Fizički podaci
```

Load:

```txt
GET /api/matches/{matchId}/physical-workloads
GET /api/physical-metrics/catalog
GET /api/imports?matchId={matchId}
GET /api/imports/capabilities
```

Use the same shared:

- metric presentation registry;
- comparability selector;
- comparison table;
- per-player workload detail;
- source import-job list;
- unsupported-vendor state.

Match-specific rules:

- rows are tied to stable appearances;
- no workload is shown for a non-appearance player;
- confirmation readiness is controlled in Imports;
- report workflow locks are shown from authoritative import/report state;
- no frontend metric mutation.

### Contextual match GPS import

For users with import permission, show:

```txt
Učitaj GPS podatke
```

in the match physical tab when the match is not archived/cancelled and source upload is permitted.

Use the shared Unit 44 dialog with locked:

```txt
importType = MATCH_GPS
teamId = match.TeamId
matchId = match.Id
trainingSessionId = null
```

The exact confirmation processor must still require a played match and respect report locks.

The contextual upload action may retain a source before match completion only if Unit 43/48 backend contracts allow it; the UI must clearly show that official confirmation is unavailable until target conditions are satisfied.

Do not bypass report workflow locks.

### Player detail physical section

Extend `/players/:playerId` with a real:

```txt
Fizički podaci
```

section or tab using the established player detail composition.

Do not add empty tabs for unrelated future modules.

Load:

```txt
GET /api/players/{playerId}/physical-workloads
GET /api/physical-metrics/catalog
```

Support scoped URL state:

```txt
physicalTeamId
physicalContextType
physicalDateFrom
physicalDateTo
physicalMetric
physicalComparison
physicalPage
```

Stable context labels:

```txt
TRAINING -> Trening
MATCH -> Utakmica
```

Rules:

- team choices respect current user scope;
- data filtering/pagination remains server-side;
- historical records remain visible only when current scope allows their team;
- no complete history is loaded client-side;
- no manual metric edit action.

### Player workload history list

Use TanStack Table or a dense app-level history list.

Recommended fields:

- date;
- context;
- team;
- session/match;
- selected metric value;
- threshold/method context;
- source;
- revision;
- recorded time;
- open context.

When no metric is selected, show a compact workload summary per event rather than choosing a potentially misleading default metric.

Do not flatten missing metrics to zero.

### Comparison-safe player trend chart

Use existing shadcn Chart/Recharts infrastructure.

Show a trend chart only when:

- one metric code is selected;
- one exact comparability key is selected;
- at least two chronological values exist.

Chart rules:

- x-axis uses `OccurredOn`;
- y-axis uses the selected canonical unit;
- training and match points may use different point shapes/labels while sharing the exact comparability key;
- tooltip shows context, exact value, threshold/method, source, and revision;
- zero values are plotted;
- absent values are omitted;
- no interpolation across incompatible keys;
- no aggregation of multiple values on the same date unless an explicit stable display rule is used;
- no area/stacked chart that implies totals;
- chart heading includes the exact threshold/method context;
- chart is supplementary to an accessible data table.

If multiple comparability keys exist:

- require explicit context selection;
- show a warning;
- do not draw one combined line.

If fewer than two values exist:

- show the table/list without an empty decorative chart.

Do not add a new charting package.

### Workload source/provenance display

Every workload current revision should expose safe provenance.

Show:

- source kind;
- source system;
- processor key/version;
- revision number/count;
- recorded by/time;
- import job link.

Localized source-kind labels:

```txt
IMPORT -> Import
MANUAL -> Ručni unos
```

Unit 48 production data should currently be `IMPORT`.

If a future `MANUAL` revision appears, display it safely but do not add edit controls.

Do not show:

- source rows;
- vendor spreadsheet headers;
- storage key/path;
- file contents.

### Revision behavior in UI

Current revision is authoritative.

Rules:

- show revision number and total count;
- do not imply historical revisions were deleted;
- do not add rollback;
- do not add previous-revision diff without an API;
- a newer revision should invalidate/refetch current workload views;
- no optimistic revision creation.

### Role-aware UI behavior

#### Read

All authenticated active staff may read sessions/workloads for authorized teams.

#### Session and participant mutation

Show only when backend/session state permits:

- `ADMIN`;
- in-scope `DATA_OPERATOR`.

#### Import creation

Show only for:

- `ADMIN`;
- in-scope `DATA_OPERATOR` with `canImportData = true`.

#### Other roles

`ANALYST`, `COACH`, `MEDICAL_STAFF`, and `VIEWER` remain read-only.

Use role/team/permission data for obvious navigation/header hints.

Use backend `allowedActions` and endpoint authorization for final decisions.

### Query keys

Use TanStack Query.

Define stable keys for:

```txt
trainingSessionList(filters)
trainingSessionDetail(id)
trainingParticipantList(id)
trainingParticipantCandidates(id, filters)
trainingWorkloads(id, filters)
trainingSessionImports(id, filters)
trainingSessionAudit(id, filters)
physicalMetricCatalog
matchPhysicalWorkloads(matchId, filters)
matchGpsImports(matchId, filters)
playerPhysicalWorkloads(playerId, filters)
importCapabilities
```

Reuse existing:

- session/current user;
- teams;
- matches;
- players;
- imports;
- audit components;
- date/size formatting.

Do not store server data in Zustand or React Context.

### Focused invalidation

After:

- create/edit/complete/cancel session:
  - invalidate relevant session list/detail/audit;
- add/remove participant:
  - invalidate participant/detail/list/candidates/audit/workloads;
- contextual import upload:
  - invalidate import lists/session imports/match imports;
- import preview/validation/confirmation:
  - existing Unit 44 handlers invalidate import queries;
  - when confirmation returns target context, also invalidate relevant training/match/player workload queries;
- new workload revision:
  - invalidate training/match/player workload views and relevant session counts.

Do not clear the full query cache.

Do not optimistically invent a workload revision or metric value.

### Processing refresh

Reuse Unit 44 polling behavior for an open import job.

Training-session and workload pages do not continuously poll by default.

After returning from an import operation:

- refetch the contextual import list;
- refetch workloads when the job is imported;
- provide a manual refresh action.

Do not make training detail poll all source jobs continuously.

### Frontend organization

Create or extend:

```txt
frontend/src/features/training/
frontend/src/features/physical-workloads/
```

Suggested organization:

```txt
frontend/src/features/training/
├── api/
├── components/
├── hooks/
├── schemas/
├── types/
├── utils/
└── index.ts

frontend/src/features/physical-workloads/
├── api/
├── components/
├── hooks/
├── types/
├── utils/
└── index.ts
```

Training owns:

- session list/detail;
- session forms;
- participant management;
- session-specific imports/audit composition.

Physical workloads owns:

- metric catalogue presentation;
- comparability grouping;
- value/unit formatting;
- workload tables;
- workload detail;
- player trend chart;
- shared training/match/player workload components.

Do not create one giant training component.

Avoid circular imports.

Use `@/` imports.

Avoid `any`.

### Shadcn-first component selection

Review new UI against:

```txt
context/references/shadcn-components.md
```

Prefer:

- `Table`;
- `Tabs`;
- `Dialog`;
- `Alert Dialog`;
- `Sheet`;
- `Dropdown Menu`;
- `Card`;
- `Item`;
- `Badge`;
- `Alert`;
- `Empty`;
- `Skeleton`;
- `Select`;
- `Combobox`;
- `Command`;
- `Popover`;
- `Calendar`;
- `Tooltip`;
- `Collapsible`;
- `Pagination`;
- `Scroll Area`;
- `Chart`;
- `Sonner`;
- `Button`.

Do not add:

- another table/data-grid library;
- another charting library;
- a timeline package;
- a GPS map package;
- a geospatial package;
- a unit-conversion package;
- a vendor UI SDK;
- a drag-and-drop package.

Do not modify generated shadcn primitives.

Do not apply ad-hoc visual overrides.

### Localization

Visible copy is Bosnian Latin with proper characters.

Suggested labels:

```txt
Treninzi i GPS
Novi trening
Planiran
Završen
Otkazan
Pregled
Učesnici
GPS / Fizički podaci
Historija promjena
Dodaj učesnika
Ukloni učesnika
Završi trening
Otkaži trening
Učitaj GPS podatke
Potvrđeni fizički podaci
Izvorni importi
Nema potvrđenih fizičkih podataka
Metrika
Kontekst uporedivosti
Timski prag
Individualni prag
Prag izvora
Metodologija
Revizija
Izvor
Trening
Utakmica
Nije dostupno
```

Internal API codes remain English.

Do not use mixed labels such as `Training GPS` in the visible default Bosnian interface.

### Accessibility

Requirements:

- list/detail tabs are keyboard accessible;
- dialogs and confirmations restore focus;
- status/action information does not rely only on color;
- participant candidate selector is keyboard usable;
- date/time fields have labels;
- comparison selector has clear labels;
- threshold/method context is available to screen readers;
- workload tables use semantic markup;
- sort controls announce selected comparability context;
- charts have accessible titles/descriptions and an equivalent data table;
- zero versus unavailable is conveyed in text;
- icon-only actions have accessible labels/tooltips;
- large tables use contained horizontal scrolling;
- import links announce navigation;
- no destructive action receives automatic focus.

### Responsive behavior

Desktop/tablet are primary.

Requirements:

- training list uses contained table overflow;
- detail header/actions wrap cleanly;
- tabs remain usable on mobile;
- participant table/list adapts to stacked cards when needed;
- workload comparison table uses contained scrolling;
- metric/context selectors stack on small screens;
- workload detail Sheet uses full mobile width;
- chart remains readable and has a table fallback;
- source import cards stack;
- no page-wide uncontrolled overflow.

Do not create a separate mobile product.

### Error handling

Use existing ProblemDetails handling.

Expected behavior:

- `401`: existing auth/session flow;
- `403`: safe permission/scope feedback and session refetch where relevant;
- `404`: safe missing/inaccessible session/player/match/import;
- `409`: invalid lifecycle, ineligible participant, duplicate participant, workload-bearing removal, workflow lock, stale revision, or target conflict; refetch authoritative data;
- `422`: field-level semantic validation where applicable;
- network error: retain forms/filters and offer retry;
- unknown metric/catalog mismatch: show compatibility warning without hiding other values.

Do not display:

- source rows;
- vendor headers;
- storage keys/paths;
- raw exception text;
- hidden entity existence;
- complete audit internals.

Do not log workload DTOs, metric values, player physical history, or import source data to the browser console.

### Tests and verification approach

Add backend tests for the two query extensions.

Add frontend tests only if the repository already has an approved frontend testing foundation.

When frontend tests exist, prioritize:

- session status/action label mappings;
- local date/time conversion with separate `SessionDate`;
- participant candidate query parameters;
- metric label/unit formatting;
- zero versus unavailable;
- exact comparability grouping;
- threshold/method labels;
- unknown metric fallback;
- contextual import target locking;
- training/player physical URL state;
- trend-chart eligibility;
- no cross-key chart series;
- audit action labels.

Do not introduce a new frontend testing framework solely for Unit 49.

Manual verification must cover:

- every role and team scope;
- training list filters/pagination;
- create/edit;
- complete/cancel;
- planned/completed/cancelled restrictions;
- date change with eligible and ineligible participants;
- candidate search using historical/current/future assignment coverage;
- add/remove/re-add;
- workload-bearing removal conflict;
- empty workload state;
- multiple threshold contexts for one metric;
- multiple player-load method versions;
- zero versus absent values;
- provenance/revision display;
- contextual training import;
- blocked Gpexe/Zone14 capability state;
- session-linked import history;
- match physical tab;
- contextual match import;
- player history filters;
- exact-key trend chart;
- audit history;
- responsive layout;
- keyboard/focus behavior.

### Documentation synchronization

Update:

```txt
context/ui-context.md
context/progress-tracker.md
```

Record:

- routes and navigation label;
- participant-candidate endpoint;
- import `trainingSessionId` filter;
- session lifecycle/forms;
- participant eligibility UI;
- metric presentation and comparability behavior;
- contextual training/match import integration;
- player physical history/chart;
- blocked vendor state;
- verification results;
- intentionally deferred vendor mappings/manual entry/dashboard aggregates.

If implementation reveals a mismatch in Unit 48 API contracts, update the relevant context/spec before continuing.

Do not silently weaken comparability rules or add guessed vendor support.

## Implementation

### 1. Add the participant-candidate backend query

Implement:

```txt
GET /api/training-sessions/{id}/participant-candidates
```

Reuse Unit 28 assignment date logic and Unit 48 participant authorization.

Add pagination/search and focused tests.

No migration.

### 2. Extend import list filtering

Add optional:

```txt
trainingSessionId
```

to the existing import list query and typed clients.

Validate target scope.

Add focused tests.

No migration.

### 3. Add training frontend contracts and queries

Create typed clients/hooks for:

- session list/detail;
- create/update/complete/cancel;
- participants;
- participant candidates;
- session workloads;
- session imports;
- session audit.

### 4. Add routes and navigation

Create:

```txt
/training-sessions
/training-sessions/:id
```

Add `Treninzi i GPS` to Performance navigation.

Update UI context.

### 5. Build the training list

Implement URL filters, status tabs, server pagination, TanStack Table, role-aware create action, and all list states.

### 6. Build create/edit/lifecycle flows

Implement:

- create dialog;
- edit dialog;
- complete confirmation;
- cancel confirmation;
- local time to UTC conversion;
- focused invalidation.

### 7. Build the detail shell

Implement:

```txt
Pregled
Učesnici
GPS / Fizički podaci
Historija promjena
```

Use URL-backed tab state and backend actions.

### 8. Build participant management

Implement candidate search, add, remove, conflict handling, and eligibility context.

Do not use current-assignment-only queries.

### 9. Build canonical workload presentation

Create:

- metric registry;
- unit formatter;
- threshold/method formatter;
- comparability grouping;
- workload comparison table;
- per-player workload detail;
- provenance/revision presentation.

### 10. Add contextual import integration

Reuse Unit 44 upload dialog for:

- locked `TRAINING_GPS` session target;
- locked `MATCH_GPS` match target.

Show actual processor capabilities.

Do not activate unsupported stages.

### 11. Add session import history

Query imports by `trainingSessionId`.

Link to the existing Import detail workspace.

Do not duplicate the import workflow.

### 12. Implement match physical tab

Replace the existing placeholder with shared workload components and contextual import history/action.

Respect appearances and report locks.

### 13. Implement player physical history

Add the real section/tab, filters, workload list, exact-context selection, and comparison-safe trend chart.

### 14. Add training audit history

Reuse Unit 39 shared audit components and Unit 48 action mappings.

### 15. Wire focused query invalidation

Update session, participant, import, match, player, workload, and audit queries only where affected.

### 16. Verify shadcn-first implementation

Review all new UI against the component catalogue.

Remove unnecessary custom primitives.

Confirm generated `frontend/src/components/ui/*` files remain unmodified.

### 17. Update progress documentation

Update `context/progress-tracker.md` with actual implementation and verification state.

Do not mark Unit 49 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use existing backend/frontend packages and infrastructure:

- ASP.NET Core 8;
- existing Unit 48 APIs;
- existing Unit 44 import components and XHR upload behavior;
- Unit 39 audit components;
- TanStack Query;
- `nuqs`;
- React Hook Form;
- Zod;
- TanStack Table;
- shadcn/ui;
- existing Recharts-backed shadcn Chart;
- Lucide React;
- existing date, duration, decimal, ProblemDetails, and query utilities.

Add required shadcn components just in time through the approved CLI workflow.

Do not add:

- another charting package;
- a data-grid package;
- GPS/geospatial packages;
- mapping packages;
- vendor SDKs;
- unit-conversion packages;
- timeline packages;
- another query/state library;
- a new frontend testing framework solely for this unit.

## Verification checklist

- [ ] `GET /api/training-sessions/{id}/participant-candidates` exists.
- [ ] Candidate eligibility uses assignment coverage on session date.
- [ ] Current-assignment-only logic is not used.
- [ ] Active participants are excluded.
- [ ] Removed historical participants may appear when otherwise eligible.
- [ ] Candidate endpoint is restricted to users who may add participants.
- [ ] Candidate search/pagination/order are database-backed.
- [ ] Existing participant POST still revalidates eligibility.
- [ ] `GET /api/imports` supports `trainingSessionId`.
- [ ] Training-session import filtering is scope-safe and database-backed.
- [ ] No Unit 49 migration is added.
- [ ] `/training-sessions` exists.
- [ ] `/training-sessions/:id` exists.
- [ ] Visible navigation uses `Treninzi i GPS`.
- [ ] Navigation is team/role aware.
- [ ] Training list filters and pagination use `nuqs`.
- [ ] Search, team, status, dates, and page are URL-driven.
- [ ] Training list uses TanStack Table and shadcn primitives.
- [ ] Compact rows use participant/workload counts without N+1 detail requests.
- [ ] Loading, empty, no-results, error, and inaccessible states exist.
- [ ] Create form uses RHF/Zod and real backend mutation.
- [ ] Create always produces `PLANNED`.
- [ ] Team and status cannot be changed through generic edit.
- [ ] Session date remains distinct from UTC start/end timestamps.
- [ ] Completed/cancelled lifecycle uses explicit confirmation actions.
- [ ] No reopen or hard-delete action exists.
- [ ] Detail tabs use URL-backed state.
- [ ] Overview, Participants, GPS/Physical, and Audit are real views.
- [ ] Detail mutations use backend `allowedActions`.
- [ ] Participant add uses the new candidate endpoint.
- [ ] Participant add never accepts free-text player names.
- [ ] Participant remove explains history preservation.
- [ ] Workload-bearing participant removal conflict is handled.
- [ ] Session-date eligibility conflict is mapped clearly.
- [ ] Canonical metric labels are centralized.
- [ ] Backend metric catalogue remains authoritative.
- [ ] Unknown metric codes remain visible through safe fallback.
- [ ] Zero and absent metrics are displayed differently.
- [ ] Canonical units are formatted consistently.
- [ ] Optional km/h display is clearly secondary to canonical m/s.
- [ ] Threshold context is visibly shown.
- [ ] Method key/version is shown for method-dependent metrics.
- [ ] Comparability groups use exact backend comparability keys.
- [ ] Metric code alone is never treated as sufficient comparability.
- [ ] Different thresholds/method versions remain separate.
- [ ] Sorting/comparison occurs only inside one exact group.
- [ ] No team totals, averages, percentiles, or top-performer labels are invented.
- [ ] Workload detail shows current revision and safe provenance.
- [ ] No previous-revision diff is fabricated.
- [ ] Session import history uses `trainingSessionId`.
- [ ] Contextual training import locks import type/team/session.
- [ ] Contextual match import locks import type/team/match.
- [ ] Shared Unit 44 upload flow is reused rather than duplicated.
- [ ] Upload does not auto-preview, auto-validate, or auto-confirm.
- [ ] Gpexe/Zone14 remain generic-preview-only.
- [ ] No fake vendor validation/confirmation action is shown.
- [ ] Planned-session official workload limitation is explained.
- [ ] Cancelled sessions expose no new participant/import actions.
- [ ] Match GPS/physical placeholder is replaced with real workload data.
- [ ] Match workloads remain tied to appearances.
- [ ] Match import/confirmation state respects report workflow locks.
- [ ] Player detail includes a real `Fizički podaci` section.
- [ ] Player history filtering/pagination remains server-side.
- [ ] Player trend chart requires one metric and one exact comparability key.
- [ ] No chart combines incompatible threshold/method contexts.
- [ ] Zero values are plotted and missing values omitted.
- [ ] Chart has an accessible table equivalent.
- [ ] Training audit history reuses shared audit components.
- [ ] Training audit actions have localized labels.
- [ ] No global training/workload audit page is added.
- [ ] Read access follows team scope for all roles.
- [ ] Admin/in-scope data operator may mutate sessions/participants.
- [ ] Import creation requires admin or in-scope data operator with `canImportData`.
- [ ] Other roles remain read-only.
- [ ] Backend `401`, `403`, `404`, `409`, `422`, network, and compatibility errors are handled safely.
- [ ] Workload/server data is not stored in Zustand or React Context.
- [ ] Query invalidation is focused.
- [ ] No optimistic workload revision or metric value is invented.
- [ ] New UI follows shadcn-first guidance.
- [ ] Generated `frontend/src/components/ui/*` files are not manually modified.
- [ ] No data-grid, charting, GPS map, geospatial, vendor, unit-conversion, or timeline package is added.
- [ ] No raw Tailwind palette classes or hardcoded component colors are introduced.
- [ ] No ad-hoc visual overrides are applied to shadcn components.
- [ ] Frontend imports use `@/`.
- [ ] Visible copy uses Bosnian Latin with proper characters.
- [ ] Forms, tabs, participant picker, tables, confirmations, workload selectors, chart, and pagination are keyboard accessible.
- [ ] Desktop, tablet, and mobile layouts remain usable.
- [ ] No backend persistence model, metric catalogue, import status, vendor processor, or authorization semantic is changed.
- [ ] Backend support-query tests pass.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for affected backend projects/solution.
- [ ] Relevant backend tests pass.
- [ ] `npm run format` completes successfully.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] Frontend typecheck/build passes using the configured project command.
- [ ] Existing approved frontend tests pass when present.
- [ ] `context/ui-context.md` reflects `Treninzi i GPS` and real physical-data surfaces.
- [ ] `context/progress-tracker.md` records actual Unit 49 implementation, vendor-gate state, and verification results.
