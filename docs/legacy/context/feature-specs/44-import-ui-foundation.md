# Unit 44: Import UI Foundation

## Goal

Build the complete first import workflow UI on top of Unit 43 without inventing parsers, mappings, or vendor fields. Add a protected team-scoped import workspace for source-file upload, import type/source selection, import history, job detail, source download, processor capability display, preview tables, validation issues, explicit confirmation, cancellation, processing-state handling, and audit history. Render workflow actions only from backend `allowedActions` and registered processor capabilities so unsupported combinations are shown truthfully rather than as fake placeholder actions.

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
10. `context/feature-specs/23-staff-users-team-scope-account-lifecycle-backend.md`
11. `context/feature-specs/30-matches-backend-foundation.md`
12. `context/feature-specs/39-audit-ui-foundation.md`
13. `context/feature-specs/40-file-storage-abstraction.md`
14. `context/feature-specs/43-import-workflow-backend-foundation.md`
15. `context/feature-specs/44-import-ui-foundation.md`

Use relevant project-local skills from `.agents/skills/` when applicable.

Before creating custom UI primitives or interaction patterns:

- check `context/references/shadcn-components.md`;
- prefer suitable shadcn/ui components;
- install required shadcn components just in time through the approved workflow;
- compose app-level components outside `frontend/src/components/ui`;
- do not manually modify generated shadcn/ui primitive files;
- do not hand-build equivalents of existing shadcn primitives without a documented reason.

This unit is frontend-only.

Do not change:

- import statuses or transitions;
- processor contracts;
- parser behavior;
- validation rules;
- confirmation semantics;
- permissions;
- team-scope rules;
- audit codes;
- storage behavior;
- backend APIs;
- EF Core migrations.

### Scope

This unit introduces:

- protected `/imports` route;
- `Importi` navigation in the Performance group;
- import capabilities loading;
- URL-driven import filters and pagination;
- import history table using real Unit 43 data;
- streamed CSV/XLSX upload with progress and cancellation;
- file-first, then import-type/source/context selection before the upload request is sent;
- import job detail workspace;
- status and processing-state display;
- source-file download;
- backend-owned `allowedActions` rendering;
- processor capability display;
- preview action and preview table;
- validation action and validation issue display;
- explicit confirmation flow;
- cancellation flow;
- safe unsupported-processor state;
- processing lease/stale-state feedback;
- import audit history using the Unit 43 audit endpoint and Unit 39 shared audit components;
- loading, empty, partial, conflict, failure, and terminal states.

This unit does not introduce:

- CSV parsing;
- XLSX parsing;
- column mapping mutations;
- generic mapping editor;
- Gpexe field mapping;
- Zone14 field mapping;
- roster import logic;
- statistics import logic;
- GPS metric import logic;
- training-session import logic;
- background processing;
- polling faster than the approved cadence;
- automatic confirmation;
- import rollback;
- import deletion;
- import restore/reopen;
- source-file editing or replacement;
- drag-and-drop upload packages;
- spreadsheet/data-grid packages;
- frontend-side parsing of source files;
- frontend-side validation of file contents;
- fake preview rows;
- fake validation results;
- fake processor availability.

### No placeholder action rule

Unit 44 must not leave buttons that merely show messages such as:

```txt
Ova funkcija bit će dostupna u sljedećem ažuriranju.
```

Workflow availability comes from:

- `GET /api/imports/capabilities`;
- the selected job's `allowedActions`;
- the current job status.

If no processor exists for a job combination:

- do not render active `Generiši pregled`, `Validiraj`, or `Potvrdi import` buttons;
- show a clear capability state explaining that the uploaded source is retained but processing for that exact combination is not currently supported;
- keep source download, audit history, and cancellation available when backend actions permit them;
- do not guess how the file should be parsed;
- do not simulate a future result.

When Units 45–47 register processors, the same UI should expose newly available actions without requiring a rewritten workflow model.

### Route and navigation

Add:

```txt
/imports
```

Suggested Bosnian navigation label:

```txt
Importi
```

Place it in the existing Performance group.

Navigation visibility:

- `ADMIN`;
- `DATA_OPERATOR` only when `canImportData = true`.

Other roles must not see the import navigation entry.

The backend remains authoritative. A stale client session may still receive `403`.

Do not add separate top-level routes for each import status or vendor.

### Import page structure

Use one route with URL-addressable selected job state.

Recommended structure:

```txt
PageHeader
Status summary / status tabs
Filter controls
Import history table
Import detail Sheet
```

Suggested title:

```txt
Importi
```

Suggested description:

```txt
Učitajte izvorne CSV/XLSX fajlove, pregledajte validaciju i eksplicitno potvrdite podržane importe.
```

Primary header action when the user may upload:

```txt
Novi import
```

Do not show a create action to users who cannot use the import workflow.

### Import capabilities

Load:

```txt
GET /api/imports/capabilities
```

Use it as the source of truth for:

- maximum upload size;
- preview row limit;
- processing lease timeout display where useful;
- accepted CSV/XLSX extension/content-type hints;
- stable import types;
- source systems;
- registered processor capabilities.

Do not hardcode runtime maximum size.

