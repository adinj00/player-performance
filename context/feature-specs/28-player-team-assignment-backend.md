# Unit 28: Player Team Assignment Backend

## Goal

Add persistent, time-bound player-to-selection assignment history so one FK Velež player can move through multiple selections without duplicating the player record. Support multiple simultaneous assignments to different selections, enforce non-overlapping history for the same player and selection, expose assignment-aware read access, and preserve Clean Architecture, authorization, persistence, and test boundaries.

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
9. `context/feature-specs/28-player-team-assignment-backend.md`

Use relevant installed backend Codex plugins when applicable, but do not let plugin guidance override project context or this spec.

This unit is backend-only. It adds player assignment history and the minimum assignment-aware player read authorization required for the future Players UI. It must not implement player UI, match lineups, match appearances, positions, shirt numbers, medical data, imports, media, or audit persistence.

### Persistent player identity remains unchanged

`Player` remains the stable club-level entity introduced in Unit 27.

Do not add any of the following directly to `Player`:

- `TeamId`;
- `CurrentTeamId`;
- current team name;
- a single current selection field;
- duplicated player records per team.

Selection membership belongs exclusively to time-bound `PlayerTeamAssignment` records.

### Player team assignment model

Add a dedicated domain entity named `PlayerTeamAssignment` or an equivalently explicit name with this minimum model:

```txt
PlayerTeamAssignment
- Id
- PlayerId
- TeamId
- StartDate
- EndDate (optional)
- CreatedAtUtc
- UpdatedAtUtc
```

Required semantics:

- `StartDate` and `EndDate` are date-only values.
- `StartDate` is required.
- `EndDate = null` means the assignment has no recorded end date.
- A record may represent a historical, current, or future assignment.
- Date ranges are inclusive for human-facing assignment history.
- `EndDate`, when present, must be greater than or equal to `StartDate`.
- A player may have overlapping assignment periods across different teams/selections.
- A player must not have overlapping assignment periods for the same team/selection.
- A player may leave a selection and later return through a new non-overlapping assignment record.
- An existing assignment is historical data and must not be physically deleted through the API.

Do not persist a separate `IsCurrent` boolean. Current/upcoming/past state is derived from dates and the approved clock abstraction.

### Derived assignment state

For API responses, derive assignment timing relative to the current UTC date:

```txt
UPCOMING
- StartDate is after today

CURRENT
- StartDate is on or before today
- and EndDate is null or on or after today

PAST
- EndDate is before today
```

The exact enum/type name may follow existing response-contract conventions, but this timing state is read-model behavior and must not become a second source-of-truth persistence field.

### Overlap invariant

For the same `PlayerId` and `TeamId`, assignment date ranges must never overlap.

Treat ranges as inclusive. Therefore:

```txt
Existing: 2026-01-01 -> 2026-06-30
Allowed new start: 2026-07-01
Rejected new start: 2026-06-30
```

Open-ended ranges overlap every later range for the same player/team until they are ended.

Required rules:

- multiple simultaneous assignments to different teams are allowed;
- multiple simultaneous assignments to the same team are not allowed;
- at most one open-ended assignment may exist for the same player/team pair;
- creating a historical assignment must also pass overlap validation;
- ending an open-ended assignment must not create an overlap with another existing assignment for the same player/team;
- all overlap checks must exclude the current record when validating a change to that record.

Enforce the invariant inside Domain/Application behavior, not in Minimal API handlers.

Use database constraints and indexes where practical, but do not introduce a speculative PostgreSQL extension or hand-written provider-specific range constraint solely for this unit unless the existing repository already has an approved pattern for it. The Application layer must still validate overlap before persistence.

### Assignment creation rules

A new assignment may be created only when:

- the player exists;
- the player record status is `ACTIVE`;
- the target team/selection exists;
- the target team/selection is operationally active according to the Unit 22 team lifecycle;
- the supplied date range is valid;
- the assignment does not overlap another assignment for the same player/team.

