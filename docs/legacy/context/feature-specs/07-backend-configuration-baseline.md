# Unit 07: Backend Configuration Baseline

## Goal

Add a safe backend configuration foundation for local development and future production deployment. This unit introduces local `.env` loading, a committed backend `.env.example`, typed configuration options, and startup validation without adding authentication, database access, CORS behavior, domain models, or product API modules.

## Design

The backend already has a Clean Architecture solution baseline. This unit builds on that foundation by making backend configuration explicit, validated, and safe before later units add authentication, PostgreSQL, file storage, email, imports, or vendor integrations.

Configuration must follow the project rules documented in `context/architecture.md` and `context/code-standards.md`:

- local backend configuration uses `backend/.env`;
- only `backend/.env.example` is committed;
- real `.env` files are ignored;
- production uses real environment variables or a managed secret store;
- no secrets, credentials, connection strings, initial admin passwords, signing keys, SMTP credentials, storage keys, or vendor tokens may be hardcoded or committed.

This unit should keep configuration small and boring. It must establish the pattern, not configure every future subsystem.

The backend should support this local workflow:

1. Developer copies `backend/.env.example` to `backend/.env`.
2. `backend/.env` is loaded during local development if it exists.
3. Environment variables still remain the final configuration source.
4. Startup validates required non-secret application configuration.
5. Invalid configuration fails clearly at startup.
6. The existing `GET /health` endpoint continues to work.

Use typed options for application-level settings only. Keep the section small, for example:

```txt
PlayerPerformance:ServiceName
PlayerPerformance:FrontendOrigin
```

Equivalent environment variable names may use ASP.NET Core double-underscore syntax:

```txt
PlayerPerformance__ServiceName
PlayerPerformance__FrontendOrigin
```

`ServiceName` should be required and non-empty. `FrontendOrigin` may be optional at this stage because this unit does not implement CORS. If present, it should be a valid absolute HTTP or HTTPS URL.

Do not add database, Identity, JWT, cookie, file storage, SMTP, object storage, import vendor, Gpexe, Zone14, or production hosting configuration values in this unit unless they are already present and need to be preserved. Future feature specs will add those just in time.

Do not add frontend behavior in this unit. Frontend files should remain unchanged unless a repository-level ignore/config issue directly requires a tiny adjustment.

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
8. `context/feature-specs/07-backend-configuration-baseline.md`

Implement only what this spec defines.

### Scope boundaries

This is a backend configuration foundation unit.

Do not implement:

- authentication, authorization, cookies, JWTs, sessions, invite flows, password reset, or user management;
- PostgreSQL setup, EF Core, migrations, repositories, seed data, or domain entities;
- CORS middleware or frontend-backend integration behavior;
- API modules for users, teams, players, matches, reports, imports, media, medical, dashboard, audit, or settings;
- file storage, email delivery, object storage, video/media handling, import parsing, Gpexe mapping, Zone14 mapping, or vendor integrations;
- Docker, Docker Compose, CI pipelines, deployment configuration, or production hosting setup;
- frontend UI changes, routing changes, fake data, placeholder dashboards, or domain screens.

If implementation discovers unrelated starter code, do not expand this unit into cleanup work. Only remove or adjust code when it directly conflicts with backend configuration loading or startup validation.

### Backend `.env.example`

Create or update:

```txt
backend/.env.example
```

The file should be safe to commit and should document only configuration needed by this unit.

It should include comments explaining:

- copy this file to `backend/.env` for local development;
- never commit real `backend/.env` files;
- production should use real environment variables or a managed secret store.

Include non-secret example values only, such as:

```txt
ASPNETCORE_ENVIRONMENT=Development
PlayerPerformance__ServiceName=Player Performance API
PlayerPerformance__FrontendOrigin=http://localhost:5173
```

Rules:

- Do not include real secrets.
- Do not include real production URLs.
- Do not include database connection strings yet.
- Do not include SMTP, object storage, authentication, cookie signing, admin password, or vendor API values yet.
- Do not invent Gpexe or Zone14 configuration keys.

### Git ignore protection for local env files

Confirm root or backend ignore rules protect real local env files.

At minimum, real local files like these must not be committed:

```txt
backend/.env
backend/.env.local
.env
.env.local
```

The committed example file must remain allowed:

```txt
backend/.env.example
```

If ignore rules already cover this safely, do not duplicate them unnecessarily. If they are missing, update the appropriate `.gitignore` file with the smallest safe change.

### Local `.env` loading

Add local `.env` loading to the backend API startup.

Requirements:

- Load `backend/.env` only when it exists.
- Do not fail startup just because `backend/.env` is missing.
- Do not print secrets or full environment values to logs.
- Preserve normal ASP.NET Core environment variable behavior.
- Keep production-compatible behavior: production must not depend on `.env` files.
- Keep `.env` loading near API startup/configuration composition, not inside Domain or Application.

Use the package listed in Dependencies for this unit unless the project already has an equivalent approved `.env` loader.

The `Domain` and `Application` projects must not depend on the `.env` loader package.

### Typed application options

Create a typed options class for the small application-level configuration section.

Recommended location:

```txt
backend/src/Api/Configuration/PlayerPerformanceOptions.cs
```

or another API-owned configuration folder that matches the existing backend project structure.

Expected shape:

```csharp
public sealed class PlayerPerformanceOptions
{
    public const string SectionName = "PlayerPerformance";

    public string ServiceName { get; init; } = string.Empty;
    public string? FrontendOrigin { get; init; }
}
```

Rules:

- Keep this options type API/configuration-owned for now.
- Do not put provider-specific configuration in Domain.
- Do not add database, Identity, storage, email, import, or vendor options yet.
- Prefer immutable or init-only properties where practical.
- Avoid `any`-style loose dictionaries or untyped configuration access spread across the app.

### Startup validation

Bind and validate the typed options during API startup.

Validation rules:

- `ServiceName` is required and must not be whitespace.
- `FrontendOrigin` is optional.
- If `FrontendOrigin` is provided, it must be an absolute HTTP or HTTPS URL.

Validation should fail clearly during startup when configuration is invalid.

Acceptable approaches:

- options validation with `Validate(...)` and `ValidateOnStart()`;
- a small explicit validation method called during startup;
- another clear ASP.NET Core options validation pattern already used by the project.

Do not add a broad custom configuration framework. Do not hide validation failures or silently replace invalid values with guessed defaults.

### Health endpoint alignment

Keep the existing `GET /health` endpoint from Unit 06.

It may include the configured service name if doing so is safe and useful, for example:

```json
{
  "status": "ok",
  "service": "Player Performance API",
  "timestampUtc": "..."
}
```

Rules:

- Do not expose environment variable names or values.
- Do not expose local file paths.
- Do not expose machine names, connection strings, secrets, or internal exception details.
- Do not add database, storage, auth, or downstream dependency checks yet.
- Do not make the health endpoint require authentication in this unit.

### Documentation and progress tracker

Update `context/progress-tracker.md` after implementation.

The progress note should mention:

- Unit 07 was implemented;
- backend local `.env` loading was added;
- `backend/.env.example` was added or updated;
- typed backend application options and startup validation were added;
- verification commands run and whether they passed.

Do not add Git workflow notes to the spec or progress tracker. Do not update project-level architecture or code standards unless implementation reveals a real context-level decision that must change.

## Dependencies

Backend dependency:

- `DotNetEnv` — loads `backend/.env` for local development while preserving normal environment variable configuration behavior.

No frontend dependencies are required in this unit.

Do not add EF Core, Identity, authentication, CORS, file storage, email, import parsing, test, Docker, or deployment packages in this unit.

## Verification checklist

- [ ] `backend/.env.example` exists and contains only safe non-secret example values for this unit.
- [ ] Real local env files such as `backend/.env` are ignored and not staged.
- [ ] Backend API startup loads `backend/.env` when present and still starts when it is missing.
- [ ] Environment variables remain compatible with ASP.NET Core double-underscore configuration naming.
- [ ] Typed `PlayerPerformanceOptions` or equivalent configuration class exists.
- [ ] `ServiceName` is required and validated at startup.
- [ ] Optional `FrontendOrigin` is validated as an absolute HTTP or HTTPS URL when provided.
- [ ] `GET /health` still returns a safe response and does not expose secrets or local machine details.
- [ ] No authentication, database, CORS middleware, domain modules, import logic, storage, or vendor configuration was added.
- [ ] `PlayerPerformance.Domain` still has no project references.
- [ ] `PlayerPerformance.Application` does not depend on Infrastructure or Api.
- [ ] The `.env` loader package is not referenced by Domain or Application.
- [ ] `dotnet restore backend/PlayerPerformance.sln` passes.
- [ ] `dotnet build backend/PlayerPerformance.sln` passes.
- [ ] Relevant `dotnet test` commands pass if test projects already exist.
- [ ] Frontend files were not changed; if they were changed intentionally, `npm run format:check`, `npm run lint`, and `npm run build` pass from `frontend/`.
- [ ] `context/progress-tracker.md` is updated with actual implementation and verification results.
