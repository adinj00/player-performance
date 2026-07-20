# Unit 21: Seasons and Competitions Backend

## Goal

Implement the first admin-managed settings backend for seasons and competitions, including persistence, validation, authenticated `AdminOnly` API endpoints, and archive/restore lifecycle behavior. This unit must provide stable settings records for later teams, matches, filters, and reporting without adding frontend screens or unrelated club configuration.

## Design

### Scope

This is a backend-only vertical-slice unit covering two settings resources:

- seasons;
- competitions.

The completed unit must provide:

- domain entities and lifecycle behavior;
- EF Core mappings and a migration;
- Application commands, queries, DTOs, and validators;
- Minimal API endpoint groups under `/api/settings`;
- `AdminOnly` authorization from Unit 20;
- ProblemDetails-compatible errors using existing API conventions;
- focused unit and integration tests;
- progress-tracker updates.

This unit does not add:

- frontend settings pages or forms;
- teams/selections or tracking levels;
- venues or opponents;
- matches or season/competition usage by matches;
- staff CRUD, invitations, or team scopes;
- audit-log persistence;
- localization infrastructure;
- sample/demo settings records;
- public settings endpoints;
- physical deletion endpoints.

### Resource ownership and boundaries

Seasons and competitions belong to the backend `Settings` module defined in `context/architecture.md`.

Use the existing Clean Architecture boundaries:

- `Domain` owns entity invariants and archive/restore behavior;
- `Application` owns use cases, contracts, validation, and persistence abstractions;
- `Infrastructure` owns EF Core persistence, mappings, uniqueness constraints, and migrations;
- `Api` owns Minimal API route mapping, HTTP request/response concerns, authorization application, and ProblemDetails conversion.

Do not place EF Core queries, validation rules, duplicate detection, or archive logic directly in endpoint handlers.

Follow the existing Vertical Slice organization. Do not introduce a generic repository, MediatR, a broad CRUD framework, or a settings service abstraction solely to reduce file count. Reuse established project patterns when they already exist.

### Season model

A season represents a club reporting and competition period that later teams, matches, dashboards, and filters can reference.

The model must include only the fields needed by the confirmed scope:

```txt
Season
- Id
- Name
- StartDate
- EndDate
- IsArchived
- CreatedAtUtc
- UpdatedAtUtc
```

Requirements:

- Use the existing project ID strategy and entity base conventions.
- `Name` is required, trimmed, and stored in its user-entered display form.
- `StartDate` and `EndDate` use a date-only representation where supported by the current stack and conventions.
- `StartDate` must not be later than `EndDate`.
- Season names must be unique using a deterministic normalized comparison.
- Uniqueness must be protected by a database constraint, not only by a pre-query.
- Do not add an `IsCurrent`, `IsDefault`, or automatic-current-season flag in this unit.
- Do not prohibit overlapping season date ranges because that product rule is not documented.
- Do not add match, team, or competition navigation collections before those relationships are implemented by later units.

Example valid display names may include `2026/27` or `2026`, but the backend must not force one naming format beyond required length and uniqueness rules.

### Competition model

A competition is a reusable club setting that later matches can reference across seasons.

The model must include only:

```txt
Competition
- Id
- Name
- IsArchived
- CreatedAtUtc
- UpdatedAtUtc
```

Requirements:

- Use the existing project ID strategy and entity base conventions.
- `Name` is required, trimmed, and stored in its user-entered display form.
- Competition names must be unique using a deterministic normalized comparison.
- Uniqueness must be protected by a database constraint.
- Do not add season-specific competition rows, a season/competition join table, governing-body metadata, competition type, format, country, logo, abbreviation, or external IDs.
- Do not seed Bosnian league or cup names. Administrators will configure real records later.

### Name normalization and uniqueness

Season and competition display names remain exactly the administrator-entered domain data after trimming. Duplicate detection must nevertheless be case-insensitive and resilient to insignificant surrounding whitespace.

Use either:

- an existing approved normalized-name pattern already present in the codebase; or
- a deterministic normalized value maintained by the entity/Application layer and protected with a unique index.

Requirements:

- Do not rely only on application-level `AnyAsync` checks because concurrent requests can race.
- Translate unique-constraint failures into a safe `409 Conflict` ProblemDetails response.
- Do not expose database constraint names or provider exceptions to clients.
- A name used by an archived record remains reserved. Restoring or recreating a duplicate must not create two logically identical records.
- Do not introduce PostgreSQL extensions solely for case-insensitive text unless the existing persistence setup already uses and approves them.