The frontend may maintain centralized localized presentation metadata for stable enum values, but must not invent processor support.

If capabilities fail to load:

- keep already loaded import history available;
- disable `Novi import`;
- show a retryable capabilities error;
- do not guess accepted file types or processing support.

### Import type labels

Centralize visible labels:

```txt
PLAYER_ROSTER -> Spisak igrača
MATCH_PLAYER_STATISTICS -> Statistika igrača za utakmicu
MATCH_GPS -> GPS / fizički podaci utakmice
TRAINING_GPS -> GPS / fizički podaci treninga
```

These are workflow labels only.

Do not imply that every type currently has a processor.

### Source system labels

Centralize:

```txt
GENERIC -> Generički format
GPEXE -> Gpexe
ZONE14 -> Zone14
OTHER -> Drugi izvor
```

Do not imply confirmed Gpexe or Zone14 field support merely because the source system can be selected.

### File format labels

Centralize:

```txt
CSV -> CSV
XLSX -> Excel (.xlsx)
```

Do not display raw MIME strings as the main UI.

### Import status labels

Centralize:

```txt
UPLOADED -> Učitan
PARSING -> Obrada u toku
VALIDATION_FAILED -> Validacija nije prošla
READY_TO_CONFIRM -> Spremno za potvrdu
IMPORTED -> Importovan
FAILED -> Obrada nije uspjela
CANCELLED -> Otkazan
```

Status badges must include readable text and not rely only on color.

### Status tabs

Use shadcn `Tabs` or the approved app-level tabs pattern.

Suggested tabs:

```txt
Sve
Učitani
Obrada u toku
Validacija nije prošla
Spremni za potvrdu
Importovani
Neuspjeli
Otkazani
```

Internal values remain Unit 43 status codes.

Store active status in URL state.

Do not combine processor capability with status.

### URL-driven filters

Use `nuqs`.

Support at minimum:

```txt
search
teamId
matchId
importType
sourceSystem
fileFormat
status
dateFrom
dateTo
page
importJobId
```

Rules:

- search uses the established debounce pattern;
- discrete filters update immediately;
- changing a filter resets page to 1;
- invalid URL values normalize safely;
- selected job detail is URL-addressable through `importJobId`;
- list filtering/pagination remains server-side;
- do not fetch all imports and filter client-side;
- selected-team users are offered only authorized teams;
- explicit out-of-scope server responses are handled safely;
- `matchId` selector is optional and scoped to authorized matches;
- provide `Očisti filtere` when non-default filters are active.

Do not add filters the backend does not support.

### Import history table

Use TanStack Table and shadcn table primitives.

Recommended columns:

- created date/time;
- original filename;
- team/selection;
- match when present;
- import type;
- source system;
- format;
- status;
- validation/result summary;
- creator;
- actions.

Do not show:

- storage key;
- local path;
- processing lease ID;
- raw JSON;
- raw MIME as the main source label;
- source file contents.

Row behavior:

- primary row action opens the detail Sheet;
- action menu uses shadcn `Dropdown Menu`;
- high-impact confirm/cancel actions should normally happen inside the detail workspace where the user can see context;
- action menu may provide `Otvori` and `Preuzmi izvorni fajl` when allowed.

### Import table states

#### No import jobs

Show:

```txt
Još nema import jobova.
```

Authorized users receive `Novi import`.

#### No filter results

Show a filter-specific empty state and `Očisti filtere`.

#### Loading

Use `Skeleton`.

#### Query error

Use `Alert` and retry.

#### Restricted user

The route guard should prevent ordinary access. Do not reveal hidden import counts.

### New import dialog

Use a large shadcn `Dialog` or `Sheet`.

Use React Hook Form + Zod for metadata.

The user-visible sequence should preserve the product intent:

```txt
1. Odaberite fajl
2. Odaberite vrstu i izvor importa
3. Odaberite ciljnu selekciju / utakmicu
4. Pregledajte sažetak
5. Učitajte originalni fajl
```

The HTTP upload request is sent only after all required metadata is selected.

This preserves the conceptual file-first flow while satisfying the Unit 43 requirement that immutable type/source/context metadata is persisted atomically with the source file.

Do not create a temporary backend job before the final upload request.

### Upload form fields

Fields:

```txt
file
importType
sourceSystem
sourceLabel when OTHER
teamId
matchId when required
description
```

Rules:

- exactly one CSV/XLSX file;
- file accept hints come from capabilities;
- file size precheck uses runtime max size;
- zero-byte rejected;
- team choices use authorized active teams;
- target fields react to import type;
- match types require a match;
- selected match must belong to selected team;
- roster/training GPS types do not accept a match;
- source label appears only for `OTHER`;
- no processor is required to upload/retain the source;
- show processor availability summary before submit;
- unsupported processing capability is not an upload blocker;
- explain that the original file can be retained even when automated processing is not yet available.

Do not inspect or parse file contents in the browser.

### Upload processor-capability summary

Before upload, show the selected combination:

```txt
Import type
Source system
File format
```

Then show capability stages:

```txt
Pregled
Validacija
Potvrda
```

Each stage displays:

```txt
Dostupno
Nije dostupno
```

