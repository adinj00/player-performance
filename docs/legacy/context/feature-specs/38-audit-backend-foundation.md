# Unit 38: Audit Backend Foundation

## Goal

Build the append-only backend audit foundation for important already implemented staff-account, access-control, match-report workflow, and manual match-statistics mutations. Persist safe semantic before/after change data atomically with successful business mutations and expose authorized entity-history queries for the Unit 39 UI without adding audit editing, deletion, export, retention jobs, frontend UI, or speculative coverage for future modules.

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
9. `context/feature-specs/20-backend-staff-roles-authorization-foundation.md`
10. `context/feature-specs/23-staff-users-team-scope-account-lifecycle-backend.md`
11. `context/feature-specs/32-match-report-workflow-backend.md`
12. `context/feature-specs/33-manual-match-statistics-backend.md`
13. `context/feature-specs/38-audit-backend-foundation.md`

Use relevant project-local skills from `.agents/skills/` when applicable.

Skills/plugins may guide implementation workflow, but must not override the project context files, architecture rules, code standards, or this spec.

This unit is backend-only.

Do not add or change frontend routes, pages, components, navigation, shadcn/ui components, or frontend API wrappers.

### Scope

This unit introduces:

- persistent append-only `AuditLog` records;
- Application-layer audit-writing abstraction;
- safe structured previous/new value snapshots;
- semantic audit action codes;
- audit writes integrated into existing critical mutations;
- transaction coordination so audited business changes and their audit records commit together;
- entity-specific audit history queries for match reports and staff users;
- role/team-scope-aware audit read authorization;
- pagination and deterministic ordering;
- actor summaries for future UI display;
- migration and tests.

Initial mutation coverage includes:

- staff invitation creation;
- staff invitation credential reissue;
- invitation acceptance/account activation;
- staff profile changes;
- staff role, permission, and team-scope replacement;
- staff account disable;
- staff account reactivate;
- match report creation;
- match report submit for review;
- match report verification;
- match report correction request;
- match report archive;
- manual player/goalkeeper statistics snapshot changes.

This unit does not introduce:

- audit frontend UI;
- a global audit administration page;
- audit record editing;
- audit record deletion;
- audit restore;
- audit export;
- retention/cleanup jobs;
- event streaming;
- message queues;
- event sourcing;
- system-wide automatic EF change tracking;
- IP-address or user-agent storage;
- failed-login/security-event auditing;
- login-email recovery;
- imports;
- medical/availability auditing;
- media auditing;
- player-management auditing;
- match-metadata auditing;
- lineup/appearance auditing;
- GPS auditing.

Future modules must add audit coverage when their own feature specs require it.

### Audit semantics

Audit logs record meaningful successful business actions.

They are not:

- application debug logs;
- request logs;
- exception logs;
- access logs;
- a copy of every database column update;
- a substitute for domain workflow metadata;
- a substitute for observability/telemetry.

Create audit entries explicitly from Application use cases after validation and authorization have passed, while the business transaction is still active.

Do not use a blanket EF Core `SaveChanges` interceptor that serializes every modified entity.

A generic interceptor cannot reliably determine:

- the business action;
- the authenticated actor;
- the target aggregate;
- safe field-level redaction;
- workflow intent;
- meaningful before/after structure.

Infrastructure may provide persistence support, but Application owns when and why an audit event is recorded.

### Append-only behavior

Audit records are immutable after creation.

Rules:

- no normal update method;
- no normal delete method;
- no archive method;
- no restore method;
- no hard-delete endpoint;
- no mutation endpoint of any kind;
- normal entity lifecycle operations must not cascade-delete audit history;
- later account disable/archive operations preserve existing audit entries;
- no V1 retention job removes old audit entries.

If an audit record is malformed because of an implementation defect, correction is an operational/database-administration concern outside normal product APIs.

### Audit record model

Add a persistent `AuditLog` entity using existing ID and UTC clock conventions.

Required fields:

```txt
Id
ActorUserId
Action
EntityType
EntityId
OccurredAtUtc
PreviousValuesJson
NewValuesJson
MetadataJson
```