### Archive and restore behavior

Settings records must be archived instead of physically deleted.

Requirements:

- No hard-delete API endpoint is added.
- Archive marks `IsArchived = true` and updates the entity timestamp using the approved clock abstraction.
- Restore marks `IsArchived = false` and updates the timestamp.
- Repeating archive on an archived record and restore on an active record should be idempotent: return the current resource without creating an error or duplicate write where practical.
- Archived records are excluded from default list responses.
- Administrators can request archived records through an explicit `includeArchived` query parameter.
- Archived records remain available by identifier for administrative lifecycle operations.
- Archived records are read-only for normal update operations; restore them before changing their editable fields.
- Attempting to update an archived record returns a safe `409 Conflict`.
- Future units may add reference-aware restrictions, but this unit must not guess relationships that do not exist yet.

This archive behavior preserves stable identifiers for future match and reporting references.

### API contract

Create two admin-protected endpoint groups:

```txt
/api/settings/seasons
/api/settings/competitions
```

Recommended routes:

```txt
GET    /api/settings/seasons
GET    /api/settings/seasons/{seasonId}
POST   /api/settings/seasons
PATCH  /api/settings/seasons/{seasonId}
POST   /api/settings/seasons/{seasonId}/archive
POST   /api/settings/seasons/{seasonId}/restore

GET    /api/settings/competitions
GET    /api/settings/competitions/{competitionId}
POST   /api/settings/competitions
PATCH  /api/settings/competitions/{competitionId}
POST   /api/settings/competitions/{competitionId}/archive
POST   /api/settings/competitions/{competitionId}/restore
```

Equivalent route shapes are allowed only when they follow existing project conventions and preserve the same behavior.

All routes must:

- require authentication;
- remain subject to the Unit 18 required-password-change gate;
- require the Unit 20 `AdminOnly` policy;
- return API responses rather than HTML redirects;
- use existing ProblemDetails behavior for errors;
- ignore any client-supplied actor/user identifier and derive authorization from the current authenticated user.

Do not expose a public list endpoint in this unit. Later authorized feature modules can reuse Application queries without weakening settings administration.

### Request contracts

Recommended request shapes:

```txt
CreateSeasonRequest
- name
- startDate
- endDate

UpdateSeasonRequest
- name
- startDate
- endDate

CreateCompetitionRequest
- name

UpdateCompetitionRequest
- name
```

Requirements:

- Request DTOs remain API/Application contracts, not EF entities.
- IDs come from route parameters, not duplicated request-body fields.
- Archive state is changed only through archive/restore operations, not arbitrary create/update payloads.
- Created/updated timestamps are server-owned.
- Normalized names are server-owned and never accepted from clients.
- Unknown JSON properties follow the existing API serializer behavior; do not add a second serializer configuration only for this unit.

### Response contracts

Recommended safe response shapes:

```txt
SeasonResponse
- id
- name
- startDate
- endDate
- isArchived
- createdAtUtc
- updatedAtUtc

CompetitionResponse
- id
- name
- isArchived
- createdAtUtc
- updatedAtUtc
```

List responses may be plain arrays or the existing project list envelope if one has already been established. Do not introduce pagination abstractions for these expected-small settings lists.

Sorting must be deterministic:

- seasons: `StartDate` descending, then `Name` ascending;
- competitions: `Name` ascending using the normalized comparison, then identifier as a stable tie-breaker if needed.

The default list excludes archived records. `includeArchived=true` includes both active and archived records without changing authorization.

### Validation and error behavior

This is the first real settings mutation unit, so backend validation must be explicit and reusable.

Use FluentValidation in the Application layer unless it is already installed and configured through an equivalent approved pattern. Do not add frontend validation in this unit.

Minimum validation:

#### Season

- name is required after trimming;
- name respects a documented, reasonable maximum length consistent with database configuration;
- start date is required;
- end date is required;
- start date is not later than end date.

#### Competition

- name is required after trimming;
- name respects a documented, reasonable maximum length consistent with database configuration.

Use existing error conventions. Expected behavior:

- `400 Bad Request` for malformed JSON or invalid route binding;
- `401 Unauthorized` for unauthenticated requests;
- `403 Forbidden` for authenticated non-admin users;
- `404 Not Found` for missing season or competition IDs;
- `409 Conflict` for duplicate normalized names, updating archived records, or equivalent lifecycle conflicts;
- `422 Unprocessable Entity` for semantic validation failures if the existing project already adopted `422`; otherwise use the established validation status consistently;
- `500` only for unexpected failures.

