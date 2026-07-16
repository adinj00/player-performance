# Build Plan: Player Performance Data System

This file defines the planned implementation order for the Player Performance Data System.
It is a planning artifact, not an implementation spec. Each numbered unit should receive its own scoped feature spec before implementation begins.

## Planning Rules

- Implement one unit at a time.
- Keep feature specs small, verifiable, and dependency-aware.
- Do not implement product behavior before its foundations exist.
- Do not introduce speculative integrations, packages, data fields, or vendor mappings.
- Keep backend authorization as the source of truth.
- Keep UI light-only and Bosnian Latin by default.
- Use semantic design tokens and shadcn/ui primitives without ad-hoc visual overrides.
- Use `format:check` for all frontend and full-stack units after Unit 04.
- Keep Gpexe and Zone14 field-specific imports blocked until real export samples are reviewed.

## Current Completed / Started Foundation Units

### Unit 01: Project Boilerplate Cleanup

Builds a clean baseline by removing default starter/demo code while preserving project tooling, shadcn/ui setup, theme tokens, aliases, and minimal Bosnian Latin placeholder copy.

Dependencies: none.

### Unit 02: Frontend App Shell Baseline

Builds the first static frontend application shell with sidebar, topbar, main content area, responsive behavior, and minimal placeholder UI.

Dependencies: Unit 01.

### Unit 03: Frontend Routing Foundation

Adds frontend routing, placeholder pages, active navigation state, topbar title behavior, and not-found handling without real domain features or data fetching.

Dependencies: Unit 02.

### Unit 04: Code Formatting Foundation

Adds Prettier, Tailwind class sorting, ESLint compatibility, formatting scripts, ignore rules, and formatting verification.

Dependencies: Unit 03.

### Unit 05: Common UI Primitives Foundation

Adds reusable app-level UI primitives such as page headers, empty/loading/error states, and content sections, then refactors placeholders to use them.

Dependencies: Unit 04.

### Unit 06: Backend Solution Baseline

Adds the ASP.NET Core solution, Clean Architecture projects, dependency direction, basic API host, DI extension points, and a minimal health endpoint.

Dependencies: Unit 05.

### Unit 07: Backend Configuration Baseline

Adds backend environment configuration, `.env.example`, local env loading, typed options, startup validation, and safe configuration behavior.

Dependencies: Unit 06.

### Unit 08: Backend API Error Handling Foundation

Adds ProblemDetails, global exception handling, safe trace IDs, API endpoint mapping structure, and consistent backend error foundations.

Dependencies: Unit 07.

## Next Foundation Units

### Unit 09: Backend Testing Foundation

Add backend unit/integration test project structure, shared test conventions, first health/error-handling tests where practical, and `dotnet test` verification.

Dependencies: Unit 08.

### Unit 10: Frontend API Client and Environment Foundation

Add frontend API base URL configuration, typed fetch wrapper, safe error handling, and TanStack Query provider foundation without calling real domain endpoints.

Dependencies: Unit 08.

### Unit 11: Frontend Form and Validation Foundation

Add React Hook Form + Zod conventions, reusable form error display patterns, and one non-domain sample/internal demonstration only if needed for verification.

Dependencies: Unit 05, Unit 10.

### Unit 12: Backend Persistence Foundation

Add EF Core, Npgsql/PostgreSQL provider configuration, Infrastructure DbContext baseline, migration structure, and database health checks without full domain modules.

Dependencies: Unit 07, Unit 08, Unit 09.

### Unit 13: Backend Domain Shared Primitives

Add shared domain primitives such as entity base patterns, audit metadata contracts where appropriate, result/error abstractions if needed, and system clock abstractions.

Dependencies: Unit 06, Unit 09.

## Authentication and Staff Access

### Unit 14: Backend Auth and Identity Foundation

Add ASP.NET Core Identity-style persistence, user account statuses, secure cookie authentication baseline, password hashing/session behavior, and no public registration.

Dependencies: Unit 12, Unit 13.

### Unit 15: Backend CSRF and Session API

Add CSRF protection for unsafe cookie-authenticated requests, session endpoint, logout endpoint, and consistent unauthorized/forbidden behavior.

Dependencies: Unit 14.

### Unit 16: Frontend Auth Shell and Session Provider

Add sign-in, forgot/reset password surfaces where supported by backend, session provider, protected route handling, user menu placeholder behavior, and Bosnian Latin copy.

Dependencies: Unit 10, Unit 15.

### Unit 17: First Admin Bootstrap

Add environment-based first admin creation when no admin exists, temporary password handling, required password change marker, and startup safeguards.

Dependencies: Unit 14, Unit 15.

### Unit 18: Backend Login and Required Password Change

Add secure cookie-based login, first-login password-change enforcement, password change endpoint, safe authentication failures, and backend tests.

Dependencies: Unit 14, Unit 15, Unit 17.

### Unit 19: Frontend Sign-In and Password Change Wiring

Wire the existing auth shell to the backend login/session/password-change APIs, enforce required password change in protected navigation, and provide complete Bosnian Latin authentication states.

Dependencies: Unit 16, Unit 18.