based only on capabilities.

Do not show unavailable stages as active buttons.

Suggested explanatory copy:

```txt
Fajl se može sigurno sačuvati i auditovati i kada automatska obrada ove kombinacije još nije podržana.
```

This is a capability statement, not a placeholder implementation message.

### Streaming upload

Use the Unit 43 metadata-first multipart contract.

Construct:

- metadata JSON part first;
- one file part second.

Do not:

- Base64 encode;
- parse CSV/XLSX in JavaScript;
- fully buffer a large source file unnecessarily;
- send duplicate parts.

Reuse or generalize the native XHR upload infrastructure introduced by Unit 42 where practical.

Do not add Axios solely for progress.

### Upload progress and cancellation

Use browser-native `XMLHttpRequest`.

Requirements:

- existing API base URL convention;
- credentials/cookies;
- existing CSRF behavior;
- ProblemDetails parsing;
- progress percentage when computable;
- indeterminate state otherwise;
- prevent duplicate submit;
- support user abort.

Suggested states:

```txt
Priprema
Učitavanje izvornog fajla
Završavanje
Import job je kreiran
Upload nije uspio
```

Use shadcn `Progress`.

If the user aborts:

- explain that the browser request may race with server completion;
- invalidate/refetch the import list;
- show:
  `Zahtjev je prekinut. Provjerite listu importa jer je server možda već završio čuvanje fajla.`

Do not promise guaranteed rollback from a client-side abort.

### Upload success

On success:

- close upload dialog;
- show success feedback;
- invalidate import lists;
- open the new job detail Sheet;
- do not automatically start preview;
- do not automatically validate;
- do not automatically confirm.

### Import detail Sheet

Use shadcn `Sheet`.

Store selected ID in URL state.

Requirements:

- open sets `importJobId`;
- refresh restores an accessible selected job;
- close clears selected ID;
- stale/missing/inaccessible job closes safely or uses the established detail error state;
- no Zustand selected-job store.

Suggested detail sections:

```txt
Job Header
Status and Processing
Source File
Workflow Steps
Preview
Validation
Result / Failure
Audit History
```

Use nested tabs where useful.

### Detail header

Show:

- original filename;
- import type;
- source system;
- team;
- match when present;
- status;
- creator;
- created/updated times;
- current allowed primary action;
- secondary allowed actions.

Do not show the processing lease ID.

### Processing state

When status is `PARSING`, show:

- current operation:
  - preview;
  - validation;
  - confirmation;
- processing start time;
- requested-by summary when returned;
- processor key/version only as secondary technical context;
- a visible in-progress state.

Suggested labels:

```txt
Generisanje pregleda
Validacija u toku
Potvrda importa u toku
```

Do not show a cancel button when backend `allowedActions` omits it.

### Stale processing UX

Unit 43 supports stale lease recovery through a later authorized action/cancellation.

The UI must not calculate final authority from its own clock.

It may show:

```txt
Obrada traje duže od očekivanog.
```

after the configured lease timeout has visibly elapsed, but backend `allowedActions` remains authoritative.

When a stale recovery action becomes available after refetch:

- render the backend-provided action;
- explain that a previous processing attempt may no longer own the job.

Do not forcibly reset status client-side.

### Backend-owned actions

Render workflow actions only from `allowedActions`:

```txt
VIEW
DOWNLOAD_SOURCE
GENERATE_PREVIEW
VALIDATE
CONFIRM
CANCEL
```

Rules:

- do not recreate the full status/permission/capability matrix in React components;
- role/session data may hide clearly impossible route/header actions;
- job-level actions come from the backend;
- unknown future action codes are ignored safely for mutations and documented if encountered;
- always handle stale `403`/`409`.

### Unsupported processor state

If the job has no registered processor for preview/validation/confirmation:

- show the selected combination;
- show unavailable capability stages;
- keep job metadata, source download, audit history, and cancel action available as backend permits;
- do not show fake preview/validate/confirm buttons;
- do not display “coming in next update” toaster messages;
- do not generate local sample data;
- do not parse in the browser.

Suggested copy:

```txt
Za ovu kombinaciju vrste importa, izvora i formata trenutno nije registrovan procesor. Originalni fajl je sigurno sačuvan i može se pregledati ili otkazati prema dostupnim akcijama.
```

### Source file section

Show safe metadata:

- original filename;
- format;
- content type;
- size;
- uploaded by;
- created time.

When `DOWNLOAD_SOURCE` is present, provide:

```txt
Preuzmi izvorni fajl
```

Use the authorized source endpoint directly.

Do not fetch large source files into JavaScript memory solely for download.

Do not display storage keys or paths.

### Workflow stepper

Provide an informational progress structure such as:

```txt
1. Fajl učitan
2. Pregled
3. Validacija
4. Potvrda
```

Use status/capability data to mark:

- complete;
- current;
- unavailable;
- failed;
- waiting.

This stepper is informational.

Do not use it to mutate status.

Do not mark `Pregled` complete merely because a job is `UPLOADED`; use actual preview timestamp/result state.

Do not mark validation complete unless validation outcome exists.

### Generate preview action

Render only when:

