# Unit 32: Match Report Workflow Backend

## Goal

Build the backend workflow for one match report per played FK Velež match, including report lifecycle states, backend-owned allowed actions, review and verification permissions, correction requests, and workflow-aware locking of match data. Keep statistics, GPS, media, imports, audit-log persistence, and frontend UI out of scope.

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
9. `context/feature-specs/30-matches-backend-foundation.md`
10. `context/feature-specs/31-match-lineup-appearance-backend.md`
11. `context/feature-specs/32-match-report-workflow-backend.md`

Use relevant installed backend Codex skills/plugins when applicable. Skills/plugins may guide implementation workflow but must not override project context, architecture rules, code standards, or this spec.

This unit is backend-only.

Do not add or change frontend routes, pages, components, navigation, shadcn/ui components, or frontend API wrappers in this unit.

### Scope

This unit introduces the lifecycle and authorization model for a match report.

Each match may have at most one match report.

The report workflow must support:

```txt
DRAFT
READY_FOR_REVIEW
VERIFIED
NEEDS_CORRECTION
ARCHIVED
```

The report is the workflow boundary for match analysis data.

This unit owns:

- creation of a draft report for a played match;
- explicit status transitions;
- correction request behavior;
- verification permission behavior;
- backend-calculated `allowedActions`;
- current workflow metadata such as who submitted, verified, or requested correction and when;
- workflow-aware locking of match metadata and lineup/appearance mutations;
- report list/detail read models needed by later UI and dashboard units;
- persistence and tests for the workflow.

This unit does not introduce:

- player match statistics;
- goalkeeper statistics;
- team match statistics;
- GPS/physical metrics;
- media;
- imports;
- audit-log storage;
- frontend report UI;
- comments or threaded review discussions;
- version history;
- report export.

Those remain owned by later units.

### Report identity and relationship to match

Add a persistent `MatchReport` entity.

Rules:

- one `MatchReport` belongs to exactly one `Match`;
- one match may have at most one report;
- enforce the one-report-per-match invariant at both application and database levels;
- the report has its own stable identifier;
- the report must not be hard-deleted;
- report status is not the same as match status.

At minimum, the report should contain:

- `Id`;
- `MatchId`;
- `Status`;
- `CreatedByUserId`;
- `CreatedAtUtc`;
- `UpdatedAtUtc`;
- optional `SubmittedByUserId`;
- optional `SubmittedAtUtc`;
- optional `VerifiedByUserId`;
- optional `VerifiedAtUtc`;
- optional `LastCorrectionRequestedByUserId`;
- optional `LastCorrectionRequestedAtUtc`;
- optional `LastCorrectionReason`;
- optional archive metadata consistent with project conventions.

Use existing user identifier conventions. Do not introduce a local duplicate user profile just for workflow metadata.

The current correction reason is not a substitute for future audit history. Unit 38 will add persistent audit records for workflow transitions and historical changes.

### Report creation rules

A draft report is created explicitly.

Required endpoint behavior:

```txt
POST /api/matches/{matchId}/report
```

Creation rules:

- the caller must be authenticated;
- the match must exist and be accessible to the caller;
- the match must not be archived;
- the match status must be `PLAYED`;
- only `ADMIN` or `DATA_OPERATOR` may create a report;
- `DATA_OPERATOR` may create only for a match whose `TeamId` is within their authorized team scope;
- a second report for the same match returns `409 Conflict`;
- the new report starts in `DRAFT`;
- the creator and creation timestamp are recorded.

Do not auto-create a report when a match is created or when a lineup is saved.

Do not create reports for `SCHEDULED`, `POSTPONED`, or `CANCELLED` matches.

### Workflow transitions

Use explicit domain/application transition methods. Endpoint handlers and persistence code must not assign report status directly.

Required transitions:

```txt
DRAFT -> READY_FOR_REVIEW

READY_FOR_REVIEW -> VERIFIED
READY_FOR_REVIEW -> NEEDS_CORRECTION

NEEDS_CORRECTION -> READY_FOR_REVIEW

VERIFIED -> NEEDS_CORRECTION
VERIFIED -> ARCHIVED
```

Within this unit, `ARCHIVED` is terminal.

Do not implement restore from `ARCHIVED` until the architecture explicitly defines the restore target and audit behavior.

Invalid transitions return `409 Conflict`.

Transition behavior:

#### Submit for review

Allowed from:

- `DRAFT`;
- `NEEDS_CORRECTION`.

Requirements:

- caller is `ADMIN` or `DATA_OPERATOR`;
- caller has access to the match team;
- match remains `PLAYED` and not archived;
- the match has at least one persisted `PlayerMatchAppearance`;
- any readiness checks introduced by this unit pass.

