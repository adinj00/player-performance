# Unit 48: Training GPS Backend Foundation

## Goal

Build the vendor-neutral backend foundation for training sessions and player physical workload data while also providing the canonical metric model required by future match GPS imports. Add training-session metadata and lifecycle, date-eligible participants, a versioned physical-workload aggregate that can target either a training participant or an existing match appearance, a constrained canonical metric catalogue with explicit units and comparability context, read APIs, import-confirmation integration contracts, authorization, provenance, audit coverage, EF Core persistence, migration, and tests.

Do not implement Gpexe or Zone14 mappings, public/manual metric-entry endpoints, frontend UI, dashboard aggregates, background processing, or confirmation capability for any import combination whose exact processor remains unconfirmed.

## Design

### Required reading and implementation boundaries

Before implementation, read:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/code-standards.md`
5. `context/ai-workflow-rules.md`
6. `context/progress-tracker.md`
7. `context/feature-specs/00-build-plan.md`
8. `context/feature-specs/22-teams-selections-settings-backend.md`
9. `context/feature-specs/23-staff-users-team-scope-account-lifecycle-backend.md`
10. `context/feature-specs/27-players-backend-foundation.md`
11. `context/feature-specs/28-player-team-assignment-backend.md`
12. `context/feature-specs/30-matches-backend-foundation.md`
13. `context/feature-specs/31-match-lineup-appearance-backend.md`
14. `context/feature-specs/38-audit-backend-foundation.md`
15. `context/feature-specs/43-import-workflow-backend-foundation.md`
16. `context/feature-specs/45-generic-csv-xlsx-parsing-foundation.md`
17. `context/feature-specs/46-gpexe-sample-review-mapping-gate.md`
18. `context/feature-specs/48-training-gps-backend-foundation.md`

Use relevant project-local skills from `.agents/skills/` when applicable.

Skills/plugins may guide implementation workflow but must not override project context, architecture rules, code standards, or this spec.

This unit is backend-only.

Do not add or change frontend routes, pages, components, navigation, shadcn/ui components, browser charts, or frontend API wrappers.

### Required architecture synchronization

The current architecture still contains older illustrative entities such as:

```txt
GpsImport
GpsMetric
```

Unit 48 must update `context/architecture.md` so the canonical model reflects the actual implementation:

```txt
TrainingSession
TrainingSessionParticipant
PlayerPhysicalWorkload
PhysicalWorkloadRevision
PhysicalMetricValue
```

Clarify that:

- `ImportJob` owns import workflow and original-file retention;
- physical workload is canonical official data created only by approved use cases/import confirmation;
- one workload may belong to a training participant or a match appearance through explicit foreign keys;
- workload revisions preserve source/provenance history;
- vendor mappings remain outside the canonical model;
- metric codes do not prove that a vendor export supplies those values.

Do not retain `GpsImport` as a second import workflow beside Unit 43.

### Scope

This unit introduces:

- persistent training-session metadata;
- explicit training-session lifecycle;
- date-eligible training participants;
- participant link/unlink history;
- a canonical physical metric catalogue;
- stable canonical units and value kinds;
- explicit threshold and methodology context;
- one `PlayerPhysicalWorkload` per training participant or match appearance;
- immutable append-only workload revisions;
- metric values belonging to one revision;
- current-revision selection;
- safe comparability keys;
- training-session list/detail/participant/workload APIs;
- player physical-workload history query;
- match physical-workload query;
- canonical metric catalogue query;
- an Application-level writer for future exact import-confirmation processors;
- optional explicit `TrainingSessionId` on `ImportJob`;
- import confirmation integration requirements;
- role/team-scope authorization;
- semantic audit coverage;
- EF Core mapping, migration, and focused tests.

This unit does not introduce:

- Gpexe mappings;
- Zone14 mappings;
- guessed vendor headers;
- a Gpexe or Zone14 exact processor;
- a generic physical-data confirmation processor;
- confirmation capability for Unit 45 raw preview;
- public/manual metric-write endpoints;
- training technical-event analysis;
- drills, exercises, coaching plans, attendance reasons, RPE, wellness, or notes;
- injury or medical availability;
- dashboard read models;
- charts;
- frontend UI;
- training media links;
- training import UI changes;
- background jobs;
- automatic session creation from an import;
- automatic player-name matching;
- arbitrary client-defined metric codes;
- a vendor-specific wide metrics table;
- physical deletion of sessions, participants, workloads, or revisions.

### Canonical model principle

The official database must store canonical football-performance concepts rather than vendor column names.

Required separation:

```txt
Vendor source file
    -> exact approved processor
    -> canonical metric code/unit/context
    -> versioned official workload revision
