# Unit 27: Players Backend Foundation

## Goal

Create the first persistent backend foundation for FK Velež player records, including a minimal player profile, safe record lifecycle, administrator-authorized CRUD operations, PostgreSQL persistence, validation, and tests. Players must be modeled as club-level entities that remain stable across future team/selection assignments.

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
9. `context/feature-specs/27-players-backend-foundation.md`

Use relevant installed backend Codex plugins when applicable, but do not let plugin guidance override project context or this spec.

This unit is backend-only. It establishes a persistent club player record and its lifecycle. It must not implement team assignment history, match appearances, medical availability, player media, performance data, imports, or frontend screens.

### Player identity and persistence principle

A player is a persistent FK Velež club entity.

The same player record must later be assignable to U17, U19, the first team, or multiple selections without recreating the player. Team membership is intentionally deferred to Unit 28 through `PlayerTeamAssignment` records.

Do not place `TeamId`, current team, team name, selection history, or a team-owned foreign key directly on the `Player` entity in this unit.

### Minimal player model

Add a `Player` domain entity with only the profile data required for the first backend foundation:

```txt
Player
- Id
- FirstName
- LastName
- PreferredName (optional)
- DateOfBirth (optional, date only)
- Status
- CreatedAtUtc
- UpdatedAtUtc
```

Use a dedicated lifecycle enum named clearly enough not to be confused with medical availability, for example:

```txt
PlayerRecordStatus
- ACTIVE
- INACTIVE
- ARCHIVED
```

Required semantics:

- `ACTIVE` — the player record is active and may be used by future assignment and match workflows.
- `INACTIVE` — the player remains in the club database but is not currently intended for new operational use.
- `ARCHIVED` — the record is historical and read-only until restored.

New players start as `ACTIVE`.

The model must not add speculative fields. Specifically, do not add:

- current team or selection;
- shirt number;
- primary or secondary position;
- nationality;
- place of birth;
- height or weight;
- dominant foot;
- contract information;
- registration or association identifiers;
- email, phone, or address;
- profile image or storage key;
- medical status or injury details;
- notes;
- external vendor identifiers;
- match, training, GPS, or performance statistics.

Those concerns belong to later dedicated units or require explicit product decisions.

### Player names and display behavior

Name rules:

- `FirstName` is required.
- `LastName` is required.
- `PreferredName` is optional.
- Trim surrounding whitespace before persistence.
- Convert an empty or whitespace-only preferred name to `null`.
- Preserve Bosnian and other Unicode characters exactly as entered.
- Do not transliterate names.
- Do not enforce uniqueness on first name, last name, preferred name, or their combination because different people may share the same name.
- Do not reject a player merely because another player has the same name and date of birth.

Expose a server-derived `displayName` in API responses:

- use `PreferredName` when present;
- otherwise use trimmed `FirstName + LastName`.

Do not persist `displayName` as a separate source-of-truth column unless an established project pattern requires a computed/read-model projection.

### Date of birth behavior

`DateOfBirth` is optional because historical or imported player records may initially be incomplete.

When supplied:

- represent it as a date-only value, not a local or UTC timestamp;
- reject future dates;
- do not invent a minimum or maximum player age in this unit;
- do not derive or persist age because age changes over time;
- return the date in an unambiguous ISO date format through the API contract.

### Lifecycle transitions

Player lifecycle changes must use explicit domain methods or Application use cases. Endpoint handlers must not assign status values directly.

Allowed transitions:

```txt
ACTIVE   -> INACTIVE
INACTIVE -> ACTIVE
ACTIVE   -> ARCHIVED
INACTIVE -> ARCHIVED
ARCHIVED -> INACTIVE
```

Required behavior:

- Archiving never physically deletes a player.
- Restoring an archived player returns the record to `INACTIVE`, not automatically to `ACTIVE`.
- Updating profile fields on an archived player returns a safe `409 Conflict` until the player is restored.
- Activating an archived player directly is not allowed; restore first, then activate.
- Deactivating an archived player is not allowed.
- Repeating the same lifecycle command may be idempotent when consistent with existing project conventions, but it must not produce an invalid state.
- Every successful lifecycle change updates `UpdatedAtUtc` through the approved clock abstraction.

