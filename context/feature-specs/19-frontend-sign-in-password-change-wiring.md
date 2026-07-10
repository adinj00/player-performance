# Unit 19: Frontend Sign-In and Password Change Wiring

## Goal

Connect the existing frontend authentication shell to the backend login, session, CSRF, logout, and password-change APIs. Staff users must be able to sign in, complete a required first-login password change, and enter protected application routes without fake authentication or client-stored tokens.

## Design

This is a frontend authentication wiring unit. It completes the browser-facing flow built on Units 15, 16, and 18 while keeping authentication and account enforcement backend-owned.

The application remains closed and staff-only:

- No public registration.
- No player login.
- No social login, OAuth, JWT, bearer-token, or browser token storage.
- No forgot-password or reset-password backend behavior.
- No invitation acceptance flow.
- No role, permission, or team-scope behavior yet.
- No staff-management UI.

Use the existing secure HttpOnly Identity cookie. The frontend must never read, copy, persist, or expose the authentication cookie. CSRF values may be read or requested only through the approved Unit 15 mechanism and must not be persisted in `localStorage` or `sessionStorage`.

The backend remains the source of truth for:

- whether the user is authenticated;
- whether the account may sign in;
- whether a password change is required;
- password policy validation;
- protected API authorization;
- authentication cookie creation and invalidation.

The frontend owns only user experience and navigation:

- collect email and password;
- submit login through the existing auth API layer;
- refresh the session after successful mutations;
- route users requiring a password change to the password-change page;
- prevent users from navigating around the required-password-change flow in the UI;
- show safe Bosnian Latin feedback for loading, validation, and authentication errors.

### Authentication flow

Expected first-admin and future staff flow:

1. Unauthenticated user opens a protected route.
2. Frontend redirects to `/sign-in` and preserves the requested internal path where safe.
3. User submits email and password.
4. Frontend obtains or reads the approved CSRF value and calls `POST /api/auth/login` with credentials included.
5. Frontend refreshes or replaces the session query from the successful response.
6. If `mustChangePassword` is `true`, frontend routes to `/change-password`.
7. User submits current password, new password, and confirmation.
8. Frontend calls `POST /api/auth/change-password` with CSRF protection and credentials included.
9. Frontend refreshes the session.
10. When `mustChangePassword` becomes `false`, frontend routes to the preserved safe path or the main application entry route.

Normal returning staff flow:

1. User signs in.
2. Session reports `mustChangePassword = false`.
3. Frontend routes to the preserved safe path or main application entry route.

### Route behavior

Use these route responsibilities:

```txt
/sign-in         public authentication entry route
/change-password authenticated completion route for required password changes
```

Requirements:

- An unauthenticated user visiting a protected app route is redirected to `/sign-in`.
- An unauthenticated user visiting `/change-password` is redirected to `/sign-in`.
- An authenticated user with `mustChangePassword = true` is redirected from normal protected app routes to `/change-password`.
- An authenticated user with `mustChangePassword = false` is redirected away from `/change-password` to the main app entry route or a valid preserved return path.
- An authenticated user visiting `/sign-in` is redirected according to their session state:
  - `/change-password` when password change is required;
  - the main app entry route otherwise.
- Loading state must render before route decisions to avoid protected-content flashes and redirect loops.

Preserved return paths must be safe:

- Accept only internal relative application paths.
- Reject absolute URLs, protocol-relative URLs, malformed values, and external origins.
- Do not return users to `/sign-in`, `/change-password`, logout, or another redirect-loop destination.
- Fall back to the existing main application entry route when the value is absent or invalid.

The query parameter name may follow the existing Unit 16 implementation, such as `returnTo`, but it must remain consistent across route guards and successful authentication navigation.

### UI direction

Keep the existing light-only FK Velež Mostar identity and semantic token system.

- Bosnian Latin is the default visible language.
- Keep the sign-in and password-change pages compact, professional, and operational.
- Reuse the existing auth shell, common UI primitives, form foundation, and generated shadcn/ui components.
- Do not add gradients, oversized marketing content, decorative feature cards, dark mode, raw Tailwind palette classes, hardcoded colors, or ad-hoc visual overrides to shadcn/ui primitives.
- Use accessible labels, keyboard submission, visible focus states, field-level errors, and a clear form-level error state.

Recommended visible copy:

```txt
Prijava za osoblje
E-mail adresa
Lozinka
Prijavi se
Promjena privremene lozinke
Trenutna lozinka
Nova lozinka
Potvrdi novu lozinku
Sačuvaj novu lozinku
Odjava
```

Exact wording may be refined to match existing translation/copy conventions, but one screen must not mix Bosnian and English operational UI text.

### Error behavior

Frontend messages must remain safe and useful:

- Invalid credentials: generic sign-in failure; do not indicate whether an email exists.
- Disabled, locked, invited, or otherwise unavailable account: safe account-unavailable message based on the backend response, without exposing internal details.
- Invalid current password: attach a safe message to the current-password field or form summary.
- Password confirmation mismatch: validate client-side before submission.
- Password policy failure: display backend-provided safe validation information without exposing raw Identity objects.
- CSRF/session expiry: show a recoverable message, refresh required auth state where appropriate, and avoid infinite automatic retries.
- Network/unexpected error: use the existing ProblemDetails-aware error pattern and provide a retry path.

Do not map errors by brittle English sentence matching. Prefer stable backend machine-readable codes introduced by Unit 18, such as:

```txt
invalid_credentials
account_unavailable
password_change_required
current_password_invalid
password_validation_failed
```

Adapt names only if Unit 18 implemented documented equivalents.

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
9. `context/feature-specs/19-frontend-sign-in-password-change-wiring.md`

Inspect the implemented outputs and final contracts from:

- Unit 03 frontend routing foundation.
- Unit 05 common UI primitives foundation.
- Unit 10 frontend API client and environment foundation.
- Unit 11 frontend form and validation foundation.
- Unit 15 backend CSRF and session API.
- Unit 16 frontend auth shell and session provider.
- Unit 18 backend login and required password change.

Use relevant frontend skills from `frontend/.agents/` when applicable. Skills may guide implementation but must not override the project context files, active feature spec, or existing codebase conventions.

### Final contract inspection

Before changing frontend code, inspect the implemented backend response contracts rather than assuming that every recommended Unit 18 shape was used verbatim.

Confirm:

- login request and success response;
- change-password request and success response;
- unauthenticated and authenticated session shapes;
- CSRF token acquisition/header convention;
- stable ProblemDetails error-code field and validation-error shape;
- logout behavior;
- exact main protected application entry route already established by routing.

Keep frontend types aligned with the implemented backend contracts. Do not silently reshape the backend or add parallel compatibility paths unless a documented mismatch must be fixed.

### Auth API helpers

Extend the existing auth feature API layer from Unit 16.

Required operations:

```txt
login
changePassword
getSession
getCsrf
logout
```

Recommended request contracts, subject to the final Unit 18 implementation:

```ts
export interface LoginRequest {
  email: string;
  password: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
```

Requirements:

- Reuse the shared typed API client from Unit 10.
- Keep `credentials: "include"` behavior.
- Apply the existing CSRF header convention to login, password change, and logout as required by the backend.
- Do not duplicate generic fetch or ProblemDetails parsing logic inside the auth feature.
- Do not store credentials, CSRF tokens, session data, or authentication tokens in browser storage.
- Do not log submitted passwords or full authentication payloads.
- Keep API types explicit and narrowly scoped.

If Unit 18 returns the updated session directly after login or password change, use it to update the session query cache and still preserve a reliable refetch path. If Unit 18 returns `204`, invalidate/refetch the session before navigating.

### Session query and mutation synchronization

Extend the Unit 16 session provider/hook without moving server state to Zustand or duplicating it in React Context.

Requirements:

- Session data remains TanStack Query-owned.
- Add login and password-change mutation hooks or equivalent feature-owned mutation functions.
- On successful login:
  - update or invalidate the session query;
  - wait until the authenticated session is known before final navigation;
  - route based on `mustChangePassword`.
- On successful password change:
  - update or invalidate the session query;
  - verify the refreshed session reports `mustChangePassword = false` before navigating to the normal app.
- On logout:
  - invalidate or replace the session query with the unauthenticated shape;
  - clear only transient in-memory auth mutation state;
  - navigate to `/sign-in`.
- Avoid duplicate parallel session requests where the existing query cache can be reused.
- Configure mutation retry behavior so credential/password failures are not automatically resubmitted.

Do not cache submitted password values in TanStack Query, global state, URLs, logs, or persistent storage.

