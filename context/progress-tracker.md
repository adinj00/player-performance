# Progress Tracker

Update this file after every meaningful implementation change.

This file intentionally starts lightweight. It should become more detailed as build units are added under `/feature-specs` and implementation work begins.

## Current Phase

- Feature implementation kickoff

## Current Goal

- Unit 27: Players Backend Foundation is complete.

## Completed

- Unit 27 completed:
  - Added the persistent club-level `Player` aggregate with only first name, last name, optional preferred name, optional `DateOnly` date of birth, `PlayerRecordStatus`, and UTC created/updated timestamps. There is no team/current-selection foreign key or other future player metadata.
  - Added explicit create/profile-update/lifecycle use cases with trimmed Unicode-safe names, future-date rejection, ACTIVE creation, archive/restore behavior (restore returns INACTIVE), and archived-record update/activation/deactivation conflicts.
  - Added admin-only, authenticated, password-change-gated `/api/players` list, detail, create, update, activate, deactivate, archive, and restore routes. Non-administrators receive `403`; no hard-delete route exists.
  - Added database-side filtering, deterministic last-name/first-name/id ordering, paging metadata, archived-default exclusion, status/search support, EF configuration, and the focused `20260711120000_AddPlayersBackendFoundation` migration. Team assignment history remains deferred to Unit 28, non-admin team-scoped player access remains deferred until those assignments exist, and player mutation audit persistence remains deferred to Unit 38.
  - Added domain and integration coverage for name normalization, date validation, lifecycle constraints, authorization, duplicate names, filtering, archive/restore, archived update conflicts, validation, and missing/delete routes. The API `.http` file contains player request examples.
  - Verification passed in isolated outputs: `dotnet restore backend/PlayerPerformance.sln`, `dotnet build backend/PlayerPerformance.sln --no-restore` with zero warnings/errors, `dotnet test backend/PlayerPerformance.sln --no-build` (40 unit and 27 integration tests), repository whitespace formatting/check, and `git diff --check`.
  - After the authorized API process was stopped, EF migration discovery showed `20260711120000_AddPlayersBackendFoundation (Pending)` and `dotnet ef database update` applied it successfully to the configured PostgreSQL development database. The EF CLI reported only the existing tools/runtime version warning (`8.0.0` tools vs `8.0.10` runtime).

