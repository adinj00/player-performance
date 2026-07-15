# Unit 43: Import Workflow Backend Foundation

## Goal

Build the secure backend workflow foundation for auditable CSV/XLSX imports without guessing vendor columns or mutating official data before explicit confirmation. Add persistent import jobs, original-file retention through Unit 40 storage, backend-owned statuses and allowed actions, preview/validation result persistence, processor registries, explicit confirmation orchestration contracts, cancellation, source download, authorization, semantic audit coverage, and safe processing leases without adding CSV/XLSX reader packages, vendor mappings, imported GPS/statistics models, background workers, or frontend UI.

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
8. `context/feature-specs/00-build-plan.md`
9. `context/feature-specs/22-teams-selections-settings-backend.md`
10. `context/feature-specs/23-staff-users-team-scope-account-lifecycle-backend.md`
11. `context/feature-specs/30-matches-backend-foundation.md`
12. `context/feature-specs/38-audit-backend-foundation.md`
13. `context/feature-specs/40-file-storage-abstraction.md`
14. `context/feature-specs/41-media-backend-foundation.md`
15. `context/feature-specs/43-import-workflow-backend-foundation.md`

Use relevant installed backend Codex skills/plugins when applicable.

Skills/plugins may guide implementation workflow but must not override project context, architecture rules, code standards, or this spec.

This unit is backend-only.

Do not add or change frontend routes, pages, components, navigation, shadcn/ui components, or frontend API wrappers.

### Scope

This unit introduces:

- persistent `ImportJob` workflow records;
- original CSV/XLSX retention through one explicitly owned `StoredFile`;
- stable import type, source system, file format, status, processing operation, validation severity, and allowed-action values;
- streamed multipart import upload;
- import upload capabilities query;
- team- and optional match-scoped import metadata;
- list and detail queries;
- secure original-source download;
- preview column and preview row persistence;
- validation issue persistence;
- validation and result summaries;
- processor registration/discovery contracts;
- preview, validation, and confirmation orchestration endpoints;
- explicit confirmation safety and revision checks;
- cancellation;
- processing leases and stale-processing recovery rules;
- role, permission, and team-scope authorization;
- semantic import audit events;
- import-specific audit history query;
- EF Core mapping, migration, and focused tests.

This unit does not introduce:

- CSV parsing packages;
- XLSX reader packages;
- automatic delimiter/header detection;
- formula evaluation;
- vendor-specific mappings;
- Gpexe field mappings;
- Zone14 field mappings;
- generic column-mapping UI;
- imported player roster mutations;
- imported match statistics mutations;
- GPS/physical metric models;
- training sessions;
- background jobs;
- message queues;
- scheduled processors;
- automatic confirmation;
- import rollback;
- import deletion;
- import archive/restore;
- public source-file URLs;
- media-item creation for import files;
- frontend UI.

Unit 44 builds the workflow screens against this API foundation.

Unit 45 adds generic CSV/XLSX reading and preview support.

Units 46 and 47 add vendor-specific processing only after real samples/documentation confirm available fields.

### Import files are not media items

An import source file is owned directly by its `ImportJob`.

Model:

```txt
ImportJob -> StoredFile
```

Do not create a `MediaItem` for the spreadsheet.

Do not route import uploads through `/api/media`.

Do not add a generic media/import polymorphic link.

The same Unit 40 storage infrastructure is reused, but media and imports remain separate owning workflows with separate authorization, accepted formats, lifecycle, and API routes.

### Core import guarantees

The following guarantees are mandatory:

1. The original source file is retained before preview/validation/confirmation.
2. Preview does not mutate official data.
3. Validation does not mutate official data.
4. Confirmation is explicit.
5. Only a currently validated job may be confirmed.
6. A stale validation result cannot be confirmed after configuration or processor changes.
7. Confirmation and official data mutations commit atomically.
8. Failed or cancelled jobs retain their original file and history.
9. Unknown vendor fields are never guessed.
10. No processor means no processing—not a fallback mapping.
11. Backend authorization and allowed actions remain authoritative.
12. Important import workflow changes are audited.

### Stable import types

Use centralized stable English values:

```txt
PLAYER_ROSTER
MATCH_PLAYER_STATISTICS
MATCH_GPS
TRAINING_GPS
```

These values describe intended data ownership, not a confirmed vendor schema.

Rules:

- persisted/API values remain English;
- do not add field-specific assumptions to the enum;
- `MATCH_PLAYER_STATISTICS` requires a match target;
- `MATCH_GPS` requires a match target;
- `PLAYER_ROSTER` requires a team and no match target;
- `TRAINING_GPS` requires a team and no match target in Unit 43 because training sessions do not yet exist;
- later Unit 48 may add an explicit training-session relationship without changing historical source-file ownership;
- no import type may silently mutate official data until a matching confirmation handler exists.

Do not add a generic `OTHER` import type that bypasses target semantics.

Adding a new import type later requires an explicit context/spec update.

### Stable source systems

Use centralized stable English values:

```txt
GENERIC
GPEXE
ZONE14
OTHER
```

Source system identifies where the file originated.

Rules:

- it does not imply that a processor exists;
- it does not prove file authenticity;
- it does not select guessed field mappings;
- `OTHER` requires a bounded optional source label;
- Gpexe and Zone14 jobs may be uploaded and retained before their mappings exist;
- processing capabilities must truthfully indicate whether preview, validation, and confirmation are available for the selected combination.

Do not enforce speculative vendor/import-type compatibility before samples are confirmed.

### Stable file formats

Unit 43 accepts only:

```txt
CSV
XLSX
```

Approved extension/content-type pairs:

#### CSV

```txt
.csv
text/csv
application/csv
application/vnd.ms-excel
```

#### XLSX

```txt
.xlsx
application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
```

Rules:

- extension comparison is case-insensitive;
- content type comparison is case-insensitive after normalization;
- extension and content type must form an approved pair;
- `.xls` is not accepted;
- `text/plain` and `application/octet-stream` are not accepted as fallback bypasses;
- zero-byte files are rejected;
- file content is not parsed or sniffed in Unit 43;
- accepted metadata does not prove the file is structurally valid;
- structural validity begins in Unit 45.

Do not accept CSV/XLSX through media upload merely to bypass these rules.

### Import statuses

Use the architecture-defined statuses:

```txt
UPLOADED
PARSING
VALIDATION_FAILED
READY_TO_CONFIRM
IMPORTED
FAILED
CANCELLED
```

Semantic meaning:

#### `UPLOADED`

The original file and import metadata are retained.

A preview may or may not exist.

