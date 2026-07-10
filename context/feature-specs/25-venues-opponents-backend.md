# Unit 25: Venues and Opponents Backend

## Goal

Implement the remaining minimal club-settings backend required before match creation: persistent venues and opponents with validation, normalized uniqueness, authenticated administrator management, and archive/restore lifecycle behavior. The completed unit must provide stable identifiers that later match records can reference without introducing match functionality, frontend screens, seed data, or speculative venue/opponent metadata.

## Design

### Scope

This is a backend-only vertical-slice unit in the existing `Settings` module.

It covers two resources:

- venues;
- opponents.

The completed unit must provide:

- `Venue` and `Opponent` domain entities;
- EF Core mappings and one focused migration;
- Application commands, queries, DTOs, validators, and persistence behavior;
- administrator-protected Minimal API endpoint groups under `/api/settings`;
- normalized-name conflict handling backed by database constraints;
- archive and restore operations instead of physical deletion;
- focused unit and integration tests;
- documentation/progress updates required by the project workflow.

This unit does not add:

- frontend settings pages, dialogs, tables, filters, or forms;
- match creation or editing;
- match relationships or navigation collections;
- club, team, player, competition, or season changes;
- public lookup endpoints;
- venue addresses, coordinates, capacity, surface type, country, images, maps, or default-home-venue behavior;
- opponent logos, colors, country, aliases, external provider IDs, league membership, scouting data, or squad data;
- automatic import or synchronization with external providers;
- demo or production seed records;
- hard-delete endpoints;
- audit-log persistence before Unit 38.

Keep the model intentionally small. Add only data confirmed as necessary for later match references.

### Module and architecture boundaries

Venues and opponents belong to the backend `Settings` module defined in `context/architecture.md`.

Use the existing Clean Architecture boundaries:

- `Domain` owns entity state and archive/restore invariants;
- `Application` owns use cases, validation, response contracts, duplicate detection, and persistence abstractions;
- `Infrastructure` owns EF Core mappings, PostgreSQL constraints, migration output, and provider-specific exception interpretation;
- `Api` owns endpoint grouping, authorization application, route binding, and ProblemDetails-compatible HTTP mapping.

Do not place business rules, EF Core queries, normalization rules, duplicate checks, or lifecycle transitions directly in Minimal API handlers.

Follow the established Vertical Slice organization from Unit 21 and Unit 22. Do not introduce a generic CRUD framework, generic repository, MediatR, reflection-based handlers, or a broad reusable settings abstraction solely for these two simple resources.

### Venue model

A venue is a reusable club setting that a later match can reference as the place where the match occurred.

The initial model must contain only:

```txt
Venue
- Id
- Name
- NormalizedName
- IsArchived
- CreatedAtUtc
- UpdatedAtUtc
```

Requirements:

- Use the existing project ID strategy and shared entity conventions.
- `Name` is required, trimmed, and stored in its administrator-entered display form.
- `NormalizedName` is server-owned and never accepted from clients.
- Venue names must be unique using the same deterministic normalization strategy established for seasons, competitions, and selections.
- Database configuration must enforce normalized uniqueness.
- Archived venue names remain reserved.
- Do not add a `City` field merely to anticipate a future need.
- Do not add `IsHomeVenue`, `IsDefault`, `Address`, `Latitude`, `Longitude`, `Capacity`, or `PitchType` in this unit.
- Do not model a relationship to matches until the match backend unit introduces it.
- Do not seed Stadion Rođeni or any other real venue automatically.

If later requirements prove that two real venues must share the same display name, that must be resolved through a documented context/model change rather than weakening uniqueness speculatively now.

### Opponent model

An opponent is a reusable club setting that a later match can reference as the opposing team.

The initial model must contain only:

```txt
Opponent
- Id
- Name
- NormalizedName
- IsArchived
- CreatedAtUtc
- UpdatedAtUtc
```

Requirements:

- Use the existing project ID strategy and shared entity conventions.
- `Name` is required, trimmed, and stored in its administrator-entered display form.
- `NormalizedName` is server-owned and never accepted from clients.
- Opponent names must be unique using the established deterministic normalization strategy.
- Database configuration must enforce normalized uniqueness.
- Archived opponent names remain reserved.
- An opponent is not a `Team`/selection from Unit 22 and must not reuse the internal club-selection entity.
- Do not add abbreviations, logos, colors, country, external IDs, aliases, competition membership, or home stadium fields.
- Do not create player or squad relationships.
- Do not model a relationship to matches until Unit 30.
- Do not seed Bosnian or international clubs.

### Name normalization and uniqueness

Reuse the canonical settings-name normalization behavior already implemented by Unit 21 and Unit 22.

The behavior must:

- trim surrounding whitespace;
- preserve the trimmed display name exactly as entered;
- compare names case-insensitively through a deterministic normalized value;
- use the same internal normalization utility or value pattern already approved by the codebase;
- protect uniqueness with a PostgreSQL unique index/constraint;
- use an application pre-check only for friendly feedback, not as the final authority;
- convert the relevant provider unique-constraint violation into a safe `409 Conflict` response;
- avoid leaking constraint names, SQL text, stack traces, or provider exception details;
- avoid treating unrelated database failures as duplicate-name conflicts.

Venue and opponent namespaces are independent. A venue and an opponent may have the same display name because they are different resources and tables.

Do not add a PostgreSQL extension solely for case-insensitive text if the existing normalized-name strategy already solves the requirement.

### Archive and restore lifecycle

Venues and opponents must use soft lifecycle management rather than physical deletion.

Required behavior:

- No `DELETE` endpoint is added.
- Archive sets `IsArchived = true` and updates `UpdatedAtUtc` using the approved UTC clock pattern.
- Restore sets `IsArchived = false` and updates `UpdatedAtUtc`.
- Archiving an already archived record is idempotent where consistent with the established settings pattern.
- Restoring an active record is idempotent where consistent with the established settings pattern.
- Default list responses exclude archived records.
- `includeArchived=true` includes active and archived records.
- A resource remains retrievable by ID for administrator lifecycle operations even when archived.
- Normal update operations against archived resources return a safe `409 Conflict`; restore before editing.
- Archived records retain their identifiers and names for future reference integrity.
- Future match-reference restrictions must not be guessed in this unit because match relationships do not exist yet.

Do not use a global query filter if it would make archive/restore lookups inconsistent with the existing settings implementation.

### API surface

Create administrator-protected endpoint groups:

```txt
/api/settings/venues
/api/settings/opponents
```

Required route behavior:

```txt
GET    /api/settings/venues
GET    /api/settings/venues/{venueId}
POST   /api/settings/venues
PATCH  /api/settings/venues/{venueId}
POST   /api/settings/venues/{venueId}/archive
POST   /api/settings/venues/{venueId}/restore

GET    /api/settings/opponents
GET    /api/settings/opponents/{opponentId}
POST   /api/settings/opponents
PATCH  /api/settings/opponents/{opponentId}
POST   /api/settings/opponents/{opponentId}/archive
POST   /api/settings/opponents/{opponentId}/restore
```

Equivalent route naming is acceptable only if it follows an already established settings convention and preserves the same behavior.

Every route in this unit must:

- require an authenticated session;
- remain subject to the required-password-change gate from Unit 18;
- require the Unit 20 `AdminOnly` authorization policy;
- derive the actor from the authenticated current-user context;
- return API responses rather than HTML redirects;
- map failures through the existing ProblemDetails-compatible response conventions;
- avoid returning EF Core entities or Identity internals.

This unit keeps settings endpoint groups administrator-only, matching Unit 21 and Unit 22. Later match feature units may reuse Application queries or introduce narrowly authorized lookup contracts for staff workflows without weakening settings administration here.

### Request contracts

Recommended request shapes:

```txt
CreateVenueRequest
- name

UpdateVenueRequest
- name

CreateOpponentRequest
- name

UpdateOpponentRequest
- name
```