```

Do not persist official workload fields named after:

- Gpexe;
- Zone14;
- export headers;
- device columns;
- spreadsheet positions.

Vendor-specific information belongs in:

- `ImportJob`;
- processor key/version;
- source system;
- safe provenance;
- mapping documentation.

### Training session model

Add a persistent aggregate equivalent to:

```txt
TrainingSession
```

Required fields:

```txt
Id
TeamId
SessionDate
StartsAtUtc
EndsAtUtc
Title
Location
Description
Status
CreatedByUserId
CreatedAtUtc
UpdatedAtUtc
CompletedByUserId
CompletedAtUtc
CancelledByUserId
CancelledAtUtc
```

Field behavior:

- `TeamId` is required and immutable;
- `SessionDate` is required and represents the club-operational calendar date;
- `StartsAtUtc` is optional when exact time is not known;
- `EndsAtUtc` is optional;
- when both times are present, end must be after start;
- `Title` is required, trimmed, and bounded;
- `Location` is optional, trimmed, and bounded;
- `Description` is optional, trimmed, and bounded;
- creator comes from authenticated current-user context;
- timestamps use the approved UTC clock;
- lifecycle actor/time pairs are both present or both absent;
- no hard-delete behavior exists.

Recommended bounds unless repository conventions define stricter values:

```txt
Title: 200
Location: 200
Description: 2,000
```

`SessionDate` is authoritative for:

- player assignment eligibility;
- date filtering;
- workload history grouping.

Do not infer assignment eligibility from the UTC date component of `StartsAtUtc`.

### Training session statuses

Use stable values:

```txt
PLANNED
COMPLETED
CANCELLED
```

Allowed transitions:

```txt
PLANNED -> COMPLETED
PLANNED -> CANCELLED
```

Rules:

- new sessions start `PLANNED`;
- `COMPLETED` and `CANCELLED` are terminal lifecycle statuses;
- status changes use explicit domain/Application methods;
- endpoint handlers and clients never assign status directly;
- no reopen, restore, or hard-delete action exists;
- cancellation preserves participants, import references, audit, and any already committed historical data;
- a cancelled session cannot receive new participants or workload revisions;
- a planned session cannot receive official physical-workload revisions;
- official workload revisions require `COMPLETED`.

Historical session backfill uses:

1. create a planned session with the historical `SessionDate`;
2. add/derive participants where required;
3. complete the session;
4. confirm approved workload data later.

Do not create a session directly as completed through the public API.

### Metadata editing

Allowed mutation roles:

- `ADMIN`;
- in-scope `DATA_OPERATOR`.

Rules:

- `TeamId` is never editable;
- `SessionDate`, `StartsAtUtc`, and `EndsAtUtc` are editable only while `PLANNED`;
- title, location, and description may be corrected while `PLANNED` or `COMPLETED`;
- `CANCELLED` sessions are read-only;
- changing `SessionDate` must revalidate all current participants against assignment coverage;
- reject the date change when any active participant would become ineligible;
- changing date/time does not rewrite audit/workload provenance;
- metadata updates with no semantic change create no audit event.

### Training participants

Add a persistent relation equivalent to:

```txt
TrainingSessionParticipant
```

Required fields:

```txt
Id
TrainingSessionId
PlayerId
AddedByUserId
AddedAtUtc
RemovedByUserId
RemovedAtUtc
```

Rules:

- an active participant has no removal actor/time;
- removal actor/time are both present or both absent;
- participant records are not hard-deleted through product APIs;
- only one active participant may exist for one session/player pair;
- a removed player may be added again, creating a new history record;
- active-link uniqueness is database-protected where practical;
- historical references survive later player/team lifecycle changes;
- participant identity is explicit and does not depend on a name string.

### Participant eligibility

A player may be added when:

- player exists;
- player is not archived;
- target session is not cancelled;
- player has a `PlayerTeamAssignment` to `TrainingSession.TeamId` covering `SessionDate`;
- no active participant link already exists.

Assignment date ranges are inclusive and use Unit 28 semantics.

Rules:

- historical, current, or future assignment records may satisfy eligibility relative to the session date;
- current-day player access rules do not replace historical session eligibility;
- do not use only the player's current assignment;
- do not auto-create an assignment;
- do not fuzzy-match player names;
- import confirmation must use stable matched `PlayerId` values supplied by an exact validated processor.

Existing participant history remains valid when:

- the player is later archived;
- the team is later inactive/archived;
- assignment dates are corrected later.

### Participant mutations

Add and remove are allowed for:

- `ADMIN`;
- in-scope `DATA_OPERATOR`.

Add:

- allowed for `PLANNED` and `COMPLETED`;
- blocked for `CANCELLED`.

Remove:

- allowed for `PLANNED` and `COMPLETED`;
- blocked when the participant owns a physical workload;
- blocked for `CANCELLED`;
- semantically records removal rather than deleting the row.

The ability to correct participants on a completed session is required for controlled historical data preparation.

Do not interpret participant presence as:

- full participation;
- minutes completed;
- starting group;
- drill participation;
- medical availability.

Those concepts are out of scope unless represented by approved physical metrics later.

### Shared physical-workload context

The canonical workload model must support:

- training workload for one `TrainingSessionParticipant`;
- match workload for one existing `PlayerMatchAppearance`.

Add a persistent aggregate equivalent to:

```txt
PlayerPhysicalWorkload
```

Required fields:

```txt
Id
TeamId
PlayerId
OccurredOn
TrainingSessionParticipantId
PlayerMatchAppearanceId
CurrentRevisionId
CreatedAtUtc
```

Context invariant:

```txt
Exactly one of TrainingSessionParticipantId or PlayerMatchAppearanceId is present.
```

Rules:

- context type is immutable;
- `TeamId`, `PlayerId`, and `OccurredOn` are immutable canonical snapshots validated against the selected context;
- one workload exists at most per training participant;
- one workload exists at most per match appearance;
- no generic `TargetType + TargetId` is used;
- no workload exists without an explicit player context;
- no player-name-only workload record exists;
- match workload uses the stable Unit 31 appearance ID;
- training workload uses the stable participant ID.

For training:

```txt
TeamId = session.TeamId
PlayerId = participant.PlayerId
OccurredOn = session.SessionDate
```

For match:

```txt
TeamId = match.TeamId
PlayerId = appearance.PlayerId
OccurredOn = match operational date
```

Use the same established match-date semantics as Unit 31 eligibility.

### Workload revisions

Add an append-only entity equivalent to:

```txt
PhysicalWorkloadRevision
```

Required fields:

```txt
Id
PlayerPhysicalWorkloadId
RevisionNumber
SourceKind
ImportJobId
SourceSystem
ProcessorKey
ProcessorVersion
RecordedByUserId
RecordedAtUtc
CreatedAtUtc
```

Stable source kinds:

```txt
IMPORT
MANUAL
```

Unit 48 production behavior uses only:

```txt
IMPORT
```

`MANUAL` is reserved for a future explicit manual-entry spec and must not be exposed through a public writer in Unit 48.

Rules:

- revisions are immutable after commit;
- revision numbers are positive and unique within one workload;
- revisions are ordered sequentially;
- parent `CurrentRevisionId` identifies the authoritative current snapshot;
- creating a new revision does not delete old revisions/values;
- `ImportJobId` is required for `IMPORT`;
- source system, processor key, and processor version are required for `IMPORT`;
- `RecordedByUserId` is the authenticated user who confirmed the import;
- client does not supply actor/provenance values;
- no public endpoint can edit or delete a revision;
- no revision is current until its transaction commits.

This model allows corrected re-imports to create a new official revision without losing prior provenance.

### Metric value model

Add an immutable entity equivalent to:

```txt
PhysicalMetricValue
```

Required fields:

```txt
Id
PhysicalWorkloadRevisionId
MetricCode
Value
UnitCode
ThresholdValue
ThresholdUnitCode
ThresholdDirection
ThresholdScope
MethodKey
MethodVersion
```

Rules:

- one metric code may occur at most once in one revision;
- `Value` is decimal/numeric using an approved precision;
- metric-specific validation decides whether integer semantics are required;
- values are finite and non-negative for the initial catalogue;
- `UnitCode` must match the canonical definition;
- threshold fields follow all-or-none consistency rules;
- method key/version follow definition-specific consistency rules;
- values are immutable after commit;
- no raw vendor header is stored as the canonical metric code;
- no JSON blob replaces queryable canonical value/unit fields.

Use a precision sufficient for physical metrics without binary floating-point errors, for example a PostgreSQL numeric mapping following existing project conventions.

### Canonical metric catalogue

Add a code-owned canonical catalogue exposed through an Application abstraction equivalent to:

```txt
IPhysicalMetricCatalog
```

Definitions are stable application contracts, not user-created database records.

Each definition contains at minimum:

```txt
Code
ValueKind
CanonicalUnit
AggregationKind
ComparabilityRequirement
RequiresThresholdContext
RequiresMethodContext
```

Stable value kinds:

```txt
DECIMAL
INTEGER
DURATION
```

Stable canonical units:

```txt
METERS
METERS_PER_SECOND
METERS_PER_SECOND_SQUARED
COUNT
SECONDS
ARBITRARY_UNITS
```

Stable aggregation hints:

```txt
SUM
MAX
LATEST
```

These hints support future read models; they do not authorize aggregation across incompatible contexts.

### Initial canonical metric definitions

Register the following vendor-neutral canonical destinations:

```txt
TOTAL_DISTANCE_METERS
HIGH_SPEED_RUNNING_DISTANCE_METERS
SPRINT_DISTANCE_METERS
SPRINT_COUNT
MAX_SPEED_METERS_PER_SECOND
ACCELERATION_COUNT
DECELERATION_COUNT
PLAYER_LOAD_ARBITRARY_UNITS
SESSION_DURATION_SECONDS
```

Definition rules:

#### `TOTAL_DISTANCE_METERS`

```txt
ValueKind = DECIMAL
CanonicalUnit = METERS
AggregationKind = SUM
Threshold context = not allowed
Method context = optional
```

#### `HIGH_SPEED_RUNNING_DISTANCE_METERS`

```txt
ValueKind = DECIMAL
CanonicalUnit = METERS
AggregationKind = SUM
Threshold context = required
Method context = optional
```

#### `SPRINT_DISTANCE_METERS`

```txt
ValueKind = DECIMAL
CanonicalUnit = METERS
AggregationKind = SUM
Threshold context = required
Method context = optional
```

#### `SPRINT_COUNT`

```txt
ValueKind = INTEGER
CanonicalUnit = COUNT
AggregationKind = SUM
Threshold context = required
Method context = optional
```

#### `MAX_SPEED_METERS_PER_SECOND`

```txt
ValueKind = DECIMAL
CanonicalUnit = METERS_PER_SECOND
AggregationKind = MAX
Threshold context = not allowed
Method context = optional
```

#### `ACCELERATION_COUNT`

```txt
ValueKind = INTEGER
CanonicalUnit = COUNT
AggregationKind = SUM
Threshold context = required
Method context = optional
```

#### `DECELERATION_COUNT`

```txt
ValueKind = INTEGER
CanonicalUnit = COUNT
AggregationKind = SUM
Threshold context = required
Method context = optional
```

#### `PLAYER_LOAD_ARBITRARY_UNITS`

```txt
ValueKind = DECIMAL
CanonicalUnit = ARBITRARY_UNITS
AggregationKind = SUM
Threshold context = not allowed
Method context = required
```

#### `SESSION_DURATION_SECONDS`

```txt
ValueKind = DURATION
CanonicalUnit = SECONDS
AggregationKind = LATEST
Threshold context = not allowed
Method context = optional
```

Important:

- registering a canonical destination does not claim Gpexe, Zone14, or any other source exports it;
- no source mapping exists until its evidence gate is satisfied;
- Unit 45 generic preview does not populate these metrics;
- unknown/unconfirmed vendor fields are not converted to these codes;
- adding another canonical code requires a reviewed spec/context change;
- metric labels for frontend remain centralized in Unit 49, not persisted in the database.

### Threshold context

Stable values:

```txt
ThresholdDirection
- ABOVE_OR_EQUAL
- BELOW_OR_EQUAL

