# Unit 23: Staff Users, Team Scope, and Account Lifecycle Backend

## Goal

Add the backend staff-administration module for the closed FK Velež internal system. Administrators must be able to create invited staff accounts, issue and reissue one-time setup credentials, list staff, update display names and access settings, assign all-team or selected-team scope, disable/reactivate accounts, and rely on reusable backend team-access checks without adding frontend UI, public registration, email delivery, login-email recovery, or audit persistence.

## Design

This is a backend-only user-management and authorization unit. It extends the Identity, authentication, staff-role, permission, settings, and team foundations implemented in Units 14–22.

The V1 access model remains:

- one primary role per staff user;
- explicit permission flags;
- either all-team access or selected-team access;
- backend authorization as the source of truth;
- no public registration;
- no player login;
- no multi-club tenancy.

This unit must use the existing Identity user and the authoritative `StaffAccessProfile` introduced earlier. Extend those models only where required; do not create a second competing user, role, permission, or staff-profile system.

### Scope boundary

In scope:

- staff list and staff detail queries for administrators;
- administrator-created invitations;
- one-time invitation/setup token issuance and reissuance;
- anonymous invitation acceptance that sets the staff member's password;
- display-name maintenance;
- primary-role replacement;
- explicit-permission replacement;
- `ALL_TEAMS` and `SELECTED_TEAMS` scope persistence;
- relational selected-team assignments linked to Unit 22 team records;
- reusable current-user team-access resolution;
- account disable and reactivate behavior;
- session-contract enrichment with safe team-scope data;
- EF Core migration and focused backend tests.

Out of scope:

- frontend staff-management or setup screens;
- public registration;
- automatic email sending or an email-provider integration;
- forgot-password and general password-reset flows;
- login-email changes or audited email recovery;
- physical deletion of staff accounts;
- manual account unlock UI or a new lockout workflow;
- multiple roles per user;
- custom per-feature permission creation;
- audit-log tables or audit-history UI;
- player, match, report, import, media, or medical functionality.

Unit 38 will add the audit foundation and connect audit coverage to critical staff mutations. This unit must keep commands explicit and actor-aware so later audit integration does not require moving business logic into endpoints.

### Canonical team-scope model

Define the V1 team-scope enum:

```txt
TeamScopeType
- ALL_TEAMS
- SELECTED_TEAMS
```

Rules:

- `ALL_TEAMS` grants access to all existing and future FK Velež selections, subject to the user's role, permission flags, feature rules, and account status.
- `SELECTED_TEAMS` grants access only to explicitly assigned team IDs.
- A non-admin `SELECTED_TEAMS` scope must contain at least one team.
- A selected-team assignment must reference an existing non-archived Unit 22 team when created or replaced.
- Inactive teams remain assignable because historical and preparatory workflows may still require access.
- Existing assignments are not automatically removed when a team is archived; preserving the relationship supports historical data access. Feature-specific queries decide whether archived teams are visible or actionable.
- Switching to `ALL_TEAMS` removes all selected-team assignment rows.
- Replacing a `SELECTED_TEAMS` scope atomically replaces the complete selected-team set.
- Duplicate selected-team IDs are invalid.
- Client-submitted team IDs must never be trusted without database validation.

`ADMIN` behavior:

- `ADMIN` has effective `ALL_TEAMS` access regardless of selected-team rows or stored permission flags.
- Persist an admin profile in the canonical normalized form: `ALL_TEAMS`, no selected-team rows, and no latent explicit permission grants required for effective access.
- Changing a staff member to `ADMIN` clears selected-team assignments and normalizes the stored scope to `ALL_TEAMS`.
- Changing a staff member from `ADMIN` to a non-admin role requires an explicit valid scope and explicit permission values in the same access-replacement operation.

Do not store team IDs in JSON, comma-separated strings, claims, browser tokens, or unvalidated Identity fields.

### Staff profile and Identity ownership

Continue using the existing separation:

- Identity owns email, normalized email/user name, password hash, security stamp, lockout data, and authentication internals.
- The authoritative staff access/profile record owns business-facing staff data such as display name, primary role, explicit permissions, team-scope type, and access timestamps where applicable.
- The relational team-scope join owns selected-team assignments.

Add a required staff display name to the existing authoritative staff profile if it does not already exist. Do not create a second profile table only for the name.

Do not expose `ApplicationUser`, password hashes, security stamps, concurrency stamps, lockout internals, token data, normalized email fields, or persistence navigation objects through API contracts.

### Invitation and account setup model

Administrators create staff accounts; users cannot register themselves.

Invitation creation must:

1. validate the request and administrator authorization;
2. normalize and validate the email through the approved Identity conventions;
3. create an Identity user without a password;
4. set account status to `INVITED`;
5. create the authoritative staff profile and selected-team assignments atomically;
6. generate a one-time, URL-safe setup token through the existing ASP.NET Core Identity token-provider infrastructure;
7. return the setup credential only in the successful invitation response.

The setup token:

- must not be stored in plaintext;
- must not be logged;
- must not appear in staff list/detail responses;
- must be safe to transport as a URL/query value after proper encoding;
- must expire through an explicit, configuration-driven token lifetime;
- must become invalid after successful setup;
- must become invalid when a replacement invitation is issued.

Use a dedicated named Identity token purpose/provider where practical so invitation setup semantics are not accidentally coupled to unrelated future password-reset behavior. Configure a documented invitation-token lifetime. Use a conservative default of 72 hours unless the existing project configuration already defines an approved equivalent. Add only non-secret configuration documentation to `backend/.env.example` when needed.

No email provider is introduced. The API returns the one-time setup credential to the authenticated administrator so Unit 24 can construct and copy a setup link for delivery through a trusted club channel. Do not hardcode a frontend origin in backend source code.

Invitation acceptance must:

- be available without an authenticated cookie;
- accept the invited email/user identifier, URL-safe token, new password, and password confirmation;
- return a generic safe error for invalid, expired, replaced, already-used, or mismatched credentials;
- enforce the existing Identity password policy;
- set the password through approved Identity APIs rather than writing a hash directly;
- mark the account `ACTIVE` after successful setup;
- clear `mustChangePassword` because the invited user chose their own password;
- mark email confirmation consistently with the closed trusted invitation flow if the existing Identity setup uses confirmation;
- rotate security state so the token cannot be reused;
- not automatically sign the user in;
- allow the user to sign in normally after setup.

Because invitation acceptance does not rely on an existing authenticated cookie, it is not a cookie-authenticated CSRF target. It must still use strict input validation, safe error responses, and the established API/CORS/security pipeline.

### Invitation reissue

Administrators must be able to reissue setup credentials for an account that is still `INVITED`.

Reissue behavior:

- require `AdminOnly`;
- reject accounts that are not currently `INVITED`;
- invalidate previously issued setup tokens before generating a new token;
- preserve the user's display name, role, permissions, and team scope;
- return the new setup credential once;
- never expose old tokens;
- remain safe to retry only by deliberately issuing a new token each time.

Do not reissue setup credentials for `ACTIVE`, `DISABLED`, or `LOCKED` accounts.

### Account lifecycle

Use the account statuses introduced in Unit 14:

```txt
INVITED
ACTIVE
DISABLED
LOCKED
```

Required administration behavior:

```txt
INVITED -> DISABLED
ACTIVE  -> DISABLED
LOCKED  -> DISABLED
DISABLED -> INVITED   when no password/setup exists
DISABLED -> ACTIVE    when setup/password already exists
```

Rules:

- Disable is idempotent when the account is already `DISABLED`.
- Reactivate is valid only from `DISABLED`.
- Reactivation does not automatically sign the user in.
- Reactivating a previously invited account returns it to `INVITED`; the administrator must reissue a setup credential.
- Reactivating an account that already completed setup returns it to `ACTIVE`.
- Disabling an account must invalidate its active authentication state through the approved Identity security-stamp/session strategy.
- Existing authenticated cookies for a disabled account must fail closed on subsequent protected requests.
- Disabled and invited users cannot sign in.
- This unit does not create a manual unlock action for `LOCKED`; existing Identity lockout behavior remains authoritative until a later spec explicitly adds administration for it.
- Accounts are never physically deleted in normal administration.

### Last-active-admin safeguard

The system must not allow an administration action to leave zero active administrators.

Treat an active administrator as a staff account that:

- has primary role `ADMIN`; and
- has an account status that permits normal authenticated use.

Before changing an active admin to a non-admin role or disabling that account, verify that at least one other active admin will remain. Return `409 Conflict` when the operation would remove the final active administrator.

Apply the safeguard transactionally to reduce race conditions. Do not rely only on a frontend warning.

### Server-side team authorization

Extend the Application-safe current-user access abstraction from Unit 20 with team scope without introducing HTTP, Identity, claims, or EF Core dependencies into Application use cases.

The effective access contract should expose equivalent information to:

```txt
CurrentUserAccess
- userId
- isAuthenticated
- accountStatus
- primaryRole
- effectivePermissions
- teamScopeType
- selectedTeamIds
- isAdmin
```

Add a reusable team-access service/policy abstraction suitable for upcoming player, match, report, import, media, medical, and dashboard modules.

Required behavior:

- `ADMIN` succeeds for every team.
- `ALL_TEAMS` succeeds for every team.
- `SELECTED_TEAMS` succeeds only for an assigned team ID.
- Missing access profiles, disabled/invited accounts, and malformed scope data fail closed.
- Client-supplied role, permission, scope type, or selected-team claims are not authoritative.
- Archived/inactive team lifecycle does not silently alter the scope relation; feature use cases separately determine whether an operation is valid for that team's state.
- The service must support unit testing without an HTTP server.

Do not add broad product endpoints merely to demonstrate team authorization. Verify it through unit/integration tests and use it in later feature modules.

### Session contract

Extend the existing authenticated `GET /api/auth/session` response with safe effective scope data.

Recommended shape:

```txt
teamScope:
  type: ALL_TEAMS | SELECTED_TEAMS
  selectedTeamIds: string[]
```

Rules:

- `ADMIN` sessions report effective `ALL_TEAMS` and an empty selected-team list.
- `ALL_TEAMS` sessions report an empty selected-team list.
- `SELECTED_TEAMS` sessions report the canonical selected-team IDs.
- IDs use the existing public ID serialization convention.
- Session responses must not include setup tokens, password/security metadata, normalized email fields, or join-table identifiers.
- Session data supports frontend rendering only; backend authorization remains authoritative.

### API surface

Create an admin-protected staff endpoint group using the existing Users-module conventions. Preferred route prefix:

```txt
/api/users
```

Required routes:

```txt
GET    /api/users
GET    /api/users/{userId}
POST   /api/users/invitations
POST   /api/users/{userId}/invitations/reissue
PATCH  /api/users/{userId}
PUT    /api/users/{userId}/access
POST   /api/users/{userId}/disable
POST   /api/users/{userId}/reactivate
```

Create the invitation-acceptance auth endpoint:

```txt
POST /api/auth/invitations/accept
```

Equivalent paths are acceptable only when they follow a clearly established project convention and keep administration endpoints separate from anonymous setup acceptance.

Administration routes must:

- require authentication;
- remain subject to the Unit 18 required-password-change gate;
- require the Unit 20 `AdminOnly` policy;
- derive the actor from the authenticated current-user context;
- return API responses rather than redirects;
- use established ProblemDetails behavior;
- avoid exposing EF/Identity entities directly.

The invitation-acceptance route must be explicitly anonymous and narrowly scoped. Do not add a registration route, account-discovery route, or public staff lookup.