No current successful validation authorizes confirmation.

#### `PARSING`

A preview, validation, or confirmation operation currently owns the processing lease.

Despite the historical name, this status is the V1 transient processing lock for all three synchronous workflow operations.

#### `VALIDATION_FAILED`

The latest validation completed successfully as a process, but one or more blocking validation issues prevent confirmation.

#### `READY_TO_CONFIRM`

The latest validation completed without blocking errors for the current configuration revision and processor version.

No official data has been mutated yet.

#### `IMPORTED`

Explicit confirmation successfully mutated official data and committed the import result.

Terminal.

#### `FAILED`

A technical preview, validation, or confirmation operation failed and the safe failure state was persisted.

The original file remains.

#### `CANCELLED`

An authorized user cancelled the job before successful import.

Terminal.

### Status transitions

Allowed transitions:

```txt
UPLOADED -> PARSING

VALIDATION_FAILED -> PARSING
FAILED -> PARSING
READY_TO_CONFIRM -> PARSING

PARSING -> UPLOADED
PARSING -> VALIDATION_FAILED
PARSING -> READY_TO_CONFIRM
PARSING -> IMPORTED
PARSING -> FAILED

UPLOADED -> CANCELLED
VALIDATION_FAILED -> CANCELLED
READY_TO_CONFIRM -> CANCELLED
FAILED -> CANCELLED

stale PARSING -> CANCELLED
stale PARSING -> PARSING through an authorized replacement lease
```

Interpretation:

- preview success returns to `UPLOADED`;
- validation success moves to `READY_TO_CONFIRM`;
- validation with blocking issues moves to `VALIDATION_FAILED`;
- confirmation success moves to `IMPORTED`;
- technical operation failure moves to `FAILED`;
- active non-stale `PARSING` cannot be cancelled or replaced;
- `IMPORTED` and `CANCELLED` are terminal;
- status changes occur only through domain/workflow methods;
- endpoint handlers and clients never assign status directly.

Do not add hidden transitions such as:

```txt
CANCELLED -> UPLOADED
IMPORTED -> READY_TO_CONFIRM
IMPORTED -> CANCELLED
```

A new attempt uses a new import job and original upload.

### Processing operations

Use stable operation values:

```txt
PREVIEW
VALIDATION
CONFIRMATION
```

Persist transient lease metadata:

```txt
ProcessingOperation
ProcessingLeaseId
ProcessingStartedAtUtc
ProcessingRequestedByUserId
ProcessingProcessorKey
ProcessingProcessorVersion
```

These fields are populated only while status is `PARSING`.

They are cleared when the operation finalizes.

The lease ID is server-generated and never supplied by the client.

### Processing lease and stale recovery

Do not keep a database transaction open while reading and processing an entire import file.

Use a lease workflow:

1. authorize and validate the requested operation;
2. in a short transaction, acquire a unique processing lease and set `PARSING`;
3. commit the lease;
4. open/process the immutable source file outside the database transaction;
5. in a new transaction, finalize only if the lease still matches;
6. write outcome state, preview/issues/result data, and audit;
7. clear lease fields;
8. commit.

Add typed configuration:

```txt
Imports__ProcessingLeaseTimeoutMinutes
```

Rules:

- value is positive and bounded;
- a non-stale lease cannot be replaced;
- an authorized request may replace a stale lease;
- stale replacement uses a new lease ID;
- the old operation cannot later finalize because its lease no longer matches;
- stale cancellation clears the lease and sets `CANCELLED`;
- normal cancellation does not interrupt an active non-stale operation;
- no background cleanup service is added;
- UI/manual retry triggers stale recovery;
- lease times use the approved UTC clock.

When an operation loses its lease before finalization:

- discard its generated in-memory result;
- do not mutate official data;
- do not overwrite the current job state;
- return a safe stale-operation conflict;
- log operation/job/lease identifiers without row data or file content.

### Import job model

Add a persistent aggregate equivalent to:

```txt
ImportJob
```

Required fields:

```txt
Id
TeamId
MatchId
StoredFileId
ImportType
SourceSystem
SourceLabel
FileFormat
Status
Description
CreatedByUserId
CreatedAtUtc
UpdatedAtUtc

ConfigurationRevision
ValidatedConfigurationRevision
ValidatedProcessorKey
ValidatedProcessorVersion
ValidatedAtUtc

PreviewGeneratedAtUtc
ValidationCompletedAtUtc

TotalRowCount
PreviewRowCount
ValidRowCount
InvalidRowCount
WarningCount

FailureCode
FailureMessage
ResultSummaryJson

ProcessingOperation
ProcessingLeaseId
ProcessingStartedAtUtc
ProcessingRequestedByUserId
ProcessingProcessorKey
ProcessingProcessorVersion

ConfirmedByUserId
ConfirmedAtUtc
CancelledByUserId
CancelledAtUtc
```

Use existing identifier and UTC conventions.

Rules:

- `TeamId` is required and immutable;
- `MatchId` is optional and immutable;
- `StoredFileId` is required, unique, and immutable;
- import type, source system, source label, file format, and original source are immutable;
- description is optional, trimmed, and bounded;
- source label is permitted only for `OTHER`;
- configuration revision starts at `1`;
- validated revision is nullable until successful validation;
- counts are nullable until produced;
- failure code/message contain safe bounded operational information;
- result summary is structured `jsonb`;
- confirmation actor/time are both present or both absent;
- cancellation actor/time are both present or both absent;
- no hard-delete or archive fields are added.

Recommended bounds unless existing conventions define stricter values:

```txt
SourceLabel: 100
Description: 1,000
FailureCode: 100
FailureMessage: 1,000
ProcessorKey: 200
ProcessorVersion: 100
```

### Target-context rules

Every import is team-scoped.

#### `PLAYER_ROSTER`

Requires:

- active/non-archived team;
- no `MatchId`.

#### `MATCH_PLAYER_STATISTICS`

Requires:

- existing non-archived match;
- job `TeamId` equals immutable match `TeamId`.

#### `MATCH_GPS`

Requires:

- existing non-archived match;
- job `TeamId` equals immutable match `TeamId`.

#### `TRAINING_GPS`

Requires:

- active/non-archived team;
- no match target in Unit 43.

Do not create a generic target type/ID pair.

Do not add a training-session foreign key before Unit 48 exists.

Upload-time context validation does not replace confirmation-time checks.

Future confirmation handlers must re-check current target lifecycle, report workflow locks, and domain constraints immediately before mutation.

### Original stored file ownership

Each import job owns exactly one `StoredFile`.

