# Unit 33: Manual Match Statistics Backend

## Goal

Build the backend foundation for manual player and goalkeeper match statistics linked to concrete `PlayerMatchAppearance` records and governed by the selected team’s tracking level. Extend the Unit 32 match-report workflow with statistics completeness checks while keeping statistics editable only in `DRAFT` and `NEEDS_CORRECTION` states.

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
9. `context/feature-specs/22-teams-selections-backend.md`
10. `context/feature-specs/31-match-lineup-appearance-backend.md`
11. `context/feature-specs/32-match-report-workflow-backend.md`
12. `context/feature-specs/33-manual-match-statistics-backend.md`

Use relevant project-local skills from `.agents/skills/` when applicable. Skills may guide implementation workflow but must not override project context, architecture rules, code standards, or this spec.

This unit is backend-only.

Do not add or change frontend routes, pages, components, navigation, shadcn/ui components, or frontend API wrappers in this unit.

### Scope

This unit introduces manual structured statistics for match appearances.

It owns:

- `PlayerMatchStats` linked to one concrete `PlayerMatchAppearance`;
- `GoalkeeperMatchStats` linked to one concrete `PlayerMatchAppearance`;
- a centralized V1 tracking-level field matrix;
- tracking-level-aware read and write contracts;
- validation of statistical relationships;
- report-workflow-aware statistics edit locking;
- report submission completeness checks;
- persistence and tests.

This unit does not introduce:

- team match statistics;
- event-by-event football actions or an event timeline;
- GPS/physical metrics;
- CSV/XLSX imports;
- Gpexe-specific fields;
- Zone14-specific fields;
- media;
- audit-log persistence;
- frontend statistics entry UI;
- player positions;
- goalkeeper position assignments;
- custom per-team metric configuration beyond the existing `BASIC`, `STANDARD`, and `FULL` tracking levels;
- speculative metrics not already confirmed in project context.

### Statistics ownership and identity

Statistics belong to the match report and must be attached to concrete appearances.

Add persistent records equivalent to:

```txt
PlayerMatchStats
- Id
- MatchReportId
- PlayerMatchAppearanceId
- metric fields
- CreatedAtUtc
- UpdatedAtUtc

GoalkeeperMatchStats
- Id
- MatchReportId
- PlayerMatchAppearanceId
- goalkeeper metric fields
- CreatedAtUtc
- UpdatedAtUtc
```

Rules:

- a `PlayerMatchStats` record belongs to exactly one report and one appearance;
- an appearance may have at most one `PlayerMatchStats` record;
- a `GoalkeeperMatchStats` record belongs to exactly one report and one appearance;
- an appearance may have at most one `GoalkeeperMatchStats` record;
- the appearance must belong to the same match as the report;
- statistics cannot be attached directly to a player without an appearance;
- unused substitutes without a `PlayerMatchAppearance` cannot receive match statistics;
- a goalkeeper appearance may also have ordinary player statistics;
- multiple goalkeeper-stat records may exist in one match when more than one goalkeeper appearance needs to be recorded;
- do not introduce player-position modeling merely to identify goalkeeper rows.

The presence of a `GoalkeeperMatchStats` record is the explicit manual indication that the appearance has goalkeeper-specific statistics for this match.

### Confirmed V1 player-statistics catalog

Implement only the confirmed manual player statistics already documented by the project:

```txt
Goals
Assists
YellowCards
RedCards
Shots
ShotsOnTarget
PassesAttempted
PassesCompleted
KeyPasses
DuelsAttempted
DuelsWon
FoulsCommitted
FoulsWon
Offsides
BallRecoveries
PossessionLosses
```

Use stable English code/property names.

Do not add “similar metrics” beyond this explicit list in Unit 33.

Statistics values are nullable while a report is being edited so the system can distinguish:

- not entered yet;
- deliberately entered as zero.

A value of `0` is valid and must not be treated as missing.

### Confirmed V1 goalkeeper-statistics catalog

Implement only the unambiguous goalkeeper statistics confirmed by project context:

```txt
Saves
GoalsConceded
CleanSheet
PenaltySaves
```

Use:

- nullable non-negative integers for `Saves`, `GoalsConceded`, and `PenaltySaves` while editing;
- nullable boolean for `CleanSheet` while editing.

Do not invent separate `Punches`, `Claims`, or combined `PunchesClaims` fields in this unit because the project context only says punches/claims “if tracked” and does not yet define the exact data shape.

Adding those fields later requires a confirmed product/data decision and the relevant context/spec update.

### Canonical V1 tracking-level field matrix

Unit 22 intentionally deferred exact tracking-level field gating until the statistics module existed. Unit 33 now defines the V1 matrix.

Centralize this matrix in one backend statistics-profile component/service or equivalent domain/application configuration. Do not duplicate field lists across endpoint handlers, validators, or query handlers.

#### `BASIC`

Enabled player fields:

```txt
Goals
Assists
YellowCards
RedCards
```

Enabled goalkeeper fields:

```txt
Saves
GoalsConceded
CleanSheet
```

#### `STANDARD`

Enabled player fields:

```txt
Goals
Assists
YellowCards
RedCards
Shots
ShotsOnTarget
FoulsCommitted
FoulsWon
Offsides
```

Enabled goalkeeper fields:

```txt
Saves
GoalsConceded
CleanSheet
PenaltySaves
```

#### `FULL`

Enabled player fields:

```txt
Goals
Assists
YellowCards
RedCards
Shots
ShotsOnTarget
PassesAttempted
PassesCompleted
KeyPasses
DuelsAttempted
DuelsWon
FoulsCommitted
FoulsWon
Offsides
BallRecoveries
PossessionLosses
```

Enabled goalkeeper fields:

```txt
Saves
GoalsConceded
CleanSheet
PenaltySaves
```

The goalkeeper field set is intentionally the same for `STANDARD` and `FULL` in Unit 33. Do not invent additional goalkeeper metrics solely to make every level numerically different.

### Tracking-level source of truth

The applicable tracking level comes from the `Team` referenced by the report’s match.

Rules:

- do not store a second mutable tracking-level value on each statistics row;
- reads and writes resolve the current match team’s configured tracking level;
- the statistics response must expose the resolved tracking level and enabled field lists so future UI can render the correct inputs;
- disabled fields must not be accepted with non-null values;
- disabled fields must not be required for report submission.

Because changing a team’s tracking level after historical reports exist could change the interpretation of old report completeness, freeze the applied tracking level on the `MatchReport` when the first statistics snapshot is persisted or, if no statistics have yet been saved, when the report first attempts submission.

Add an immutable report-level field equivalent to:

```txt
AppliedTrackingLevel
```

Rules:

- it is initially unset on a newly created Unit 32 report;
- the first successful statistics save sets it from the match team’s current `TrackingLevel`;
- if submission is attempted before any statistics save, submission readiness sets it before validating required statistics;
- once set, it never changes for that report;
- all later statistics reads, writes, and completeness checks use `AppliedTrackingLevel`, not a later team configuration change.

This preserves historical report semantics without duplicating the entire metric matrix in the database.

### Statistics read API

Add:

```txt
GET /api/match-reports/{reportId}/statistics
```

The response must include at minimum:

- report ID;
- match ID;
- report status;
- resolved/applied tracking level;
- enabled player field codes;
- enabled goalkeeper field codes;
- appearance summaries needed to associate rows with players;
- one player-statistics view per current appearance;
- zero or more goalkeeper-statistics views;
- whether each row is complete for the applied tracking level;
- overall statistics completeness state;
- whether the current caller may edit statistics.

For appearances without a persisted `PlayerMatchStats` row, the read model may synthesize an empty editable row with all enabled values `null` rather than requiring eager persistence of blank database rows.

Do not include GPS, media, imports, audit history, or event timelines.

### Statistics save API

Add one atomic statistics-save operation:

```txt
PUT /api/match-reports/{reportId}/statistics
```

