# Unit 16: Frontend Auth Shell and Session Provider

## Goal

Add the frontend authentication shell, session provider, auth-aware route protection, and user menu baseline for the staff-only application. This unit must connect to the existing backend session, CSRF, and logout endpoints without implementing real login, password reset, registration, roles, team scopes, or authorization logic.

## Design

This is a frontend foundation unit for the closed FK Velež Mostar staff authentication experience.

The backend currently provides session inspection, CSRF token issuance, logout, secure HttpOnly cookie authentication behavior, and API-friendly unauthenticated/forbidden responses. It does not yet provide real login, forgot-password, reset-password, invitation setup, first-admin bootstrap, roles, team scopes, or permission APIs. The frontend must respect that boundary.

The UI must stay light-only and follow the FK Velež red, white, and gold identity through existing semantic tokens. Do not add dark mode, theme toggles, hardcoded colors, raw Tailwind palette classes, or ad-hoc shadcn/ui visual overrides.

The default visible UI language is Bosnian Latin. Route names, code identifiers, API contract fields, and translation keys remain English. Visible text should be short, operational, and localizable later.

The session model for this unit is intentionally minimal:

- The frontend asks the backend whether a browser has an authenticated staff session.
- The backend remains the source of truth for authentication state.
- The frontend does not store tokens in `localStorage` or `sessionStorage`.
- The frontend does not create fake users or fake authenticated sessions.
- The frontend route guard is a UX boundary only; server-side authorization remains required on all protected backend endpoints.

Auth page behavior must be honest about the current backend scope:

- Sign-in page shell may exist, but real sign-in submission must remain disabled or clearly unavailable until a backend login endpoint exists.
- Forgot-password and reset-password surfaces may be added only as route shells or unavailable states because backend support does not exist yet.
- Do not imply that password reset, invite setup, or first-admin setup works before backend endpoints exist.

Out of scope for this unit:

- Backend login endpoint.
- Backend forgot-password endpoint.
- Backend reset-password endpoint.
- First admin bootstrap.
- Staff invitation/setup flow.
- Role, team-scope, or permission model.
- Navigation filtering by role or team scope.
- User-management UI.
- Real protected product data fetching.
- Public registration.
- Player login.
- JWT, OAuth, social login, or third-party auth providers.
- Storing auth tokens in browser storage.
- Backend code changes.

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
8. `context/feature-specs/16-frontend-auth-shell-session-provider.md`

Also inspect the implemented outputs from these dependencies:

- Unit 03 routing foundation.
- Unit 05 common UI primitives.
- Unit 10 frontend API client and environment foundation.
- Unit 11 form and validation foundation.
- Unit 15 backend CSRF and session API.

### Auth feature structure

Create a frontend auth feature area using the existing frontend folder conventions.

Recommended structure:

```txt
frontend/src/features/auth/
├── api/
├── components/
├── hooks/
├── types/
└── index.ts
```

Adapt the exact structure to the existing codebase if Unit 03, Unit 05, Unit 10, or Unit 11 already established a more specific convention.

Requirements:

- Keep auth-specific API helpers inside the auth feature.
- Keep shared API client behavior in the existing shared API client from Unit 10.
- Use `@/` imports for frontend source imports.
- Avoid deep relative imports.
- Keep components small and single-purpose.
- Do not place auth state in Zustand.
- Do not duplicate backend session data in multiple global stores.

### Session types

Define frontend session types aligned with the Unit 15 backend session response.

Recommended TypeScript shape:

```ts
export interface SessionUser {
  id: string;
  email: string;
  accountStatus: string;
  mustChangePassword: boolean;
}

export interface SessionResponse {
  isAuthenticated: boolean;
  user: SessionUser | null;
}
```

Rules:

- Keep API field names in English.
- Do not invent roles, team scopes, permissions, allowed actions, avatar fields, display names, or navigation rights in this unit.
- Treat unknown or malformed session responses as unauthenticated or error states through the existing API client/error pattern.
- Do not expose password, reset-token, invite-token, security-stamp, lockout, normalized Identity, or internal backend fields in frontend types.

### Auth API helpers

Add small auth API helpers that use the existing Unit 10 API client.

Required helpers:

- `getSession()` for `GET /api/auth/session`.
- `getCsrf()` for `GET /api/auth/csrf` when needed before logout or future unsafe auth actions.
- `logout()` for `POST /api/auth/logout`.

Requirements:

- Use the existing configured API base URL.
- Preserve `credentials: "include"` behavior for cookie-authenticated requests.
- Send the CSRF header required by Unit 15 for logout.
- Do not store CSRF tokens or auth tokens in `localStorage` or `sessionStorage`.
- Do not add login, forgot-password, reset-password, invite setup, or first-admin API calls.
- Keep error parsing compatible with the existing ProblemDetails-aware API client.

