# Unit 17: First Admin Bootstrap

## Goal

Add a safe backend-only first admin bootstrap flow for the closed FK Velež staff system. When the user store is empty, the API must be able to create exactly one initial staff account from environment configuration, mark it as requiring a password change, and never expose a public setup route or hardcoded credentials.

## Design

This unit creates the operational path that lets the internal system receive its first administrator account before staff-management screens and role/team-scope management exist.

The bootstrap flow must follow the project security model:

- The application is staff-only.
- There is no public registration.
- There is no player login.
- There is no browser-accessible setup wizard.
- There are no hardcoded emails, usernames, passwords, invite tokens, or reset tokens.
- Backend-managed secure HttpOnly cookies remain the authentication model.
- The frontend must not be changed in this unit.

This unit is backend-only and should reuse the Identity foundation from Unit 14 and the CSRF/session API foundation from Unit 15. It must not introduce login endpoints, password-reset endpoints, staff invitation flows, role/team-scope authorization, frontend auth forms, or admin user-management UI.

The first admin account should be created only when the persisted user store contains no users. If at least one user exists, bootstrap configuration must not create, update, overwrite, or reactivate any account.

Because the full role and team-scope model is introduced in a later unit, this unit should not invent the final authorization model. The bootstrap account may be created as an active staff Identity account with a clear marker or documented handoff for Unit 18 to assign the `ADMIN` role when the role model exists. Do not add broad custom role tables or permission flags in this unit.

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
8. `context/feature-specs/17-first-admin-bootstrap.md`

Also inspect the implemented outputs from these dependencies:

- Unit 07 backend configuration baseline.
- Unit 12 backend persistence foundation.
- Unit 14 backend auth and Identity foundation.
- Unit 15 backend CSRF and session API.

### Bootstrap configuration

Add typed backend configuration for first admin bootstrap.

Recommended configuration keys:

```txt
Bootstrap__FirstAdmin__Enabled
Bootstrap__FirstAdmin__Email
Bootstrap__FirstAdmin__TemporaryPassword
```

Requirements:

- Add safe placeholder values to `backend/.env.example` only.
- Keep real `backend/.env` values ignored by Git.
- Do not commit a real email/password combination.
- Validate configuration through the existing options/startup validation pattern from Unit 07.
- Validate that the email is present and email-shaped when bootstrap is enabled and no users exist.
- Validate that the temporary password is present when bootstrap is enabled and no users exist.
- The temporary password must satisfy the configured Identity password policy.
- Do not log, return, display, or expose the temporary password.
- If the user store is empty and bootstrap is disabled or incomplete, fail startup with a clear safe error message unless the current environment is an automated test environment where bootstrap is explicitly skipped.

Example `.env.example` values must be placeholders only:

```txt
Bootstrap__FirstAdmin__Enabled=false
Bootstrap__FirstAdmin__Email=admin@example.com
Bootstrap__FirstAdmin__TemporaryPassword=change-this-temporary-password
```

Adapt exact names to the configuration conventions already implemented in the backend, but keep the purpose clear.

### Bootstrap service

Create a backend service responsible for the bootstrap operation.

Recommended location:

```txt
backend/src/Infrastructure/Identity/FirstAdminBootstrapper.cs
```

or a nearby Infrastructure-owned Identity folder matching the current codebase.

Requirements:

- Use the existing Identity user store through `UserManager` or the approved Identity abstraction already present in the codebase.
- Run the bootstrap check during API startup after services and persistence are available.
- Check whether any users already exist before attempting creation.
- If one or more users exist, exit without side effects.
- If no users exist, create exactly one user from the configured email and temporary password.
- Normalize email/user name through Identity conventions.
- Set account status according to the Unit 14 account lifecycle model.
- Set the password-change-required flag to `true`.
- Set created/updated timestamps using the approved clock abstraction if available.
- Do not seed roles, team scopes, permission flags, players, teams, matches, reports, settings, or sample data.
- Do not write secrets to logs.
- If creation fails, fail startup with a clear safe message that includes validation errors where safe, but not the temporary password.

Recommended first-account state:

- `accountStatus`: `ACTIVE`
- `mustChangePassword`: `true`

If the implemented Unit 14 account model uses different property names, use the existing model without changing architecture unnecessarily.

### Startup wiring

Wire the bootstrapper into the API startup path.

Requirements:

- Keep startup wiring small and explicit.
- Do not place Identity creation logic directly inside `Program.cs` beyond calling a clearly named extension/service method.
- Ensure the bootstrap runs only after configuration, persistence, and Identity services are registered.
- Do not run migrations automatically unless a previous unit already established that convention.
- Do not block health endpoint behavior after a successful bootstrap.
- Keep failures safe and visible during local startup.

Recommended shape:

```csharp
await app.Services.BootstrapFirstAdminAsync(app.Environment);
```