On success:

- status becomes `READY_FOR_REVIEW`;
- `SubmittedByUserId` and `SubmittedAtUtc` are set to the latest submission actor/time;
- current correction reason may remain available for history/display until a later correction request replaces it, but it must not block resubmission.

Do not require player or goalkeeper statistics yet; Unit 33 will add statistics-specific readiness validation.

#### Verify

Allowed from:

- `READY_FOR_REVIEW`.

Requirements:

- `ADMIN` may verify;
- `ANALYST` may verify only when `canVerifyReports = true`;
- caller must have access to the match team;
- match remains `PLAYED` and not archived.

On success:

- status becomes `VERIFIED`;
- `VerifiedByUserId` and `VerifiedAtUtc` are recorded.

Do not allow `DATA_OPERATOR`, `COACH`, `MEDICAL_STAFF`, or `VIEWER` to verify.

#### Request correction

Allowed from:

- `READY_FOR_REVIEW`;
- `VERIFIED`.

Requirements:

- `ADMIN` may request correction;
- `ANALYST` may request correction for teams in their authorized scope;
- `ANALYST` does not need `canVerifyReports` merely to request correction;
- correction reason is required;
- correction reason is trimmed, non-empty, and bounded by a reasonable maximum length consistent with existing validation conventions.

On success:

- status becomes `NEEDS_CORRECTION`;
- `LastCorrectionRequestedByUserId` is recorded;
- `LastCorrectionRequestedAtUtc` is recorded;
- `LastCorrectionReason` is stored.

A correction request reopens editable report-owned data for the authorized data-entry workflow.

#### Archive report

Allowed from:

- `VERIFIED`.

Requirements:

- `ADMIN` only;
- caller may archive across all teams according to admin access.

On success:

- status becomes `ARCHIVED`;
- archive metadata is recorded.

Do not use match archive behavior as a substitute for report archive behavior.

### Backend-owned allowed actions

Every report read model must expose backend-calculated `allowedActions`.

Do not hardcode workflow actions in the frontend later.

Use stable internal action names. The exact enum/type names may follow existing project conventions, but the semantic actions must include at least:

```txt
EDIT
SUBMIT_FOR_REVIEW
VERIFY
REQUEST_CORRECTION
ARCHIVE
VIEW
```

`allowedActions` must be calculated from:

- report status;
- caller role;
- caller team scope;
- explicit permission flags such as `canVerifyReports`;
- match archive state;
- match status where relevant.

Expected behavior:

#### `DRAFT`

Authorized `ADMIN` / `DATA_OPERATOR`:

- `VIEW`
- `EDIT`
- `SUBMIT_FOR_REVIEW`

Other authorized readers:

- no mutation actions.

#### `READY_FOR_REVIEW`

Authorized `ADMIN`:

- `VIEW`
- `VERIFY`
- `REQUEST_CORRECTION`

Authorized `ANALYST`:

- `VIEW`
- `REQUEST_CORRECTION`
- `VERIFY` only when `canVerifyReports = true`

No `EDIT`.

#### `NEEDS_CORRECTION`

Authorized `ADMIN` / `DATA_OPERATOR`:

- `VIEW`
- `EDIT`
- `SUBMIT_FOR_REVIEW`

Authorized `ANALYST`:

- `VIEW`

#### `VERIFIED`

Authorized `ADMIN`:

- `VIEW`
- `REQUEST_CORRECTION`
- `ARCHIVE`

Authorized `ANALYST`:

- `VIEW`
- `REQUEST_CORRECTION`

Other authorized read roles:

- `VIEW`

#### `ARCHIVED`

Authorized readers:

- `VIEW` only.

Do not expose actions the backend would reject.

### Read authorization and report visibility

Backend authorization is the source of truth.

Use the existing role and team-scope infrastructure.

Report visibility:

- `ADMIN` may read reports for all teams and all statuses;
- `DATA_OPERATOR` may read all report statuses for accessible teams;
- `ANALYST` may read all report statuses for accessible teams;
- `COACH`, `MEDICAL_STAFF`, and `VIEWER` may read only `VERIFIED` or `ARCHIVED` reports for accessible teams;
- all reads remain team-scope restricted;
- inaccessible resources must use the existing safe non-disclosure behavior.

A caller must not learn that a draft/review/correction report exists for a team or status they are not allowed to view.

### API shape

Add focused endpoints following existing Minimal API and vertical-slice conventions.

Required operations:

```txt
GET  /api/matches/{matchId}/report
POST /api/matches/{matchId}/report

POST /api/match-reports/{reportId}/submit
POST /api/match-reports/{reportId}/verify
POST /api/match-reports/{reportId}/request-correction
POST /api/match-reports/{reportId}/archive
```

Add a report list query for workflow and future dashboard/UI needs:

```txt
GET /api/match-reports
```

Do not add a generic `PATCH status` endpoint.

Each transition must use an explicit action endpoint/use case.

Do not add a report delete endpoint.

### Report list query

`GET /api/match-reports` must support server-side filtering and pagination.

Support at minimum:

- `seasonId`;
- `teamId`;
- `competitionId`;
- `status`;
- `dateFrom`;
- `dateTo`;
- page;
- page size.

Rules:

- results are always restricted by team scope;
- status visibility rules are applied server-side;
- explicit unauthorized `teamId` filtering returns `403`;
- use deterministic sorting;
- do not load all reports into memory for API-layer filtering.

Return a compact read model including at minimum:

- report ID;
- match ID;
- match kickoff date/time;
- team summary;
- opponent summary;
- competition summary;
- report status;
- `allowedActions`;
- submission metadata when present;
- verification metadata when present;
- latest correction-request metadata when present.

Do not include statistics, lineup collections, GPS, media, or audit-log history in list results.

### Report detail read model

`GET /api/matches/{matchId}/report` returns the workflow metadata for the caller when visible.

Include at minimum:

- report ID;
- match ID;
- report status;
- `allowedActions`;
- creation metadata;
- latest submission metadata;
- verification metadata;
- latest correction-request metadata;
- archive state/metadata.

Do not include placeholder collections for future statistics, GPS, media, or audit records.

If no report exists, return the project's established missing-resource response.

### Workflow-aware editing locks

Once Unit 32 exists, report workflow status must protect related match data from casual overwrite.

Update the existing match metadata mutation and lineup/appearance save behavior so that:

#### No report exists

- existing Unit 30 and Unit 31 mutation rules apply.

#### `DRAFT`

- authorized match metadata edits remain allowed;
- authorized lineup/appearance edits remain allowed.

#### `NEEDS_CORRECTION`

- authorized match metadata edits remain allowed;
- authorized lineup/appearance edits remain allowed.

#### `READY_FOR_REVIEW`

- match metadata edits are locked;
- lineup/appearance edits are locked.

#### `VERIFIED`

- match metadata edits are locked;
- lineup/appearance edits are locked.

To change verified data:

1. an authorized reviewer requests correction;
2. report moves to `NEEDS_CORRECTION`;
3. authorized data-entry mutations become available again.

#### `ARCHIVED`

- match metadata edits are locked;
- lineup/appearance edits are locked.

Locked mutations should return `409 Conflict` with a safe workflow-specific ProblemDetails response.

Do not bypass these locks for admins through the normal mutation endpoints.

### Match archive coordination

Update match archive behavior introduced in Unit 30.

Rules:

- a match with no report may be archived using existing admin rules;
- a match with a report in `DRAFT`, `READY_FOR_REVIEW`, `NEEDS_CORRECTION`, or `VERIFIED` must not be archived;
- a match whose report is `ARCHIVED` may be archived by an admin;
- restoring a match does not restore or change the report status.

This prevents the match record from being archived while an active report workflow still exists.

### Readiness checks before submission

Unit 32 introduces only workflow-level readiness checks.

Before `DRAFT` or `NEEDS_CORRECTION` may transition to `READY_FOR_REVIEW`:

- the linked match exists;
- match status is `PLAYED`;
- match is not archived;
- at least one `PlayerMatchAppearance` exists;
- lineup/appearance persistence is internally valid according to Unit 31.

Do not require statistics yet.

Unit 33 must extend submission/readiness validation with tracking-level-aware statistics rules where appropriate without bypassing the Unit 32 transition service.

### Workflow metadata and audit readiness

Store current workflow metadata needed for operational use:

- creator;
- latest submitter and submission time;
- verifier and verification time;
- latest correction requester, time, and reason;
- archive metadata.

Do not implement a general audit table or audit history API in this unit.

Keep transitions explicit and centralized so Unit 38 can attach persistent audit logging without rewriting workflow rules.

If an audit abstraction already exists in the implementation by the time this unit is built, use it according to the existing architecture. Do not create speculative audit infrastructure solely for this unit.

### Validation and error handling

Use FluentValidation where appropriate.

Use existing Result and ProblemDetails conventions.

Expected behavior:

- `400` for malformed input;
- `401` for unauthenticated requests;
- `403` for explicit authorization failures where disclosure is appropriate, such as an out-of-scope team mutation request;
- `404` for missing or safely inaccessible report/match resources according to existing conventions;
- `409` for invalid workflow transitions, workflow-locked mutations, duplicate report creation, or match archive conflicts;
- `422` for semantic validation errors if that is the established convention.

Do not expose internal authorization, database, or workflow implementation details.

### Clean Architecture boundaries

Domain owns:

- `MatchReport`;
- report status enum;
- transition invariants;
- correction reason invariant where domain-owned;
- archive lifecycle transition;
- report-level workflow state.

Application owns:

- create report use case;
- list/detail queries;
- submit for review;
- verify;
- request correction;
- archive;
- allowed-action calculation;
- team-scope and role/permission orchestration;
- submission readiness checks;
- workflow-aware mutation guards for match metadata and lineup/appearance operations.

Infrastructure owns:

- EF Core mapping;
- persistence;
- query implementation;
- migration.

API owns:

- route mapping;
- HTTP input/output mapping;
- policy application where appropriate;
- ProblemDetails-compatible result mapping.

Do not place workflow rules or EF Core queries in endpoint handlers.

### EF Core persistence and migration

Add `MatchReport` persistence to the existing `AppDbContext`.

Create the migration using the approved `dotnet ef` workflow.

Keep migration files under:

```txt
backend/src/Infrastructure/Persistence/Migrations/
```

Use:

```txt
--output-dir Persistence/Migrations
```

Persistence requirements:

- unique constraint/index enforcing one report per `MatchId`;
- foreign key to `Match`;
- no cascade behavior that could silently delete report history;
- practical indexes for status and workflow list queries;
- actor IDs stored using existing user identifier conventions;
- correction reason bounded consistently with validation.

Do not handwrite migration files unless the CLI workflow is genuinely blocked; document any exception in `context/progress-tracker.md`.

### Tests

Add focused tests for the workflow introduced by this unit.

Domain/unit tests should cover at minimum:

- new report starts in `DRAFT`;
- allowed transitions;
- invalid transitions;
- correction reason requirement;
- verify metadata behavior;
- archive transition;
- archived report remains terminal in this unit.

Application/integration tests should cover at minimum:

- report can be created only for a played, non-archived, accessible match;
- one report per match;
- unauthorized roles cannot create reports;
- `DATA_OPERATOR` scope enforcement;
- submit requires at least one appearance;
- `ADMIN` can verify;
- `ANALYST` without `canVerifyReports` cannot verify;
- `ANALYST` with `canVerifyReports` can verify inside scope;
- analyst may request correction without verify permission;
- correction reason validation;
- `COACH`, `MEDICAL_STAFF`, and `VIEWER` cannot read unverified reports;
- verified/archived report visibility for authorized read roles;
- `allowedActions` matches actual backend permissions;
- invalid transitions return `409`;
- `READY_FOR_REVIEW`, `VERIFIED`, and `ARCHIVED` lock match metadata mutations;
- `READY_FOR_REVIEW`, `VERIFIED`, and `ARCHIVED` lock lineup/appearance mutations;
- `NEEDS_CORRECTION` re-enables authorized edits;
- match cannot be archived while a non-archived report workflow exists;
- archived report permits later admin match archive;
- report list filtering remains team-scope aware;
- explicit unauthorized team filter returns `403`;
- migration/database behavior works in the existing integration-test environment.

Do not add frontend tests in this unit.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

If implementation changes documented workflow statuses, transition rules, authorization behavior, allowed actions, or architecture boundaries, update the relevant context file before continuing.

Do not silently add restore behavior for `ARCHIVED` reports. If restore becomes required, define the target transition and audit implications in project context first.

## Implementation

### 1. Add the match report domain model

Create:

- `MatchReport`;
- report status enum;
- explicit transition methods;
- correction-request behavior;
- archive behavior;
- workflow metadata updates.

Keep one report per match.

### 2. Add persistence mapping

Configure:

- table mapping;
- one-to-one/unique `MatchId` relationship;
- actor/timestamp metadata;
- correction-reason length;
- indexes for status/workflow queries;
- restrictive historical delete behavior.

Generate the migration with `dotnet ef`.

### 3. Add report creation and read use cases

Implement:

- create draft report;
- get report by match;
- list reports with server-side filtering and team-scope/status visibility rules.

Return backend-calculated `allowedActions`.

### 4. Add explicit workflow transition use cases

Implement focused commands/use cases for:

- submit for review;
- verify;
- request correction;
- archive.

Do not add a generic status mutation command.