Use a snapshot-oriented request suitable for the future desktop/tablet statistics grid.

The request must contain:

- `PlayerStatistics`: exactly one row for every current `PlayerMatchAppearance` in the report’s match;
- `GoalkeeperStatistics`: zero or more rows for appearances that have goalkeeper-specific statistics.

Each row identifies the target by `PlayerMatchAppearanceId`.

Rules:

- player-statistics rows must contain each current appearance exactly once;
- unknown, duplicate, foreign-match, or omitted appearance IDs fail the whole save;
- goalkeeper rows must reference current appearances from the same match;
- duplicate goalkeeper rows for the same appearance fail the whole save;
- the save is atomic;
- partial database updates must not remain after validation failure;
- values may remain `null` while the report is editable;
- zero values are preserved as entered;
- disabled fields must be `null` or omitted according to the chosen request DTO shape;
- a non-null value for a field disabled by the applied tracking level fails validation;
- the save upserts one player-statistics record per appearance;
- goalkeeper rows represent the complete current goalkeeper-statistics set for the report, so omitting a previously persisted goalkeeper row removes that goalkeeper-specific record while the report is editable.

Do not add many per-cell mutation endpoints.

### Workflow-aware edit authorization

Statistics are report-owned editable data.

Reuse the Unit 32 workflow and authorization model.

Statistics may be changed only when:

- report status is `DRAFT` or `NEEDS_CORRECTION`;
- caller is `ADMIN` or `DATA_OPERATOR`;
- caller has access to the match team.

Statistics are read-only when report status is:

- `READY_FOR_REVIEW`;
- `VERIFIED`;
- `ARCHIVED`.

A save attempt in a locked workflow state returns `409 Conflict` using the existing workflow-lock ProblemDetails convention.

Do not add an admin bypass through the normal statistics save endpoint.

Read visibility follows Unit 32 report visibility rules.

### Player-statistics validation

For every non-null numeric player-stat value:

- value must be an integer;
- value must be non-negative.

Add only relationship rules that are unambiguous:

```txt
ShotsOnTarget <= Shots
PassesCompleted <= PassesAttempted
DuelsWon <= DuelsAttempted
```

Apply a relationship rule only when both related fields are enabled for the applied tracking level and both values are non-null.

Do not enforce speculative relationships such as:

- goals must be less than or equal to shots on target;
- assists must be less than or equal to key passes;
- cards must have competition-specific hard maximums.

Those assumptions are not required by current project context and may vary by data-entry convention.

### Goalkeeper-statistics validation

For every non-null numeric goalkeeper value:

- value must be an integer;
- value must be non-negative.

`CleanSheet` is manually recorded as a boolean.

Do not derive `CleanSheet` automatically from `GoalsConceded` because multiple goalkeeper appearances may exist in one match and individual clean-sheet conventions can differ.

Do not require one goalkeeper-statistics row per appearance.

### Report submission completeness

Extend the Unit 32 submit-for-review readiness checks.

Before a report may transition from `DRAFT` or `NEEDS_CORRECTION` to `READY_FOR_REVIEW`:

1. existing Unit 32 readiness checks still pass;
2. `AppliedTrackingLevel` is set;
3. every current `PlayerMatchAppearance` has one persisted `PlayerMatchStats` record;
4. every player field enabled by `AppliedTrackingLevel` is non-null for every appearance;
5. at least one `GoalkeeperMatchStats` record exists for the match;
6. every enabled goalkeeper field is non-null on every persisted goalkeeper-statistics row;
7. all cross-field validation rules pass;
8. no disabled field contains a persisted non-null value.

If completeness fails, submission must fail with a safe semantic validation response that identifies which appearance/field requirements remain incomplete without exposing internal persistence details.

Do not require statistics fields disabled by the applied tracking level.

Do not require team statistics, GPS, media, or imports for submission in Unit 33.

### Interaction with lineup/appearance changes

Statistics remain linked to concrete appearance IDs.