Requirements:

- IDs come from route parameters and are not repeated in request bodies.
- `IsArchived`, timestamps, and normalized names are server-owned.
- Archive/restore state changes occur only through dedicated lifecycle operations.
- Request DTOs are API/Application contracts, not persistence entities.
- Do not accept arbitrary metadata dictionaries or future fields.

### Response contracts

Recommended safe response shapes:

```txt
VenueResponse
- id
- name
- isArchived
- createdAtUtc
- updatedAtUtc

OpponentResponse
- id
- name
- isArchived
- createdAtUtc
- updatedAtUtc
```

Requirements:

- Normalized names are internal implementation details and are not returned.
- List responses use the existing settings list shape: plain arrays or the already established project envelope.
- Do not introduce pagination for these expected-small settings lists.
- Sort venues by normalized name ascending and then identifier as a deterministic tie-breaker where needed.
- Sort opponents by normalized name ascending and then identifier as a deterministic tie-breaker where needed.
- Default lists exclude archived records.
- `includeArchived=true` includes all lifecycle states.

### Validation and errors

Reuse FluentValidation and the established Application validation pipeline from Unit 21.

Venue and opponent names must:

- be present after trimming;
- respect the same canonical settings-name maximum length already used for comparable name-only settings records;
- produce stable machine-readable validation keys/error codes suitable for future Bosnian localization;
- not rely on frontend validation for correctness.

Expected HTTP behavior:

- `400 Bad Request` for malformed JSON, invalid route binding, or unsupported query binding;
- `401 Unauthorized` for unauthenticated requests;
- `403 Forbidden` for authenticated users without administrator access;
- `404 Not Found` for missing venue/opponent identifiers;
- `409 Conflict` for normalized-name duplicates, updates to archived records, or equivalent lifecycle conflicts;
- the project-standard validation status (`422` if already adopted, otherwise the established consistent status) for semantic validation failures;
- `500 Internal Server Error` only for unexpected failures.

Do not use English UI prose as the only client signal. Preserve stable error identifiers consistent with the existing Result/ProblemDetails implementation.

### Authorization behavior

Apply the existing canonical `AdminOnly` policy at the endpoint-group level where supported.

Required behavior:

- `ADMIN` may list, read, create, update, archive, and restore venues/opponents.
- Non-admin authenticated roles receive `403` regardless of explicit permission flags.
- `canImportData`, `canVerifyReports`, and `canViewMedicalDetails` do not grant settings administration.
- Missing or malformed staff access profiles fail closed.
- `INVITED`, `DISABLED`, and otherwise non-active accounts cannot use these endpoints.
- Accounts still requiring password change cannot use normal settings endpoints.
- The frontend hiding links is not authorization; backend policy enforcement is mandatory.

Do not duplicate role checks inside each handler if the group-level policy already guarantees them.

### Persistence and migration

Add `Venue` and `Opponent` to the existing Application persistence abstraction and Infrastructure `AppDbContext` using explicit EF Core configurations.

Requirements:

- Follow established table/column naming and ID conventions.
- Match validator and database maximum lengths.
- Configure required display and normalized names.
- Configure one unique normalized-name index per resource.
- Configure archive/timestamp columns consistently with existing settings entities.
- Do not add match foreign keys or navigation collections yet.
- Do not add seed data.
- Add one focused migration containing only Unit 25 schema changes.
- Do not auto-apply destructive migrations in production startup behavior.
- Inspect generated migration SQL/operations for unintended Identity, staff, season, competition, or selection modifications.

### Concurrency and conflict handling

The unit does not require new concurrency tokens.

It must still handle duplicate races safely:

- perform an application pre-check when useful for friendly conflict feedback;
- rely on the database unique constraint as the final authority;
- map only the specific venue/opponent unique violation to the duplicate-name conflict;
- let unrelated provider errors follow the existing unexpected-error pipeline;
- keep mutation save behavior within the approved use-case/DbContext transaction boundary.