- Unit 26 completed:
  - Replaced the settings placeholder with administrator-only, nested routes for seasons, competitions, teams/selections, venues, and opponents; `/settings` redirects to seasons and the sidebar hides `Postavke` for non-administrators.
  - Added typed, CSRF-capable settings API clients and TanStack Query hooks for the existing Unit 21, Unit 22, and Unit 25 contracts. Archived visibility is URL-backed with `archived=include`; server ordering is preserved.
  - Added reusable settings navigation, RHF/Zod forms, lifecycle confirmations, table states, centralized Bosnian Latin team/tracking-level labels, date-only season formatting, and accessible move-up/move-down ordering controls that submit the complete non-archived team ID list.
  - No packages, shadcn primitives, backend files, or API contracts were added or changed.
  - Verification passed: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint` (two pre-existing/non-blocking React Compiler compatibility warnings, including React Hook Form usage in this unit), `npm.cmd run build`, and `git diff --check`.

- Settings list endpoint correction:
  - Made the `includeArchived` query value optional with a `false` default for seasons, competitions, venues, opponents, and teams. This matches the documented default-list contract and the frontend behavior, which only sends `includeArchived=true` when archived visibility is selected.
  - Configured an explicit invalid enum sentinel for `StaffAccessProfile.TeamScopeType`, removing EF Core's generated-default warning while preserving explicit `ALL_TEAMS` and `SELECTED_TEAMS` values and the existing database default.
  - Verification passed: isolated `dotnet restore`, `dotnet build PlayerPerformance.sln --no-restore`, and `dotnet test PlayerPerformance.sln --no-restore --no-build` (36 unit and 25 integration tests), `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes`, and `git diff --check`.
  - Replaced the archived-record native checkbox with the established shadcn checkbox primitive.
  - Added and applied `20260711082629_RepairVenuesAndOpponentsSchema`, an idempotent repair migration that creates the Unit 25 `venues` and `opponents` tables plus normalized-name indexes only when missing. It repairs local databases where the original Unit 25 migration was present in history but its schema was absent.
  - Follow-up verification passed: backend whitespace check, isolated restore/build/test (36 unit and 25 integration tests), frontend formatting/format check/lint/build, and `git diff --check`. Frontend lint retains its two non-blocking React Compiler compatibility warnings.
  - Removed duplicate create actions from settings empty states; each resource now keeps its single persistent create action in the top-right page controls.

- Unit 25 implementation is ready for final EF migration verification:
  - Added separate `Venue` and `Opponent` aggregates with only the approved name, normalized-name, archive, and UTC timestamp fields. Both use the canonical settings normalization and soft archive/restore lifecycle; archived records cannot be normally updated.
  - Extended the existing Settings application slice, repository, EF Core model, and admin-only/password-change-gated `/api/settings/venues` and `/api/settings/opponents` endpoints. Lists sort by normalized name then identifier, exclude archived records by default, and accept `includeArchived=true`.
  - Added a focused `20260711110000_AddVenuesAndOpponents` migration that creates only `venues` and `opponents`, including required names, archive/timestamp columns, and unique normalized-name indexes. No seed records, match relationships, deletion endpoints, or speculative metadata were added.
  - Added domain and integration coverage for lifecycle behavior, administrator authorization, CSRF-protected mutations, independent venue/opponent namespaces, and archived-list filtering. The API `.http` file now documents every new route.

- Unit 22 completed:
  - Added the canonical `Team` aggregate with server-owned normalized names, display order, and UTC timestamps; `TeamTrackingLevel` is fixed to `BASIC`, `STANDARD`, and `FULL`, while `TeamStatus` is fixed to `ACTIVE`, `INACTIVE`, and `ARCHIVED`.
  - Added admin-only, password-change-gated `/api/settings/teams` list, get, create, update, reorder, activate, deactivate, archive, and restore operations. Team names are trimmed and normalized with the established deterministic rule; the database unique index protects names across all lifecycle states and duplicate names return safe `409` responses.
  - Added contiguous non-archived ordering, complete-list reorder validation, archive normalization, and restore append behavior. Restored teams always return as `INACTIVE`; administrators explicitly activate them when appropriate.
  - Added the `20260710143906_AddTeamsSelections` migration, string-persisted tracking/status enums, non-negative display-order constraint, relevant indexes, and idempotent startup seeding. An empty teams table seeds `First Team` (`FULL`), `U19`/`U17` (`STANDARD`), and `U15`/`U13`/`U11` (`BASIC`) in that order; populated tables are not supplemented or reset.
  - Added domain and integration coverage for aggregate lifecycle rules, default seeding, admin authorization, CSRF-protected mutations, and archived-list filtering. Staff team-scope assignments intentionally remain deferred to Unit 23.

- Unit 21 completed:
  - Added `Season` (`Id`, display/normalized name, `DateOnly` start/end dates, archive flag, and UTC created/updated timestamps) and `Competition` (the same fields except dates) as EF-free Domain entities with explicit update, archive, and restore behavior.
  - Added focused Application settings use cases and FluentValidation contracts for list, get, create, update, archive, and restore. Names are trimmed and normalized with `Trim().ToUpperInvariant()`; unique normalized-name indexes remain the final concurrency authority while provider unique violations return a safe `409 duplicate_name` response.
  - Added admin-only, password-change-gated routes: `GET/POST /api/settings/seasons`, `GET/PATCH /api/settings/seasons/{id}`, `POST /api/settings/seasons/{id}/archive`, `POST /api/settings/seasons/{id}/restore`, plus the equivalent `/api/settings/competitions` routes. Lists exclude archived records by default and accept `includeArchived=true`; archive/restore are idempotent and archived records cannot be normally updated.
  - Added Infrastructure mappings, settings repository, and the `20260710131639_AddSettingsSeasonsAndCompetitions` migration. It creates only `seasons` and `competitions`, with `date` season columns, archive defaults, server-owned timestamps, and unique normalized-name indexes; no seed data or unrelated settings entities were added.
  - Added `FluentValidation.DependencyInjectionExtensions` 11.11.0 and focused domain/validator tests for date range, lifecycle idempotency, normalization, blank values, and maximum length.

- Unit 20 completed:
  - Added the canonical `StaffRole` model with `ADMIN`, `DATA_OPERATOR`, `ANALYST`, `COACH`, `MEDICAL_STAFF`, and `VIEWER`, plus explicit `CanVerifyReports`, `CanImportData`, and `CanViewMedicalDetails` permissions.
  - Added the authoritative one-to-one `staff_access_profiles` persistence model keyed by the existing Identity user ID. Roles persist as their stable uppercase string values and non-admin permission flags default to `false`.
  - Added the Application-safe `ICurrentUserAccess` abstraction and a scoped, request-cached Infrastructure resolver. It reads current persisted account/profile data, treats unavailable or profile-less users as non-authorized, and leaves an extension point for Unit 23 team scope.
  - Registered centralized `AdminOnly`, `CanVerifyReports`, `CanImportData`, and `CanViewMedicalDetails` policies. `ADMIN` satisfies all policies; non-admin users require the matching persisted flag; unauthenticated callers remain `401` and insufficient/profile-less callers receive `403`.
  - Added the idempotent first-admin role handoff after the existing bootstrap operation. It uses the configured bootstrap email when available, otherwise permits the documented single-user/no-profile fallback, and fails safely on ambiguous fallback state. It does not alter credentials, account status, lockout state, or password-change state.
  - Enriched authenticated `GET /api/auth/session` users with `primaryRole` and effective `permissions` (`canVerifyReports`, `canImportData`, `canViewMedicalDetails`). No team IDs, persistence identifiers beyond the existing compatible user ID, or Identity security metadata are exposed.
  - Added the `AddStaffAccessProfiles` and `AddStaffAccessProfileUserForeignKey` EF Core migrations and verified them against the configured local PostgreSQL database. The access profile has a unique one-to-one foreign-key reference to the Identity user. Selected-team scope remains intentionally deferred to Unit 23.

- Unit 19 completed:
  - Connected the React Hook Form and Zod sign-in form to `POST /api/auth/login` and added the protected `/change-password` form for `POST /api/auth/change-password`.
  - Confirmed the frontend auth contract: login and password change return `SessionResponse`; session is read from `GET /api/auth/session`; unsafe requests first acquire `GET /api/auth/csrf` and send its returned request token in the `X-CSRF-TOKEN` header while the API-issued `XSRF-TOKEN` cookie is included automatically; logout remains `POST /api/auth/logout` with the same CSRF convention.
  - Kept session server state in TanStack Query. Login and password-change mutations replace the cache with the direct response, invalidate it, then refetch the session before navigation. Logout replaces the cache with the unauthenticated shape only after backend success.
  - Added explicit route decisions for loading, session error, unauthenticated, required-password-change, and completed-password-change states. Safe return paths are internal-only, reject protocol-relative/backslash paths, and reject authentication-loop routes.
  - Added a central `password_change_required` ProblemDetails signal that invalidates session state and routes to `/change-password` without retrying the rejected request.
  - Kept error messages safe and Bosnian Latin, avoided browser storage for session/CSRF/credentials, and retained honest unavailable forgot/reset-password surfaces.
  - Added accessible show/hide controls for every password input, using an icon button with Bosnian Latin accessible labels and no change to password storage or submission behavior.
  - Corrected CSRF wiring to use the backend-issued antiforgery request token rather than the companion cookie value, preventing the `400 Invalid CSRF token` login failure. Added an opt-out for shared-button press translation and applied it only to password visibility controls, then corrected the unavailable-password-flow link to render as a native link rather than a Base UI button.

- Product discovery completed for the initial V1 scope.
- Core V1 product direction defined: internal FK Velež Mostar player performance and match analysis system.
- Initial context files drafted:
  - `context/project-overview.md`
  - `context/architecture.md`
  - `context/ui-context.md`
  - `context/code-standards.md`
  - `context/ai-workflow-rules.md`
- Root `AGENTS.md` reviewed and kept as the universal context entry point.
- Unit 01 completed:
  - Removed default Vite/React demo UI and starter assets from `frontend/`.
  - Replaced the frontend root screen with a minimal Bosnian baseline placeholder using semantic theme utilities.
  - Preserved the existing Tailwind CSS v4, shadcn/ui, theme token, and `@/` alias foundation.
  - Removed backend WeatherForecast template code and cleaned the sample `.http` request file.
  - Removed stale missing-project references from `backend/PlayerPerformanceDataSystem.sln` so the existing backend baseline can build.
  - Adjusted frontend ESLint configuration so generated shadcn/ui primitives are not blocked by the React Fast Refresh export rule.
- Unit 02 completed:
  - Added a reusable frontend app composition in `frontend/src/app/app.tsx` and kept `frontend/src/App.tsx` as a thin root entry.
  - Implemented a responsive light-only app shell with a persistent desktop sidebar, mobile drawer behavior, top bar, and main content region.
  - Added grouped Bosnian Latin navigation placeholders for dashboard, performance, club, and administration areas without introducing routing or feature behavior.
  - Added a minimal empty main-content state so the shell remains static and ready for future feature specs.
  - Reused the existing `lucide-react` dependency for shell icons; no new frontend package was required.
- Unit 03 completed:
  - Added `react-router-dom` and introduced a frontend route foundation with `BrowserRouter`, route constants, and centralized route metadata.
  - Wired the existing app shell to render routed placeholder pages for dashboard, matches, players, training GPS, imports, teams, medical, media, users, and settings.
  - Converted sidebar navigation from inert buttons to active router links and kept mobile drawer close behavior after navigation.
  - Updated the top bar title to reflect the active route and added a routed Bosnian Latin not-found page with a return link to the dashboard.
  - Kept all route content as restrained placeholders without fake domain data, backend calls, auth behavior, or extra frontend state providers.
- Unit 04 completed:
  - Added Prettier as the frontend formatting source of truth with a single `frontend/.prettierrc` configuration.
  - Enabled Tailwind utility sorting through `prettier-plugin-tailwindcss`.
  - Added `frontend/.prettierignore` to exclude dependencies, build outputs, coverage output, and local environment files.
  - Added `format` and `format:check` frontend package scripts without changing the build pipeline.
  - Configured flat ESLint compatibility with `eslint-config-prettier` while preserving the existing project lint rules.
  - Ran a full frontend formatting pass and accepted formatting-only file changes without altering product behavior.
- Unit 05 completed:
  - Added reusable common UI primitives for page headers, empty states, loading states, error states, and bordered content sections in `frontend/src/components/common/`.
  - Refactored the dashboard placeholder, generic module placeholder, and not-found page to compose the new shared primitives instead of repeating bespoke markup.
  - Preserved the routed shell structure and kept placeholder content generic, Bosnian Latin, and free of fake domain datasets or backend integration.
  - Kept the work frontend-only with no backend changes.
- Unit 06 completed:
  - Replaced the old single-project backend starter layout with the documented backend baseline under `backend/src` and `backend/tests`.
  - Added `PlayerPerformance.Api`, `PlayerPerformance.Application`, `PlayerPerformance.Domain`, and `PlayerPerformance.Infrastructure` projects with Clean Architecture references in `backend/PlayerPerformance.sln`.
  - Added minimal `AddApplication()` and `AddInfrastructure()` composition extension points without registering speculative services.
  - Replaced the stock controller/swagger host with a minimal ASP.NET Core 8 API that exposes only `GET /health`.
  - Added a placeholder `backend/.env.example` and removed obsolete root-level backend template artifacts that conflicted with the new structure.
- Unit 07 completed:
  - Added API-owned backend configuration loading that reads `backend/.env` when present and refreshes ASP.NET Core environment-variable configuration without overriding already-set process environment variables.
  - Added typed `PlayerPerformanceOptions` with startup validation for required `ServiceName` and optional absolute HTTP/HTTPS `FrontendOrigin`.
  - Updated `backend/.env.example` with safe local-development guidance and non-secret example values for this unit only.
  - Updated `GET /health` to return the configured service name while keeping the response safe and free of machine details or secrets.
- Unit 08 completed:
  - Added API-owned ProblemDetails registration with a safe `traceId` extension for consistent error responses.
  - Added centralized exception handling that returns a generic `500` ProblemDetails response without leaking internal exception details.
  - Added status-code ProblemDetails handling for unknown routes and unsupported methods so `404` and `405` API responses are consistent and safe.
  - Moved endpoint registration into API-owned endpoint extension methods and preserved the unauthenticated `GET /health` endpoint through the new structure.
- Unit 09 completed:
  - Added `backend/tests/UnitTests/PlayerPerformance.UnitTests.csproj` and `backend/tests/IntegrationTests/PlayerPerformance.IntegrationTests.csproj`.
  - Added both test projects to `backend/PlayerPerformance.sln` using the documented backend test structure.
  - Added architecture dependency unit tests that verify the current Clean Architecture reference direction stays intact.
  - Added an ASP.NET Core `WebApplicationFactory`-based integration test foundation with safe in-memory configuration for startup validation.
  - Added integration tests covering `GET /health` and the safe unknown-route ProblemDetails behavior already implemented in Unit 08.
  - Added `public partial class Program;` to the API entry point so the existing app host is discoverable by integration tests without changing runtime behavior.
  - Standardized backend verification on `dotnet restore`, `dotnet build`, and `dotnet test` for future units.
- Unit 10 completed:
  - Added `frontend/.env.example` with a documented `VITE_API_BASE_URL` example and kept frontend local environment usage secret-free.
  - Added a shared frontend environment module that normalizes `VITE_API_BASE_URL` and throws a clear Bosnian Latin error only when backend calls are attempted without configuration.
  - Added shared frontend API error types plus a generic `apiRequest<TResponse>()` wrapper with safe JSON parsing, empty-response handling, normalized non-2xx errors, and default `credentials: "include"` support.
  - Added an app-level TanStack Query provider with conservative default query and mutation behavior and wired it into the existing app root without changing visible route behavior.
  - Added `@tanstack/react-query` as the only new frontend dependency required for this unit.
  - Moved frontend TypeScript incremental build info out of `node_modules/.tmp` into `frontend/.tmp/` and updated frontend Vite scripts/config so verification can build successfully in the current environment.
- Unit 11 completed:
  - Added `react-hook-form`, `zod`, and `@hookform/resolvers` to the frontend project as the form and schema-validation foundation required by this unit.
  - Added shared generic form helpers in `frontend/src/lib/form-errors.ts` for field error extraction, unknown error normalization, multi-error normalization, and a safe Bosnian Latin fallback message.
  - Added reusable app-level form UI components in `frontend/src/components/common/` for form-level error summaries, field-level error messages, action layout, and required-field indication.
  - Kept the implementation non-domain and non-routed: no real product form, auth flow, API mutation, backend change, or visible navigation change was introduced.
  - Documented the form convention inline near the shared helpers so feature-owned schemas can stay close to future forms while shared helpers remain generic.
- Unit 12 completed:
  - Added EF Core, EF Core design-time, Npgsql, and EF Core DbContext health-check package references only to `backend/src/Infrastructure`.
  - Added `Microsoft.EntityFrameworkCore.Design` to `backend/src/Api` as a design-time-only dependency so `dotnet ef` can use the API startup project reliably.
  - Added Infrastructure-owned persistence setup with `AppDbContext`, PostgreSQL `DbContext` registration, and a required `ConnectionStrings:DefaultConnection` startup check.
  - Added a database readiness health check exposed through `/health/ready` while preserving the existing safe `/health` endpoint behavior.
  - Added an initial empty persistence baseline migration and model snapshot under `backend/src/Infrastructure/Persistence/` without introducing speculative business tables.
  - Updated backend startup logging to a safe explicit provider set so unhealthy database readiness checks do not crash on Windows Event Log permission issues in local environments.
  - Updated `backend/.env.example` with a placeholder `ConnectionStrings__DefaultConnection` value following the existing local configuration convention.
- Unit 13 completed:
  - Added shared backend primitives under `backend/src/Domain/Common/` for entity identity, value-based equality, developer-facing error objects, result wrappers, and universal guard helpers.
  - Added framework-independent audit metadata contracts under `backend/src/Domain/Abstractions/Auditing/` without introducing audit entities, persistence hooks, or schema changes.
  - Added an application-facing `ISystemClock` abstraction and an Infrastructure `SystemClock` UTC implementation registered through the existing Infrastructure dependency injection extension point.
  - Added focused unit tests covering entity equality, value object equality, result invariants, guard helpers, and clock registration/UTC behavior.
  - Kept the unit product-agnostic: no FK Velež entities, API routes, auth behavior, migrations, or frontend changes were introduced.

- Unit 14 completed:
  - Added a framework-independent `UserAccountStatus` enum in `backend/src/Domain/Users/` with `INVITED`, `ACTIVE`, `DISABLED`, and `LOCKED`.
  - Added an Infrastructure-owned `ApplicationUser` Identity model with account status, password-change requirement, and created/updated timestamps using Guid identifiers.
  - Integrated the existing `AppDbContext` with user-only ASP.NET Core Identity persistence and stable staff-only table names: `staff_users`, `staff_user_claims`, `staff_user_logins`, and `staff_user_tokens`.
  - Registered IdentityCore, EF stores, token providers, and a custom sign-in manager hook that blocks future sign-in for disabled or locked accounts.
  - Added API-owned cookie authentication and authorization wiring with explicit password/lockout options, `RequireUniqueEmail`, `HttpOnly` cookies, explicit `SameSite=Lax`, environment-sensitive secure-cookie policy, and non-redirecting `401`/`403` behavior for API callers.
  - Generated the `AddIdentityFoundation` EF Core migration without adding roles, user-management tables, seed users, or any auth/session endpoints.
  - Added focused tests for the new account-status enum, Identity service resolution, and cookie-auth security behavior while preserving the existing health endpoint and architecture coverage.
  - Follow-up cleanup: moved EF Core migration artifacts and the model snapshot into `backend/src/Infrastructure/Persistence/Migrations/` so the `Persistence` area separates the DbContext from generated migration files more clearly.

- Unit 15 completed:
  - Added API-owned antiforgery configuration with a stable `X-CSRF-TOKEN` request header and explicit cookie behavior for the staff cookie-auth flow.
  - Added `GET /api/auth/csrf` to mint antiforgery tokens for browser clients and return the stable CSRF header name in a minimal safe response.
  - Added `GET /api/auth/session` with a stable `200 OK` unauthenticated payload and a minimal authenticated-session payload that excludes Identity internals, roles, team scopes, and permission data.
  - Added `POST /api/auth/logout` as an authenticated API endpoint that validates CSRF before clearing the auth cookie and returns no HTML redirects.
  - Upgraded cookie-auth challenge/forbid behavior from bare status codes to API-friendly ProblemDetails responses for `401` and `403`.
  - Added a reusable API helper pattern for future unsafe cookie-authenticated endpoints to validate CSRF without enabling global CSRF checks on safe endpoints.
  - Added integration coverage for the unauthenticated session path, CSRF bootstrap endpoint, logout unauthorized behavior, antiforgery option wiring, and the new auth ProblemDetails responses.

- Unit 16 completed:
  - Added a frontend `auth` feature area with session types, auth API helpers, a query-owned `useSession()` hook, and a logout mutation built on the existing shared API client.
  - Added public auth route shells for `sign-in`, `forgot-password`, and `reset-password`, keeping all visible copy in Bosnian Latin and clearly marking login/password-reset actions as unavailable until backend endpoints exist.
  - Split routing into public auth routes and protected app-shell routes so existing dashboard/module placeholders now require a valid backend session.
  - Added protected-route loading, unauthenticated redirect, and safe session-error handling without introducing fake users, local token storage, role logic, or permission logic.
  - Replaced the topbar placeholder with a session-aware user menu that shows the authenticated email when present and performs CSRF-protected logout.

- Unit 17 completed:
  - Added typed Infrastructure-owned `Bootstrap:FirstAdmin` configuration for first-admin bootstrap, with placeholder-only values in `backend/.env.example`.
  - Added startup bootstrap wiring that checks the persisted Identity user store first, exits without side effects when any user exists, and otherwise fails safely when bootstrap is disabled or incomplete.
  - Added first-user creation through `UserManager`, which applies the configured Identity normalization and password policy; the created account is `ACTIVE`, requires a password change, and receives clock-based audit timestamps.
  - Kept the flow backend-only: no public setup route, login/reset/invitation endpoints, frontend changes, roles, team scopes, permissions, or sample data were added.
  - Bootstrap configuration is skipped explicitly in the automated `Testing` environment so existing API integration tests remain isolated from persistence requirements.
  - Added focused tests for empty-store bootstrap configuration validation and safe validation messages. Full database-backed bootstrap behavior requires manual empty-database verification with a real configured PostgreSQL instance.
  - The temporary password is never committed, logged, returned, or displayed; it must be supplied by local or production environment configuration.

- Unit 18 completed:
  - Added Application-layer login, current-session, logout, and current-user password-change contracts, implemented by the Infrastructure Identity adapter without exposing Identity types outside Infrastructure.
  - Added `POST /api/auth/login` and `POST /api/auth/change-password`. Successful login and password change return the safe session contract: `isAuthenticated` and `user` with `id`, `email`, `accountStatus`, and `mustChangePassword`.
  - Extended `GET /api/auth/csrf` to return the request token alongside the stable `X-CSRF-TOKEN` header name, enabling validated unsafe requests.
  - Added centralized middleware that returns `403` ProblemDetails with code `password_change_required` for authenticated users whose `RequiresPasswordChange` flag is set, while allowing session, CSRF, change-password, logout, and anonymous health endpoints.
  - Login uses Identity normalization and lockout-on-failure. Invited, disabled, explicitly locked, and Identity-locked accounts cannot receive an auth cookie. Successful password changes use `UserManager.ChangePasswordAsync`, clear `RequiresPasswordChange` only after success, and refresh the sign-in cookie.
  - Added focused integration coverage using an isolated EF Core in-memory test host and a test-environment-only protected route; no production demo endpoint was added. The official first-admin role handoff remains Unit 20.

## In Progress

- Unit 23: Staff Users, Team Scope, and Account Lifecycle Backend is in progress. `TeamScopeType` contains `ALL_TEAMS` and `SELECTED_TEAMS`; the authoritative staff profile now stores display name and scope type, while `staff_team_scopes` uses a composite user/team key with restricted foreign keys and a team lookup index. Migration `20260710152754_AddStaffUsersTeamScopeLifecycle` was generated and applied locally.
  - Initial staff endpoints are mapped at `/api/users` (list, detail, invitation creation/reissue, profile/access update, disable, reactivate) and anonymous setup acceptance is mapped at `/api/auth/invitations/accept`. Staff responses expose only safe profile/access/scope/lifecycle fields; setup credentials are URL-safe and returned only from create/reissue.
  - The current session response now includes effective `teamScope`; administrators report `ALL_TEAMS` with no selected IDs. A reusable server-side team-access service fails closed for unavailable or malformed current access.
  - Invitation acceptance uses the Identity token provider, one-time security-stamp invalidation on reissue/acceptance, and does not sign users in. Disable invalidates the security stamp; reactivation preserves profile/scope and selects `ACTIVE` or `INVITED` from password presence. The final-active-admin safeguard is applied to demotion and disable.
  - Verification: isolated `dotnet build PlayerPerformance.sln --no-restore` passed with zero warnings/errors; isolated `dotnet test PlayerPerformance.sln --no-build --no-restore` passed (34 unit, 24 integration). The migration was applied to the configured local PostgreSQL development database. Focused Unit 23 integration and lifecycle/team-scope test coverage is still pending before this unit can be marked complete.

## Next Up

- Start the next scoped feature spec on top of the shared UI primitive baseline.
- Build the first real frontend feature or backend login flow on top of the new auth/session foundation.
- Build the next backend foundation unit on top of the new configuration-ready, testable persistence baseline.
- Build the next backend module on top of the new shared primitives and persistence baseline.
- Build the next auth-focused backend unit on top of the new Identity persistence, CSRF/session foundation, auth utility endpoints, and frontend auth shell.

## Open Questions

- Obtain real Gpexe CSV/XLSX export samples and document confirmed import fields.
- Obtain real Zone14 export samples, if available, and document confirmed data fields.
- Confirm whether Zone14 provides any structured event data or only video/tagging/running-stat support.
- Select the production hosting provider later.
- Select the production object storage provider later.
- Decide when Docker and Docker Compose should be introduced.
- Confirm final media/video storage constraints after production hosting direction is known.

## Architecture Decisions

- The application is a single-club system for FK Velež Mostar; multi-club tenancy is out of scope for V1.
- V1 focuses primarily on match performance analysis, with training support limited mainly to GPS/physical workload tracking.
- Players are persistent club entities and move through teams using time-bound assignment history.
- Teams/selections are configurable and use tracking levels: basic, standard, and full.
- The access model uses one primary role per user, team scopes, and limited explicit permission flags.
- Analysts can review match reports by default and can verify reports only when granted verification permission.
- Backend uses ASP.NET Core 8, PostgreSQL, Clean Architecture, Vertical Slice organization in the Application layer, and Minimal APIs endpoint groups.
- Backend Identity persistence uses a user-only ASP.NET Core Identity setup on the shared `AppDbContext`, with staff-only table naming and no role tables in the foundation unit.
- Frontend uses React, Vite, TypeScript, Tailwind CSS v4, shadcn/ui, TanStack Query, nuqs, Zustand for limited global UI state, Zod, React Hook Form, TanStack Table, and shadcn/Recharts charts.
- The UI is light-only and follows FK Velež red, white, and gold identity.
- Bosnian Latin is the default UI language; English may be supported as an optional selectable UI language.
- Local development uses `backend/.env` and `frontend/.env.local`; production uses real environment variables or a managed secret store.
- Docker is out of scope for the initial local development setup and may be introduced later.
- Exact Gpexe and Zone14 import mappings must not be guessed before real export samples are reviewed.
- Backend cookie authentication uses the Identity application scheme, `HttpOnly` cookies, `SameSite=Lax`, production `SecurePolicy=Always`, development `SecurePolicy=SameAsRequest`, and API-friendly non-redirecting unauthorized/forbidden responses.
- Backend auth utility endpoints use `GET /api/auth/csrf`, `GET /api/auth/session`, and `POST /api/auth/logout` as the stable initial cookie-auth API surface.
- First-admin bootstrap is a single-instance startup operation that only creates an account when the persisted user store is empty. A multi-instance deployment will need a distributed lock or equivalent database-safe coordination if concurrent initial startup becomes a supported deployment mode.
- Unit 20 will assign or migrate the bootstrap-created account into the official `ADMIN` role when the staff role and team-scope model is introduced.
- The frontend-facing CSRF request header name is `X-CSRF-TOKEN`; the browser-readable CSRF token cookie currently uses `XSRF-TOKEN`.
- Frontend auth routing now treats `/sign-in`, `/forgot-password`, and `/reset-password` as public routes, while the existing app-shell routes require a successful session query before rendering.
- Frontend session state is owned by TanStack Query through the auth feature hook and is not duplicated in Zustand or browser storage.

## Session Notes

- Unit 01 verification results:
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
  - `backend`: `dotnet build PlayerPerformanceDataSystem.sln` passed after allowing NuGet network access for restore.
- No backend tests exist yet, so no `dotnet test` command was run.
- The frontend `@/` alias was already configured correctly in both TypeScript and Vite, so no alias changes were required.
- The documented target architecture remains unchanged; Unit 01 only cleaned the current starter baseline.
- Follow-up correction applied after Unit 01:
  - Updated the frontend placeholder copy to use proper Bosnian Latin characters.
  - Added an explicit context rule requiring proper Bosnian Latin characters like `č`, `ć`, `š`, `ž`, and `đ` in user-facing Bosnian copy.
- Unit 02 verification results:
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
  - No backend files were changed for Unit 02.
- Unit 03 verification results:
  - `frontend`: `npm.cmd install react-router-dom` passed after allowing network access for dependency installation.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
  - No backend files were changed for Unit 03.
- Unit 04 verification results:
  - `frontend`: `npm.cmd install -D prettier prettier-plugin-tailwindcss eslint-config-prettier` passed after allowing network access for dependency installation.
  - `frontend`: `npm.cmd run format` passed.
  - `frontend`: `npm.cmd run format:check` passed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
  - `backend`: initial `dotnet build PlayerPerformanceDataSystem.sln` attempt was blocked by NuGet network access in the sandbox.
  - `backend`: `dotnet build PlayerPerformanceDataSystem.sln` passed after allowing network access for NuGet restore.
- Unit 05 verification results:
  - `frontend`: `npm.cmd run format` passed.
  - `frontend`: `npm.cmd run format:check` passed after rerunning sequentially because an earlier parallel run raced the formatter.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
  - `backend`: no backend files were changed, so no backend build was required for Unit 05.
  - `frontend`: the formatter also updated pre-existing frontend files outside the new primitives so the repository now satisfies the configured Prettier checks.
- Unit 06 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln` passed.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --no-build` completed successfully with no test projects present in the solution.
  - `backend`: `dotnet run --project src/Api/PlayerPerformance.Api.csproj --no-build --urls http://127.0.0.1:5099` served `GET /health` successfully and returned the expected safe JSON response.
  - `frontend`: no frontend files were changed for Unit 06, so no frontend verification commands were required.
