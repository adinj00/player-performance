# Unit 50: Medical Availability Backend

## Goal

Build the privacy-separated backend foundation for player availability and injury records.

Add coach-safe current availability summaries and immutable availability revisions, restricted versioned injury records, explicit medical permissions, team-scoped list/detail/history APIs, current-team availability counts, optimistic concurrency, lifecycle rules, and semantic audit coverage.

Ensure that coaches and other ordinary team-scoped readers can access only operational availability information, while diagnoses, body-area details, restricted notes, and detailed injury history are returned only through separate medical-detail contracts and authorization checks.

Do not add frontend UI, medical documents, treatment plans, medications, test results, wellness/RPE, automatic availability changes, public/manual deletion, medical file uploads, or dashboard endpoints.

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
8. `context/feature-specs/20-backend-staff-roles-authorization-foundation.md`
9. `context/feature-specs/22-teams-selections-settings-backend.md`
10. `context/feature-specs/23-staff-users-team-scope-account-lifecycle-backend.md`
11. `context/feature-specs/27-players-backend-foundation.md`
12. `context/feature-specs/28-player-team-assignment-backend.md`
13. `context/feature-specs/38-audit-backend-foundation.md`
14. `context/feature-specs/50-medical-availability-backend.md`

Use relevant installed backend Codex skills/plugins when applicable.

Skills/plugins may guide implementation workflow but must not override the project context, architecture rules, code standards, privacy rules, or this spec.

This unit is backend-only.

Do not add or change:

- frontend routes;
- frontend components;
- navigation;
- shadcn/ui files;
- dashboard read models;
- medical file storage;
- player assignment semantics;
- existing staff role values;
- existing team-scope behavior;
- session/authentication behavior.

### Required architecture synchronization

Update `context/architecture.md` so the medical module records the implemented disclosure boundary:

```txt
Coach-safe availability
Restricted medical details
```

Document that:

- availability status, expected return date, and a deliberately coach-visible note are operational fields;
- body area, diagnosis, restricted notes, and injury revision history are medical-detail fields;
- generic player, dashboard, team, match, training, and availability DTOs must never embed restricted injury fields;
- `canViewMedicalDetails` grants restricted read only after team-scope and feature checks;
- injury mutation remains limited to administrators and authorized medical staff;
- availability and injury changes are versioned and audited;
- absence of an availability aggregate is presented as `UNKNOWN`;
- injury resolution does not automatically change availability.

Do not create a second role or duplicate medical-detail permission.

### Scope

This unit introduces:

- the stable availability statuses already defined in architecture;
- one team-scoped availability aggregate per player/team;
- immutable append-only availability revisions;
- synthesized `UNKNOWN` status when no record exists;
- a current-team availability list;
- current availability status counts;
- player availability history;
- safe availability mutation;
- optimistic revision concurrency;
- coach-safe operational notes;
- versioned restricted injury records;
- injury lifecycle;
- restricted injury list/detail/revision APIs;
- safe permission and team-scope enforcement;
- separate read contracts for safe and restricted information;
- medical-specific audit entity/action codes;
- safe audit payload rules;
- availability and injury entity-history APIs;
- EF Core mapping;
- migration;
- focused domain, authorization, privacy, audit, and integration tests.

This unit does not introduce:

- medical UI;
- medical document upload;
- images or scans;
- prescriptions;
- medication fields;
- treatment plans;
- surgery records;
- laboratory/test results;
- medical appointments;
- medical provider contact data;
- insurance data;
- wellness questionnaires;
- sleep or RPE;
- medical clearance signatures;
- automated notifications;
- availability automation from injury lifecycle;
- physical workload medical interpretation;
- injury prediction;
- return-to-play protocols;
- public medical APIs;
- player login;
- hard deletion;
- archive/restore;
- generic medical search across note contents;
- a global medical audit browser;
- dashboard aggregation endpoints.

### Privacy tiers

The backend must treat medical data as two distinct disclosure tiers.

#### Coach-safe operational availability

May be returned to ordinary authenticated active staff for teams in their scope.

Allowed fields:

```txt
player summary
team summary
availability status
effective date
expected return date
coach-visible note
availability revision number
last recorded by summary
last recorded time
allowed actions
```

This tier must not contain:

```txt
injury ID
injury existence flag
body area
diagnosis
restricted medical notes
injury revision count
injury creator
injury timeline
medical-detail permission state
```

Do not include restricted fields with `null` values.

Use a dedicated safe DTO that does not define those properties.

#### Restricted medical details

May be returned only after:

- authentication;
- active account checks;
- team-scope authorization;
- `CanViewMedicalDetails` feature authorization.

Restricted fields include:

```txt
body area
diagnosis
restricted notes
injury revisions
injury lifecycle actor/time details
```

Use separate routes, queries, DTOs, projections, and validators.

Do not deserialize or project restricted values into ordinary availability responses and then remove them later.

### Role and permission behavior

Backend authorization is authoritative.

#### Safe availability read

Allowed for authenticated active staff within team scope:

- `ADMIN`;
- `DATA_OPERATOR`;
- `ANALYST`;
- `COACH`;
- `MEDICAL_STAFF`;
- `VIEWER`.