Rules:

- `StoredFileId` has a unique relationship to one import job;
- the source file is immutable;
- source file metadata comes from Unit 40;
- actual bytes written determine stored size;
- storage key is never returned through import DTOs;
- source file is retained for `UPLOADED`, `VALIDATION_FAILED`, `READY_TO_CONFIRM`, `IMPORTED`, `FAILED`, and `CANCELLED`;
- cancellation/failure does not archive or physically delete the file;
- no normal import endpoint deletes source bytes or metadata;
- import source retention has no V1 cleanup deadline;
- future retention changes require a separate policy/spec.

### Import upload size configuration

Add typed options equivalent to:

```txt
ImportOptions
```

Configuration:

```txt
Imports__MaxUploadSizeBytes
Imports__PreviewRowLimit
Imports__ProcessingLeaseTimeoutMinutes
```

Rules:

- maximum upload size is positive;
- maximum upload size is less than or equal to Unit 40 `FileStorage__MaxObjectSizeBytes`;
- preview row limit is positive and bounded;
- processing lease timeout is positive and bounded;
- startup validation fails clearly for invalid values;
- no secrets or storage paths appear in validation errors.

Update `backend/.env.example` with safe development examples and comments.

Do not hardcode a single browser-visible upload limit as the source of truth.

### Import capabilities endpoint

Add:

```txt
GET /api/imports/capabilities
```

Authorization:

- authenticated active user;
- caller must satisfy import-workflow access:
  - `ADMIN`; or
  - `DATA_OPERATOR` with `canImportData = true`;
- no team parameter is required.

Return at minimum:

```txt
maxUploadSizeBytes
previewRowLimit
processingLeaseTimeoutMinutes

fileTypes:
  fileFormat
  extensions
  contentTypes

importTypes
sourceSystems

processorCapabilities:
  importType
  sourceSystem
  fileFormat
  canPreview
  canValidate
  canConfirm
  processorKey
  processorVersion
```

Rules:

- processor capabilities come from the registered processor registry;
- Unit 43 may return no available processors;
- unsupported combinations remain truthfully unavailable;
- do not claim Gpexe/Zone14 support before Units 46/47;
- do not expose storage provider, storage path, storage key, secrets, or implementation type names;
- deterministic ordering;
- no database access required except normal user authorization;
- no migration specific to this endpoint.

### Processor abstractions

Add Application-layer provider-independent contracts equivalent to:

```txt
IImportProcessorRegistry
IImportWorkflowProcessor
```

A processor is selected by:

```txt
ImportType
SourceSystem
FileFormat
```

A processor declares:

```txt
ProcessorKey
ProcessorVersion
CanGeneratePreview
CanValidate
CanConfirm
```

Conceptual asynchronous operations:

```txt
GeneratePreviewAsync
ValidateAsync
ConfirmAsync
```

Contracts receive:

- immutable import job context;
- readable original source stream;
- current configuration revision;
- cancellation token;
- required repositories/domain services through normal DI—not endpoint objects.

Contracts return safe structured outcomes.

Do not pass:

- `HttpContext`;
- `IFormFile`;
- local paths;
- storage provider clients;
- arbitrary endpoint DTOs;
- current user IDs supplied by clients.

Unit 43 registers the registry infrastructure but no actual CSV/XLSX processor.

If no processor/capability exists:

- the action is omitted from `allowedActions`;
- direct calls return a safe `409`/unsupported-processing result;
- status remains unchanged;
- no audit entry is created.

Do not provide a “best effort” generic parser in Unit 43.

### Preview result model

Persist bounded preview data independently from official domain data.

Add:

```txt
ImportPreviewColumn
ImportPreviewRow
```

`ImportPreviewColumn` contains at minimum:

```txt
Id
ImportJobId
Ordinal
SourceHeader
NormalizedHeader
DetectedDataType
```

`ImportPreviewRow` contains at minimum:

```txt
Id
ImportJobId
SourceRowNumber
ValuesJson
```

Rules:

- columns are ordered by `Ordinal`;
- source row number refers to the original file row where meaningful;
- row values are structured `jsonb`;
- preview rows are bounded by `Imports__PreviewRowLimit`;
- preview values are plain data and never executed;
- formulas/macros are not evaluated;
- preview generation does not mutate official entities;
- preview replacement removes/replaces previous preview records only after the new result has been successfully produced and the lease is still valid;
- no partial preview is committed after processor failure;
- no full-file row persistence is required.

`DetectedDataType` is an optional generic presentation hint only.

Do not infer domain field mappings in Unit 43.

### Validation issue model

Add:

```txt
ImportValidationIssue
```

Required fields:

```txt
Id
ImportJobId
Severity
Code
Message
SourceRowNumber
ColumnKey
MetadataJson
CreatedAtUtc
```

Stable severities:

```txt
ERROR
WARNING
```

Rules:

- code is stable English and bounded;
- message is safe, user-presentable, and bounded;
- row/column are optional for file-level issues;
- metadata is structured `jsonb`;
- issues are replaced as one validation outcome;
- `ERROR` blocks confirmation;
- `WARNING` does not block confirmation by itself;
- counts on the import job match persisted issue/row summaries;
- validation issues do not mutate official data;
- do not persist exception stack traces, connection details, storage paths, or secrets.

### Import result summary

`ResultSummaryJson` stores a bounded, structured summary of confirmation outcome.

It may contain:

```txt
importedRecordCounts
updatedRecordCounts
skippedRecordCounts
targetIds or bounded target summaries
```

Rules:

- no full imported dataset duplication;
- no source file contents;
- no passwords/tokens;
- no storage key/path;
- bounded collection sizes;
- target-specific confirmation handlers define their safe summary schema;
- imported official records must retain `ImportJobId`/source provenance where their future domain model requires it.

Do not create target-specific result fields before the target import handler exists.

### Configuration revision and stale confirmation protection

`ConfigurationRevision` represents all job settings that affect parsing, mapping, validation, or confirmation.

Unit 43 has no public mapping update endpoint, but the revision fields must exist for Unit 45 and later units.

Rules:

- revision starts at `1`;
- future mapping/configuration changes increment it;
- successful validation stores:
  - current revision;
  - processor key;
  - processor version;
  - validation time;
- confirmation requires:
  - status `READY_TO_CONFIRM`;
  - current revision equals validated revision;
  - current registered processor key/version equals validated key/version;
  - processor supports confirmation;
- any configuration change invalidates readiness and returns status to `UPLOADED` or the future mapping workflow's approved editable state;
- processor version changes require revalidation;
- client cannot submit or override revision values.