### Audit readiness

Do not implement an audit table or parallel event system before Unit 38.

Keep future audit integration possible by:

- using explicit create, update, archive, and restore Application use cases;
- preserving server-owned timestamps;
- resolving the current actor through the existing current-user abstraction;
- avoiding direct entity mutation in API handlers;
- returning stable resource IDs and typed results.

## Implementation

### Required reading

Before implementation, read in order:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/ui-context.md`
5. `context/code-standards.md`
6. `context/ai-workflow-rules.md`
7. `context/progress-tracker.md`
8. `context/feature-specs/00-build-plan.md`
9. `context/feature-specs/21-seasons-competitions-backend.md`
10. `context/feature-specs/22-teams-selections-backend.md`
11. `context/feature-specs/25-venues-opponents-backend.md`

Inspect the final implemented code from Units 08, 09, 12, 13, 18, 20, 21, and 22 before choosing concrete names, folders, base types, validation status codes, and test helpers.

Use relevant installed backend Codex plugins when applicable. Plugins may guide implementation workflow but must not override the project context files, active feature spec, implemented contracts, or architecture boundaries.

### Existing-pattern inspection

Before adding code, confirm and reuse the actual implementation of:

- entity ID and timestamp conventions;
- settings normalized-name utility/value pattern;
- settings archive/restore entity behavior;
- `IClock` or equivalent UTC abstraction;
- `Result`, error code, and ProblemDetails mapping conventions;
- Application persistence abstraction;
- EF Core configuration and migrations assembly paths;
- endpoint group registration and `AdminOnly` application;
- validation pipeline and response status;
- unique-constraint conflict mapping;
- integration-test host/database setup.

Do not create a second normalizer, second clock, duplicate validation pipeline, alternative error mapper, alternate DbContext interface, or parallel authorization constant when an approved implementation already exists.

### Dependencies and package handling

No new package is expected.

Reuse the already installed/project-approved:

- EF Core;
- Npgsql PostgreSQL provider;
- FluentValidation;
- xUnit and integration-test packages;
- existing Result/error and API infrastructure.

Do not add:

- MediatR;
- generic repository packages;
- specification-pattern packages;
- slug/normalization packages;
- geographic/location packages;
- external football-data SDKs;
- AutoMapper or Mapster solely for trivial response mapping;
- new testing frameworks.

If implementation reveals that a package is genuinely missing despite prior units, stop and document the mismatch in `context/progress-tracker.md` rather than installing a speculative dependency silently.

### Domain entities

Add `Venue` and `Opponent` entities in the Domain Settings area using the current entity encapsulation conventions.

Each entity must support explicit behavior equivalent to:

- create with canonical display and normalized name;
- update name while active;
- archive;
- restore;
- timestamp maintenance through the approved clock/use-case mechanism.

Keep setters appropriately encapsulated and EF-compatible. Do not expose broad public setters solely for persistence or test convenience.

Use separate entity types and behavior. Do not combine both into a generic `NamedSetting` persisted hierarchy unless that abstraction already exists and is demonstrably the established project pattern.

### Application vertical slices

Create focused slices equivalent to:

```txt
Settings/Venues/ListVenues
Settings/Venues/GetVenueById
Settings/Venues/CreateVenue
Settings/Venues/UpdateVenue
Settings/Venues/ArchiveVenue
Settings/Venues/RestoreVenue