or an equivalent extension method consistent with the implemented backend style.

### Idempotency and safeguards

The bootstrap flow must be idempotent and safe to rerun.

Requirements:

- Restarting the API must not create duplicate first admin accounts.
- Existing users must never be overwritten by bootstrap configuration.
- Changing bootstrap env values after users exist must have no effect.
- The bootstrapper must not reset passwords for existing users.
- The bootstrapper must not reactivate disabled/locked users.
- The bootstrapper must not downgrade or mutate an existing account into an admin.
- If a user already exists with the bootstrap email but the total user count is not zero, do nothing.

If the project is later deployed with multiple API instances, a stronger distributed lock or database constraint strategy may be needed. Do not implement multi-instance locking in this unit unless it is already available; document the limitation in `context/progress-tracker.md` if relevant.

### First admin role handoff

Do not implement the final role/team-scope model in this unit.

Requirements:

- Do not add `ADMIN`, `DATA_OPERATOR`, `ANALYST`, `COACH`, `MEDICAL_STAFF`, or `VIEWER` role assignment tables in this unit.
- Do not add authorization policies for role/team-scope permissions in this unit.
- Add a short progress-tracker note that Unit 18 must assign or migrate the bootstrap account into the `ADMIN` role when staff roles are implemented.
- If the Identity model already has a minimal bootstrap marker from Unit 14, use it only as a temporary handoff marker and document it clearly.
- If no such marker exists, do not add one unless it is necessary to preserve the ability to identify the initial account safely later.

Preferred approach: create the first staff account now, then let Unit 18 introduce the official role/scope model and assign the first existing account as admin through a controlled migration or startup rule.

### Tests

Add or update backend tests where practical.

Recommended coverage:

- Bootstrap does nothing when at least one user already exists.
- Bootstrap creates one user when no users exist and valid configuration is present.
- Created user has the expected account status.
- Created user has `mustChangePassword` set to `true`.
- Bootstrap fails safely when no users exist and required configuration is missing.
- Bootstrap does not log or expose the temporary password.
- Re-running bootstrap does not create duplicates.
- Existing health and auth/session tests still pass.

Testing rules:

- Prefer tests that use the existing backend test infrastructure.
- Do not introduce Testcontainers in this unit unless it already exists from a previous task.
- Do not require a developer-specific local PostgreSQL instance for normal automated tests unless the project already supports that pattern.
- If full database-backed bootstrap tests are not practical yet, add focused unit tests around the bootstrap decision logic and document manual DB verification in `context/progress-tracker.md`.

### Documentation updates

Update `context/progress-tracker.md` during implementation to reflect:

- Unit 17 is in progress when work starts.
- Unit 17 is complete only after verification passes.
- First admin bootstrap exists and is environment-configured.
- The temporary password is not committed and must be supplied through local/production environment configuration.
- Unit 18 must connect the bootstrap account to the official `ADMIN` role model.
- Any manual verification that could not be automated.

Do not update `context/architecture.md` or `context/code-standards.md` unless implementation changes a documented architecture or standards decision.

## Dependencies

None expected.

Use the Identity, EF Core, configuration, and testing packages already introduced by earlier backend foundation units. Do not install frontend packages, JWT/OAuth packages, email providers, object storage packages, CSV/XLSX packages, role/permission frameworks, or Testcontainers in this unit unless an existing dependency gap makes the bootstrap impossible and the reason is documented.

## Verification checklist

- [ ] `AGENTS.md` and all required context files were read before implementation.
- [ ] First admin bootstrap configuration exists as typed backend options.
- [ ] `backend/.env.example` contains placeholder bootstrap values only.
- [ ] Real bootstrap credentials are not committed.
- [ ] Bootstrap runs only when the persisted user store is empty.
- [ ] Bootstrap does nothing when at least one user already exists.
- [ ] Bootstrap creates exactly one first staff account from environment configuration when no users exist.
- [ ] Created first staff account is active according to the existing account status model.
- [ ] Created first staff account is marked as requiring a password change.
- [ ] Bootstrap never logs, returns, displays, or exposes the temporary password.
- [ ] Bootstrap does not create public registration, public setup routes, invite setup, login, forgot-password, reset-password, or frontend UI.
- [ ] Bootstrap does not add role/team-scope tables or final authorization policies.
- [ ] Existing health endpoint still works after successful startup.
- [ ] Existing session/logout/CSRF behavior from Unit 15 still works.
- [ ] Automated bootstrap tests were added where practical.
- [ ] Manual empty-database bootstrap verification is documented if full automated coverage is not practical yet.
- [ ] `context/progress-tracker.md` documents completion and the Unit 18 role handoff note.
- [ ] Clean Architecture dependency tests still pass.
- [ ] `dotnet restore` passes.
- [ ] `dotnet build` passes.
- [ ] `dotnet test` passes.