```txt
GENERATE_PREVIEW
```

is present.

Suggested button:

```txt
Generiši pregled
```

Use a confirmation only if the action will replace an existing preview.

If no existing preview:

- direct explicit action is acceptable.

If a preview already exists:

- use shadcn `Alert Dialog`;
- explain that the stored preview will be regenerated from the immutable source file.

On action:

```txt
POST /api/imports/{id}/preview
```

Do not send parser options or mappings that Unit 43 does not support.

On success:

- refetch detail;
- refetch preview;
- refetch audit;
- show success feedback.

On `409`:

- refetch detail/capabilities;
- show authoritative conflict.

### Preview tab/section

Load:

```txt
GET /api/imports/{id}/preview
```

only when the preview section is active or preview metadata indicates it exists.

Use TanStack Table and shadcn table primitives.

Requirements:

- columns come from backend preview metadata;
- column order follows backend `Ordinal`;
- row order follows backend source row ordering;
- source row number is visible;
- values are rendered as plain text/data;
- no HTML execution;
- `null` has a clear empty marker;
- numeric zero stays `0`;
- wide tables use contained horizontal scrolling;
- JetBrains Mono may be used for dense preview values;
- backend pagination is used;
- no client-side full-file parsing;
- no client-side mapping assumptions.

Do not add editable column mapping controls in Unit 44 because Unit 43 provides no mapping mutation contract.

When mapping arrives in Unit 45 or later, extend this section through a new spec instead of creating fake controls now.

### Preview empty states

#### Not generated

Show:

```txt
Pregled još nije generisan.
```

If `GENERATE_PREVIEW` exists, show the real action.

If no preview processor exists, show the unsupported-processor explanation.

#### Generated but zero rows

Show the backend's truthful empty result.

Do not fabricate headers/rows.

#### Preview query error

Show `Alert` and retry without hiding job metadata.

### Validate action

Render only when:

```txt
VALIDATE
```

is present.

Suggested button:

```txt
Validiraj import
```

If prior validation exists:

- use confirmation explaining that validation issues and readiness result will be recalculated.

On:

```txt
POST /api/imports/{id}/validate
```

Success may produce either:

- `READY_TO_CONFIRM`;
- `VALIDATION_FAILED`.

Both are valid completed validation outcomes.

Do not show a generic success toast that implies data is valid when the resulting status is `VALIDATION_FAILED`.

Use status-aware feedback:

```txt
Validacija je završena.
```

Then render the authoritative result.

### Validation summary

Show:

- validation status;
- total rows when known;
- valid rows;
- invalid rows;
- warnings;
- validation completed time;
- validated processor key/version as secondary context;
- current configuration revision and validated revision only when useful for explaining stale readiness.

Suggested labels:

```txt
Ukupno redova
Ispravni redovi
Neispravni redovi
Upozorenja
```

Do not treat warnings as errors.

### Validation issue table

Load:

```txt
GET /api/imports/{id}/validation-issues
```

Support URL/local detail state for:

```txt
severity
code
page
```

Use TanStack Table + shadcn table primitives.

Recommended columns:

- severity;
- source row;
- column;
- code;
- message.

Rules:

- server-side filtering/pagination;
- preserve safe backend message;
- display stable issue code as secondary technical context;
- no raw metadata dump by default;
- metadata may be shown in bounded `Collapsible` technical details when safe and useful;
- no stack trace;
- no source file content dump.

Severity labels:

```txt
ERROR -> Greška
WARNING -> Upozorenje
```

Do not rely only on color.

### Validation empty states

#### No validation run

Show:

```txt
Validacija još nije pokrenuta.
```

Render `Validiraj import` only when backend action exists.

#### Zero issues

Show a positive but precise state:

```txt
Nema evidentiranih grešaka ili upozorenja u posljednjoj validaciji.
```

Do not claim official data has been imported.

#### Validation failed with issues

Show counts and paginated issues.

Provide a clear status explanation.

### Explicit confirmation

Render only when:

```txt
CONFIRM
```

is present.

Suggested button:

```txt
Potvrdi import
```

Use shadcn `Alert Dialog`.

The confirmation must explain:

- official data will be mutated;
- the current validation revision and processor version will be rechecked;
- the operation is explicit and cannot be undone through the Unit 44 UI;
- source file and audit history remain retained.

Show a concise summary before confirm:

- team;
- match when relevant;
- import type;
- source system;
- filename;
- valid/invalid/warning counts;
- processor/version when returned.

On confirm:

```txt
POST /api/imports/{id}/confirm
```

Do not send a target status.

On success:

- refetch detail;
- refetch list;
- refetch audit;
- invalidate target-domain queries when the response/result summary identifies them or when future processor integrations define focused invalidation;
- show success feedback;
- remain in the detail workspace.

In Unit 43 alone, no real confirmation processor exists, so the action should not be available.

Do not leave a clickable confirmation placeholder.

### Confirmation conflict handling

Handle:

- stale validation revision;
- processor-version mismatch;
- target workflow/lifecycle change;
- active processing conflict;
- authorization change.

On conflict:

- refetch job detail;
- refetch capabilities;
- refetch validation issues when relevant;
- remove stale action buttons;
- show the safe backend reason.

