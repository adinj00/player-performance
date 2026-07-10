# Unit 22: Teams / Selections Backend

## Goal

Implement the configurable FK Velež team/selection backend, including persistence, fixed V1 tracking levels, lifecycle state, deterministic ordering, idempotent default seeding, admin-protected API operations, and focused tests. This unit must establish stable selection identifiers for later staff team scopes, player assignments, matches, dashboards, and tracking-level-dependent workflows without implementing those later features.

## Design

### Scope

This is a backend-only vertical-slice unit for the club's own teams/selections.

The completed unit must provide:

- a canonical `Team` domain aggregate representing an FK Velež selection;
- the fixed V1 `TeamTrackingLevel` values `BASIC`, `STANDARD`, and `FULL`;
- explicit `ACTIVE`, `INACTIVE`, and `ARCHIVED` lifecycle states;
- EF Core mapping and a database migration;
- normalized unique names;
- deterministic display ordering and an atomic reorder operation;
- idempotent default selection seeding when no team records exist;
- Application commands, queries, DTOs, validators, and persistence abstractions;
- Minimal API endpoints protected by the existing `AdminOnly` policy;
- ProblemDetails-compatible error behavior using established API conventions;
- unit and integration tests;
- progress-tracker updates.

This unit does not add:

- frontend settings screens or forms;
- staff user CRUD, invitations, or team-scope assignments;
- player records or player-team assignments;
- matches, lineups, reports, imports, training sessions, or dashboards;
- season-specific team records or a season/team join table;
- user-defined tracking-level records;
- tracking-level field gating for later workflows;
- audit-log persistence;
- hard-delete endpoints;
- public or non-admin team-list endpoints;
- logos, kits, age metadata, external IDs, or other speculative fields.

### Canonical naming

Use `Team` as the canonical backend aggregate and module concept because that name is already established in `context/architecture.md`. In the product and future Bosnian Latin UI, these records may be presented as club selections (`Selekcije`).

Do not create parallel `Team` and `Selection` entities for the same concept. Opponents remain a separate future settings resource and must not reuse this aggregate.

### Architecture boundaries

Teams belong to the backend `Teams` module defined in `context/architecture.md`.

Use the existing Clean Architecture boundaries:

- `Domain` owns the `Team` aggregate, enums, invariants, and lifecycle transitions;
- `Application` owns commands, queries, DTOs, validation, ordering orchestration, seeding abstraction where needed, and persistence interfaces;
- `Infrastructure` owns EF Core persistence, database constraints, migration, transactional reorder behavior, and startup seed implementation;
- `Api` owns endpoint mapping, authorization metadata, request/response mapping, and ProblemDetails conversion.

Do not place EF Core queries, status transition rules, duplicate-name handling, reorder logic, or seed rules directly in Minimal API handlers.

Follow the existing Vertical Slice organization and established Unit 21 settings patterns where they remain appropriate. Do not add a generic repository, MediatR, a generic CRUD framework, or a broad settings abstraction solely for this unit.

### Team model

The aggregate must contain only the confirmed V1 fields:

```txt
Team
- Id
- Name
- NormalizedName
- TrackingLevel
- Status
- DisplayOrder
- CreatedAtUtc
- UpdatedAtUtc
```

Requirements:

- Use the existing project ID strategy and shared domain primitive conventions.
- `Name` is required, trimmed, and stored in its administrator-entered display form.
- Reuse the settings-name length convention established in Unit 21 rather than introducing a conflicting limit.
- `NormalizedName` is server-owned and must never be accepted from API clients.
- Team names are unique by deterministic normalized comparison, including archived records.
- Name uniqueness must be protected by a database constraint in addition to application checks.
- `TrackingLevel` is required and must be one of the fixed V1 enum values.
- `Status` is required and defaults to `ACTIVE` for newly created teams.
- `DisplayOrder` is server-owned, non-negative, and used only for ordering club selections.
- Created and updated timestamps use the approved clock abstraction and UTC.
- Do not add a `SeasonId`; FK Velež selections are persistent club structures across seasons.
- Do not add navigation collections for users, players, assignments, or matches before those relationships are implemented in later units.

### Tracking levels

Define the canonical V1 tracking-level enum:

```txt
TeamTrackingLevel
- BASIC
- STANDARD
- FULL
```

The levels themselves are fixed system values in V1. Administrators configure which level is assigned to each team; they do not create, rename, reorder, or delete tracking-level definitions.