Validation responses must be safe for the future Bosnian frontend to map to localized messages. Backend code identifiers and error codes remain English. Do not hardcode English UI prose as the only machine-readable signal; include stable error codes or structured validation keys consistent with existing error abstractions.

### Authorization behavior

Apply the canonical Unit 20 `AdminOnly` policy to both endpoint groups.

Requirements:

- `ADMIN` can list, read, create, update, archive, and restore.
- Other authenticated roles receive `403` regardless of explicit permission flags.
- Explicit permissions such as `canImportData` do not grant settings administration.
- Missing access profiles fail closed.
- Unauthenticated requests receive `401`.
- Accounts requiring initial password change cannot access normal settings endpoints until the password-change requirement is cleared.
- Do not duplicate role checks inside every handler when the centralized policy already enforces them.
- Backend authorization is authoritative; no frontend visibility exists in this unit.

### Persistence and migration

Add both entities to the existing EF Core DbContext through Infrastructure-owned configuration.

Requirements:

- Use explicit table and column mappings consistent with existing conventions.
- Configure required fields and maximum lengths consistently with validators.
- Configure unique constraints/indexes for normalized names.
- Configure date-only persistence correctly for PostgreSQL.
- Configure `IsArchived` with a safe active default.
- Do not use a global query filter that would make archive/restore lookup error-prone unless the project already has an explicit, well-tested archived-record pattern.
- Add one migration for the Unit 21 settings schema unless the current migration workflow requires separate migrations.
- Do not auto-apply destructive migrations at startup.
- Do not add seed data.

### Concurrency and duplicate handling

The unit does not require optimistic concurrency tokens, but it must handle common races safely.

Requirements:

- Perform a friendly duplicate pre-check where useful for clear feedback.
- Still rely on the unique database constraint as the final authority.
- Convert provider-specific unique violations into the same safe conflict result as the pre-check.
- Do not swallow unrelated database exceptions as duplicate-name errors.
- Keep update and archive/restore operations transactional through the existing DbContext/use-case boundary.

### Audit readiness

Unit 38 will add the audit backend foundation. Do not implement a parallel audit log now.

Prepare for later audit integration by:

- keeping create/update/archive/restore as explicit Application use cases;
- preserving server-owned created/updated timestamps;
- deriving the current actor through existing access context where needed;
- avoiding direct entity mutation in API handlers.

Do not add unused audit tables, event buses, or domain-event infrastructure solely for future work.

## Implementation

### Required reading

Before implementation, read:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/ui-context.md`
5. `context/code-standards.md`
6. `context/ai-workflow-rules.md`
7. `context/progress-tracker.md`
8. `context/feature-specs/00-build-plan.md`
9. `context/feature-specs/21-seasons-competitions-backend.md`

Inspect the final implemented output from Units 08, 09, 12, 13, 14, 15, 18, and 20 before choosing concrete class names and extension points.

Use relevant project-local skills from `.agents/skills/` when applicable. Skills may guide workflow but must not override project context, the active feature spec, existing architecture, or implemented contracts.

### Existing-contract inspection

Before adding code, confirm the current implementation of:

- entity ID and timestamp base conventions;
- `IClock` or equivalent UTC time abstraction;
- `Result`/error contracts and HTTP result mapping;
- DbContext interface/abstraction available to Application;
- EF Core configuration and migration assembly paths;
- endpoint-group registration pattern;
- ProblemDetails and validation error response format;
- current-user access abstraction;
- Unit 20 authorization policy names/constants;
- integration-test application factory and database strategy.

Extend these patterns. Do not create parallel error mappers, duplicate clocks, a second DbContext abstraction, or alternate authorization constants.

### Dependency setup

If FluentValidation is not already installed, add only the packages needed for Application-layer validators and DI registration, normally:

```txt
FluentValidation
FluentValidation.DependencyInjectionExtensions
```

Keep package versions compatible with .NET 8 and the existing central/package management approach.

Do not add:

- MediatR;
- AutoMapper or Mapster solely for these simple mappings;
- generic repository packages;
- specification-pattern packages;
- date/time libraries when `DateOnly` and the existing clock are sufficient;
- slug/normalization packages for simple deterministic name normalization.

### Domain entities

Add `Season` and `Competition` to the Domain layer using existing entity and result conventions.

Provide explicit creation and state-change methods where consistent with the current domain style.

Season behavior must cover:

- validated creation inputs supplied by the Application use case;
- update of name and date range;
- archive;
- restore;
- timestamp updates through the approved clock/use-case mechanism.

Competition behavior must cover:

- validated creation name;
- name update;
- archive;
- restore;
- timestamp updates.

Do not make entity setters broadly public solely to simplify EF or tests. Use EF-compatible encapsulation consistent with existing project conventions.

### Application vertical slices

Create focused slices for seasons and competitions. Recommended responsibilities:

```txt
Settings/Seasons/ListSeasons
Settings/Seasons/GetSeasonById
Settings/Seasons/CreateSeason
Settings/Seasons/UpdateSeason
Settings/Seasons/ArchiveSeason
Settings/Seasons/RestoreSeason