Do not confirm stale validation output.

### Backend-owned allowed actions

Return `allowedActions` in import detail/list items where practical.

Stable actions:

```txt
VIEW
DOWNLOAD_SOURCE
GENERATE_PREVIEW
VALIDATE
CONFIRM
CANCEL
```

Calculate actions from:

- current user;
- role;
- `canImportData`;
- team scope;
- job status;
- lease state/staleness;
- processor capabilities;
- validation revision;
- source-file availability.

Rules:

- `VIEW` and `DOWNLOAD_SOURCE` require import workflow access and scope;
- `GENERATE_PREVIEW` requires preview capability and a compatible non-terminal state;
- `VALIDATE` requires validation capability and a compatible non-terminal state;
- `CONFIRM` requires `READY_TO_CONFIRM`, current validation revision/version, and confirmation capability;
- `CANCEL` is available only before import and when no active non-stale lease exists;
- terminal jobs expose no mutation action;
- frontend visibility never replaces backend enforcement;
- unknown future actions must not be inferred client-side.

### Authorization model

Add or reuse a policy equivalent to:

```txt
CanImportData
```

Allowed users:

#### `ADMIN`

May:

- upload;
- list/view/download;
- preview;
- validate;
- confirm;
- cancel;

for all teams.

`canImportData` flag is not required for administrators.

#### `DATA_OPERATOR`

May perform import workflow operations only when:

- `canImportData = true`;
- the target team is in authorized scope.

#### Other roles

No import-job access in Unit 43, even if a misconfigured permission flag is present.

Rules:

- explicit out-of-scope `teamId` returns `403`;
- inaccessible import IDs use established safe non-disclosure behavior;
- target match visibility/team scope must be checked;
- source download uses the same authorization;
- audit history uses the same authorization;
- account disable/session invalidation behavior remains authoritative.

Do not create a generic “all staff can see imports” rule.

### Upload endpoint

Add:

```txt
POST /api/imports
```

Use streaming multipart parsing.

The request contains exactly:

- one bounded metadata JSON section;
- one file section.

Metadata fields:

```txt
teamId
matchId
importType
sourceSystem
sourceLabel
description
```

Rules:

- metadata section must appear before file section;
- require exactly one metadata section;
- require exactly one file section;
- reject duplicate/extra sections;
- bound metadata section size, for example 64 KiB;
- authenticate, authorize, and validate target context before committed storage ownership;
- stream through Unit 40;
- do not buffer entire file;
- do not use Base64;
- do not use `ReadToEnd`;
- do not depend on fully buffered `IFormFile` for the source;
- normalize original filename through Unit 40;
- validate extension/content type and declared size;
- actual written bytes are authoritative;
- client does not supply storage key;
- new job starts in `UPLOADED`;
- no preview/validation/confirmation runs automatically;
- mutation requires existing CSRF protection.

### Upload orchestration and compensation

Use Unit 40's compensation pattern:

1. authenticate and authorize;
2. validate metadata and target context;
3. generate trusted storage key;
4. stream original file;
5. begin/use database transaction;
6. create `StoredFile`;
7. create `ImportJob`;
8. write `IMPORT_JOB_CREATED` audit event;
9. commit;
10. if database/audit persistence fails, call `DeleteIfExistsAsync` for the uncommitted object.

Rules:

- success requires object write and database/audit commit;
- compensation failure is logged safely and never returns false success;
- original file remains after committed job creation;
- do not create a media item;
- do not automatically begin parsing;
- no processor is required to upload/retain a job.

### Import list API

Add:

```txt
GET /api/imports
```

Support bounded server-side filters:

```txt
teamId
matchId
importType
sourceSystem
fileFormat
status
search
dateFrom
dateTo
page
pageSize
```

Rules:

- team scope always applies;
- explicit unauthorized team filter returns `403`;
- search covers safe metadata such as original filename, description, source label, and job ID where supported;
- search does not inspect file bytes, preview rows, or validation messages;
- deterministic ordering:
  - `CreatedAtUtc` descending;
  - `Id` descending;
- pagination is database-backed;
- no unbounded history;
- no client-supplied arbitrary sorting.

Return compact items with:

```txt
id
team summary
match summary when present
importType
sourceSystem
sourceLabel
fileFormat
status
originalFileName
contentType
sizeBytes
createdBy summary
createdAtUtc
updatedAtUtc
row/validation counts
processing summary
allowedActions
```

Never return `StorageKey` or local path.

### Import detail API

Add:

```txt
GET /api/imports/{importJobId}
```

Return:

- full safe immutable metadata;
- team/match summary;
- original file metadata;
- current status;
- processing lease summary without exposing lease ID unless needed only as opaque diagnostics;
- preview/validation timestamps;
- counts;
- failure summary;
- result summary;
- confirmation/cancellation metadata;
- processor capability/validated processor summary;
- `allowedActions`.

Do not return:

- storage key;
- absolute path;
- file bytes;
- full preview rows;
- full validation issue list;
- stack traces;
- secrets.

Preview rows and validation issues use dedicated paginated endpoints.

### Original source download

Add:

```txt
GET /api/imports/{importJobId}/source
```

Rules:

- authorize before opening storage;
- available for every retained status;
- use `StoredFile` filename/content type;
- force attachment disposition;
- include `X-Content-Type-Options: nosniff`;
- never expose storage key/path;
- stream through Unit 40;
- do not Base64 encode;
- missing physical object after valid metadata returns a safe operational error and is logged;
- no public or presigned URL;
- cancellation/failed validation does not remove download access for authorized users.

### Preview action and query

Add:

```txt
POST /api/imports/{importJobId}/preview
GET  /api/imports/{importJobId}/preview
```

`POST /preview`:

- requires `GENERATE_PREVIEW`;
- acquires a `PREVIEW` lease;
- invokes the registered preview processor;
- persists a complete bounded preview only after successful processing and valid lease finalization;
- sets status back to `UPLOADED`;
- records preview timestamp/count;
- clears any stale failure state;
- does not mark the job ready to confirm;
- does not mutate official data;
- creates `IMPORT_JOB_PREVIEW_GENERATED` audit;
- returns safe outcome.

If no preview processor exists:

- return `409`;
- keep status unchanged;
- create no audit.

`GET /preview` supports:

```txt
page
pageSize
```

Return:

```txt
columns
rows
pagination
generatedAtUtc
previewRowCount
totalRowCount when known
```

Rules:

- backend pagination even though preview is bounded;
- preserve source column order and row order;
- values remain structured/plain;
- no HTML execution;
- no mapping assumptions;
- an absent preview returns a neutral empty/not-generated response, not fabricated rows.