### Unit 20: Backend Staff Roles and Authorization Foundation

Add the canonical primary-role model, explicit permission flags, first-admin role handoff, current-user access context, and reusable backend authorization policies. Team-specific scopes and staff CRUD remain deferred until teams/selections exist.

Dependencies: Unit 14, Unit 17, Unit 18.

## Club Configuration and Staff Scope Foundations

### Unit 21: Seasons and Competitions Backend

Add settings backend for seasons and competitions with CRUD, validation, archive behavior, and admin authorization checks.

Dependencies: Unit 20.

### Unit 22: Teams / Selections Backend

Add configurable selections, tracking levels, default seed behavior where appropriate, active/archive state, ordering, and backend permission checks.

Dependencies: Unit 20, Unit 21.

### Unit 23: Staff Users, Team Scope, and Account Lifecycle Backend

Add admin-managed staff listing and invitation/setup flow, role updates, all-team or selected-team scope assignments, explicit permission updates, disable/reactivate actions, and team-scope authorization services.

Dependencies: Unit 18, Unit 20, Unit 22.

### Unit 24: Staff Users and Roles UI

Add admin-facing staff list, invite/create flow, invitation setup surface, role/scope assignment UI, disable/reactivate actions, and permission-aware interface behavior.

Dependencies: Unit 19, Unit 23.

### Unit 25: Venues and Opponents Backend

Add venues and opponents settings with CRUD, validation, archive behavior, and authorization checks.

Dependencies: Unit 21.

### Unit 26: Settings UI Foundation

Add settings navigation and UI screens for seasons, competitions, selections, tracking levels, venues, and opponents.

Dependencies: Unit 19, Unit 21, Unit 22, Unit 25.

## Player Foundations

### Unit 27: Players Backend Foundation

Add player records, profile metadata, archive/status behavior, and basic CRUD with backend authorization.

Dependencies: Unit 20, Unit 22, Unit 23.

### Unit 28: Player Team Assignment Backend

Add persistent player assignment history, time-bound selection movement, multiple active assignments support where allowed, and invariant tests.

Dependencies: Unit 22, Unit 27.

### Unit 29: Players UI Foundation

Add players list, filters, create/edit dialogs, archive behavior, and player detail shell with assignment history display.

Dependencies: Unit 19, Unit 27, Unit 28.

## Match and Report Foundations

### Unit 30: Matches Backend Foundation

Add match metadata, season/competition/opponent/venue/team selection references, match status basics, and authorized CRUD.

Dependencies: Unit 21, Unit 22, Unit 23, Unit 25, Unit 27.

### Unit 31: Match Lineup and Appearance Backend

Add lineup, starters, substitutes, captain, substitutions, player minutes, and concrete match appearances linked to players.

Dependencies: Unit 28, Unit 30.

### Unit 32: Match Report Workflow Backend

Add match report statuses, allowed transitions, allowed actions, verification permission behavior, correction request behavior, and audit hooks where available.

Dependencies: Unit 20, Unit 23, Unit 30, Unit 31.

### Unit 33: Manual Match Statistics Backend

Add player match statistics and goalkeeper statistics for manual entry according to tracking level rules, with validation and report status rules.

Dependencies: Unit 22, Unit 31, Unit 32.

### Unit 34: Matches UI Foundation

Add matches list, filters, status tabs, match creation/edit dialogs, and match detail shell.

Dependencies: Unit 19, Unit 26, Unit 30.

### Unit 35: Lineup and Appearances UI

Add the authoritative match-date eligible-player query, then build the match lineup editor, appearances/minutes UI, ordered substitution handling, workflow-lock behavior, and validation feedback using the atomic Unit 31 lineup contract.

Dependencies: Unit 31, Unit 32, Unit 34.

### Unit 36: Manual Match Statistics UI

Add tracking-level-driven player and goalkeeper statistics read/edit UI optimized for desktop/tablet, minimal draft report initialization, completeness states, atomic saves, and validation/workflow feedback.

Dependencies: Unit 32, Unit 33, Unit 35.

### Unit 37: Match Report Review UI

Add a team-scope-aware report workflow queue and the real match `Revizija` tab with backend-provided allowed actions, readiness display, submit for review, request correction, verify, and terminal report archive behavior. Do not add restore because Unit 32 defines `ARCHIVED` as terminal in the current V1 workflow.

Dependencies: Unit 32, Unit 33, Unit 35, Unit 36.

## Audit and Data Integrity

### Unit 38: Audit Backend Foundation

Add append-only audit persistence, an Application-layer audit writer, safe semantic before/after change sets, atomic audit coverage for implemented staff-access/account, match-report workflow, and manual statistics mutations, plus authorized entity-history APIs for the Unit 39 UI.

Dependencies: Unit 20, Unit 23, Unit 32, Unit 33.

### Unit 39: Audit UI Foundation

Add entity-specific, authorized audit history UI: a paginated `Historija promjena` view inside match report review and an admin-only staff-user audit Sheet, with semantic before/after rendering, action/date filters, safe unknown-value handling, and no global audit browser.

Dependencies: Unit 24, Unit 37, Unit 38.

