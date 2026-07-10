# Unit 24: Staff Users and Roles UI

## Goal

Build the complete frontend administration surface for staff accounts using the backend contracts from Unit 23. Administrators must be able to list staff, create and reissue invitations, copy a one-time setup link, edit profile and access settings, disable/reactivate accounts, and support invited staff through a public password-setup page without adding backend changes, email delivery, public registration, user deletion, login-email recovery, audit history, or unrelated domain features.

## Design

This is a frontend-only feature unit. It replaces the existing `/users` placeholder with a real administrator workflow and adds one narrowly scoped public invitation-acceptance route.

The UI must preserve the established Player Performance Data System direction:

- light-only FK Velež Mostar identity;
- semantic tokens from `frontend/src/index.css` only;
- Bosnian Latin as the default visible UI language;
- desktop-first staff administration with clean tablet and mobile behavior;
- shadcn/ui primitives composed through app-level and feature-level components;
- no raw Tailwind palette classes, hardcoded colors, dark mode, or ad-hoc visual overrides;
- no fake staff, teams, roles, permissions, statuses, or invitation data.

Internal route names, file names, TypeScript identifiers, API fields, enums, and query keys remain in English. Backend enum values must never be rendered directly to users.

### Scope boundary

This unit includes:

- the real `/users` administrator page;
- admin-only frontend route behavior for `/users`;
- permission-aware visibility for the `Korisnici i uloge` navigation item;
- staff list loading, filtering, empty, error, and mutation states;
- invitation creation using the real backend API;
- one-time setup-link display and copy behavior;
- invitation credential reissue with explicit confirmation;
- display-name editing;
- complete role, permission, and team-scope replacement;
- disable/reactivate actions;
- handling self-access changes and self-disable safely;
- a public invitation-acceptance page that sets the invited user's password;
- Bosnian Latin labels for roles, statuses, permissions, and scope types;
- frontend documentation/progress updates.

This unit does not include:

- backend or database changes;
- email delivery;
- public registration;
- administrator-chosen passwords;
- user deletion;
- login-email editing or recovery;
- forgot-password or general reset-password completion;
- manual account unlock;
- audit-log display;
- staff profile photos;
- bulk staff actions;
- pagination for the expected-small staff list;
- player, team-management, match, report, import, media, medical, dashboard, or settings feature expansion;
- localization infrastructure or English translations;
- optimistic updates for security-sensitive staff mutations.

### Routes and access behavior

Use the existing React Router structure.

Required routes:

```txt
/users                 protected administrator staff-management page
/accept-invitation     public invitation setup page
```

`/users` requirements:

- require an authenticated resolved session;
- require `primaryRole === ADMIN` in the frontend before rendering or requesting staff data;
- show the existing loading state while the session is unresolved;
- show a clear access-denied state or safely redirect to `/` when the authenticated user is not an administrator;
- never treat frontend route protection as authorization; Unit 23 backend policies remain authoritative;
- remain subject to Unit 19 required-password-change routing before the administrator page can render.

Navigation requirements:

- show `Korisnici i uloge` only to authenticated administrators;
- do not expose it while session state is unknown;
- keep direct `/users` access guarded even when the navigation item is hidden;
- do not broadly redesign unrelated sidebar groups or permissions in this unit.

`/accept-invitation` requirements:

- remain public and outside the authenticated application shell;
- use the existing auth-page visual shell or a minimal equivalent that matches it;
- never require an existing authenticated session;
- if an authenticated user visits this route, do not silently replace their current account or auto-submit credentials; show a safe instruction to sign out first or provide the existing logout path before setup;
- do not add account lookup or invitation discovery behavior.

### Role labels

Map backend `StaffRole` values to Bosnian Latin labels in one centralized frontend mapping:

| Backend value | Visible label |
| --- | --- |
| `ADMIN` | Administrator |
| `DATA_OPERATOR` | Operater podataka |
| `ANALYST` | Analitičar |
| `COACH` | Trener |
| `MEDICAL_STAFF` | Medicinsko osoblje |
| `VIEWER` | Pregled |