The meaning of the levels is intentionally limited in this unit:

- `BASIC` identifies selections with the lightest data/workflow expectations;
- `STANDARD` identifies selections with intermediate tracking expectations;
- `FULL` identifies selections with the fullest approved V1 tracking expectations.

Do not yet hardcode exact match-stat fields, GPS fields, reports, import mappings, or permissions for each level. Later feature specs must define those behaviors when the relevant modules exist. This unit only stores and exposes the selected level.

Persist enum values using the established project enum convention. If no convention exists yet, prefer stable string values rather than provider-specific ordinal assumptions.

### Lifecycle state

Define the canonical team status enum:

```txt
TeamStatus
- ACTIVE
- INACTIVE
- ARCHIVED
```

Required transitions:

```txt
ACTIVE   -> INACTIVE
INACTIVE -> ACTIVE
ACTIVE   -> ARCHIVED
INACTIVE -> ARCHIVED
ARCHIVED -> INACTIVE
```

Behavior:

- New teams start as `ACTIVE`.
- Deactivation changes `ACTIVE` to `INACTIVE`.
- Activation changes `INACTIVE` to `ACTIVE`.
- Archiving changes an active or inactive team to `ARCHIVED`.
- Restoring an archived team changes it to `INACTIVE`, not automatically to `ACTIVE`; an administrator must explicitly activate it.
- Repeating activate, deactivate, archive, or restore against the already-achieved state should be idempotent where the requested state is valid.
- An archived team cannot be renamed, have its tracking level changed, or participate in normal reorder requests until restored.
- An inactive team remains editable and reorderable.
- No physical delete behavior is added.
- Do not invent a rule requiring at least one active selection; that business rule is not documented.

Every successful state change updates `UpdatedAtUtc` through the approved clock abstraction.

### Name normalization and uniqueness

Reuse the deterministic normalized-name pattern introduced for seasons and competitions in Unit 21.

Requirements:

- Trim surrounding whitespace before validating and storing the display name.
- Treat names differing only by case or insignificant surrounding whitespace as duplicates.
- Keep the administrator-entered casing for display.
- Keep archived names reserved; creating or renaming another team to the same normalized name returns `409 Conflict`.
- Translate database unique-constraint failures into safe ProblemDetails responses.
- Do not expose provider exceptions, SQL details, or database constraint names.
- Do not add a PostgreSQL extension solely for case-insensitive comparison unless the existing persistence layer already adopted one.

### Display ordering

The non-archived team list must have a deterministic administrator-controlled order.

Creation behavior:

- A newly created team is appended after the highest current non-archived `DisplayOrder`.
- Creating the first team uses order `0`.

Reorder behavior:

- Provide one atomic reorder use case accepting the complete ordered list of all current non-archived team IDs.
- Every non-archived team must appear exactly once.
- Duplicate IDs, unknown IDs, archived IDs, omitted current IDs, or extra IDs fail validation without changing any order.
- Successful reorder assigns contiguous zero-based values in the supplied order.
- Perform the reorder in one transaction.
- Do not save one HTTP-visible partial order if any part fails.
- Concurrent or database failures must leave the previous valid order intact.

Archive and restore behavior:

- Archived teams are excluded from the normal ordered list.
- An archived team may retain its historical stored order internally, but it must not create ambiguous ordering for active/inactive results.
- On restore, append the team after the last current non-archived team and assign a valid contiguous position through the approved ordering logic.
- After archive or restore, normalize non-archived positions when needed so the returned list remains contiguous.

Database-level uniqueness for `DisplayOrder` is optional if it would make transactional reordering unnecessarily brittle. The Application/Infrastructure operation must nevertheless guarantee a unique contiguous order for all non-archived records after each successful mutation.

### Default selection seeding

Seed the confirmed default FK Velež selections only when the teams table contains no records:

| Display order | Name | Tracking level | Initial status |
| --- | --- | --- | --- |
| 0 | `First Team` | `FULL` | `ACTIVE` |
| 1 | `U19` | `STANDARD` | `ACTIVE` |
| 2 | `U17` | `STANDARD` | `ACTIVE` |
| 3 | `U15` | `BASIC` | `ACTIVE` |
| 4 | `U13` | `BASIC` | `ACTIVE` |
| 5 | `U11` | `BASIC` | `ACTIVE` |

Seed rules:

- Use the names and default tracking levels confirmed in `context/project-overview.md`.
- Run through the existing controlled backend startup initialization pattern, not through a public endpoint.
- Seeding must be idempotent across restarts.
- If any team record already exists, do not add missing defaults, rename records, reset tracking levels, reactivate records, or reorder administrator-managed data.
- Archived records still count as existing records and prevent first-run seeding.
- Do not use seeding to create demo players, matches, users, seasons, competitions, or other sample content.
- Do not log secrets or unnecessary record payloads.
- Handle simultaneous startup attempts safely enough that duplicate default teams are not committed.
- Use the existing clock and ID conventions.

The default names are initial club data, not translatable application chrome. Future administrators may rename them, and future UI must display the stored domain values as entered.

### API contract

Create one admin-protected endpoint group:

```txt
/api/settings/teams
```

Recommended routes:

```txt
GET    /api/settings/teams
GET    /api/settings/teams/{teamId}
POST   /api/settings/teams
PATCH  /api/settings/teams/{teamId}
PUT    /api/settings/teams/order
POST   /api/settings/teams/{teamId}/activate
POST   /api/settings/teams/{teamId}/deactivate
POST   /api/settings/teams/{teamId}/archive
POST   /api/settings/teams/{teamId}/restore
```

An equivalent reorder verb/path is acceptable only when it follows an already-established project convention and preserves the same atomic semantics.

All routes must:

- require authentication;
- remain subject to the Unit 18 required-password-change gate;
- require the Unit 20 `AdminOnly` policy;
- derive the actor from the authenticated backend context;
- return API responses rather than HTML redirects;
- use established ProblemDetails behavior;
- avoid exposing EF entities directly.

Do not add a general authenticated `GET /api/teams` endpoint yet. Unit 23 will introduce team-scope rules, after which later specs can define scope-filtered team queries for non-admin users.

### Request contracts

Recommended contracts:

```txt
CreateTeamRequest
- name
- trackingLevel

UpdateTeamRequest
- name
- trackingLevel

ReorderTeamsRequest
- orderedTeamIds
```

Requirements:

- IDs come from route parameters for single-resource operations.
- `DisplayOrder`, `Status`, normalized names, and timestamps are server-owned.
- Create/update payloads cannot directly assign lifecycle status.
- Lifecycle changes use their dedicated endpoints.
- Reorder payload accepts only the ordered identifiers required for the operation.
- Request DTOs remain API/Application contracts, not EF entities.
- Enum parsing failures produce safe validation errors rather than unhandled exceptions.

### Response contracts

Recommended response shape:

```txt
TeamResponse
- id
- name
- trackingLevel
- status
- displayOrder
- createdAtUtc
- updatedAtUtc
```

Requirements:

- Default list excludes `ARCHIVED` teams.
- `includeArchived=true` includes all statuses.
- Default and archived-inclusive lists sort by `DisplayOrder`, then normalized name, then identifier as a stable tie-breaker.
- Tracking-level and status values are returned as stable internal English enum values; future UI translates their labels.
- Do not add pagination for this expected-small settings list.
- Do not include future player counts, staff counts, match counts, or allowed-actions objects in this unit.

### Validation and errors

Use the FluentValidation/Application validation pattern established in Unit 21.

Minimum create/update validation:

- name is required after trimming;
- name uses the existing settings-name maximum length;
- tracking level is a defined `TeamTrackingLevel` value;
- route ID is valid according to the project's ID strategy;
- archived teams cannot be updated before restore.

Minimum reorder validation:

- `orderedTeamIds` is present;
- every ID is valid;
- no ID appears more than once;
- the list exactly matches the current non-archived team set;
- archived teams are rejected;
- validation completes before any persistent order change.

Expected errors:

- `400 Bad Request` for malformed route/body input;
- the established validation status (`400` or `422`) for semantic validation failures, consistently with Unit 21;
- `401 Unauthorized` for unauthenticated requests;
- `403 Forbidden` for authenticated non-admin users;
- `404 Not Found` for missing team IDs;
- `409 Conflict` for duplicate normalized names, updating archived teams, or invalid lifecycle conflicts not represented as idempotent success;
- `500` only for unexpected failures handled through the global exception pipeline.

Errors must not reveal database details, stack traces, secrets, or whether inaccessible protected resources exist beyond the established authorization behavior.

## Implementation

### 1. Required reading and existing-pattern review

Before changing code:

1. Read root `AGENTS.md`.
2. Read the six context files in the required order.
3. Read this feature spec completely.
4. Review Unit 20 authorization/current-user patterns.
5. Review Unit 21 settings entities, normalized-name behavior, FluentValidation registration, ProblemDetails mapping, endpoint-group conventions, and test setup.
6. Use applicable installed backend Codex skills/plugins without allowing them to override project context or this spec.

Do not refactor prior units unless a small change is required to reuse an established pattern or correctly register this module.

### 2. Domain implementation

Add the Teams domain types in the `Domain` project/module:

- `Team` aggregate;
- `TeamTrackingLevel` enum;
- `TeamStatus` enum.

The aggregate must own:

- creation with trimmed valid name and required tracking level;
- rename behavior;
- tracking-level change behavior;
- activate/deactivate transitions;
- archive/restore transitions;
- display-order assignment through a controlled method suitable for Application reorder orchestration;
- timestamp updates through supplied UTC time.

Keep constructors/setters appropriately restricted so API, Application, and Infrastructure code cannot bypass invariants through arbitrary property assignment.

Do not put authorization, EF Core, HTTP, localization, or DTO concerns in Domain.

### 3. Application slices

Create focused vertical slices for:

- list teams;
- get team by ID;
- create team;
- update team;
- activate team;
- deactivate team;
- archive team;
- restore team;
- reorder teams.

Each slice must contain only the contracts, validator, handler/use case, and mapping it needs. Reuse genuinely shared module contracts where that improves consistency without creating a generic CRUD abstraction.

Application behavior must:

- query through Application-owned persistence abstractions;
- perform normalized duplicate checks while still relying on database uniqueness as the final guarantee;
- use the approved clock;
- execute reorder/archive/restore order normalization transactionally through an appropriate abstraction;
- map domain/application failures to established result/error primitives;
- avoid exposing EF entities or provider exceptions.

### 4. Persistence and migration

Add the team persistence model through the existing `AppDbContext` and Infrastructure conventions.

Configure:

- table and column names consistent with the project;
- ID mapping consistent with existing entities;
- required name and normalized-name lengths;
- unique index on normalized name;
- enum persistence using the approved stable convention;
- non-negative display order constraints where supported by existing migration conventions;
- useful indexes for status and ordered listing;
- UTC timestamp persistence;
- no cascade relationships because later foreign keys do not exist yet.

Create a named migration for this unit. Review the generated migration to ensure it contains only expected team schema changes and no unrelated destructive operations.

Do not edit an already-applied prior migration to insert this model.

### 5. Default-team initializer

Implement an idempotent startup initializer using the established startup initialization composition.

Requirements:

- Run after persistence is available and before the application is considered ready.
- Check whether any team row exists.
- Insert all six defaults in one transaction only when the table is empty.
- Preserve exact initial order, tracking levels, and active status from this spec.
- Avoid duplicate inserts across normal repeated startup.
- Fail startup clearly if a genuine persistence/configuration failure prevents required first-run initialization; do not swallow the error and continue with a partially seeded set.
- Keep initialization logic outside `Program.cs` except for concise registration/invocation composition.

### 6. API endpoint group

Add a focused Teams settings endpoint group under `/api/settings/teams`.

Endpoint handlers must:

- stay thin;
- bind route/query/body input;
- delegate to Application use cases;
- apply `AdminOnly` once at the group level where possible;
- preserve API-specific `401`/`403` behavior;
- map validation, not-found, and conflict outcomes to established ProblemDetails responses;
- return the correct response DTO and status code;
- use `201 Created` with a stable location for successful creation where consistent with existing Unit 21 patterns;
- avoid direct `AppDbContext` access.

Register the group through the existing endpoint-mapping composition rather than expanding `Program.cs` with feature logic.

### 7. Automated tests

Add focused tests following Unit 09 and Unit 21 test conventions.

Domain/unit coverage must include at minimum:

- valid creation trims the name and sets active status/order/timestamps correctly;
- blank or invalid names are rejected through the approved layer;
- undefined tracking levels are rejected;
- rename and tracking-level changes update timestamps;
- archived teams reject normal updates;
- active/inactive/archive/restore transitions follow the required state graph;
- restore returns a team to inactive state;
- idempotent lifecycle calls preserve valid state;
- no invalid direct transition bypass is available.

Application/unit coverage must include at minimum:

- normalized duplicate detection on create and rename;
- new-team append order;
- complete-list reorder success;
- duplicate, missing, unknown, and archived reorder IDs fail without partial changes;
- restore appends the restored team to the non-archived order;
- archive/reorder operations leave contiguous non-archived positions.