- Unit 07 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln` passed after allowing NuGet network access for the new `DotNetEnv` package restore.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --no-build` completed successfully with no test projects present in the solution.
  - `backend`: `dotnet run --project src/Api/PlayerPerformance.Api.csproj --no-build --urls http://127.0.0.1:5099` returned a healthy response when `PlayerPerformance__ServiceName` and `PlayerPerformance__FrontendOrigin` were provided through environment variables.
  - `backend`: a temporary ignored `backend/.env` file was used to verify local `.env` loading, and `GET /health` returned the service name loaded from that file.
  - `backend`: startup validation failed as expected when `PlayerPerformance__ServiceName` was missing, with an `OptionsValidationException` stating that `PlayerPerformance:ServiceName` is required.
  - `frontend`: no frontend files were changed for Unit 07, so no frontend verification commands were required.
- Unit 08 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln` partially failed in the sandbox because the API project could not reach NuGet repository-signature metadata at `api.nuget.org`; no new package was added in this unit.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --no-build` completed successfully with no test projects present in the solution.
  - `backend`: `dotnet run --project src/Api/PlayerPerformance.Api.csproj --no-build --urls http://127.0.0.1:5099` returned the expected safe `GET /health` response when `PlayerPerformance__ServiceName` was provided through environment variables.
  - `backend`: requesting `http://127.0.0.1:5099/missing-route` returned a `404` ProblemDetails response with a safe `traceId` extension.
  - `backend`: sending `POST http://127.0.0.1:5099/health` returned a `405` ProblemDetails response with a safe `traceId` extension.
  - `frontend`: no frontend files were changed for Unit 08, so no frontend verification commands were required.