Field behavior:

- `ActorUserId` identifies the user responsible for the successful action;
- `Action` is a stable English semantic action code;
- `EntityType` is a stable English aggregate/entity category;
- `EntityId` is the stable identifier of the primary audited aggregate;
- `OccurredAtUtc` uses the approved clock abstraction;
- `PreviousValuesJson` is optional structured JSON containing only relevant changed values before the action;
- `NewValuesJson` is optional structured JSON containing only relevant changed values after the action;
- `MetadataJson` is optional structured JSON for safe contextual values that are not direct before/after fields.

Use PostgreSQL `jsonb` for the JSON fields.

Do not store serialized CLR type names.

Do not depend on frontend labels or Bosnian translations in persisted audit codes.

### Actor rules

Current initial audited actions always have a known user identity.

Actor resolution:

- authenticated administration/report/statistics mutations use the current authenticated user ID;
- invitation acceptance uses the invited/activated target user's ID as the actor because possession and successful use of that account-specific one-time credential performs the activation;
- do not trust actor IDs supplied by clients;
- do not derive actor identity from request bodies;
- do not persist display names/emails as the authoritative actor identity.

Unit 39 may resolve current actor display summaries through the preserved staff account.

If a future system/background action has no user actor, its feature spec must define system-actor behavior before implementation. Do not invent a fake user in this unit.

### Stable entity types

Use stable English entity-type constants.

Initial values:

```txt
STAFF_USER
MATCH_REPORT
```

Rules:

- staff profile/access/lifecycle/invitation actions target `STAFF_USER`;
- match report creation, workflow transitions, and report-owned statistics changes target `MATCH_REPORT`;
- statistics audit metadata may identify affected `PlayerMatchAppearanceId` values, but the primary entity remains the report;
- do not create separate top-level entity types for every statistics row in this unit;
- do not persist .NET class names as entity types.

This structure allows Unit 39 to show one coherent history on:

- a staff-user detail page;
- a match-report review page.

### Stable action codes

Use centralized constants or an equivalent constrained value model.

Initial action codes:

```txt
STAFF_INVITATION_CREATED
STAFF_INVITATION_REISSUED
STAFF_INVITATION_ACCEPTED
STAFF_PROFILE_UPDATED
STAFF_ACCESS_REPLACED
STAFF_ACCOUNT_DISABLED
STAFF_ACCOUNT_REACTIVATED

MATCH_REPORT_CREATED
MATCH_REPORT_SUBMITTED
MATCH_REPORT_VERIFIED
MATCH_REPORT_CORRECTION_REQUESTED
MATCH_REPORT_ARCHIVED
MATCH_REPORT_STATISTICS_UPDATED
```

Do not scatter raw action strings across handlers.

Do not use localized action labels in persistence.

Unit 39 will map stable codes to localized display text.

### Successful mutations only

Create an audit entry only when a business mutation succeeds and is committed.

Do not create business audit entries for:

- unauthenticated requests;
- forbidden actions;
- validation failures;
- missing resources;
- invalid transitions;
- duplicate conflicts;
- last-active-admin conflicts;
- statistics snapshot mismatches;
- database rollback;
- network failure before commit.

Those events may appear in normal operational logs, but they are not successful business-history entries.

### No-op behavior

Do not create misleading audit noise for semantic no-op mutations.

Examples:

- saving an unchanged staff display name;
- replacing staff access with an identical normalized role/permission/scope state;
- idempotently disabling an already disabled account when the existing use case returns success without a state change;
- saving an identical statistics snapshot;
- issuing a workflow request that performs no valid transition.

Invitation credential reissue is not a no-op because it invalidates/replaces a credential, even though the credential itself must never be audited.

The existing business endpoint behavior may remain unchanged; the audit layer decides whether a meaningful committed change occurred.

### Atomicity and transaction boundaries

For every required audited mutation:

- the business change and audit record must commit in the same database transaction;
- if audit persistence fails, the business mutation must roll back;
- if the business mutation fails, no audit entry remains;
- do not enqueue audit work for later;
- do not use fire-and-forget tasks;
- do not write audit logs after returning the HTTP response.