Use the same mapping in filters, tables, dialogs, details, and validation feedback. Do not duplicate role-label switches across components.

### Account-status labels

Map backend statuses centrally:

| Backend value | Visible label |
| --- | --- |
| `INVITED` | Pozvan |
| `ACTIVE` | Aktivan |
| `DISABLED` | Onemogućen |
| `LOCKED` | Zaključan |

Status badges must include readable text and must not rely only on color.

Do not invent an `Otključaj` action. Unit 23 does not provide manual unlock behavior.

### Permission labels

Map the three explicit permission flags centrally:

| Backend field | Visible label |
| --- | --- |
| `canVerifyReports` | Može verifikovati izvještaje |
| `canImportData` | Može importovati podatke |
| `canViewMedicalDetails` | Može pregledati medicinske detalje |

The UI may include short helper descriptions, but it must not imply broader access than the backend guarantees.

For `ADMIN`:

- show that administrator access includes all permissions;
- normalize the form UI to all permissions enabled and team scope `ALL_TEAMS`;
- disable or hide controls that would imply an administrator can have restricted effective access;
- still send a complete explicit request contract accepted by Unit 23;
- rely on the backend as the final normalization and authorization source.

### Team-scope labels and behavior

Map scope types centrally:

| Backend value | Visible label |
| --- | --- |
| `ALL_TEAMS` | Svi timovi |
| `SELECTED_TEAMS` | Odabrane selekcije |

Use the admin teams endpoint from Unit 22 to resolve IDs to stored team names.

Team-option rules:

- load the small team list through TanStack Query;
- request archived records when needed to resolve existing stored scope IDs;
- allow `ACTIVE` and `INACTIVE` teams to be selected for new/updated scope;
- do not allow an `ARCHIVED` team to be newly selected;
- clearly mark archived teams when they appear in an existing staff scope;
- never silently remove an archived stored assignment from the rendered current state;
- if a user has archived selected-team assignments, explain that saving a replacement access configuration requires choosing a valid current selection set;
- `SELECTED_TEAMS` requires at least one unique valid non-archived selected team for a non-admin user;
- `ALL_TEAMS` clears selected IDs in the submitted contract;
- switching role to `ADMIN` forces `ALL_TEAMS` and clears selected IDs in the form state.

Do not translate stored team names. Display them exactly as returned by the backend.

### Staff list layout

Replace the `/users` placeholder with an administrator page following the existing page pattern:

```txt
PageHeader
FilterBar
Staff Table
Dialogs / Confirmation Flows
```

Page header:

- title: `Korisnici i uloge`;
- short operational description;
- primary action: `Pozovi korisnika`.

Recommended filters:

- text search over display name/email;
- role;
- status;
- team-scope type;
- selected team where supported by Unit 23.

Use URL search parameters through `nuqs` when available or install it in this unit if it is not already present. Use stable English parameter names such as:

```txt
q
role
status
scope
team
```

Do not store filters in Zustand or duplicate them in global React Context.

Use TanStack Query for staff/team server state and TanStack Table for table behavior.

Recommended staff columns:

- staff member: display name with email below;
- role;
- account status;
- team access;
- permissions summary;
- row actions.

Keep timestamps out of the main table unless they improve the existing desktop layout without making it dense. They may appear in a detail/edit surface using the project's Bosnian date/time formatting direction.

Use deterministic backend ordering. Do not add frontend-only pagination unless the implemented Unit 23 endpoint already requires it.

Responsive behavior:

- desktop/tablet: semantic table inside a responsive overflow container;
- mobile: preserve essential identification, role, status, scope, and actions without breaking horizontal layout;
- a mobile sheet/detail pattern is acceptable if it composes existing primitives and does not create a second data model;
- action controls must remain keyboard and touch accessible.

### Staff actions

Provide row actions according to account state and backend capabilities.

Common actions:

- `Uredi ime`;
- `Uredi pristup`.

`INVITED` actions:

- reissue invitation credential;
- disable account.

`ACTIVE` actions:

- disable account.

`LOCKED` actions:

- disable account;
- do not show a manual unlock action.

`DISABLED` actions:

- reactivate account.