Do not automatically end assignments in other teams when a new assignment is created.

This is required because the product explicitly allows a player to train or play with multiple selections at the same time.

Do not infer promotion, demotion, loan, temporary call-up, or transfer semantics. This unit stores assignment history only.

### Ending an assignment

Provide an explicit command to end an assignment.

Required behavior:

- only assignments with `EndDate = null` can be ended through the dedicated end action;
- the supplied end date must be greater than or equal to the assignment start date;
- ending an assignment must pass same-team overlap validation;
- ending an already ended assignment returns a safe `409 Conflict` unless an existing canonical idempotency convention clearly requires a successful no-op;
- ending an assignment does not change the player lifecycle status;
- ending one team assignment does not affect assignments in other teams.

Do not add reopen, hard-delete, or arbitrary team-change behavior in this unit.

### Historical assignment creation

The create contract may include an optional `EndDate` so administrators can backfill historical development-path records in one operation.

This must use the same validation and overlap rules as current assignments.

Do not require historical data to be entered chronologically.

### Player lifecycle interaction

After Unit 28, active assignments become relevant to the player lifecycle.

Strengthen player lifecycle orchestration so that:

- a player with at least one `CURRENT` assignment cannot be deactivated;
- a player with at least one `CURRENT` assignment cannot be archived;
- the user must first end current assignments before deactivating or archiving the player;
- activating an inactive player does not create an assignment automatically;
- restoring an archived player still returns the player to `INACTIVE` as defined in Unit 27.

Return a safe `409 Conflict` for lifecycle commands blocked by current assignments.

Keep this cross-entity rule in Application/domain orchestration, not endpoint handlers.

Do not retroactively rewrite Unit 27 migration history. Add only the Unit 28 migration required for the assignment table and related indexes/constraints.

### Team lifecycle interaction

Existing assignment history must survive later team deactivation or archival.

In this unit:

- only operationally active teams may receive new assignments;
- existing historical/current assignment rows are not deleted when a team is deactivated or archived;
- do not automatically end player assignments when a team status changes;
- do not modify Unit 22 team lifecycle behavior unless a concrete compile/runtime integration requirement makes a minimal change necessary.

Future business rules may tighten team archival behavior, but this unit must not invent them.

### Authorization model

Assignment mutations are administrative roster-management operations in this unit.

Required mutation authorization:

- create assignment: `AdminOnly`;
- end assignment: `AdminOnly`;
- no non-admin assignment mutation endpoint is introduced.

Player and assignment read access becomes team-scope aware in this unit.

Required read behavior:

- `ADMIN` may read all players and all assignment history;
- a non-admin user with `ALL_TEAMS` scope may read all non-restricted player records allowed by the existing player lifecycle filters;
- a non-admin user with `SELECTED_TEAMS` scope may read a player only when that player has at least one `CURRENT` assignment to a team inside the user's selected team scope;
- historical assignment membership alone must not grant current access;
- future assignment membership alone must not grant current access;
- once a player no longer has a current assignment in any team inside a selected-scope user's access, that user must no longer receive the player through list/detail queries;
- when access to a specific player is denied, return the project's established safe inaccessible-resource behavior without exposing restricted player existence unnecessarily.

Use the reusable team-access/current-user infrastructure introduced in Unit 23.

Do not copy role/team-scope authorization logic into endpoint handlers or duplicate it separately in each query.

### Player read API changes

Unit 27 made all `/api/players` routes administrator-only because assignment-aware scope did not yet exist.

In this unit, split authorization deliberately:

- player mutations remain `AdminOnly`;
- player list and detail reads become available to authenticated active staff according to the assignment-aware access rules above;
- required-password-change enforcement remains active;
- malformed or missing staff access profiles fail closed.

Do not weaken create, update, lifecycle, or assignment mutation authorization.

### API surface

Add assignment endpoints under the player resource:

```txt
GET  /api/players/{playerId}/assignments
POST /api/players/{playerId}/assignments
POST /api/players/{playerId}/assignments/{assignmentId}/end
```

Equivalent route naming is acceptable only when it follows an established repository convention and preserves clear resource ownership.

Do not add:

- `DELETE` assignment endpoints;
- assignment hard-delete;
- assignment reopen;
- direct team-change endpoint on an existing assignment;
- generic PATCH of arbitrary assignment fields.

### Assignment list behavior

`GET /api/players/{playerId}/assignments` must:

- enforce player read access before returning history;
- return the full assignment history for an accessible player;
- include team identity needed for future display without requiring N+1 frontend requests;
- sort deterministically by `StartDate` descending, then `EndDate` descending with open-ended/current records handled consistently, then `Id` as a final tie-breaker;
- expose derived timing state;
- return inactive or archived team names as historical references rather than hiding valid history.

Recommended response fields:

```txt
PlayerTeamAssignmentResponse
- id
- playerId
- teamId
- teamName
- startDate
- endDate
- timingState
- createdAtUtc
- updatedAtUtc
```

Do not expose EF Core entities.

### Assignment request contracts

Recommended create request:

```txt
CreatePlayerTeamAssignmentRequest
- teamId
- startDate
- endDate (optional)
```

Recommended end request:

```txt
EndPlayerTeamAssignmentRequest
- endDate
```

Requirements:

- `playerId` comes from the route;
- `assignmentId` comes from the route for end operations;
- the request must not accept player lifecycle status, team lifecycle status, timestamps, derived timing state, or authorization fields;
- team identity on an existing assignment is immutable in this unit.

### Player list filtering and current assignment summary

Extend the Unit 27 player list contract only as needed for the future Unit 29 Players UI.

Add an optional team filter:

```txt
teamId (optional)
```

Rules:

- filtering by `teamId` means players with a `CURRENT` assignment to that team;
- administrators may filter by any existing team;
- `ALL_TEAMS` users may filter by any team they are allowed to access under the canonical access model;
- `SELECTED_TEAMS` users may request only a team within their scope;
- an out-of-scope requested team returns `403` or the canonical access-denied result rather than silently broadening the query;
- without a team filter, selected-scope users receive the union of players currently assigned to at least one team in their scope;
- pagination and filtering remain database-side.

Add a compact current-assignment summary to player list/detail read models when practical for Unit 29:

```txt
currentAssignments
- teamId
- teamName
- startDate
- endDate (optional)
```

Do not return fabricated placeholder sections for matches, statistics, medical data, or media.

### Persistence and indexing

Add `PlayerTeamAssignment` to the existing persistence abstraction and Infrastructure `AppDbContext` with explicit EF Core configuration.

Required persistence behavior:

- required foreign key to `Player`;
- required foreign key to `Team`;
- no cascade delete that could erase assignment history through normal player/team operations;
- date-only mapping for start/end dates;
- explicit indexes supporting:
  - player history queries;
  - current assignments by player;
  - current assignments by team;
  - overlap checks for a player/team pair;
- at minimum, prevent duplicate open-ended assignment rows for the same player/team when practical with the existing PostgreSQL/EF Core conventions;
- keep overlap validation in Application/domain logic even if database constraints assist it;
- no seed assignment data.

Add one focused migration containing only Unit 28 schema changes.

Inspect the generated migration for unintended changes to Identity, staff access, settings, teams, seasons, competitions, venues, opponents, or the Unit 27 player schema beyond required foreign-key/index integration.

### Transaction and consistency requirements

Assignment creation and assignment ending must validate invariants against current persisted assignment state and save atomically.

Use the existing Application transaction/persistence pattern.

Do not:

- perform overlap validation solely in the frontend;
- load all assignments for all players into memory;
- place EF Core queries in API endpoint handlers;
- introduce a generic repository abstraction if the project has not adopted one.