Settings/Opponents/ListOpponents
Settings/Opponents/GetOpponentById
Settings/Opponents/CreateOpponent
Settings/Opponents/UpdateOpponent
Settings/Opponents/ArchiveOpponent
Settings/Opponents/RestoreOpponent
```

Exact folder/file names may follow the codebase's implemented Vertical Slice conventions.

Each mutation must:

1. receive a typed command/request;
2. execute Application validation;
3. trim and normalize the name through the canonical settings behavior;
4. perform a friendly duplicate check where useful;
5. load or create the correct domain entity;
6. invoke explicit domain lifecycle behavior;
7. persist through the approved Application abstraction;
8. handle the database uniqueness race through established infrastructure/error mapping;
9. return a typed Result/DTO.

Do not implement one reflection-driven generic handler for both resources.

### Validators

Create validators near the owning Application slices or reuse an established shared name validator only when it already exists and keeps error codes/resource ownership clear.

Requirements:

- Trim-aware required validation.
- Maximum lengths match EF configurations.
- Error codes/keys are stable and English internally.
- Endpoint handlers do not repeat semantic validation.
- Tests cover validation through the real application/API pipeline.

### Persistence abstraction and DbContext

Extend the existing Application persistence abstraction with the narrow additions required for venues and opponents.

Use the same approach already used for seasons, competitions, and selections. Do not expose provider-specific EF Core types outside Infrastructure if the existing abstraction avoids them.

Register both entity configurations through the established Infrastructure mechanism.

### EF Core configuration and migration

Create explicit EF Core configurations for both entities.

Verify:

- correct table naming;
- required IDs and timestamps;
- required `Name` and `NormalizedName`;
- matching maximum lengths;
- unique normalized-name indexes;
- safe default archive state;
- no unintended relationships or cascade rules;
- no seed operations;
- migration contains only intended venue/opponent schema additions.

Generate the migration through the existing solution's documented startup project, target project, context, and output location.

### Endpoint groups

Map both endpoint groups through the existing API registration approach.

Requirements:

- Apply `AdminOnly` at group level where supported.
- Keep route handlers thin.
- Parse public IDs through the established route-binding convention.
- Delegate all behavior to Application use cases.
- Return `201 Created` with a useful location for successful creation when consistent with current API behavior.
- Use the same success response convention as Unit 21/22 for update/archive/restore.
- Map Result failures through the shared ProblemDetails mapper.
- Do not return EF entities.
- Do not add unrelated OpenAPI packages or endpoint metadata systems.

### Unit tests

Use the existing UnitTests project and conventions.

Minimum focused coverage for each resource:

- creation stores trimmed display name and canonical normalized name;
- blank/whitespace name fails validation;
- excessive name length fails validation;
- active name update succeeds and refreshes update time;
- archived resource cannot be normally updated;
- archive transitions active to archived;
- repeated archive is idempotent according to the established settings convention;
- restore transitions archived to active;
- repeated restore is idempotent according to the established settings convention.

Also cover the normalized-name behavior through the shared normalizer tests if the implementation already centralizes it. Do not duplicate identical low-value test matrices unnecessarily; keep resource-specific lifecycle coverage clear.

### Integration tests

Use the existing IntegrationTests application factory and database isolation strategy.

Minimum API coverage:

#### Authorization

- unauthenticated list and mutation requests return `401`;
- authenticated non-admin requests return `403`;
- active administrator requests are allowed;
- required-password-change sessions are blocked from normal settings endpoints.

#### Venue behavior

- administrator can create a venue;
- created response does not expose normalized/internal fields;
- default list includes the active venue;
- list sorting is deterministic;
- duplicate normalized names return `409`;
- concurrent/database-enforced duplicate path maps safely where practical;
- administrator can update an active venue;
- missing ID returns `404`;
- archive removes the venue from the default list;
- `includeArchived=true` returns the archived venue;
- updating archived venue returns `409`;
- restore returns it to the default list;
- invalid name returns the project-standard validation response.

#### Opponent behavior

- administrator can create an opponent;
- created response does not expose normalized/internal fields;
- default list includes the active opponent;
- list sorting is deterministic;
- duplicate normalized names return `409`;
- administrator can update an active opponent;
- missing ID returns `404`;
- archive removes the opponent from the default list;
- `includeArchived=true` returns the archived opponent;
- updating archived opponent returns `409`;
- restore returns it to the default list;
- invalid name returns the project-standard validation response.

#### Independence

- the same display name can exist once as a venue and once as an opponent because uniqueness is resource-specific.

Tests must not depend on fixed shared database order, real production records, or external services.

### Documentation synchronization

After successful implementation and verification:

- update `context/progress-tracker.md` with Unit 25 as completed;
- record Unit 26 as the next planned unit;
- document any actual migration name, relevant implementation decision, or verification limitation;
- update architecture/code standards only if implementation truly changes those project-level decisions;
- do not rewrite context files merely to restate implementation details already covered by this spec.

## Dependencies

None.

All required runtime and test packages should already exist from Units 09, 12, and 21.

## Verification checklist

- [ ] `AGENTS.md`, all six context files, the build plan, Unit 21, Unit 22, and this spec were read before implementation.
- [ ] Relevant installed backend Codex plugins were used when applicable without overriding project context or scope.
- [ ] The implementation remains backend-only.
- [ ] No frontend settings UI or routing changes were added.
- [ ] No match, player, team-scope, import, media, GPS, medical, or dashboard behavior was added.
- [ ] No speculative venue metadata, opponent metadata, external IDs, logos, addresses, coordinates, or provider integrations were introduced.
- [ ] `Venue` contains only the approved identifier, name/normalized name, archive state, and timestamps.
- [ ] `Opponent` contains only the approved identifier, name/normalized name, archive state, and timestamps.
- [ ] Opponents are not modeled as internal FK Velež `Team`/selection entities.
- [ ] Display names are trimmed and preserved in user-entered form.
- [ ] Canonical settings-name normalization is reused instead of duplicated.
- [ ] Venue normalized names have a database-enforced unique constraint.
- [ ] Opponent normalized names have a database-enforced unique constraint.
- [ ] Archived names remain reserved.
- [ ] The same name may exist independently in the venue and opponent namespaces.
- [ ] No hard-delete routes or persistence operations were added.
- [ ] Archive and restore use explicit Application/domain behavior.
- [ ] Default lists exclude archived records.
- [ ] `includeArchived=true` includes archived records.
- [ ] Archived records cannot be normally updated until restored.
- [ ] API route groups exist under the established `/api/settings` pattern.
- [ ] All Unit 25 routes require authentication and the canonical `AdminOnly` policy.
- [ ] Required-password-change accounts cannot access normal Unit 25 endpoints.
- [ ] Non-admin permission flags do not grant settings administration.
- [ ] Endpoint handlers remain thin and contain no EF Core queries or business rules.
- [ ] Requests and responses do not expose normalized fields, EF entities, Identity internals, or provider errors.
- [ ] Validation uses the existing FluentValidation/Application pipeline.
- [ ] Validator and database maximum lengths match.
- [ ] Stable error codes/keys are available for future localized frontend messages.
- [ ] Duplicate conflicts return a safe `409` for both pre-check and database-race paths.
- [ ] Missing resources return `404`.
- [ ] Unauthenticated requests return `401` and unauthorized authenticated requests return `403`.
- [ ] One focused EF Core migration was generated and inspected.
- [ ] The migration contains no unintended changes to existing tables, Identity, staff access, seasons, competitions, or selections.
- [ ] No venue or opponent seed records were added.
- [ ] Domain/unit tests cover validation and lifecycle behavior.
- [ ] Integration tests cover authorization, CRUD-like operations, conflicts, archive/restore behavior, list filtering, and safe response contracts.
- [ ] Tests use isolated test data and no external services.
- [ ] Clean Architecture dependency direction remains valid.
- [ ] No new package was installed unless a documented pre-existing dependency mismatch required it.
- [ ] `dotnet restore backend/PlayerPerformance.sln` passes.
- [ ] `dotnet build backend/PlayerPerformance.sln --no-restore` passes with no new warnings introduced by this unit.
- [ ] `dotnet test backend/PlayerPerformance.sln --no-build` passes.
- [ ] `git diff --check` passes.
- [ ] `context/progress-tracker.md` reflects the actual implementation and verification result.
- [ ] No unrelated files or features were changed.