Do not force local status changes.

### Cancellation

Render only when:

```txt
CANCEL
```

is present.

Suggested button:

```txt
Otkaži import
```

Use shadcn `Alert Dialog`.

Explain:

- the import job becomes terminal;
- the original source file remains retained;
- preview/validation history remains;
- cancellation does not delete already imported official data because an `IMPORTED` job cannot be cancelled;
- there is no restore/reopen action.

On:

```txt
POST /api/imports/{id}/cancel
```

On success:

- refetch detail/list/audit;
- keep the cancelled job visible;
- remove unavailable workflow actions.

Do not use HTTP delete or a trash icon as the primary semantic.

### Failed state

For `FAILED`, show:

- safe failure code;
- safe failure message;
- last processing operation when known;
- available retry actions from backend;
- source download;
- audit history;
- cancel when allowed.

Do not show raw exception details.

Do not imply data was partially imported.

### Imported state

For `IMPORTED`, show:

- terminal success state;
- confirmation actor/time;
- bounded result summary;
- source download;
- audit history.

Do not show:

- retry;
- cancel;
- confirm again;
- reopen.

Render result summary through a safe structured renderer.

Do not dump raw JSON as the default.

### Result summary renderer

Reuse the bounded safe structured-value patterns from Unit 39 where practical.

Show known keys with localized labels when later processors define them.

For unknown safe keys:

- show stable key and value;
- preserve `null`, zero, booleans, arrays;
- bound nested depth and list sizes;
- never execute HTML;
- never show storage/security keys.

Do not build a generic arbitrary database-object viewer.

### Import audit history

Use:

```txt
GET /api/imports/{id}/audit
```

Reuse Unit 39 shared audit components.

Add a nested detail view:

```txt
Tok importa
Historija promjena
```

Suggested URL-scoped state:

```txt
importView=workflow
importView=audit
```

Audit view supports:

- action filter;
- date from;
- date to;
- page.

Use the Unit 43 import audit action labels:

```txt
IMPORT_JOB_CREATED -> Kreiran import job
IMPORT_JOB_PREVIEW_GENERATED -> Generisan pregled importa
IMPORT_JOB_VALIDATION_FAILED -> Validacija importa nije prošla
IMPORT_JOB_READY_TO_CONFIRM -> Import spreman za potvrdu
IMPORT_JOB_CONFIRMED -> Import potvrđen
IMPORT_JOB_CANCELLED -> Import otkazan
IMPORT_JOB_PROCESSING_FAILED -> Obrada importa nije uspjela
```

Do not create a global import audit page.

Do not reconstruct missing history client-side.

### Audit field presentation

Add import-specific audit field labels for safe known fields:

```txt
teamId -> Selekcija
matchId -> Utakmica
importType -> Vrsta importa
sourceSystem -> Izvor
sourceLabel -> Naziv izvora
fileFormat -> Format
status -> Status
originalFileName -> Izvorni fajl
contentType -> Tip sadržaja
sizeBytes -> Veličina
processorKey -> Procesor
processorVersion -> Verzija procesora
configurationRevision -> Revizija konfiguracije
totalRowCount -> Ukupno redova
previewRowCount -> Redovi u pregledu
validRowCount -> Ispravni redovi
invalidRowCount -> Neispravni redovi
warningCount -> Upozorenja
failureCode -> Kod greške
```

Reuse safe value rendering from Unit 39.

Do not display storage keys, preview datasets, or full validation issue datasets.

### Query keys and cache behavior

Use TanStack Query.

Define stable keys for:

```txt
importCapabilities
importList(filters)
importDetail(importJobId)
importPreview(importJobId, page)
importValidationIssues(importJobId, filters)
importAudit(importJobId, filters)
```

Reuse existing:

- teams;
- matches;
- current session;
- audit components.

Use focused invalidation:

- upload -> import lists/detail;
- preview -> detail/preview/audit/list;
- validate -> detail/issues/audit/list;
- confirm -> detail/list/audit plus target-specific future queries;
- cancel -> detail/list/audit.

Do not clear the full query cache.

Do not optimistically assign workflow status before backend success.

### Processing refresh behavior

A job in `PARSING` needs eventual authoritative refresh.

Use conservative polling only while the selected detail job is actively `PARSING`.

Recommended default:

```txt
5 seconds
```

Rules:

- poll detail only, not every list row;
- stop polling when status leaves `PARSING`;
- stop when the detail Sheet closes;
- stop when the browser tab is hidden if the current TanStack Query conventions support it;
- do not poll faster than once every 5 seconds;
- list page does not continuously poll by default;
- manual refresh remains available.

This is UI refresh, not background processing.

Do not claim the browser is executing the import job.

### Role-aware behavior

#### `ADMIN`

May access the complete import workflow across teams.

#### `DATA_OPERATOR`

May access only when:

- `canImportData = true`;
- team is inside authorized scope.

#### Other roles

No import route/navigation/access.

Use session data for route/navigation hints only.

Job `allowedActions` remains authoritative for actual workflow actions.

### Shadcn-first component selection

Review all interactions against:

```txt
context/references/shadcn-components.md
```

Prefer suitable components such as:

- `Table`;
- `Sheet`;
- `Dialog`;
- `Alert Dialog`;
- `Tabs`;
- `Stepper` only if present in the approved catalog; otherwise compose a simple app-level progress list from existing primitives;
- `Card`;
- `Item`;
- `Badge`;
- `Progress`;
- `Input`;
- `Textarea`;
- `Select`;
- `Combobox`;
- `Command`;
- `Popover`;
- `Calendar`;
- `Alert`;
- `Empty`;
- `Skeleton`;
- `Pagination`;
- `Tooltip`;
- `Collapsible`;
- `Scroll Area`;
- `Sonner`;
- `Button`.

Do not add:

- a spreadsheet/data-grid package;
- a CSV parser;
- an XLSX parser;
- a drag-and-drop package;
- Axios solely for upload;
- a workflow/state-machine library;
- a timeline package.

Do not modify generated shadcn primitive files.

Do not apply ad-hoc visual overrides.

### Frontend organization

Create an imports feature:

```txt
frontend/src/features/imports/
├── api/
├── components/
├── hooks/
├── schemas/
├── types/
├── utils/
└── index.ts
```

Suggested components:

```txt
ImportListPage
ImportFilters
ImportTable
CreateImportDialog
ImportDetailSheet
ImportWorkflowView
ImportPreviewTable
ImportValidationSummary
ImportValidationIssuesTable
ImportAuditView
ImportStatusBadge
ImportCapabilitySummary
```

Reuse shared:

- audit components from Unit 39;
- XHR upload infrastructure patterns from Unit 42;
- team/match selectors;
- ProblemDetails handling;
- date/size formatting.

Do not store import server data in Zustand or React Context.

Avoid `any`.

### Localization

Visible copy is Bosnian Latin with proper characters.

Suggested labels include:

```txt
Importi
Novi import
Odaberite fajl
Vrsta importa
Izvor
Format
Selekcija
Utakmica
Opis
Učitaj izvorni fajl
Pregled
Validacija
Potvrda
Generiši pregled
Validiraj import
Potvrdi import
Otkaži import
Preuzmi izvorni fajl
Tok importa
Historija promjena
Obrada u toku
Spremno za potvrdu
Validacija nije prošla
Importovan
Otkazan
Obrada nije uspjela
Dostupno
Nije dostupno
```

Internal API values remain English.

Use centralized mappings.

Do not mix Bosnian and English labels on the operational screen except imported/source values and technical processor identifiers.

### Accessibility

Requirements:

- route, tabs, dialogs, Sheet, tables, filters, and pagination are keyboard accessible;
- file input has a visible associated label;
- upload progress has accessible current value;
- cancel/confirm dialogs explain consequences;
- status/capability states do not rely only on color;
- preview table remains semantic;
- validation issue severity has text;
- wide preview tables use contained horizontal scrolling;
- processing state is announced through accessible text;
- icon-only actions have labels/tooltips;
- focus returns correctly after dialogs close;
- audit nested tabs are keyboard navigable;
- no auto-focus behavior causes destructive confirmation accidentally.

### Responsive behavior

Desktop/tablet are primary for import work.

Requirements:

- import table uses contained horizontal overflow;
- detail Sheet uses wide bounded desktop layout;
- preview table scrolls inside its own container;
- validation issues remain readable;
- dialogs fit tablet/mobile viewports;
- upload progress/cancel remain visible;
- mobile supports viewing job state, issues, and lightweight actions;
- no separate mobile import product;
- do not hide required validation data on small screens.

### Error handling

Use existing ProblemDetails handling.

Expected behavior:

- `401`: existing auth/session flow;
- `403`: safe permission/scope feedback and session refetch when relevant;
- `404`: safe missing/inaccessible job;
- `409`: unsupported processor, invalid transition, active/stale lease, stale revision, or workflow conflict; refetch authoritative job/capabilities;
- `413`: show runtime max-size context;
- `415`: show accepted CSV/XLSX formats;
- `422`: map metadata/context validation;
- network error: preserve user-entered metadata and retry;
- malformed preview/issue response: show a compatibility error without crashing the whole page.

Do not display:

- storage keys;
- local paths;
- raw stack traces;
- parser internals;
- full source rows in errors;
- hidden entity existence.

Do not log upload bodies, preview rows, validation issues, or source-file data to the browser console.

### Tests and verification approach

Add frontend tests only if the repository already has an approved frontend testing foundation.

When tests exist, prioritize:

- status/action label mappings;
- capability combination lookup;
- file accept/max-size helpers;
- metadata-first multipart construction;
- XHR ProblemDetails parsing;
- route/filter URL state;
- unsupported processor rendering;
- allowed-action rendering;
- preview value rendering;
- validation severity mapping;
- safe result-summary rendering;
- audit action mapping.

Do not introduce a new frontend testing framework solely for Unit 44.

Manual verification must cover:

- admin navigation/access;
- data operator with import permission;
- data operator without import permission;
- out-of-scope team behavior;
- capabilities failure;
- CSV upload;
- XLSX upload;
- unsupported/oversized/empty file;
- match/type context rules;
- upload progress;
- upload abort/reconciliation;
- job detail URL restoration;
- no-processor truthful state;
- source download;
- preview action/result when a fake/test or future processor is registered;
- validation success/failure/warnings;
- paginated issues;
- explicit confirm availability;
- confirmation conflict;
- imported terminal state;
- cancellation;
- failed processing;
- active processing polling;
- stale-state refetch;
- audit history;
- responsive behavior;
- keyboard/focus behavior.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

Record:

- `/imports` route/navigation;
- capability-driven upload behavior;
- exact visible upload sequence;
- no-placeholder action rule;
- processor-unavailable UX;
- detail/preview/validation/confirmation/cancel surfaces;
- audit reuse;
- processing polling behavior;
- verification results;
- intentionally deferred mapping/parser/vendor work.

If the implemented Unit 43 response contracts differ from this spec, update the relevant context/spec before continuing.

Do not silently add client-side parsing, mapping, or vendor assumptions.

## Implementation

### 1. Add import frontend contracts and clients

Create typed API contracts for:

- capabilities;
- list;
- detail;
- upload;
- source URL;
- preview;
- validation issues;
- workflow actions;
- audit.

Use explicit TypeScript DTOs.

### 2. Add the `/imports` route and navigation

Add:

```txt
/imports
```

Wire permission-aware Performance navigation and route protection.

### 3. Build URL-driven list filters and table

Use:

- `nuqs`;
- TanStack Query;
- TanStack Table;
- real Unit 43 server filtering/pagination.

Add all required list states.

### 4. Build the new import dialog

Implement:

- file-first UX;
- import/source/context selection;
- capability summary;
- metadata validation;
- metadata-first multipart creation;
- XHR upload progress;
- cancellation/reconciliation;
- upload success -> detail Sheet.

Do not parse the file in the browser.

### 5. Build the import detail Sheet

Implement:

- metadata/status header;
- source section;
- processing state;
- workflow stepper;
- backend-owned actions;
- failure/result summaries;
- URL-addressable selected job.

### 6. Build preview UI

Implement:

- real preview action;
- backend preview query;
- dynamic TanStack Table columns;
- backend pagination;
- safe values;
- empty/error states.

Do not add mapping controls.

### 7. Build validation UI

Implement:

- validate action;
- status-aware result feedback;
- summary counts;
- paginated/filterable issue table;
- zero-issue and no-validation states.

### 8. Build explicit confirmation

Implement:

- `allowedActions`-driven button;
- full confirmation dialog;
- current job/validation summary;
- stale-state handling;
- focused invalidation.

No active button should appear when confirmation capability is absent.

### 9. Build cancellation and terminal states

Implement:

- cancel confirmation;
- imported state;
- cancelled state;
- failed state;
- source retention messaging.

Do not add restore/delete.

### 10. Add processing refresh behavior

Poll only the selected detail job while status is `PARSING`.

Stop polling when processing ends or Sheet closes.

### 11. Add import audit history

Reuse Unit 39 audit foundation.

Add:

```txt
Tok importa
Historija promjena
```

with scoped URL state and import action/field mappings.

### 12. Wire focused query invalidation

Refresh only affected import/detail/preview/issues/audit/target queries.

Do not clear the full cache.

### 13. Verify shadcn-first implementation

Review all new UI against the shadcn component catalog.

Remove unnecessary custom low-level primitives.

Confirm generated `frontend/src/components/ui/*` files remain unmodified.

### 14. Update progress documentation

Update `context/progress-tracker.md` with actual implementation and verification state.

Do not mark Unit 44 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use existing frontend packages and infrastructure:

- TanStack Query;
- `nuqs`;
- React Hook Form;
- Zod;
- TanStack Table;
- shadcn/ui;
- Lucide React;
- browser-native `XMLHttpRequest`;
- Unit 39 audit components;
- existing auth/session/team/match queries;
- existing ProblemDetails/date/size helpers.

Add required shadcn components just in time through the approved CLI workflow.

Do not add:

- CSV parser packages;
- XLSX/OpenXML packages;
- spreadsheet/data-grid packages;
- drag-and-drop packages;
- Axios solely for upload;
- workflow/state-machine libraries;
- timeline packages;
- another query/state library;
- a new frontend testing framework solely for this unit.

## Verification checklist