This enables coach-safe and dashboard-safe operational summaries.

#### Safe availability mutation

Allowed for:

- `ADMIN`;
- in-scope `MEDICAL_STAFF`.

`canViewMedicalDetails` is not required to record coach-safe availability.

A medical staff user without the detail flag may update only:

```txt
status
effectiveOn
expectedReturnOn
coachVisibleNote
```

They cannot access or mutate injury records.

#### Restricted medical-detail read

Allowed for:

- `ADMIN`;
- any non-admin active staff member with:
  - `canViewMedicalDetails = true`;
  - target team inside current authorized scope.

The explicit permission permits specially authorized coaches/analysts to read details when club policy grants it.

It does not permit mutation.

#### Injury mutation

Allowed for:

- `ADMIN`;
- in-scope `MEDICAL_STAFF` with `canViewMedicalDetails = true`.

Other roles cannot create, revise, or resolve injury records even when they have read permission.

#### Medical audit read

- availability audit follows coach-safe availability read;
- injury audit follows restricted medical-detail read.

Do not use frontend action visibility as authorization.

### Stable availability statuses

Use the architecture-defined values:

```txt
AVAILABLE
LIMITED
UNAVAILABLE
REHAB
UNKNOWN
```

Persist/API values remain English.

Meaning:

#### `AVAILABLE`

The player is operationally available without a recorded limitation.

#### `LIMITED`

The player is available only with restrictions or reduced participation.

#### `UNAVAILABLE`

The player is not operationally available.

#### `REHAB`

The player is in a rehabilitation phase.

#### `UNKNOWN`

The club has no current confirmed operational availability state, or medical staff explicitly reset the state to unknown.

Do not add severity levels, percentages, or vendor statuses.

### Availability aggregate

Add an aggregate equivalent to:

```txt
PlayerAvailability
```

Required fields:

```txt
Id
PlayerId
TeamId
CurrentRevisionId
CreatedAtUtc
```

Rules:

- one aggregate exists at most for one player/team pair;
- `PlayerId` is immutable;
- `TeamId` is immutable;
- `CurrentRevisionId` points to the authoritative current revision;
- the aggregate is not hard-deleted;
- player/team relationships are non-cascading;
- absence of an aggregate is a valid state and reads as synthesized `UNKNOWN`;
- creating an explicit `UNKNOWN` revision is also allowed;
- synthetic unknown does not create database or audit records.

### Availability revisions

Add an immutable entity equivalent to:

```txt
PlayerAvailabilityRevision
```

Required fields:

```txt
Id
PlayerAvailabilityId
RevisionNumber
Status
EffectiveOn
ExpectedReturnOn
CoachVisibleNote
RecordedByUserId
RecordedAtUtc
CreatedAtUtc
```

Rules:

- revisions are append-only;
- revision number is positive and unique per aggregate;
- current revision changes only inside the transaction that commits the new revision;
- previous revisions are never overwritten;
- no public endpoint edits or deletes an existing revision;
- one mutation creates one complete current snapshot;
- exact no-op mutations create no revision and no audit event;
- optimistic concurrency uses the caller's expected current revision ID;
- no last-write-wins behavior.

### Effective date

`EffectiveOn` is the operational date on which the recorded status applies.

Rules:

- required;
- uses the project/application calendar date convention;
- cannot be in the future;
- cannot precede the current revision's `EffectiveOn`;
- multiple revisions on the same date are allowed;
- `RecordedAtUtc` records when the application change occurred;
- current state is chosen by `CurrentRevisionId`, not by sorting dates client-side;
- the backend clock, not the client, defines “today” for future-date validation.

This is a current-state revision model, not a complete retrospective interval editor.

Do not add backdated range reconstruction in Unit 50.

### Expected return date

`ExpectedReturnOn` is coach-safe operational information.

Rules:

- optional;
- may be present only for:
  - `LIMITED`;
  - `UNAVAILABLE`;
  - `REHAB`;
- must be on or after `EffectiveOn`;
- must be `null` for:
  - `AVAILABLE`;
  - `UNKNOWN`;
- is an estimate, not a medical guarantee;
- does not automatically close or resolve an injury;
- changing it requires a new availability revision.

Do not derive it automatically from restricted notes.

### Coach-visible note

`CoachVisibleNote` is an optional bounded operational note.

Recommended maximum:

```txt
300 characters
```

Rules:

- trimmed;
- blank normalizes to `null`;
- explicitly coach-visible;
- may be returned to all ordinary team-scoped readers;
- must not be auto-populated from body area, diagnosis, or restricted notes;
- must not contain medication, test results, diagnosis, or other sensitive medical content;
- backend field separation and UI labeling provide the privacy boundary;
- the server cannot reliably classify medical meaning from arbitrary language, so implementation documentation and Unit 51 UI must warn the author clearly;
- note contents are not duplicated into audit JSON;
- audit records only whether the coach-visible note changed.

Do not name this field simply `Notes`.

### Availability eligibility and team ownership

A safe availability revision may be recorded only when:

- player exists;
- player is not archived;
- team exists and is active according to existing team rules;
- player has an assignment to that team covering the backend's current operational date;
- caller has mutation access for the team.

Rules:

- use Unit 28 inclusive assignment-date logic;
- do not use only a denormalized current-team field;
- do not create an assignment automatically;
- do not accept player/team IDs from untrusted nested objects;
- historical revisions remain valid if the player later changes teams or is archived;
- a new team requires its own availability aggregate;
- current availability is team-scoped and not automatically copied between teams.

This avoids leaking an old team's medical state into a new team without an explicit authorized record.

### Availability mutation request

Add an endpoint equivalent to:

```txt
POST /api/players/{playerId}/availability
```

Request:

```txt
teamId
status
effectiveOn
expectedReturnOn
coachVisibleNote
expectedCurrentRevisionId
```

Rules:

- route player ID is authoritative;
- status is parsed from stable enum values;
- `expectedCurrentRevisionId` is:
  - `null` when the caller expects no aggregate/current revision;
  - required to match the current revision when one exists;
- mismatch returns `409`;
- aggregate and first revision are created atomically when absent;
- subsequent calls append a new revision atomically;
- current pointer and audit update in the same transaction;
- no injury ID or restricted field is accepted;
- mutation requires CSRF protection under existing cookie-auth rules.

Do not expose a PATCH that edits the current row in place.

### Safe current-team availability list

Add:

```txt
GET /api/player-availability
```

Required query parameter:

```txt
teamId
```

Optional:

```txt
status
search
page
pageSize
```

The list population is:

```txt
all non-archived players with an assignment to the selected team covering the backend current operational date
```

For every player:

- return the current availability revision for that player/team when present;
- otherwise return synthesized `UNKNOWN`;
- do not create missing rows;
- do not expose injury existence/details.

Rules:

- explicit out-of-scope team returns `403`;
- server-side search/pagination;
- status filtering includes synthesized unknown rows;
- deterministic ordering:
  - player display name;
  - player ID;
- no N+1 queries;
- no current-team-only shortcut that ignores assignment dates;
- response includes safe `allowedActions`, for example:
  - `VIEW`;
  - `UPDATE`.

Do not return players merely because they have historical availability records for the team.

### Safe availability summary counts

Add:

```txt
GET /api/player-availability/summary
```

Required:

```txt
teamId
```

Return:

```txt
totalPlayers
availableCount
limitedCount
unavailableCount
rehabCount
unknownCount
generatedAtUtc
```

Rules:

- uses the same current assignment population and synthetic unknown semantics as the list;
- counts are mutually exclusive;
- total equals the status-count sum;
- team scope applies;
- no restricted medical fields or counts;
- no injury count;
- no diagnosis/body-area aggregation;
- database query is bounded and efficient;
- endpoint is suitable for Unit 51 summary cards and Unit 52 dashboard composition.

Do not add dashboard-specific labels or chart structures.

### Player availability history

Add:

```txt
GET /api/players/{playerId}/availability
```

Optional query:

```txt
teamId
page
pageSize
```

Return safe availability aggregates/revisions only for teams in the caller's current scope.

Rules:

- `teamId` must be in scope when supplied;
- without `teamId`, include only scoped team records;
- archived players remain readable through existing player visibility rules;
- deterministic newest-first revision ordering;
- each entry contains safe operational fields only;
- no injury linkage/details;
- no global player medical timeline;
- pagination is database-backed.

This endpoint supports player profile availability history without exposing injury records.

### Injury record aggregate

Add a restricted aggregate equivalent to:

```txt
InjuryRecord
```

Required fields:

```txt
Id
PlayerId
TeamId
OccurredOn
Status
CurrentRevisionId
CreatedByUserId
CreatedAtUtc
ResolvedOn
ResolvedByUserId
ResolvedAtUtc
```

Rules:

- player/team/occurred date are immutable;
- one player may have multiple open injury records;
- no natural uniqueness constraint is guessed;
- current revision points to the latest restricted detail snapshot;
- record is not hard-deleted or archived;
- relationships are non-cascading;
- resolution actor/date/time are all present or all absent;
- a resolved record cannot be reopened;
- resolving an injury does not automatically record `AVAILABLE`;
- an availability change does not automatically resolve an injury.

### Injury statuses

Use:

```txt
OPEN
RESOLVED
```

Allowed transition:

```txt
OPEN -> RESOLVED
```

Rules:

- new records start `OPEN`;
- `RESOLVED` is terminal;
- status changes through an explicit resolve use case;
- no direct status field in create/update requests;
- no reopen;
- no delete.

### Injury revisions

Add an immutable entity equivalent to:

```txt
InjuryRecordRevision
```

Required fields:

```txt
Id
InjuryRecordId
RevisionNumber
BodyArea
Diagnosis
RestrictedNotes
RecordedByUserId
RecordedAtUtc
CreatedAtUtc
```

Rules:

- revisions are append-only;
- values are restricted medical details;
- revision number is positive and unique per injury;
- current pointer changes atomically with the new revision;
- exact no-op update creates no revision/audit;
- update uses expected current revision concurrency;
- revisions may be appended to open or resolved records for factual correction;
- revision history is readable only with medical-detail permission;
- no full revision contents are copied into audit logs.

