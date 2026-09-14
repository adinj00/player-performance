# Backend

## Overview

This area contains the ASP.NET Core 8 API and the server side source of truth for authorization, workflows, persistence, imports, and audit history. Its four source projects keep domain rules, application use cases, infrastructure adapters, and HTTP concerns separate.

## Key files

| File | Owns |
|---|---|
| `PlayerPerformance.sln` | Backend source and test projects |
| `src/Api/Program.cs` | Host startup, middleware order, and service composition |
| `src/Api/Endpoints/EndpointRouteBuilderExtensions.cs` | API endpoint group registration |
| `src/Application/DependencyInjection.cs` | Application use case registration |
| `src/Infrastructure/DependencyInjection.cs` | Persistence, Identity, storage, import, and adapter registration |
| `src/Infrastructure/Persistence/AppDbContext.cs` | EF Core model entry point |
| `tests/IntegrationTests/TestApplicationFactory.cs` | Shared API test host and Identity test setup |

## Commands

Run these from `backend/`.

```powershell
dotnet restore PlayerPerformance.sln
dotnet build PlayerPerformance.sln --no-restore
dotnet test PlayerPerformance.sln --no-restore --no-build
dotnet format PlayerPerformance.sln whitespace --no-restore
dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes
```

For EF Core model changes, use the CLI and keep generated files in the documented migrations folder.

```powershell
dotnet ef migrations add <Name> --project src/Infrastructure/PlayerPerformance.Infrastructure.csproj --startup-project src/Api/PlayerPerformance.Api.csproj --context PlayerPerformance.Infrastructure.Persistence.AppDbContext --output-dir Persistence/Migrations
dotnet ef database update --project src/Infrastructure/PlayerPerformance.Infrastructure.csproj --startup-project src/Api/PlayerPerformance.Api.csproj
```

## Conventions

- Keep `Domain` independent, let `Application` depend on `Domain`, let `Infrastructure` depend on both, and let `Api` compose `Application` and `Infrastructure`.
- Keep business rules and workflow transitions in Domain or Application. API endpoint handlers should map HTTP concerns and delegate work.
- Define infrastructure abstractions in Application, then implement them in Infrastructure.
- Keep EF Core configuration and migrations under `src/Infrastructure/Persistence/`.
- Use ProblemDetails responses, server side authorization, CSRF checks for unsafe cookie authenticated requests, and audit records for important mutations.
- Put domain and application coverage in `tests/UnitTests/`. Put host, authorization, persistence boundary, and endpoint coverage in `tests/IntegrationTests/`.

## Gotchas

- Local settings come from the ignored `backend/.env`. Only `.env.example` is committed, and production must use real environment variables or an approved secret store.
- `IdentityTestApplicationFactory` replaces PostgreSQL with the EF Core in memory provider. The base `TestApplicationFactory` keeps the production provider and uses an unreachable test connection for startup and configuration checks.
- Production startup must not apply migrations. Run `dotnet ef database update` as an explicit deployment or local verification step.
- The context rules prohibit direct EF Core queries in API endpoints, while `src/Api/Endpoints/Players/MedicalEndpoints.cs` and `src/Api/Endpoints/Training/TrainingSessionEndpoints.cs` currently inject `AppDbContext`. This is unresolved documentation and code drift. Do not treat either side as the new convention without an explicit decision.

_Drafted by /audit from the repo, worth a quick human pass. Edit freely: once a line stops matching this draft, later runs treat it as curated and will flag rather than overwrite it._