If the CSRF token is supplied through a readable `XSRF-TOKEN` cookie, implement a small helper to read only that CSRF cookie value for request-header use. Do not read or expose authentication cookie values.

If Unit 15 returns the CSRF token body value instead of relying only on a readable cookie, consume only that value and keep it in memory for the request that needs it.

### Session provider

Create a session provider using TanStack Query or the existing Unit 10 provider conventions.

Requirements:

- Fetch the current session from `GET /api/auth/session`.
- Expose session state through a small hook such as `useSession()`.
- Provide clear states for:
  - loading
  - unauthenticated
  - authenticated
  - error
- Do not use Zustand for server-owned session data.
- Do not use React Context for frequently changing server state if TanStack Query already owns it.
- React Context may wrap stable helper functions and query access if needed, but the session data itself should remain TanStack Query-owned.
- Configure sensible retry behavior so an unauthenticated session does not produce noisy repeated retries.
- Invalidate or refetch the session after logout.
- Do not create fake session data for local development.

Recommended hook output:

```ts
interface UseSessionResult {
  session: SessionResponse | undefined;
  user: SessionUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  isError: boolean;
  error: unknown;
  refetchSession: () => void | Promise<unknown>;
}
```

Adapt the exact type to the existing code style, but keep the contract small and explicit.

### Route protection

Add frontend protected-route handling using the routing system introduced in Unit 03.

Requirements:

- Public routes:
  - sign-in page
  - forgot-password unavailable/shell page if implemented
  - reset-password unavailable/shell page if implemented
  - not-found page
- Protected routes:
  - existing app shell routes from Unit 03.
  - dashboard and module placeholder pages.
- While session is loading, show the existing common loading state.
- If unauthenticated, redirect or navigate to the sign-in route.
- Preserve the originally requested path where practical so a future successful login can return users to that page.
- Avoid redirect loops between auth pages and protected routes.
- Authenticated users who visit the sign-in page should be redirected to the main app entry route.
- Do not implement role-based redirects, permission-based navigation, or team-scope filtering yet.

Recommended route paths:

```txt
/sign-in
/forgot-password
/reset-password
```

Use English route paths to match the project rule that route names remain English. Use Bosnian Latin for visible page text.

### Sign-in shell

Create a sign-in page shell that matches the light-only app identity.

Requirements:

- Use existing app-level layout/common primitives where appropriate.
- Keep the page simple and professional.
- Use Bosnian Latin visible copy.
- Include FK Velež / system identity text, for example:
  - `FK Velež Mostar`
  - `Player Performance Data System`
  - `Prijava za osoblje`
- If email/password fields are shown, the submit action must be disabled or clearly unavailable until a backend login endpoint exists.
- Prefer an honest message such as:
  - `Prijava će biti omogućena nakon povezivanja backend login endpointa.`
- Do not call non-existent login APIs.
- Do not fake successful login.
- Do not store email/password values outside local form state.
- Do not add public registration links.
- Do not add social login buttons.
- Do not add decorative hero sections, gradients, or oversized marketing content.

If using the Unit 11 form foundation, keep validation minimal and do not overbuild login validation before the backend login contract exists.

### Forgot/reset password shells

If the app includes forgot-password and reset-password routes in this unit, implement them as unavailable route shells only.

Requirements:

- Use Bosnian Latin visible copy.
- State clearly that password reset is not available until backend support exists.
- Provide a link back to sign-in.
- Do not collect reset tokens.
- Do not submit email addresses.
- Do not call non-existent backend endpoints.
- Do not add email-provider behavior.

If adding these routes creates unnecessary UI noise in the current app, it is acceptable to keep them out of the visible navigation and link them only from the sign-in page as disabled/unavailable text. Do not invent working flows.

### User menu placeholder and logout

Add a user menu area to the app shell/topbar that can display authenticated session data when present.

Requirements:

- For authenticated sessions, show minimal staff identity such as email.
- Provide a logout action that calls `POST /api/auth/logout` with CSRF protection.
- After logout succeeds, invalidate/refetch the session and navigate to the sign-in page.
- If logout fails, show a compact error state or message using existing common UI patterns.
- Do not add profile settings, avatar upload, account management, role labels, team scope labels, or permission badges yet.
- Do not show fake user identity in unauthenticated state.

If no authenticated session can be manually created yet because first-admin/login support is not implemented, ensure the component still compiles and handles the unauthenticated state safely.