### Request contracts

Recommended contracts:

```txt
CreateStaffInvitationRequest
- displayName
- email
- primaryRole
- canVerifyReports
- canImportData
- canViewMedicalDetails
- teamScopeType
- selectedTeamIds

UpdateStaffProfileRequest
- displayName

ReplaceStaffAccessRequest
- primaryRole
- canVerifyReports
- canImportData
- canViewMedicalDetails
- teamScopeType
- selectedTeamIds

AcceptStaffInvitationRequest
- email
- token
- password
- confirmPassword
```

Requirements:

- invitation creation and access replacement treat the supplied access configuration as one complete atomic state;
- omitted access fields are not interpreted as permissive defaults;
- email cannot be changed through profile/access update routes;
- route IDs identify target users and are never taken from the body;
- status, normalized email, password metadata, timestamps, and setup state are server-owned;
- setup tokens are accepted only by the dedicated invitation endpoint;
- enum parsing failures use safe validation responses.

### Response contracts

Recommended staff response:

```txt
StaffUserResponse
- id
- displayName
- email
- status
- primaryRole
- permissions
  - canVerifyReports
  - canImportData
  - canViewMedicalDetails
- teamScope
  - type
  - selectedTeamIds
- createdAtUtc
- updatedAtUtc
```

Invitation creation/reissue may add a one-time envelope:

```txt
StaffInvitationCredentialResponse
- user
- setupToken
```

A safe relative setup path may be returned if useful, but do not hardcode or infer an unconfigured frontend origin. The token must appear only in the successful creation/reissue response.

Staff list behavior:

- return all staff statuses by default so administrators can manage invited, active, disabled, and locked accounts;
- support small, bounded optional filters for search, role, status, scope type, and team ID when consistent with existing query conventions;
- search only safe staff fields such as display name and email;
- use deterministic ordering, preferably display name, then email, then ID;
- do not expose setup credentials or security metadata;
- do not add complex pagination unless an established project-wide list convention already requires it.

### Validation and errors

Use FluentValidation and established Application/ProblemDetails patterns.

Minimum invitation validation:

- display name is required after trimming and uses a documented maximum length;
- email is required and valid;
- email uniqueness is case-insensitive through Identity normalization and database constraints;
- primary role is a defined `StaffRole`;
- permission values are explicit booleans;
- scope type is defined;
- `ALL_TEAMS` requires an empty selected-team list;
- non-admin `SELECTED_TEAMS` requires at least one unique existing non-archived team;
- `ADMIN` is normalized to `ALL_TEAMS` with no selected-team rows.

Minimum access-replacement validation:

- target user exists;
- role, permissions, and scope form one complete valid access state;
- selected IDs are unique and valid;
- changing the final active admin is rejected;
- account status is not changed through this request.

Minimum invitation-acceptance validation:

- email, token, password, and confirmation are required;
- password and confirmation match;
- password satisfies the existing Identity policy;
- failures do not reveal whether a staff email exists outside possession of a valid invitation credential.

Expected errors:

- `400 Bad Request` for malformed input;
- the established validation status (`400` or `422`) for semantic validation failures;
- `401 Unauthorized` for unauthenticated administration requests;
- `403 Forbidden` for authenticated non-admin requests;
- `404 Not Found` for missing target users on authenticated admin routes;
- `409 Conflict` for duplicate email, invalid lifecycle conflict, final-active-admin protection, or conflicting concurrent mutation;
- a generic safe validation/authentication error for invalid invitation acceptance;
- `500` only for unexpected failures handled by the global exception pipeline.

Do not reveal database constraints, Identity internals, stack traces, token validation details, or whether an arbitrary public email address exists.

## Implementation

### 1. Required reading and existing-pattern review

Before changing code:

1. Read root `AGENTS.md`.
2. Read the six context files in the required order.
3. Read the current `context/feature-specs/00-build-plan.md`.
4. Read this feature spec completely.
5. Review Units 14–20 for Identity, account status, cookie/session, current-user, role, permission, policy, and first-admin patterns.
6. Review Unit 22 for Team entity IDs, lifecycle, query abstractions, persistence, and test setup.
7. Use relevant installed backend Codex skills/plugins when applicable without allowing them to override project context or this spec.

Extend established patterns. Do not create parallel Identity, authorization, result/error, validation, transaction, clock, or endpoint-group infrastructure.

### 2. Domain and Application access types

Add the canonical `TeamScopeType` in the appropriate framework-independent layer.

Extend the existing staff profile/access model through approved domain methods or controlled Application behavior so arbitrary property assignment cannot bypass normalization rules.

Add or extend Application contracts for:

- current effective team scope;
- selected-team access checks;
- Identity staff account creation and lookup;
- setup-token generation/validation;
- password-presence/setup-state checks;
- security-state invalidation;
- atomic staff profile and scope persistence.

Application must not depend on:

- `ApplicationUser`;
- `UserManager` or `SignInManager`;
- EF Core;
- ASP.NET Core HTTP context;
- `ClaimsPrincipal`;
- provider-specific token types.

Infrastructure implements the required Identity and persistence abstractions.

### 3. Persistence model and migration

Extend the authoritative staff profile with:

- required display name if not already present;
- team-scope type;
- any minimal timestamps required by existing entity conventions.

Add a relational selected-team scope entity/table equivalent to:

```txt
StaffTeamScope
- staffAccessProfileId or userId
- teamId
- createdAtUtc, only if required by established conventions
```

Configure:

- a required FK to the authoritative staff profile/user access record;
- a required FK to Unit 22 `Team`;
- a unique composite constraint preventing duplicate user/team assignments;
- indexes supporting lookup by user and by team;
- no cascade behavior that could physically delete staff or team history unexpectedly;
- stable enum persistence using the existing convention.

Create a named migration for this unit. Review it to ensure it contains only expected staff-profile, team-scope, and necessary Identity/account changes.

Do not add audit tables, email-delivery tables, password-reset tables, player/team-assignment tables, or unrelated schema.

### 4. Invitation creation and reissue slices

Create focused Application vertical slices for:

- create staff invitation;
- reissue staff invitation.

Creation must coordinate Identity and staff-profile persistence safely. Use one transaction when the existing DbContext/Identity setup supports it. If token generation must occur after the transaction commits, ensure a token-generation failure leaves a valid `INVITED` account that can be recovered through reissue rather than a partially corrupt profile.

Ensure duplicate email races return a safe `409 Conflict`.

Return setup credentials through Application/API contracts only for the current response. Never persist or log plaintext credentials.

### 5. Invitation acceptance slice

Add a focused anonymous Application/API flow for setup acceptance.

Requirements:

- decode the URL-safe token safely;
- resolve the intended invited account through an abstraction;
- verify account status is `INVITED`;
- apply the password through Identity;
- activate the account only after password setup succeeds;
- clear required-password-change state;
- invalidate token reuse;
- avoid partial state if activation fails;
- return a minimal success response such as `204 No Content`;
- keep failure messages generic.

Do not sign the user in automatically and do not issue a browser-accessible token.

### 6. Staff query slices

Create focused queries for:

- list staff users;
- get staff user by ID.

Map Identity and staff-profile data behind Application abstractions. Avoid direct EF or Identity queries in API endpoint handlers.

Return only the safe response contract. Selected-team IDs must be deterministic and distinct.

### 7. Profile and access replacement slices

Create separate focused mutations for:

- update staff display name;
- replace staff access settings.

Access replacement must atomically apply:

- primary role;
- explicit permissions;
- team-scope type;
- complete selected-team assignment set.

Validate all target team IDs before changing persisted access. If any validation or persistence step fails, preserve the previous complete valid access state.

Apply admin normalization and the final-active-admin safeguard server-side.