No hard-delete endpoint is allowed.

### Authorization model for this unit

Because team assignments do not exist until Unit 28, this unit must not pretend that team-scoped player access can already be calculated safely.

Use the conservative authorization boundary:

- all `/api/players` routes require authentication;
- all routes remain subject to the required-password-change gate from Unit 18;
- all routes require an active staff account and valid staff access profile;
- player list, detail, create, update, activate, deactivate, archive, and restore operations require the canonical `AdminOnly` policy from Unit 20;
- non-admin roles receive `403 Forbidden` regardless of team scope or explicit permission flags.

Unit 28 may introduce team-assignment-aware query access for other staff roles. Do not weaken authorization speculatively in this unit.

Frontend route visibility is not authorization. Backend policy enforcement remains mandatory.

### API surface

Create an authenticated endpoint group:

```txt
/api/players
```

Required endpoints:

```txt
GET   /api/players
GET   /api/players/{playerId}
POST  /api/players
PATCH /api/players/{playerId}
POST  /api/players/{playerId}/activate
POST  /api/players/{playerId}/deactivate
POST  /api/players/{playerId}/archive
POST  /api/players/{playerId}/restore
```

Do not add `DELETE /api/players/{playerId}`.

Equivalent lifecycle route naming is acceptable only when it follows an already established project convention and preserves the same behavior.

### List query behavior

The player list must support a stable server-side contract suitable for the future Unit 29 table.

Supported query parameters:

```txt
search (optional)
status (optional: ACTIVE | INACTIVE | ARCHIVED)
includeArchived (optional boolean, default false)
page (optional, default 1)
pageSize (optional, default 25, maximum 100)
```

Rules:

- Default results exclude `ARCHIVED` records.
- `includeArchived=true` permits archived records to appear.
- A specific `status=ARCHIVED` request is valid only when archived records are included according to the established query convention; normalize this behavior consistently and test it.
- `search` is trimmed and matches first name, last name, preferred name, or derived full name case-insensitively.
- Search must remain Unicode-safe.
- Sort by `LastName`, then `FirstName`, then `Id` as a deterministic tie-breaker.
- Keep filtering and pagination in the database query.
- Do not load the entire player table into memory before filtering or paging.
- Return paging metadata with the result.

Recommended response shape:

```txt
PagedPlayerListResponse
- items
- page
- pageSize
- totalCount
- totalPages
```

### Request contracts

Recommended create request:

```txt
CreatePlayerRequest
- firstName
- lastName
- preferredName (optional)
- dateOfBirth (optional)
```

Recommended update request:

```txt
UpdatePlayerRequest
- firstName
- lastName
- preferredName (optional)
- dateOfBirth (optional)
```

Requirements:

- IDs come from route parameters and are not repeated in request bodies.
- Status, timestamps, and display name are server-owned.
- Create does not accept an initial archived or inactive state.
- Lifecycle state changes use dedicated endpoints, not the profile update request.
- API contracts must not expose EF Core entities.

### Response contracts

Recommended summary response:

```txt
PlayerSummaryResponse
- id
- firstName
- lastName
- preferredName
- displayName
- dateOfBirth
- status
- createdAtUtc
- updatedAtUtc
```

The detail response may use the same shape in this unit because assignments, availability, media, notes, matches, and performance data do not exist yet.

Do not return placeholder arrays or fabricated values for future profile sections.

### Validation and errors

Use FluentValidation and the established Application validation pipeline.

Validation requirements:

- `FirstName` and `LastName` are required after trimming.
- `FirstName`, `LastName`, and `PreferredName` use explicit maximum lengths consistent between validators and EF Core configuration.
- Use a maximum of 100 characters for each name field unless an already established canonical person-name limit exists in the codebase; in that case reuse the existing limit.
- `DateOfBirth` cannot be in the future.
- `page` must be at least 1.
- `pageSize` must be between 1 and 100.
- Unsupported status values and malformed query values return the established safe binding/validation response.
- Stable machine-readable validation keys or error codes must be available for future Bosnian localization.

Expected HTTP behavior:

- `400 Bad Request` for malformed JSON, route binding, or query binding failures;
- `401 Unauthorized` for unauthenticated requests;
- `403 Forbidden` for authenticated users without administrator authorization;
- `404 Not Found` for unknown player IDs;
- `409 Conflict` for invalid lifecycle transitions or profile updates against archived players;
- the project-standard semantic validation response (`422` if already adopted, otherwise the existing consistent status);
- `500 Internal Server Error` only for unexpected failures.

Do not reveal database provider messages, constraint names, SQL, stack traces, Identity internals, or sensitive account data.

### Persistence model and indexing

Add `Player` to the existing Application persistence abstraction and Infrastructure `AppDbContext` through explicit EF Core configuration.

Requirements:

- Follow the existing project ID, table naming, timestamp, enum persistence, and configuration conventions.
- Persist `DateOfBirth` as a PostgreSQL date-compatible value.
- Persist the lifecycle status explicitly and safely.
- Match database maximum lengths to Application validation limits.
- Add indexes that support the defined list behavior, including lifecycle status and deterministic name ordering/search where practical.
- Do not create a unique index on player names.
- Do not add team, assignment, match, media, medical, import, or audit foreign keys.
- Do not seed real or sample players.
- Add one focused migration containing only Unit 27 schema changes.
- Inspect the generated migration for unintended changes to Identity, staff access, settings, teams, seasons, competitions, venues, or opponents.

### Audit readiness

The full audit subsystem is planned for Unit 38.

In this unit:

- do not introduce a second temporary audit table or custom logging framework;
- keep create, update, activate, deactivate, archive, and restore actions inside explicit Application use cases so Unit 38 can add audit recording without moving business logic out of endpoints later;
- do not claim player mutations are fully audited until the audit unit is implemented;
- record this deferred audit coverage accurately in `context/progress-tracker.md` if the existing tracker pattern requires it.

### Scope limits

Do not implement any of the following:

- player-to-team assignment records or history;
- a current-team field on `Player`;
- multiple active assignments;
- player position taxonomy;
- shirt numbers;
- match lineup or appearance records;
- match or training statistics;
- goalkeeper statistics;
- availability or injury records;
- restricted medical notes;
- player notes;
- player images, uploads, or media links;
- CSV/XLSX roster imports;
- Gpexe or Zone14 mappings;
- audit persistence;
- frontend pages, forms, tables, routes, or navigation changes;
- public or player-facing access;
- hard deletion;
- speculative packages or abstractions.

## Implementation

### 1. Create the Players domain model

Add the `Player` entity and dedicated player record status enum in the Domain project.

Implement domain-owned behavior for:

- creation with trimmed required names;
- optional preferred-name normalization;
- optional date-of-birth validation;
- profile updates;
- activation;
- deactivation;
- archive;
- restore to `INACTIVE`;
- UTC timestamp updates through the approved clock pattern.

Keep invariants inside the entity or dedicated domain logic rather than endpoint handlers.

### 2. Add Application contracts and persistence abstraction support

Add the necessary player contracts to the Application layer:

- create/update commands;
- activate/deactivate/archive/restore commands;
- list query with search, status, archived filter, and pagination;
- get-by-ID query;
- request/response DTOs;
- validators;
- persistence queries and mutation abstractions consistent with the existing DbContext/repository approach;
- mapping logic without leaking EF Core entities.

Keep each use case as a focused vertical slice.

### 3. Implement Application authorization-aware use cases

Every player use case must:

- run only for an authenticated active staff account;
- rely on the canonical administrator policy/current-user access infrastructure;
- fail closed for malformed or missing staff access profiles;
- preserve the required-password-change gate;
- return typed Result/error outcomes that map predictably to HTTP responses;
- avoid direct database or authorization logic inside endpoint handlers.

### 4. Add Infrastructure persistence

Add:

- the `Players` DbSet or equivalent persistence registration;
- explicit EF Core entity configuration;
- enum/date/timestamp mapping;
- useful indexes for status and deterministic name queries;
- one focused migration;
- database query implementation that performs filtering, ordering, and pagination server-side.

Do not add seed data or unrelated schema changes.

### 5. Add Minimal API endpoints

Create the `/api/players` endpoint group and map the required list, detail, create, update, and lifecycle routes.

Endpoint handlers must:

- stay thin;
- bind HTTP input;
- delegate to Application use cases;
- use the existing ProblemDetails mapping;
- require the canonical administrator authorization policy at the group level where practical;
- return safe API responses without HTML redirects.

### 6. Add domain and Application tests

Add focused unit tests for:

- required and trimmed first/last names;
- optional preferred-name normalization;
- future date-of-birth rejection;
- duplicate names being allowed;
- new players starting as `ACTIVE`;
- all allowed lifecycle transitions;
- rejected lifecycle transitions;
- restore returning to `INACTIVE`;
- archived profile update rejection;
- display-name behavior.

Add Application-level tests where the existing test architecture supports them for validation, pagination normalization, and authorization-aware outcomes.

### 7. Add integration tests

Add integration coverage for at least:

- unauthenticated requests returning `401`;
- non-admin active users returning `403`;
- administrator create/list/detail/update flow;
- duplicate-name players both being accepted;
- search and status filters;
- default exclusion of archived players;
- explicit archived inclusion;
- deterministic paging behavior;
- activate/deactivate lifecycle;
- archive/restore lifecycle;
- update of archived player returning `409`;
- unknown player ID returning `404`;
- validation failures using the established ProblemDetails contract;
- no physical deletion route being exposed.

Use the existing integration-test database and authentication helpers. Do not introduce Testcontainers or another new testing platform unless the repository already adopted it in a previous unit.

### 8. Update project progress documentation

After implementation and successful verification, update `context/progress-tracker.md` to reflect:

- Unit 27 implementation state;
- the player record fields and lifecycle actually implemented;
- that team assignment history remains for Unit 28;
- that non-admin team-scoped player access remains deferred until assignment data exists;
- that player mutation audit coverage remains deferred to Unit 38;
- any failed verification or necessary deviation from this spec.

Do not mark the unit complete unless required verification passes or failures are explicitly documented.

## Dependencies

None.

Use the existing ASP.NET Core, EF Core/PostgreSQL, FluentValidation, authentication/authorization, Result/ProblemDetails, clock, and test infrastructure already introduced by earlier units. Do not add a new NuGet package unless the current repository is missing a capability explicitly required by this spec and the addition is documented.

## Verification checklist

- [ ] `Player` is a persistent club-level entity and has no direct current-team foreign key.
- [ ] The implemented model contains only the approved minimal profile fields.
- [ ] `PlayerRecordStatus` or an equivalently clear lifecycle enum distinguishes record lifecycle from medical availability.
- [ ] New players start as `ACTIVE`.
- [ ] Restore moves an archived player to `INACTIVE`.
- [ ] Archived players cannot be updated until restored.
- [ ] Player names are trimmed, Unicode-safe, and not incorrectly constrained to be unique.
- [ ] Optional date of birth is stored as a date and future values are rejected.
- [ ] No player age is persisted.
- [ ] `/api/players` exposes list, detail, create, update, activate, deactivate, archive, and restore behavior.
- [ ] No hard-delete player endpoint exists.
- [ ] List filtering, ordering, and pagination execute through the database query.
- [ ] Default list results exclude archived players.
- [ ] API responses do not expose EF Core entities or internal persistence details.
- [ ] All player routes require authentication, an active account, completed required password change, and administrator authorization.
- [ ] Non-admin roles receive `403` in this unit.
- [ ] Validation and lifecycle errors use the established safe ProblemDetails contract.
- [ ] EF Core configuration and one focused Unit 27 migration exist.
- [ ] The migration contains no unintended changes to previously implemented modules.
- [ ] No team assignments, positions, shirt numbers, medical data, notes, media, imports, match data, or frontend UI were added.
- [ ] Domain/Application tests cover player invariants and lifecycle transitions.
- [ ] Integration tests cover authorization, CRUD, filters, paging, lifecycle, validation, and not-found behavior.
- [ ] `dotnet restore backend/PlayerPerformance.sln` passes.
- [ ] `dotnet build backend/PlayerPerformance.sln --no-restore` passes.
- [ ] `dotnet test backend/PlayerPerformance.sln --no-build` passes.
- [ ] `context/progress-tracker.md` reflects the actual Unit 27 outcome and deferred Unit 28/Unit 38 work.
- [ ] No unrelated frontend, authentication, settings, match, import, media, or deployment changes were introduced.
