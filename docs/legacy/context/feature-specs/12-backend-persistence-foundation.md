# Unit 12: Backend Persistence Foundation

## Goal

Add the backend persistence foundation for PostgreSQL using EF Core, without implementing real domain modules or business entities yet.

When this unit is complete, the backend has a configured `AppDbContext`, migration structure, database connection wiring, and a database health check that future backend modules can build on safely.

## Design

This unit is infrastructure-only. It prepares persistence plumbing but does not introduce player, match, report, user, role, import, media, audit, or medical domain behavior.

Persistence must follow the Clean Architecture boundaries from `context/architecture.md`:

- `Domain` remains independent and must not reference EF Core, PostgreSQL, ASP.NET Core, or Infrastructure.
- `Application` may define abstractions later, but this unit should not force premature repository or unit-of-work patterns.
- `Infrastructure` owns EF Core, PostgreSQL provider configuration, migrations, and database-specific setup.
- `Api` composes Infrastructure through dependency injection and exposes health checks.

Use PostgreSQL as the only relational provider. Do not add SQLite, SQL Server, in-memory database providers, or provider-switching logic unless tests require a separate test strategy later.

The existing backend configuration conventions from Unit 07 must remain the source of truth for local development configuration. Real secrets and real connection strings must stay outside source control.

Database health checks must be safe and operational:

- The existing general health endpoint must remain available.
- Add a database-aware health check endpoint or extend the existing health check setup in a way that clearly reports database readiness.
- Health responses must not expose raw connection strings, credentials, stack traces, or database internals.

The initial DbContext may have no real domain `DbSet` properties yet. It may include only EF Core infrastructure setup needed for migrations and future entity configuration.

Keep this unit compatible with future ASP.NET Core Identity work, but do not implement Identity, auth tables, staff users, roles, permissions, sessions, CSRF, or first-admin bootstrap here.

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
8. `context/feature-specs/12-backend-persistence-foundation.md`

### EF Core package setup

Add EF Core/PostgreSQL packages only to the backend projects that need them.

Expected package ownership:

- `backend/src/Infrastructure`
  - EF Core relational infrastructure
  - PostgreSQL provider
  - design-time/migration support if required by the chosen setup
- `backend/src/Api`
  - health check integration and DI composition only where required

Do not add EF Core packages to `Domain`.

Avoid adding packages unrelated to persistence, such as Identity, authentication, CSV/XLSX readers, object storage SDKs, background jobs, or vendor integrations.

### Infrastructure DbContext

Create an Infrastructure-owned DbContext, for example:

```txt
backend/src/Infrastructure/Persistence/AppDbContext.cs
```

Requirements:

- The class derives from `DbContext`.
- It accepts `DbContextOptions<AppDbContext>` through the constructor.
- It contains no business logic.
- It contains no speculative domain entities.
- It is prepared for future entity configuration through `OnModelCreating`.
- It can apply configurations from the Infrastructure assembly when future entity configurations exist.

Do not create fake entities just to make EF Core work.

### Persistence configuration

Add persistence registration through Infrastructure dependency injection, for example in the existing `AddInfrastructure(...)` extension.

Requirements:

- Register `AppDbContext` using Npgsql/PostgreSQL.
- Read the connection string from the existing backend configuration pattern.
- Fail clearly at startup if the required database connection string is missing or blank.
- Do not log the connection string.
- Keep provider-specific configuration inside Infrastructure or API composition, not Domain or Application.
- Do not run migrations automatically on startup.

Connection-string naming should follow the existing project configuration conventions. If no final name exists yet, use a conventional key such as:

```txt
ConnectionStrings__DefaultConnection
```

Also document the expected variable in `backend/.env.example` without including a real secret.

### Design-time migration support

Add design-time EF Core support only if required for reliable migration commands.

If a design-time factory is needed, keep it in Infrastructure and make it read configuration consistently with local development conventions.

Requirements:

- Migration commands can be run from the repository/backend context with clear project/startup-project arguments.
- The design-time path does not require real production secrets.
- The design-time setup does not duplicate application startup logic more than necessary.

### Initial migration structure

Create the initial EF Core migration structure for the persistence baseline.

Requirements:

- Migrations live under Infrastructure, not Api, Application, or Domain.
- The migration name should communicate that it is a baseline, such as `InitialPersistenceBaseline`.
- If no domain entities exist yet, an empty baseline migration is acceptable.
- Do not add placeholder tables for players, matches, users, reports, imports, audit logs, or media.
- Do not introduce seeded business data in this unit.

### Database health check

Add a database-aware health check using the configured DbContext or PostgreSQL connection.

Requirements:

- The health check verifies that the backend can reach the configured database.
- The response must remain safe for operational display.
- Failed database connectivity should produce an unhealthy response without exposing secrets.
- The existing basic health behavior from earlier units must not regress.

If there are separate endpoints, use clear names such as:

```txt
/health
/health/ready
```

Keep endpoint naming consistent with the current backend structure.

### Environment example updates

Update `backend/.env.example` to include the database connection configuration key.

Requirements:

- Use placeholder values only.
- Do not include real local credentials, real production credentials, or personal machine-specific paths.
- Keep real `.env` files ignored.
- Preserve existing configuration examples from Unit 07.

### Tests

Update or add backend tests where practical.

Required test coverage:

- Clean Architecture dependency tests still pass.
- API integration tests for existing health behavior still pass.
- Add persistence/health-check tests only if they can run reliably without requiring a developer-specific PostgreSQL instance.

If database connectivity tests require a real local PostgreSQL instance and no test container strategy exists yet, keep those as manual verification steps instead of brittle automated tests.

Do not introduce Testcontainers in this unit unless the project owner explicitly approves a separate testing infrastructure decision.

### Progress tracker update

After implementation, update `context/progress-tracker.md` to reflect:

- Unit 12 is implemented.
- EF Core/PostgreSQL persistence foundation exists.
- Any manual database verification requirement.
- Any known limitation, such as health check verification requiring a local PostgreSQL database.

Do not mark the unit complete until the verification checklist is addressed or any skipped verification is clearly documented with the reason.

## Dependencies

Install only the packages required for this unit.

Expected backend NuGet packages:

- `Microsoft.EntityFrameworkCore` — EF Core runtime abstractions.
- `Microsoft.EntityFrameworkCore.Design` — design-time tooling for migrations.
- `Npgsql.EntityFrameworkCore.PostgreSQL` — PostgreSQL provider for EF Core.
- `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` — DbContext-based health checks, if used by the implementation.

Do not install Identity, authentication, object storage, CSV/XLSX, Testcontainers, or frontend packages in this unit.

## Verification checklist

- [ ] `AGENTS.md` and all required context files were read before implementation.
- [ ] EF Core packages are added only to appropriate backend projects.
- [ ] `Domain` has no dependency on EF Core, Infrastructure, Api, PostgreSQL, or ASP.NET Core.
- [ ] `Application` does not depend on Infrastructure implementation details.
- [ ] `Infrastructure` contains the DbContext and migration structure.
- [ ] `Api` composes the persistence configuration through dependency injection without owning persistence logic.
- [ ] `AppDbContext` exists and contains no speculative business entities.
- [ ] PostgreSQL is configured through existing backend configuration conventions.
- [ ] Missing or blank database connection configuration fails clearly and safely.
- [ ] No real connection strings or secrets are committed.
- [ ] `backend/.env.example` includes only placeholder database configuration.
- [ ] Real `.env` files remain ignored.
- [ ] Initial migration structure exists under Infrastructure.
- [ ] No player, match, report, user, role, import, media, audit, or medical tables are introduced in this unit.
- [ ] No automatic migration execution is added to application startup.
- [ ] Existing `/health` behavior still works.
- [ ] Database-aware health check behavior is implemented and documented.
- [ ] Health responses do not leak secrets, stack traces, or connection details.
- [ ] Backend architecture/dependency tests pass.
- [ ] `dotnet restore` passes for the backend solution.
- [ ] `dotnet build` passes for the backend solution.
- [ ] `dotnet test` passes for the backend solution, or any intentionally skipped database-dependent verification is documented in `context/progress-tracker.md`.
- [ ] Frontend checks are not required unless frontend files were changed.
- [ ] `context/progress-tracker.md` is updated after implementation.
