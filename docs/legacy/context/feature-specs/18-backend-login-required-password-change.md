# Unit 18: Backend Login and Required Password Change

## Goal

Complete the backend authentication flow so an active staff user can sign in with a secure Identity cookie, inspect the current session, and change their password. A user marked with `mustChangePassword` must be prevented from using protected application APIs until the password is changed successfully.

## Design

This is a backend-only authentication unit. It closes the gap between the existing Identity/session foundations and the first-admin bootstrap before staff-management features are introduced.

Use the existing ASP.NET Core Identity, secure cookie, CSRF, ProblemDetails, configuration, persistence, and testing foundations. Keep HTTP endpoints thin and delegate authentication behavior through Application abstractions/use cases implemented by Infrastructure.

The authentication flow must remain closed and staff-only:

- No public registration.
- No social login, OAuth, JWT, bearer-token, or localStorage/sessionStorage token flow.
- No forgot-password or reset-password implementation in this unit.
- No invitation acceptance flow in this unit.
- No staff role, permission, or team-scope model in this unit.
- No frontend changes in this unit.

Use the existing Identity password policy and lockout configuration. Do not duplicate password rules in endpoint handlers.

### Login contract

Add a cookie-based login endpoint:

```txt
POST /api/auth/login
```

Recommended request shape:

```json
{
  "email": "staff@example.com",
  "password": "user-supplied-password"
}
```

Do not add a persistent `rememberMe` option unless it already exists in the approved Identity/cookie configuration. Prefer the existing session-cookie behavior.

On successful login:

- Create the existing secure HttpOnly authentication cookie.
- Return the current session contract or another small typed response aligned with `GET /api/auth/session`.
- Include `mustChangePassword` so the frontend can route the user correctly in Unit 19.
- Do not return password, password hash, security stamp, reset token, cookie value, or internal Identity details.

Authentication failures must be safe and predictable:

- Invalid credentials return `401` with a generic message.
- Do not reveal whether an email exists.
- Disabled or locked accounts must not receive an authenticated cookie.
- Account-status failures may use safe machine-readable ProblemDetails codes, but user-facing detail must not reveal sensitive internal information.
- Identity lockout-on-failure behavior should remain enabled according to the existing Identity configuration.

### Required password change

Add an authenticated password-change endpoint:

```txt
POST /api/auth/change-password
```

Recommended request shape:

```json
{
  "currentPassword": "current-password",
  "newPassword": "new-password",
  "confirmPassword": "new-password"
}
```

Requirements:

- Require an authenticated active account.
- Require the existing antiforgery/CSRF protection for the unsafe request.
- Validate that `newPassword` and `confirmPassword` match before calling Identity.
- Use the configured Identity password policy as the authoritative password-strength validation.
- Change the password through the approved Identity API; do not write password hashes directly.
- Set `mustChangePassword` to `false` only after the password change succeeds.
- Preserve `mustChangePassword = true` when the change fails.
- Refresh the authentication session after success so the current cookie/session reflects the updated account state.
- Return the updated safe session response or `204` followed by a required session refresh. Prefer one consistent contract across login, session, and password-change endpoints.

### Forced password-change enforcement

Authenticated users with `mustChangePassword = true` may access only the minimal endpoints needed to complete or leave the authentication flow:

- `GET /api/auth/csrf`
- `GET /api/auth/session`
- `POST /api/auth/change-password`
- `POST /api/auth/logout`
- public health endpoints already defined by the project

All other protected application API endpoints must reject the request until the password is changed.

Recommended rejection:

- HTTP `403`
- ProblemDetails-compatible response
- Stable machine-readable code such as `password_change_required`

Implement this enforcement as reusable middleware, an authorization requirement/policy, or another centralized backend mechanism consistent with the existing architecture. Do not scatter `mustChangePassword` checks through endpoint handlers.

The enforcement foundation must be reusable by future protected API groups. If no domain-protected endpoint exists yet, add focused tests using a test-only endpoint or approved test host configuration rather than adding a production demo endpoint.

### Session contract

Update the existing `GET /api/auth/session` response where necessary so an authenticated frontend can distinguish at least:

- unauthenticated session
- authenticated active session
- authenticated session requiring password change

Recommended authenticated fields:

```txt
isAuthenticated
userId
email
name/displayName when already available
accountStatus
mustChangePassword
```

Do not add role, permission, or team-scope fields yet. Those belong to Unit 20 after the corrected build-plan ordering.

### First-admin flow

The environment-created first admin account from Unit 17 must be able to:

1. Sign in with its configured temporary password.
2. Receive a session with `mustChangePassword = true`.
3. Access the password-change endpoint.
4. Be blocked from all other protected application APIs before changing the password.
5. Change the password successfully.
6. Receive a refreshed session with `mustChangePassword = false`.