Settings/Competitions/ListCompetitions
Settings/Competitions/GetCompetitionById
Settings/Competitions/CreateCompetition
Settings/Competitions/UpdateCompetition
Settings/Competitions/ArchiveCompetition
Settings/Competitions/RestoreCompetition
```

Exact folder names may follow established Vertical Slice conventions.

Each mutation use case must:

1. receive a typed command/request;
2. run Application validation;
3. normalize/trim names through one shared deterministic rule;
4. check expected conflicts for friendly feedback;
5. load or create the domain entity;
6. invoke explicit domain behavior;
7. persist through the approved Application abstraction;
8. handle unique-constraint races safely through Infrastructure/API error mapping;
9. return a typed result/DTO.

Do not combine season and competition operations into one generic reflection-driven CRUD handler.

### Validation registration

Register validators through the existing `AddApplication()` extension or equivalent Application composition root.

Requirements:

- Validators live in Application near the owning use case.
- API handlers do not duplicate validation rules.
- Database maximum lengths and validator maximum lengths match.
- Validation is exercised through integration tests, not only direct validator unit tests.
- Do not introduce automatic endpoint scanning that changes unrelated endpoint behavior without tests.

### Persistence abstractions

Use the current Application persistence abstraction if Unit 12 already introduced one. If none exists, add the narrowest abstraction required without exposing EF Core types to Domain or Api.

Acceptable approaches include:

- extending an existing `IApplicationDbContext` with `Seasons` and `Competitions` queryable sets plus save behavior;
- adding focused settings repositories only if repository use is already an approved existing pattern.

Do not expose `DbContext`, `DbSet`, `EntityEntry`, or provider exceptions through Application contracts.

### EF Core configurations and migration

Create explicit Infrastructure configurations for both entities.

Verify:

- table naming follows existing conventions;
- IDs use the existing strategy;
- display and normalized names have appropriate maximum lengths;
- normalized names are uniquely indexed;
- season dates are required;
- archive and timestamp fields are required where appropriate;
- no accidental cascade relationships are introduced;
- migration output contains only intended Unit 21 schema changes.

Generate the migration using the established startup project, target project, context, and output path. Do not invent a second migrations location.

### Endpoint groups

Map settings endpoints through the existing API endpoint registration pattern.

Requirements:

- use separate, readable season and competition route groups under `/api/settings`;
- apply `AdminOnly` once at the group level where supported;
- keep endpoint lambdas/handlers thin;
- parse route IDs safely;
- map Application results to existing ProblemDetails-compatible responses;
- return `201 Created` with a useful location for successful creation where consistent with current API conventions;
- return successful update/archive/restore resources or the established no-content response consistently;
- do not return EF entities;
- do not add Swagger/OpenAPI packages if they are not already part of the project.

### Tests

Use the existing Unit 09 backend test infrastructure.

#### Domain/unit coverage

Minimum focused coverage:

- season date range rejects start after end;
- season update preserves valid invariants;
- season archive and restore behavior works and is idempotent;
- competition archive and restore behavior works and is idempotent;
- normalization treats case/outer whitespace consistently;
- archived records cannot be updated until restored;
- validator maximum lengths and required fields behave as specified.

#### Integration coverage

Minimum API/persistence coverage for both resources:

- unauthenticated requests return `401`;
- authenticated non-admin requests return `403`;
- admin can create a valid record;
- create returns the expected response contract;
- list excludes archived records by default;
- `includeArchived=true` includes archived records;
- get-by-id returns the expected resource;
- missing IDs return `404`;
- update persists valid changes;
- invalid season date range returns the established validation response;
- missing/blank names return the established validation response;
- duplicate names differing only by case or surrounding whitespace return `409`;
- concurrent/database-level duplicate violations are translated safely where practical to test;
- archive removes the record from default lists without deleting it;
- archived records reject normal update with `409`;
- restore returns the record to default lists;
- repeated archive/restore remains safe;
- required-password-change accounts cannot access the settings endpoints;
- migration/schema constraints are exercised by the integration database strategy;
- no response leaks normalized-name fields or provider details.

Existing health, architecture, auth, CSRF, login, password-change, first-admin, authorization, and error-handling tests must continue to pass.

Do not create production demo endpoints or developer-specific test credentials.

### Documentation updates

Update `context/progress-tracker.md` during implementation:

- mark Unit 21 in progress when work begins;
- mark it complete only after migration and verification succeed;
- record the final Season and Competition fields;
- record exact API routes;
- record normalization/uniqueness strategy;
- record archive/restore semantics;
- record FluentValidation packages if newly introduced;
- record the migration name;
- record any database-dependent verification limitation.

Update `context/architecture.md` or `context/code-standards.md` only if implementation changes a project-level decision or boundary. Do not update them merely to copy this spec.

## Dependencies

Add only if not already installed:

- `FluentValidation` — Application-layer request/command validation.
- `FluentValidation.DependencyInjectionExtensions` — validator discovery and DI registration.

Use the existing ASP.NET Core, EF Core, Npgsql/PostgreSQL, Identity/auth, authorization, ProblemDetails, domain primitive, configuration, and test packages from previous units.

Do not add MediatR, generic repository libraries, mapping libraries, date/time libraries, frontend packages, audit packages, seed-data packages, or settings UI dependencies.

## Verification checklist

- [ ] `AGENTS.md`, all required context files, the current build plan, and this feature spec were read before implementation.
- [ ] Relevant project-local skills from `.agents/skills/` were used when applicable without overriding project context or the active spec.
- [ ] `Season` exists with only the approved ID, name, date range, archive, and timestamp fields.
- [ ] `Competition` exists with only the approved ID, name, archive, and timestamp fields.
- [ ] No `IsCurrent`, default-season, season/competition join, team, match, venue, opponent, or sample-data behavior was introduced.
- [ ] Season date validation rejects a start date later than the end date.
- [ ] Display names are trimmed and duplicate detection is case-insensitive through one deterministic normalization strategy.
- [ ] Unique database constraints protect normalized season and competition names.
- [ ] Unique-constraint races return safe `409 Conflict` responses without leaking provider details.
- [ ] Create, read, update, archive, and restore use cases exist as focused Application slices.
- [ ] Hard-delete endpoints do not exist.
- [ ] Archived records are excluded from default lists and included only when explicitly requested.
- [ ] Archived records cannot be normally updated until restored.
- [ ] Archive and restore operations are safe and idempotent.
- [ ] Season lists sort by start date descending and competition lists sort predictably by name.
- [ ] API responses use DTOs and do not expose EF entities, normalized names, or security data.
- [ ] All settings routes require authentication, the cleared password-change requirement, and the centralized `AdminOnly` policy.
- [ ] Authenticated non-admin roles and missing access profiles fail closed with the established `403` behavior.
- [ ] Application and Domain do not depend on EF Core, ASP.NET Core, Identity, or HTTP context.
- [ ] Endpoint handlers remain thin and contain no EF queries or duplicated business validation.
- [ ] FluentValidation is installed/configured only if it was not already present.
- [ ] Validator limits and EF Core column limits match.
- [ ] EF Core mappings and the Unit 21 migration contain only intended season/competition schema changes.
- [ ] The migration applies successfully to the configured development database.
- [ ] Focused domain/validator tests pass.
- [ ] Season and competition API integration tests pass for authentication, authorization, validation, uniqueness, lifecycle, and persistence behavior.
- [ ] Existing backend tests continue to pass.
- [ ] `dotnet restore backend/PlayerPerformance.sln` passes.
- [ ] `dotnet build backend/PlayerPerformance.sln` passes with no errors.
- [ ] `dotnet test backend/PlayerPerformance.sln` passes.
- [ ] No frontend code, UI, routes, packages, or localization resources were changed.
- [ ] No staff CRUD, team scope, teams/selections, venues, opponents, matches, audit log, or seed data was added.
- [ ] `context/progress-tracker.md` reflects the actual Unit 21 implementation, migration, routes, validation, and archive decisions.
