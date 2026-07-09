# Code Standards

## General Principles

- Keep modules small, explicit, and single-purpose.
- Fix root causes instead of layering workarounds.
- Do not mix unrelated concerns in one component, endpoint, handler, service, or module.
- Respect the system boundaries defined in `context/architecture.md`.
- Prefer boring, stable, maintainable solutions over clever abstractions.
- Do not implement speculative features, future integrations, or generalized frameworks unless a feature spec explicitly requires them.
- Install dependencies just in time, only when the current task needs them.
- Keep implementation aligned with the current context files. If a task changes architecture, scope, standards, or workflow assumptions, update the relevant context file before or together with the implementation.
- Never hardcode credentials, secrets, connection strings, temporary admin passwords, storage keys, API keys, or signing/cookie secrets.
- Preserve auditability for important business mutations.

## Repository Organization

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
└── backend/
```

- `AGENTS.md` is the root entry point for AI coding agents.
- `context/` contains the project operating context and must be read before implementation.
- `context/feature-specs/` contains scoped implementation units.
- `frontend/` contains the React/Vite application.
- `backend/` contains the ASP.NET Core solution, source projects, and tests.
- Do not duplicate context decisions across random files. Update the appropriate context file when project-level decisions change.

## Backend Standards

### Platform

- Use ASP.NET Core 8 and .NET 8 LTS.
- Use PostgreSQL for relational data.
- Use Clean Architecture with separate projects:
  - `Api`
  - `Application`
  - `Domain`
  - `Infrastructure`
  - `UnitTests`
  - `IntegrationTests`
- Use Minimal APIs with endpoint groups.
- Use Vertical Slice organization inside the Application layer.
- Use Mapster for repetitive DTO/entity mapping when mapping is needed.
- Use FluentValidation for backend request/use-case validation when validation is needed.
- Use mature NuGet packages for common concerns, but add them only when required by the active task.

### Backend Layer Rules

#### Api

- Contains ASP.NET Core startup, middleware, dependency injection composition, auth configuration, endpoint groups, OpenAPI configuration, and HTTP concerns.
- Endpoint handlers must stay thin.
- Endpoint handlers must not contain business rules, state transition logic, import parsing, or EF Core query logic.
- Endpoint handlers validate HTTP-specific concerns, call Application use cases, and return consistent HTTP responses.
- All protected endpoint groups must require authorization unless the feature spec explicitly defines a public endpoint.

#### Application

- Contains use cases, commands, queries, handlers, validators, DTOs, application services, and interfaces needed by the use cases.
- Coordinates domain behavior and infrastructure abstractions.
- Owns transaction boundaries at the use-case level where appropriate.
- Must not depend on Infrastructure implementation details.
- Must not expose EF Core entities or database-specific concerns to API handlers.
- Must enforce workflow and permission rules by calling the appropriate domain/application services.

#### Domain

- Contains entities, value objects, enums, domain events, domain services, invariants, and workflow/state machine rules.
- Must not depend on Api, Application, Infrastructure, EF Core, ASP.NET Core, or external services.
- Owns business invariants and allowed state transitions.
- Workflow statuses must not be changed by directly assigning enum values outside approved transition methods/services.

#### Infrastructure

- Contains EF Core DbContext, entity configurations, migrations, Identity persistence, PostgreSQL implementation, file/object storage implementations, email provider implementation, import file readers, and external integration adapters.
- Implements interfaces defined by Application.
- Must not contain business workflow decisions that belong in Domain or Application.
- Must not leak provider-specific details into Domain or Api.

### Dependency Direction

- Domain has no project dependencies.
- Application depends on Domain.
- Infrastructure depends on Application and Domain.
- Api depends on Application and Infrastructure.
- Never introduce reverse dependencies to “make something easier.”

### Backend Naming and Structure

- Name files after the responsibility they contain, not the pattern alone.
- Keep endpoint groups organized by module, such as Auth, Users, Teams, Players, Matches, MatchReports, TrainingSessions, Imports, Media, Medical, Dashboard, Audit, and Settings.
- Keep commands and queries scoped to one use case.
- Avoid generic service names such as `DataService`, `Manager`, or `Helper` unless the responsibility is genuinely generic and clear.
- Prefer explicit names such as `SubmitMatchReportForReview`, `VerifyMatchReport`, `CreatePlayerTeamAssignment`, or `ParseGpexeImportPreview`.

### Backend Validation

- Validate request input before executing business logic.
- Treat all external input as untrusted, including API requests, uploaded files, import rows, route parameters, and query parameters.
- Use FluentValidation for command/query validation when rules exceed trivial checks.
- Backend validation is mandatory even when frontend validation exists.
- Do not trust frontend Zod schemas as the source of truth for backend correctness.
- Import validation must report row-level and field-level errors where possible.

### API Responses and Errors

- Return consistent and predictable response shapes.
- Use ProblemDetails-compatible error responses.
- Use appropriate HTTP status codes:
  - `400` for malformed requests.
  - `401` for unauthenticated requests.
  - `403` for authenticated users without permission.
  - `404` for missing entities.
  - `409` for conflicts, invalid state transitions, or concurrency conflicts.
  - `422` for semantic validation errors when the request is syntactically valid but fails validation.
- Do not leak internal exception details to clients.
- Include correlation/request identifiers in logs and error handling when available.
- For workflow-driven resources, backend responses should include `allowedActions` where the UI needs to display valid actions.

### Authentication and Authorization

- Use backend-managed secure HttpOnly cookies for authenticated sessions.
- Do not store auth tokens in browser localStorage or sessionStorage.
- Use CSRF protection for state-changing requests when cookie authentication is used.
- Enforce role, team scope, and permission checks server-side on every protected query and mutation.
- UI visibility is not authorization. Backend checks are mandatory.
- The V1 access model is one primary role per user plus team scopes plus limited explicit permission flags.
- Analysts can verify match reports only when explicitly granted verification permission.
- Admin users have full access by default.
- Disabled users must not be able to authenticate.

### Workflow and State Machines

- Use explicit state transition methods/services for workflow entities.
- Do not directly assign workflow status values in endpoint handlers or ad-hoc update code.
- State transitions must validate current status, actor role, team scope, and explicit permission flags.
- Invalid state transitions should fail clearly and return an appropriate conflict or authorization error.
- Expose backend-calculated `allowedActions` for workflow resources where needed.
- Important workflow transitions must be audited.

### Data Access

- Use EF Core through Infrastructure.
- Keep database queries out of Api endpoint handlers.
- Keep persistence-specific mapping/configuration in Infrastructure.
- Use PostgreSQL relational modeling for metadata, relationships, users, players, teams, matches, reports, imports, availability, and audit logs.
- Do not store large media files, import source files, or generated files directly in PostgreSQL.
- Database records for files must store metadata and storage keys/references only.


### Localization and UI Copy

- Bosnian Latin is the default UI language.
- When writing visible Bosnian Latin UI copy, use proper Bosnian Latin characters such as `č`, `ć`, `š`, `ž`, and `đ`; do not replace them with ASCII fallbacks.
- English may be supported as an optional selectable UI language.
- Keep source code, identifiers, file names, route names, API contracts, enum values, and translation keys in English.
- User-facing labels, navigation, page titles, button text, validation messages, empty states, and error messages should be localized once localization infrastructure exists.
- Do not hardcode long-lived visible UI copy in feature components after the localization foundation is implemented.
- Do not translate user-entered domain data such as player names, staff names, opponents, venues, competitions, notes, or imported values.
- Display labels for backend enum/status values must be localized in the UI instead of showing raw enum names.
- Prefer 24-hour time and clear day-month-year date formatting for Bosnian UI.
- Add localization dependencies only in the feature spec that implements the localization foundation or first requires real localized UI behavior.

### Imports

- Do not guess unconfirmed Gpexe or Zone14 export fields.
- Import features must be mapping-friendly and extensible.
- Store original import source files for auditability where required.
- Parse, preview, validate, and confirm imports as separate workflow steps.
- Do not commit to full Zone14 event-data assumptions unless real export samples confirm them.
- Import code should separate file reading, format detection, row parsing, validation, mapping, and persistence.

### Audit Logging

- Audit important mutations, including match report changes, player stat changes, imports, verification actions, medical/availability changes, user role/scope changes, account disable/reactivate, and login email recovery.
- Audit records should include actor, action, entity type, entity id, timestamp, and meaningful metadata.
- Do not remove or overwrite audit history during normal operations.

## Frontend Standards

### Platform

- Use React, Vite, and TypeScript.
- Use Tailwind CSS v4.
- Use shadcn/ui primitives in `src/components/ui`.
- Use the design tokens and component rules defined in `context/ui-context.md`.
- Use feature-based organization.
- Use strict TypeScript and avoid unsafe typing.

### Frontend Folder Structure

```txt
frontend/src/
├── app/
├── components/
│   ├── ui/
│   ├── layout/
│   └── common/
├── features/
│   ├── auth/
│   ├── users/
│   ├── teams/
│   ├── players/
│   ├── matches/
│   ├── match-reports/
│   ├── training-sessions/
│   ├── imports/
│   ├── media/
│   ├── dashboard/
│   └── medical/
├── hooks/
├── lib/
├── styles/
└── types/
```

- `app/` contains app setup, routing, providers, and top-level shell wiring.
- `components/ui/` contains generated shadcn/ui primitives and must not be moved.
- `components/layout/` contains app layout components such as sidebar, topbar, page shell, and navigation.
- `components/common/` contains reusable application components such as page headers, empty states, status badges, confirm dialogs, stat cards, and form sections.
- `features/<feature>/` contains feature-owned components, hooks, API clients, schemas, types, utilities, and index exports.
- Keep feature code inside its feature folder unless it is genuinely shared.
- Do not create broad shared utilities prematurely.

### TypeScript

- TypeScript strict mode is required.
- Avoid `any`. Use explicit interfaces, discriminated unions, or narrow types.
- Use `unknown` for untrusted values and validate before use.
- Prefer `interface` for object contracts and public component props.
- Prefer `type` for unions, utility types, and composition.
- Do not suppress TypeScript errors with `@ts-ignore` unless a task explicitly justifies it and the reason is documented.
- Keep frontend API types aligned with backend contracts.
- Validate external input and complex form data with Zod.

### Imports

- Use absolute imports through the `@/` alias for frontend source imports.
- Avoid deep relative imports such as `../../../`.
- Same-folder or immediately adjacent relative imports are acceptable when clearer.
- Configure `@/*` to map to `frontend/src/*` in TypeScript and Vite configuration.
- Prefer feature index exports when they improve readability and avoid circular dependencies.
- Do not create circular imports between features.

Preferred:

```ts
import { Button } from "@/components/ui/button";
import { MatchStatusBadge } from "@/features/matches/components/match-status-badge";
```

Avoid:

```ts
import { Button } from "../../../components/ui/button";
```

### Server State, URL State, and Client State

- Use TanStack Query for server state, API fetching, caching, mutations, and invalidation.
- Do not duplicate server-owned data in Zustand or React Context.
- Use `nuqs` for URL/search-parameter state such as filters, selected season, selected team, tabs, pagination, and search terms.
- Use Zustand only for global client-side UI state that is not server data and does not belong in the URL.
- Use React Context Providers for stable app-level context and provider composition, such as auth/session provider, query client provider, and static app configuration.
- Keep form state inside form libraries/components unless it must be shared outside the form.

### Forms and Validation

- Use React Hook Form with Zod for forms.
- Use `@hookform/resolvers` when integrating Zod schemas with React Hook Form.
- Define schemas close to the feature that owns the form.
- Keep form sections readable and grouped by user task.
- Display clear validation messages.
- Frontend validation improves UX but does not replace backend validation.
- Avoid building custom form frameworks unless explicitly required.

### Tables

- Use TanStack Table for table behavior such as sorting, filtering, pagination, row selection, column visibility, and import previews.
- Use shadcn/ui table primitives for rendering.
- Do not confuse shadcn table primitives with a table behavior engine.
- Keep table column definitions close to the feature that owns the table.
- For repeated table patterns, create reusable app-level table components with semantic props.

### Charts

- Use shadcn/ui chart components built on Recharts for V1 charts.
- Do not add Tremor, ECharts, visx, Evil Charts, or another chart library unless a specific feature requirement cannot be met with shadcn/Recharts.
- Keep chart colors token-based.
- Charts must have readable labels, accessible tooltips where possible, and useful empty states.

### Styling

- Use Tailwind CSS v4 utilities backed by semantic tokens in `index.css`.
- Do not use raw Tailwind palette classes such as `bg-zinc-950`, `text-slate-500`, `border-gray-200`, or similar in application UI.
- Do not hardcode hex, rgb, hsl, or oklch values in component files.
- Use semantic token utilities such as `bg-background`, `text-foreground`, `bg-card`, `border-border`, `text-muted-foreground`, `bg-primary`, and project-approved token utilities.
- Respect the radius, shadow, spacing, and typography decisions in `context/ui-context.md`.
- The application is light-only. Do not add dark mode classes, `.dark` theme tokens, theme toggles, or dark-specific UI behavior unless explicitly requested later.

### shadcn/ui Component Rules

- Generated shadcn/ui primitives live in `frontend/src/components/ui`.
- Do not move generated shadcn/ui primitive files.
- Do not modify generated shadcn/ui primitive files unless a task explicitly requires it.
- Use shadcn/ui primitives through their documented variants and default behavior.
- Do not apply ad-hoc color, border, shadow, or typography overrides directly to shadcn/ui component usages.
- Layout-only utility classes are allowed on shadcn/ui usages when needed for width, spacing, grid placement, flex behavior, alignment, or responsive behavior.
- Repeated visual variations must be implemented through documented component variants or app-level reusable components, not through one-off local styling.

Allowed layout-only example:

```tsx
<Button className="w-full">Create Match</Button>
<Card className="col-span-2">...</Card>
```

Avoid visual override example:

```tsx
<Button className="bg-red-700 text-white border-yellow-400 shadow-xl">
  Save
</Button>
```

### Components

- Keep components focused on rendering and local interaction.
- Do not place API calls directly inside deeply nested presentational components.
- Use feature hooks or TanStack Query hooks for data access.
- Extract reusable app-level components only when a pattern repeats.
- Prefer semantic component props over forcing callers to pass arbitrary class names for visual variants.
- Keep role/status display logic centralized in reusable status badge/action components where appropriate.

## Configuration and Environment Variables

### Backend

- Local backend configuration uses `backend/.env`.
- Commit only `backend/.env.example`.
- ASP.NET Core must load local `.env` configuration in development through a small, mature loader package or a clearly documented equivalent when backend configuration is implemented.
- Production must not depend on `.env` files. Production uses real environment variables or a managed secret store.
- Use ASP.NET Core nested environment variable naming where appropriate, such as `ConnectionStrings__DefaultConnection`.
- Validate required backend configuration at startup and fail clearly when required values are missing.

### Frontend

- Local frontend configuration uses `frontend/.env.local`.
- Commit only `frontend/.env.example`.
- Vite-exposed variables must use the `VITE_` prefix.
- `VITE_*` variables are public browser configuration and must not contain secrets.

### Git Ignore

- Real `.env` files must be ignored.
- Example files may be committed.

Recommended ignore rules:

```gitignore
.env
.env.*
**/.env
**/.env.*
!.env.example
!**/.env.example
```

## Package and Dependency Rules

- Prefer mature, stable packages for common concerns.
- Install dependencies just in time, only when the active task needs them.
- Do not install packages speculatively because they might be useful later.
- Feature specs should list required dependencies for that task.
- Do not replace established project choices without updating the relevant context file.
- For frontend, selected standard libraries include TanStack Query, nuqs, Zustand, Zod, React Hook Form, TanStack Table, shadcn/ui, and Recharts through shadcn charts.
- For backend, selected likely libraries include EF Core, Npgsql provider, ASP.NET Core Identity persistence, Mapster, FluentValidation, CSV/XLSX readers when needed, and test packages when needed.

## Testing and Quality Gates

### Backend

- `dotnet build` must pass before a backend task is considered complete.
- Run relevant `dotnet test` commands when tests exist or when the task changes test-covered logic.
- Add unit tests for domain rules, workflow state transitions, permission/allowedActions logic, and value objects when those areas are implemented.
- Add integration tests for critical API flows when the feature scope justifies them.
- Testcontainers PostgreSQL may be introduced later when integration testing requires production-like database behavior.
- Backend workflow logic should be easier to test because it lives outside endpoint handlers.

### Frontend

- TypeScript build/typecheck must pass before a frontend task is considered complete.
- Vite build must pass when configured and practical for the task.
- ESLint must pass if configured.
- UI code must comply with token, import, and shadcn usage rules.
- Do not introduce raw color utilities, hardcoded colors, deep relative imports, or one-off shadcn visual overrides.
- UI/E2E tests can be introduced later for critical workflows. They are not mandatory for every initial UI task unless a feature spec requires them.

## Security Standards

- Never hardcode secrets.
- Never commit real `.env` files.
- Never expose backend secrets through frontend `VITE_*` variables.
- Do not log passwords, reset tokens, invite tokens, cookies, storage keys, or connection strings.
- Use secure HttpOnly cookies for authentication.
- Use CSRF protection for cookie-authenticated mutations.
- Invalidate sessions/tokens when changing login email, resetting password, disabling accounts, or performing sensitive recovery actions.
- Apply least privilege: users only access data/actions allowed by role, scope, and explicit permissions.
- Medical notes and sensitive availability details require restricted access.

## Performance and UX Standards

- Optimize dense data-entry workflows for desktop and tablet first.
- Mobile browsers must render cleanly and support essential viewing/lightweight actions.
- Avoid unnecessary rerenders caused by overusing React Context for frequently changing state.
- Use TanStack Query caching instead of manually caching server data in client stores.
- Use pagination, filtering, and server-side query parameters for large lists when needed.
- Import previews should handle validation errors clearly and avoid freezing the UI on large files.
- Prefer simple, predictable loading, empty, and error states.

## Documentation Standards

- Update `context/progress-tracker.md` after each meaningful implementation change.
- Update context files when implementation changes architecture, scope, standards, or workflow decisions.
- Keep feature specs scoped, buildable, and explicit.
- Do not leave misleading comments or stale TODOs.
- Document non-obvious decisions close to the code when necessary.
- Do not document implementation guesses as facts, especially for Gpexe and Zone14 export fields.

## Non-Negotiable Rules

- Do not violate Clean Architecture dependency direction.
- Do not put business logic in API endpoint handlers.
- Do not bypass backend authorization.
- Do not directly mutate workflow statuses outside approved transition logic.
- Do not store large files directly in PostgreSQL.
- Do not assume unconfirmed vendor export fields.
- Do not use raw Tailwind palette classes in application UI.
- Do not visually override shadcn/ui primitives with ad-hoc color, border, shadow, or typography classes.
- Do not use deep relative frontend imports when `@/` imports are available.
- Do not add dark mode.
- Do not ignore the Bosnian Latin default UI language decision.
- Do not hardcode or commit secrets.