Integration coverage must include at minimum:

- database migration/model mapping works against the configured test persistence setup;
- first empty startup seeds exactly the six required selections;
- repeated startup does not duplicate or reset seeded/administrator-modified records;
- existing records suppress default seeding;
- unauthenticated requests return `401`;
- authenticated non-admin requests return `403`;
- required-password-change accounts cannot bypass the existing gate;
- admin can list/get/create/update/lifecycle-change/reorder teams;
- default list excludes archived records;
- `includeArchived=true` includes archived records;
- duplicate normalized names return safe `409` responses;
- invalid reorder input leaves the previous order unchanged;
- archived teams cannot be updated until restored;
- responses contain no sensitive or provider-specific data.

Use real application composition and existing integration-test infrastructure. Do not replace meaningful tests with mocks of the endpoint being tested.

### 8. Documentation sync

Update `context/progress-tracker.md` after implementation to record:

- Unit 22 as completed only after all verification succeeds;
- the canonical `Team`, `TeamTrackingLevel`, and `TeamStatus` decisions;
- the fixed V1 tracking levels;
- default seed selections and their levels;
- the restore-to-inactive lifecycle rule;
- the fact that concrete staff team scopes remain deferred to Unit 23;
- any verification limitation or unresolved blocker.

Update other context files only if implementation requires an actual architecture, scope, UI, or code-standard decision change. Do not rewrite documentation merely to restate implementation details already consistent with the context.

## Dependencies

None expected.

Use the EF Core, PostgreSQL, FluentValidation, authentication/authorization, result/error, and test packages already introduced by prior units. Do not add a package for enum handling, ordering, seeding, repositories, or generic CRUD behavior unless an existing documented requirement makes it unavoidable.

## Verification checklist

- [ ] Root `AGENTS.md`, all required context files, Unit 20 authorization patterns, Unit 21 settings patterns, and this spec were reviewed before implementation.
- [ ] Relevant installed backend Codex skills/plugins were used when applicable without overriding project context.
- [ ] A single canonical `Team` aggregate represents FK Velež selections; no duplicate `Selection` entity was introduced.
- [ ] `TeamTrackingLevel` contains exactly `BASIC`, `STANDARD`, and `FULL`.
- [ ] `TeamStatus` contains exactly `ACTIVE`, `INACTIVE`, and `ARCHIVED`.
- [ ] Team names are trimmed, normalized deterministically, and protected by a database unique constraint across active and archived records.
- [ ] New teams default to active and append to the current non-archived display order.
- [ ] Lifecycle transitions and restore-to-inactive behavior match this spec.
- [ ] Archived teams cannot be edited or normally reordered until restored.
- [ ] Reordering requires the complete non-archived team set and succeeds atomically.
- [ ] Successful team ordering is deterministic, unique, contiguous, and zero-based for non-archived records.
- [ ] Empty first-run persistence seeds exactly `First Team`, `U19`, `U17`, `U15`, `U13`, and `U11` with the specified tracking levels and order.
- [ ] Seeding is idempotent and does not modify or supplement an already-populated teams table.
- [ ] EF Core mapping and the new migration contain only intended team-schema changes.
- [ ] API routes are grouped under `/api/settings/teams` and use thin endpoint handlers.
- [ ] All team routes require authentication, pass the required-password-change gate, and enforce `AdminOnly` authorization.
- [ ] Unauthenticated requests return `401` and authenticated non-admin requests return `403` without HTML redirects.
- [ ] Missing resources, validation failures, duplicate names, archived-update conflicts, and unexpected failures use safe established ProblemDetails behavior.
- [ ] No hard-delete endpoint, staff scope, player, match, report, import, training, dashboard, audit-log, or frontend feature was added.
- [ ] No speculative team fields, user-defined tracking-level records, or tracking-level-dependent domain fields were introduced.
- [ ] Domain, Application, Infrastructure, and Api dependencies still follow Clean Architecture direction.
- [ ] Domain, Application, and integration tests cover seed, lifecycle, uniqueness, authorization, ordering, and transaction behavior.
- [ ] `dotnet restore backend/PlayerPerformance.sln` passes.
- [ ] `dotnet build backend/PlayerPerformance.sln --no-restore` passes with no warnings introduced by this unit.
- [ ] `dotnet test backend/PlayerPerformance.sln --no-build` passes.
- [ ] `context/progress-tracker.md` reflects the actual completed implementation and any limitations.