Do not show delete, reset-password, edit-email, unlock, audit, or impersonation actions.

The frontend may hide unavailable actions for clarity, but backend lifecycle checks and final-active-admin protection remain authoritative.

### Invitation form

Create an invitation dialog or sheet using React Hook Form and Zod.

Fields:

- `displayName`;
- `email`;
- `primaryRole`;
- `canVerifyReports`;
- `canImportData`;
- `canViewMedicalDetails`;
- `teamScopeType`;
- `selectedTeamIds`.

Requirements:

- use the same role/permission/scope form component later reused by access editing where practical;
- validate required fields and selected-team rules before submission;
- send the complete `CreateStaffInvitationRequest` contract;
- do not allow the administrator to enter a password;
- do not auto-generate fake staff values;
- disable duplicate submission;
- preserve entered non-sensitive form data after a recoverable server validation error;
- map safe ProblemDetails validation responses into field errors and the existing form error summary;
- close/reset the form only after confirmed success.

### One-time setup credential display

After successful invitation creation, show a dedicated success dialog containing the one-time setup link.

Construct the absolute link in the browser using the current frontend origin and the public invitation route. Do not hardcode a deployment origin.

The link must include only the backend-required invitation values. Use URL encoding for email and token.

Security and lifecycle requirements:

- treat `setupToken` as sensitive transient data;
- never write it to logs, analytics, localStorage, sessionStorage, Zustand, persistent query cache, or documentation;
- do not include it in normal staff-list query data;
- keep the invitation mutation response uncached beyond the active success surface;
- clear the token and generated link from component state when the success dialog closes or the component unmounts;
- show a clear warning that the link is displayed once and must be sent through a trusted club channel;
- provide `Kopiraj setup link` with temporary `Kopirano` feedback;
- keep a read-only selectable input as a fallback when clipboard access is unavailable;
- do not send email in this unit.

### Invitation reissue

For an `INVITED` user, provide `Ponovo izdaj poziv`.

Before calling the API:

- show an explicit confirmation that the previous link will stop working;
- identify the target by safe display name/email;
- disable duplicate confirmation requests.

After success:

- show the same one-time credential dialog used after invitation creation;
- construct and copy the new link using the returned credential;
- do not retain or display any previous token;
- invalidate/refetch the affected staff query as needed;
- surface lifecycle conflicts safely if the account is no longer `INVITED`.

### Profile editing

Provide a focused `Uredi ime` dialog.

Requirements:

- prefill the current display name;
- update only `displayName` through the Unit 23 profile endpoint;
- do not allow email editing;
- use React Hook Form and Zod;
- map server validation errors safely;
- invalidate/refetch staff list/detail data after success;
- do not optimistically modify security-sensitive cached user data.

### Access editing

Provide a focused `Uredi pristup` dialog or sheet.

Fields:

- `primaryRole`;
- the three explicit permission flags;
- `teamScopeType`;
- `selectedTeamIds`.

Requirements:

- prefill from the complete current `StaffUserResponse`;
- send one complete `ReplaceStaffAccessRequest`;
- never send a partial permissive patch;
- apply administrator normalization in the UI;
- enforce selected-team form rules;
- display archived current assignments honestly;
- show a clear confirmation or warning when changing an existing administrator to a non-admin role;
- rely on the backend for the final-active-admin safeguard;
- display a safe `409` message when the final active administrator cannot be demoted;
- invalidate/refetch staff data after success.

If the target user is the currently signed-in user:

- refetch the session after successful access replacement;
- let the updated session become authoritative before continuing;
- if the user is no longer an administrator, remove access to `/users` and navigate safely to `/` or show the route guard result;
- do not continue rendering stale admin-only controls from the previous session state.

### Disable and reactivate flows

Use destructive confirmation for disable and a clear confirmation for reactivate.

Disable behavior:

- show the target display name/email;
- explain that access will be blocked and existing authenticated access will be invalidated;
- call the dedicated Unit 23 disable endpoint;
- handle idempotent success cleanly;
- show a safe conflict when the action would remove the final active administrator;
- invalidate/refetch staff data after success.

If the administrator disables their own account:

- refetch/clear the session immediately after success;
- navigate to `/sign-in` when the backend session is no longer valid;
- do not falsely leave the application rendered as authenticated.

Reactivate behavior:

- call the dedicated reactivation endpoint;
- display the returned state from refetched staff data;
- when the account returns to `INVITED`, explain that a new invitation must be reissued before setup can continue;
- do not automatically reissue a credential;
- when the account returns to `ACTIVE`, do not auto-sign the target user in.

### Public invitation acceptance

Create a public invitation setup page for the Unit 23 `POST /api/auth/invitations/accept` endpoint.

Expected incoming link values:

```txt
email
token
```

Requirements:

- read and validate that both values are present;
- capture them only in short-lived component memory;
- remove sensitive query values from the visible address bar immediately after capture using safe router/history replacement;
- never persist email/token in browser storage or a global store;
- never log or render the token;
- do not call an account-discovery endpoint;
- show a generic invalid-link state when required values are missing or acceptance fails;
- make clear that expired/replaced/used links require a new invitation from an administrator without revealing backend token-validation details.

Form fields:

- new password;
- confirm password.

The request must send:

- captured email;
- captured token;
- password;
- confirmPassword.

Behavior:

- use React Hook Form and Zod;
- reflect the existing backend password policy through frontend guidance where that policy is already represented in prior auth code;
- never weaken or duplicate the backend as the source of truth;
- disable duplicate submission;
- clear sensitive local state after success;
- do not sign the user in automatically;
- show success copy and a `Nastavi na prijavu` action to `/sign-in`;
- keep error text generic enough not to confirm arbitrary account existence;
- ensure refresh/back behavior does not restore the token through application state after URL sanitization.

### Data-fetching and mutation conventions

Use the API client, ProblemDetails mapping, CSRF helper, and TanStack Query setup from prior units.

Recommended frontend API modules:

```txt
frontend/src/features/users/api/
frontend/src/features/users/hooks/
frontend/src/features/users/components/
frontend/src/features/users/schemas/
frontend/src/features/users/types/
frontend/src/features/users/utils/
```

Use stable query keys for:

- staff list with normalized filters;
- optional staff detail;
- admin teams list.

Mutation rules:

- invitation create/reissue, profile update, access replacement, disable, reactivate, and invitation acceptance must use the real API;
- apply the existing CSRF convention to all unsafe requests where required by the backend;
- include cookie credentials through the shared API client;
- do not place server-owned staff data in Zustand or React Context;
- do not use optimistic updates for role, permission, scope, invitation, or account-status changes;
- invalidate/refetch only relevant queries after success;
- avoid duplicate concurrent mutation requests;
- use safe retry behavior and do not automatically retry lifecycle/security mutations in a way that could issue multiple invitation credentials.

### Error, loading, and empty states

Staff page:

- use the shared loading state while initial data loads;
- use the shared error state with a retry action for recoverable list failures;
- use a clear empty state when no staff records match filters;
- distinguish `Nema korisnika` from `Nema rezultata za odabrane filtere`;
- do not expose stack traces, database details, Identity internals, or raw ProblemDetails payloads.

Mutation errors:

- field validation errors appear next to relevant fields and in the existing error summary where useful;
- `401` refreshes/invalidates session and follows the existing sign-in flow;
- `403` shows access-denied feedback and does not retry automatically;
- `404` reports that the target is no longer available and refreshes the list;
- `409` shows a clear conflict message, especially for duplicate email, invalid lifecycle, or final-active-admin protection;
- `password_change_required` follows the existing Unit 19 redirect behavior;
- unknown failures use safe Bosnian Latin fallback copy.

### Accessibility

- every dialog/sheet has a clear title and description;
- labels are programmatically associated with fields;
- role, status, scope, and permission information is readable without color;
- action menus and confirmation dialogs are keyboard accessible;
- focus moves to the first invalid field or error summary after validation failure where practical;
- sensitive setup-link dialog clearly announces one-time behavior;
- copy feedback is accessible and not conveyed only visually;
- loading and pending states prevent duplicate actions without trapping focus;
- table markup remains semantic when TanStack Table is used;
- public setup form supports keyboard submission and appropriate password autocomplete values.