## Media and File Storage

### Unit 40: File Storage Abstraction

Add provider-neutral Application file-storage contracts, opaque storage keys, streaming local-development storage, persistent `StoredFile` metadata, safe size/path/configuration rules, and compensation support without public file endpoints or production provider lock-in.

Dependencies: Unit 12, Unit 13, Unit 20.

### Unit 41: Media Backend Foundation

Add a unified team-scoped media catalog with uploaded `MediaAsset` and `ExternalMediaReference` sources, streamed storage-backed uploads/content, explicit relational links to matches, match reports, and players, archive/restore behavior, workflow-aware authorization, and semantic audit coverage.

Dependencies: Unit 23, Unit 28, Unit 32, Unit 38, Unit 40.

### Unit 42: Media UI Foundation

Add the team-scoped `Medijateka`, runtime upload capabilities, streamed upload progress, uploaded/external media detail and lifecycle UI, authoritative link-candidate search, and explicit match/report/player attachment surfaces, including real match and player media sections.

Dependencies: Unit 29, Unit 34, Unit 37, Unit 41.

## Imports and GPS Data

### Unit 43: Import Workflow Backend Foundation

Add persistent team-scoped import jobs, streamed CSV/XLSX source retention, backend-owned statuses and allowed actions, preview/validation result persistence, processor registries, safe processing leases, explicit confirmation contracts, cancellation, source download, and semantic audit coverage without adding parser packages or vendor mappings.

Dependencies: Unit 23, Unit 30, Unit 38, Unit 40.

### Unit 44: Import UI Foundation

Add the protected `Importi` workspace with capability-driven CSV/XLSX upload, URL-filtered import history, detail workflow, source download, backend preview and validation views, explicit confirmation, cancellation, processing-state refresh, and import audit history. Unsupported processor combinations must be shown truthfully rather than through placeholder actions.

Dependencies: Unit 39, Unit 43.

### Unit 45: Generic CSV/XLSX Parsing Foundation

Add mature CSV/XLSX reader packages, format detection, row parsing abstraction, column preview, validation result structure, and tests without vendor-specific mappings.

Dependencies: Unit 43.

### Unit 46: Gpexe Mapping Spec After Sample Review

Implement confirmed Gpexe import mappings only after real export samples are reviewed and documented.

Dependencies: Unit 45. Blocked until samples are available.

### Unit 47: Zone14 Mapping Spec After Sample Review

Implement confirmed Zone14 data handling only after real exports or documentation prove available fields. Do not assume full event data.

Dependencies: Unit 45. Blocked until samples are available.

## Training, Medical, and Dashboard

### Unit 48: Training GPS Backend Foundation

Add training session metadata and GPS/physical workload models using confirmed generic metric structure where practical.

Dependencies: Unit 22, Unit 27, Unit 43.

### Unit 49: Training GPS UI Foundation

Add training session list/detail UI and GPS workload display where data exists.

Dependencies: Unit 48.

### Unit 50: Medical Availability Backend

Add availability statuses, injury/availability records, restricted notes, medical permissions, and audit coverage.

Dependencies: Unit 20, Unit 23, Unit 27, Unit 38.

### Unit 51: Medical Availability UI

Add team-filtered availability views, player availability editing for authorized roles, restricted note behavior, and coach-safe summaries.

Dependencies: Unit 50.

### Unit 52: Dashboard Backend Read Models

Add dashboard read endpoints for selected season/team: recent matches, report statuses, availability summary, top performers, workload summary, and data quality alerts where data exists.

Dependencies: Unit 32, Unit 33, Unit 48, Unit 50.

### Unit 53: Dashboard UI

Add dashboard page with filters, KPI cards, summaries, charts, empty states, and token-based visualization.

Dependencies: Unit 52.

## Localization and Polish

### Unit 54: Localization Foundation

Add i18next/react-i18next or approved localization stack, Bosnian Latin default resources, optional English infrastructure, and translation conventions.

Dependencies: Unit 19, Unit 26.

### Unit 55: Localization Pass for Existing UI

Move long-lived visible UI strings into translation resources and verify Bosnian Latin default behavior.

Dependencies: Unit 54.

### Unit 56: Responsive and Accessibility Hardening

Audit key pages for keyboard navigation, focus states, mobile/tablet rendering, semantic tables, status labels, and accessible actions.

Dependencies: Unit 37, Unit 44, Unit 51, Unit 53.

### Unit 57: Production Configuration Readiness

Add production configuration validation, deployment notes, object storage provider decision points, and environment documentation. Do not choose hosting unless confirmed.

Dependencies: Unit 40, Unit 53.

## Next Immediate Unit

The next implementation unit after Unit 43 should be:

**Unit 44: Import UI Foundation**

Reason: the backend now owns secure source-file retention, import workflow statuses, allowed actions, processor capabilities, preview/validation persistence, explicit confirmation safety, cancellation, and audit history. The next dependency-safe step is to build the real `Importi` workspace so authorized users can upload and inspect jobs immediately, while unsupported processing combinations remain truthful until Unit 45 registers generic CSV/XLSX readers.