### 8. Disable and reactivate slices

Create explicit Application use cases for disable and reactivate.

Disable must:

- apply the documented lifecycle transition;
- prevent removal of the final active admin;
- invalidate active sessions/security state;
- keep user/profile/scope records intact;
- remain idempotent for an already-disabled account.

Reactivate must:

- work only from `DISABLED`;
- choose `INVITED` or `ACTIVE` based on whether setup/password exists;
- preserve role, permissions, and team scope;
- not generate a setup token automatically;
- not sign the account in automatically.

### 9. Team-access resolver and session enrichment

Extend the existing current-access resolver to load effective team scope through the authoritative relational data.

Add a reusable team-access service/policy abstraction and focused tests.

Update the existing session use case/endpoint rather than adding a parallel access endpoint. Keep role and permission behavior from Unit 20 unchanged while adding the documented `teamScope` response.

Ensure account-status and required-password-change gates continue to run before normal protected module access.

### 10. API endpoint mapping

Map the staff and invitation routes through the existing endpoint-group conventions.

API handlers must remain thin:

- parse route/body/query input;
- invoke Application use cases;
- map established result/errors to HTTP responses;
- never contain Identity, EF Core, transaction, lifecycle, or team-scope business logic.

Apply `AdminOnly` to the entire administration group. Mark only invitation acceptance anonymous.

### 11. Tests

Add focused unit and integration tests using the existing test infrastructure.

Minimum domain/Application coverage:

- `ALL_TEAMS` and `SELECTED_TEAMS` validation;
- selected scope rejects empty, duplicate, unknown, and archived team IDs;
- switching to all teams clears selected assignments;
- role change to admin normalizes scope and effective permissions;
- changing from admin requires an explicit valid non-admin access state;
- team-access service grants admin/all-team access and restricts selected-team access;
- missing/malformed access fails closed;
- final-active-admin safeguard rejects demotion/disable and allows change when another active admin exists;
- disable/reactivate transition behavior, including invited-account reactivation.

Minimum integration coverage:

- unauthenticated staff administration returns `401`;
- non-admin administration returns `403`;
- admin can create an invitation with valid profile and scope;
- duplicate email returns `409` safely;
- invitation response exposes the setup credential only at creation/reissue;
- valid invitation acceptance sets password and activates the account;
- accepted user can sign in through the existing login endpoint;
- invalid, expired, replaced, and reused setup tokens fail safely;
- reissue invalidates the previous token;
- admin can list and read safe staff responses;
- display name update works without changing email/access;
- access replacement is atomic;
- disabling a user prevents login and protected session use;
- reactivation returns a completed account to `ACTIVE` and an incomplete invitation to `INVITED`;
- session response includes effective team-scope data;
- no public registration endpoint exists;
- existing auth, role, settings, teams, health, error, architecture, and migration tests continue to pass.

Use the established database test strategy. Do not introduce developer-specific credentials or a new container/testing stack unless explicitly required and documented.

### 12. Documentation updates

Update `context/progress-tracker.md` during implementation:

- mark Unit 23 in progress when work starts;
- mark it complete only after all verification passes;
- record the final team-scope enum and persistence shape;
- record the invitation-token lifetime/configuration and reissue behavior;
- record the staff endpoint paths and safe response fields;
- record account lifecycle and final-active-admin rules;
- record the session team-scope extension;
- note that email delivery, login-email recovery, frontend administration, and audit persistence remain deferred;
- document any test or migration limitation honestly.

Update `context/architecture.md` or `context/code-standards.md` only when implementation changes a project-level decision. Do not duplicate feature-level details unnecessarily.

## Dependencies

None expected.

Use the existing ASP.NET Core Identity token providers, EF Core/PostgreSQL persistence, FluentValidation, authorization, ProblemDetails, clock/result abstractions, configuration, and backend test packages introduced by previous units.