Reuse existing Unit of Work/transaction patterns.

Where ASP.NET Core Identity and application tables participate in the same PostgreSQL database:

- coordinate Identity changes, access-profile/scope changes, and audit persistence inside the existing safe transaction approach;
- do not create a second independent transaction that can commit only half of the operation;
- preserve security-stamp/session invalidation behavior.

Do not introduce a distributed transaction.

### Audit writing abstraction

Add an Application-layer abstraction equivalent to:

```txt
IAuditWriter
- AddAsync(AuditEntry entry, CancellationToken cancellationToken)
```

The exact name and shape may follow existing conventions.

The abstraction must accept an already safe semantic audit payload.

It must not accept:

- raw HTTP requests;
- endpoint context;
- arbitrary EF entities;
- passwords;
- tokens;
- unfiltered request DTOs;
- exception objects.

Provide small builders/factories/helpers where useful for:

- normalized staff access snapshots;
- report status transitions;
- statistics diffs;
- metadata dictionaries.

Avoid a reflection-heavy generalized object-diff framework.

Explicit mapping is preferred for security and stability.

### Structured JSON rules

Persist JSON objects with stable English property names.

Values must be JSON-native:

- string;
- number;
- boolean;
- null;
- arrays;
- nested objects.

Do not store JSON as a doubly encoded JSON string.

Normalize unordered collections before comparison/persistence.

Examples:

- sort `selectedTeamIds`;
- use deterministic appearance-ID ordering;
- use stable statistic-field ordering;
- preserve explicit `null`;
- preserve numeric zero.

The serializer must use the existing backend JSON naming convention consistently.

### Safe data and redaction

Never store any of the following in audit JSON or metadata:

- passwords;
- password hashes;
- invitation tokens;
- invitation setup credentials;
- password-reset tokens;
- email-confirmation tokens;
- security stamps;
- concurrency stamps unless specifically required for technical concurrency and approved later;
- authentication cookies;
- CSRF tokens;
- bearer tokens;
- API keys;
- connection strings;
- private storage credentials;
- full request headers;
- full request bodies;
- stack traces;
- raw exception text.

Do not audit the one-time token returned by invitation creation/reissue.

Do not serialize full Identity entities or complete EF tracked objects.

Safe current-scope data may include:

- display name;
- login email where part of staff invitation/account identity;
- account status;
- primary role;
- explicit permission booleans;
- team-scope type;
- selected team IDs;
- report status;
- correction reason;
- applied tracking level;
- changed statistics values;
- match/report IDs;
- affected appearance IDs.

Email values are sensitive administrative data. Return them only through user-audit APIs protected by `AdminOnly`.

### JSON size and relevance

Store only values relevant to the semantic action.

Do not store a full aggregate snapshot for every change when a smaller change set is sufficient.

Use bounded validation/serialization.

If a generated audit payload exceeds a documented safe limit:

- fail the audited business mutation before commit;
- return a safe unexpected/operational error through the established pipeline;
- log the technical issue without logging the sensitive payload;
- document the incident in `context/progress-tracker.md` during development.

Do not silently truncate JSON because truncation would make history misleading.

### Staff invitation creation audit

After successful `POST /api/users/invitations`, create:

```txt
EntityType = STAFF_USER
Action = STAFF_INVITATION_CREATED
EntityId = created user ID
```

Suggested values:

`PreviousValuesJson`:

```json
null
```

`NewValuesJson`:

```txt
displayName
email
status = INVITED
primaryRole
permissions
teamScopeType
selectedTeamIds
```

Suggested metadata:

```txt
invitationIssued = true
```

Do not include the setup token or setup URL.

### Staff invitation reissue audit

After successful invitation reissue, create:

```txt
EntityType = STAFF_USER
Action = STAFF_INVITATION_REISSUED
EntityId = target user ID
```

Do not include:

- previous token;
- new token;
- token hash;
- token provider internals.

Metadata may safely include:

```txt
credentialReissued = true
```

The action itself is the important history.

### Invitation acceptance audit