Recommended bounds:

```txt
BodyArea: 200
Diagnosis: 500
RestrictedNotes: 4,000
```

`BodyArea` and `Diagnosis` may be optional if club information is not yet confirmed.

`RestrictedNotes` is optional.

At least one of the three detail fields must be present on creation.

Do not add an uncontrolled JSON medical-data field.

### Injury occurrence and resolution dates

`OccurredOn`:

- required;
- cannot be in the future;
- immutable after creation.

`ResolvedOn`:

- required when resolving;
- cannot precede `OccurredOn`;
- cannot be in the future;
- immutable after resolution.

Use the backend clock for date validation.

Do not infer resolution date from availability changes.

### Injury create

Add:

```txt
POST /api/medical/injuries
```

Request:

```txt
playerId
teamId
occurredOn
bodyArea
diagnosis
restrictedNotes
```

Rules:

- caller must have injury mutation access;
- player must exist and not be archived;
- team must be active;
- player must have an assignment to the team covering `OccurredOn`;
- new record starts open;
- record and first revision commit atomically;
- audit commits atomically;
- no availability revision is created automatically;
- mutation requires CSRF protection.

Do not accept creator, status, resolution, or revision number from the client.

### Injury update

Add:

```txt
PATCH /api/medical/injuries/{injuryRecordId}
```

Request:

```txt
bodyArea
diagnosis
restrictedNotes
expectedCurrentRevisionId
```

Rules:

- full replacement of the restricted detail snapshot;
- omitted fields are not guessed as unchanged unless the established PATCH convention explicitly supports optional-field semantics;
- use a clear request contract;
- expected revision mismatch returns `409`;
- exact no-op returns the current representation without creating a revision/audit or returns the established no-op result;
- update and audit commit atomically;
- no status/date/player/team mutation.

### Injury resolve

Add:

```txt
POST /api/medical/injuries/{injuryRecordId}/resolve
```

Request:

```txt
resolvedOn
expectedCurrentRevisionId
```

Rules:

- caller must have injury mutation access;
- record must be open;
- expected current revision must match;
- no detail revision is required solely to resolve;
- status and resolution metadata update atomically;
- audit commits atomically;
- repeated resolve returns lifecycle conflict;
- no automatic availability update.

### Restricted injury list

Add:

```txt
GET /api/medical/injuries
```

Optional filters:

```txt
teamId
playerId
status
occurredFrom
occurredTo
page
pageSize
```

Rules:

- medical-detail permission required;
- current team scope always applies;
- explicit unauthorized team returns `403`;
- player filter does not bypass scope;
- no free-text search over diagnosis or restricted notes;
- deterministic ordering:
  - open before resolved when no status filter;
  - occurred date descending;
  - ID descending;
- database pagination;
- list DTO may include current body area/diagnosis only for authorized callers;
- restricted notes should be omitted from compact list or returned only as a boolean/short safe indicator;
- no note snippets;
- no N+1 projections.

### Restricted injury detail

Add:

```txt
GET /api/medical/injuries/{injuryRecordId}
```

Return:

```txt
record identity/status
player/team summary
occurred/resolved metadata
current restricted revision
revision count
creator/resolver summaries
allowed actions
```

Allowed actions may include:

```txt
VIEW
UPDATE
RESOLVE
```

Rules:

- medical-detail permission and team scope required;
- `UPDATE` and `RESOLVE` only for authorized medical mutation roles;
- resolved records do not expose `RESOLVE`;
- current revision values are returned only through this restricted DTO;
- do not include availability safe note automatically;
- do not infer the player's current availability.

### Restricted injury revision history

Add:

```txt
GET /api/medical/injuries/{injuryRecordId}/revisions
```

Support:

```txt
page
pageSize
```

Rules:

- medical-detail permission required;
- deterministic newest-first ordering;
- return complete restricted revision snapshots to authorized users;
- preserve actor/time and revision number;
- no diff reconstruction is required;
- no hard-delete;
- no source-file or unrelated player data;
- pagination is database-backed.

This immutable history is the authoritative detailed change history.

### Separation from availability

Availability and injury are related operationally but separate aggregates.

Rules:

- no required foreign key from availability revision to injury;
- no injury ID in coach-safe responses;
- no automatic availability status change after injury create/update/resolve;
- no automatic injury create after availability becomes unavailable;
- Unit 51 may show both sections only for authorized detail viewers;
- workflows remain explicit and auditable.

This supports:

- non-injury unavailability;
- multiple open injuries;
- coach-safe status without diagnosis disclosure;
- resolved injury records without rewriting operational availability.

### Safe DTO construction

Create dedicated query projections for:

```txt
SafeAvailabilityListItem
SafeAvailabilityRevision
SafeAvailabilitySummary
RestrictedInjuryListItem
RestrictedInjuryDetail
RestrictedInjuryRevision
```

Rules:

- do not reuse an EF entity as an API DTO;
- do not serialize navigation properties;
- do not use a single DTO with conditional sensitive fields;
- ordinary availability queries must not select diagnosis or restricted notes;
- restricted DTOs are never returned from safe endpoints;
- Mapster mappings must preserve the separation;
- tests must inspect serialized JSON property names, not only C# object values.

