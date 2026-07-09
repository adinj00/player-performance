# AI Workflow Rules

## Approach

Build this project incrementally through a context-first, spec-driven workflow. The context files define the product, architecture, UI system, code standards, and current project state. Feature specs define the exact buildable unit to implement. Implement only what is defined by the context files and the active feature spec.

The developer is the architect. The AI coding agent is the implementation engine. Do not replace documented decisions with guesses. Do not expand scope because a related improvement seems useful.

Before implementing or making architectural decisions, read the root `AGENTS.md` file and the context files in the order listed there.

## Working Modes

Use the correct mode for the current task.

### Discovery Mode

Use discovery mode when requirements, architecture, UI behavior, data sources, or workflow rules are still being clarified.

- Ask focused questions only when the answer is necessary to avoid a bad architectural decision.
- Prefer documenting confirmed decisions over speculating.
- Add unresolved items to `context/progress-tracker.md` as open questions.
- Do not implement production code during discovery.

### Context Documentation Mode

Use context documentation mode when creating or updating the foundation files.

- Update the relevant context file directly.
- Keep context files specific, concrete, and implementation-guiding.
- Do not add aspirational features that are not confirmed.
- Keep out-of-scope sections explicit.
- Keep `context/progress-tracker.md` aligned with the current state.

### Feature Spec Mode

Use feature spec mode when breaking the build into implementation units.

- Create one feature spec per buildable unit.
- Keep each spec small enough to verify end to end.
- Do not combine unrelated backend, frontend, import, and infrastructure work unless the feature cannot function without all of them.
- Place feature specs in `/feature-specs` unless the project owner explicitly changes the location later.
- Number feature specs in implementation order, for example `01-backend-solution-structure.md`.

Each feature spec must follow the README structure as the canonical format:

1. Goal
   - 1-2 specific, concrete sentences.
2. Design
   - Visual, structural, API, data, workflow, and architectural decisions relevant to this unit.
   - Include scope and out-of-scope notes here when needed.
3. Implementation
   - Break implementation into concrete sub-sections.
   - Include required reading, backend work, frontend work, data/model changes, UI requirements, workflow rules, and edge cases as relevant to the unit.
4. Dependencies
   - Packages to install for this unit only.
   - Write `None` when no new dependency is required.
5. Verification checklist
   - Build, test, lint/typecheck, manual verification, acceptance criteria, and documentation updates required before the unit is complete.

Additional details are allowed inside these five headings, but do not replace the five-heading structure with a different template unless the project owner explicitly requests it.

### Implementation Mode

Use implementation mode only when a feature spec exists or the project owner explicitly asks for a small direct change.

- Read the active feature spec before editing code.
- Implement exactly what the feature spec says.
- Do not add features, packages, abstractions, screens, endpoints, or integrations that are not required by the spec.
- Stop and update the relevant context file if the implementation reveals a context-level decision must change.
- Update `context/progress-tracker.md` after each meaningful implementation change.

## Scoping Rules

- Work on one feature unit at a time.
- Prefer small, verifiable increments over broad speculative changes.
- Keep backend, frontend, database, import, media, and deployment work separated unless the active spec intentionally combines them.
- Do not introduce production hosting, Docker, external APIs, object storage providers, or advanced analytics unless the active spec requires them.
- Do not implement direct vendor integrations before access, documentation, licensing, and real export samples are confirmed.
- Do not implement player login or public registration.
- Do not implement multi-club or multi-tenant behavior in V1.
- Do not implement dark mode.

## When To Split Work

Split a task into smaller feature specs if it combines:

- Backend project structure and frontend layout work.
- Authentication infrastructure and role-management UI.
- Database schema design and complex frontend dashboards.
- Import parsing and match-report verification workflow.
- Media upload/storage and video analysis UI.
- Multiple unrelated API modules.
- Multiple unrelated entity models.
- New packages that require separate configuration and unrelated feature behavior.
- Behavior not clearly defined in the context files.

If a change cannot be built, reviewed, and verified end to end quickly, the scope is too broad. Split it.

## Handling Missing Requirements

Do not invent missing product behavior.

When a requirement is missing or ambiguous:

1. Check `context/project-overview.md`, `context/architecture.md`, `context/ui-context.md`, `context/code-standards.md`, and `context/progress-tracker.md`.
2. If the answer is not documented, add the issue to `context/progress-tracker.md` as an open question.
3. Resolve the requirement in the relevant context file before implementing behavior that depends on it.
4. If the implementation can continue safely without that answer, keep the uncertain behavior out of scope for the current spec.

Project-specific missing requirement rules:

- Do not guess Gpexe export columns.
- Do not guess Zone14 export columns.
- Do not assume Zone14 provides full football event data such as passes, shots, duels, possession losses, or ball actions unless confirmed by real export samples.
- Do not assume SofaScore or other public-data APIs are available, licensed, stable, or suitable for V1.
- Do not hardcode team selections, player stats fields, or import mappings that are meant to be configurable or confirmed later.

## Dependency Rules

- Install packages just in time, only when the active feature spec requires them.
- Do not install speculative packages for future features.
- Prefer mature, stable packages for common concerns.
- Document newly added packages in the implementation notes or progress tracker when meaningful.
- Do not replace an approved library choice without updating `context/architecture.md` and `context/code-standards.md` first.