After successful invitation acceptance/account activation, create:

```txt
EntityType = STAFF_USER
Action = STAFF_INVITATION_ACCEPTED
EntityId = activated user ID
ActorUserId = activated user ID
```

Suggested previous/new values:

```txt
status
mustChangePassword when part of the established account contract
setupCompleted
```

Do not include the submitted password, token, Identity password hash, or security-stamp values.

The acceptance and audit entry must commit atomically with account activation/security-state changes.

### Staff profile update audit

After a meaningful display-name/profile update, create:

```txt
EntityType = STAFF_USER
Action = STAFF_PROFILE_UPDATED
EntityId = target user ID
```

Store only changed safe profile fields.

Do not include role, permission, scope, account-status, password, or token values unless that action actually changes them.

Do not create this action for a normalized no-op.

### Staff access replacement audit

After successful `PUT /api/users/{userId}/access`, create:

```txt
EntityType = STAFF_USER
Action = STAFF_ACCESS_REPLACED
EntityId = target user ID
```

Both previous and new snapshots must include the complete normalized access state:

```txt
primaryRole
permissions:
  canVerifyReports
  canImportData
  canViewMedicalDetails
teamScope:
  type
  selectedTeamIds
```

Rules:

- selected team IDs are unique and deterministically sorted;
- admin normalization to `ALL_TEAMS` is reflected in the stored snapshot;
- record only the final persisted state, not untrusted raw request values;
- do not write an audit entry when normalized previous/new states are identical.

### Staff disable/reactivate audit

Disable:

```txt
Action = STAFF_ACCOUNT_DISABLED
EntityType = STAFF_USER
EntityId = target user ID
```

Reactivate:

```txt
Action = STAFF_ACCOUNT_REACTIVATED
EntityType = STAFF_USER
EntityId = target user ID
```

Store at minimum:

```txt
previous status
new status
```

Metadata may include safe lifecycle context such as:

```txt
sessionsInvalidated = true
reactivationResult = INVITED | ACTIVE
```

Do not store security stamps or cookie values.

No audit entry is created when a rejected lifecycle transition does not commit.

### Match report creation audit

After successful report creation, create:

```txt
EntityType = MATCH_REPORT
Action = MATCH_REPORT_CREATED
EntityId = report ID
```

Suggested new values:

```txt
matchId
status = DRAFT
```

Metadata may include:

```txt
teamId
```

Use persisted values, not request-body assumptions.

### Match report workflow transition audit

Create one audit entry for each successful explicit transition:

```txt
MATCH_REPORT_SUBMITTED
MATCH_REPORT_VERIFIED
MATCH_REPORT_CORRECTION_REQUESTED
MATCH_REPORT_ARCHIVED
```

Use:

```txt
EntityType = MATCH_REPORT
EntityId = report ID
```

Previous/new values must include at minimum:

```txt
status
```

Include relevant workflow metadata changed by the transition:

Submit:

```txt
submittedByUserId
submittedAtUtc
```

Verify:

```txt
verifiedByUserId
verifiedAtUtc
```

Correction request:

```txt
lastCorrectionRequestedByUserId
lastCorrectionRequestedAtUtc
lastCorrectionReason
```

Archive:

```txt
archive metadata
```

Correction reason may be stored because it is an operationally meaningful workflow value already visible to authorized report readers.

Do not create a generic `MATCH_REPORT_STATUS_CHANGED` action when a more specific semantic action exists.

### Statistics audit target and granularity

Audit each meaningful successful atomic Unit 33 statistics save as one report-level entry:

```txt
EntityType = MATCH_REPORT
Action = MATCH_REPORT_STATISTICS_UPDATED
EntityId = report ID
```

Do not create hundreds of independent audit rows per cell.

The one entry must contain an explicit change set sufficient to understand what changed.

Include only changed statistics rows/fields.

Suggested structure:

```txt
PreviousValuesJson:
  playerStatistics:
    [appearanceId]:
      changedField: previousValue
  goalkeeperStatistics:
    [appearanceId]:
      changedField: previousValue

NewValuesJson:
  playerStatistics:
    [appearanceId]:
      changedField: newValue
  goalkeeperStatistics:
    [appearanceId]:
      changedField: newValue

MetadataJson:
  matchId
  appliedTrackingLevel
  affectedPlayerAppearanceIds
  affectedGoalkeeperAppearanceIds
  goalkeeperRowsAdded
  goalkeeperRowsRemoved
```

Rules:

- preserve `null` versus `0`;
- include row additions/removals;
- include first-time `AppliedTrackingLevel` freezing when it changes from unset to a value;
- use stable field codes from Unit 33;
- include only enabled fields and persisted changes;
- do not include player names as the authoritative change key;
- do not include disabled or rejected request fields;
- do not create an audit entry for a semantically identical snapshot.

The statistics business update and audit diff must use the same loaded previous state within the same transaction.

### Historical data before Unit 38

Do not fabricate or reconstruct audit records for mutations that occurred before Unit 38 was deployed.

Rules:

- no backfill based on current timestamps;
- no inference of past actors;
- no synthetic transition history;
- no conversion of current workflow metadata into fake historical events.

Audit history begins with successfully committed audited mutations after the Unit 38 migration/implementation is active.

Unit 39 must handle an empty audit history honestly.

### Audit read APIs

Provide entity-specific read endpoints required by Unit 39.

Required routes:

```txt
GET /api/match-reports/{reportId}/audit
GET /api/users/{userId}/audit
```

Use the existing endpoint-group conventions.

Do not add a global `/api/audit` browser in this unit.

Do not add mutation routes.

### Match-report audit authorization

`GET /api/match-reports/{reportId}/audit` must use the same safe report visibility rules as the Unit 32 report detail query.

Requirements:

- `ADMIN` may read all report audit history;
- non-admin access remains restricted by team scope;
- report status visibility remains consistent with Unit 32;
- users who may view the report may view its audit history;
- users who cannot discover/read the report must not discover its audit entries;
- safely inaccessible reports use the established non-disclosure response;
- audit JSON returned to report readers contains only report/statistics-safe fields.

Do not return staff-account audit data through this endpoint.

### Staff-user audit authorization

`GET /api/users/{userId}/audit` requires `AdminOnly`.

Requirements:

- unauthenticated request returns `401`;
- authenticated non-admin request returns `403`;
- missing target user follows existing admin staff-detail behavior;
- account disable does not make its audit history unavailable to admins;
- invitation/setup secrets are never returned because they were never persisted;
- email/access/lifecycle before/after values may be returned only through this admin-protected endpoint.

Do not expose user audit history through session or public invitation endpoints.

### Audit query contract

Both entity-history endpoints support bounded pagination.

Recommended query parameters:

```txt
page
pageSize
action
dateFrom
dateTo
```

`action` is optional and must be validated against known/persisted action conventions without allowing arbitrary SQL behavior.

Response items include at minimum:

```txt
id
actor:
  id
  displayName
action
entityType
entityId
occurredAtUtc
previousValues
newValues
metadata
```

Actor email is not required for report audit responses.

For the admin-only staff-user endpoint, include actor email only if the existing safe staff-summary convention already requires it. Prefer ID and display name.

If an actor summary cannot be resolved:

- retain the immutable actor ID;
- return a safe fallback display state;
- do not drop the audit record.

Do not expose internal JSON serialization strings. Return structured JSON objects.

### Audit query ordering and pagination

Default ordering:

```txt
OccurredAtUtc descending
Id descending
```

The secondary ID order makes pagination deterministic for equal timestamps.

Rules:

- bounded page size;
- no unpaginated full history;
- filter in the database;
- do not load all entries into memory;
- no client-supplied sort expressions;
- use UTC date filters;
- include pagination metadata consistent with existing project conventions.

### Clean Architecture boundaries

Domain owns:

- append-only `AuditLog` entity/invariants if consistent with the existing domain model;
- stable audit action/entity-type value constraints or constants when domain-appropriate.

Application owns:

- audit-writing abstraction;
- semantic audit payload construction;
- before/after mapping;
- authorization orchestration for history queries;
- audit query contracts/read models;
- transaction-level coordination from existing use cases.