### Sign-in form

Replace the unavailable/disabled sign-in shell behavior from Unit 16 with a working form.

Use React Hook Form with Zod and the existing Unit 11 form conventions.

Fields:

- email
- password

Requirements:

- Use semantic input types and browser autocomplete values appropriate for sign-in.
- Normalize only harmless presentation whitespace where appropriate; do not transform passwords.
- Validate required fields and basic email shape client-side.
- Treat backend authentication as authoritative.
- Disable duplicate submission while the login request is pending.
- Support Enter-key submission.
- Show a clear pending state without replacing the entire page unexpectedly.
- Keep the email value available after a failed request for correction.
- Clear or avoid unnecessarily retaining the password after failed/successful submission according to the form library's safe local-state behavior.
- Display generic invalid-credentials feedback.
- Do not add a registration link or social-login actions.

Forgot-password and reset-password links/shells from Unit 16 may remain clearly unavailable. Do not imply those flows work.

### Required password-change page

Create or complete the `/change-password` page within the auth feature.

Fields:

- current password
- new password
- confirm new password

Requirements:

- Use React Hook Form with Zod.
- Validate that all fields are present.
- Validate that new-password confirmation matches client-side.
- Do not duplicate the complete Identity password policy in frontend code unless an approved shared/documented rule already exists.
- Treat backend password-policy validation as authoritative and display safe returned rules/messages.
- Use appropriate password autocomplete attributes for current and new passwords.
- Disable duplicate submission while pending.
- Support keyboard submission.
- Do not expose the normal application shell/navigation as a bypass while password change is required.
- Provide an accessible logout action so the user can leave the session without completing the change.
- Do not add a cancel action that navigates into the protected application.
- After success, reset sensitive local form state and navigate only after the refreshed session confirms completion.

Recommended explanatory copy:

```txt
Prije nastavka morate promijeniti privremenu lozinku.
Nova lozinka mora ispunjavati sigurnosna pravila sistema.
```

Do not show the temporary password, bootstrap environment values, or backend configuration details.

### Auth-aware route guards

Refine the Unit 16 route guard into explicit session-state routing.

The route decision must account for:

```txt
loading
unauthenticated
authenticated + mustChangePassword
authenticated + password change complete
error
```

Requirements:

- Keep public auth routes separate from normal protected app routes.
- Prevent protected route content from flashing before session resolution.
- Prevent redirect loops.
- Preserve safe internal return paths for unauthenticated navigation.
- Do not trust a return-path query parameter without validation.
- Do not use client route guards as a replacement for backend authorization.
- Handle a backend `password_change_required` response from any protected API as a session-flow signal:
  - invalidate/refetch session when appropriate;
  - route to `/change-password`;
  - do not repeatedly retry the blocked request.

Centralize route decision helpers where practical instead of repeating conditions in every page.

### User menu and logout

Complete the user-menu/logout behavior introduced in Unit 16.

Requirements:

- Show only safe currently available session information, such as email or display name when present.
- Do not invent roles, teams, permissions, avatars, or account-management links.
- Logout must use the existing CSRF-protected backend endpoint.
- Disable duplicate logout requests.
- Clear/refetch session state after logout.
- Navigate to `/sign-in` after successful logout.
- If logout fails, show safe recoverable feedback and do not falsely render the session as logged out.

Users on `/change-password` must have an accessible logout action even if the normal app shell is not rendered.

### Existing auth shell cleanup

Remove temporary Unit 16 messages that say login is not connected or unavailable.

Keep forgot/reset password routes honest:

- They may remain unavailable placeholders.
- Do not submit email addresses or reset tokens.
- Do not add backend calls that do not exist.
- Keep a link back to sign-in.

Remove dead mock handlers, disabled-demo submission code, and stale TODO comments that are fully resolved by this unit. Do not remove future-flow placeholders that are still accurate and intentionally scoped.

### Accessibility and interaction states

Verify:

- Every input has an accessible label.
- Validation errors are associated with their fields.
- Form-level authentication errors are announced or otherwise discoverable.
- Focus moves predictably after validation or server failure where practical.
- Buttons expose pending/disabled state.
- Passwords are not shown in plain text by default.
- Any show-password control added must be keyboard accessible and have a clear accessible name.
- Status and error meaning does not rely on color alone.

