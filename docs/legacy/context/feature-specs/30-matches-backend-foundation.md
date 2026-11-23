# Unit 30: Matches Backend Foundation

## Goal

Build the backend foundation for FK Velež match records using the existing seasons, competitions, teams/selections, opponents, venues, staff roles, and team-scope authorization infrastructure. Provide secure, team-scope-aware match reads and authorized match metadata mutations without adding lineup, appearances, report workflow, statistics, media, imports, or frontend UI.

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

Use relevant project-local skills from `.agents/skills/` when applicable. Skills may guide implementation workflow but must not override project context, architecture rules, code standards, or this spec.

This unit is backend-only.

Do not add or change frontend routes, pages, components, navigation, shadcn/ui components, or frontend API wrappers in this unit.

### Scope

This unit introduces the persistent match record and match metadata APIs only.

A match belongs to one FK Velež team/selection and references the existing configuration records needed to describe the fixture:

- season;
- competition;
- FK Velež team/selection;
- opponent;
- optional venue;
- scheduled kickoff date/time;
- optional round/phase label;
- home, away, or neutral location context;
- match status;
- optional final score when the match has been played.

This unit does not introduce:

- lineup;
- starting XI;
- substitutes;
- captain;
- substitutions;
- player minutes;
- player appearances;
- match report;
- report review/verification status;
- player or goalkeeper statistics;
- team statistics;
- GPS data;
- media;
- imports;
- audit persistence;
- frontend match screens.

Those remain owned by later units in the build plan.

### Match identity and persistence model

Add a persistent `Match` domain entity.

Use the existing project ID conventions and shared domain primitives. Do not introduce a separate club or tenant identifier because the application remains single-club in V1.

The match record should include at minimum:

- `Id`;
- `SeasonId`;
- `CompetitionId`;
- `TeamId`;
- `OpponentId`;
- optional `VenueId`;
- `KickoffAtUtc`;
- optional `Round`;
- `LocationType`;
- `Status`;
- optional `TeamScore`;
- optional `OpponentScore`;
- archive metadata consistent with existing archive patterns;
- created/updated timestamps consistent with project conventions.

Do not add speculative fields such as:

- formation;
- referee;
- attendance;
- weather;
- broadcast channel;
- tactical notes;
- player notes;
- external provider IDs;
- Zone14 IDs;
- Gpexe IDs.

### Location model

Use a small explicit enum for the relationship of FK Velež to the fixture:

```txt
HOME
AWAY
NEUTRAL
```

Use an English internal enum/code name consistent with project conventions, for example `MatchLocationType`.

`VenueId` is optional because a match may be created before the exact venue is known.

Do not infer `HOME` or `AWAY` from the selected venue. Location context and venue reference are separate pieces of match metadata.

### Match status model

Use a small explicit V1 status enum:

```txt
SCHEDULED
PLAYED
POSTPONED
CANCELLED
```

Use English internal enum names. User-facing localization belongs to the frontend later.

New matches default to `SCHEDULED`.

Status changes must go through explicit domain/application logic. Do not directly assign the status enum in endpoint handlers or persistence code.

Allowed V1 transitions:

```txt
SCHEDULED -> PLAYED
SCHEDULED -> POSTPONED
SCHEDULED -> CANCELLED

POSTPONED -> SCHEDULED
POSTPONED -> CANCELLED
```

Within this unit:

- `PLAYED` is terminal as a status transition;
- `CANCELLED` is terminal as a status transition;
- updating other editable metadata on an existing `PLAYED` or `CANCELLED` match is allowed when authorization and validation pass;
- correcting the final score of an already `PLAYED` match is allowed through the approved match update use case without changing the status;
- reopening a `PLAYED` or `CANCELLED` match into another status is out of scope.

Invalid transitions must fail with a clear conflict result and map to `409 Conflict`.

### Score rules

Use score fields from the FK Velež perspective:

- `TeamScore`;
- `OpponentScore`.

Do not store `HomeScore` and `AwayScore`, because the match already stores explicit home/away/neutral context and FK Velež remains the primary team in the single-club system.

Rules:

- scores are non-negative integers;
- both score values must either be present together or absent together;
- `PLAYED` requires both scores;
- `SCHEDULED`, `POSTPONED`, and `CANCELLED` must not contain final scores;
- changing a match from `PLAYED` is not allowed in this unit;
- correcting `TeamScore` and `OpponentScore` while the match remains `PLAYED` is allowed.

Do not add extra-time score, penalty-shootout score, half-time score, aggregate score, or result-winner fields in this unit.

### Reference integrity

A match references existing records from prior units.

On create:

- `SeasonId` must reference an existing non-archived season;
- `CompetitionId` must reference an existing non-archived competition;
- `TeamId` must reference an existing active FK Velež selection;
- `OpponentId` must reference an existing non-archived opponent;
- `VenueId`, when provided, must reference an existing non-archived venue.

On update:

- referenced replacement values must satisfy the same existence/archive rules;
- historical matches remain valid if a referenced setting is archived after the match already exists;
- archiving a season, competition, opponent, venue, or team must not cascade-delete historical matches.

Validate that `KickoffAtUtc` falls within the selected season's configured date range.

Do not duplicate season, competition, team, opponent, or venue names into the match table as denormalized source-of-truth fields.

### Team selection immutability

`TeamId` is part of the match identity and must be immutable after creation in this unit.

The generic update use case must not change the FK Velež selection attached to an existing match.

This avoids silently moving future lineup, appearance, report, statistics, and audit data between selections.

If a match is created for the wrong selection, the safe V1 correction path is to archive the incorrect match and create the correct match. A dedicated privileged reassignment workflow may be considered later only if a real requirement appears.

### Archive behavior

Do not hard-delete matches.

Use archive/restore behavior consistent with existing project patterns.

Rules:

- archive and restore are explicit operations;
- archived matches are excluded from default list queries;
- archived records remain persisted for history and future auditability;
- restoring a match preserves its metadata and match status;
- archiving a match must not delete referenced or future child data;
- archive and restore are administrator-only actions.

Do not use the `CANCELLED` match status as a substitute for archive. Cancellation describes the football fixture; archive describes application record lifecycle.

### Authorization model

Backend authorization is the source of truth.

Use the current staff role and team-scope infrastructure from Units 20 and 23.

Read behavior:

- authenticated active staff may read matches only for teams/selections within their authorized scope;
- `ADMIN` users may read matches across all teams;
- `ALL_TEAMS` scope may read matches across all teams;
- `SELECTED_TEAMS` scope may read only matches whose `TeamId` is inside the user's assigned team scope;
- list queries must apply team scope server-side;
- detail requests for a match outside the caller's accessible scope must use the existing safe inaccessible-resource behavior and must not leak restricted match metadata.

Mutation behavior:

- `ADMIN` may create and update matches for any team;
- `DATA_OPERATOR` may create and update matches only for teams inside their authorized scope;
- other roles do not receive match create/update permission in this unit;
- archive and restore are `ADMIN` only;
- every mutation must re-check authorization server-side.

When a create request explicitly targets a team outside the caller's scope, return `403`.

When an update request targets an existing match outside the caller's accessible scope, follow the existing safe inaccessible-resource convention.

Do not rely on future frontend action hiding for authorization.

### API shape

Add a focused Matches endpoint group following existing Minimal API and vertical-slice conventions.

Required operations:

```txt
GET    /api/matches
GET    /api/matches/{matchId}
POST   /api/matches
PATCH  /api/matches/{matchId}
POST   /api/matches/{matchId}/archive
POST   /api/matches/{matchId}/restore
```

The exact internal command/query type names may follow existing project conventions.

Do not put EF Core queries, business rules, status transitions, or authorization decisions directly in endpoint handlers.

### Match list query

`GET /api/matches` must support server-side filtering and pagination.

Support at minimum:

- `seasonId`;
- `teamId`;
- `competitionId`;
- `opponentId`;
- `status`;
- `dateFrom`;
- `dateTo`;
- archive visibility according to the established project pattern;
- page;
- page size.

Rules:

- default list behavior excludes archived matches;
- list results are always restricted to the caller's team scope;
- if a caller explicitly supplies a `teamId` they are not authorized to access, return `403` rather than silently querying a different scope;
- use deterministic sorting, with newest/upcoming date behavior implemented consistently and documented in the query handler;
- keep pagination bounded using the project's existing pagination conventions;
- do not load all matches into memory and filter them in the API layer.

Return a read model sufficient for the future Unit 34 Matches UI, including at minimum:

- match ID;
- kickoff date/time;
- season summary;
- competition summary;
- team summary;
- opponent summary;
- optional venue summary;
- round;
- location type;
- status;
- optional score;
- archive state.

Do not include lineup, report, statistics, GPS, or media data.

### Match detail query

`GET /api/matches/{matchId}` returns the match metadata required by future detail screens.

Include:

- identifiers and display summaries for referenced settings;
- kickoff date/time;
- round;
- location type;
- status;
- score when present;
- archive state;
- timestamps consistent with existing API conventions.

Do not include placeholder child collections for future lineup, reports, statistics, GPS, media, or audit data.

### Create match use case

Create requires:

- season;
- competition;
- team/selection;
- opponent;
- optional venue;
- kickoff date/time;
- optional round;
- location type.

New matches must start as `SCHEDULED` with no final score.