Infrastructure owns:

- EF Core configuration;
- `jsonb` persistence;
- audit repository/writer implementation;
- query implementation;
- migration;
- actor-summary data joins/queries through approved abstractions.

API owns:

- endpoint mapping;
- HTTP input/output mapping;
- policy application where appropriate;
- ProblemDetails-compatible result mapping.

Do not put audit payload construction or EF queries in endpoint handlers.

### Persistence and migration

Add audit persistence to the existing `AppDbContext`.

Create the migration using the approved `dotnet ef` workflow.

Migration files remain under:

```txt
backend/src/Infrastructure/Persistence/Migrations/
```

Use:

```txt
--output-dir Persistence/Migrations
```

Configure:

- table and column naming consistent with the repository;
- `jsonb` for previous/new/metadata;
- required actor relationship using non-cascading behavior;
- bounded action/entity-type columns;
- UTC timestamp;
- no cascade delete from staff user, report, or other entities;
- indexes for entity-history and actor-history queries.

Required practical indexes include:

```txt
(EntityType, EntityId, OccurredAtUtc)
(ActorUserId, OccurredAtUtc)
(Action, OccurredAtUtc)
```

Use provider-supported descending/index configuration when practical; correctness does not depend on descending physical index order.

Do not create foreign keys from generic `EntityId` to multiple target tables.

`EntityId` is a stable logical identifier constrained by `EntityType`.

### Logging and observability

Normal application logging may record:

- audit write failure;
- action code;
- entity type;
- entity ID;
- correlation/request identifier where available.

Normal logs must not include:

- previous/new JSON payloads by default;
- correction reason;
- statistics values;
- email snapshots;
- tokens;
- passwords;
- security metadata.

Do not silently swallow audit persistence failures.

### Tests

Add focused domain/unit, application, and integration tests.

Audit model/writer tests should cover:

- required fields;
- append-only construction;
- stable action/entity values;
- structured JSON serialization;
- deterministic normalization;
- no secret-bearing fields in builders;
- `null` and zero preservation;
- no-op detection.

Staff mutation integration tests should cover:

- invitation creation creates one safe audit entry;
- invitation reissue creates an entry without token data;
- invitation acceptance creates an entry without password/token data;
- profile change records only changed profile fields;
- access replacement records normalized previous/new role, permissions, and scope;
- selected-team IDs are deterministic;
- identical access replacement creates no new audit entry;
- disable records previous/new status;
- reactivate records the resulting status;
- rejected final-admin/authorization/validation changes create no audit entry;
- failed transaction leaves neither business change nor audit entry.

Report workflow integration tests should cover:

- report creation audit;
- submit audit;
- verify audit;
- correction-request audit including reason;
- archive audit;
- invalid/rejected transitions create no audit entry;
- actor IDs are correct;
- status before/after values are correct.

Statistics integration tests should cover:

- initial statistics save creates one report-level audit entry;
- changed fields only are represented;
- zero versus null is preserved;
- player row changes are represented;
- goalkeeper row add/change/remove is represented;
- first `AppliedTrackingLevel` freeze is represented;
- identical snapshot creates no audit entry;
- validation/workflow conflict creates no audit entry;
- statistics and audit commit atomically.

Read API integration tests should cover:

- report audit uses report visibility/team scope;
- inaccessible report audit does not leak existence;
- coach/viewer access matches Unit 32 verified/archived visibility;
- user audit is admin-only;
- disabled/invited target user history remains available to admin;
- ordering is newest first and deterministic;
- pagination is bounded;
- action/date filters work;
- actor summary/fallback behavior works;
- returned JSON is structured;
- no secret fields are present;
- there are no audit mutation endpoints.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

Record:

- migration creation/application result;
- implemented initial action codes;
- integrated mutation coverage;
- intentionally deferred modules;
- verification results;
- any environment limitation preventing transaction/database verification.

If implementation reveals that an existing mutation cannot be audited atomically with the current architecture, stop that integration and update the relevant architecture/context before proceeding. Do not accept a best-effort non-atomic audit write silently.