This unit does not assign the official `ADMIN` primary role. That handoff now belongs to Unit 20, after Unit 19 completes frontend authentication wiring.

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
8. `context/feature-specs/00-build-plan.md`
9. `context/feature-specs/18-backend-login-required-password-change.md`

Also inspect the implemented outputs from:

- Unit 08 backend API error handling foundation.
- Unit 09 backend testing foundation.
- Unit 12 backend persistence foundation.
- Unit 14 backend auth and Identity foundation.
- Unit 15 backend CSRF and session API.
- Unit 17 first admin bootstrap.

Use relevant project-local skills from `.agents/skills/` when applicable, while keeping the context files and this feature spec authoritative.

### Application authentication contracts

Add or extend Application-layer contracts/use cases for:

- signing in a staff user with email and password
- changing the current user's password
- obtaining a safe current-session representation where the existing implementation needs consolidation

Keep Application code independent of ASP.NET Core Identity implementation types. Do not expose `UserManager`, `SignInManager`, Identity entities, cookie APIs, or EF Core types outside Infrastructure/Api boundaries.

Use explicit request/response types and the existing result/error conventions from Unit 13.

Recommended responsibilities:

```txt
Application/Auth/Login
Application/Auth/ChangePassword
Application/Auth/Session
```

Adapt folder names to the existing Vertical Slice conventions rather than introducing a parallel architecture.

### Identity authentication implementation

Implement the Application contracts through the existing Infrastructure Identity setup.

Requirements:

- Normalize email through Identity conventions.
- Use `SignInManager`/`UserManager` or the approved existing abstraction.
- Enable lockout counting for failed password attempts.
- Check the persisted account status before issuing an authenticated cookie.
- Do not authenticate `INVITED`, `DISABLED`, or explicitly `LOCKED` accounts.
- Do not reset or alter account status during login.
- Do not clear the first-login password-change flag during login.
- Do not log submitted passwords or authentication cookies.

If Identity lockout state and the project's explicit `LOCKED` account status are separate concepts, preserve that distinction and document it in `context/progress-tracker.md`. Do not redesign the account lifecycle in this unit.

### Login endpoint

Map `POST /api/auth/login` inside the existing auth endpoint group.

Requirements:

- Parse and validate the request before authentication logic runs.
- Apply antiforgery validation using the Unit 15 foundation where supported by the existing API pattern.
- Return `400` or the project's selected validation status for malformed input.
- Return generic `401` ProblemDetails for invalid credentials.
- Return safe `403` ProblemDetails for a valid account that cannot sign in because of account lifecycle state, without exposing internal details unnecessarily.
- Return the safe session representation on success.
- Keep the endpoint handler thin.

Do not add username-based login, phone login, public account creation, or persistent browser-token storage.

### Change-password endpoint

Map `POST /api/auth/change-password` inside the auth endpoint group.

Requirements:

- Require authentication.
- Require antiforgery validation.
- Resolve the current user through an approved current-user abstraction or the existing session identity.
- Validate request shape and password confirmation.
- Verify the current password through Identity.
- Apply the configured Identity password policy to the new password.
- Persist `mustChangePassword = false` only in the same successful operation/path as the password change.
- Refresh the sign-in session after success.
- Return safe validation errors without exposing password policy internals beyond actionable rules.
- Do not allow an administrator to change another user's password through this endpoint.

### Central password-change gate

Add centralized enforcement for authenticated users whose `mustChangePassword` flag is true.

Requirements:

- Run after authentication and before protected application endpoint execution.
- Allow only the explicitly listed authentication-completion endpoints and public health routes.
- Return a ProblemDetails-compatible `403` with a stable error code.
- Avoid route-name string duplication where a policy or endpoint metadata marker gives a cleaner implementation.
- Ensure future API endpoint groups can adopt the gate without adding per-handler checks.
- Do not interfere with unauthenticated `401` behavior.
- Do not interfere with logout or password-change completion.

If endpoint metadata is used to bypass the gate for allowed auth endpoints, use a clearly named marker rather than ad-hoc magic strings.

### Session endpoint alignment

Update `GET /api/auth/session` only as needed to align it with the completed auth flow.

Requirements:

- Keep unauthenticated response behavior compatible with Unit 16.
- Include `mustChangePassword` for authenticated users.
- Include account status only as a safe display/flow value.
- Do not include password metadata, security stamps, lockout counters, setup tokens, reset tokens, roles, permissions, or selected team identifiers.
- Keep response names stable and documented for Unit 19 frontend wiring.

### ProblemDetails and error codes