### App shell integration

Wire the session provider into the frontend app provider tree.

Requirements:

- The session provider must sit inside the existing TanStack Query provider if it depends on TanStack Query.
- Do not create multiple QueryClient providers.
- Do not move unrelated app shell, routing, or common UI code unless required for the auth boundary.
- Keep protected route logic close to routing/layout code, not scattered across every page.
- Existing placeholder pages should remain minimal and Bosnian Latin by default.
- Do not add real dashboard data, user data, match data, player data, or settings data.

### Documentation updates

Update `context/progress-tracker.md` during implementation to reflect:

- Unit 16 is in progress when work starts.
- Unit 16 is complete only after verification passes.
- Frontend session provider exists.
- Auth route shells exist.
- Protected route behavior exists.
- Real login/password-reset flows remain blocked until backend endpoints exist.
- Any manual verification gap caused by lack of first-admin/login support.

Do not update `context/architecture.md`, `context/ui-context.md`, or `context/code-standards.md` unless implementation changes a documented project-level decision.

## Dependencies

None expected.

Use packages and foundations already introduced by earlier units:

- existing frontend router from Unit 03
- existing common UI primitives from Unit 05
- existing API client and TanStack Query setup from Unit 10
- existing form/validation foundation from Unit 11 where useful
- existing backend session, CSRF, and logout endpoints from Unit 15

Do not install auth-provider packages, JWT libraries, OAuth/social-login packages, cookie-management libraries, global state libraries, UI kits, email packages, or speculative dependencies in this unit.

If a tiny browser cookie helper is truly needed for reading only the CSRF cookie, prefer a local utility instead of adding a dependency.

## Verification checklist

- [ ] `AGENTS.md` and all required context files were read before implementation.
- [ ] Frontend auth feature files are organized within the approved frontend boundaries.
- [ ] Session types match the Unit 15 session API shape.
- [ ] `getSession()` calls `GET /api/auth/session` through the existing API client.
- [ ] `getCsrf()` calls `GET /api/auth/csrf` or otherwise follows the Unit 15 CSRF token pattern.
- [ ] `logout()` calls `POST /api/auth/logout` with CSRF protection.
- [ ] Auth API helpers preserve `credentials: "include"`.
- [ ] No auth tokens are stored in `localStorage` or `sessionStorage`.
- [ ] No fake authenticated session or fake staff user is created.
- [ ] Session state is owned by TanStack Query, not duplicated in Zustand.
- [ ] `useSession()` or equivalent hook exposes loading, authenticated, unauthenticated, and error states.
- [ ] Session query retry behavior does not create noisy retries for unauthenticated users.
- [ ] Protected routes show loading state while session status is loading.
- [ ] Unauthenticated users are redirected or navigated to the sign-in route.
- [ ] Authenticated users visiting the sign-in route are redirected to the main app entry route.
- [ ] Redirect loops are avoided.
- [ ] Sign-in page shell exists and uses Bosnian Latin visible copy.
- [ ] Sign-in page does not call a non-existent login endpoint.
- [ ] Sign-in page does not fake successful login.
- [ ] Public registration is not added.
- [ ] Social login is not added.
- [ ] Forgot/reset password routes, if added, are clearly unavailable shells and do not submit data.
- [ ] User menu placeholder handles authenticated session data when present.
- [ ] Logout action invalidates/refetches the session and navigates to sign-in on success.
- [ ] Logout errors are handled with compact UI feedback.
- [ ] Existing app shell/routing placeholders remain minimal and Bosnian Latin by default.
- [ ] No role, team-scope, permission, or navigation authorization model is implemented.
- [ ] No real dashboard, players, matches, reports, imports, settings, or staff-management feature is implemented.
- [ ] No backend code is changed in this unit.
- [ ] No new speculative dependencies are installed.
- [ ] Frontend imports use the `@/` alias where appropriate.
- [ ] No raw Tailwind palette classes or hardcoded colors are introduced in application UI.
- [ ] Generated shadcn/ui primitive files are not modified unless an existing component bug truly requires it and the reason is documented.
- [ ] `npm run format` is run for changed frontend files.
- [ ] `npm run format:check` passes for the frontend.
- [ ] `npm run lint` passes for the frontend if configured.
- [ ] `npm run build` passes for the frontend.
- [ ] Manual verification confirms unauthenticated app routes go to the sign-in shell without a redirect loop.
- [ ] Manual verification confirms auth public routes render without requiring a session.
- [ ] Manual verification confirms the app handles backend session endpoint errors with a safe error state.
- [ ] `context/progress-tracker.md` is updated to reflect the actual implementation state and any known gaps.