### Validation action and issue query

Add:

```txt
POST /api/imports/{importJobId}/validate
GET  /api/imports/{importJobId}/validation-issues
```

`POST /validate`:

- requires `VALIDATE`;
- acquires a `VALIDATION` lease;
- invokes the registered validator;
- replaces the previous validation issue set atomically at lease finalization;
- stores row/issue counts and validation timestamp;
- stores processor key/version and current configuration revision;
- moves to:
  - `READY_TO_CONFIRM` when there are no blocking errors;
  - `VALIDATION_FAILED` when one or more errors exist;
  - `FAILED` for technical processor failure;
- never mutates official data;
- creates:
  - `IMPORT_JOB_READY_TO_CONFIRM`; or
  - `IMPORT_JOB_VALIDATION_FAILED`; or
  - `IMPORT_JOB_PROCESSING_FAILED`,
  according to the committed outcome.

A validation outcome with errors is a successfully completed validation process and may be audited.

`GET /validation-issues` supports:

```txt
severity
code
page
pageSize
```

Rules:

- database-backed filtering/pagination;
- deterministic order by source row, column, severity/code, and ID;
- file-level issues remain visible;
- no raw exception/internal data;
- zero issues is a valid response;
- summary counts come from persisted job state.

### Confirmation action

Add:

```txt
POST /api/imports/{importJobId}/confirm
```

Requirements:

- `CONFIRM` is present;
- status is `READY_TO_CONFIRM`;
- validation revision equals current configuration revision;
- validated processor key/version matches the current registered confirmation processor;
- original source file still exists;
- target context is reauthorized and revalidated;
- confirmation handler supports this exact import/source/format combination.

Confirmation workflow:

1. acquire a `CONFIRMATION` lease;
2. open immutable source file;
3. let the confirmation handler prepare/validate the official mutation;
4. begin database transaction;
5. re-check lease, status, revision, processor version, authorization, target lifecycle, and workflow locks;
6. apply official mutations;
7. persist provenance using `ImportJobId`, source system, importing user/time where the target model requires it;
8. set job `IMPORTED`;
9. store bounded result summary and confirmation metadata;
10. write `IMPORT_JOB_CONFIRMED` audit;
11. write target-specific audit events required by existing audited domains, including `importJobId` metadata;
12. commit atomically.

Rules:

- no automatic confirmation after validation;
- no client-provided target status;
- no partial official mutation;
- no “best effort” row commit in Unit 43;
- one failed target mutation rolls back the full confirmation unless a future import type explicitly defines a different transactional model;
- official data and job `IMPORTED` status cannot diverge;
- technical failure may set `FAILED` only in a separate safe finalization transaction when no official mutation committed;
- semantic/current-target validation failure returns `VALIDATION_FAILED` with refreshed issues where the processor supports it;
- no confirmation processor in Unit 43, so no job can become imported yet.

### Confirmation handler requirements for future units

Every future confirmation handler must define:

- exact target domain;
- exact accepted columns/mapping;
- validation rules;
- transaction boundary;
- duplicate/upsert behavior;
- target workflow locks;
- provenance fields;
- target-specific audit events;
- safe result summary;
- idempotency/concurrency behavior.

Do not add a universal reflection-based row-to-entity importer.

Do not allow a generic parser to directly mutate arbitrary tables.

### Cancellation

Add:

```txt
POST /api/imports/{importJobId}/cancel
```

Rules:

- requires `CANCEL`;
- allowed from:
  - `UPLOADED`;
  - `VALIDATION_FAILED`;
  - `READY_TO_CONFIRM`;
  - `FAILED`;
  - stale `PARSING`;
- not allowed for active non-stale `PARSING`;
- not allowed for `IMPORTED` or `CANCELLED`;
- sets cancellation actor/time;
- clears stale lease fields;
- preserves source file;
- preserves preview, validation issues, result/failure history already persisted;
- creates `IMPORT_JOB_CANCELLED` audit;
- no restore/reopen endpoint;
- a new attempt requires a new job/upload.

Do not use HTTP delete.

### Failure handling

Safe failure fields:

```txt
FailureCode
FailureMessage
```

Rules:

- values are bounded and user-presentable;
- no stack trace;
- no local path;
- no connection details;
- no storage key;
- no source row dump;
- no secrets;
- processor exceptions are logged through normal server logging with safe context;
- failed preview/validation/confirmation leaves source file retained;
- failed operation clears the matching lease only when it still owns the lease;
- stale operation cannot overwrite a newer state.

Use stable error codes where practical, such as:

```txt
PROCESSOR_NOT_AVAILABLE
SOURCE_FILE_MISSING
SOURCE_FILE_INVALID
PROCESSING_LEASE_STALE
PROCESSING_FAILED
CONFIRMATION_TARGET_CHANGED
VALIDATION_REVISION_STALE
```

Do not expose raw exception messages as the public failure message.

### Audit integration

Extend Unit 38 with:

```txt
EntityType = IMPORT_JOB
```

Add centralized action codes:

```txt
IMPORT_JOB_CREATED
IMPORT_JOB_PREVIEW_GENERATED
IMPORT_JOB_VALIDATION_FAILED
IMPORT_JOB_READY_TO_CONFIRM
IMPORT_JOB_CONFIRMED
IMPORT_JOB_CANCELLED
IMPORT_JOB_PROCESSING_FAILED
```

Audit successful committed workflow outcomes only.

Safe audit payloads may include:

```txt
teamId
matchId
importType
sourceSystem
sourceLabel
fileFormat
status
originalFileName
contentType
sizeBytes
processorKey
processorVersion
configurationRevision
totalRowCount
previewRowCount
validRowCount
invalidRowCount
warningCount
failureCode
result counts
```

Do not include:

- storage key;
- absolute path;
- file bytes;
- complete preview rows;
- complete validation issue messages/metadata;
- complete imported dataset;
- passwords/tokens;
- request body;
- processing lease ID.

For confirmation:

- include bounded imported/updated/skipped counts;
- include target IDs only when bounded and safe;
- include `ImportJobId` in target-specific audit metadata.

No audit entry is created for:

- unauthorized requests;
- invalid upload;
- unsupported direct processor call;
- active lease conflict;
- failed database transaction;
- stale operation that loses its lease before finalization.

A persisted `VALIDATION_FAILED` or `FAILED` outcome may have an audit event because the workflow outcome itself committed successfully.

### Import audit history API

Add:

```txt
GET /api/imports/{importJobId}/audit
```