Do not allow clients to create a match directly as `PLAYED`, `POSTPONED`, or `CANCELLED`.

Validate:

- required identifiers;
- referenced records;
- team scope;
- mutation role;
- season date range;
- round length/normalization where applicable;
- kickoff timestamp validity.

Return the created match read model or identifier according to existing API conventions.

### Update match use case

The update operation may change:

- season;
- competition;
- opponent;
- optional venue;
- kickoff date/time;
- optional round;
- location type;
- status according to the allowed transition rules;
- final score according to the score rules.

The update operation must not change:

- `Id`;
- `TeamId`;
- created timestamp.

Use domain/application methods for status and score changes. Do not directly map request values onto entity properties.

When updating a `PLAYED` match:

- status must remain `PLAYED`;
- score corrections are allowed;
- other editable metadata corrections are allowed;
- changing the match to another status is rejected.

When updating a `CANCELLED` match:

- status must remain `CANCELLED`;
- final scores remain absent;
- other editable metadata corrections are allowed;
- reopening the status is rejected.

When changing `POSTPONED` back to `SCHEDULED`, the request must provide the current intended kickoff date/time.

### Duplicate protection

Do not introduce an overly broad uniqueness constraint that would reject legitimate fixtures such as multiple matches against the same opponent in one competition or season.

Instead, add an application-level duplicate conflict rule only for an exact duplicate fixture identity where all of these match:

- same `TeamId`;
- same `OpponentId`;
- same `KickoffAtUtc`;
- same non-archived record state.

An exact duplicate create should return `409 Conflict`.

Do not treat different kickoff times, rounds, competitions, or seasons as duplicates.

### Validation and error handling

Use FluentValidation for request/use-case validation where appropriate.

Use the existing Result/ProblemDetails conventions.

Expected behavior:

- `400` for malformed request input;
- `401` for unauthenticated requests;
- `403` for authenticated callers without required mutation or explicit team access;
- `404` for missing or safely inaccessible match detail resources according to existing conventions;
- `409` for invalid status transitions, exact duplicate fixture conflicts, or other domain conflicts;
- `422` for semantic validation errors if that is the established API convention.

Errors must be safe to display and must not expose internal database or authorization details.

### Clean Architecture boundaries

Keep responsibilities aligned with the existing architecture.

Domain owns:

- match entity;
- match location enum;
- match status enum;
- status transition rules;
- score invariants;
- archive/lifecycle domain behavior where domain-owned by existing conventions.

Application owns:

- create/update/archive/restore use cases;
- list/detail queries;
- validation;
- team-scope-aware authorization orchestration;
- duplicate checks;
- reference validation abstractions/use;
- transaction boundaries.

Infrastructure owns:

- EF Core configuration;
- PostgreSQL persistence;
- query implementations;
- migration.

API owns:

- endpoint mapping;
- HTTP input/output mapping;
- authorization policy application where appropriate;
- ProblemDetails-compatible response mapping.

Do not place business rules or EF Core queries in endpoint handlers.

### EF Core persistence and migration

Add the match persistence model to the existing `AppDbContext`.

Create a migration through the approved `dotnet ef` workflow.

Keep migration files under:

```txt
backend/src/Infrastructure/Persistence/Migrations/
```

Use:

```txt
--output-dir Persistence/Migrations
```

Do not handwrite migration files unless the CLI workflow is genuinely blocked; document any such exception in `context/progress-tracker.md`.

Use appropriate foreign keys and indexes for common match queries, including practical indexes around:

- `TeamId`;
- `SeasonId`;
- `CompetitionId`;
- `OpponentId`;
- `KickoffAtUtc`;
- `Status`;
- archive filtering.

Do not configure cascade deletion from settings/team records to matches.

### Tests

Add focused tests for the behavior introduced by this unit.

Domain/unit tests should cover at minimum:

- new match defaults to `SCHEDULED`;
- score pairing invariant;
- `PLAYED` requires both non-negative scores;
- non-played statuses reject final scores;
- allowed status transitions;
- invalid status transitions;
- `PLAYED` score correction while remaining `PLAYED`;
- archive/restore behavior if domain-owned.

Application/integration tests should cover at minimum:

- unauthenticated access;
- team-scope filtering on list queries;
- explicit unauthorized `teamId` filter returns `403`;
- inaccessible detail does not leak match data;
- `ADMIN` create/update behavior;
- `DATA_OPERATOR` create/update inside assigned team scope;
- `DATA_OPERATOR` cannot mutate outside assigned team scope;
- other roles cannot create/update;
- archive/restore is admin-only;
- reference validation;
- season date-range validation;
- exact duplicate conflict;
- team selection cannot be changed through update;
- invalid status transition returns `409`;
- played score correction succeeds;
- archived matches are excluded from default lists;
- migration/database behavior works in the existing integration-test environment.