ThresholdScope
- TEAM
- PLAYER
- SOURCE_DEFINED
```

Threshold context consists of:

```txt
ThresholdValue
ThresholdUnitCode
ThresholdDirection
ThresholdScope
```

Rules:

- all threshold fields are present or all are absent;
- threshold-required metric definitions reject missing context;
- metrics that disallow threshold context reject supplied threshold fields;
- threshold unit must be physically compatible with the metric definition:
  - speed thresholds use `METERS_PER_SECOND`;
  - acceleration/deceleration thresholds use `METERS_PER_SECOND_SQUARED`;
- threshold value is positive;
- `SOURCE_DEFINED` means the exact processor proved the source's threshold semantics but cannot classify it as team- or player-specific;
- raw threshold labels may remain in import/mapping evidence, not canonical values.

Do not compare threshold-based metrics across different threshold comparability keys without explicit grouping.

### Method context

Method context consists of:

```txt
MethodKey
MethodVersion
```

Rules:

- both are present or both absent;
- method-required definitions reject missing method context;
- method-disallowed definitions reject it when a future definition says so;
- values are bounded stable technical identifiers;
- they must not contain secrets, paths, or full vendor configuration;
- `PLAYER_LOAD_ARBITRARY_UNITS` requires method context because arbitrary-unit values from different methodologies are not automatically comparable.

Example conceptual method identifiers:

```txt
gpexe-player-load
vendor-neutral-derived-load
```

These examples do not authorize a mapping.

A real key/version is approved only by an exact processor spec.

### Comparability key

Every metric value read model must expose or allow derivation of a deterministic comparability key from:

```txt
MetricCode
UnitCode
ThresholdValue
ThresholdUnitCode
ThresholdDirection
ThresholdScope
MethodKey
MethodVersion
```

Rules:

- values with different comparability keys must not be summed/ranked together silently;
- future dashboard/read models must group or warn;
- no conversion between different threshold/method contexts occurs automatically;
- plain canonical-unit equality is insufficient for threshold- or method-dependent metrics.

Do not store a random comparability key.

It should be derived deterministically or persisted as a deterministic normalized value.

### Official workload writer

Add an Application-layer abstraction/use case equivalent to:

```txt
IPhysicalWorkloadWriter
```

It is not exposed directly as a public HTTP endpoint.

It accepts a fully validated canonical write command from an exact confirmation processor.

The command must contain:

```txt
context:
  trainingSessionParticipantId or playerMatchAppearanceId

