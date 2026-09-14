# Unit 09: Backend Testing Foundation

## Goal

Add the initial backend testing foundation for the ASP.NET Core backend. This unit creates unit and integration test projects, establishes test naming and organization conventions, and makes `dotnet test` part of the standard backend verification flow before database, authentication, domain workflows, and product API modules are implemented.

## Design

The backend already has a Clean Architecture solution baseline, configuration baseline, and API error-handling foundation. This unit makes those foundations testable before future units add EF Core, Identity, authorization, players, teams, matches, reports, imports, media, medical, dashboard, audit, and settings modules.

Testing must follow the backend architecture and code standards documented in `context/architecture.md` and `context/code-standards.md`:

- backend source remains under `backend/src`;
- backend tests remain under `backend/tests`;
- test projects do not introduce production dependencies into source projects;
- tests must not require real secrets, real `.env` files, a real database, external services, email, object storage, vendor APIs, or Docker;
- Clean Architecture dependency direction remains enforceable;
- future domain, workflow, permission, import, and API logic can add tests into this structure without reorganizing the backend later.

Use a standard .NET test stack that is boring and maintainable:

- xUnit for test cases;
- `Microsoft.NET.Test.Sdk` for test execution;
- `Microsoft.AspNetCore.Mvc.Testing` for API integration tests using an in-memory test host;
- no Testcontainers, database providers, browser/E2E tooling, coverage gates, mutation testing, snapshot testing, or CI pipelines in this unit.

This unit should add tests for behavior that already exists and is meaningful to verify:

- backend source projects build;
- test projects are part of `backend/PlayerPerformance.sln`;
- Clean Architecture project references do not violate the documented dependency direction;
- `GET /health` remains reachable through an integration test;
- unknown routes or error responses follow the safe API error-handling foundation where practical.

Do not add fake product code only to make tests more interesting. Do not introduce sample domain entities, sample handlers, fake users, fake teams, fake players, fake matches, seed data, repositories, database contexts, or mocked vendor integrations.

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
8. `context/feature-specs/09-backend-testing-foundation.md`

Implement only what this spec defines.

### Scope boundaries

This is a backend testing foundation unit.

Do not implement:

- authentication, authorization, sessions, CSRF, Identity, password reset, invite flows, user roles, team scopes, or permission logic;
- PostgreSQL, EF Core, migrations, repositories, DbContext, database health checks, seed data, or Testcontainers;
- product API modules for users, teams, players, matches, match reports, training sessions, imports, media, medical, dashboard, audit, or settings;
- domain entities, workflow state machines, allowed actions, audit logging, import parsing, file storage, email delivery, object storage, Gpexe mapping, Zone14 mapping, or vendor integrations;
- frontend routes, UI screens, frontend tests, Playwright, Vitest, React Testing Library, or frontend API clients;
- CI pipelines, GitHub Actions, Husky, lint-staged, coverage thresholds, Docker, Docker Compose, or deployment configuration.

If the existing backend shape differs slightly from earlier specs, align the tests to the actual implemented backend without expanding this unit into a refactor.

### Test project structure

Create or align the backend test projects under:

```txt
backend/tests/UnitTests/PlayerPerformance.UnitTests.csproj
backend/tests/IntegrationTests/PlayerPerformance.IntegrationTests.csproj
```

Add both projects to:

```txt
backend/PlayerPerformance.sln
```

Expected project reference direction:

```txt
PlayerPerformance.UnitTests -> PlayerPerformance.Domain
PlayerPerformance.UnitTests -> PlayerPerformance.Application
PlayerPerformance.IntegrationTests -> PlayerPerformance.Api
```

Additional references are allowed only when required for testing existing behavior. Do not add test references that create production source project dependency violations.

Use clear root namespaces:

```txt
PlayerPerformance.UnitTests
PlayerPerformance.IntegrationTests
```

Keep test folders simple for now:

```txt
backend/tests/UnitTests/
├── Architecture/
└── GlobalUsings.cs

backend/tests/IntegrationTests/
├── Api/
├── TestApplicationFactory.cs
└── GlobalUsings.cs
```

Adjust names if the existing backend conventions use a different but equivalent structure.

### Unit test conventions

Use xUnit facts and theories.

Test naming convention:

```txt
MethodOrBehavior_ShouldExpectedResult_WhenCondition
```

Examples:

```txt
DomainProject_ShouldNotReferenceApplicationInfrastructureOrApi
ApplicationProject_ShouldNotReferenceInfrastructureOrApi
```

Keep tests readable and direct. Avoid broad helper abstractions until repeated patterns exist.

Add a small architecture dependency test suite that verifies the current Clean Architecture dependency direction using assembly references or project metadata.

Required checks:

- `PlayerPerformance.Domain` does not reference `PlayerPerformance.Application`, `PlayerPerformance.Infrastructure`, or `PlayerPerformance.Api`.
- `PlayerPerformance.Application` does not reference `PlayerPerformance.Infrastructure` or `PlayerPerformance.Api`.
- `PlayerPerformance.Infrastructure` does not reference `PlayerPerformance.Api`.
- The tests should not require a third-party architecture testing package unless the implementer determines it is cleaner and keeps dependencies minimal.

Do not introduce fake domain rules or placeholder business tests.

### Integration test conventions

Use `Microsoft.AspNetCore.Mvc.Testing` and a test application factory for API integration tests.

Create a reusable test factory, for example:

```txt
backend/tests/IntegrationTests/TestApplicationFactory.cs
```

The factory should:

- use the API project entry point;
- set a test environment such as `Testing` where useful;
- provide only safe test configuration values needed for the existing backend startup validation;
- not read or require a real `backend/.env` file;
- not require PostgreSQL, external services, file storage, email, or vendor credentials.

If the API entry point needs to be discoverable by `WebApplicationFactory`, add the minimal conventional support in the API project, such as:

```csharp
public partial class Program;
```

This must not change runtime behavior.

Add integration tests for existing API behavior:

- `GET /health` returns a successful status code.
- `GET /health` returns a safe response body without secrets or environment-specific details.
- An unknown route returns a safe not-found response.
- If Unit 08 implemented ProblemDetails for unknown routes, verify the response content type or JSON shape where practical.

Do not add test-only production endpoints. Do not expose exception-trigger endpoints only for testing unhandled exception behavior.

### Test configuration

Tests must not depend on real local developer secrets.

If Unit 07 introduced required configuration options, the integration test factory should provide safe in-memory values for those options. Example categories:

- service name;
- allowed development/test origin if already required;
- non-secret placeholder values only when startup validation requires them.

Do not use real connection strings, passwords, signing keys, storage keys, SMTP credentials, or vendor tokens.

Do not commit `backend/.env`. Do not read `backend/.env` from tests unless the existing startup code does so indirectly and cannot be avoided without over-refactoring. Prefer in-memory test configuration overrides.

### Solution and command alignment

Ensure the following commands work from the backend directory:

```txt
dotnet restore
dotnet build
dotnet test
```

If the repository also uses root-level scripts or documentation for commands, do not invent a new command system in this unit. Keep the test foundation compatible with standard .NET CLI usage.

Do not add coverage collection scripts or fail builds on coverage thresholds in this unit.

### Documentation and progress tracker

Update `context/progress-tracker.md` after implementation.

The progress note should mention:

- Unit 09 was implemented;
- backend unit and integration test projects were added;
- architecture dependency tests were added;
- health/error foundation integration tests were added where practical;
- `dotnet test` is now a standard backend verification command;
- verification commands run and whether they passed.

Do not add Git workflow notes to the spec or progress tracker. Do not update architecture or code standards unless implementation reveals a real context-level decision that must change.

## Dependencies

Add backend test dependencies only inside the test projects.

Recommended packages:

- `Microsoft.NET.Test.Sdk` — .NET test execution.
- `xunit` — test framework.
- `xunit.runner.visualstudio` — Visual Studio and `dotnet test` runner integration.
- `Microsoft.AspNetCore.Mvc.Testing` — ASP.NET Core integration testing with `WebApplicationFactory`.
- `coverlet.collector` — optional standard collector if included by the test template; do not add coverage thresholds or coverage gates in this unit.

Do not add Testcontainers, EF Core providers, database packages, mocking frameworks, FluentAssertions, NetArchTest, Verify, Bogus, AutoFixture, or CI/deployment packages unless the implementer has a concrete reason and keeps the dependency strictly test-only. Prefer no additional test libraries beyond the standard stack.

## Verification checklist

- [ ] `backend/tests/UnitTests/PlayerPerformance.UnitTests.csproj` exists and is included in `backend/PlayerPerformance.sln`.
- [ ] `backend/tests/IntegrationTests/PlayerPerformance.IntegrationTests.csproj` exists and is included in `backend/PlayerPerformance.sln`.
- [ ] Test project dependencies are test-only and do not affect production source projects.
- [ ] Unit tests verify Clean Architecture dependency direction for Domain, Application, Infrastructure, and Api where practical.
- [ ] Integration tests use an in-memory API test host and do not require a real `.env` file.
- [ ] Integration tests verify `GET /health` returns a successful safe response.
- [ ] Integration tests verify an unknown route returns a safe not-found response.
- [ ] No fake product endpoints, sample domain entities, seed data, database code, auth code, or vendor integration code was added.
- [ ] No real secrets, connection strings, local file paths, environment values, storage keys, SMTP credentials, or vendor tokens were committed.
- [ ] `dotnet restore` passes from `backend/`.
- [ ] `dotnet build` passes from `backend/`.
- [ ] `dotnet test` passes from `backend/`.
- [ ] Frontend `npm run format:check`, `npm run lint`, and `npm run build` are not required unless frontend files were changed.
- [ ] `context/progress-tracker.md` is updated with the Unit 09 implementation result and verification status.