## Implementation

### 1. Add the audit domain and Application contracts

Create:

- append-only `AuditLog`;
- stable entity-type constants;
- stable action-code constants;
- safe audit entry contract;
- `IAuditWriter` or equivalent;
- query/read models;
- structured value helpers.

Keep the contracts free of ASP.NET Core and EF Core dependencies.

### 2. Add safe payload builders

Implement explicit builders/mappers for:

- staff invitation/account snapshots;
- staff access snapshots;
- account-status transitions;
- report workflow transitions;
- statistics change sets.

Include redaction by construction.

Do not build a generic reflection-based serializer/diff engine.

### 3. Add Infrastructure persistence

Implement:

- EF Core entity configuration;
- `jsonb` value persistence;
- audit writer/repository;
- audit history query projections;
- actor-summary resolution;
- required indexes;
- non-cascading relationships.

Generate and apply the migration through `dotnet ef`.

### 4. Integrate staff invitation and lifecycle auditing

Update the existing Unit 23 use cases so the following successful committed actions create audit entries:

- invitation creation;
- invitation reissue;
- invitation acceptance;
- profile update;
- access replacement;
- disable;
- reactivate.

Preserve all current authorization, validation, last-admin, Identity, token, and session-invalidation behavior.

### 5. Integrate match-report workflow auditing

Update Unit 32 use cases so successful committed actions create semantic audit entries:

- report creation;
- submit;
- verify;
- request correction;
- archive.

Do not change transition rules or `allowedActions`.

### 6. Integrate statistics auditing

Update the Unit 33 atomic save handler.

Before persistence:

- load the current persisted statistics state;
- compare normalized persisted and requested snapshots;
- build a changed-fields-only audit change set;
- include row additions/removals and tracking-level freeze;
- skip audit creation for a semantic no-op.

Commit statistics and audit together.

Do not alter the statistics API contract.

### 7. Add entity-history queries

Implement:

```txt
GET /api/match-reports/{reportId}/audit
GET /api/users/{userId}/audit
```

Add:

- bounded pagination;
- optional action/date filters;
- deterministic ordering;
- actor summaries;
- structured JSON responses;
- safe errors.

### 8. Enforce audit read authorization

Reuse:

- Unit 32 report visibility/team-scope behavior for match-report history;
- `AdminOnly` for staff-user history.

Do not invent parallel access rules.

### 9. Add tests

Add unit/application/integration tests for:

- payload safety;
- mutation coverage;
- atomicity;
- no-op behavior;
- failed-mutation behavior;
- authorization;
- ordering/pagination;
- secret exclusion.

### 10. Update progress documentation

Update `context/progress-tracker.md` with actual implementation and verification state.

Do not mark Unit 38 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use existing backend infrastructure and approved packages:

- ASP.NET Core 8;
- EF Core;
- Npgsql/PostgreSQL;
- FluentValidation where needed;
- existing Result/ProblemDetails patterns;
- existing authentication/current-user/team-scope services;
- existing test infrastructure.

Do not add:

- an event bus;
- a message queue;
- an event-sourcing framework;
- a generic object-diff package;
- a JSON patch package;
- a second ORM;
- an audit SaaS SDK;
- a logging database/package solely for this feature.

## Verification checklist