- Unit 09 verification results:
  - `backend`: `dotnet restore` passed after allowing NuGet network access for the new test packages.
  - `backend`: a lingering `PlayerPerformance.Api` process from earlier verification was stopped so the API project could rebuild without locked output files.
  - `backend`: `dotnet build --no-restore` passed.
  - `backend`: `dotnet test --no-build --no-restore` passed.
  - `backend`: unit tests now verify the documented Domain/Application/Infrastructure/API reference direction.
  - `backend`: integration tests now verify `GET /health` returns a successful safe JSON response and that an unknown route returns a safe `404` ProblemDetails response with a `traceId`.
  - `backend`: an initial `dotnet test --no-build` attempt started too early while a parallel build was still producing outputs, so it was rerun sequentially.
  - `frontend`: no frontend files were changed for Unit 09, so no frontend verification commands were required.
- Unit 10 verification results:
  - `frontend`: `npm.cmd install @tanstack/react-query` passed after allowing network access for dependency installation.
  - `frontend`: `npm.cmd run format` passed after an initial file-write race/permission issue was resolved by rerunning formatting sequentially.
  - `frontend`: `npm.cmd run format:check` passed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
  - `frontend`: TypeScript incremental cache output was moved from `frontend/node_modules/.tmp` to `frontend/.tmp` because the current environment denied recreating build-info files under `node_modules`.
  - `frontend`: Vite scripts were switched to `--configLoader runner`, `vite.config.ts` was made ESM-safe, and an old generated `frontend/dist` folder was removed so the build could complete cleanly in the current environment.
  - `backend`: no backend files were changed for Unit 10, so no backend build or test command was required.
