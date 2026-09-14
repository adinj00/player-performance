# Unit 14: Backend Auth and Identity Foundation

## Goal

Add the backend authentication and Identity persistence foundation for the closed FK Velež staff-only system. This unit must configure Identity, account status storage, secure cookie authentication, password/security options, and the initial Identity database migration without adding public registration, first-admin bootstrapping, role management, CSRF endpoints, session APIs, or frontend screens.

## Design

This is a backend foundation unit. It prepares the authentication substrate that later units will use for first-admin bootstrap, staff user management, CSRF/session APIs, protected frontend auth flows, roles, team scopes, and permission checks.

The application remains a closed internal staff system:

- There is no public registration.
- There is no player login.
- There is no multi-club tenant model.
- Users are staff accounts only.
- Authentication uses backend-managed secure HttpOnly cookies.
- Frontend code must not store tokens in `localStorage` or `sessionStorage`.

Identity persistence must fit the existing Clean Architecture structure:

- `Domain` may own framework-independent account concepts such as account status enums.
- `Application` may define abstractions later, but must not depend on ASP.NET Core Identity or EF Core implementation details.
- `Infrastructure` owns the Identity EF Core user type, DbContext integration, entity configuration, and migrations.
- `Api` owns authentication/cookie middleware composition and security option wiring.

This unit must not implement real auth workflows yet. Do not add login, logout, session, password reset, forgot password, invite setup, CSRF, staff invitation, first admin creation, roles, team scopes, permission flags, user-management endpoints, or frontend auth UI. Those are separate units.

The database schema may add only the tables required for the Identity foundation and any minimal fields required for the account lifecycle baseline. Do not add staff-management domain tables beyond what is required for Identity persistence in this unit.

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
8. `context/feature-specs/14-backend-auth-identity-foundation.md`

### Account status model

Add a framework-independent account status enum for staff accounts.

Required statuses:

- `INVITED`
- `ACTIVE`
- `DISABLED`
- `LOCKED`

Implementation rules:

- Keep the enum in a backend layer that does not depend on EF Core or ASP.NET Core Identity.
- Use English enum names internally.
- Do not add localized display labels in this unit.
- Do not add role names, team scopes, verification permissions, or staff-management workflows yet.
- Do not implement account lifecycle actions yet; only store the status foundation needed by later units.

### Identity user model

Create the backend Identity user type in Infrastructure.

Recommended location:

```txt
backend/src/Infrastructure/Identity/ApplicationUser.cs
```

Requirements:

- Base the user type on ASP.NET Core Identity.
- Use a stable identifier strategy consistent with the backend persistence foundation.
- Include account lifecycle fields required for later units, at minimum:
  - account status
  - whether password change is required
  - created timestamp
  - optional updated timestamp if consistent with existing primitives
- Keep staff profile details minimal. Do not add full name, avatar, role, team scope, permissions, invite metadata, recovery audit data, or staff assignment data unless strictly required for Identity to function.
- Do not add navigation properties for future modules.
- Do not expose the Infrastructure Identity user type to frontend contracts.

### DbContext Identity integration

Update the existing Infrastructure `AppDbContext` to support Identity persistence.

Requirements:

- Integrate the Identity user type into the existing EF Core persistence setup.
- Keep Identity table configuration in Infrastructure.
- Keep migrations under Infrastructure.
- Do not move persistence logic into `Api`, `Application`, or `Domain`.
- Do not add custom role tables in this unit unless the chosen Identity setup creates unavoidable default tables; prefer user-only Identity setup where practical.
- Do not seed users, admins, passwords, roles, or permissions.

If table names are customized, use clear, stable names that will remain suitable for a staff-only internal system. Avoid names that imply public users, club tenants, or player accounts.

### Identity service registration

Register Identity services through the existing backend dependency injection structure.

Requirements:

- Configure IdentityCore or equivalent user-focused Identity services.
- Register EF Core stores using the existing `AppDbContext`.
- Register password hashing and token providers needed for future password reset/invite setup flows.
- Configure sign-in behavior only as foundation; do not expose sign-in endpoints yet.
- Keep implementation details in Infrastructure and API composition.
- Do not add authorization policies for roles, team scopes, or permissions yet.

### Password and account security options

Configure conservative baseline Identity options.

Requirements:

- Password requirements must be explicit and suitable for staff accounts.
- Lockout behavior must be explicit.
- User email uniqueness must be enforced.
- Email confirmation behavior may be configured for future invite/setup flows, but do not build those flows yet.
- Disabled users must be prevented from successful sign-in in the future design. If no sign-in path exists yet, prepare a clear extension point or sign-in validation hook for Unit 15 or Unit 17 to enforce this.
- Do not hardcode admin credentials, temporary passwords, reset tokens, invite tokens, or secret keys.

### Cookie authentication baseline

Configure secure backend-managed cookie authentication.

Requirements:

- Authentication cookies must be HttpOnly.
- Cookie security options must be production-safe where possible.
- SameSite behavior must be explicit and compatible with the planned frontend/backend local development setup.
- Cookie names must be project-specific enough to avoid ambiguity.
- Login, logout, access denied, and unauthorized response behavior must be API-friendly and must not redirect API callers to HTML pages.
- Do not store authentication tokens in frontend-accessible storage.
- Do not add frontend code in this unit.

If local development requires less strict cookie security than production, keep that behavior configuration-driven and document it safely without committing secrets.

### Authentication middleware

Wire authentication middleware into the API host in the correct order.

Requirements:

- `UseAuthentication()` is registered before `UseAuthorization()`.
- Existing health endpoints remain reachable and unchanged unless a prior unit already made them protected intentionally.
- No product endpoints are protected yet because product endpoints do not exist.
- Do not add temporary fake protected endpoints just to test authentication.

### Migration

Create the initial Identity migration.

Requirements:

- Migration lives under Infrastructure with the existing migration convention.
- Migration name clearly reflects Identity foundation, for example `AddIdentityFoundation`.
- Migration contains only Identity/account foundation schema changes.
- No first admin seed data is added.
- No staff role, team scope, player, match, report, import, media, audit, medical, or settings tables are added in this unit.

### Tests

Add or update backend tests where practical.

Recommended coverage:

- Clean Architecture dependency tests still pass.
- `Domain` does not depend on Identity, EF Core, Infrastructure, or ASP.NET Core.
- Existing health endpoint integration tests still pass.
- Identity services can be resolved from the application service provider if the existing test setup supports this safely.
- Cookie/auth configuration is API-friendly and does not produce HTML redirects for unauthorized API requests, if this can be tested without adding fake endpoints.
- Account status enum contains the required statuses.

Do not introduce a brittle test that requires a developer-specific local PostgreSQL instance unless the existing test infrastructure already supports it. If database-backed Identity tests require a real database and no test database strategy exists yet, document that as manual verification in `context/progress-tracker.md`.

### Documentation updates

Update `context/progress-tracker.md` during implementation to reflect:

- Unit 14 is in progress when work starts.
- Unit 14 is complete only after verification passes.
- Identity persistence foundation exists.
- Any manual database or migration verification that could not be automated.
- Any important decisions about cookie settings or Identity table naming.

Do not update `context/architecture.md` or `context/code-standards.md` unless implementation changes a documented architecture or standards decision.

## Dependencies

Install only backend packages required for this unit.

Expected NuGet package:

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` — Identity EF Core stores and Identity persistence integration.

Use existing EF Core, PostgreSQL, configuration, and testing packages from earlier units. Do not install frontend packages, OAuth/social-login packages, JWT bearer packages, email providers, object storage packages, CSV/XLSX packages, Testcontainers, or role/permission frameworks in this unit.

## Verification checklist

- [ ] `AGENTS.md` and all required context files were read before implementation.
- [ ] Account status enum exists with `INVITED`, `ACTIVE`, `DISABLED`, and `LOCKED`.
- [ ] Account status model is framework-independent and does not depend on EF Core or ASP.NET Core Identity.
- [ ] Infrastructure contains the Identity user model and Identity EF Core configuration.
- [ ] `Application` does not depend on ASP.NET Core Identity or Infrastructure implementation details.
- [ ] `Domain` does not depend on ASP.NET Core Identity, EF Core, Infrastructure, Api, or ASP.NET Core.
- [ ] Existing `AppDbContext` is integrated with Identity without adding speculative business entities.
- [ ] Identity services are registered through the existing backend dependency injection structure.
- [ ] Password requirements are explicitly configured.
- [ ] Lockout behavior is explicitly configured.
- [ ] Email uniqueness is enforced.
- [ ] Cookie authentication is configured with HttpOnly cookies.
- [ ] Cookie options are explicit and safe for production-oriented behavior.
- [ ] API auth failures do not redirect API callers to HTML login/access-denied pages.
- [ ] Authentication middleware is wired in the correct order.
- [ ] Existing health endpoints still work.
- [ ] Identity migration exists under Infrastructure.
- [ ] Migration adds only Identity/account foundation schema changes.
- [ ] No admin user, password, role, permission, team scope, player, match, report, import, media, audit, medical, or settings seed data is added.
- [ ] No login, logout, session, CSRF, forgot-password, reset-password, invite, first-admin, staff-management, or frontend auth UI is implemented in this unit.
- [ ] No public registration endpoint or behavior is added.
- [ ] No authentication token is stored in frontend-accessible storage.
- [ ] Backend architecture/dependency tests pass.
- [ ] Existing backend integration tests pass.
- [ ] `dotnet restore` passes for the backend solution.
- [ ] `dotnet build` passes for the backend solution.
- [ ] `dotnet test` passes for the backend solution.
- [ ] Frontend files are not changed.
- [ ] `context/progress-tracker.md` is updated to reflect the actual implementation state.
