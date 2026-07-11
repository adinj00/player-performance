# Architecture Context

## Stack

| Layer | Technology | Role |
| --- | --- | --- |
| Frontend Framework | React + Vite + TypeScript | Single-page web application for staff workflows, dashboards, forms, tables, and analysis views |
| Frontend Styling | Tailwind CSS v4 + shadcn/ui | Token-based UI system, reusable primitives, dashboard layout, forms, tables, dialogs, and charts |
| Frontend Data Fetching | TanStack Query | Server-state fetching, caching, invalidation, and mutations |
| Frontend URL State | React Router search params and local React state | URL state where a workflow requires shareable navigation; local component state for current Players/Users filters to keep typing and filter changes independent of router updates |
| Frontend Client UI State | Zustand | Limited global client-side UI state such as sidebar state, command palette state, or transient layout preferences |
| Frontend Validation | Zod + React Hook Form | Form schemas, frontend validation, and typed form handling |
| Frontend Tables | TanStack Table + shadcn/ui table primitives | Headless table behavior with design-system rendering |
| Frontend Charts | shadcn/ui chart components built on Recharts | V1 dashboard and trend charts |
| Frontend Localization | i18next + react-i18next when localization is implemented | Bosnian Latin default UI language with optional English UI selection |
| Backend Framework | ASP.NET Core 8 | Backend API, authentication, authorization, middleware, OpenAPI, and endpoint groups |
| Backend API Style | Minimal APIs with endpoint groups | Thin HTTP endpoint layer organized by module |
| Backend Architecture | Clean Architecture + Vertical Slice Application layer | Separation of domain, use cases, infrastructure, and API boundaries |
| Backend Mapping | Mapster | DTO/entity mapping where mapping automation improves clarity |
| Backend Validation | FluentValidation | Request and command validation in the Application layer |
| Authentication | ASP.NET Core Identity-style user management + secure HttpOnly cookies | Staff-only authentication, account lifecycle, invite/setup flow, password reset, and session handling |
| Database | PostgreSQL | Relational source of truth for structured application data |
| File Storage | File storage abstraction with local development adapter and S3-compatible production target | Media, import source files, player images, generated reports, and large artifacts |
| Local Configuration | `backend/.env` and `frontend/.env.local` | Local development configuration and secrets; only example files are committed |
| Production Configuration | Hosting environment variables or managed secret store | Production secrets and deployment configuration |
| Testing | .NET unit/integration tests; frontend type/build/lint checks; later Vitest/RTL/Playwright as needed | Quality gates and regression protection |

## Repository Structure

```txt
/
├── AGENTS.md
├── context/
│   ├── project-overview.md
│   ├── architecture.md
│   ├── ui-context.md
│   ├── code-standards.md
│   ├── ai-workflow-rules.md
│   ├── progress-tracker.md
│   └── feature-specs/
├── frontend/
│   ├── components.json
│   ├── package.json
│   ├── .env.example
│   ├── .env.local              # ignored
│   └── src/
└── backend/
    ├── PlayerPerformance.sln
    ├── .env.example
    ├── .env                    # ignored
    ├── src/
    │   ├── Api/
    │   ├── Application/
    │   ├── Domain/
    │   └── Infrastructure/
    └── tests/
        ├── UnitTests/
        └── IntegrationTests/
```

`AGENTS.md` remains the root entry point that tells AI agents which context files to read before implementation or architecture decisions.

## System Boundaries

### Root

- `/AGENTS.md` — universal entry point for AI coding agents. It should remain small and stable.
- `/context` — product, architecture, UI, code, workflow, progress, and feature-spec documentation.
- `/context/feature-specs` — implementation task units created after the core context files are established.
- `/frontend` — React/Vite application and frontend-only configuration.
- `/backend` — ASP.NET Core solution, backend projects, tests, and backend-only configuration.

### Backend Projects