- Unit 11 verification results:
  - `frontend`: `npm.cmd install react-hook-form zod @hookform/resolvers` passed after allowing network access for dependency installation.
  - `frontend`: `npx.cmd shadcn@latest add form` completed the registry check, but did not materialize a new primitive file in this repository, so the unit was completed with shared generic helpers and components only.
  - `frontend`: `npm.cmd run format` passed.
  - `frontend`: an initial parallel `npm.cmd run format:check` run failed because it raced the formatter and reported temporary style drift in already-updating files.
  - `frontend`: `npm.cmd run format:check` passed when rerun sequentially after formatting completed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
  - `backend`: no backend files were changed for Unit 11, so no backend build or test command was required.
- Unit 12 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts` passed after allowing network access for the new EF Core/PostgreSQL packages.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts` passed.
  - `backend`: integration tests now verify `/health` still returns the safe service response and `/health/ready` returns a safe unhealthy response when the configured database is unreachable.
  - `backend`: the repository's default backend `obj/bin` paths were locally locked in this environment, so verification used the .NET SDK `--artifacts-path` option to keep restore/build/test isolated from those locked outputs.
  - `backend`: the baseline migration files were added manually in EF Core format because the local EF CLI metadata path was blocked by the same locked default build-output issue; the migration files compile successfully in the verified solution build.
  - `backend`: follow-up local verification succeeded after stopping the running API process, rebuilding normally, and running `dotnet ef database update --project src/Infrastructure/PlayerPerformance.Infrastructure.csproj --startup-project src/Api/PlayerPerformance.Api.csproj`.
  - `backend`: the EF update created the `player_performance` database, created `__EFMigrationsHistory`, and applied the `20260710120000_InitialPersistenceBaseline` migration.
