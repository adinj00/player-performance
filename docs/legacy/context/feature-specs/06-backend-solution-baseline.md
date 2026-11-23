# Unit 06: Backend Solution Baseline

## Goal

Create a clean ASP.NET Core 8 backend solution baseline for the Player Performance Data System. This unit establishes the backend folder structure, Clean Architecture project boundaries, project references, and a minimal API host without implementing authentication, database persistence, domain models, or product features.

## Design

The backend must follow the architecture documented in `context/architecture.md` and the implementation rules in `context/code-standards.md`.

This unit is foundation-only. Its purpose is to make the backend compile and provide a stable structure for later auth, users, teams, players, matches, reports, imports, media, medical, dashboard, audit, and settings modules.

Use the documented backend structure:

```txt
backend/
├── PlayerPerformance.sln
├── .env.example
├── src/
│   ├── Api/
│   ├── Application/
│   ├── Domain/
│   └── Infrastructure/
└── tests/
    ├── UnitTests/
    └── IntegrationTests/
```

The backend project names should be explicit and consistent:

```txt
PlayerPerformance.Api
PlayerPerformance.Application
PlayerPerformance.Domain
PlayerPerformance.Infrastructure
```

Clean Architecture dependency direction must be preserved:

```txt
Domain ← Application ← Infrastructure
          ↑              ↑
          └──── Api ─────┘
```

Project reference rules:

- `PlayerPerformance.Domain` must not reference any other project.
- `PlayerPerformance.Application` may reference `PlayerPerformance.Domain` only.
- `PlayerPerformance.Infrastructure` may reference `PlayerPerformance.Application` and `PlayerPerformance.Domain`.
- `PlayerPerformance.Api` may reference `PlayerPerformance.Application` and `PlayerPerformance.Infrastructure`.
- Do not create reverse dependencies to make setup easier.

The API host should be minimal and operational:

- Use ASP.NET Core 8.
- Use Minimal APIs.
- Add only a non-domain health endpoint, for example `GET /health`.
- Return a small safe response such as `status`, `service`, and UTC timestamp.
- Do not expose secrets, local paths, environment variable values, or machine-specific data.
- Do not add production OpenAPI, auth, database, EF Core, Identity, CORS, file storage, import parsing, or business endpoints in this unit.

The implementation may include empty dependency injection extension points so future units have a consistent composition pattern:

- `Application` can expose `AddApplication()`.
- `Infrastructure` can expose `AddInfrastructure(...)`.
- `Api` can call those methods during startup.

These extension points must not register speculative services. They should only establish the composition location for later units.

If backend starter/template code already exists, clean it carefully:

- Remove sample endpoints such as weather forecast/demo APIs.
- Remove unused template classes, records, controllers, or generated sample data.
- Preserve working project files and settings that are already aligned with this spec.
- Do not delete user-created code outside the scope of this unit.

Visible user-facing UI is not part of this unit. Do not modify frontend screens unless required to fix accidental build breakage caused by this backend setup.

## Implementation

### Required reading

Before implementing, read:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/ui-context.md`
5. `context/code-standards.md`
6. `context/ai-workflow-rules.md`
7. `context/progress-tracker.md`
8. `context/feature-specs/06-backend-solution-baseline.md`

Implement only what this spec defines.

### Backend directory setup

Create or align the `backend/` directory.

Expected structure:

```txt
backend/
├── PlayerPerformance.sln
├── .env.example
├── src/
│   ├── Api/
│   ├── Application/
│   ├── Domain/
│   └── Infrastructure/
└── tests/
    ├── UnitTests/
    └── IntegrationTests/