### Caching and HTTP behavior

Medical and availability responses are private authenticated data.

Use conservative cache behavior consistent with existing API conventions.

At minimum:

- do not mark responses public;
- do not produce permanent public URLs;
- source pages/browser caching must not be relied on as an access-control layer;
- authorization is checked on every request.

If the project has an existing private/no-store policy for sensitive responses, apply it to restricted injury endpoints.

Do not expose medical data through static files.

### Concurrency

Use optimistic concurrency for availability and injury revisions.

Requests carry:

```txt
expectedCurrentRevisionId
```

Rules:

- first availability revision expects `null`;
- existing availability/injury update expects the exact current revision ID;
- resolving an injury also checks the current revision;
- mismatch returns `409`;
- response explains that the record changed and must be refreshed;
- no last-write-wins;
- current revision pointer is database-protected;
- concurrent first availability creation is protected by the unique player/team constraint;
- database conflicts are translated to safe domain conflicts.

Do not use timestamps supplied by the client as concurrency tokens.

### Audit integration

Extend Unit 38 with entity types:

```txt
PLAYER_AVAILABILITY
INJURY_RECORD
```

Add stable actions:

```txt
PLAYER_AVAILABILITY_RECORDED
INJURY_RECORD_CREATED
INJURY_RECORD_UPDATED
INJURY_RECORD_RESOLVED
```

Audit successful committed mutations only.

#### Availability audit payload

May include:

```txt
playerId
teamId
previousStatus
newStatus
previousEffectiveOn
newEffectiveOn
previousExpectedReturnOn
newExpectedReturnOn
coachVisibleNoteChanged
previousRevisionId
newRevisionId
```

Do not include:

- coach-visible note contents;
- injury data;
- diagnosis;
- restricted notes;
- inferred reasons.

Availability audit is safe for ordinary team-scoped availability readers.

#### Injury audit payload

May include:

```txt
playerId
teamId
occurredOn
status
resolvedOn
previousRevisionId
newRevisionId
changedFieldKeys
```

Allowed changed keys:

```txt
bodyArea
diagnosis
restrictedNotes
```

Do not include the previous/new restricted values.

The immutable injury revision history remains the authoritative detailed record.

Injury audit is restricted to medical-detail readers.

#### Audit transaction behavior

- mutation and audit commit atomically;
- failed/no-op/unauthorized mutations create no event;
- no audit entry for reads;
- no audit entry for synthetic unknown availability;
- no fire-and-forget audit.

### Entity-history audit APIs

Add:

```txt
GET /api/player-availability/{availabilityId}/audit
GET /api/medical/injuries/{injuryRecordId}/audit
```

Use Unit 38 structured history response.

Support:

```txt
page
pageSize
action
dateFrom
dateTo
```

Authorization:

- availability audit:
  - safe availability read plus team scope;
- injury audit:
  - restricted medical-detail read plus team scope.

Rules:

- bounded pagination;
- newest-first deterministic order;
- no global medical audit route;
- no mutation endpoint;
- no sensitive restricted values in audit DTOs;
- safe inaccessible behavior.

### API surface

Required routes:

```txt
GET  /api/player-availability
GET  /api/player-availability/summary
GET  /api/player-availability/{availabilityId}/audit

GET  /api/players/{playerId}/availability
POST /api/players/{playerId}/availability

GET   /api/medical/injuries
GET   /api/medical/injuries/{injuryRecordId}
GET   /api/medical/injuries/{injuryRecordId}/revisions
GET   /api/medical/injuries/{injuryRecordId}/audit
POST  /api/medical/injuries
PATCH /api/medical/injuries/{injuryRecordId}
POST  /api/medical/injuries/{injuryRecordId}/resolve
```

Do not add:

- generic `/api/medical` dump endpoint;
- hard delete;
- archive/restore;
- reopen;
- bulk mutation;
- public medical endpoint;
- note-content search;
- manual audit mutation;
- availability status patch-in-place.

### Backend-owned actions

Safe availability list/history responses may expose:

```txt
VIEW
UPDATE
```

Injury detail may expose:

```txt
VIEW
UPDATE
RESOLVE
```

Calculate actions from:

- current user;
- role;
- `canViewMedicalDetails`;
- current team scope;
- player/team lifecycle;
- injury lifecycle;
- current revision.

Rules:

- frontend will use actions as hints;
- backend endpoints always enforce authorization independently;
- unknown future action codes are not inferred by clients;
- no action discloses hidden medical-detail permission to ordinary safe readers.

### Error handling

Use existing Result and ProblemDetails conventions.

Expected behavior:

- `400` for malformed route/query/request syntax;
- `401` for unauthenticated requests;
- `403` for explicit out-of-scope team or missing medical permission;
- `404` for missing or safely inaccessible player/availability/injury;
- `409` for revision mismatch, duplicate current availability creation, invalid lifecycle, invalid assignment eligibility, or team/player conflict;
- `422` for semantic field validation when consistent with existing conventions;
- safe `5xx` for persistence/infrastructure failures.