Use the Unit 38 structured history response.

Authorization:

- same import workflow access and team scope as import detail;
- inaccessible job uses safe non-disclosure behavior.

Support:

```txt
page
pageSize
action
dateFrom
dateTo
```

Rules:

- bounded database pagination;
- deterministic newest-first ordering;
- no global import audit endpoint;
- no audit mutation endpoint;
- original source file values/rows are not returned through audit.

Unit 44 may choose whether to render this history; the backend foundation must be ready.

### API surface

Required routes:

```txt
GET  /api/imports/capabilities
GET  /api/imports
GET  /api/imports/{importJobId}
GET  /api/imports/{importJobId}/source
GET  /api/imports/{importJobId}/preview
GET  /api/imports/{importJobId}/validation-issues
GET  /api/imports/{importJobId}/audit

POST /api/imports
POST /api/imports/{importJobId}/preview
POST /api/imports/{importJobId}/validate
POST /api/imports/{importJobId}/confirm
POST /api/imports/{importJobId}/cancel
```

Do not add:

- generic stored-file endpoints;
- generic database import endpoints;
- status patch endpoints;
- mapping endpoints before Unit 45 defines the contract;
- delete/archive/restore endpoints;
- background-job control endpoints.

### Error handling

Use existing Result and ProblemDetails conventions.

Expected responses:

- `400` malformed multipart/query syntax;
- `401` unauthenticated;
- `403` explicit out-of-scope team or missing import permission;
- `404` missing or safely inaccessible job/target/source metadata;
- `409` unsupported processor action, invalid transition, active lease, stale revision, processor-version mismatch, or confirmation conflict;
- `413` oversized upload where practical;
- `415` unsupported file/multipart type;
- `422` semantic metadata/context validation when consistent with current API conventions;
- safe `5xx` for storage/database/processor failures.

Do not leak:

- storage keys;
- local paths;
- parser stack traces;
- SQL/provider details;
- source row values;
- hidden target existence;
- permission internals.

### Data retention and privacy

Import workflow data may contain sensitive internal performance information.

Rules:

- original file remains restricted to import-authorized users in team scope;
- preview rows and validation issues use the same restriction;
- no public links;
- no media-library exposure;
- no browser/client cache instructions that make source files public;
- no file/row contents in server logs;
- no preview/validation payloads in audit logs;
- no retention cleanup in Unit 43;
- source download is attachment-only;
- cancelled/failed jobs remain available for authorized operational review.

### Clean Architecture boundaries

Domain owns:

- `ImportJob`;
- status transitions;
- lease invariants;
- immutable metadata;
- confirmation/cancellation invariants;
- preview/validation issue entities where consistent with current domain conventions;
- stable enum/value constraints.

Application owns:

- upload orchestration;
- import authorization;
- list/detail/source/preview/issue queries;
- processor registry/contracts;
- lease acquisition/finalization;
- preview/validation/confirmation workflows;
- allowed-action calculation;
- target-context validation;
- audit payload construction;
- transaction and compensation orchestration.

Infrastructure owns:

- EF Core mappings;
- import query projections;
- stored-file repository integration;
- processor registry implementation/DI discovery;
- source stream access through Unit 40;
- migration.

API owns:

- endpoint mapping;
- streaming multipart HTTP parsing;
- request/response mapping;
- source-file response;
- CSRF/policy application;
- ProblemDetails mapping.

Do not place parser logic, mappings, EF queries, storage keys, workflow transitions, or authorization rules in endpoint handlers.

### EF Core persistence

Add the import model to `AppDbContext`.

Configure:

- required fields and bounds;
- enums consistent with repository conventions;
- unique `StoredFileId`;
- explicit foreign keys to team, optional match, stored file, creator, processing requester, confirmer, canceller;
- non-cascading user/team/match/file relationships;
- UTC timestamps;
- optimistic concurrency for workflow/lease finalization;
- `jsonb` for preview row values, validation metadata, and result summary;
- preview/issue cascade behavior only from impossible administrative hard deletion/migration cleanup, not product endpoints;
- indexes for list/status/team/match/history queries.

Practical indexes:

```txt
ImportJob(TeamId, Status, CreatedAtUtc)
ImportJob(MatchId, CreatedAtUtc)
ImportJob(ImportType, SourceSystem, CreatedAtUtc)
ImportJob(CreatedByUserId, CreatedAtUtc)
ImportJob(ProcessingStartedAtUtc) where Status = PARSING
ImportPreviewColumn(ImportJobId, Ordinal unique)
ImportPreviewRow(ImportJobId, SourceRowNumber)
ImportValidationIssue(ImportJobId, Severity, SourceRowNumber)
```

Use a suitable concurrency mechanism consistent with existing PostgreSQL/EF conventions.

Do not store source file bytes in PostgreSQL.

### Migration

Create the migration through the approved `dotnet ef` workflow.

Keep files under:

```txt
backend/src/Infrastructure/Persistence/Migrations/
```

Use:

```txt
--output-dir Persistence/Migrations
```

Do not handwrite migration files unless tooling is genuinely blocked. Document any exception in `context/progress-tracker.md`.

### Tests

Add focused domain/unit, application, storage-orchestration, and integration tests.

#### Domain/workflow tests

Cover:

- valid job construction;
- immutable team/match/type/source/format/file;
- source label rules;
- target-context rules;
- all valid transitions;
- invalid transitions;
- terminal states;
- lease acquisition;
- active lease conflict;
- stale lease replacement;
- old lease finalization rejection;
- stale cancellation;
- revision matching;
- processor-version matching;
- confirmation prerequisites;
- cancellation behavior;
- actor/time consistency.

#### Upload tests

Cover:

- admin upload;
- data operator with `canImportData`;
- data operator without permission;
- out-of-scope team;
- read-only role;
- match/team mismatch;
- import-type target requirements;
- valid CSV pairs;
- valid XLSX pair;
- invalid `.xls`;
- unsupported MIME;
- extension/MIME mismatch;
- zero-byte;
- declared oversize;
- streamed oversize;
- exact limit;
- metadata-first multipart;
- duplicate/missing/extra parts;
- storage failure;
- database/audit failure compensation;
- compensation failure observability;
- one unique stored file per job;
- no media item created;
- no automatic processing;
- no storage key/path leakage;
- CSRF enforcement.

#### Capabilities tests

Cover:

- authorized access;
- non-import user rejection;
- runtime size/preview/lease configuration;
- accepted file pairs;
- stable import/source values;
- empty processor registry in Unit 43;
- deterministic ordering;
- no provider/path/secret leakage.