## Implementation

### 1. Required reading and existing-pattern review

Before changing code:

1. Read root `AGENTS.md`.
2. Read the six context files in their required order.
3. Read `context/feature-specs/00-build-plan.md`.
4. Read this feature spec completely.
5. Review Unit 03 routing and sidebar patterns.
6. Review Unit 05 common loading/error/empty/page-header components.
7. Review Unit 10 API client, ProblemDetails, environment, and TanStack Query patterns.
8. Review Unit 11 form/Zod/error-summary patterns.
9. Review Units 16 and 19 session, protected-route, CSRF, logout, and auth-shell behavior.
10. Review Unit 20 role/permission/session contracts.
11. Review Unit 22 team list contract and lifecycle values.
12. Review the implemented Unit 23 endpoint paths, exact request/response shapes, error status conventions, invitation credential response, and session extension before finalizing frontend types.
13. Use relevant installed frontend skills in `frontend/.agents/` when applicable without allowing them to override project context or this spec.

Do not refactor unrelated frontend foundations unless a small change is required to integrate this feature cleanly.

### 2. Add only required frontend dependencies and primitives

Inspect current dependencies before installing anything.

Add only when missing:

- `@tanstack/react-table` for staff table behavior;
- `nuqs` for URL filter state.

Add missing shadcn/ui primitives through the existing shadcn CLI rather than writing replacements manually. Likely primitives include:

- Table;
- Badge;
- Select;
- Checkbox;
- Dropdown Menu;
- Alert Dialog;
- Sheet or Dialog as required;
- Scroll Area where needed.

Do not modify generated `frontend/src/components/ui/*` files after generation unless the active task explicitly requires a generated integration fix. App-specific styling and behavior must live in feature/app-level components.

Do not add another table, form, validation, query, state, clipboard, or multi-select package.

### 3. Create the users feature contracts and mappings

Create strict frontend interfaces/types aligned with the implemented Unit 23 API:

- `StaffRole`;
- `StaffAccountStatus`;
- `StaffPermissionSet`;
- `TeamScopeType`;
- `StaffTeamScope`;
- `StaffUserResponse`;
- `StaffInvitationCredentialResponse`;
- request contracts for invitation, profile update, access replacement, disable/reactivate, reissue, and invitation acceptance;
- staff-list filter types.

Use centralized mappings/utilities for:

- role labels;
- status labels;
- permission labels;
- scope labels;
- status-badge variants through app-level semantic components;
- safe team-name resolution;
- date/time display if timestamps are shown.

Do not use `any`. Validate untrusted URL/query values before using them in typed filters or invitation setup.

### 4. Create staff and teams API wrappers

Add focused API functions using the shared client.

Required operations:

- list staff;
- optionally get staff detail if the UI uses it;
- create invitation;
- reissue invitation;
- update display name;
- replace access;
- disable;
- reactivate;
- accept invitation;
- list teams with the parameters required for scope resolution.

Keep endpoint paths aligned with the implemented backend. Do not add fallback calls to invented routes.

Ensure unsafe operations use the approved CSRF behavior and are not automatically retried in a way that can duplicate one-time credential issuance.

### 5. Create query and mutation hooks

Create feature-owned TanStack Query hooks for the required reads and mutations.

Requirements:

- normalized stable query keys;
- URL filters included in the staff-list query key;
- team query shared across invitation/access forms;
- relevant invalidation after mutation success;
- no staff server-state duplication in Zustand/Context;
- no optimistic security-sensitive mutation state;
- invitation credentials remain mutation-local and are cleared deliberately;
- self-targeted access/status changes trigger session refetch/invalidation.

### 6. Add administrator route and navigation behavior

Replace the `/users` placeholder route with the real page.

Add a small reusable admin-route guard only if it cleanly extends the existing Unit 19 session route behavior. Do not introduce a second auth/session system.

Update sidebar visibility for `Korisnici i uloge` based on resolved session role.

Requirements:

- no staff request before admin access is confirmed;
- non-admin direct navigation fails safely;
- no protected-content flash;
- required-password-change behavior still wins before normal route rendering;
- backend `401`/`403` remains authoritative.