### 5. Add allowed-action calculation

Create a centralized backend calculation for report actions.

Ensure API responses never advertise actions that the corresponding mutation endpoint would reject for the same caller and report state.

### 6. Add workflow-aware mutation guards

Integrate report status checks into:

- Unit 30 match metadata update behavior;
- Unit 31 lineup/appearance save behavior;
- Unit 30 match archive behavior.

Use shared Application-layer workflow guard logic rather than duplicating status checks across endpoint handlers.

### 7. Add Minimal API endpoints

Map the required report endpoints.

Keep endpoint handlers thin and delegate workflow, authorization, and persistence to Application use cases.

### 8. Add tests

Add domain, application, and integration coverage for transitions, permissions, team scope, visibility, allowed actions, editing locks, archive coordination, and persistence.

### 9. Update progress documentation

Update `context/progress-tracker.md` with the actual implementation and verification state.

Do not mark Unit 32 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use the existing backend packages, authentication/authorization infrastructure, EF Core persistence, FluentValidation, Result/ProblemDetails conventions, and test infrastructure.

Do not add new NuGet or npm packages unless an existing approved dependency is genuinely missing and the need is documented before installation.

## Verification checklist

- [ ] `MatchReport` exists as a persistent entity with one-report-per-match enforcement.
- [ ] Report statuses are `DRAFT`, `READY_FOR_REVIEW`, `VERIFIED`, `NEEDS_CORRECTION`, and `ARCHIVED`.
- [ ] New reports start in `DRAFT`.
- [ ] Reports can be created only for played, non-archived matches.
- [ ] Only `ADMIN` and in-scope `DATA_OPERATOR` users can create reports.
- [ ] Duplicate report creation returns `409 Conflict`.
- [ ] Status changes use explicit workflow transitions rather than generic status assignment.
- [ ] `DRAFT -> READY_FOR_REVIEW` works.
- [ ] `READY_FOR_REVIEW -> VERIFIED` works for authorized verifiers.
- [ ] `READY_FOR_REVIEW -> NEEDS_CORRECTION` works.
- [ ] `NEEDS_CORRECTION -> READY_FOR_REVIEW` works.
- [ ] `VERIFIED -> NEEDS_CORRECTION` works.
- [ ] `VERIFIED -> ARCHIVED` is admin-only.
- [ ] `ARCHIVED` is terminal in Unit 32.
- [ ] Invalid transitions return `409 Conflict`.
- [ ] Submission requires at least one persisted `PlayerMatchAppearance`.
- [ ] Statistics are not required yet for submission in Unit 32.
- [ ] Analyst verification requires `canVerifyReports`.
- [ ] Analyst correction requests do not require `canVerifyReports`.
- [ ] Correction requests require a valid non-empty reason.
- [ ] Workflow metadata records creator, latest submitter, verifier, latest correction requester, and relevant timestamps.
- [ ] Every report read model exposes backend-calculated `allowedActions`.
- [ ] `allowedActions` match actual backend mutation authorization.
- [ ] Team-scope restrictions are enforced server-side for report list/detail access.
- [ ] `COACH`, `MEDICAL_STAFF`, and `VIEWER` cannot read unverified reports.
- [ ] Authorized read roles can read `VERIFIED` and `ARCHIVED` reports for accessible teams.
- [ ] Explicit unauthorized team filtering returns `403`.
- [ ] `READY_FOR_REVIEW` locks match metadata edits.
- [ ] `READY_FOR_REVIEW` locks lineup/appearance edits.
- [ ] `VERIFIED` locks match metadata and lineup/appearance edits.
- [ ] `ARCHIVED` locks match metadata and lineup/appearance edits.
- [ ] `NEEDS_CORRECTION` re-enables authorized correction edits.
- [ ] A match with an active non-archived report workflow cannot be archived.
- [ ] A match whose report is `ARCHIVED` may be archived by an admin.
- [ ] No report hard-delete endpoint exists.
- [ ] No statistics, GPS, media, import, frontend, or general audit-log implementation is added.
- [ ] Endpoint handlers remain thin and contain no EF Core queries or workflow rules.
- [ ] Migration files are generated under `backend/src/Infrastructure/Persistence/Migrations/`.
- [ ] Relevant domain/unit tests pass.
- [ ] Relevant integration tests pass.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for the affected backend solution/projects.
- [ ] `dotnet test` passes for the relevant backend test projects.
- [ ] `dotnet ef database update` succeeds when the local database environment is available.
- [ ] No frontend implementation files are changed.
- [ ] `context/progress-tracker.md` reflects the actual Unit 32 implementation and verification state.