Do not leak:

- whether a hidden injury exists;
- diagnosis;
- notes;
- body area;
- hidden team/player existence;
- database/provider details;
- raw exception text;
- current revision IDs of inaccessible resources.

### Logging and observability

Server logs may include:

- operation name;
- entity ID;
- team ID;
- player ID;
- safe action;
- revision number/ID;
- correlation ID;
- safe error code.

Do not log:

- diagnosis;
- body area;
- restricted notes;
- coach-visible note contents;
- request bodies;
- serialized injury revisions;
- full availability responses;
- permission claims beyond safe authorization diagnostics.

Do not add medical analytics tracking.

### EF Core persistence

Add to `AppDbContext`:

```txt
PlayerAvailability
PlayerAvailabilityRevision
InjuryRecord
InjuryRecordRevision
```

Configure:

- required fields and bounds;
- enum persistence following repository conventions;
- unique player/team availability aggregate;
- unique revision number per aggregate;
- explicit current revision relationships;
- non-cascading player/team/user relationships;
- injury status/resolution consistency;
- revision actor/time fields;
- useful indexes for team/player/status/date queries;
- concurrency/current-pointer protection;
- no encrypted/free-form JSON substitute for modeled fields.

Practical indexes:

```txt
PlayerAvailability(PlayerId, TeamId) unique
PlayerAvailability(TeamId)
PlayerAvailabilityRevision(PlayerAvailabilityId, RevisionNumber) unique
PlayerAvailabilityRevision(RecordedAtUtc)
InjuryRecord(TeamId, Status, OccurredOn)
InjuryRecord(PlayerId, OccurredOn)
InjuryRecordRevision(InjuryRecordId, RevisionNumber) unique
InjuryRecordRevision(RecordedAtUtc)
```

Database constraints should enforce where practical:

- expected return validity cannot be fully status/date-validated only through SQL, so domain/Application validation remains required;
- resolved metadata all-or-none;
- current revision belongs to parent;
- positive revision numbers.

Avoid cascade cycles around current revision pointers.

### Migration

Create the migration through the approved `dotnet ef` workflow.

Keep migration files under:

```txt
backend/src/Infrastructure/Persistence/Migrations/
```

Use:

```txt
--output-dir Persistence/Migrations
```

Do not handwrite migration files unless CLI tooling is genuinely blocked.

Document any exception in `context/progress-tracker.md`.

### Tests

Add focused domain, application, authorization, privacy, persistence, audit, and integration tests.

#### Availability domain tests

Cover:

- stable status values;
- aggregate construction;
- unique player/team ownership;
- first revision;
- append revision;
- current revision pointer;
- immutable old revisions;
- revision numbering;
- no-op detection;
- effective date validation;
- same-day revision;
- future date rejection;
- nondecreasing effective date;
- expected return status/date rules;
- note normalization/bounds;
- zero/empty/null behavior where applicable;
- concurrency mismatch.

#### Availability eligibility tests

Cover:

- assignment covering backend current date;
- inclusive start/end dates;
- current/future/historical assignment cases;
- current-date logic rather than denormalized team;
- archived player rejection;
- inactive team rejection;
- out-of-team player rejection;
- no automatic assignment;
- history retained after transfer/archive.

#### Safe list/summary tests

Cover:

- all current assigned players included;
- no-record player synthesized as unknown;
- explicit unknown;
- status filter including synthetic unknown;
- search;
- pagination/order;
- exact summary counts;
- count sum equals total;
- no N+1 where testable;
- historical-only players excluded;
- restricted fields absent from serialized JSON;
- no injury existence indicator;
- team scope;
- explicit out-of-scope `403`.

#### Safe player-history tests

Cover:

- scoped team filtering;
- multi-team history restricted by current scope;
- archived player visibility;
- newest-first pagination;
- safe fields only;
- no injury linkage/details.

#### Availability mutation authorization tests

Cover:

- admin;
- in-scope medical staff;
- out-of-scope medical staff;
- medical staff without detail permission still updates safe availability;
- coach/data operator/analyst/viewer rejection;
- CSRF;
- stale expected revision;
- concurrent first create conflict;
- audit atomicity.

#### Injury domain tests

Cover:

- open creation;
- occurred date validation;
- at least one detail field;
- detail field bounds;
- append revision;
- no-op update;
- update after resolution for factual correction;
- resolve transition;
- resolved date validation;
- repeated resolve conflict;
- no reopen/delete;
- actor/time consistency;
- immutable player/team/occurred date;
- revision concurrency.

#### Injury authorization tests

Cover:

- admin read/mutation;
- in-scope medical staff with detail permission read/mutation;
- in-scope medical staff without detail permission denied injury routes;
- coach/analyst/data operator/viewer with detail flag may read but not mutate;
- same roles without flag denied;
- out-of-scope flag holder denied;
- missing profile denied safely;
- explicit team filter behavior;
- safe non-disclosure.

#### Injury list/detail/revision tests

Cover:

- filters;
- pagination/order;
- open/resolved;
- no note-content search;
- compact list omits note snippets;
- detail includes restricted current revision only when authorized;
- revision history;
- serialized JSON privacy;
- no availability inference;
- no N+1 projections.

#### Separation tests

Cover:

- injury create does not change availability;
- injury resolve does not change availability;
- unavailable status does not create injury;
- safe availability endpoints never query/project restricted values where testable;
- generic player/session/dashboard DTOs remain unchanged;
- no single conditional sensitive DTO.

#### Audit tests

Cover:

- entity/action codes;
- availability safe payload;
- note contents excluded;
- injury values excluded;
- changed field keys included;
- no-op/failed/unauthorized mutation creates no audit;
- mutation/audit atomicity;
- availability audit accessible to scoped ordinary reader;
- injury audit requires detail permission;
- pagination/filtering;
- synthetic unknown creates no event.

#### Persistence/migration tests

Cover:

- unique player/team aggregate;
- revision uniqueness;
- current revision constraints;
- non-cascading history;
- injury resolution consistency;
- migration application;
- safe decimal/date mappings not relevant to medical notes;
- no broad JSON medical record column.

### Documentation synchronization

Update:

```txt
context/architecture.md
context/progress-tracker.md
```

Record:

- privacy-tier design;
- safe and restricted route boundaries;
- exact role/permission behavior;
- availability revision model;
- synthetic unknown semantics;
- injury lifecycle and revisions;
- coach-visible note warning;
- no automatic availability/injury coupling;
- audit disclosure behavior;
- migration status;
- verification results;
- intentionally deferred UI, medical documents, treatment workflows, and dashboard composition.

If implementation reveals that the existing `CanViewMedicalDetails` policy resolves differently from this spec, update the relevant authorization context/spec before proceeding.

Do not silently weaken disclosure rules.

## Implementation

### 1. Synchronize medical architecture

Document:

- coach-safe availability;
- restricted injury details;
- role/permission rules;
- separate DTO/projection requirements;
- versioned/audited behavior.

### 2. Add availability domain model

Implement:

- `PlayerAvailability`;
- immutable revisions;
- status/date/return/note invariants;
- current pointer;
- expected revision concurrency;
- no-op detection.

### 3. Add injury domain model

Implement:

- `InjuryRecord`;
- open/resolved lifecycle;
- immutable detail revisions;
- current pointer;
- resolution invariants;
- expected revision concurrency.

### 4. Add authorization requirements

Implement or compose clear feature authorization for:

- safe availability read;
- safe availability mutation;
- restricted medical-detail read;
- injury mutation.

Reuse existing:

- role;
- `canViewMedicalDetails`;
- team scope;
- active-account checks.

Do not scatter permission logic across handlers.

### 5. Add EF Core mapping and migration

Configure:

- entities;
- revisions;
- current pointers;
- constraints;
- indexes;
- non-cascading relationships.

Generate and apply the migration.

### 6. Implement safe availability queries

Add:

- current team list;
- status counts;
- player availability history;
- safe actions;
- synthetic unknown behavior.

Use dedicated safe projections.

### 7. Implement availability mutation

Add append-only revision creation with:

- eligibility;
- validation;
- optimistic concurrency;
- current pointer;
- audit;
- atomic transaction.

### 8. Implement restricted injury queries

Add:

- list;
- detail;
- revision history;
- allowed actions.

Use restricted DTOs and medical-detail authorization.

### 9. Implement injury mutations

Add:

- create;
- append revision;
- resolve.

Keep injury and availability workflows separate.

### 10. Extend audit foundation

Add:

- entity types;
- action codes;
- privacy-safe payload builders;
- availability history endpoint;
- injury history endpoint;
- atomic integration.

### 11. Add privacy-focused tests

Verify serialized property absence, role/flag/team combinations, log/audit redaction, and no accidental coupling.

### 12. Update progress documentation

Update `context/progress-tracker.md` with actual implementation and verification state.

Do not mark Unit 50 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use existing backend packages and infrastructure:

- ASP.NET Core 8;
- EF Core/Npgsql;
- FluentValidation;
- Mapster;
- Unit 20 authorization policies;
- Unit 23 team scopes/account lifecycle;
- Unit 28 assignment history;
- Unit 38 audit foundation;
- existing Result/ProblemDetails patterns;
- approved UTC clock and current-user services;
- existing test infrastructure.

Do not add:

- medical terminology packages;
- healthcare interoperability packages;
- encryption packages solely for this unit;
- full-text search packages;
- document storage packages;
- notification/background-job packages;
- frontend dependencies.

## Verification checklist