- Unit 13 verification results:
  - `backend`: creating the new shared source/test folders required an escalated directory-creation step because the sandbox denied creating nested directories inside the workspace.
  - `backend`: default restore/build paths under the repository `obj/bin` folders remained locked in this environment, so verification again used `--artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit13`.
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit13` passed after allowing temporary NuGet network access.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit13` passed.
  - `backend`: an initial parallel `dotnet test --no-build` run failed because it raced the build outputs in the shared artifacts directory, so the test step was rerun sequentially.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit13` passed with 18 unit tests and 3 integration tests.
  - `frontend`: no frontend files were changed for Unit 13, so no frontend verification commands were required.
- Unit 14 verification results:
  - `backend`: creating the new Domain/Infrastructure/test folders for the Identity foundation required an escalated directory-creation step because the sandbox denied nested folder creation inside the workspace.
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit14` passed after allowing temporary NuGet network access for the added Identity EF Core package.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit14` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit14` passed with 19 unit tests and 6 integration tests.
  - `backend`: `dotnet ef migrations add AddIdentityFoundation --project src/Infrastructure/PlayerPerformance.Infrastructure.csproj --startup-project src/Api/PlayerPerformance.Api.csproj --context PlayerPerformance.Infrastructure.Persistence.AppDbContext --output-dir Persistence` succeeded after allowing normal workspace build-output access for EF tooling.
  - `backend`: `dotnet ef database update --project src/Infrastructure/PlayerPerformance.Infrastructure.csproj --startup-project src/Api/PlayerPerformance.Api.csproj` succeeded after allowing normal workspace build-output access for EF tooling and applied the `20260709232300_AddIdentityFoundation` migration to the local database.
  - `backend`: an intermediate generated migration was removed and recreated so the final schema also enforces unique normalized email at the database level.
  - `backend`: the local EF tools reported that version `8.0.0` is older than the runtime `8.0.10`, but migration generation still completed successfully and the warning did not block verification.
  - `backend`: default repository `obj` paths remain sensitive in this environment, so normal solution verification continued to use `--artifacts-path` even though `dotnet ef` needed temporary unrestricted workspace output access.
  - `backend`: follow-up cleanup created `backend/src/Infrastructure/Persistence/Migrations/` and moved the generated migration files plus `AppDbContextModelSnapshot` there; solution verification still passed afterward.
  - `frontend`: no frontend files were changed for Unit 14, so no frontend verification commands were required.