While a report is editable (`DRAFT` or `NEEDS_CORRECTION`), Unit 31 lineup/appearance updates may still add or remove appearances.

Required behavior:

- unchanged appearances retain their stable statistics records;
- newly created appearances begin with no persisted player statistics until saved;
- if an appearance is removed through an authorized lineup save, associated player and goalkeeper statistics must be removed in the same successful transaction or through restrictive application cleanup before the appearance is deleted;
- do not leave orphan statistics rows;
- removing an appearance with statistics is allowed only while the report workflow already permits lineup editing;
- `READY_FOR_REVIEW`, `VERIFIED`, and `ARCHIVED` continue to block lineup changes through Unit 32.

Do not create replacement appearances merely to reset statistics.

### Report and match consistency

A statistics save must validate that:

- report exists and is visible to the caller;
- report belongs to the same match as every referenced appearance;
- match is not archived;
- match status remains `PLAYED`;
- appearance still exists at save time;
- report status remains editable at save time.

Perform these checks inside the transactional/use-case boundary to avoid stale authorization or workflow decisions.

### API and DTO design

Use explicit typed request/response contracts.

Do not expose EF Core entities directly.

The response should provide stable field codes suitable for future frontend mapping, for example camelCase or the project’s established JSON naming convention:

```txt
goals
assists
yellowCards
redCards
shots
shotsOnTarget
passesAttempted
passesCompleted
keyPasses
duelsAttempted
duelsWon
foulsCommitted
foulsWon
offsides
ballRecoveries
possessionLosses

saves
goalsConceded
cleanSheet
penaltySaves
```

Keep internal C# names in normal project conventions.

Do not return raw enum names as future user-facing localized labels; this backend unit returns stable codes/data only.

### Duplicate and concurrency behavior

Database constraints must prevent duplicate statistics rows for the same appearance.

Use unique constraints equivalent to:

```txt
PlayerMatchStats.PlayerMatchAppearanceId UNIQUE
GoalkeeperMatchStats.PlayerMatchAppearanceId UNIQUE
```

Also retain report foreign keys for report-owned querying and integrity.

Translate unique-constraint conflicts into safe application/API responses.

If the project already has a concurrency convention by Unit 33, follow it. Do not invent a broad new concurrency framework solely for this unit.

### Clean Architecture boundaries

Domain owns:

- statistics entities/value invariants where appropriate;
- non-negative value and field relationship rules that are domain-level;
- immutable applied tracking-level behavior on `MatchReport` if that aggregate owns it.

Application owns:

- tracking-level statistics profile/matrix;
- statistics read query;
- atomic statistics save use case;
- report/appearance consistency checks;
- role/team-scope/workflow authorization orchestration;
- submission completeness integration;
- transactional cleanup when editable lineup changes remove appearances.

Infrastructure owns:

- EF Core mapping;
- PostgreSQL persistence;
- query implementation;
- migration;
- database uniqueness and foreign-key constraints.

API owns:

- route mapping;
- HTTP request/response mapping;
- ProblemDetails-compatible result conversion.

Do not put statistics rules, workflow rules, or EF Core queries in endpoint handlers.

### EF Core persistence and migration

Add statistics persistence and `AppliedTrackingLevel` to the existing model.

Generate the migration through the approved `dotnet ef` workflow.

Keep migration files under:

```txt
backend/src/Infrastructure/Persistence/Migrations/
```

Use:

```txt
--output-dir Persistence/Migrations
```

Persistence requirements:

- foreign key from player statistics to match report;
- foreign key from player statistics to player appearance;
- foreign key from goalkeeper statistics to match report;
- foreign key from goalkeeper statistics to player appearance;
- unique appearance constraint for each statistics table;
- no orphan statistics;
- restrictive deletion from report/match history where appropriate;
- appearance removal cleanup only through the authorized editable workflow described above;
- nullable metric columns while editing;
- stable persistence of the report’s immutable applied tracking level.

Do not handwrite migration files unless the CLI workflow is genuinely blocked; document any exception in `context/progress-tracker.md`.