- [ ] `/imports` exists and uses real Unit 43 APIs.
- [ ] `Importi` appears in the Performance navigation only for admins and data operators with `canImportData`.
- [ ] Backend authorization remains authoritative for stale permission/scope state.
- [ ] Import capabilities load from `GET /api/imports/capabilities`.
- [ ] Runtime upload size and accepted CSV/XLSX formats are not hardcoded.
- [ ] Processor availability comes only from backend capabilities.
- [ ] No fake processor, parser, mapping, preview, validation, or confirmation capability is invented.
- [ ] No placeholder action button opens a “future update” toaster.
- [ ] Unsupported combinations show a truthful non-actionable capability state.
- [ ] Import list filters and pagination use `nuqs`.
- [ ] Search, team, match, type, source, format, status, date, page, and selected job state are URL-driven.
- [ ] Import filtering/pagination remains server-side.
- [ ] The import history table uses TanStack Table and shadcn table primitives.
- [ ] Import rows never expose storage keys, local paths, or source contents.
- [ ] Loading, no-data, no-results, error, and inaccessible states are implemented.
- [ ] `Novi import` uses a file-first visible workflow but sends one final metadata-first upload request.
- [ ] Upload metadata contains immutable type/source/team/match context required by Unit 43.
- [ ] Match-target requirements change correctly with import type.
- [ ] Source label appears only for `OTHER`.
- [ ] Upload accepts only runtime capability CSV/XLSX types.
- [ ] The browser does not parse CSV/XLSX source contents.
- [ ] Upload does not Base64 encode the file.
- [ ] Upload uses native XHR progress without adding Axios.
- [ ] Existing credentials and CSRF behavior are preserved.
- [ ] Upload abort warns about possible server completion and refetches imports.
- [ ] Successful upload opens the real job detail and does not automatically preview, validate, or confirm.
- [ ] Import detail is URL-addressable through `importJobId`.
- [ ] Detail shows safe source metadata, status, creator, team/match, and processing state.
- [ ] Processing lease ID is not exposed.
- [ ] Job-level mutation buttons come from backend `allowedActions`.
- [ ] Unknown future action codes do not create unsafe buttons.
- [ ] Source download uses the authorized Unit 43 source endpoint.
- [ ] Source download does not fetch the full file into JavaScript memory solely to trigger download.
- [ ] The workflow stepper is informational and does not mutate status.
- [ ] Preview completion is based on actual preview state, not merely `UPLOADED`.
- [ ] `GENERATE_PREVIEW` renders only when backend-provided.
- [ ] Preview query uses backend columns, rows, order, and pagination.
- [ ] Preview values are plain text/data and preserve `null` versus `0`.
- [ ] No editable column mapping UI is added before a backend mapping contract exists.
- [ ] `VALIDATE` renders only when backend-provided.
- [ ] Validation success feedback does not falsely imply zero errors when status is `VALIDATION_FAILED`.
- [ ] Validation summary distinguishes valid rows, invalid rows, and warnings.
- [ ] Validation issues use backend filters/pagination.
- [ ] Severity is displayed with readable text.
- [ ] Raw issue metadata/stack traces are not dumped into the main UI.
- [ ] `CONFIRM` renders only when backend-provided.
- [ ] Confirmation uses an explicit consequence dialog.
- [ ] Confirmation never sends a target status.
- [ ] Unit 43 alone produces no clickable confirmation placeholder because no real confirmation processor exists.
- [ ] Stale revision/version/workflow conflicts refetch authoritative state.
- [ ] `CANCEL` renders only when backend-provided.
- [ ] Cancellation clearly preserves source and history while making the job terminal.
- [ ] No restore, reopen, delete, or archive action exists.
- [ ] `FAILED` shows only safe failure code/message and backend-provided retry actions.
- [ ] `IMPORTED` is displayed as terminal.
- [ ] Result summary uses safe bounded structured rendering rather than raw JSON.
- [ ] Selected `PARSING` job detail polls no faster than every 5 seconds.
- [ ] Polling stops when processing ends or detail closes.
- [ ] The list page does not continuously poll all jobs by default.
- [ ] Import audit history uses the Unit 43 endpoint and Unit 39 shared components.
- [ ] `Tok importa` and `Historija promjena` are separate nested views.
- [ ] Import audit action/field labels are centralized.
- [ ] Missing historical events are not reconstructed client-side.
- [ ] Admin has cross-team import UI.
- [ ] Data operator requires `canImportData` and team scope.
- [ ] Other roles cannot see or open import UI.
- [ ] Backend `401`, `403`, `404`, `409`, `413`, `415`, `422`, network, and malformed-response states are handled safely.
- [ ] Import server data is not stored in Zustand or React Context.
- [ ] Query invalidation is focused.
- [ ] No optimistic local workflow status assignment occurs before backend success.
- [ ] New UI follows shadcn-first component-selection guidance.
- [ ] Generated `frontend/src/components/ui/*` files are not manually modified.
- [ ] No CSV/XLSX parser, spreadsheet grid, drag-and-drop, Axios, workflow engine, or timeline dependency is added.
- [ ] No raw Tailwind palette classes or hardcoded component colors are introduced.
- [ ] No ad-hoc visual overrides are applied to shadcn components.
- [ ] Frontend imports use `@/` instead of deep relative paths.
- [ ] Visible Bosnian copy uses proper Bosnian Latin characters.
- [ ] File input, progress, dialogs, Sheet, tabs, tables, issue filters, confirmations, and pagination are keyboard accessible.
- [ ] Desktop, tablet, and mobile layouts remain usable.
- [ ] No backend files, migrations, processor implementations, parser packages, vendor mappings, or official data mutation logic are added.
- [ ] `npm run format` completes successfully.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] Frontend typecheck/build passes using the configured project command.
- [ ] Existing approved frontend tests pass when present.
- [ ] `context/progress-tracker.md` reflects actual Unit 44 route, capability handling, workflow UI, polling, audit reuse, and verification state.