- `backend/src/Domain` — enterprise/domain rules with no dependency on API, Infrastructure, EF Core, or external providers.
- `backend/src/Application` — vertical slices, commands, queries, DTOs, validators, mapping configuration, use-case orchestration, abstractions, and transaction boundaries.
- `backend/src/Infrastructure` — EF Core, PostgreSQL, Identity implementation, file storage implementations, email provider, CSV/XLSX readers, and future vendor integrations.
- `backend/src/Api` — ASP.NET Core host, Minimal API endpoint groups, authentication/authorization setup, middleware, OpenAPI, dependency injection composition, and HTTP response mapping.
- `backend/tests/UnitTests` — domain, workflow, permission, and application unit tests.
- `backend/tests/IntegrationTests` — API/database integration tests for critical flows.

### Frontend Folders

- `frontend/src/app` — app bootstrap, routing, providers, and route-level composition.
- `frontend/src/components/ui` — generated shadcn/ui primitives. These files remain in place and should not be modified directly unless a task explicitly requires it.
- `frontend/src/components/layout` — application shell, sidebar, top bar, navigation, responsive layout, and page containers.
- `frontend/src/components/common` — reusable app-level components such as page headers, stat cards, data tables, empty states, confirm dialogs, status badges, and form sections.
- `frontend/src/features/*` — feature-owned UI, hooks, API wrappers, schemas, types, and utilities.
- `frontend/src/lib` — shared utilities, API client setup, route helpers, constants, and cross-feature logic.
- `frontend/src/hooks` — shared hooks that are not owned by one feature.
- `frontend/src/types` — shared TypeScript types when they are truly cross-feature.
- `frontend/src/styles` — global styles and theme entry points when needed.

## Dependency Direction

Backend dependency direction must remain stable:

```txt
Domain ← Application ← Infrastructure
          ↑              ↑
          └──── Api ─────┘
```

Rules:

1. `Domain` depends on nothing else in the solution.
2. `Application` depends on `Domain`.
3. `Infrastructure` depends on `Application` and `Domain`.
4. `Api` depends on `Application` and `Infrastructure`.
5. `Api` must not contain business rules, workflow rules, import parsing logic, or direct EF queries.
6. `Application` must not depend on Infrastructure implementations.
7. Domain workflow and invariant logic must not be implemented in controllers, endpoint handlers, or frontend components.

## Backend Architecture

The backend uses Clean Architecture with a Vertical Slice Application layer.

### Api Layer

Owns:

- ASP.NET Core application startup.
- Middleware configuration.
- Authentication and cookie setup.
- Authorization policies.
- Minimal API endpoint groups.
- OpenAPI configuration.
- HTTP request/response mapping.
- Dependency injection composition.

Does not own:

- Business rules.
- Domain invariants.
- Workflow status transitions.
- Import parsing.
- EF Core queries.
- File storage implementation details.

Endpoint handlers should be thin and should delegate to Application use cases.

### Application Layer

Owns:

- Use cases organized as vertical slices.
- Commands and queries.
- Request and command validators.
- Application DTOs.
- Mapping configuration where appropriate.
- Authorization-aware use-case orchestration.
- Transaction boundaries.
- Application service abstractions.
- Calling domain services/entities to perform state changes.

Does not own:

- EF Core implementation details.
- PostgreSQL provider configuration.
- Email provider implementation.
- File storage provider implementation.
- ASP.NET Core endpoint definitions.

### Domain Layer

Owns:

- Core entities and value objects.
- Domain enums.
- Domain services where needed.
- Workflow state machine rules.
- Allowed action rules where they belong to business workflow.
- Domain events where useful.
- Invariants that must hold regardless of UI or API entry point.

Domain must model football performance concepts such as players, teams, assignments, matches, appearances, reports, statuses, availability, imports, media references, and audit-relevant changes.

### Infrastructure Layer

Owns:

- EF Core DbContext and entity configurations.
- PostgreSQL persistence.
- ASP.NET Core Identity persistence implementation.
- Migrations.
- File storage adapters.
- Email/invite delivery implementation.
- CSV/XLSX readers.
- External vendor integration implementations if confirmed later.
- System clock or other infrastructure services when needed.

Infrastructure must implement abstractions defined by Application.

## Backend Modules

V1 backend modules:

- `Auth` — login, logout, session, invite setup, password reset, first-login password setup.
- `Users` — staff management, roles, team scopes, disable/reactivate, audited login email recovery.
- `Teams` — selections, tracking levels, active/archive state, player and staff assignments.
- `Players` — player records, profile data, assignment history, archive/status handling.
- `Matches` — match metadata, competition/season/opponent/venue context, lineup, substitutions, appearances.
- `MatchReports` — player stats, goalkeeper stats, review workflow, allowed actions, verification, corrections.
- `TrainingSessions` — training session metadata and GPS/physical workload tracking.
- `Imports` — upload, parse, preview, mapping, validation, confirmation, import history.
- `Media` — uploaded files, external links, entity attachments, archive behavior.
- `Medical` — availability, injuries, restricted notes, return estimates.
- `Dashboard` — read models and aggregated summaries for staff views.
- `Audit` — audit log creation and entity audit history views.
- `Settings` — seasons, competitions, opponents, venues, and system configuration.

Modules may share domain entities, but feature implementation should stay inside the relevant vertical slice unless code is genuinely shared.

## Domain Model

V1 domain model includes, but is not limited to:

- `Season`
- `Team` / `Selection`
- `TeamTrackingLevel`
- `Player`
- `PlayerTeamAssignment`
- `UserAccount`
- `UserTeamScope`
- `StaffAssignment`
- `Competition`
- `Opponent`
- `Venue`
- `Match`
- `MatchLineup`
- `PlayerMatchAppearance`
- `MatchReport`
- `PlayerMatchStats`
- `GoalkeeperMatchStats`
- `TeamMatchStats`
- `TrainingSession`
- `GpsImport`
- `GpsMetric`
- `ImportJob`
- `MediaAsset`
- `ExternalMediaReference`
- `PlayerAvailability`
- `InjuryRecord`
- `AuditLog`

The application is a single-club system for FK Velež Mostar. A `Club` aggregate/table is not required in V1. Club identity, logo, default venue, and similar values can live in system settings or static configuration. Multi-club tenancy is out of scope.

## Storage Model

### PostgreSQL

PostgreSQL is the relational source of truth for structured data:

- Users and account lifecycle.
- Roles, team scopes, and permissions.
- Seasons, teams/selections, tracking levels, competitions, opponents, and venues.
- Player records and assignment history.
- Matches, lineups, substitutions, appearances, and reports.
- Player, goalkeeper, team, GPS, and availability metrics.
- Import jobs, validation results, mapping metadata, and import history.
- Media metadata and storage keys.
- Audit logs.
- System settings.

### Object/File Storage

Large files and artifacts do not belong directly in PostgreSQL.

File storage owns:

- Uploaded match and training videos.
- Player images.
- Uploaded CSV/XLSX source files.
- Generated reports.
- Thumbnails or derived assets later.
- Other large artifacts.

PostgreSQL stores metadata only:

- File id.
- Storage key.
- Original file name.
- Content type.
- File size.
- Uploaded by.
- Linked entity type.
- Linked entity id.
- Created timestamp.
- Archive status.

Production storage should use S3-compatible object storage or another explicit production-ready provider. Local filesystem storage may be used only as a development adapter behind an application abstraction.

### Storage Abstraction

Use a storage abstraction such as `IFileStorageService` or equivalent in the Application layer.

Implementations:

- `LocalFileStorage` for local development if needed.
- `S3CompatibleStorage` or another provider-specific adapter later for production.

The application must not hardcode production media storage to local filesystem paths.

## Authentication and Access Model

### Authentication

- Closed staff-only authentication.
- No public registration.
- No player login in V1.
- Backend-managed authentication using secure HttpOnly cookies.
- Frontend must not store access tokens in localStorage or sessionStorage.
- ASP.NET Core Identity-style user management for password, user, and account lifecycle features.
- CSRF protection is required for unsafe/mutating cookie-authenticated requests.
- Cookie settings must be secure in production.

### First Admin

- First admin is created from environment-based configuration only when no admin exists.
- No hardcoded credentials.
- No public setup route.
- First admin must be required to change the temporary password after first login.

### User Invitation Flow

1. Admin creates a staff user with name, email, primary role, and team scope.
2. System creates status `INVITED`.
3. System generates a one-time invite/setup token.
4. Email delivery may send the invite link when email infrastructure exists.
5. If email delivery is not available in early V1, the system may generate a one-time setup link for the admin to send through a trusted channel.
6. User sets their own password.
7. User status becomes `ACTIVE`.