### Tests

Add focused tests for the behavior introduced by this unit.

Unit/domain tests should cover at minimum:

- tracking-level matrix contents for `BASIC`, `STANDARD`, and `FULL`;
- applied tracking level becomes immutable once set;
- non-negative metric validation;
- `ShotsOnTarget <= Shots`;
- `PassesCompleted <= PassesAttempted`;
- `DuelsWon <= DuelsAttempted`;
- disabled fields are rejected when populated;
- nullable values remain distinguishable from zero while editing.

Application/integration tests should cover at minimum:

- statistics read is report-visibility and team-scope aware;
- `ADMIN` can save statistics in an editable report;
- in-scope `DATA_OPERATOR` can save statistics in an editable report;
- out-of-scope `DATA_OPERATOR` cannot save statistics;
- other roles cannot save statistics;
- `READY_FOR_REVIEW`, `VERIFIED`, and `ARCHIVED` reject saves with `409`;
- `NEEDS_CORRECTION` allows authorized saves;
- first successful save freezes `AppliedTrackingLevel`;
- later team tracking-level changes do not change historical report field requirements;
- player snapshot requires exactly one row per current appearance;
- unknown, duplicate, omitted, or foreign-match appearance IDs fail atomically;
- goalkeeper snapshot supports zero or more rows while editing;
- duplicate goalkeeper appearance rows fail;
- disabled fields with non-null values fail;
- zero values persist correctly;
- submission fails when enabled player fields are incomplete;
- submission fails when any current appearance has no persisted player statistics row;
- submission fails when no goalkeeper-statistics row exists;
- submission fails when a goalkeeper row has incomplete enabled fields;
- submission succeeds when all Unit 32 and Unit 33 readiness requirements pass;
- lineup removal of an appearance in an editable report does not leave orphan statistics;
- unchanged appearance IDs retain their existing statistics;
- unique constraints prevent duplicate statistics rows;
- migration/database behavior works in the existing integration-test environment.

Do not add frontend tests in this unit.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

Because Unit 33 is the first unit that defines exact V1 tracking-level field gating, update the relevant context documentation if the implemented field matrix differs from this spec or if a new confirmed product decision changes it.

Do not silently add new statistics fields or vendor-specific metrics.

## Implementation

### 1. Add statistics entities and applied tracking level

Create:

- `PlayerMatchStats`;
- `GoalkeeperMatchStats`;
- immutable `AppliedTrackingLevel` behavior on the report.

Use concrete appearance relationships and existing ID/timestamp conventions.

### 2. Add centralized tracking-level statistics profile

Implement one canonical backend source for:

- enabled player fields per tracking level;
- enabled goalkeeper fields per tracking level.

Reuse it for:

- reads;
- validation;
- saves;
- submission completeness.

Do not duplicate the matrix across multiple slices.

### 3. Add persistence mapping

Configure statistics tables, foreign keys, unique constraints, nullable metric fields, and report applied tracking level.

Generate the EF Core migration using the approved workflow.

### 4. Add statistics read query

Implement:

```txt
GET /api/match-reports/{reportId}/statistics
```

Return:

- applied tracking profile;
- enabled field codes;
- appearance/player summaries;
- current values;
- row completeness;
- overall completeness;
- editability for the current caller.

### 5. Add atomic statistics save use case

Implement:

```txt
PUT /api/match-reports/{reportId}/statistics
```

Validate the complete appearance snapshot, goalkeeper subset, tracking-level field gating, workflow state, team scope, role, and cross-field rules before committing changes.

### 6. Extend report submission readiness

Integrate Unit 33 completeness checks into the existing Unit 32 submit-for-review use case.

Do not bypass or duplicate the Unit 32 transition service.

### 7. Integrate appearance-removal cleanup

Update the authorized editable lineup save path so removing an appearance cannot leave orphan player or goalkeeper statistics.

Preserve statistics for unchanged stable appearance IDs.

### 8. Add Minimal API endpoints

Map the statistics GET and PUT endpoints with thin handlers and existing ProblemDetails conventions.