Where concurrent requests could race, use the strongest consistency pattern already established by the repository. Do not introduce a large new concurrency framework in this unit.

### Error behavior

Use the existing Result and ProblemDetails conventions.

Expected behavior:

- `400 Bad Request` for malformed route/request binding;
- `401 Unauthorized` for unauthenticated requests;
- `403 Forbidden` for unauthorized mutation or out-of-scope team access;
- the established inaccessible-resource behavior for a player outside the caller's read scope;
- `404 Not Found` for an accessible but missing player, team, or assignment according to existing conventions;
- `409 Conflict` for overlapping assignments, invalid assignment ending, blocked player deactivation/archive, or other state conflicts;
- the project-standard semantic validation response for invalid dates;
- `500 Internal Server Error` only for unexpected failures.

Use stable machine-readable error codes suitable for future Bosnian localization, for example equivalents of:

```txt
player_assignment_overlap
player_assignment_already_ended
player_has_current_assignments
team_not_active
player_not_active
```

Do not expose SQL, constraint names, stack traces, or provider internals.

### Audit readiness

The full audit subsystem remains planned for Unit 38.

In this unit:

- keep assignment creation and ending in explicit Application use cases;
- keep player lifecycle conflicts and authorization decisions outside endpoint handlers;
- do not add a temporary audit table or duplicate logging framework;
- document that assignment mutation audit persistence remains deferred.

### Scope limits

Do not implement any of the following:

- frontend player pages, assignment dialogs, or assignment timeline UI;
- player positions or position history;
- shirt numbers;
- captaincy;
- registration, contract, loan, transfer, promotion, or demotion semantics;
- match lineup or match appearance records;
- automatic assignment creation from match selection;
- medical availability or injuries;
- player media or images;
- notes;
- roster import;
- Gpexe or Zone14 mappings;
- assignment hard-delete;
- generic assignment edit/reopen UI or API;
- audit persistence;
- speculative dependencies.

## Implementation

### 1. Add the player assignment domain model

Create `PlayerTeamAssignment` and the minimum domain behavior required for:

- valid date-range construction;
- optional end date;
- ending an open-ended assignment;
- immutable player/team identity after creation;
- UTC timestamp updates through the approved clock pattern.

Keep same-player/same-team overlap checks in a domain/application service or use case that has access to persisted assignment history.

Do not place cross-record overlap queries inside the entity itself.

### 2. Add assignment persistence contracts and queries

Extend the Application persistence abstraction with focused capabilities for:

- loading a player for assignment operations;
- loading a team for assignment operations;
- checking overlapping assignments for one player/team pair;
- checking whether a player has current assignments;
- listing assignment history for one player;
- filtering accessible players by current team assignments;
- loading compact current-assignment summaries for player read models.

Use explicit queries/use-case-oriented persistence access consistent with the existing architecture.

Do not introduce broad speculative repositories.

### 3. Add create-assignment use case

Implement a focused Application vertical slice that:

1. requires administrator authorization;
2. validates request dates;
3. loads the player and requires `ACTIVE` player status;
4. loads the team and requires an operationally active team;
5. checks same-player/same-team overlap;
6. creates the assignment;
7. saves atomically;
8. returns the assignment response contract.

Do not end or modify assignments in other teams.

### 4. Add end-assignment use case

Implement a focused Application vertical slice that:

1. requires administrator authorization;
2. verifies the route player and assignment relationship;
3. rejects an already ended assignment according to the conflict rule;
4. validates the end date against the start date;
5. rechecks same-team overlap with the proposed end date;
6. ends the assignment;
7. saves atomically;
8. returns the updated assignment response.

Do not allow team reassignment, reopen, or hard-delete through this use case.

### 5. Strengthen player lifecycle orchestration

Update the existing Unit 27 deactivate and archive use cases so they reject the operation when the player has a `CURRENT` assignment according to the approved clock date.