- [ ] Persistent append-only `AuditLog` exists.
- [ ] Audit fields include actor, action, entity type, entity ID, UTC timestamp, previous values, new values, and metadata.
- [ ] Previous/new/metadata values use PostgreSQL `jsonb`.
- [ ] Stable action and entity-type codes are centralized.
- [ ] No localized labels are persisted as audit codes.
- [ ] No audit update, delete, archive, restore, or mutation endpoint exists.
- [ ] Audit history is not cascade-deleted by staff/report lifecycle changes.
- [ ] No retention/cleanup job is added.
- [ ] Audit events are created explicitly from Application use cases.
- [ ] A blanket automatic EF entity-dump interceptor is not used.
- [ ] Business mutations and required audit writes commit atomically.
- [ ] Audit failure rolls back the business mutation.
- [ ] Business failure leaves no audit entry.
- [ ] Fire-and-forget or post-response audit writes are not used.
- [ ] Only successful committed business actions are audited.
- [ ] Semantic no-op staff/statistics saves do not create audit noise.
- [ ] Invitation reissue remains auditable even though tokens are excluded.
- [ ] Passwords, password hashes, invitation tokens, reset tokens, security stamps, cookies, CSRF values, API keys, and connection strings are never audited.
- [ ] Full request bodies, Identity entities, exception objects, and stack traces are never serialized into audit entries.
- [ ] `STAFF_USER` and `MATCH_REPORT` are the initial stable entity types.
- [ ] All specified staff action codes exist.
- [ ] All specified match-report/statistics action codes exist.
- [ ] Staff invitation creation creates a safe audit entry.
- [ ] Staff invitation reissue creates an entry without credential data.
- [ ] Invitation acceptance creates an entry without password/token data.
- [ ] Staff profile changes record only meaningful changed fields.
- [ ] Staff access replacement records normalized previous/new role, permissions, scope type, and selected team IDs.
- [ ] Selected team IDs are stored deterministically.
- [ ] Staff disable/reactivate records previous/new account status.
- [ ] Rejected last-active-admin or authorization operations create no audit entry.
- [ ] Match report creation creates a `MATCH_REPORT_CREATED` entry.
- [ ] Submit creates a `MATCH_REPORT_SUBMITTED` entry.
- [ ] Verify creates a `MATCH_REPORT_VERIFIED` entry.
- [ ] Correction request creates a `MATCH_REPORT_CORRECTION_REQUESTED` entry with the safe reason metadata/change.
- [ ] Archive creates a `MATCH_REPORT_ARCHIVED` entry.
- [ ] Invalid workflow transitions create no audit entry.
- [ ] Statistics save creates one report-level `MATCH_REPORT_STATISTICS_UPDATED` entry.
- [ ] Statistics audit stores changed fields rather than one audit row per cell.
- [ ] Statistics audit preserves `null` versus numeric zero.
- [ ] Goalkeeper row additions, changes, and removals are represented.
- [ ] First `AppliedTrackingLevel` freeze is represented when applicable.
- [ ] Identical statistics snapshots create no audit entry.
- [ ] Failed statistics validation/workflow conflicts create no audit entry.
- [ ] No historical audit events are fabricated for pre-Unit-38 changes.
- [ ] `GET /api/match-reports/{reportId}/audit` exists.
- [ ] Report audit authorization reuses Unit 32 visibility and team-scope rules.
- [ ] Inaccessible report history does not leak report existence.
- [ ] `GET /api/users/{userId}/audit` exists.
- [ ] Staff-user audit history is `AdminOnly`.
- [ ] Disabled/invited staff history remains readable to admins.
- [ ] Audit history responses use structured JSON objects rather than encoded JSON strings.
- [ ] Actor ID and safe display summary are returned.
- [ ] Missing actor summary does not remove the immutable audit entry.
- [ ] History ordering is `OccurredAtUtc` descending with deterministic ID tie-breaking.
- [ ] Pagination is bounded and performed in the database.
- [ ] Optional action/date filters are validated and database-backed.
- [ ] Required audit indexes exist.
- [ ] Generic entity IDs do not use impossible polymorphic foreign keys.
- [ ] No global audit browser endpoint is added in Unit 38.
- [ ] No frontend files are changed.
- [ ] No import, medical, media, player, match-metadata, lineup, GPS, login-email-recovery, export, or notification audit scope is added prematurely.
- [ ] Migration files are generated under `backend/src/Infrastructure/Persistence/Migrations/`.
- [ ] `dotnet ef database update` succeeds when the local database environment is available.
- [ ] Relevant domain/unit tests pass.
- [ ] Relevant integration tests pass.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for affected backend projects/solution.
- [ ] `dotnet test` passes for relevant backend test projects.
- [ ] `context/progress-tracker.md` records actual Unit 38 coverage, deferred audit areas, migration status, and verification outcome.