Approved frontend library direction:

- TanStack Query for server state.
- nuqs for URL/search-parameter state.
- Zustand only for global client-side UI state.
- React Context Providers for stable app-level provider composition.
- Zod for frontend schemas.
- React Hook Form with Zod for forms.
- TanStack Table for table behavior.
- shadcn/ui table primitives for rendering.
- shadcn/ui charts built on Recharts for V1 charts.

Approved backend library direction:

- ASP.NET Core 8.
- PostgreSQL with EF Core when persistence is implemented.
- Mapster for repetitive DTO/entity mapping when useful.
- FluentValidation for backend validation when validation infrastructure is implemented.
- Mature CSV/XLSX parsing packages only when import work requires them.
- A small mature `.env` loader package only when backend configuration setup requires it.

## Protected Files and Areas

Do not modify protected foundation files unless the active task explicitly requires it.

Protected frontend areas:

- Generated shadcn/ui primitive components in `frontend/src/components/ui/*`.
- Third-party library internals.
- Generated files from package managers or tooling, except when the task intentionally changes dependencies.

Project-specific UI customization must happen through:

- `frontend/src/styles/index.css` design tokens.
- App-level reusable components.
- Documented component variants.
- Feature-level components.

Do not apply ad-hoc visual overrides directly to shadcn/ui component usages. Layout-only utility classes are allowed for width, spacing, grid placement, flex behavior, alignment, and responsive behavior.

Protected backend boundaries:

- Do not put business logic in API endpoint handlers.
- Do not put EF Core implementation details in Domain or Application layers.
- Do not bypass workflow state transition services.
- Do not bypass authorization checks for mutations.
- Do not store browser tokens in localStorage or sessionStorage.

## Documentation Sync Rules

Keep documentation synchronized with implementation.

Update the relevant context file before or alongside implementation when a change affects:

- Product scope.
- System architecture.
- Layer boundaries.
- Storage model.
- Authentication or authorization model.
- Workflow statuses or allowed actions.
- Import behavior or confirmed vendor fields.
- UI design tokens, visual rules, or component conventions.
- Code standards.
- Local configuration or deployment assumptions.

Update `context/progress-tracker.md` after each meaningful implementation change.

`context/progress-tracker.md` should reflect actual project state, not intended state. At the start of the project it may be mostly empty. As `/feature-specs` are added and implemented, use it to track current phase, completed units, active unit, next units, open questions, and decisions made during implementation.

## Progress Tracker Rules

At minimum, keep `context/progress-tracker.md` updated with:

- Current phase.
- Active feature spec, if any.
- Completed feature specs.
- Next planned feature specs.
- Open questions.
- Confirmed decisions made after the initial context files.
- Known blockers.

Do not mark a unit complete until its acceptance criteria and verification steps pass or the failure is explicitly documented.

## Verification Before Moving To The Next Unit

Before moving to another feature spec, verify the current unit.

Backend verification:

- Run `dotnet build` for affected backend projects.
- Run relevant `dotnet test` commands when tests exist or when the task adds domain, workflow, permission, import, or validation logic.
- When a unit introduces or changes EF Core persistence, prefer `dotnet ef` commands for migration creation and `dotnet ef database update` for applying schema changes during verification when the local environment supports it.
- Verify Clean Architecture dependency rules were not violated.
- Verify mutations enforce authorization and audit rules where applicable.
- Verify state transitions use the approved workflow/state-machine approach.

Frontend verification:

- Run the configured typecheck/build command.
- Run lint if configured.
- Verify imports use the `@/` alias instead of deep `../../..` paths.
- Verify shadcn/ui primitives were not visually overridden ad hoc.
- Verify raw Tailwind palette classes were not introduced for app UI colors.
- Verify server data is handled with TanStack Query, not duplicated in Zustand or React Context.
- Verify URL/filter state uses nuqs where appropriate.
- Verify forms use React Hook Form with Zod when form validation is required.

Full-stack verification:

- Verify frontend behavior matches backend authorization and allowed actions.
- Verify protected actions are enforced server-side, not only hidden in the UI.
- Verify important mutations are auditable when audit logging exists for that module.
- Verify configuration uses environment variables and no hardcoded secrets.

If verification cannot be completed, document the reason in `context/progress-tracker.md` before moving on.

## AI Tooling Rules

- Use the installed shadcn/ui skill when working on frontend UI tasks where available.
- Use installed official .NET skills/plugins when working on backend .NET tasks where available.
- Do not require skill or plugin files to exist inside the project repository if the coding environment exposes them globally.
- Do not install new skills, plugins, or MCP servers as part of a feature implementation unless the project owner explicitly requests it.

## Non-Negotiable Project Rules

- Keep the application single-club for FK Velež Mostar in V1.
- Keep the UI light-only using red, white, and gold club identity.
- Keep players as persistent club entities across team selections.
- Keep team selections configurable and scoped by role/access.
- Keep backend authorization as the source of truth.
- Keep workflow allowed actions backend-owned.
- Keep secrets out of source code.
- Keep `.env` files ignored and commit only `.env.example` files.
- Keep production secrets in real environment variables or a managed secret store.
- Keep media and import file architecture compatible with object storage, even if local development uses a local adapter.
- Keep exact Gpexe and Zone14 mappings pending until real export samples are reviewed.
