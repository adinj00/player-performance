# Unit 08: Backend API Error Handling Foundation

## Goal

Add a consistent backend API error-handling foundation for the ASP.NET Core backend. This unit standardizes safe ProblemDetails responses, centralizes endpoint mapping structure, and keeps the API ready for future authenticated modules without adding authentication, database access, domain entities, or product endpoints.

## Design

The backend already has a Clean Architecture solution baseline and safe configuration baseline. This unit builds on those foundations by making HTTP error behavior predictable before later units introduce authentication, users, teams, players, matches, reports, imports, media, medical, dashboard, audit, and settings modules.

The API must follow the backend rules from `context/architecture.md` and `context/code-standards.md`:

- endpoint handlers stay thin;
- business rules do not live in the Api layer;
- responses and errors are consistent and safe to display;
- internal exception details, secrets, local paths, connection strings, machine names, and stack traces must not be exposed to clients;
- backend authorization remains the source of truth when it is introduced later.

Use ASP.NET Core 8 built-in ProblemDetails support where practical. Do not introduce a custom framework or broad abstraction for errors. This unit should establish a small, boring convention that future modules can reuse.

Standard API error behavior:

- Unhandled exceptions return a safe `500` ProblemDetails response.
- Unsupported or missing routes should produce consistent ProblemDetails responses where ASP.NET Core supports this cleanly.
- Error responses should use `application/problem+json` when possible.
- ProblemDetails responses should include a request/trace identifier through a safe extension such as `traceId`.
- Error responses must not expose implementation details.
- Future validation, authorization, and workflow errors should be able to map to this same convention, but those modules are not implemented in this unit.

Keep the existing public `GET /health` endpoint. The health endpoint remains unauthenticated and must keep returning only safe operational information.

Create or align endpoint mapping structure so future feature modules have a clear place to register routes. This may include extension methods such as:

```txt
backend/src/Api/Endpoints/HealthEndpoints.cs
backend/src/Api/Endpoints/EndpointRouteBuilderExtensions.cs
```

or an equivalent project-appropriate structure.

This unit is backend-only. Do not modify frontend screens, frontend routing, frontend API clients, shadcn/ui components, or UI copy unless required to fix an accidental build issue caused by the backend changes.

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
8. `context/feature-specs/08-backend-api-error-handling-foundation.md`

Implement only what this spec defines.

### Scope boundaries

This is a backend API foundation unit.

Do not implement:

- authentication, authorization, cookies, JWTs, sessions, invite flows, password reset, user management, or role checks;
- PostgreSQL, EF Core, migrations, repositories, seed data, database health checks, or domain entities;
- CORS middleware or frontend-backend integration behavior;
- product API modules for users, teams, players, matches, match reports, training sessions, imports, media, medical, dashboard, audit, or settings;
- FluentValidation validation pipelines, Mapster mappings, workflow state machines, allowed actions, or audit logging;
- file storage, email delivery, object storage, video/media handling, import parsing, Gpexe mapping, Zone14 mapping, or vendor integrations;
- Docker, Docker Compose, CI pipelines, deployment configuration, or production hosting setup;
- frontend UI changes, routing changes, fake data, placeholder dashboards, or domain screens.

If implementation discovers unrelated starter code, do not expand this unit into cleanup work unless the code directly conflicts with the API error-handling or endpoint-mapping foundation.

### ProblemDetails setup

Configure backend ProblemDetails support in the API project.

Requirements:

- Register ProblemDetails using ASP.NET Core 8 built-in services where practical.
- Add safe request identification to ProblemDetails responses, for example a `traceId` extension based on `HttpContext.TraceIdentifier`.
- Keep error responses generic and safe.
- Do not include exception messages, stack traces, local file paths, environment variable values, connection strings, secrets, machine names, or provider-specific details in client responses.
- Do not implement custom domain exception hierarchies in this unit.
- Do not add validation libraries in this unit.

The exact implementation may use `AddProblemDetails(...)`, `UseExceptionHandler(...)`, status code pages, and small API-owned helper methods if that produces clearer code.

### Global exception handling

Add centralized handling for unhandled exceptions.

Requirements:

- Unhandled exceptions should return a safe `500` ProblemDetails response.
- The response title should be generic, such as `Unexpected server error` or an equivalent stable API-facing message.
- The response should not expose exception details to the client.
- The backend may still log exceptions through the normal ASP.NET Core logging pipeline.
- Do not swallow exceptions silently.
- Do not build product-specific exception mapping yet.

If the current ASP.NET Core template already has exception handling, align it with this unit instead of duplicating competing middleware.

