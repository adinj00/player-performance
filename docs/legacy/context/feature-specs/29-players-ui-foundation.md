# Unit 29: Players UI Foundation

## Goal

Build the first complete Players frontend module using the real player and assignment APIs from Units 27 and 28. Provide a team-scope-aware players list, administrator-only create/edit/lifecycle and assignment actions, and a player detail shell that shows the player's persistent club identity and full selection assignment history without duplicating current-team state on the player record.

## Design

### Required reading and implementation boundaries

Before implementation, read:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/ui-context.md`
5. `context/code-standards.md`
6. `context/ai-workflow-rules.md`
7. `context/progress-tracker.md`
8. `context/references/shadcn-components.md`
9. `context/feature-specs/00-build-plan.md`
10. `context/feature-specs/29-players-ui-foundation.md`

Use relevant project-local skills from `.agents/skills/` when applicable. Check the shadcn component reference before creating custom UI primitives or interaction patterns. Skills and component references may guide implementation but must not override project context or this spec.

This unit is frontend-only. Use the existing backend contracts from Units 27 and 28. Do not add or change backend endpoints, persistence, authorization rules, player fields, assignment invariants, or migrations.

### UI language and visual rules

All new visible UI copy must be Bosnian Latin by default and use proper Bosnian characters.

Follow the existing light-only FK Velež visual system:

- use semantic design tokens only;
- use existing shadcn/ui primitives and documented variants;
- do not introduce raw Tailwind palette classes or hardcoded colors;
- do not visually override generated shadcn primitives ad hoc;
- use app-level components only when they compose project-specific behavior;
- install missing shadcn components just in time through the approved shadcn workflow.

### Players navigation and routes

Use the existing application routing conventions.

Required routes:

```txt
/players
/players/:playerId
```

Required behavior:

- the existing sidebar `Players` entry navigates to `/players`;
- `/players` shows the players list available to the current authenticated user;
- `/players/:playerId` shows the player detail shell when the backend grants access;
- inaccessible or missing player detail responses use the existing safe error/not-found pattern and must not expose restricted player information;
- direct route refreshes must work;
- protected-route and required-password-change behavior from the existing auth foundation remains unchanged.

Do not create nested edit routes in this unit. Use focused dialogs for create/edit/lifecycle/assignment actions.

### Authorization-aware UI behavior

Backend authorization remains the source of truth.

The UI must derive administrative action visibility from the current session/access data already available in the application.

Required behavior:

- all authenticated active staff who can access `/players` may see the player records returned by the backend;
- non-admin users must not see create, edit, lifecycle, or assignment mutation actions;
- admin users may see and use player mutation actions;
- the frontend must not attempt to reconstruct team-scope rules itself beyond rendering what the backend returns;
- mutation failures such as `401`, `403`, `404`, validation errors, and `409 Conflict` must be handled safely even when an action was visible;
- hiding an action in the UI is convenience only and must never be treated as authorization.

### Player list page

Build the `/players` page as a real data-driven module using TanStack Query.

Recommended page structure:

```txt
PageHeader
FilterBar
Players table
Pagination
```

The page header should include:

- title: `Igrači`;
- concise Bosnian Latin description;
- admin-only primary action: `Novi igrač`.

Do not wrap the whole page in a decorative card if the existing page pattern does not require it.

### Player list filters and URL state

Use `nuqs` for shareable list state according to existing project conventions.

Support the backend query capabilities from Unit 27/28, including at minimum:

- free-text search;
- player lifecycle status filter;
- team/selection filter;
- page/pagination state.

Rules:

- debounce free-text search by approximately 300 ms because it triggers server queries and URL updates;
- do not debounce discrete status or team filters;
- reset page to the first page when search/status/team filters change;
- omit default/empty values from the URL when practical;
- do not fetch a separate client-side copy of all players and filter it locally when the backend supports server-side filtering;
- the team filter must use real teams/selections data already available through the existing settings/team APIs;
- only show selectable teams that the current user can meaningfully query according to the established backend/API behavior; do not invent broader client-side access.

### Players table

Use TanStack Table for table behavior and shadcn/ui table primitives for rendering.

The table should remain readable on desktop/tablet and use a responsive overflow wrapper where needed.

Display at minimum:

- player name;
- lifecycle status;
- current selection membership summary derived from assignment data returned by the API or an existing list read model;
- date of birth when available;
- row action affordance for admins.

Do not add speculative columns such as:

- position;
- shirt number;
- nationality;
- dominant foot;
- medical status;
- goals or match statistics.

Current team membership must be displayed as a derived assignment summary. Do not add or infer a persisted `currentTeamId` field.

When a player has multiple current assignments, display all relevant current selections in a compact, readable way using existing badge/item patterns rather than arbitrarily picking one as the single current team.

### Player list states

Provide explicit states:

- loading: use existing skeleton/loading components and suitable shadcn primitives;
- empty dataset: clear Bosnian Latin empty state with admin create action when allowed;
- empty filtered result: distinguish from a globally empty dataset and offer a filter reset action;
- error: use the existing error-state pattern with retry where appropriate.

Before creating a new empty/loading/error primitive, reuse the app-level components introduced earlier and check `context/references/shadcn-components.md` for a suitable shadcn primitive.

### Create player dialog

Admin-only.

Use an existing shadcn `Dialog` and the established React Hook Form + Zod form foundation.

The form must expose only fields supported by Unit 27:

- first name;
- last name;
- optional preferred name;
- optional date of birth.

Use an appropriate shadcn date input/picker pattern if one already exists or can be added just in time. Do not create a custom calendar interaction when a suitable shadcn component exists.

Required behavior:

- client validation mirrors obvious backend constraints for UX but does not replace backend validation;
- submit calls the real create-player API;
- display field-level and safe form-level errors;
- prevent duplicate submission while pending;
- on success, close/reset the dialog and invalidate/refetch the relevant player queries;
- if the API returns the created player identifier, navigating to the detail page is allowed only if it follows the existing product flow cleanly; otherwise remain on the list and show success feedback through the project's established feedback pattern.

Do not add team assignment fields to the create-player dialog. Player identity and team assignments remain separate operations.

### Edit player dialog

Admin-only.

Use the same supported identity/profile fields as the create form.

Required behavior:

- prefill with current player data;
- preserve optional empty values correctly;
- submit only through the existing Unit 27 update API contract;
- show safe validation/conflict errors;
- invalidate both list and detail queries on success;
- do not allow lifecycle status mutation inside the generic edit form unless Unit 27's API contract explicitly makes that the canonical flow.

Lifecycle actions must use explicit actions described below.

### Player lifecycle actions

Admin-only.

Expose explicit actions according to the player's current lifecycle status and the Unit 27/28 backend contracts:

- deactivate when allowed;
- activate when allowed;
- archive when allowed;
- restore when allowed.

Use existing shadcn components appropriately:

- use `Alert Dialog` for destructive or consequential confirmation actions such as archive;
- use `Dropdown Menu` or another suitable existing action primitive for compact row/detail actions;
- use `Badge` for status display where it matches the existing app pattern.

Do not create custom modal/menu primitives when shadcn already provides the interaction.

Required behavior:

- lifecycle action visibility follows the known current status;
- backend remains authoritative for whether the transition is actually allowed;
- `409 Conflict` caused by current assignments must show a clear Bosnian Latin explanation that current assignments must be ended first;
- successful mutations invalidate list and detail queries;
- restored players must display the backend-returned `INACTIVE` state rather than assuming `ACTIVE`.

Do not hard-delete players.

### Player detail shell

Build `/players/:playerId` as a real detail view.

The first version should include:

```txt
Player detail
├── Player header
├── Current status and current selection summary
├── Overview section
└── Assignment history section
```

The header should show:

- display name;
- lifecycle status;
- current selection badges/summary derived from current assignments;
- admin-only action menu.

The overview section should show only data currently supported by the backend:

- full name;
- preferred name when present;
- date of birth when present;
- lifecycle status;
- current selection membership summary.

Use clear fallback text for optional values.

Do not add empty fake tabs for match statistics, medical data, performance, physical metrics, notes, media, or availability. Those areas will be added by later units when their data exists.

### Assignment history display

Load the real assignment history from Unit 28 for an accessible player.

Display every assignment returned by the backend, including historical references to inactive or archived teams.

Each assignment should show at minimum:

- team/selection name;
- start date;
- end date or a clear ongoing value;
- derived timing state (`UPCOMING`, `CURRENT`, `PAST`) as localized display text.

Sort/render according to the backend response order. Do not duplicate or re-sort with conflicting client business logic unless required by the table/list component for presentation only.

Use a compact timeline/list/table pattern that works with dense sports operations UI. Check existing shadcn components such as `Item`, `Badge`, `Table`, `Accordion`, or other suitable primitives before creating a bespoke pattern.

The history must make the player's development path visible, including cases such as:

```txt
U17 → U19 → First Team
```

but must also support:

- overlapping current assignments to different selections;
- return to a previous selection after a historical gap;
- future assignments;
- archived/inactive team names preserved as history.

Do not flatten history into one `currentTeam` field.

### Assignment management UI

Admin-only.

Provide focused UI for creating and ending assignments using the Unit 28 API.

#### Create assignment

Use a shadcn `Dialog` with React Hook Form + Zod.

Fields:

- team/selection;
- start date;
- optional end date.

Rules:

- load real selectable teams from the existing team/settings API;
- do not allow the user to choose teams that the backend marks unavailable for new assignments if the API exposes that distinction;
- if the frontend cannot know a lifecycle restriction reliably, submit and render the backend error rather than duplicating hidden business rules;
- date validation should catch obvious invalid ranges client-side;
- the backend remains authoritative for overlap checks and player/team lifecycle rules;
- display overlap and conflict errors clearly in Bosnian Latin;
- do not automatically end assignments in other teams;
- on success, invalidate player detail, assignment history, and any affected player list queries.

#### End assignment

Only show the action for open-ended/current or otherwise backend-endable assignments according to the Unit 28 contract.

Use a confirmation/dialog flow with an end-date field.

Rules:

- require an end date;
- validate end date against the assignment start date for immediate UX feedback;
- submit through the explicit Unit 28 end-assignment endpoint;
- render safe `409` conflict errors;
- do not implement reopen, arbitrary edit, delete, or direct team-change actions;
- on success, invalidate assignment history, player detail, and player list queries.

### Data fetching and cache ownership

Use TanStack Query for all server state.

Create feature-owned API/query modules under the existing `frontend/src/features/players/` structure.

At minimum, define stable query keys for:

- paginated/filterable players list;
- player detail by id;
- player assignment history by player id;
- teams/selections used by player filters and assignment forms, reusing an existing canonical team query when available rather than duplicating cache ownership.

Mutation success must invalidate only the relevant canonical queries.

Do not copy server-owned player or assignment data into Zustand or React Context.

### Feature organization

Keep player-specific frontend code inside the existing feature-based structure, for example:

```txt
frontend/src/features/players/
├── api/
├── components/
├── hooks/
├── schemas/
├── types/
├── utils/
└── index.ts
```

Exact file names may follow repository conventions, but responsibilities must remain clear.

Do not move generic app-shell components into the players feature.

### Out of scope

Do not implement in this unit:

- backend changes or migrations;
- player positions;
- shirt numbers;
- nationality or dominant-foot fields;
- player images;
- medical or availability data;
- notes;
- match appearances;
- match statistics;
- GPS/physical metrics;
- media;
- audit UI;
- import workflows;
- assignment deletion, reopen, arbitrary edit, or direct team-change APIs;
- frontend-generated authorization decisions that replace backend checks;
- localization infrastructure beyond the existing Bosnian Latin default copy rules.

## Implementation

### 1. Read current contracts before coding

Inspect the actual Unit 27 and Unit 28 backend request/response contracts and route shapes before writing frontend API types.

Do not guess field names or response envelopes from this spec when the implemented backend differs in naming while preserving the documented behavior.

Align frontend types with the real API contracts.

### 2. Check shadcn coverage before custom UI work

Read `context/references/shadcn-components.md` and inspect existing generated primitives in `frontend/src/components/ui/`.

Reuse or add through the shadcn CLI, as needed, suitable primitives for:

- dialogs;
- alert dialogs;
- badges;
- dropdown menus;
- selects/comboboxes;
- date input/picker behavior;
- tables/data tables;
- skeleton/loading feedback;
- empty states;
- feedback/toast behavior.

Do not install the entire catalog. Add only components required by this unit and do not modify generated primitive files unless explicitly required by the project rules.

### 3. Add player feature API types and client functions

Create typed frontend contracts and API helpers for the real Unit 27/28 endpoints, including:

- list players;
- get player detail;
- create player;
- update player;
- player lifecycle actions;
- list assignments;
- create assignment;
- end assignment.

Use the shared API client and ProblemDetails error handling foundation.

Do not place fetch logic directly inside presentational components.

### 4. Add query keys and TanStack Query hooks

Create stable feature-owned query keys and hooks for player data.

Ensure:

- filters are part of the list query key;
- player id is part of detail/assignment query keys;
- mutations invalidate the minimum correct query set;
- errors remain available to the UI for safe user feedback.

### 5. Build `/players` list page

Replace any placeholder route content with the real players module.

Add:

- page header;
- admin-only create action;
- search/status/team filters;
- server-driven table;
- pagination;
- loading/empty/error states;
- admin-only row actions.

Keep URL state in `nuqs` and server state in TanStack Query.

### 6. Build player create and edit forms

Use React Hook Form + Zod and existing form primitives.

Keep schemas close to the players feature.

Handle:

- field validation;
- backend validation errors;
- pending state;
- dialog close/reset behavior;
- query invalidation on success.

Do not include assignment fields in identity/profile forms.

### 7. Build lifecycle action flows

Add explicit activate/deactivate/archive/restore actions according to real backend routes and current status.

Use appropriate confirmation UI for consequential actions.

Render current-assignment conflicts clearly and keep the dialog/action state recoverable after a failed mutation.

### 8. Build player detail route

Load the accessible player record and assignment history.

Render:

- player identity header;
- lifecycle status;
- current assignment summary;
- overview details;
- assignment history;
- admin-only actions.

Use the existing page shell and shared common components.

### 9. Build assignment create/end flows

Add admin-only assignment management using the real Unit 28 contracts.

Ensure assignment mutations:

- do not invent automatic cross-team behavior;
- surface overlap/lifecycle conflicts from the backend;
- invalidate all affected player read models after success.

### 10. Keep documentation synchronized

Update `context/progress-tracker.md` after meaningful implementation changes and when Unit 29 is completed.

Update another context file only if implementation requires a genuine project-level decision change. Do not rewrite project context merely to describe routine implementation details.

## Dependencies

No new general-purpose npm library is required beyond the frontend stack already approved and introduced by previous units.

Add only missing shadcn/ui components required by this unit through the approved shadcn CLI workflow. Do not install the full component catalog.

## Verification checklist

- [ ] `AGENTS.md`, all required context files, `context/references/shadcn-components.md`, the build plan, and this spec were read before implementation.
- [ ] Relevant frontend Codex skills were used when applicable without overriding project context.
- [ ] Existing shadcn primitives and the component reference were checked before custom UI primitives were created.
- [ ] Only required missing shadcn components were added just in time.
- [ ] `/players` uses the real backend API and no mock player data.
- [ ] `/players/:playerId` uses the real backend player and assignment APIs.
- [ ] Players list respects backend team-scope filtering and does not reproduce authorization logic client-side.
- [ ] Non-admin users do not see player or assignment mutation actions.
- [ ] Admin users can create and edit supported player profile fields.
- [ ] Player lifecycle actions use explicit backend endpoints and no player is hard-deleted.
- [ ] `409 Conflict` caused by current assignments is shown with a clear Bosnian Latin message.
- [ ] Search, status, team, and pagination state follow the approved URL-state rules.
- [ ] Free-text search is debounced by approximately 300 ms; discrete filters are not debounced.
- [ ] TanStack Query owns all player, assignment, and related server state.
- [ ] No server-owned player or assignment data was duplicated in Zustand or React Context.
- [ ] Player list shows multiple current selections when applicable and does not invent a single persisted current team.
- [ ] Player detail displays full assignment history, including historical, current, future, inactive-team, and archived-team references returned by the backend.
- [ ] Admin users can create historical/current/future assignments according to the backend contract.
- [ ] Admin users can end eligible assignments through the explicit end action.
- [ ] No assignment delete, reopen, arbitrary edit, or direct team-change UI was added.
- [ ] Visible UI copy is Bosnian Latin with proper Bosnian characters.
- [ ] No raw Tailwind palette classes or hardcoded component colors were introduced.
- [ ] Generated shadcn/ui primitive files were not modified unless explicitly required and documented.
- [ ] Frontend imports use the `@/` alias and avoid deep relative paths.
- [ ] `npm run format` completes successfully in `frontend/`.
- [ ] `npm run format:check` passes in `frontend/`.
- [ ] `npm run lint` passes in `frontend/`.
- [ ] `npm run build` passes in `frontend/`.
- [ ] Existing backend remains unchanged by this frontend-only unit.
- [ ] Manual verification covers admin and non-admin behavior, list filters, detail access, player mutations, assignment creation, assignment ending, loading/empty/error states, and responsive desktop/tablet/mobile rendering.
- [ ] `context/progress-tracker.md` reflects the actual completed Unit 29 implementation state.