expected current revision / duplicate policy

source:
  importJobId
  sourceSystem
  processorKey
  processorVersion

metric values:
  canonical code
  canonical value
  canonical unit
  threshold context
  method context
```

The writer must:

1. load and validate the explicit player context;
2. re-check team and player consistency;
3. require a completed training session or played match;
4. validate every metric against the catalogue;
5. reject duplicate metric codes;
6. reject empty metric sets;
7. enforce threshold/method requirements;
8. enforce duplicate/revision concurrency;
9. create the workload if absent;
10. append one immutable revision;
11. append all immutable metric values;
12. set `CurrentRevisionId`;
13. write a semantic audit event;
14. commit inside the surrounding import-confirmation transaction.

Do not let the writer:

- choose a player by name;
- parse vendor columns;
- infer units;
- infer thresholds;
- select a match/training target;
- authorize a public request independently of the import confirmation;
- perform a partial metric commit.

### Revision/duplicate policy

A future exact processor must explicitly choose one approved policy:

```txt
REJECT_IF_WORKLOAD_EXISTS
APPEND_REVISION_IF_CURRENT_MATCHES_EXPECTATION
```

Unit 48's writer supports both safe behaviors.

#### `REJECT_IF_WORKLOAD_EXISTS`

Use when the mapping spec does not define correction/re-import behavior.

#### `APPEND_REVISION_IF_CURRENT_MATCHES_EXPECTATION`

Requires an expected current revision ID.

Rules:

- `null` expectation means no existing workload/revision is expected;
- mismatched expectation returns `409`;
- no last-write-wins;
- no implicit merge of metric subsets;
- a new revision is a complete authoritative snapshot for the metrics supplied by that exact processor contract;
- processor spec must define whether omitted optional metrics are absent or preserved—Unit 48 defaults to complete replacement within that processor's approved metric set.

Do not use filename alone to decide duplication.

### Training and match write context

#### Training workload

Requirements:

- import type is `TRAINING_GPS`;
- `ImportJob.TrainingSessionId` is present;
- session is `COMPLETED`;
- import job team equals session team;
- participant exists or the exact confirmation command explicitly requests creation using a stable validated `PlayerId`;
- any created participant passes assignment eligibility for `SessionDate`;
- cancelled session is rejected.

#### Match workload

Requirements:

- import type is `MATCH_GPS`;
- `ImportJob.MatchId` is present;
- match is `PLAYED`;
- match is not archived;
- exact `PlayerMatchAppearanceId` exists for that match;
- import job team equals match team;
- report workflow lock behavior defined by the exact processor must be rechecked;
- when an existing match report is `READY_FOR_REVIEW`, `VERIFIED`, or `ARCHIVED`, physical workload confirmation is blocked;
- `DRAFT` and `NEEDS_CORRECTION` may allow confirmation when exact mapping rules otherwise pass.

Reuse Unit 32 workflow guard logic rather than duplicating raw status checks in endpoint handlers.

### Import job training-session relation

Extend `ImportJob` with:

```txt
TrainingSessionId
```

Rules:

- nullable for backward compatibility;
- explicit foreign key;
- non-cascading;
- valid only for `TRAINING_GPS`;
- `MatchId` and `TrainingSessionId` cannot both be present;
- when present, training session team must equal import job team;
- immutable after job creation in Unit 48;
- existing Unit 43 jobs remain valid with `null`;
- confirmation for `TRAINING_GPS` requires a non-null training session;
- source retention, preview, and cancellation remain available without a training-session target;
- Unit 49 will add contextual UI support for selecting/creating the target;
- no generic target type/ID pair is introduced.

Extend import upload/detail/list contracts safely with optional `trainingSessionId`/summary.

Do not make Unit 44 crash when the new optional field is absent or ignored.

### No processor activation

Unit 48 adds no exact import processor.

Required capability outcomes remain:

```txt
GPEXE:
  generic preview only