Do not install email providers, external RBAC libraries, JWT/OAuth packages, audit packages, frontend packages, queue/background-job systems, object storage packages, or generic user-management frameworks in this unit.

## Verification checklist

- [ ] `AGENTS.md`, all required context files, the current build plan, relevant prior auth/authorization/team specs, and this feature spec were read before implementation.
- [ ] Relevant installed backend Codex plugins were used when applicable without overriding project context or the active spec.
- [ ] No public registration, player login, multi-club behavior, frontend UI, or email-provider integration was added.
- [ ] The existing authoritative Identity user and staff access/profile model were extended rather than duplicated.
- [ ] A required staff display name exists in the authoritative staff profile.
- [ ] `TeamScopeType` contains exactly `ALL_TEAMS` and `SELECTED_TEAMS`.
- [ ] Selected team scope uses relational foreign keys to Unit 22 teams rather than JSON, strings, claims, or unvalidated IDs.
- [ ] Duplicate user/team assignments are prevented by a database constraint.
- [ ] `SELECTED_TEAMS` requires at least one valid unique non-archived team for non-admin users.
- [ ] Inactive teams may be assigned, and existing scope rows are preserved when a team is archived.
- [ ] Switching to `ALL_TEAMS` clears selected assignments atomically.
- [ ] `ADMIN` is normalized to effective `ALL_TEAMS` with no selected-team rows and retains full effective permissions.
- [ ] Changing from `ADMIN` to a non-admin role requires a complete valid scope and explicit permission state.
- [ ] Application use cases do not depend on Identity, EF Core, HTTP context, or `ClaimsPrincipal`.
- [ ] Reusable server-side team-access checks grant admin/all-team access and correctly restrict selected-team users.
- [ ] Missing profiles, disabled/invited accounts, and malformed scope data fail closed.
- [ ] `GET /api/auth/session` includes safe effective team-scope data and no security metadata.
- [ ] Admin-only staff list and detail endpoints return safe deterministic contracts.
- [ ] Invitation creation atomically creates the Identity account, staff profile, and valid team scope.
- [ ] New invited users have status `INVITED` and no administrator-chosen password.
- [ ] Setup credentials are URL-safe, time-limited, one-time, never stored in plaintext, never logged, and returned only on invitation creation/reissue.
- [ ] Reissuing an invitation invalidates earlier setup credentials.
- [ ] Invitation acceptance enforces the existing password policy, activates the user, clears required-password-change state, and does not auto-sign in.
- [ ] Invalid, expired, replaced, mismatched, and reused invitation credentials fail with a generic safe response.
- [ ] Duplicate staff email creation returns a safe `409 Conflict`.
- [ ] Display-name update cannot change login email or access state.
- [ ] Access replacement updates role, permissions, scope type, and selected-team assignments atomically.
- [ ] Disable/reactivate transitions match the documented account lifecycle and preserve profile/scope history.
- [ ] Disabling a user invalidates effective authenticated access and prevents subsequent login.
- [ ] Reactivating a completed account returns it to `ACTIVE`; reactivating an incomplete invitation returns it to `INVITED`.
- [ ] The final active administrator cannot be disabled or demoted, and the safeguard is enforced server-side.
- [ ] No staff account is physically deleted.
- [ ] No manual login-email recovery, general password reset, manual unlock, or audit persistence was added.
- [ ] The EF Core migration contains only expected staff-profile, team-scope, and necessary account changes.
- [ ] The migration applies successfully to the configured development database.
- [ ] Focused team-scope, invitation, lifecycle, authorization, and session tests pass.
- [ ] Existing backend tests continue to pass.
- [ ] `dotnet restore backend/PlayerPerformance.sln` passes.
- [ ] `dotnet build backend/PlayerPerformance.sln` passes with no errors.
- [ ] `dotnet test backend/PlayerPerformance.sln` passes.
- [ ] No frontend source or package changes were made.
- [ ] `context/progress-tracker.md` reflects the actual Unit 23 implementation, verification results, and deferred work.