```

Rules:

- Keep backend source code under `backend/src`.
- Keep future backend tests under `backend/tests`.
- Do not place backend source projects at the repository root.
- Do not introduce Docker, deployment files, production environment files, or real `.env` secrets in this unit.
- Create `backend/.env.example` only as a placeholder for future documented backend environment variables. It should not contain real secrets or guessed connection strings.

### Solution and source projects

Create or align the solution file:

```txt
backend/PlayerPerformance.sln
```

Create or align these source projects:

```txt
backend/src/Api/PlayerPerformance.Api.csproj
backend/src/Application/PlayerPerformance.Application.csproj
backend/src/Domain/PlayerPerformance.Domain.csproj
backend/src/Infrastructure/PlayerPerformance.Infrastructure.csproj
```

Project type expectations:

- `PlayerPerformance.Api` is the ASP.NET Core host project.
- `PlayerPerformance.Application` is a class library.
- `PlayerPerformance.Domain` is a class library.
- `PlayerPerformance.Infrastructure` is a class library.

Configuration expectations:

- Target .NET 8.
- Enable nullable reference types.
- Enable implicit usings unless the existing project style intentionally disables them.
- Keep project files simple.
- Do not add NuGet packages that are not required by this unit.

### Project references

Add project references according to the approved dependency direction.

Required references:

```txt
PlayerPerformance.Application -> PlayerPerformance.Domain
PlayerPerformance.Infrastructure -> PlayerPerformance.Application
PlayerPerformance.Infrastructure -> PlayerPerformance.Domain
PlayerPerformance.Api -> PlayerPerformance.Application
PlayerPerformance.Api -> PlayerPerformance.Infrastructure
```

Forbidden references:

```txt
PlayerPerformance.Domain -> any project
PlayerPerformance.Domain -> Infrastructure
PlayerPerformance.Domain -> Api
PlayerPerformance.Application -> Infrastructure
PlayerPerformance.Application -> Api
PlayerPerformance.Infrastructure -> Api
```

Do not introduce shared projects, cross-cutting projects, or extra layers in this unit.

### Application composition placeholder

In `PlayerPerformance.Application`, add a small dependency injection extension point for future use.

Expected responsibility:

- Provide a method such as `AddApplication(...)`.
- Return `IServiceCollection` to support fluent startup composition.
- Register no speculative services.
- Add comments only if they explain the boundary clearly and are not stale TODOs.

This file should establish where future application-layer use cases, validators, mappers, and services will be registered.

### Infrastructure composition placeholder

In `PlayerPerformance.Infrastructure`, add a small dependency injection extension point for future use.

Expected responsibility:

- Provide a method such as `AddInfrastructure(...)`.
- Accept configuration only if needed for future composition shape.
- Return `IServiceCollection` to support fluent startup composition.
- Register no database, Identity, file storage, email, import, or vendor services yet.

This file should establish where future EF Core, PostgreSQL, Identity, file storage, email, and import infrastructure will be registered.

### API host baseline

In `PlayerPerformance.Api`, create or clean `Program.cs`.

The API host should:

- Build and run as a minimal ASP.NET Core app.
- Register `AddApplication()` and `AddInfrastructure(...)` if those extension methods are created.
- Map `GET /health`.
- Return a safe, simple health response.
- Avoid domain behavior and database checks.

Suggested health response shape:

```json
{
  "status": "ok",
  "service": "PlayerPerformance.Api",
  "timestampUtc": "2026-01-01T00:00:00Z"
}
```

The timestamp should be generated dynamically in UTC. Do not hardcode a timestamp.

Do not add:

- Authentication or authorization.
- User management.
- Database connectivity.
- EF Core.
- PostgreSQL setup.
- Migrations.
- Seed data.
- Domain entities.
- Match, player, team, report, import, media, medical, dashboard, audit, or settings endpoints.
- CORS policy.
- OpenAPI customization.
- Swagger UI unless it already exists and is intentionally preserved from a clean template.

If a template generated weather forecast or demo code, remove it.

### Test folder baseline

Create the `backend/tests/UnitTests` and `backend/tests/IntegrationTests` folders if they do not exist.

Do not add test projects or testing packages in this unit unless they already exist and only need cleanup to keep the solution building.

Dedicated backend test project setup can be handled in a later feature spec once the first domain/application behavior exists.

### Backend starter cleanup

If the repository already contains backend starter code, clean only obvious starter/template artifacts:

- Weather forecast sample endpoint.
- Template controllers.
- Template records/classes used only by sample endpoints.
- Unused sample data.
- Placeholder code that conflicts with the clean baseline.

Do not remove:

- Existing `.sln` or `.csproj` files that are aligned with this spec.
- Existing project references that match Clean Architecture.
- Existing configuration files that are still valid and safe.
- Any code that appears intentionally project-specific.

### Documentation updates

Update `context/progress-tracker.md` after implementation.

At minimum, record:

- Unit 06 was implemented.
- Backend solution baseline exists.
- Clean Architecture project boundaries were established.
- The health endpoint exists.
- Verification commands that passed or failed.

Do not update `context/architecture.md` or `context/code-standards.md` unless the implementation requires a real project-level decision that differs from the existing context.

## Dependencies

None for production code beyond the .NET 8 SDK and packages included by the selected ASP.NET Core project template.

Do not install EF Core, Npgsql, ASP.NET Core Identity packages, FluentValidation, Mapster, test packages, CSV/XLSX parsers, file storage SDKs, Swagger/OpenAPI packages, or Docker tooling in this unit unless they already exist from a template and are intentionally preserved.

## Verification checklist

- [ ] `AGENTS.md` and all context files were read before implementation.
- [ ] `backend/PlayerPerformance.sln` exists.
- [ ] `PlayerPerformance.Api`, `PlayerPerformance.Application`, `PlayerPerformance.Domain`, and `PlayerPerformance.Infrastructure` projects exist under `backend/src`.
- [ ] Project references follow the approved Clean Architecture dependency direction.
- [ ] `PlayerPerformance.Domain` has no project references.
- [ ] `PlayerPerformance.Application` does not reference `Infrastructure` or `Api`.
- [ ] `PlayerPerformance.Infrastructure` does not reference `Api`.
- [ ] `PlayerPerformance.Api` references only the backend layers it is allowed to compose.
- [ ] Backend starter/demo code such as weather forecast samples has been removed if present.
- [ ] No domain entities, database schema, authentication, authorization, imports, media, audit, or product endpoints were implemented.
- [ ] `GET /health` exists and returns a safe non-domain response.
- [ ] No real secrets or environment-specific values were committed.
- [ ] `backend/.env.example` exists if backend environment placeholder documentation is needed, and it contains no real secrets.
- [ ] `backend/tests/UnitTests` and `backend/tests/IntegrationTests` folders exist or the existing tests folder structure remains aligned.
- [ ] From `backend/`, `dotnet restore PlayerPerformance.sln` passes.
- [ ] From `backend/`, `dotnet build PlayerPerformance.sln --no-restore` passes.
- [ ] If backend test projects already exist, `dotnet test PlayerPerformance.sln --no-build` passes or failures are documented.
- [ ] If frontend files were changed, from `frontend/`, `npm run format:check` passes.
- [ ] If frontend files were changed, from `frontend/`, `npm run lint` passes if the script exists.
- [ ] If frontend files were changed, from `frontend/`, `npm run build` passes.
- [ ] `context/progress-tracker.md` was updated with the actual implementation state and verification results.