#### List/detail/source tests

Cover:

- team scope;
- explicit unauthorized team filter;
- all supported filters;
- bounded search/pagination;
- deterministic ordering;
- allowed actions;
- terminal states;
- active/stale processing summary;
- source download for every retained status;
- attachment disposition;
- `nosniff`;
- missing physical source handling;
- no storage key/path in DTOs.

#### Processor/lease endpoint tests

With fake test processors, cover:

- no processor leaves state unchanged and returns conflict;
- preview capability/action;
- preview success returns to `UPLOADED`;
- bounded columns/rows;
- preview replacement atomicity;
- preview processor failure -> `FAILED`;
- validation success -> `READY_TO_CONFIRM`;
- validation errors -> `VALIDATION_FAILED`;
- warnings alone do not block readiness;
- technical validation failure -> `FAILED`;
- issue replacement atomicity;
- lease conflict;
- stale lease replacement;
- old operation cannot finalize;
- source stream disposal/ownership;
- no official data mutation during preview/validation.

#### Confirmation tests

With a fake confirmation handler, cover:

- explicit confirm only;
- stale revision rejection;
- processor-version mismatch;
- missing confirm capability;
- current target reauthorization;
- target lifecycle/workflow conflict;
- full official-data/job/audit transaction success;
- rollback on target mutation failure;
- rollback on import audit failure;
- target-specific audit metadata includes import job ID;
- semantic revalidation can return `VALIDATION_FAILED`;
- technical failure produces safe `FAILED`;
- duplicate confirmation prevention;
- no partial row commit;
- result summary bounded;
- `IMPORTED` terminal.

#### Cancellation tests

Cover:

- every allowed source status;
- active lease rejection;
- stale lease cancellation;
- imported/cancelled rejection;
- source file retained;
- preview/issues retained;
- audit creation;
- no restore/reopen endpoint.

#### Audit tests

Cover:

- `IMPORT_JOB` entity type;
- all action codes;
- safe payloads;
- no storage key/file rows/full issues;
- committed validation-failure audit;
- processing-failure audit;
- failed transaction has no audit;
- entity-history authorization;
- pagination/filtering.

#### Persistence/migration tests

Cover:

- unique stored-file ownership;
- non-cascading relations;
- `jsonb` projections;
- preview column order uniqueness;
- issue indexes/queries;
- concurrency behavior;
- migration application.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

Record:

- import model;
- status/transition semantics;
- accepted file types;
- configuration keys;
- permission policy;
- processor registry behavior;
- fact that Unit 43 registers no real processors;
- upload/source retention behavior;
- lease/revision design;
- audit action codes;
- migration status;
- verification results;
- intentionally deferred parser/vendor/target mutation work.

If implementation reveals that Unit 40 storage cannot support the required streamed upload/source retention or that existing audit transaction boundaries cannot support confirmation atomicity, update the relevant architecture/context before proceeding.

Do not silently add parser packages, mappings, or background workers.

## Implementation

### 1. Add import domain values and aggregate

Create:

- import types;
- source systems;
- file formats;
- statuses;
- processing operations;
- validation severities;
- allowed-action codes;
- `ImportJob`;
- preview columns/rows;
- validation issues;
- workflow/lease/revision invariants.

### 2. Add import configuration

Add typed options for:

- maximum upload size;
- preview row limit;
- processing lease timeout.

Validate against Unit 40 global storage maximum.

Update `backend/.env.example`.

### 3. Add processor contracts and registry

Implement provider-neutral Application contracts and a registry keyed by:

```txt
ImportType + SourceSystem + FileFormat
```

Register no real processor in Unit 43.

Expose truthful capabilities.

### 4. Add EF Core persistence and migration

Configure:

- import job;
- preview columns;
- preview rows;
- validation issues;
- relationships;
- `jsonb`;
- concurrency;
- indexes.

Generate and apply the migration.

### 5. Implement streamed import upload

Add metadata-first multipart parsing.

Integrate:

- permission/team scope;
- target context;
- CSV/XLSX allowlist;
- Unit 40 storage;
- `StoredFile`;
- `ImportJob`;
- audit;
- compensation.

Do not start parsing automatically.

### 6. Implement capabilities, list, detail, and source queries

Add:

- runtime capabilities;
- server-side list filters;
- detail with allowed actions;
- authorized source download.

Never expose storage keys or paths.

### 7. Implement processing lease orchestration

Add reusable Application services for:

- acquiring leases;
- detecting staleness;
- replacing stale leases;
- finalizing only the current lease;
- clearing lease metadata;
- safe failure outcomes.

Do not hold long database transactions around file processing.

### 8. Implement preview workflow

Add preview action/query using registered processors.

Persist bounded complete preview results atomically.

No processor in Unit 43 means the action remains unavailable.

### 9. Implement validation workflow

Add validation action and paginated issue query.

Persist complete outcomes and readiness revision/version metadata.

Do not mutate official data.

### 10. Implement confirmation workflow contract

Add explicit confirmation endpoint/orchestration and transaction requirements.

With no confirmation processors registered, direct confirmation remains unavailable.

Use fake processors in tests to prove atomic behavior.

### 11. Implement cancellation

Add explicit cancellation with terminal behavior, source retention, stale-lease handling, and audit.

Do not add delete/restore.

### 12. Extend audit foundation

Add:

- `IMPORT_JOB`;
- import action codes;
- safe payload builders;
- import entity-history endpoint;
- confirmation provenance/audit contract.

### 13. Add tests

Implement all required domain/application/integration coverage, including isolated Unit 40 storage roots and fake processors.

### 14. Update progress documentation

Update `context/progress-tracker.md` with actual implementation and verification state.

Do not mark Unit 43 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use existing packages and infrastructure:

- ASP.NET Core 8;
- built-in streaming multipart APIs;
- EF Core/Npgsql;
- FluentValidation;
- Unit 40 file storage;
- Unit 38 audit foundation;
- current-user/team-scope/permission services;
- match/team repositories;
- existing Result/ProblemDetails;
- existing test infrastructure.

Do not add:

- CSV reader packages;
- XLSX/OpenXML reader packages;
- spreadsheet formula engines;
- vendor SDKs;
- background job packages;
- message queues;
- generic object-mapping/import packages;
- MIME-sniffing packages;
- frontend dependencies.

## Verification checklist