### 7. Build staff list and filters

Build the page header, URL-backed filters, table, and states described in Design.

Use TanStack Table for behavior and shadcn table primitives for semantic rendering.

Do not introduce client-only fake sorting or columns unsupported by the backend contract. If sorting is not exposed by Unit 23 and deterministic server ordering is sufficient, keep sorting controls out of scope.

### 8. Build invitation and one-time credential flows

Implement:

- invitation form;
- successful one-time setup-link dialog;
- copy feedback;
- invitation reissue confirmation;
- reissue success credential dialog.

Keep sensitive credential data transient and uncached as defined in Design.

### 9. Build profile and access editing

Implement separate focused profile and access forms.

Reuse the role/permission/scope form section between invitation and access editing where doing so keeps validation and behavior consistent without creating an over-general form framework.

Handle self-access changes through session refetch and route reevaluation.

### 10. Build lifecycle actions

Implement disable/reactivate confirmations and mutation behavior.

Handle:

- final-active-admin conflicts;
- invited-account reactivation returning to `INVITED`;
- active completed-account reactivation returning to `ACTIVE`;
- self-disable session invalidation;
- stale row status after concurrent changes through refetch rather than optimistic assumptions.

### 11. Build public invitation setup page

Add `/accept-invitation` outside the authenticated app shell.

Implement:

- sensitive URL-value capture and sanitization;
- password/confirmation form;
- real invitation-acceptance API call;
- generic invalid/expired/used-link behavior;
- success state with sign-in navigation;
- no automatic login;
- no account discovery;
- no credential persistence.

### 12. Integrate error and accessibility behavior

Use existing shared form and page states. Add feature-level safe Bosnian Latin messages for staff and invitation workflows.

Review keyboard behavior, focus, semantic tables, accessible action names, confirmation text, copy feedback, and mobile interaction.

### 13. Documentation updates

Update `context/progress-tracker.md` during implementation:

- mark Unit 24 in progress when work starts;
- mark it complete only after all verification passes;
- record the final frontend route paths;
- record the final staff filters and URL parameter names;
- record the invitation-link construction and transient credential handling;
- record admin route/sidebar behavior;
- record self-access/self-disable session handling;
- record exact shadcn primitives and npm packages added;
- note that email delivery, user deletion, login-email recovery, manual unlock, audit UI, localization infrastructure, and unrelated modules remain deferred;
- document any API contract mismatch or verification limitation honestly.

Update `context/architecture.md`, `context/ui-context.md`, or `context/code-standards.md` only if implementation changes a project-level decision. Do not duplicate feature-level implementation detail into context files unnecessarily.

## Dependencies

Install only if not already present:

- `@tanstack/react-table` — headless staff table behavior;
- `nuqs` — URL-backed staff filter state.

Add required shadcn/ui primitives through the existing shadcn CLI only when missing.

Do not add another server-state library, form library, validation library, global state library, table package, clipboard package, multi-select package, date library, email SDK, authentication SDK, or localization package.

## Verification checklist