Requirements:

- return a typed conflict result;
- do not query assignments in the endpoint handler;
- do not automatically end assignments;
- keep restore semantics unchanged;
- add regression tests for Unit 27 lifecycle behavior that changed because assignments now exist.

### 6. Add assignment-aware player read access

Refactor the Unit 27 player list/detail authorization boundary so:

- mutations remain admin-only;
- reads use the canonical current-user/team-scope infrastructure;
- selected-team scope is evaluated against `CURRENT` assignments;
- `ALL_TEAMS` scope may read across teams;
- admins retain unrestricted read access;
- list queries are filtered in the database rather than loaded and filtered in memory.

Add the optional `teamId` player-list filter and compact current-assignment summaries where defined by this spec.

Keep authorization logic centralized and reusable for later Matches, Dashboard, and Medical modules.

### 7. Add assignment read query and endpoints

Implement:

```txt
GET  /api/players/{playerId}/assignments
POST /api/players/{playerId}/assignments
POST /api/players/{playerId}/assignments/{assignmentId}/end
```

Endpoint handlers must:

- remain thin;
- bind HTTP input;
- delegate to Application use cases;
- preserve ProblemDetails mapping;
- enforce the correct read or admin mutation policy;
- never expose EF Core entities.

### 8. Add Infrastructure persistence and migration

Add:

- `PlayerTeamAssignments` DbSet or equivalent;
- explicit EF Core entity configuration;
- player/team foreign keys with history-safe delete behavior;
- date-only mappings;
- indexes for player history, team/current assignment queries, and overlap checks;
- one focused Unit 28 migration.

Use `dotnet ef` tooling according to project standards.

Do not handwrite migration files unless the documented exception path is genuinely required.

### 9. Add domain and Application tests

Add focused tests for at least:

- valid historical assignment;
- valid open-ended assignment;
- end date before start date rejected;
- multiple simultaneous assignments to different teams allowed;
- overlapping assignments for the same team rejected;
- adjacent non-overlapping same-team periods allowed;
- a player leaving and later returning to the same team;
- open-ended duplicate same-team assignment rejected;
- ending an assignment;
- ending an already ended assignment rejected;
- inactive/archived player cannot receive a new assignment;
- non-active team cannot receive a new assignment;
- current assignment blocks player deactivation;
- current assignment blocks player archive;
- historical or future-only assignment does not incorrectly count as current.

Use the approved clock abstraction for deterministic current-date tests.

### 10. Add integration tests

Add integration coverage for at least:

- unauthenticated assignment requests return `401`;
- non-admin assignment mutation returns `403`;
- administrator can create historical, current, and future assignments;
- same-team overlap returns `409`;
- simultaneous different-team assignments are accepted;
- assignment list returns deterministic history with team names and derived timing state;
- assignment end action works and repeated end returns the expected conflict;
- inactive/archived player assignment creation is rejected;
- non-active team assignment creation is rejected;
- player deactivate/archive is blocked while a current assignment exists;
- admin player list/detail still works across all assignments;
- `ALL_TEAMS` user can read permitted player records;
- `SELECTED_TEAMS` user can read a player with a current assignment inside scope;
- `SELECTED_TEAMS` user cannot read a player whose only current assignments are outside scope;
- historical-only or future-only membership does not incorrectly grant current selected-team access;
- `teamId` filtering respects scope and returns `403` for an explicitly out-of-scope requested team;
- player list filtering/pagination stays correct after assignment joins and does not duplicate players with multiple current assignments;
- no assignment delete route is exposed.

Use existing integration-test authentication and database helpers. Do not introduce a new testing platform.

### 11. Update project progress documentation

After implementation and successful verification, update `context/progress-tracker.md` to reflect:

- Unit 28 implementation state;
- the exact assignment fields and date semantics implemented;
- that multiple simultaneous different-team assignments are supported;
- that same-team overlapping periods are rejected;
- the final player read authorization behavior for `ADMIN`, `ALL_TEAMS`, and `SELECTED_TEAMS` users;
- that assignment mutation is admin-only in this unit;
- that assignment UI remains for Unit 29;
- that assignment mutation audit persistence remains deferred to Unit 38;
- any failed verification or deviation from this spec.

Do not mark the unit complete unless required verification passes or failures are explicitly documented.

## Dependencies

None.

Use the existing ASP.NET Core, EF Core/PostgreSQL, FluentValidation, authentication/authorization, staff team-scope, Result/ProblemDetails, clock, migration, and test infrastructure introduced by earlier units. Do not add a new NuGet package unless the current repository is missing a capability explicitly required by this spec and the addition is documented.

## Verification checklist

- [ ] `Player` remains a persistent club-level entity with no direct current-team foreign key.
- [ ] `PlayerTeamAssignment` persists `PlayerId`, `TeamId`, `StartDate`, optional `EndDate`, and timestamps.
- [ ] Assignment dates use date-only semantics.
- [ ] `EndDate`, when present, cannot be before `StartDate`.
- [ ] Multiple overlapping assignments to different teams are allowed.
- [ ] Overlapping assignments for the same player/team are rejected.
- [ ] A player may leave and later return to the same team through a new non-overlapping assignment.
- [ ] Derived `UPCOMING`, `CURRENT`, and `PAST` timing state is not persisted as a second source of truth.
- [ ] New assignments require an `ACTIVE` player and an operationally active team.
- [ ] Creating an assignment does not automatically end assignments in other teams.
- [ ] Assignment creation and ending are admin-only.
- [ ] No assignment hard-delete endpoint exists.
- [ ] No generic team-change, reopen, or arbitrary assignment PATCH endpoint was added.
- [ ] Current assignments block player deactivation and archive operations with a safe conflict response.
- [ ] Player restore behavior remains `ARCHIVED -> INACTIVE`.
- [ ] Player list and detail reads are assignment-aware for non-admin team-scoped staff.
- [ ] `ADMIN` retains unrestricted player read access.
- [ ] `ALL_TEAMS` scope can read across teams according to the established lifecycle filters.
- [ ] `SELECTED_TEAMS` access is granted only through at least one `CURRENT` assignment in scope.
- [ ] Historical-only or future-only assignments do not incorrectly grant selected-team access.
- [ ] Optional `teamId` player-list filtering is enforced server-side and respects caller scope.
- [ ] Multiple current assignments do not duplicate player rows in paged list results.
- [ ] Assignment history returns deterministic ordering, team identity, and derived timing state.
- [ ] API responses do not expose EF Core entities or provider details.
- [ ] Application/domain logic owns overlap and lifecycle invariants; endpoint handlers remain thin.
- [ ] EF Core configuration and one focused Unit 28 migration exist.
- [ ] Foreign-key delete behavior does not silently erase assignment history.
- [ ] Useful indexes exist for player history, team/current assignment queries, and overlap checks.
- [ ] The generated migration contains no unintended changes to unrelated modules.
- [ ] Unit/Application tests cover assignment date rules, overlap rules, multiple-team support, lifecycle interaction, and current-date behavior.
- [ ] Integration tests cover authorization, assignment CRUD boundaries, overlap conflicts, team scope, list filtering, paging, and player lifecycle conflicts.
- [ ] `dotnet restore backend/PlayerPerformance.sln` passes.
- [ ] `dotnet format backend/PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format backend/PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes after formatting.
- [ ] `dotnet build backend/PlayerPerformance.sln --no-restore` passes.
- [ ] `dotnet test backend/PlayerPerformance.sln --no-build` passes.
- [ ] `context/progress-tracker.md` reflects the actual Unit 28 outcome and deferred Unit 29/Unit 38 work.
- [ ] No frontend UI, match, statistics, medical, media, import, vendor integration, or speculative dependency work was introduced.
