# Unit 13: Backend Domain Shared Primitives

## Goal

Add a small, framework-independent set of shared backend domain and application primitives that future domain modules can reuse consistently. This unit must establish reusable building blocks only; it must not introduce FK Velež business entities, persistence models, API endpoints, authentication behavior, or database tables.

## Design

This unit strengthens the backend foundation before implementing authentication, staff access, club configuration, players, matches, reports, imports, audit, or media. The shared primitives should make future backend units more consistent without creating a speculative framework.

The design must follow the existing Clean Architecture direction:

- `Domain` remains independent and must not reference `Application`, `Infrastructure`, `Api`, EF Core, ASP.NET Core, or external providers.
- `Application` may reference `Domain` and may define use-case-facing abstractions.
- `Infrastructure` may implement abstractions defined by `Application` or `Domain` when required.
- `Api` wires dependencies only through existing dependency injection extension points.

The primitives must be intentionally small and boring. Prefer clear records, interfaces, and simple base types over complex inheritance trees or generic frameworks.

No product-specific behavior belongs in this unit. Do not create players, teams, users, roles, seasons, matches, reports, import jobs, media assets, audit logs, or availability records yet.

Do not add EF Core entity configurations, migrations, tables, database seed data, Identity models, cookies, authorization policies, or API routes in this unit.

If a primitive is not immediately useful for the planned backend architecture, do not add it.

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
8. `context/feature-specs/13-backend-domain-shared-primitives.md`

### Domain primitive folder structure

Create a clear shared folder structure inside the backend `Domain` project.

Recommended structure:

```txt
backend/src/Domain/
├── Common/
│   ├── Entities/
│   ├── Errors/
│   ├── Results/
│   └── ValueObjects/
└── Abstractions/
```

Keep names aligned with the actual implemented files. Do not create empty folders unless the project convention allows them to be committed.

### Entity base patterns

Add minimal entity base patterns for future domain entities.

Requirements:

- Support entity identity without depending on EF Core attributes or persistence concerns.
- Use `Guid` as the default identity type for normal aggregate/entity records unless a later feature spec defines otherwise.
- Keep identity initialization explicit and testable.
- Implement equality by identity for matching concrete entity types.
- Avoid business-specific properties such as player name, team name, report status, or user role.

Recommended shape:

- An abstract generic entity base such as `Entity<TId>`.
- An optional `Entity` convenience base for `Guid` identities if it simplifies future entity code.

Do not add database annotations, table names, navigation properties, EF Core configuration, or persistence-specific constructors.

### Value object base pattern

Add a minimal value object pattern only if it is simple and useful for future domain modeling.

Requirements:

- Value object equality must be based on value components, not reference equality.
- It must not depend on EF Core, JSON serialization, ASP.NET Core, or external packages.
- Keep implementation small enough that future value objects can use it without ceremony.

If the implementation would become complex or speculative, skip the base class and document that value objects will be introduced when the first real value object is added.

### Error and result primitives

Add small result primitives for domain/application operations that should return expected failures without throwing exceptions for normal business outcomes.

Requirements:

- Add an `Error` type with at least:
  - a stable machine-readable `Code`
  - a human-readable `Message`
- Add `Result` and `Result<T>` types or equivalent simple success/failure wrappers.
- Prevent invalid result states, such as successful results with errors or failed results with no error.
- Keep messages developer-oriented for now. Do not introduce user-facing localization resources in this unit.
- Do not wire these result types into API response mapping yet unless existing code already uses a compatible pattern.

The goal is to make future application handlers consistent, not to redesign existing API error handling.

### Guard helpers

Add small guard helpers only for universally useful domain validation.

Allowed examples:

- Guard against `null` values.
- Guard against empty or whitespace strings.
- Guard against default `Guid` values.

Rules:

- Keep guard methods deterministic and side-effect free.
- Throw standard .NET exceptions such as `ArgumentException`, `ArgumentNullException`, or `ArgumentOutOfRangeException` only for programmer errors or invalid construction.
- Do not add business-rule guard methods such as `EnsurePlayerIsActive`, `EnsureTeamIsTracked`, or `EnsureReportCanBeVerified`.

### Clock abstraction

Add a clock abstraction for future testable time-dependent business rules.

Preferred approach:

- Define an application-facing abstraction such as `ISystemClock` or `IDateTimeProvider` in the layer that future use cases can consume without depending on Infrastructure.
- Provide an Infrastructure implementation that returns UTC time.
- Register the implementation through the existing Infrastructure dependency injection extension point.

Requirements:

- The clock must expose UTC time clearly, for example `UtcNow`.
- Do not expose local server time as the default.
- Do not add time zone conversion behavior yet.
- Do not inject the clock into domain entities directly.
- Do not change existing API behavior except dependency registration if needed.

### Audit metadata contracts

Add audit metadata contracts only at the abstraction level if useful for future persistence and audit behavior.

Allowed examples:

- `IHasCreatedAudit`
- `IHasModifiedAudit`
- `IHasAuditMetadata`

Rules:

- Keep contracts framework-independent.
- Use UTC timestamps.
- Use user identifier fields as strings only if needed for future auth compatibility.
- Do not create an `AuditLog` entity in this unit.
- Do not implement automatic audit stamping yet.
- Do not add EF Core interceptors or save-change hooks yet.

If audit contracts would be unused or unclear at this stage, add only the result/error and entity primitives and defer audit contracts to the audit foundation unit.

### Tests

Add focused tests in the existing backend test projects.

Recommended tests:

- Entity equality behaves correctly for same concrete type and same identity.
- Entities with different identities are not equal.
- Value object equality works if a value object base is added.
- `Result.Success` and `Result.Failure` cannot create invalid states.
- `Result<T>` exposes successful values safely and fails predictably when invalid access is attempted.
- Guard helpers reject invalid values.
- The system clock implementation returns UTC time.
- Clean Architecture dependency tests continue to pass.

Tests must not require a real database unless the existing backend test foundation already supports it safely.

### Documentation updates

Update `context/progress-tracker.md` during implementation to reflect:

- Unit 13 is in progress when work starts.
- Unit 13 is complete only after verification passes.
- Any deferred primitive decision, such as deferring audit contracts or value object base classes.

Do not update `context/architecture.md` or `context/code-standards.md` unless implementation changes an existing architectural or standards-level decision.

## Dependencies

None.

Use the existing backend test packages introduced by earlier backend testing foundation work. Do not add new NuGet packages unless the implementation proves one is strictly required and the reason is documented in `context/progress-tracker.md`.

## Verification checklist

- [ ] `Domain` shared primitive files exist in clear, responsibility-based folders.
- [ ] `Domain` still has no dependency on `Application`, `Infrastructure`, `Api`, EF Core, ASP.NET Core, or external providers.
- [ ] Entity identity/equality behavior is covered by tests.
- [ ] Result/error behavior is covered by tests.
- [ ] Guard helper behavior is covered by tests if guard helpers are added.
- [ ] Value object equality is covered by tests if a value object base is added.
- [ ] Clock abstraction and UTC implementation are covered by tests if implemented.
- [ ] No FK Velež product entities, user/account models, player models, team models, match models, report models, import models, media models, or audit log models are added.
- [ ] No EF Core migrations or database schema changes are added.
- [ ] No API endpoints are added or changed except dependency registration required for the clock implementation.
- [ ] No authentication, authorization, Identity, cookie, CSRF, or session behavior is added.
- [ ] `dotnet restore` passes from the backend solution.
- [ ] `dotnet build` passes from the backend solution.
- [ ] `dotnet test` passes from the backend solution.
- [ ] Frontend files are not changed.
- [ ] `context/progress-tracker.md` is updated to reflect the actual implementation state.