### 9. Add tests

Add unit, application, and integration coverage for the tracking matrix, field gating, atomic save, workflow locks, authorization, completeness, appearance consistency, and persistence.

### 10. Update progress documentation

Update `context/progress-tracker.md` with the actual implementation and verification state.

Do not mark Unit 33 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use the existing backend packages, EF Core persistence, FluentValidation, authentication/authorization infrastructure, Result/ProblemDetails conventions, and test infrastructure.

Do not add new NuGet or npm packages unless an existing approved dependency is genuinely missing and the need is documented before installation.

## Verification checklist

- [ ] `PlayerMatchStats` exists and links to one `MatchReport` and one concrete `PlayerMatchAppearance`.
- [ ] `GoalkeeperMatchStats` exists and links to one `MatchReport` and one concrete `PlayerMatchAppearance`.
- [ ] One player-statistics row per appearance is enforced.
- [ ] One goalkeeper-statistics row per appearance is enforced.
- [ ] Goalkeeper statistics do not require introducing player-position modeling.
- [ ] Only the confirmed Unit 33 player-statistics fields are implemented.
- [ ] Only `Saves`, `GoalsConceded`, `CleanSheet`, and `PenaltySaves` are implemented for goalkeeper statistics.
- [ ] No speculative `Punches` or `Claims` fields are added.
- [ ] The centralized `BASIC`, `STANDARD`, and `FULL` field matrix matches this spec.
- [ ] `AppliedTrackingLevel` is frozen on the report and does not change when the team tracking level changes later.
- [ ] Disabled fields cannot be persisted with non-null values.
- [ ] Null and zero remain semantically distinct while editing.
- [ ] Numeric statistics reject negative values.
- [ ] `ShotsOnTarget <= Shots` is enforced when applicable.
- [ ] `PassesCompleted <= PassesAttempted` is enforced when applicable.
- [ ] `DuelsWon <= DuelsAttempted` is enforced when applicable.
- [ ] `GET /api/match-reports/{reportId}/statistics` returns tracking profile, enabled fields, appearance rows, values, completeness, and editability.
- [ ] `PUT /api/match-reports/{reportId}/statistics` saves statistics atomically.
- [ ] The player snapshot contains exactly one row per current appearance.
- [ ] Goalkeeper rows form the complete current goalkeeper-statistics subset.
- [ ] Unknown, duplicate, omitted, or foreign-match appearance references fail safely.
- [ ] Statistics can be edited only in `DRAFT` and `NEEDS_CORRECTION`.
- [ ] `READY_FOR_REVIEW`, `VERIFIED`, and `ARCHIVED` statistics saves return `409`.
- [ ] `ADMIN` and in-scope `DATA_OPERATOR` can edit statistics.
- [ ] Other roles cannot edit statistics.
- [ ] Report/team-scope read visibility follows Unit 32 rules.
- [ ] Submission requires complete enabled player statistics for every appearance.
- [ ] Submission requires at least one complete goalkeeper-statistics row.
- [ ] Disabled fields are not required for submission.
- [ ] Unit 32 readiness checks remain active.
- [ ] Team statistics, GPS, media, imports, audit-log persistence, and frontend statistics UI are not added.
- [ ] Removing an appearance in an editable workflow does not leave orphan statistics.
- [ ] Unchanged stable appearance IDs retain their statistics.
- [ ] Endpoint handlers remain thin and contain no EF Core queries, workflow rules, or tracking-matrix duplication.
- [ ] Migration files are generated under `backend/src/Infrastructure/Persistence/Migrations/`.
- [ ] Relevant unit/domain tests pass.
- [ ] Relevant application/integration tests pass.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for the affected backend solution/projects.
- [ ] `dotnet test` passes for the relevant backend test projects.
- [ ] `dotnet ef database update` succeeds when the local database environment is available.
- [ ] No frontend implementation files are changed.
- [ ] `context/progress-tracker.md` reflects the actual Unit 33 implementation and verification state.