- [ ] Persistent `ImportJob` exists.
- [ ] Import jobs own one unique `StoredFile`.
- [ ] Import source files are not represented as media items.
- [ ] Original source files are retained for uploaded, failed, validation-failed, ready, imported, and cancelled jobs.
- [ ] No normal import operation physically deletes or archives the source file.
- [ ] Stable import types are centralized.
- [ ] Stable source systems are centralized.
- [ ] Stable CSV/XLSX file formats are centralized.
- [ ] Gpexe/Zone14 source values do not imply unsupported mappings.
- [ ] Accepted CSV/XLSX extension/content-type pairs are enforced.
- [ ] `.xls`, generic octet-stream, and unsupported formats are rejected.
- [ ] No CSV/XLSX parser package is added.
- [ ] No vendor-specific field mapping is added.
- [ ] Import statuses match the architecture list.
- [ ] Status transitions occur only through workflow/domain methods.
- [ ] `IMPORTED` and `CANCELLED` are terminal.
- [ ] Preview does not make a job ready to confirm.
- [ ] Validation does not mutate official data.
- [ ] Confirmation is explicit and never automatic.
- [ ] A stale validation revision cannot be confirmed.
- [ ] Processor key/version changes require revalidation.
- [ ] Processing operations use server-generated leases.
- [ ] Long file processing does not hold one database transaction open.
- [ ] Active non-stale leases cannot be replaced or cancelled.
- [ ] Stale leases can be safely replaced or cancelled.
- [ ] An old operation cannot finalize after losing its lease.
- [ ] Import upload size, preview limit, and lease timeout are typed/validated configuration.
- [ ] Import upload maximum does not exceed Unit 40 global storage maximum.
- [ ] `GET /api/imports/capabilities` exists.
- [ ] Capabilities expose runtime limits, accepted formats, stable types/sources, and registered processor stages.
- [ ] Capabilities truthfully show no real processors in Unit 43.
- [ ] Capabilities expose no provider, path, key, or secret.
- [ ] Application processor contracts are provider/framework neutral.
- [ ] Processor selection uses import type, source system, and file format.
- [ ] Missing processor leaves job state unchanged and returns a safe conflict.
- [ ] Preview columns and bounded rows are persisted separately from official data.
- [ ] Preview row values use structured `jsonb`.
- [ ] Formulas/macros are not evaluated.
- [ ] Validation issues persist stable code, severity, safe message, optional row/column, and metadata.
- [ ] `ERROR` blocks readiness while warnings alone do not.
- [ ] Validation issue and preview result replacement is atomic.
- [ ] Result summary is bounded structured JSON rather than a duplicate imported dataset.
- [ ] Every job has a required immutable team.
- [ ] Match import types require a matching immutable match/team context.
- [ ] Roster and training GPS imports do not accept a match target in Unit 43.
- [ ] No generic polymorphic target type/ID is added.
- [ ] `ADMIN` has import access across teams.
- [ ] `DATA_OPERATOR` requires `canImportData` and team scope.
- [ ] Other roles have no import-job access.
- [ ] Explicit out-of-scope team requests return `403`.
- [ ] Inaccessible job IDs do not leak existence.
- [ ] `POST /api/imports` uses streamed metadata-first multipart handling.
- [ ] Upload requires exactly one metadata section and one file.
- [ ] Upload does not fully buffer or Base64 encode the source.
- [ ] Actual stored bytes are authoritative.
- [ ] Client never supplies a storage key.
- [ ] Upload creates `UPLOADED` only and does not start processing.
- [ ] Storage/database/audit compensation follows Unit 40 rules.
- [ ] No false success occurs after compensation failure.
- [ ] `GET /api/imports` supports team/match/type/source/format/status/search/date pagination filters.
- [ ] Import list filtering/pagination is database-backed and deterministic.
- [ ] Import DTOs never expose storage keys or local paths.
- [ ] Detail returns backend-owned allowed actions.
- [ ] `GET /api/imports/{id}/source` securely streams the original attachment.
- [ ] Source download is available for every retained status to authorized users.
- [ ] Source download uses attachment disposition and `nosniff`.
- [ ] `POST /api/imports/{id}/preview` exists.
- [ ] `GET /api/imports/{id}/preview` exists with bounded pagination.
- [ ] Preview success returns the job to `UPLOADED`.
- [ ] Preview failure cannot leave partial committed preview data.
- [ ] `POST /api/imports/{id}/validate` exists.
- [ ] `GET /api/imports/{id}/validation-issues` exists with filters/pagination.
- [ ] Validation success moves to `READY_TO_CONFIRM`.
- [ ] Blocking issues move to `VALIDATION_FAILED`.
- [ ] Technical processing failure moves safely to `FAILED`.
- [ ] `POST /api/imports/{id}/confirm` exists but no real confirmation processor is registered in Unit 43.
- [ ] Confirmation contract rechecks permission, target state, lease, revision, and processor version.
- [ ] Official data, provenance, job status, result summary, and audits commit atomically.
- [ ] No partial row commit is allowed by the generic foundation.
- [ ] Target-specific audit events can include `importJobId` provenance.
- [ ] `POST /api/imports/{id}/cancel` exists.
- [ ] Cancellation preserves source, preview, issues, and prior safe history.
- [ ] Cancellation cannot be restored/reopened.
- [ ] No HTTP delete/archive/restore import endpoint exists.
- [ ] `IMPORT_JOB` audit entity type exists.
- [ ] All required import audit action codes exist.
- [ ] Audit payloads exclude storage keys, paths, file bytes, preview rows, and full issue datasets.
- [ ] Persisted validation/processing failure outcomes can be audited safely.
- [ ] Failed authorization/database transactions create no audit entry.
- [ ] `GET /api/imports/{id}/audit` exists with import authorization/team scope.
- [ ] No global import audit endpoint is added.
- [ ] No background worker, message queue, or scheduler is added.
- [ ] No official roster/statistics/GPS data is imported by Unit 43.
- [ ] EF Core migration is generated under `backend/src/Infrastructure/Persistence/Migrations/`.
- [ ] `dotnet ef database update` succeeds when the local database environment is available.
- [ ] Domain/workflow tests pass.
- [ ] Upload/storage compensation tests pass with isolated storage roots.
- [ ] Capabilities/list/detail/source tests pass.
- [ ] Fake-processor preview/validation/confirmation tests pass.
- [ ] Lease/concurrency/stale-operation tests pass.
- [ ] Authorization/team-scope/permission tests pass.
- [ ] Audit tests pass.
- [ ] No frontend files are changed.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for affected backend projects/solution.
- [ ] `dotnet test` passes for relevant backend test projects.
- [ ] `context/progress-tracker.md` records actual Unit 43 model, configuration, processor availability, migration, storage, audit, and verification state.