Use the existing global error/ProblemDetails conventions.

Define stable machine-readable codes where useful, such as:

```txt
invalid_credentials
account_unavailable
password_change_required
current_password_invalid
password_validation_failed
```

Exact names may follow existing project conventions, but they must be consistent and testable.

Do not return raw Identity error objects directly from endpoint handlers. Map them to safe application/API errors.

### Tests

Add focused backend tests using the existing UnitTests and IntegrationTests infrastructure.

Minimum recommended coverage:

- Valid active user can log in and receives an auth cookie.
- Invalid email/password returns generic `401` without revealing account existence.
- Disabled account cannot log in.
- Locked account cannot log in.
- First-admin account can log in with `mustChangePassword = true`.
- Session response exposes `mustChangePassword` correctly.
- A user requiring password change is blocked from a protected test route.
- The same user can access session, CSRF, password change, and logout routes.
- Incorrect current password does not clear `mustChangePassword`.
- Password-policy failure does not clear `mustChangePassword`.
- Successful password change clears `mustChangePassword` and refreshes the session.
- New password works for a subsequent login.
- Old password no longer works.
- Existing health, CSRF, logout, bootstrap, architecture, and error-handling tests still pass.

Testing rules:

- Do not add a production demo endpoint only for testing.
- Prefer a test-host-only protected endpoint or test-specific endpoint mapping when verifying the password-change gate.
- Do not introduce Testcontainers unless it is already part of the implemented test foundation or is strictly required and documented.
- Do not require committed credentials or a developer-specific password.

### Documentation updates

Update `context/progress-tracker.md` during implementation:

- Mark Unit 18 in progress when work begins.
- Mark it complete only after verification passes.
- Record the final login/session/change-password contracts.
- Record the centralized `mustChangePassword` enforcement approach.
- Replace any outdated note saying Unit 18 assigns the first admin role; the official role handoff is now Unit 20.
- Record any automated-test limitations or manual verification performed.

Use the corrected `context/feature-specs/00-build-plan.md` ordering. Update architecture or code-standard files only if implementation changes a documented project-level decision.

## Dependencies

None expected.

Use the ASP.NET Core Identity, EF Core, antiforgery, ProblemDetails, configuration, result/error, and testing packages already introduced by previous units. Do not add JWT, OAuth/social-login, external identity providers, email providers, frontend packages, role/permission frameworks, rate-limiting packages, or Testcontainers unless an existing dependency gap makes the unit impossible and the reason is documented.

## Verification checklist

- [ ] `AGENTS.md`, all required context files, the corrected build plan, and this feature spec were read before implementation.
- [ ] Relevant project-local skills from `.agents/skills/` were used when applicable without overriding project context.
- [ ] `POST /api/auth/login` exists and uses the existing secure Identity cookie.
- [ ] Login requires the approved antiforgery behavior.
- [ ] Invalid credentials return a generic safe `401` response.
- [ ] Login does not reveal whether an email exists.
- [ ] `INVITED`, `DISABLED`, and `LOCKED` accounts cannot receive an authenticated cookie.
- [ ] Failed login attempts use the existing Identity lockout configuration.
- [ ] Successful login returns or enables retrieval of the safe current-session contract.
- [ ] `GET /api/auth/session` exposes `mustChangePassword` for authenticated users.
- [ ] `POST /api/auth/change-password` requires authentication and antiforgery validation.
- [ ] Password confirmation is validated.
- [ ] Identity password policy remains authoritative.
- [ ] Password hashes are never read, written, returned, or logged directly by endpoint/application code.
- [ ] Failed password changes do not clear `mustChangePassword`.
- [ ] Successful password change clears `mustChangePassword`.
- [ ] Authentication/session state is refreshed after a successful password change.
- [ ] Users with `mustChangePassword = true` are centrally blocked from other protected application APIs.
- [ ] Password-change-required rejection uses a consistent ProblemDetails-compatible `403` response and stable error code.
- [ ] Session, CSRF, change-password, logout, and health routes remain accessible as intended during required-password-change flow.
- [ ] First-admin bootstrap account can complete login and required password change end to end.
- [ ] No public registration, forgot/reset password, invitation acceptance, role/team-scope model, frontend UI, JWT, OAuth, or social login was added.
- [ ] Endpoint handlers remain thin and Identity implementation details stay in Infrastructure/Api boundaries.
- [ ] Required unit and integration tests were added.
- [ ] Existing backend tests still pass.
- [ ] `context/progress-tracker.md` reflects the completed auth flow and corrected Unit 20 admin-role handoff.
- [ ] `dotnet restore` passes.
- [ ] `dotnet build` passes.
- [ ] `dotnet test` passes.