### Account Lifecycle

User statuses:

- `INVITED`
- `ACTIVE`
- `DISABLED`
- `LOCKED`

Rules:

- Staff accounts should not be physically deleted for normal offboarding.
- Disable or suspend accounts when staff leave.
- Reuse/reactivate the previous account if staff return.
- Admin can perform an audited login email recovery when a returning staff member no longer has access to the old email.
- Login email recovery must invalidate sessions/tokens and audit old email, new email, actor, target, timestamp, and reason.

### Role and Scope Model

V1 uses one primary role per user plus team scopes and limited explicit permission flags.

Roles:

- `ADMIN`
- `DATA_OPERATOR`
- `ANALYST`
- `COACH`
- `MEDICAL_STAFF`
- `VIEWER`

Team scope types:

- `ALL_TEAMS`
- `SELECTED_TEAMS`

Permission flags may include:

- `canVerifyReports`
- `canImportData`
- `canViewMedicalDetails`

Role behavior:

- `ADMIN` has full access by default.
- `DATA_OPERATOR` can enter draft data, upload/import data if allowed, attach media, and submit reports for review for assigned teams.
- `ANALYST` can review reports for assigned teams and can verify reports only with `canVerifyReports`.
- `COACH` can view assigned teams, dashboards, player profiles, verified reports, physical metrics, and availability summaries. Editing statistics is not allowed by default.
- `MEDICAL_STAFF` can manage availability and medical-related records for assigned teams. Sensitive medical notes remain restricted.
- `VIEWER` has read-only access to assigned teams.

Backend authorization is the source of truth. The frontend may hide unavailable actions, but server-side checks must always enforce permissions.

## Workflow State Machines

Workflow status changes must go through backend state transition services or domain methods. Endpoint handlers and frontend components must not assign workflow statuses directly.

### Match Report Statuses

Statuses:

- `DRAFT`
- `READY_FOR_REVIEW`
- `VERIFIED`
- `NEEDS_CORRECTION`
- `ARCHIVED`

Allowed transitions:

- `DRAFT` → `READY_FOR_REVIEW`
- `READY_FOR_REVIEW` → `VERIFIED`
- `READY_FOR_REVIEW` → `NEEDS_CORRECTION`
- `NEEDS_CORRECTION` → `READY_FOR_REVIEW`
- `VERIFIED` → `NEEDS_CORRECTION`
- `VERIFIED` → `ARCHIVED`

Allowed actions are calculated by backend based on report status, user role, team scope, and explicit permissions.

Example actions:

- `DRAFT`: edit, attach video, import GPS, submit for review, delete draft.
- `READY_FOR_REVIEW`: review, verify, request correction.
- `VERIFIED`: view, export, request correction, view audit log.
- `NEEDS_CORRECTION`: edit, resubmit for review.
- `ARCHIVED`: view, restore if admin.

### Import Statuses

Statuses:

- `UPLOADED`
- `PARSING`
- `VALIDATION_FAILED`
- `READY_TO_CONFIRM`
- `IMPORTED`
- `FAILED`
- `CANCELLED`

Import confirmation must be explicit. Parsed data must not silently mutate official match/player data without validation and confirmation.

### Media Statuses

Statuses may include:

- `UPLOADED`
- `PROCESSING`
- `READY`
- `FAILED`
- `ARCHIVED`

If no processing is implemented in early V1, the architecture should still leave room for it.

### Availability Statuses

Statuses:

- `AVAILABLE`
- `LIMITED`
- `UNAVAILABLE`
- `REHAB`
- `UNKNOWN`

### User Account Statuses

Statuses:

- `INVITED`
- `ACTIVE`
- `DISABLED`
- `LOCKED`

## API Contracts, Validation, and Errors

### API Style

- Use Minimal API endpoint groups organized by backend module.
- Use consistent route prefixes such as `/api/matches`, `/api/players`, `/api/imports`, and `/api/users`.
- Endpoint handlers should map HTTP concerns and delegate to Application use cases.
- Backend should expose `allowedActions` in responses where workflow actions are state/permission-dependent.