Do not add frontend tests in this unit.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

If implementation reveals that match fields, role behavior, team-scope behavior, status rules, architecture boundaries, or API conventions need to differ from the documented project context, update the relevant context file before continuing.

Do not silently introduce additional match metadata or workflow rules.

## Implementation

### 1. Add match domain model

Create the `Match` entity and the small supporting enums for location and match status.

Implement:

- construction invariants;
- status transition behavior;
- score rules;
- metadata update behavior;
- archive/restore behavior according to existing project patterns.

Keep `TeamId` immutable after creation.

### 2. Add persistence mapping

Add EF Core configuration for the match entity.

Configure:

- table mapping;
- enum persistence consistent with project conventions;
- foreign keys to season, competition, team, opponent, and optional venue;
- restrictive/no-cascade delete behavior;
- useful indexes;
- archive filtering support consistent with existing patterns.

Add the migration through `dotnet ef`.

### 3. Add Application use cases

Add focused vertical slices for:

- list matches;
- get match detail;
- create match;
- update match;
- archive match;
- restore match.

Keep commands and queries single-purpose.

Reuse existing current-user access context and team-scope authorization services.

### 4. Add validation and reference checks

Validate:

- IDs;
- required metadata;
- location/status enum values at boundaries;
- kickoff date/time;
- season range;
- referenced record state;
- score invariants;
- exact duplicate fixture conflicts;
- allowed status transitions;
- mutation role and team scope.

Do not duplicate domain invariants only in endpoint handlers.

### 5. Add Minimal API endpoints

Map the required routes under the existing Matches endpoint group conventions.

Keep handlers thin:

- parse/map HTTP input;
- call Application use case;
- map Result to existing HTTP/ProblemDetails patterns.

### 6. Add tests

Add unit and integration coverage for all critical domain, authorization, filtering, conflict, status, score, archive, and persistence behavior described above.

### 7. Update progress documentation

Update `context/progress-tracker.md` to reflect the actual implementation state and verification outcome.

Do not mark Unit 30 complete until required checks pass or a failure is explicitly documented.

## Dependencies

None.

Use the packages and infrastructure already introduced by previous units.

Do not add new NuGet or npm packages unless an existing approved dependency is genuinely missing and the need is documented before installation.

## Verification checklist

- [ ] `Match` exists as a persistent domain entity without lineup, appearance, report, statistics, media, GPS, import, or audit fields.
- [ ] Match references existing season, competition, team, opponent, and optional venue records.
- [ ] `TeamId` is immutable after match creation.
- [ ] Match location supports `HOME`, `AWAY`, and `NEUTRAL`.
- [ ] Match status supports `SCHEDULED`, `PLAYED`, `POSTPONED`, and `CANCELLED`.
- [ ] New matches default to `SCHEDULED`.
- [ ] Status changes use domain/application logic rather than direct endpoint assignment.
- [ ] Allowed and invalid V1 status transitions behave as specified.
- [ ] `PLAYED` requires both non-negative score values.
- [ ] Non-played statuses cannot contain final scores.
- [ ] Played score corrections are supported without reopening the match status.
- [ ] Match creation validates active/non-archived referenced configuration records.
- [ ] Kickoff date/time is validated against the selected season date range.
- [ ] Exact duplicate fixtures return `409 Conflict`.
- [ ] No broad uniqueness rule blocks legitimate repeat fixtures.
- [ ] Match lists are restricted server-side by the caller's team scope.
- [ ] Explicit unauthorized `teamId` filtering returns `403`.
- [ ] Match detail does not expose inaccessible team data.
- [ ] `ADMIN` can create/update matches for any team.
- [ ] `DATA_OPERATOR` can create/update only inside authorized team scope.
- [ ] Other roles cannot create/update matches in this unit.
- [ ] Archive and restore are administrator-only.
- [ ] Matches are never hard-deleted by the Unit 30 API.
- [ ] Archived matches are excluded from default list results.
- [ ] Endpoint handlers remain thin and contain no EF Core queries or business rules.
- [ ] EF Core configuration uses non-cascading historical references.
- [ ] Migration files are generated under `backend/src/Infrastructure/Persistence/Migrations/`.
- [ ] Relevant domain/unit tests pass.
- [ ] Relevant integration tests pass.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for the affected backend solution/projects.
- [ ] `dotnet test` passes for the relevant backend test projects.
- [ ] `dotnet ef database update` succeeds when the local database environment is available.
- [ ] No frontend files are changed unless a documentation-only sync is explicitly required.
- [ ] `context/progress-tracker.md` reflects the actual Unit 30 implementation and verification state.