ZONE14:
  generic preview only

GENERIC/OTHER:
  generic preview only unless another approved exact processor exists
```

No new combination receives:

```txt
canValidate = true
canConfirm = true
```

merely because the canonical destination model now exists.

The source mapping, validation, duplicate policy, and exact processor still require evidence.

### Training-session APIs

Add:

```txt
GET   /api/training-sessions
GET   /api/training-sessions/{trainingSessionId}
POST  /api/training-sessions
PATCH /api/training-sessions/{trainingSessionId}
POST  /api/training-sessions/{trainingSessionId}/complete
POST  /api/training-sessions/{trainingSessionId}/cancel
```

Do not add HTTP delete.

### Training-session list

Support bounded server-side filters:

```txt
teamId
status
dateFrom
dateTo
search
page
pageSize
```

Rules:

- team scope always applies;
- explicit out-of-scope team filter returns `403`;
- search covers title and location;
- deterministic order:
  - `SessionDate` descending;
  - `StartsAtUtc` descending/null-safe;
  - `Id` descending;
- database pagination;
- no unbounded list;
- response includes participant count and current workload count without N+1 queries;
- no raw metric values in the compact list.

### Training-session detail

Return:

```txt
id
team summary
session date/time
title/location/description
status
creator/lifecycle summaries
participant summary/count
workload summary/count
allowedActions
```

Backend-owned actions:

```txt
VIEW
EDIT
ADD_PARTICIPANT
REMOVE_PARTICIPANT
COMPLETE
CANCEL
```

Rules:

- action calculation uses role, team scope, status, and workload/participant constraints;
- frontend role logic never replaces backend enforcement;
- unknown future actions are ignored safely.

### Participant APIs

Add:

```txt
GET    /api/training-sessions/{trainingSessionId}/participants
POST   /api/training-sessions/{trainingSessionId}/participants
DELETE /api/training-sessions/{trainingSessionId}/participants/{participantId}
```

Create request contains:

```txt
playerId
```

Rules:

- no player name input;
- full active participant list by default;
- optional `includeRemoved=true` may be `ADMIN` only if required for operational history;
- responses include safe player summaries and assignment evidence summary;
- no N+1 player/team lookups;
- `DELETE` records unlink metadata;
- workload-bearing participant cannot be removed.

### Workload read APIs

Add:

```txt
GET /api/physical-metrics/catalog
GET /api/training-sessions/{trainingSessionId}/workloads
GET /api/matches/{matchId}/physical-workloads
GET /api/players/{playerId}/physical-workloads
```

No workload mutation endpoint is added.

#### Metric catalogue

Returns stable definitions:

```txt
code
valueKind
canonicalUnit
aggregationKind
requiresThresholdContext
requiresMethodContext
```

Authenticated active staff may read it.

No team data or secrets are present.

#### Training workloads

Return current workload revisions for active participants, plus optional revision history metadata.

Support:

```txt
playerId
metricCode
page
pageSize
```

Default ordering:

- player display name;
- player ID.

#### Match workloads

Return current workload revisions for appearances in the match.

Follow match/team-scope visibility.

Do not return a workload for a player who has no appearance context.

#### Player workload history

Support:

```txt
teamId
contextType
dateFrom
dateTo
metricCode
page
pageSize
```

Stable context types:

```txt
TRAINING
MATCH
```

Rules:

- team scope applies;
- selected-scope users may read only records for teams currently in their scope;
- historical workload records remain stored even when current player assignment changes;
- current team-scope authorization—not historical assignment alone—controls access;
- responses include context summaries and current revision metrics;
- pagination/database filtering is required;
- no client-side full-history aggregation.

### Workload read shape

A workload response contains at minimum:

```txt
workloadId
context:
  type
  training session summary or match summary
team summary
player summary
occurredOn
currentRevision:
  id
  revisionNumber
  sourceKind
  importJobId
  sourceSystem
  processorKey
  processorVersion
  recordedBy summary
  recordedAtUtc
  metrics:
    code
    value
    unit
    threshold context
    method context
    comparabilityKey
