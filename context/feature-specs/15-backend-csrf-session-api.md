# Unit 15: Backend CSRF and Session API

## Goal

Add the backend CSRF protection and session API foundation for the cookie-authenticated staff system. This unit must expose safe auth utility endpoints for session inspection, CSRF token issuance, and logout while keeping login, password reset, staff invitation, first-admin bootstrap, roles, team scopes, and frontend auth UI out of scope.

## Design

This is a backend security foundation unit. Unit 14 introduced Identity persistence and secure HttpOnly cookie authentication as infrastructure; this unit adds the API behavior needed for browser clients to safely interact with that cookie-based authentication model.

The authentication model remains closed and staff-only:

- There is no public registration.
- There is no player login.
- There is no JWT bearer-token authentication.
- The authentication cookie remains backend-managed and HttpOnly.
- The frontend must not store access tokens in `localStorage` or `sessionStorage`.
- Unsafe cookie-authenticated requests must be protected against CSRF.

CSRF protection must be compatible with the planned React/Vite frontend and future typed API client:

- The backend issues a CSRF token through a safe endpoint.
- The token is usable by browser JavaScript for future unsafe requests.
- The auth/session cookie itself remains HttpOnly and unreadable by JavaScript.
- Future unsafe API calls can consistently send a CSRF header.

The session API should give the frontend just enough information to render auth-aware UI later without exposing sensitive Identity internals. It should not return password hashes, security stamps, reset tokens, invite tokens, lockout internals, or authorization decisions that belong to future role/team-scope units.

This unit must also make unauthorized and forbidden API behavior predictable for frontend clients. API callers should receive status-code-appropriate JSON/ProblemDetails-compatible responses instead of HTML redirects.

Out of scope for this unit:

- Login endpoint.
- Forgot password endpoint.
- Reset password endpoint.
- Invite setup endpoint.
- First admin bootstrap.
- Staff user-management endpoints.
- Role, team-scope, or permission policy implementation.
- Frontend auth screens or session provider wiring.
- Product API modules.
- Database schema changes unless a small migration is unavoidable because of Unit 14 corrections.

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
8. `context/feature-specs/15-backend-csrf-session-api.md`

### CSRF service configuration

Configure ASP.NET Core antiforgery/CSRF services in the API host or existing backend service-registration structure.

Requirements:

- Use the framework-supported antiforgery system appropriate for ASP.NET Core 8.
- Configure a stable request header name for future frontend requests, for example:

```txt
X-CSRF-TOKEN
```

- Configure a stable CSRF token cookie name for the browser-readable token, for example:

```txt
XSRF-TOKEN
```

- The CSRF token cookie must be readable by browser JavaScript so the frontend can copy it into the CSRF request header.
- The authentication cookie must remain HttpOnly.
- Cookie `SameSite`, `Secure`, and path settings must be explicit and compatible with the existing local-development and production configuration direction.
- Do not use CSRF tokens as authentication tokens.
- Do not store CSRF tokens in localStorage or sessionStorage.
- Do not disable CSRF globally for future unsafe endpoints.

If environment-specific cookie behavior is needed, keep it configuration-driven and safe. Do not commit real secrets or machine-specific values.

### CSRF token endpoint

Add a safe endpoint for issuing or refreshing the CSRF token.

Recommended endpoint:

```txt
GET /api/auth/csrf
```

Requirements:

- The endpoint is safe and does not mutate domain state.
- The endpoint may be callable by unauthenticated clients so the frontend can prepare a form before login flows exist.
- The endpoint issues the CSRF token through the configured readable cookie and/or returns enough metadata for the frontend to know which header to send.
- The response must not expose authentication cookie values, security stamps, password hashes, reset tokens, invite tokens, or secrets.
- The response shape should be stable and small, for example:

```json
{
  "csrfTokenHeaderName": "X-CSRF-TOKEN"
}
```

If the framework implementation requires returning the token body value instead of relying only on a readable cookie, keep the response minimal and document why. Do not return unrelated auth/session data from this endpoint.

### Session endpoint

Add a session-inspection endpoint.

Recommended endpoint:

```txt
GET /api/auth/session
```

Requirements:

- The endpoint is safe and idempotent.
- Unauthenticated callers receive `200 OK` with an explicit unauthenticated response, not a redirect.
- Authenticated callers receive `200 OK` with minimal current-user session data.
- The endpoint must read the current authenticated Identity user through backend-managed cookie auth.
- The endpoint must not create users, seed admins, refresh passwords, or mutate account state.
- Disabled or locked users should not be treated as active usable sessions. If Unit 14 did not already enforce this at sign-in time because no login exists yet, this endpoint must return a safe unauthenticated or blocked state for disabled/locked accounts.

Recommended unauthenticated response shape:

```json
{
  "isAuthenticated": false,
  "user": null
}
```

Recommended authenticated response shape:

```json
{
  "isAuthenticated": true,
  "user": {
    "id": "...",
    "email": "staff@example.com",
    "accountStatus": "ACTIVE",
    "mustChangePassword": false
  }
}
```

Implementation rules:

- Use English field names in API contracts.
- Do not return localized labels in this unit.
- Do not return roles, team scopes, permission flags, allowed actions, or navigation access yet.
- Do not expose Identity implementation details such as password hash, security stamp, concurrency stamp, lockout end, failed access count, normalized fields, or token data.
- Keep response DTOs in the appropriate backend boundary instead of returning Infrastructure entities directly.

### Logout endpoint

Add a logout endpoint.

Recommended endpoint:

```txt
POST /api/auth/logout
```

Requirements:

- The endpoint requires an authenticated session.
- The endpoint validates CSRF protection because it is an unsafe cookie-authenticated request.
- Successful logout clears the backend authentication cookie.
- Successful logout returns a small JSON response or `204 No Content`, but must not redirect to an HTML page.
- Unauthenticated logout requests return a predictable `401` response.
- Missing or invalid CSRF token returns a predictable safe error response.
- The endpoint must not delete, disable, or mutate the user account record.
- The endpoint must not implement login, password reset, invite setup, or first-admin behavior.

### Unauthorized and forbidden API behavior

Ensure cookie authentication events return API-friendly responses.

Requirements:

- Unauthorized API requests return `401` without redirecting to a login HTML page.
- Forbidden API requests return `403` without redirecting to an access-denied HTML page.
- Error responses should be JSON and compatible with the existing ProblemDetails/error-handling foundation where practical.
- Responses must not leak stack traces, internal exception messages, Identity internals, or secrets.
- Existing health endpoints must remain reachable and unchanged unless already intentionally protected by an earlier unit.

If full ProblemDetails integration for authentication challenge/forbid events requires a broader error abstraction, keep this unit minimal and document any limitation in `context/progress-tracker.md`.

### Endpoint organization

Place auth utility endpoints in a clear backend API module structure.

Recommended structure:

```txt
backend/src/Api/Endpoints/Auth/AuthEndpoints.cs
```

or the closest existing endpoint-grouping convention from earlier units.

Requirements:

- Keep endpoint handlers thin.
- Do not place Identity persistence queries directly inside deeply nested HTTP handlers if an application/service abstraction already exists.
- Do not expose Infrastructure `ApplicationUser` as the public API response contract.
- Keep route names and endpoint group organization stable for future auth features.
- Do not add temporary fake protected endpoints just to test auth behavior.

### CSRF enforcement pattern for future endpoints

Create or document the pattern future unsafe endpoints should use to require CSRF validation.

Requirements:

- Future unsafe methods such as `POST`, `PUT`, `PATCH`, and `DELETE` must have a clear way to require CSRF validation when they rely on cookie authentication.
- Safe methods such as `GET`, `HEAD`, and `OPTIONS` should not require CSRF tokens just to load public-safe state.
- Do not accidentally require CSRF for `/health` endpoints.
- Do not disable CSRF checks globally because current product endpoints do not exist yet.

If the chosen ASP.NET Core 8 setup uses endpoint metadata to require antiforgery validation, apply it to `POST /api/auth/logout` and leave future endpoint usage obvious.

### Tests

Add or update backend tests where practical.

Required coverage where the existing test infrastructure supports it:

- Existing health endpoint tests still pass.
- Clean Architecture dependency tests still pass.
- `GET /api/auth/session` returns a stable unauthenticated response for unauthenticated callers.
- `GET /api/auth/csrf` returns a successful safe response and sets or describes the configured CSRF token behavior.
- `POST /api/auth/logout` without authentication returns `401` and does not redirect to HTML.
- `POST /api/auth/logout` without a valid CSRF token fails safely when authenticated testing is practical.
- Unauthorized/forbidden behavior is API-friendly where it can be tested without adding fake product endpoints.

Do not add brittle tests that depend on a developer-specific local PostgreSQL database unless the existing integration test setup already supports it. If authenticated session testing requires a test-auth helper that does not exist yet, test the unauthenticated paths now and document the authenticated/manual verification gap in `context/progress-tracker.md`.

### Documentation updates

Update `context/progress-tracker.md` during implementation to reflect:

- Unit 15 is in progress when work starts.
- Unit 15 is complete only after verification passes.
- CSRF protection foundation exists.
- Session and logout API endpoints exist.
- Any manual verification gaps, especially around authenticated logout testing if no test-auth helper exists yet.
- Any important decision about CSRF header names, cookie names, or API auth error response shape.

Do not update `context/architecture.md` or `context/code-standards.md` unless implementation changes a documented architecture or standards decision.

## Dependencies

None expected.

Use the ASP.NET Core 8 framework-supported antiforgery and authentication features already available through the backend stack where practical. Do not install JWT bearer packages, OAuth/social login packages, email providers, frontend packages, Testcontainers, role/permission frameworks, object storage packages, CSV/XLSX packages, or speculative security libraries in this unit.

If the implementation reveals that a small additional backend package is truly required for ASP.NET Core antiforgery/session behavior, add only that package, document why in `context/progress-tracker.md`, and do not add unrelated packages.

## Verification checklist

- [ ] `AGENTS.md` and all required context files were read before implementation.
- [ ] ASP.NET Core antiforgery/CSRF services are configured.
- [ ] CSRF request header name is stable and documented in code or configuration.
- [ ] CSRF token cookie name is stable and documented in code or configuration.
- [ ] CSRF token cookie is browser-readable only for CSRF purposes.
- [ ] Authentication cookie remains HttpOnly.
- [ ] Cookie `SameSite`, `Secure`, and path behavior are explicit and safe for the configured environment.
- [ ] `GET /api/auth/csrf` exists and returns a minimal safe response.
- [ ] `GET /api/auth/csrf` does not expose auth cookie values, Identity internals, reset tokens, invite tokens, or secrets.
- [ ] `GET /api/auth/session` exists.
- [ ] Unauthenticated `GET /api/auth/session` returns `200 OK` with `isAuthenticated: false` and `user: null`.
- [ ] Authenticated `GET /api/auth/session` returns only minimal safe user data.
- [ ] Session response does not expose password hashes, security stamps, normalized fields, token values, or lockout internals.
- [ ] Disabled or locked accounts are not reported as active usable sessions.
- [ ] `POST /api/auth/logout` exists.
- [ ] `POST /api/auth/logout` requires authentication.
- [ ] `POST /api/auth/logout` validates CSRF protection.
- [ ] Successful logout clears the backend authentication cookie and does not redirect to HTML.
- [ ] Unauthenticated logout returns a predictable `401` response.
- [ ] Missing or invalid CSRF token returns a safe predictable error response.
- [ ] Unauthorized API requests return `401` without HTML redirects.
- [ ] Forbidden API requests return `403` without HTML redirects.
- [ ] Error responses are JSON and ProblemDetails-compatible where practical.
- [ ] Existing `/health` behavior still works and is not accidentally CSRF-protected.
- [ ] No login, password reset, invite setup, first admin bootstrap, roles, team scopes, or frontend auth UI are added.
- [ ] No JWT bearer, OAuth/social-login, email, object storage, CSV/XLSX, Testcontainers, or speculative security packages are installed.
- [ ] API response DTOs do not expose Infrastructure Identity entities directly.
- [ ] Clean Architecture dependency tests pass.
- [ ] `dotnet restore` passes for the backend solution.
- [ ] `dotnet build` passes for the backend solution.
- [ ] `dotnet test` passes for the backend solution, or any skipped integration coverage is documented with a reason.
- [ ] `context/progress-tracker.md` is updated to reflect the actual implementation state.