- [ ] `AGENTS.md`, all required context files, the current build plan, relevant prior frontend/auth/authorization/team/staff specs, and this feature spec were read before implementation.
- [ ] Relevant frontend skills from `frontend/.agents/` were used when applicable without overriding project context or this spec.
- [ ] No backend, database, migration, email-provider, public-registration, user-deletion, login-email-recovery, audit, or unrelated domain implementation was added.
- [ ] The existing `/users` placeholder is replaced by a real staff-administration page.
- [ ] `/users` waits for resolved session state and renders only for `ADMIN` users.
- [ ] Non-admin direct access to `/users` fails safely without requesting or exposing staff data.
- [ ] `Korisnici i uloge` navigation is shown only to resolved administrator sessions.
- [ ] Backend authorization remains authoritative for every staff request.
- [ ] The public `/accept-invitation` route exists outside the authenticated app shell.
- [ ] Role, status, permission, and scope labels are centralized and displayed in Bosnian Latin.
- [ ] Raw backend enum values are not shown directly in the UI.
- [ ] Staff server state uses TanStack Query and is not duplicated in Zustand or React Context.
- [ ] Staff filters use URL search parameters through `nuqs` and stable English parameter names.
- [ ] TanStack Table owns staff table behavior and shadcn table primitives render semantic markup.
- [ ] Loading, empty, filtered-empty, error, and retry states are implemented without fake data.
- [ ] The staff table shows display name/email, role, status, team scope, permission summary, and accessible actions.
- [ ] Invitation creation uses React Hook Form and Zod and sends the exact Unit 23 request contract.
- [ ] Administrators cannot set an invited user's password.
- [ ] `ADMIN` form behavior normalizes effective permissions and `ALL_TEAMS` scope.
- [ ] Non-admin `SELECTED_TEAMS` requires at least one valid unique non-archived selected team.
- [ ] `ALL_TEAMS` submissions clear selected-team IDs.
- [ ] Active and inactive teams can be selected; archived teams cannot be newly selected.
- [ ] Existing archived scope assignments are displayed honestly and are not silently hidden or removed.
- [ ] Invitation success displays a setup link built from the current frontend origin without a hardcoded deployment origin.
- [ ] `setupToken` is never logged, persisted, cached as normal query data, written to browser storage, or left in component state after the credential dialog closes.
- [ ] Setup-link copy behavior provides accessible success feedback and a manual read-only fallback.
- [ ] Reissuing an invitation requires confirmation that the previous link becomes invalid.
- [ ] Only `INVITED` users are offered invitation reissue behavior.
- [ ] Display-name editing cannot change email or access settings.
- [ ] Access editing sends one complete atomic access contract rather than a permissive partial patch.
- [ ] Demoting/disabling the final active administrator surfaces the backend conflict safely.
- [ ] Changing the current user's own role/access refetches the session before continued navigation.
- [ ] Disabling the current user's own account invalidates/refetches session state and returns to sign-in safely.
- [ ] No delete, unlock, impersonation, edit-email, reset-password, or audit action is shown.
- [ ] Reactivating an incomplete account can return it to `INVITED` without automatically reissuing a setup credential.
- [ ] Reactivating a completed account can return it to `ACTIVE` without auto-signing in the target user.
- [ ] Invitation acceptance captures email/token only in short-lived memory and sanitizes sensitive query values from the address bar.
- [ ] Invitation token values are never rendered, logged, stored, or placed in global state.
- [ ] Invitation acceptance uses the real Unit 23 endpoint with password and confirmation validation.
- [ ] Invalid, expired, replaced, reused, or malformed invitation links show a generic safe state without account discovery.
- [ ] Successful invitation acceptance does not auto-sign in and provides `Nastavi na prijavu`.
- [ ] Unsafe staff and invitation mutations use the approved CSRF and cookie-credential behavior.
- [ ] Security-sensitive mutations are not optimistically updated or automatically retried in a way that duplicates effects.
- [ ] `401`, `403`, `404`, validation, `409`, `password_change_required`, and unknown error cases follow existing safe frontend patterns.
- [ ] Generated shadcn/ui primitive files were not manually visually customized after generation.
- [ ] No raw Tailwind palette classes, hardcoded color values, dark-mode behavior, deep relative imports, or ad-hoc shadcn visual overrides were introduced.
- [ ] Forms, action menus, tables, dialogs, copy feedback, focus states, and pending states are keyboard accessible and readable without relying only on color.
- [ ] Desktop, tablet, and mobile layouts remain usable.
- [ ] `npm run format` completes for the frontend workspace when changes require formatting.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] `npm run build` passes.
- [ ] Manual verification confirms: admin sign-in → open staff page → invite user → copy setup link → accept invitation → sign in as invited user.
- [ ] Manual verification confirms: edit display name → replace role/permissions/scope → disable → reactivate → reissue invitation when applicable.
- [ ] Manual verification confirms a non-admin cannot access staff data through navigation or direct route and receives backend `403` if a protected request is attempted.
- [ ] `context/progress-tracker.md` reflects the actual Unit 24 implementation, dependency additions, verification results, and deferred work.