Do not add a show-password control unless it can be implemented cleanly with existing primitives and accessibility requirements.

### Documentation updates

Update `context/progress-tracker.md` during implementation:

- Mark Unit 19 in progress when work starts.
- Mark it complete only after all applicable verification passes.
- Record the final frontend login/change-password/session contracts.
- Record the safe return-path approach.
- Record the session-cache update/invalidation behavior.
- Record any manual end-to-end verification limitations.
- Set Unit 20 as next when this unit is complete.

Update architecture, UI context, code standards, or workflow rules only if implementation changes an existing project-level decision. Do not modify context files merely to restate the implementation.

## Dependencies

None expected.

Use the existing React Router/routing solution, TanStack Query, React Hook Form, Zod, shared API client, shadcn/ui primitives, Lucide icons, formatting tooling, and frontend auth foundation introduced by previous units. Do not add another auth library, token library, form library, state library, notification framework, localization framework, test framework, or routing package unless an actual missing dependency makes the implementation impossible and the reason is documented first.

## Verification checklist

- [ ] `AGENTS.md`, all required context files, the current build plan, and this feature spec were read before implementation.
- [ ] Relevant frontend skills from `frontend/.agents/` were used when applicable without overriding project context.
- [ ] Final Unit 18 login, session, CSRF, change-password, logout, and ProblemDetails contracts were inspected before frontend types were finalized.
- [ ] The sign-in form calls the real `POST /api/auth/login` endpoint.
- [ ] Login uses the approved CSRF mechanism and includes cookie credentials.
- [ ] No auth token, authentication cookie, submitted password, CSRF token, or session payload is persisted in `localStorage` or `sessionStorage`.
- [ ] Invalid credentials display a generic Bosnian Latin error without revealing whether the account exists.
- [ ] Account-unavailable and unexpected errors use safe, recoverable feedback.
- [ ] Successful login updates/refetches the session before final navigation.
- [ ] Users with `mustChangePassword = true` are routed to `/change-password`.
- [ ] Users requiring a password change cannot navigate into normal protected app routes through frontend navigation.
- [ ] `/change-password` contains current, new, and confirmation password fields using React Hook Form and Zod.
- [ ] Password confirmation mismatch is validated client-side.
- [ ] Backend password policy remains authoritative.
- [ ] Password change uses the approved CSRF mechanism and includes cookie credentials.
- [ ] Failed password changes do not navigate away or falsely update the session.
- [ ] Successful password change refreshes the session and confirms `mustChangePassword = false` before normal app navigation.
- [ ] An unauthenticated user cannot remain on `/change-password`.
- [ ] An authenticated user who no longer requires a password change is redirected away from `/change-password`.
- [ ] Safe internal return-path preservation works and rejects external or loop-causing destinations.
- [ ] A backend `password_change_required` response routes the user into the required-password-change flow without infinite retries.
- [ ] Logout works from the normal user menu and from the required-password-change page.
- [ ] Logout does not falsely clear UI session state when the backend request fails.
- [ ] Temporary Unit 16 “login unavailable” behavior and resolved demo/TODO code were removed.
- [ ] Forgot/reset-password surfaces remain honest unavailable states; no unsupported backend calls were added.
- [ ] No public registration, invitation acceptance, roles, permissions, team scopes, staff management, JWT, OAuth, social login, or backend code was added.
- [ ] Visible authentication copy is Bosnian Latin and does not mix languages on the same screen.
- [ ] UI uses existing semantic tokens and shadcn/ui behavior without raw palette classes, hardcoded colors, dark mode, or ad-hoc primitive visual overrides.
- [ ] Source imports use the `@/` alias and do not introduce deep relative paths.
- [ ] Session server state remains in TanStack Query and is not duplicated in Zustand or React Context.
- [ ] Inputs, errors, focus states, keyboard submission, pending states, and logout controls meet the accessibility requirements in this spec.
- [ ] Frontend formatting was applied with the existing format command.
- [ ] Frontend `format:check` passes.
- [ ] Frontend lint passes.
- [ ] Frontend TypeScript/Vite build passes.
- [ ] Existing backend auth/integration tests still pass against the implemented contracts.
- [ ] Manual end-to-end verification confirms: protected-route redirect → sign in → required password change → protected app entry → logout.
- [ ] `context/progress-tracker.md` reflects the actual Unit 19 result and identifies Unit 20 as next.