- [ ] `context/architecture.md` documents coach-safe and restricted medical tiers.
- [ ] Safe availability and restricted injury data use separate routes, DTOs, queries, and projections.
- [ ] Safe DTOs do not define diagnosis, body area, restricted notes, injury IDs, or injury-existence fields.
- [ ] Restricted fields are not selected and later stripped from ordinary availability responses.
- [ ] Availability statuses are exactly `AVAILABLE`, `LIMITED`, `UNAVAILABLE`, `REHAB`, and `UNKNOWN`.
- [ ] One availability aggregate exists at most per player/team.
- [ ] Availability player/team ownership is immutable.
- [ ] Absence of an aggregate reads as synthesized `UNKNOWN`.
- [ ] Synthetic unknown creates no persistence or audit record.
- [ ] Availability revisions are append-only.
- [ ] Old revisions remain immutable.
- [ ] Current revision pointer updates atomically.
- [ ] Revision number is unique per availability aggregate.
- [ ] Exact no-op availability changes create no revision/audit.
- [ ] Availability mutation uses expected current revision concurrency.
- [ ] No last-write-wins availability behavior exists.
- [ ] Effective date is required, non-future, and nondecreasing.
- [ ] Expected return is allowed only for limited/unavailable/rehab.
- [ ] Expected return is null for available/unknown.
- [ ] Expected return is not before effective date.
- [ ] Coach-visible note is optional, trimmed, bounded, and explicitly operational.
- [ ] Coach-visible note is never auto-populated from restricted fields.
- [ ] Coach-visible note contents are excluded from audit payloads/logs.
- [ ] Availability mutation requires current-date assignment coverage.
- [ ] Current assignment shortcut does not replace Unit 28 date logic.
- [ ] Historical availability remains after transfer/archive.
- [ ] Safe availability list uses current assigned non-archived players.
- [ ] Status filter includes synthesized unknown.
- [ ] Availability summary counts use the identical player population.
- [ ] Summary counts are mutually exclusive and add to total.
- [ ] Summary returns no injury or diagnosis counts.
- [ ] Player availability history returns only scoped safe revisions.
- [ ] All active scoped roles can read safe availability.
- [ ] Only admin and in-scope medical staff can mutate safe availability.
- [ ] Medical staff without detail permission can still mutate safe availability.
- [ ] Stable injury statuses are `OPEN` and `RESOLVED`.
- [ ] Injury record player/team/occurred date are immutable.
- [ ] Injury records are not hard-deleted or archived.
- [ ] Multiple open injuries are allowed without guessed uniqueness.
- [ ] Injury revisions are append-only.
- [ ] Current injury revision pointer updates atomically.
- [ ] Exact no-op injury updates create no revision/audit.
- [ ] Injury update/resolve uses expected current revision concurrency.
- [ ] Occurred/resolved date rules are enforced.
- [ ] Resolved injuries cannot reopen.
- [ ] Factual restricted revisions may be added after resolution according to the spec.
- [ ] Injury create requires player assignment to team on occurred date.
- [ ] Injury create/update/resolve requires admin or in-scope medical staff with detail permission.
- [ ] `canViewMedicalDetails` plus team scope permits restricted read for non-admin roles.
- [ ] Detail permission alone does not permit injury mutation.
- [ ] Out-of-scope detail permission does not reveal records.
- [ ] Injury compact list exposes no restricted note snippets.
- [ ] Injury detail and revision history require medical-detail authorization.
- [ ] No note-content full-text search exists.
- [ ] Injury create/update/resolve never changes availability automatically.
- [ ] Availability changes never create/resolve injury records.
- [ ] No injury ID/existence indicator leaks through availability responses.
- [ ] Safe and restricted serialized JSON property tests pass.
- [ ] `PLAYER_AVAILABILITY` and `INJURY_RECORD` audit entity types exist.
- [ ] All required audit action codes exist.
- [ ] Availability audit excludes note contents and all injury data.
- [ ] Injury audit stores changed field keys but not restricted values.
- [ ] Immutable injury revisions remain the detailed authoritative history.
- [ ] Failed/no-op/unauthorized mutations create no audit event.
- [ ] Business mutations and audit entries commit atomically.
- [ ] Availability audit follows safe team-scoped read.
- [ ] Injury audit follows restricted medical-detail read.
- [ ] No global medical audit browser or mutation API is added.
- [ ] All required safe availability endpoints exist.
- [ ] All required restricted injury endpoints exist.
- [ ] No delete/archive/reopen/bulk mutation endpoint exists.
- [ ] Backend-owned actions are returned without disclosing hidden medical-detail state.
- [ ] ProblemDetails/logging never includes restricted values or request bodies.
- [ ] EF Core unique, revision, current-pointer, lifecycle, relationship, and index rules are configured.
- [ ] No broad JSON medical-record column is used.
- [ ] Migration is generated under `backend/src/Infrastructure/Persistence/Migrations/`.
- [ ] `dotnet ef database update` succeeds when the local database environment is available.
- [ ] Availability domain/eligibility/concurrency tests pass.
- [ ] Safe list/summary/history and serialized-privacy tests pass.
- [ ] Injury domain/lifecycle/revision tests pass.
- [ ] Role/permission/team-scope matrix tests pass.
- [ ] Separation/no-automatic-coupling tests pass.
- [ ] Audit privacy and atomicity tests pass.
- [ ] Migration/persistence tests pass.
- [ ] No frontend files are changed.
- [ ] No medical, healthcare, search, document, notification, or frontend package is added.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for affected backend projects/solution.
- [ ] `dotnet test` passes for relevant backend test projects.
- [ ] `context/progress-tracker.md` records actual Unit 50 privacy, model, authorization, migration, audit, and verification state.