revisionCount
```

Rules:

- previous revisions are not expanded by default;
- no source spreadsheet rows;
- no storage key/path;
- no vendor header names;
- decimal values remain exact JSON numbers/strings according to established precision handling;
- zero is preserved;
- absent metric is different from zero;
- API does not invent unavailable values.

An optional authorized endpoint for revision history may be added only if necessary for Unit 49; otherwise keep it deferred.

### Authorization

#### Read access

Authenticated active staff may read sessions/workloads only for authorized teams:

- `ADMIN`: all teams;
- `ALL_TEAMS`: all teams;
- `SELECTED_TEAMS`: selected teams only.

The same team-scope rules apply to:

- session list/detail;
- participants;
- training workload;
- match workload;
- player workload history;
- session audit.

#### Session/participant mutations

Allowed:

- `ADMIN`;
- in-scope `DATA_OPERATOR`.

Not allowed:

- `ANALYST`;
- `COACH`;
- `MEDICAL_STAFF`;
- `VIEWER`.

#### Workload mutations

No public mutation route exists.

Only an approved exact import confirmation use case may call the writer.

That import flow still requires:

- `ADMIN`; or
- in-scope `DATA_OPERATOR` with `canImportData = true`.

Do not create a separate broad `CanWriteGps` permission in Unit 48.

### Audit integration

Extend Unit 38 with entity types:

```txt
TRAINING_SESSION
PHYSICAL_WORKLOAD
```

Add centralized actions:

```txt
TRAINING_SESSION_CREATED
TRAINING_SESSION_UPDATED
TRAINING_SESSION_COMPLETED
TRAINING_SESSION_CANCELLED
TRAINING_SESSION_PARTICIPANT_ADDED
TRAINING_SESSION_PARTICIPANT_REMOVED
PHYSICAL_WORKLOAD_REVISION_CREATED
```

Audit successful committed mutations only.

#### Session audit

Safe payloads may include:

```txt
teamId
sessionDate
startsAtUtc
endsAtUtc
title
location
description
status
participant playerId
```

Do not include:

- medical notes;
- source files;
- raw metric arrays;
- storage paths;
- request bodies.

#### Workload audit

Use:

```txt
EntityType = PHYSICAL_WORKLOAD
EntityId = workload ID
Action = PHYSICAL_WORKLOAD_REVISION_CREATED
```

Safe payload/metadata may include:

```txt
teamId
playerId
contextType
trainingSessionId
matchId
playerMatchAppearanceId
revisionNumber
importJobId
sourceSystem
processorKey
processorVersion
metricCodes
metricCount
previousRevisionId
newRevisionId
```

Do not duplicate complete metric values into audit JSON by default because immutable revisions preserve authoritative values.

Do not include:

- source rows;
- raw vendor headers;
- threshold configuration beyond bounded comparability summaries when needed;
- storage key/path;
- full file data.

Business mutation and audit commit atomically.

### Training-session audit API

Add:

```txt
GET /api/training-sessions/{trainingSessionId}/audit
```

Use Unit 38 structured response.

Authorization matches session detail/team scope.

Support bounded:

```txt
page
pageSize
action
dateFrom
dateTo
```

This endpoint returns training-session lifecycle/participant audit events.

Physical workload provenance is available through workload revisions and the generic audit infrastructure; a dedicated global workload-audit page is out of scope.

### Provenance

Every official workload revision must retain:

```txt
ImportJobId
SourceSystem
ProcessorKey
ProcessorVersion
RecordedByUserId
RecordedAtUtc
```

Rules:

- import job must be `TRAINING_GPS` or `MATCH_GPS`;
- import job team/context must match the workload target;
- source file remains retained through Unit 43;
- provenance is immutable;
- a later revision does not rewrite prior provenance;
- no user-supplied provenance values;
- no workload revision without an approved source/use case.

### Transaction boundaries

Future import confirmation must atomically commit:

- target participant creation when approved;
- workload aggregate creation when absent;
- new workload revision;
- metric values;
- current revision pointer;
- import result summary;
- `ImportJob` status `IMPORTED`;
- import audit;
- physical-workload audit.

If any part fails:

- official data does not partially commit;
- import job does not become `IMPORTED`;
- prior current workload revision remains unchanged;
- no partial metric values remain.

Do not perform workload writes after the import transaction returns success.

### EF Core persistence

Add the model to `AppDbContext`.

Configure:

- required fields/bounds;
- enum/string persistence following repository conventions;
- explicit team/player/session/participant/match-appearance/import/user relationships;
- non-cascading historical relationships;
- active participant uniqueness;
- exactly-one workload-context database check;
- unique workload per training participant;
- unique workload per match appearance;
- sequential unique revision number per workload;
- unique metric code per revision;
- numeric precision;
- threshold/method all-or-none constraints where practical;
- current revision relationship without cascade cycles;
- optional `ImportJob.TrainingSessionId`;
- useful indexes for team/date/player/context/metric queries.

Practical indexes include:

```txt
TrainingSession(TeamId, SessionDate, Status)
TrainingSession(Status, SessionDate)
TrainingSessionParticipant(TrainingSessionId, RemovedAtUtc)
TrainingSessionParticipant(PlayerId, AddedAtUtc)
PlayerPhysicalWorkload(TeamId, OccurredOn)
PlayerPhysicalWorkload(PlayerId, OccurredOn)
PhysicalWorkloadRevision(PlayerPhysicalWorkloadId, RevisionNumber unique)
PhysicalWorkloadRevision(ImportJobId)
PhysicalMetricValue(PhysicalWorkloadRevisionId, MetricCode unique)
PhysicalMetricValue(MetricCode)
ImportJob(TrainingSessionId)
```

Use PostgreSQL partial unique indexes where appropriate for active participants.

Do not store all metric values as one opaque JSON document.

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

Do not handwrite migrations unless tooling is genuinely blocked.

Document any exception in `context/progress-tracker.md`.

### Error handling

Use existing Result and ProblemDetails conventions.

Expected behavior:

- `400` malformed query/request syntax;
- `401` unauthenticated;
- `403` explicit out-of-scope team or unauthorized mutation;
- `404` missing or safely inaccessible resource;
- `409` invalid lifecycle, participant conflict, ineligible player, workload-bearing participant removal, revision conflict, metric/context mismatch, or workflow lock;
- `422` semantic validation where consistent with existing API conventions;
- safe `5xx` for persistence/infrastructure failures.

Do not leak:

- hidden player/session existence;
- SQL/provider details;
- source file contents;
- vendor rows;
- storage paths/keys;
- audit internals;
- raw exception text.

### Tests

Add focused domain/unit, Application, persistence, authorization, audit, and integration tests.

#### Training-session tests

Cover:

- construction;
- required date/team/title;
- optional times;
- invalid time order;
- title/location/description bounds;
- planned default;
- valid complete/cancel transitions;
- invalid terminal transitions;
- date/time edit only while planned;
- descriptive correction on completed;
- cancelled read-only;
- no-op update;
- team immutability;
- actor/time consistency.

#### Participant tests

Cover:

- player assignment covering session date;
- historical/future/current assignment relative to session date;
- same-day inclusive boundaries;
- current-only logic is not used;
- archived player rejection;
- cancelled-session rejection;
- duplicate active participant;
- remove and re-add history;
- workload-bearing removal rejection;
- session date change blocked by participant eligibility;
- later assignment/player/team lifecycle changes preserve history.

#### Metric catalogue tests

Cover:

- every stable code;
- exact canonical unit;
- value kind;
- aggregation hint;
- threshold requirement;
- method requirement;
- uniqueness;
- deterministic ordering;
- catalogue response;
- no vendor-source claim.

#### Metric value tests

Cover:

- valid decimal/integer/duration values;
- non-negative values;
- integer semantics;
- unit mismatch;
- duplicate code;
- empty metric set;
- required threshold;
- disallowed threshold;
- threshold unit/direction/scope;
- required method;
- partial method fields;
- player-load comparability;
- zero preservation;
- decimal precision.

#### Workload context tests

Cover:

- exactly one context;
- training context derives team/player/date;
- match context derives team/player/date;
- duplicate workload per participant/appearance;
- completed training requirement;
- played match requirement;
- cancelled/archived/locked target rejection;
- appearance required for match workload;
- no name-only workload.

#### Revision tests

Cover:

- first revision;
- sequential append;
- immutable old revision;
- current pointer;
- reject-if-exists policy;
- expected-current concurrency;
- stale expectation;
- provenance requirements;
- import-job/source/context match;
- complete atomic snapshot;
- rollback preserves prior current revision.

#### Import integration tests

Use fake exact confirmation processors/commands to cover:

- `TRAINING_GPS` with explicit training session;
- missing training session blocks confirmation;
- `MATCH_GPS` with appearance;
- team/context mismatch;
- approved participant creation;
- ineligible participant;
- Unit 32 report lock;
- official workload/import/audit atomic transaction;
- target-specific result summary;
- no real Gpexe/Zone14 processor registration;
- capabilities remain preview-only for blocked vendors.

#### Read API tests

Cover:

- session filters/pagination/order;
- team scope;
- detail allowed actions;
- participant list;
- metric catalogue;
- training workloads;
- match workloads;
- player history context/date/metric filters;
- current revision only by default;
- no N+1 behavior where testable;
- no raw source/vendor/storage values.

#### Authorization tests

Cover:

- admin cross-team;
- all-teams read;
- selected-team read;
- explicit out-of-scope `403`;
- data-operator in-scope mutation;
- data-operator out-of-scope rejection;
- analyst/coach/medical/viewer read-only;
- import writer reachable only through approved confirmation orchestration.

#### Audit tests

Cover:

- entity/action codes;
- session lifecycle;
- participant add/remove;
- no-op/failed mutations create no event;
- workload revision audit contains safe codes/counts/provenance;
- complete metric arrays/source rows absent;
- atomic rollback;
- session audit authorization/pagination.

#### Migration tests

Cover:

- all constraints/indexes;
- active participant uniqueness;
- exactly-one context check;
- revision/metric uniqueness;
- non-cascading relations;
- optional import training-session relation;
- migration application.

### Documentation synchronization

Update `context/architecture.md` with the canonical workload model and remove the obsolete separate `GpsImport` concept.

Update `context/progress-tracker.md` after meaningful implementation changes.

Record:

- training-session lifecycle;
- participant eligibility;
- canonical metric codes/units;
- threshold/method comparability behavior;
- versioned workload model;
- import integration contract;
- fact that no exact vendor processor is activated;
- migration status;
- verification results;
- intentionally deferred UI/dashboard/vendor mapping/manual-entry features.

Update `context/feature-specs/00-build-plan.md` if implementation reveals a dependency change for Units 46, 47, 49, or 52.

Do not mark Gpexe/Zone14 mapping gates resolved merely because canonical destinations now exist.

## Implementation

### 1. Synchronize architecture documentation

Replace obsolete `GpsImport`/generic `GpsMetric` examples with the actual canonical training/workload model.

Keep vendor mappings evidence-gated.

### 2. Add training-session domain model

Implement:

- metadata;
- lifecycle;
- explicit transitions;
- mutation rules;
- immutable team;
- date/time validation.

### 3. Add participant model and eligibility

Implement:

- active-link history;
- assignment-date validation;
- duplicate prevention;
- removal constraints;
- participant queries.

### 4. Add the canonical metric catalogue

Implement:

- definitions;
- units;
- value kinds;
- aggregation hints;
- threshold/method requirements;
- catalogue query.

Keep it code-owned and vendor-neutral.

### 5. Add workload, revision, and metric entities

Implement:

- explicit training/match contexts;
- current revision pointer;
- append-only revisions;
- immutable values;
- comparability context;
- constraints.

### 6. Add the internal workload writer

Implement safe canonical revision creation for future exact confirmation processors.

Do not expose a public metric-write endpoint.

### 7. Extend import-job target context

Add optional `TrainingSessionId` and safe DTO/request support.

Require it only for future `TRAINING_GPS` confirmation, not for source retention/preview.

### 8. Add persistence and migration

Configure all entities, relationships, indexes, checks, precision, and concurrency.

Generate/apply the migration.

### 9. Add training-session and participant APIs

Implement list, detail, create, update, complete, cancel, participant add/remove, and backend-owned actions.

### 10. Add workload read APIs

Implement metric catalogue, training workloads, match workloads, and player workload history.

No mutation endpoint.

### 11. Extend audit foundation

Add session/workload entity types, actions, safe payloads, atomic integration, and session audit query.

### 12. Prove import-confirmation atomicity

Use fake exact processors/commands in tests to prove training and match workload confirmation behavior without registering production vendor processors.

### 13. Add comprehensive tests

Implement all required domain/Application/integration/migration/authorization/audit coverage.

### 14. Update progress documentation

Update `context/progress-tracker.md` with actual implementation and verification state.

Do not mark Unit 48 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use existing backend packages and infrastructure:

- ASP.NET Core 8;
- EF Core/Npgsql;
- FluentValidation;
- existing Result/ProblemDetails patterns;
- current user/team-scope services;
- Unit 28 player assignment history;
- Unit 31 stable match appearances;
- Unit 32 workflow guards;
- Unit 38 audit;
- Unit 43 import workflow;
- approved UTC clock/test infrastructure.

Do not add:

- GPS/vendor SDKs;
- geospatial packages;
- time-series databases;
- background job packages;
- charting packages;
- generic EAV frameworks;
- unit-conversion packages;
- frontend dependencies.

## Verification checklist

- [ ] `context/architecture.md` uses the actual canonical workload model.
- [ ] Obsolete separate `GpsImport` behavior is not implemented beside Unit 43.
- [ ] Persistent `TrainingSession` exists.
- [ ] Team and session date are required and immutable according to the rules.
- [ ] Optional start/end times validate correctly.
- [ ] Training statuses are `PLANNED`, `COMPLETED`, and `CANCELLED`.
- [ ] Only planned sessions can complete or cancel.
- [ ] Completed/cancelled sessions cannot reopen.
- [ ] No training-session hard-delete endpoint exists.
- [ ] Date/time edits are limited to planned sessions.
- [ ] Descriptive correction rules are enforced.
- [ ] Persistent participant history exists.
- [ ] Active participant uniqueness is enforced.
- [ ] Participant eligibility uses assignment coverage on `SessionDate`.
- [ ] Current-only assignment logic is not used.
- [ ] Removed participants are not hard-deleted.
- [ ] Workload-bearing participants cannot be removed.
- [ ] Session date changes cannot invalidate active participants.
- [ ] `PlayerPhysicalWorkload` has exactly one explicit context.
- [ ] Training context uses `TrainingSessionParticipant`.
- [ ] Match context uses stable `PlayerMatchAppearance`.
- [ ] No generic target type/ID or name-only workload exists.
- [ ] One workload exists at most per training participant or match appearance.
- [ ] Workload revisions are append-only and immutable.
- [ ] Parent current revision is updated atomically.
- [ ] Old revisions and provenance remain preserved.
- [ ] Metric values are immutable and queryable.
- [ ] Metric values are not stored only as opaque JSON.
- [ ] Canonical metric catalogue is code-owned and deterministic.
- [ ] All initial canonical codes are present with correct unit/value/aggregation metadata.
- [ ] Canonical code presence is not treated as proof of vendor export availability.
- [ ] Unit 45 raw preview does not populate official metrics.
- [ ] Unknown vendor fields are not mapped.
- [ ] Numeric zero is preserved and differs from absent metric.
- [ ] Metric units must match catalogue definitions.
- [ ] Integer/duration semantics are enforced.
- [ ] Duplicate metric codes in one revision are rejected.
- [ ] Empty revisions are rejected.
- [ ] Threshold-required metrics reject missing threshold context.
- [ ] Threshold-disallowed metrics reject threshold context.
- [ ] Threshold units/directions/scopes are validated.
- [ ] Player-load arbitrary units require method key/version.
- [ ] Comparability key includes metric, unit, threshold, and method context.
- [ ] Incompatible comparability keys are not silently aggregated.
- [ ] No public/manual physical metric mutation endpoint exists.
- [ ] The internal writer accepts only canonical validated commands.
- [ ] The writer does not parse files, match names, infer units, or choose targets.
- [ ] Revision duplicate/concurrency policies reject last-write-wins.
- [ ] Training workload requires a completed session.
- [ ] Match workload requires a played match and valid appearance.
- [ ] Match workload confirmation respects report workflow locks.
- [ ] `ImportJob.TrainingSessionId` is an explicit nullable foreign key.
- [ ] Match and training-session targets cannot both be present.
- [ ] Existing import jobs remain valid when training-session ID is null.
- [ ] Training source retention/preview remains possible without a target.
- [ ] TRAINING_GPS confirmation requires an explicit training session.
- [ ] No production Gpexe/Zone14 exact processor is registered.
- [ ] Blocked vendors remain generic-preview-only.
- [ ] Training-session list/detail/create/update/complete/cancel APIs exist.
- [ ] Participant list/add/remove APIs exist.
- [ ] Metric catalogue read API exists.
- [ ] Training workload read API exists.
- [ ] Match workload read API exists.
- [ ] Player workload-history API exists.
- [ ] List/filter/pagination remains database-backed.
- [ ] Read DTOs expose current revisions and safe provenance only.
- [ ] No storage key, source rows, vendor headers, or file content is exposed.
- [ ] Backend-owned training allowed actions are returned.
- [ ] Read access is team-scope aware.
- [ ] Admin and in-scope data operator can mutate sessions/participants.
- [ ] Analyst, coach, medical, and viewer remain read-only.
- [ ] Workload writes are reachable only through approved import confirmation.
- [ ] Training/workload audit entity types and actions exist.
- [ ] Failed/no-op mutations create no audit event.
- [ ] Workload audit excludes complete metric arrays and source rows.
- [ ] Session audit endpoint follows team scope.
- [ ] Official workload, import status/result, and audit changes commit atomically.
- [ ] Failure preserves prior current workload revision.
- [ ] EF Core constraints enforce explicit context, uniqueness, precision, and non-cascading history.
- [ ] Migration is generated under `backend/src/Infrastructure/Persistence/Migrations/`.
- [ ] `dotnet ef database update` succeeds when the local database environment is available.
- [ ] Training-session tests pass.
- [ ] Participant eligibility/history tests pass.
- [ ] Metric catalogue/context tests pass.
- [ ] Workload/revision/concurrency tests pass.
- [ ] Fake import-confirmation atomicity tests pass.
- [ ] Read API/team-scope tests pass.
- [ ] Audit tests pass.
- [ ] Migration/constraint tests pass.
- [ ] No frontend files are changed.
- [ ] No vendor SDK, geospatial/time-series, background-job, EAV, unit-conversion, or chart dependency is added.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for affected backend projects/solution.
- [ ] `dotnet test` passes for relevant backend test projects.
- [ ] `context/progress-tracker.md` records actual Unit 48 model, migration, vendor-gate state, and verification outcome.