### Validation

- Use FluentValidation for backend command/request validation.
- Frontend Zod validation improves UX but never replaces backend validation.
- Backend validation must protect every mutation.

### Error Handling

Use ProblemDetails-compatible errors and consistent status codes:

- `400` for malformed requests.
- `401` for unauthenticated requests.
- `403` for authenticated users lacking permission.
- `404` when a requested entity does not exist or is not accessible in the current scope.
- `409` for conflicts such as invalid workflow transition or concurrency conflicts.
- `422` for semantic validation errors if adopted by the API conventions.
- `500` only for unexpected server errors.

Errors should be safe to display, must not leak secrets, and should include enough information for the frontend to show clear validation or workflow feedback.

## Frontend Architecture

### Feature-Based Structure

Frontend feature folders own their own components, hooks, API wrappers, schemas, types, and utilities unless code is genuinely shared.

Example:

```txt
frontend/src/features/matches/
├── api/
├── components/
├── hooks/
├── schemas/
├── types/
├── utils/
└── index.ts
```

Shared logic should not be moved to global folders until at least two features need it.

### State Ownership

- Server data belongs in TanStack Query.
- URL/filter state belongs in the smallest appropriate owner: React Router search params for explicitly shareable navigation state, or local React state for high-frequency operational filters such as Players and Users search.
- Reusable global client-only UI state may belong in Zustand.
- Stable app-level providers belong in React Context providers.
- Do not duplicate server-owned data in Zustand or Context.

### UI Rendering

- shadcn/ui primitives render design-system components.
- TanStack Table owns table behavior.
- shadcn/ui table primitives render tables visually.
- shadcn/ui chart components built on Recharts provide V1 charts.
- React Hook Form and Zod own form state and frontend form validation.


## Localization Model

The application UI language is Bosnian Latin by default. English may be supported as an optional selectable UI language.

Localization rules:

- User-facing UI text should be localizable once the frontend localization foundation is implemented.
- Do not hardcode long-lived visible UI copy across feature components after localization infrastructure exists.
- Internal enum names, database values, API contract names, and code identifiers should remain in English.
- Display labels for statuses, actions, navigation, validation messages, empty states, and errors should be localized.
- User-entered domain data, such as names, notes, opponents, venues, competitions, and comments, is stored and displayed as entered. It is not automatically translated.
- Date and time presentation should follow Bosnian/BiH staff expectations by default, including 24-hour time and clear day-month-year formatting where appropriate.
- The selected language may initially be stored as a client preference and can later move to a server-side user preference when user settings are implemented.

Localization packages should be installed just in time, ideally in the frontend foundation or first UI shell feature spec that introduces real user-facing copy.

## Configuration and Secrets

### Local Development

Local development uses environment-style configuration files:

- `backend/.env` for backend local configuration and backend secrets.
- `frontend/.env.local` for frontend Vite configuration.

Committed examples:

- `backend/.env.example`
- `frontend/.env.example`

Rules:

- Real `.env` files must not be committed.
- Backend `.env` values may contain local development secrets.
- Frontend `VITE_*` variables are public browser configuration and must not contain secrets.
- ASP.NET Core should load `backend/.env` in local development through a small mature loader package when configuration setup is implemented.
- Production must not depend on `.env` files.

### Future Docker

Docker is out of scope for the initial local setup. If Docker Compose is introduced later, it may use a root `.env` file consumed by `docker-compose.yml`.

Docker migration should require changing environment values such as hostnames/ports, not rewriting application configuration patterns.

### Production

Production configuration must come from hosting-provider environment variables or a managed secret store.

Never hardcode:

- Credentials.
- Connection strings.
- Initial admin passwords.
- Cookie/signing secrets.
- Storage keys.
- SMTP credentials.
- Vendor API tokens.

## External Data Sources and Imports

### Gpexe

Gpexe is expected to provide GPS/physical data exports, but exact available fields must be confirmed from real export samples before implementation.

Supported physical metrics may include total distance, high-speed running distance, sprint distance, number of sprints, max speed, accelerations, decelerations, player load, and session duration where available.

### Zone14