- Unit 15 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit15` initially failed in the sandbox because NuGet network access was blocked, then passed after allowing temporary network access.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit15` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit15` passed with 19 unit tests and 10 integration tests.
  - `backend`: antiforgery token generation in the integration host required explicit ephemeral data-protection registration in `TestApplicationFactory` so CSRF issuance tests remain stable without changing runtime behavior.
  - `backend`: the test environment now uses `CookieSecurePolicy.SameAsRequest` for auth and antiforgery cookies so the integration host can exercise cookie behavior safely while production remains `SecurePolicy=Always`.
  - `backend`: authenticated logout with a missing or invalid CSRF token is implemented through the shared validation helper, but full end-to-end integration coverage for that path is still deferred because the repository does not yet have a test-auth sign-in helper that can establish a real cookie-authenticated session.
  - `frontend`: no frontend files were changed for Unit 15, so no frontend verification commands were required.
- Unit 16 verification results:
  - `frontend`: `npm.cmd run format` passed.
  - `frontend`: `npm.cmd run format:check` passed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
  - `frontend`: protected routing now depends on the backend session endpoint and redirects unauthenticated requests toward `/sign-in` without introducing fake auth state or browser token storage.
  - `frontend`: the sign-in, forgot-password, and reset-password pages are intentionally honest shells only; they do not submit credentials or call non-existent backend endpoints.
  - `frontend`: browser-level manual verification of a successful authenticated session and logout round-trip is still limited because the repository does not yet expose a real login/first-admin bootstrap flow for creating a staff session interactively.
- Local CORS bugfix verification results:
  - `backend`: the browser session error was traced to missing API CORS middleware even though `GET http://localhost:5051/api/auth/session` returned a healthy unauthenticated payload directly.
  - `backend`: added a configured-frontend CORS policy that allows the documented frontend origin and credentials for the cookie-auth API surface.
  - `backend`: added an integration test that verifies a preflight request for `/api/auth/session` returns `204 No Content` with `Access-Control-Allow-Origin` and `Access-Control-Allow-Credentials`.
  - `backend`: `dotnet build PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-cors-fix` passed after allowing temporary NuGet network access.
  - `backend`: `dotnet test PlayerPerformance.sln --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-cors-fix` passed with 19 unit tests and 11 integration tests.
- Local auth-page scrollbar bugfix verification results:
  - `frontend`: the public auth routes were producing unnecessary page scroll because the auth shell combined viewport-height sizing with vertical padding.
  - `frontend`: fixed the auth shell sizing so the public auth layout no longer adds height beyond the viewport.
  - `frontend`: `npm.cmd run format` passed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
