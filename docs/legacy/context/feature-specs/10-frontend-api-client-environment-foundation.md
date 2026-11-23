# Unit 10: Frontend API Client and Environment Foundation

## Goal

Add the frontend API/environment foundation required for future authenticated and data-driven features: a typed API client, safe error parsing, frontend environment configuration, and a TanStack Query provider.

This unit does not call real domain endpoints, does not implement authentication, and does not build any product workflow.

## Design

The frontend must stay aligned with the existing React/Vite architecture, light-only FK Velež UI direction, and Bosnian Latin default language decision.

This unit is infrastructure-focused. Visible UI changes should be minimal or nonexistent. If any visible placeholder or fallback copy is introduced, it must be short and in Bosnian Latin by default.

The API client must be reusable by future feature folders, but it must not assume any specific domain module such as players, matches, imports, users, medical data, or reports.

The implementation must support the backend API conventions already introduced:

- backend responses may use ProblemDetails-compatible error shapes
- safe trace/request identifiers may be present in error responses
- unexpected errors must be handled without leaking internal implementation details to the UI
- future authenticated requests will use backend-managed secure cookies, so the client must support credentialed requests

The implementation must not introduce speculative abstractions such as generated API clients, OpenAPI codegen, request retry frameworks, mock servers, or domain-specific API wrappers.

## Implementation

### Frontend environment configuration

Add frontend environment documentation and validation for the backend API base URL.

Create or update:

- `frontend/.env.example`
- `frontend/src/lib/env.ts` or an equivalent shared environment module

The environment module must:

- read `import.meta.env.VITE_API_BASE_URL`
- expose a typed frontend environment object
- validate that `VITE_API_BASE_URL` is present when the application needs to call the backend
- normalize the API base URL by removing trailing slashes
- avoid exposing or expecting secrets in any `VITE_*` variable

`frontend/.env.local` must remain ignored and must not be committed.

Recommended example value:

```env
VITE_API_BASE_URL=http://localhost:5000
```

If the current backend development URL differs, use the actual local backend URL from the existing project setup.

### API error types

Create shared API error types in a frontend-owned location such as:

```txt
frontend/src/lib/api/
```

Add types for:

- ProblemDetails-compatible errors
- field/validation error details if returned by the backend later
- normalized frontend API errors

The normalized API error shape should include, where available:

- HTTP status
- title or short message
- detail or safe description
- trace/request identifier
- validation errors
- original response body as `unknown` only when useful for debugging, not for UI rendering

Use TypeScript `interface` for object contracts where appropriate. Avoid `any`.

### Typed fetch wrapper

Create a reusable API client module such as:

```txt
frontend/src/lib/api/api-client.ts
```

The client must:

- build URLs from the configured API base URL
- accept path strings like `/health` or future `/api/...` paths
- use `credentials: "include"` by default for future cookie-authenticated requests
- set `Accept: application/json` by default
- set `Content-Type: application/json` only when a JSON body is sent
- parse JSON responses safely
- handle empty responses without throwing JSON parse errors
- throw a normalized API error for non-2xx responses
- preserve `AbortSignal` support
- allow callers to pass request options without bypassing default safety behavior

Do not create domain-specific methods in this unit. Avoid wrappers such as `getPlayers`, `createMatch`, `login`, `logout`, or `getCurrentUser`.

A small generic helper is acceptable, for example:

```ts
apiRequest<TResponse>(path, options)
```

The helper must remain generic and feature-neutral.

### TanStack Query provider

Install and configure TanStack Query for frontend server state.

Create or update an app-level provider composition file, for example:

```txt
frontend/src/app/providers.tsx
```

The provider must:

- create a stable `QueryClient`
- wrap the application with `QueryClientProvider`
- use conservative defaults suitable for internal dashboard workflows
- avoid storing server data in Zustand or React Context
- avoid adding React Query Devtools in this unit unless already installed and explicitly present

The provider should be wired into the existing frontend root without changing route behavior or page content beyond what is necessary.

Recommended Query defaults:

- avoid aggressive retries for mutation-like failures
- use predictable stale time defaults
- keep behavior simple and easy to override per feature later

Do not introduce real API calls from pages or components in this unit.

### Frontend folder alignment

Keep shared API and environment code in stable shared folders, preferably:

```txt
frontend/src/lib/api/
frontend/src/lib/env.ts
frontend/src/app/providers.tsx
```

Use the `@/` alias for source imports.

Do not add deep relative imports such as `../../../` for frontend source files.

### Placeholder UI constraints

This unit should not create new user-facing product screens.

If the provider wiring requires touching existing placeholder pages, preserve existing minimal Bosnian Latin copy and do not introduce new domain behavior.

Do not add dashboard metrics, fake data, table examples, form demos, or sample API output to visible UI.

### Progress tracker update

After implementation, update `context/progress-tracker.md` to reflect:

- Unit 10 implemented
- TanStack Query provider added
- frontend API client and environment foundation added
- any verification command that could not be run and why

Do not mark future domain modules as started.

## Dependencies

Install frontend dependency only if it is not already installed:

- `@tanstack/react-query` — server-state provider and query/mutation foundation for future API-driven frontend features

Do not install:

- React Query Devtools
- OpenAPI generators
- Axios
- mock service workers
- test libraries
- auth libraries
- localization libraries
- additional state-management packages

## Verification checklist

- [ ] `frontend/.env.example` documents `VITE_API_BASE_URL` and contains no secrets.
- [ ] Real local frontend environment files remain ignored and are not committed.
- [ ] Frontend environment module exposes a typed, normalized API base URL.
- [ ] API client uses `credentials: "include"` by default.
- [ ] API client safely handles JSON responses, empty responses, abort signals, and non-2xx responses.
- [ ] API error handling supports ProblemDetails-compatible backend errors without leaking unsafe internals into visible UI.
- [ ] TanStack Query provider is wired at the app root.
- [ ] No real domain endpoint wrappers were added.
- [ ] No auth behavior was implemented.
- [ ] No backend code was changed unless required only for compatibility with existing configuration.
- [ ] No visible UI copy mixes Bosnian and English unnecessarily.
- [ ] Frontend imports use the `@/` alias where applicable.
- [ ] No raw Tailwind palette classes or hardcoded colors were introduced.
- [ ] Generated shadcn/ui primitive files were not modified.
- [ ] `npm run format` has been run for affected frontend files.
- [ ] `npm run format:check` passes in the frontend project.
- [ ] `npm run lint` passes in the frontend project if configured.
- [ ] `npm run build` passes in the frontend project.
- [ ] `dotnet build` still passes if backend files were touched.
- [ ] `dotnet test` still passes if backend files were touched.
- [ ] `context/progress-tracker.md` is updated with the completed unit state after implementation.
