# Progress Tracker

Update this file after every meaningful implementation change.

This file intentionally starts lightweight. It should become more detailed as build units are added under `/feature-specs` and implementation work begins.

## Current Phase

- Feature implementation kickoff

## Current Goal

- Unit 11 completed; the frontend now has a shared form and validation foundation for future data-entry workflows without introducing product-specific forms.

## Completed

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

## In Progress

- No active implementation unit.

## Next Up

- Start the next scoped feature spec on top of the shared UI primitive baseline.
- Build the first real frontend feature or auth/session foundation on top of the new API client, TanStack Query provider, and shared form conventions.
- Build the next backend foundation unit on top of the new configuration-ready, testable Clean Architecture baseline.

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
- Frontend uses React, Vite, TypeScript, Tailwind CSS v4, shadcn/ui, TanStack Query, nuqs, Zustand for limited global UI state, Zod, React Hook Form, TanStack Table, and shadcn/Recharts charts.
- The UI is light-only and follows FK Velež red, white, and gold identity.
- Bosnian Latin is the default UI language; English may be supported as an optional selectable UI language.
- Local development uses `backend/.env` and `frontend/.env.local`; production uses real environment variables or a managed secret store.
- Docker is out of scope for the initial local development setup and may be introduced later.
- Exact Gpexe and Zone14 import mappings must not be guessed before real export samples are reviewed.

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