Zone14 is treated in V1 as a video, tagging/support, media reference, and possible running-stat source only where confirmed.

The system must not assume Zone14 provides full football event data such as passes, shots, duels, recoveries, or ball actions unless real exports prove it.

### Import Architecture Rules

- Imports must be previewed before confirmation.
- Imports must be validated before mutation of official data.
- Import mappings must be extensible.
- Unknown vendor fields must not be guessed.
- Store original import files or file references for auditability.
- Imported data must record source type, import job, imported by, imported at, and validation result.

## Audit Model

Audit logging is required for important changes.

Minimum audit coverage:

- Match report status changes.
- Player match statistics changes.
- Imports and import confirmations.
- Medical/availability changes.
- User role, scope, account status, and permission changes.
- Login email changes and account recovery actions.
- Verification, correction request, archive, and restore actions.

Audit log fields should include:

- Audit id.
- Actor user id.
- Action.
- Entity type.
- Entity id.
- Timestamp.
- Previous values for important changes.
- New values for important changes.
- Metadata JSON where useful.

## Testing and Quality Gates

### Backend

Backend testing should prioritize domain and workflow correctness:

- Unit tests for domain rules and workflow transitions.
- Unit tests for allowed actions and permission logic.
- Unit tests for player assignment rules.
- Integration tests for critical API flows.
- Import validation tests when import formats are implemented.
- Build and relevant tests must pass before backend task completion.

Critical flows to test:

- Match report status transitions.
- Report verification permission behavior.
- User invite, activate, disable, reactivate, and login email recovery.
- Player team assignment history.
- Import preview, validation, and confirmation.
- Audit log creation for important mutations.

### Frontend

Frontend quality gates:

- TypeScript build must pass.
- Vite build must pass when required by the task.
- ESLint must pass if configured.
- No raw Tailwind palette classes in application UI.
- No deep relative source imports.
- No ad-hoc visual overrides on shadcn/ui primitive usage.
- Forms use Zod schemas.
- Server data uses TanStack Query.

Vitest, React Testing Library, and Playwright may be introduced later for important utilities, hooks, forms, and end-to-end flows.

## Deployment Assumptions

Deployment provider is not finalized in the initial context phase.

The system must remain deployable as:

- React/Vite static frontend build.
- ASP.NET Core 8 backend API.
- PostgreSQL relational database.
- S3-compatible object storage or equivalent production file storage.

Local development initially uses:

- Vite dev server for frontend.
- `dotnet run` for backend.
- Local PostgreSQL.
- `backend/.env` and `frontend/.env.local`.

Docker-based local development and production hosting decisions are intentionally deferred.

## Invariants

1. The application is a single-club internal system for FK Velež Mostar. Multi-club tenancy is out of scope for V1.
2. Players are persistent club entities and must not be recreated when moving between selections.
3. Team/selection tracking levels must control available workflows and fields; the system must not assume all teams track the same metrics.
4. Non-admin access must be restricted by team scope and server-side authorization.
5. Backend authorization is the source of truth. Frontend-hidden actions are convenience only.
6. Workflow status changes must go through backend state transition logic, not direct status assignment in endpoints or UI.
7. Match statistics must link to concrete match appearances, not only to current player-team assignment.
8. Verified data must not be casually overwritten without a workflow transition and audit trail.
9. Important mutations must be audited with actor, action, entity, timestamp, and relevant change details.
10. No public registration and no player login exist in V1.
11. Authentication uses secure backend-managed HttpOnly cookies; tokens must not be stored in browser localStorage or sessionStorage.
12. Secrets, credentials, connection strings, storage keys, and initial admin passwords must never be hardcoded or committed.
13. PostgreSQL stores structured data and metadata. Large files must live in file/object storage.
14. Production media storage must not depend on local filesystem paths.
15. Exact Gpexe and Zone14 import fields must not be guessed. Real export samples are required before implementing field-specific mappings.
16. Application code must remain within documented backend and frontend boundaries.
17. Install packages just in time when a task requires them; do not add dependencies speculatively.
18. User-facing UI copy is Bosnian Latin by default and must remain localizable when localization infrastructure exists.
19. Context files must be updated before or with implementation when architecture, scope, or standards change.