### Status code ProblemDetails

Configure consistent responses for common non-exception HTTP status codes where practical.

Target behavior:

- `404` for unknown routes should return a safe ProblemDetails response when requested by an API client.
- `405` for unsupported methods may use default ASP.NET Core behavior if custom handling would add unnecessary complexity.
- Future `400`, `401`, `403`, `409`, and `422` responses should be compatible with this ProblemDetails convention, but do not implement auth, validation, workflow, or module-specific mappings yet.

Do not add fake endpoints only to demonstrate errors. Verification can use an unknown route and normal API behavior.

### Endpoint mapping structure

Move or align endpoint registration into small API-owned extension methods.

Recommended structure:

```txt
backend/src/Api/Endpoints/EndpointRouteBuilderExtensions.cs
backend/src/Api/Endpoints/HealthEndpoints.cs
```

Expected behavior:

- `Program.cs` remains short and focused on app composition.
- `GET /health` is mapped through the endpoint structure.
- Future endpoint groups have an obvious place to register routes.
- No product endpoints are added.
- No endpoint handler contains business logic.

Example shape:

```csharp
app.MapHealthEndpoints();
// Later units can add app.MapAuthEndpoints(), app.MapPlayersEndpoints(), etc.
```

or:

```csharp
app.MapApiEndpoints();
```

with `MapApiEndpoints()` internally mapping the current health endpoint and leaving future product modules for later specs.

Choose the simplest structure that fits the existing backend code.

### Health endpoint preservation

Preserve the existing `GET /health` behavior from previous backend units.

Requirements:

- Keep the endpoint unauthenticated.
- Keep the response safe and minimal.
- It may return `status`, configured `service`, and `timestampUtc` if those already exist from Unit 07.
- Do not add database, storage, auth, email, vendor, or downstream dependency checks.
- Do not expose environment variable names, full configuration values, machine details, or local file paths.

### Program startup alignment

Keep startup order clear and conventional.

The API startup should remain easy to scan:

1. build application builder;
2. load local configuration if Unit 07 added that pattern;
3. register typed options and validation from Unit 07;
4. register application and infrastructure composition;
5. register ProblemDetails/error-handling services;
6. build app;
7. configure exception/status-code handling middleware;
8. map endpoints;
9. run app.

Do not move configuration, dependency injection, or endpoint mapping into Domain or Application projects.

### Documentation and progress tracker

Update `context/progress-tracker.md` after implementation.

The progress note should mention:

- Unit 08 was implemented;
- backend ProblemDetails/error-handling foundation was added;
- endpoint mapping structure was added or aligned;
- `GET /health` was preserved;
- verification commands run and whether they passed.

Do not add Git workflow notes to the spec or progress tracker. Do not update project-level architecture or code standards unless implementation reveals a real context-level decision that must change.

## Dependencies

None.

Use ASP.NET Core 8 built-in ProblemDetails and exception-handling capabilities. Do not add third-party error-handling, validation, logging, OpenAPI, authentication, database, CORS, file storage, or testing packages in this unit.

## Verification checklist

- [ ] Backend API registers ProblemDetails or an equivalent ASP.NET Core 8 built-in error response foundation.
- [ ] ProblemDetails responses include a safe request identifier such as `traceId`.
- [ ] Unhandled exceptions return a safe `500` ProblemDetails response without exposing exception details.
- [ ] Unknown API routes return a consistent safe response, preferably ProblemDetails where practical.
- [ ] No response exposes secrets, connection strings, environment values, local file paths, stack traces, machine names, or internal provider details.
- [ ] Endpoint mapping is centralized or aligned through small API-owned extension methods.
- [ ] `Program.cs` remains short and focused on startup composition.
- [ ] `GET /health` still works and remains unauthenticated.
- [ ] No product endpoints, domain entities, database code, auth code, CORS behavior, validation pipeline, file storage, email, imports, Gpexe, or Zone14 logic was added.
- [ ] `PlayerPerformance.Domain` still has no project references.
- [ ] `PlayerPerformance.Application` does not depend on Infrastructure or Api.
- [ ] `dotnet restore backend/PlayerPerformance.sln` passes.
- [ ] `dotnet build backend/PlayerPerformance.sln` passes.
- [ ] Relevant `dotnet test` commands pass if test projects already exist.
- [ ] Frontend files were not changed; if they were changed intentionally, `npm run format:check`, `npm run lint`, and `npm run build` pass from `frontend/`.
- [ ] `context/progress-tracker.md` is updated with actual implementation and verification results.