- Local auth sign-in layout bugfix verification results:
  - `frontend`: the sign-in shell originally rendered the email and password placeholders side by side, which was not an appropriate default layout for this auth page.
  - `frontend`: updated the sign-in shell to use a standard vertical field stack.
  - `frontend`: corrected the progress note for the earlier scrollbar fix so it no longer attributes the issue to a body-margin change that was not part of the final fix.
  - `frontend`: `npm.cmd run format` passed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
  - `frontend`: `npm.cmd run format` passed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
- Local auth shared-shell width refinement verification results:
  - `frontend`: finalized the shared auth shell at `max-w-xl`, keeping the header and auth card at a focused single-column width without a sign-in-specific override.
  - `frontend`: retained the standard vertical sign-in field stack.
  - `frontend`: `npm.cmd run format:check` passed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.

- Unit 17 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit17` initially failed because sandbox network access to NuGet was blocked, then passed after temporary NuGet network access was allowed.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit17` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit17` passed with 25 unit tests and 11 integration tests.
  - Manual follow-up: start the API against an empty local PostgreSQL database with `Bootstrap__FirstAdmin__Enabled=true`, a real email, and a password that meets the configured Identity requirements; confirm exactly one active user is created with `RequiresPasswordChange=true`, then restart with changed bootstrap values and confirm the user is untouched.

- Unit 18 verification results:
  - `backend`: added `Microsoft.EntityFrameworkCore.InMemory` to the integration-test project only so cookie/Identity flows can be verified without developer-specific PostgreSQL credentials or Testcontainers.
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit18` passed after temporary NuGet network access was allowed.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit18` passed with zero warnings and zero errors.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit18` passed with 25 unit tests and 17 integration tests.

- Unit 19 verification results:
  - `frontend`: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, and `npm.cmd run build` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit19` passed with 25 unit tests and 17 integration tests.
  - Manual browser end-to-end verification was not run in this environment because it requires a configured local API, PostgreSQL database, bootstrap admin credentials, and cookie-capable browser session. The backend integration suite verifies the login, CSRF, required-password-change, password-change, session, and logout contracts used by the frontend.

- Unit 20 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit20` passed after temporary NuGet network access was allowed.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit20` passed with zero warnings and zero errors.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit20` passed with 25 unit tests and 22 integration tests.
  - `backend`: generated `20260710124807_AddStaffAccessProfiles` and `20260710125149_AddStaffAccessProfileUserForeignKey` with EF Core tooling and applied them successfully using `dotnet ef database update`; the local development database now contains `staff_access_profiles` with its Identity-user foreign key.
  - `backend`: test-host-only policy probes verify unauthenticated `401`, missing-profile/non-admin `403`, matching permission access, all `ADMIN` overrides, safe session enrichment, and first-admin handoff idempotency/ambiguity behavior. No production demonstration endpoint was added.

- Unit 21 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln` passed after NuGet access was allowed; `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit21` passed with zero warnings and errors.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit21` passed with 30 unit tests and 22 integration tests.
  - `backend`: generated and applied `20260710131639_AddSettingsSeasonsAndCompetitions` with EF Core tooling against the configured local PostgreSQL database. The migration was generated after a stale no-build attempt; the local development migration history also contains the harmless empty `20260710131358_AddSeasonsAndCompetitions` record from that attempt, while the source-controlled migration set contains only the intended Unit 21 migration.

## Confirmed Decisions

- Unit 25 preserves venue and opponent names across archive states; archived names remain reserved by their respective unique normalized-name indexes. Venue and opponent namespaces are independent, so the same display name is valid once in each table.

## Known Blockers

- Unit 25: `dotnet ef migrations add AddVenuesAndOpponents` could not run because the developer's running `PlayerPerformance.Api` process (PID 2564) locks the normal API build outputs. The focused migration was prepared from the verified EF model instead. The isolated restore, whitespace formatting/check, zero-warning build, and full test suite pass; generating/applying the migration with EF tooling remains the final local verification step once that process is stopped.

## Unit 24: Staff Users and Roles UI

- Status: complete.
- Added protected `/users` and public `/accept-invitation` routes. The users page is gated by resolved `ADMIN` session role and the sidebar hides the navigation item until an administrator session is known.
- Staff filters are URL-backed with `nuqs`: `q`, `role`, `status`, `scope`, and `team`.
- Added real staff/team API wrappers, TanStack Query mutations, role/scope/permission mappings, invitation/reissue one-time setup links built from `window.location.origin`, and transient invitation credential state. Setup query values are captured in component memory and removed from the public invitation URL.
- Self-disable clears session and returns to sign-in; self-access replacement invalidates the session before route re-evaluation.
- Added `@tanstack/react-table`, `nuqs`, and generated shadcn primitives: table, badge, select, checkbox, dropdown-menu, input, and label. The dialog generator could not add dialog/alert-dialog without overwriting the existing generated Button, so accessible feature-level modal semantics are used instead.
- Verification: frontend formatting, build, format check passed. Lint passed except its known TanStack Table React Compiler compatibility warning; manual end-to-end verification requires local API, PostgreSQL, and a cookie-capable browser session.
- Deferred: email delivery, user deletion, login-email recovery, manual unlock, audit UI, localization infrastructure, and unrelated modules.

- Backend formatting baseline: the root `.editorconfig` defines four-space C# indentation, multiline braces/statements, and whitespace conventions. Backend implementation work must run `dotnet format PlayerPerformance.sln whitespace --no-restore` followed by its `--verify-no-changes` check before final build/test verification. A repository-wide backend whitespace formatting pass was completed; it introduced no behavioral changes.
- Future EF Core migration creation and database update work should use `dotnet ef` tooling by default instead of handwritten migration files whenever the local environment supports the CLI workflow.
- Future EF Core migrations should be generated into `backend/src/Infrastructure/Persistence/Migrations/` using `--output-dir Persistence/Migrations` so the `AppDbContext` remains separated from generated migration artifacts.
- When a backend task requires a new EF Core migration, the preferred verification flow is to run both `dotnet ef migrations add ... --output-dir Persistence/Migrations` and `dotnet ef database update` unless the environment prevents it or the task explicitly says otherwise.
