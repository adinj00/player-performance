# Unit 20: Backend Staff Roles and Authorization Foundation

## Goal

Introduce the canonical backend staff-role and explicit-permission model, assign the environment-bootstrapped first account the official `ADMIN` role, and provide reusable authorization policies for upcoming protected backend modules. This unit establishes role-based authorization only; selected-team scope assignments and staff-management CRUD remain deferred until team/selection records exist.

## Design

This is a backend-only authorization foundation unit. It converts the authenticated staff identity introduced in Units 14–18 into an application access identity that future settings, team, player, match, report, import, media, medical, and administration modules can authorize consistently.

The V1 access model uses:

- one primary role per staff user;
- a small set of explicit permission flags;
- team scope added later, after configurable team/selection entities exist;
- backend authorization as the source of truth.

Supported primary roles:

```txt
ADMIN
DATA_OPERATOR
ANALYST
COACH
MEDICAL_STAFF
VIEWER
```

Supported explicit permission flags:

```txt
canVerifyReports
canImportData
canViewMedicalDetails
```

The role and permission model must remain independent of visible Bosnian labels. Code identifiers, database values, API contract names, policy names, and enum values remain in English. Future frontend units localize display labels.

### Scope boundary

This unit includes:

- canonical role and permission types;
- persistent one-to-one staff access data linked to the Identity user;
- first-admin handoff to the official `ADMIN` role;
- a current-user access context available to Application use cases;
- reusable role/permission authorization policies and handlers;
- session-contract enrichment with safe role and permission data;
- migrations and focused backend tests.

This unit does not include:

- staff list/create/invite endpoints;
- invitation/setup-token generation;
- role or permission management endpoints;
- team/selection entities;
- `ALL_TEAMS` or `SELECTED_TEAMS` assignment persistence;
- selected team IDs;
- staff administration UI;
- public registration;
- player login;
- login email recovery;
- audit-log persistence;
- feature-specific authorization rules for players, matches, imports, medical data, or reports.

The separation is intentional. A selected-team scope cannot be persisted with proper relational integrity before Unit 22 introduces the team/selection model. Do not store unvalidated team IDs, comma-separated IDs, JSON arrays, or foreign-key-free placeholder assignments in this unit.

### Access profile model

Prefer a dedicated staff access profile linked one-to-one with the existing Identity user instead of spreading authorization data across unrelated authentication fields.

Recommended conceptual model:

```txt
StaffAccessProfile
- UserId
- PrimaryRole
- CanVerifyReports
- CanImportData
- CanViewMedicalDetails
- CreatedAtUtc
- UpdatedAtUtc
```

Requirements:

- `UserId` references the existing Identity user and is unique.
- Deleting an Identity user is not part of normal account lifecycle behavior; choose relationship behavior that does not accidentally remove audit-relevant data through routine operations.
- The access profile stores application authorization data, not passwords, security stamps, cookies, lockout counters, invitation tokens, or reset tokens.
- Use UTC timestamps through the approved clock abstraction where available.
- Keep provider-specific EF Core configuration in Infrastructure.
- Keep role and permission concepts available through Domain/Application-safe contracts rather than leaking the Identity entity into use cases.

If the implemented codebase already has a clearly documented access-profile pattern, extend it instead of creating a competing model. Do not duplicate primary-role or permission fields in multiple tables.

### Role behavior baseline

This unit defines the following stable baseline:

- `ADMIN` has unrestricted application-level access by role and is treated as having all explicit permission flags, regardless of stored flag values.
- `DATA_OPERATOR`, `ANALYST`, `COACH`, `MEDICAL_STAFF`, and `VIEWER` do not gain broad access merely from authentication.
- Non-admin capabilities are granted by feature-specific policies introduced with those features, using primary role, explicit permissions, and later team scope.
- `ANALYST` does not automatically receive report-verification permission; `canVerifyReports` is required unless the user is `ADMIN`.
- Import permission is represented by `canImportData` and does not automatically imply authorization for every future import type.
- Medical-detail permission is represented by `canViewMedicalDetails` and does not replace future feature-specific medical authorization checks.
- Account status continues to control whether the account may authenticate and remain active. Role assignment must not reactivate, unlock, or otherwise change account lifecycle status.

Do not encode all future feature permissions in this unit. Establish reusable primitives and policies only for rules already confirmed by project context.

### Authorization policy baseline

Create clear reusable policy names and requirements. Recommended policies:

```txt
AdminOnly
CanVerifyReports
CanImportData
CanViewMedicalDetails
```

Exact constant names may follow existing conventions, but avoid raw policy-name strings scattered throughout endpoint groups.

Expected behavior:

- `AdminOnly` succeeds only for an authenticated active user whose primary role is `ADMIN`.
- Explicit-permission policies succeed for `ADMIN` automatically.
- For non-admin users, each permission policy succeeds only when the corresponding stored permission is true.
- Missing access profile fails authorization safely.
- An unauthenticated request remains `401`.
- An authenticated request lacking a required policy returns `403` through the existing ProblemDetails-compatible behavior.
- The required-password-change gate from Unit 18 remains effective before normal protected feature access.

Do not use frontend visibility, cookie claims alone, or client-provided role values as the source of truth. Authorization handlers must resolve current persisted access data through approved backend services.

### Claims and persisted access data

The authentication cookie may include minimal stable identity claims, but persisted access data remains authoritative.

Requirements:

- Do not trust a role or permission value submitted by the client.
- Do not let stale cookie claims silently grant access after persisted permissions change.
- Prefer resolving the current access profile through a scoped current-user/access service, with appropriate request-level reuse to avoid repeated database reads.
- If role/permission claims are added to the cookie for display or performance, implement a documented refresh/security-stamp strategy and still keep server-side authorization consistent with persisted state.
- Do not store selected team IDs in claims in this unit.
- Do not expose internal Identity security data in claims or API responses.

### First-admin handoff

Unit 17 created the first staff account before the official role model existed. This unit must complete that handoff safely.

The bootstrap account must receive exactly one `StaffAccessProfile` with:

```txt
PrimaryRole = ADMIN
```

The handoff must be idempotent and deterministic.

Preferred resolution order:

1. Use an existing explicit first-admin/bootstrap marker if Unit 17 implemented one.
2. Otherwise, use the configured first-admin email only during a controlled startup/backfill operation.
3. If neither is available, allow automatic assignment only when the database contains exactly one Identity user and no access profiles, and document that fallback.
4. If the state is ambiguous, fail safely with a clear operational error rather than assigning `ADMIN` to an arbitrary account.

Requirements:

- Restarting the API must not create duplicate profiles.
- Existing non-admin profiles must not be promoted automatically.
- Changing the bootstrap email after profiles exist must not transfer admin access.
- The handoff must not reset the password, clear `mustChangePassword`, alter account status, or issue a login cookie.
- Do not log credentials or setup secrets.
- Record the final handoff approach in `context/progress-tracker.md`.

### Session contract

Extend the authenticated `GET /api/auth/session` response with safe access information needed by future frontend authorization-aware rendering.

Recommended fields:

```txt
primaryRole
permissions:
  canVerifyReports
  canImportData
  canViewMedicalDetails
```

Requirements:

- Keep existing authentication, user identity, account-status, and `mustChangePassword` fields compatible with Units 18–19.
- Do not expose selected team IDs or team scope before Unit 23.
- Do not expose password/security metadata, role-assignment internals, database row IDs, or Identity security stamps.
- The session response supports UI behavior only; backend policies still enforce every protected action.
- Missing access profile for an authenticated user must be handled safely and observably. Do not silently assign a default privileged role.

### Database and migration behavior

Add the persistence mapping and migration required by the access profile.

Requirements:

- Migration belongs to the existing Infrastructure migration assembly/path.
- Use a unique one-to-one constraint on `UserId`.
- Persist the role in a stable, explicit representation consistent with current EF Core conventions.
- Configure permission flags with safe false defaults for non-admin profiles.
- Do not create team-scope tables or team foreign keys yet.
- Do not seed sample staff users.
- Avoid automatic destructive migration behavior at startup unless already approved by project context.

## Implementation

### Required reading

Before implementation, read:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/ui-context.md`
5. `context/code-standards.md`
6. `context/ai-workflow-rules.md`
7. `context/progress-tracker.md`
8. `context/feature-specs/00-build-plan.md`
9. `context/feature-specs/20-backend-staff-roles-authorization-foundation.md`

Inspect the final implemented outputs from:

- Unit 12 backend persistence foundation;
- Unit 13 backend shared domain primitives;
- Unit 14 backend auth and Identity foundation;
- Unit 15 backend CSRF and session API;
- Unit 17 first admin bootstrap;
- Unit 18 backend login and required password change.

Use relevant installed backend Codex plugins when applicable. Plugins may guide implementation workflow but must not override the project context, active feature spec, existing architecture, or final contracts already implemented.

### Existing-contract inspection

Before adding code, inspect the current implementation rather than assuming every recommended earlier shape was used verbatim.

Confirm:

- Identity user key type and entity location;
- account-status and `mustChangePassword` property names;
- DbContext and migration assembly configuration;
- clock/result/error abstractions;
- current-user/session service structure;
- authorization middleware order;
- ProblemDetails `401`/`403` behavior;
- first-admin bootstrap identification strategy;
- authenticated session response contract.

Extend existing patterns. Do not introduce parallel current-user abstractions, duplicate role enums, duplicate permission models, or a second session contract.

### Domain and Application contracts

Add canonical role and permission contracts in the layer that matches the current architecture.

Recommended types:

```csharp
public enum StaffRole
{
    Admin,
    DataOperator,
    Analyst,
    Coach,
    MedicalStaff,
    Viewer
}

public sealed record StaffPermissions(
    bool CanVerifyReports,
    bool CanImportData,
    bool CanViewMedicalDetails);
```

Names may follow established casing/serialization conventions. Persisted/API values should map predictably to the documented uppercase contract values.

Add an Application-safe current access abstraction, for example:

```txt
ICurrentUserAccess
- UserId
- IsAuthenticated
- PrimaryRole
- Permissions
- IsAdmin
```

or a query/service returning equivalent data.

Requirements:

- Application code must not depend on `ApplicationUser`, `UserManager`, `SignInManager`, EF Core, `ClaimsPrincipal`, or HTTP context.
- Infrastructure/Api adapt Identity and HTTP concerns into the Application-safe contract.
- Missing profile and unavailable account states must be explicit, not represented as `ADMIN` or another permissive default.
- Keep the abstraction extensible for Unit 23 to add team scope without breaking every consumer.

### Access profile entity and configuration

Implement the one-to-one access profile and EF Core configuration.

Requirements:

- Keep business-facing role/permission semantics outside API endpoint handlers.
- Configure `UserId` as required and unique.
- Add role and permission fields with clear names.
- Add UTC audit timestamps if consistent with existing entity patterns.
- Use the approved entity base or clock conventions where appropriate.
- Keep migrations and provider-specific mappings in Infrastructure.
- Do not add navigation patterns that force Domain to depend on Identity.

### Current access resolver

Implement a scoped resolver that maps the authenticated Identity user to the persisted access profile.

Requirements:

- Resolve the authenticated user ID from the approved Unit 18 current-user mechanism.
- Load the access profile through Infrastructure behind an Application abstraction.
- Reuse the result within one request where practical.
- Treat absent profiles as an authorization failure and log a safe diagnostic without exposing sensitive data to the client.
- Do not query by client-submitted user ID.
- Do not bypass account-status or required-password-change enforcement.
- Leave a clean extension point for Unit 23 to attach team scope.

### Authorization constants, requirements, and handlers

Create centralized policy constants and authorization requirements/handlers.

Requirements:

- Register policies through a clearly named DI extension.
- Keep handlers small and testable.
- Implement `AdminOnly` and the three explicit permission policies.
- Treat `ADMIN` as satisfying all explicit permission policies.
- Ensure authenticated non-admin users require the corresponding persisted flag.
- Ensure missing profiles fail closed.
- Avoid hardcoded role strings in future endpoint mappings by exposing constants or typed helpers.
- Do not implement feature-specific team authorization before Unit 23.

If the codebase uses Minimal API endpoint groups, make it straightforward for future groups to apply policies through `RequireAuthorization(...)` or an equivalent approved pattern.

### First-admin role backfill

Add the idempotent first-admin role handoff.

Requirements:

- Run only after persistence and Identity are available.
- Use the safest existing Unit 17 identifier.
- Create the access profile only when it does not exist.
- Assign `ADMIN` and rely on admin override semantics for explicit permissions.
- Do not create additional staff users.
- Fail clearly on ambiguous bootstrap state.
- Keep the implementation outside `Program.cs` except for one explicit invocation/extension call.

If a migration-time data operation is safer than startup backfill in the implemented architecture, use it only when it can deterministically identify the intended account. Document the chosen strategy.

### Session enrichment

Update the existing session use case/endpoint rather than creating a second access endpoint solely for the frontend.

Requirements:

- Add primary role and explicit permission values for authenticated users with valid access profiles.
- Keep unauthenticated response behavior unchanged.
- Keep required-password-change routing compatible with Unit 19.
- Ensure admin sessions report effective permissions consistently.
- Do not add team scope fields yet.
- Update frontend-facing contract documentation in `context/progress-tracker.md`; do not implement frontend changes in this unit.

### Authorization probe for verification

Do not add production demo endpoints solely to prove policies.

Use one of these approaches:

- test-host-only endpoints mapped by integration-test setup;
- direct authorization-service tests;
- applying `AdminOnly` to the first real backend settings endpoint in Unit 21 after this foundation is verified.

Unit 20 itself must verify policies through tests without expanding product API scope.

### Tests

Add focused unit and integration tests using the existing Unit 09 test infrastructure.

Minimum coverage:

- role serialization/persistence mapping is stable;
- a non-admin profile defaults explicit flags to false;
- `ADMIN` satisfies `AdminOnly`;
- non-admin roles fail `AdminOnly`;
- `ADMIN` satisfies all explicit permission policies;
- a non-admin user with a permission flag satisfies the matching policy;
- a non-admin user without the flag receives `403`;
- an unauthenticated request receives `401`;
- an authenticated user without an access profile fails closed;
- the first-admin handoff creates exactly one `ADMIN` access profile;
- repeating the handoff is idempotent;
- ambiguous bootstrap state does not grant admin to an arbitrary account;
- session response includes role and effective permissions;
- session response does not include team IDs or security-sensitive Identity data;
- existing login, password-change, CSRF, session, bootstrap, health, architecture, and error-handling tests still pass.

Testing rules:

- Do not add a production-only test route.
- Do not introduce Testcontainers unless already available or strictly required and documented.
- Do not depend on developer-specific credentials.
- Keep authorization handlers unit-testable without booting the full API where practical.

### Documentation updates

Update `context/progress-tracker.md` during implementation:

- mark Unit 20 in progress when work starts;
- mark it complete only after all verification passes;
- record the final access-profile shape and role enum names;
- record authorization policy names;
- record the first-admin role handoff strategy;
- record the enriched session fields;
- note explicitly that selected-team scope remains deferred to Unit 23;
- record any test limitation or manual migration verification.

Update `context/architecture.md` or `context/code-standards.md` only if the implementation changes a documented project-level rule or boundary. Do not update them merely to repeat implementation details already captured by this spec and the progress tracker.

## Dependencies

None expected.

Use the ASP.NET Core Identity, authorization, EF Core/PostgreSQL, configuration, ProblemDetails, clock/result abstractions, and test packages already introduced by previous units. Do not add external RBAC/permission frameworks, JWT/OAuth packages, policy engines, team-scope packages, frontend packages, audit packages, or invitation/email packages in this unit.

## Verification checklist

- [ ] `AGENTS.md`, all required context files, the revised build plan, and this feature spec were read before implementation.
- [ ] Relevant installed backend Codex plugins were used when applicable without overriding project context.
- [ ] A single canonical primary-role model exists with `ADMIN`, `DATA_OPERATOR`, `ANALYST`, `COACH`, `MEDICAL_STAFF`, and `VIEWER`.
- [ ] Explicit flags exist for report verification, imports, and medical-detail access.
- [ ] Access data is persisted in one authoritative profile linked one-to-one to the Identity user.
- [ ] Authentication/password/security fields were not duplicated into the access profile.
- [ ] No team IDs, selected-team assignments, JSON team lists, or foreign-key-free team references were introduced.
- [ ] Application use cases can resolve current role and permissions without depending on Identity, EF Core, HTTP context, or `ClaimsPrincipal`.
- [ ] `AdminOnly` and the three explicit-permission policies are registered through centralized constants/helpers.
- [ ] `ADMIN` satisfies all explicit permission policies.
- [ ] Missing profiles and insufficient permissions fail closed.
- [ ] Unauthenticated and forbidden responses remain consistent with existing `401`/`403` ProblemDetails behavior.
- [ ] The Unit 18 required-password-change gate still blocks normal protected access.
- [ ] The first bootstrapped account receives exactly one official `ADMIN` access profile.
- [ ] First-admin handoff is deterministic, idempotent, and fails safely when ambiguous.
- [ ] Role handoff does not alter password, account status, lockout state, or `mustChangePassword`.
- [ ] `GET /api/auth/session` includes safe role and effective-permission data for authenticated users.
- [ ] Session responses do not expose team IDs, Identity security metadata, password data, or internal persistence identifiers.
- [ ] The EF Core migration applies successfully to the configured development database.
- [ ] Focused role, permission, policy, session, and first-admin handoff tests pass.
- [ ] Existing backend tests continue to pass.
- [ ] `dotnet restore backend/PlayerPerformance.sln` passes.
- [ ] `dotnet build backend/PlayerPerformance.sln` passes with no errors.
- [ ] `dotnet test backend/PlayerPerformance.sln` passes.
- [ ] No frontend code or packages were changed.
- [ ] No staff CRUD, invitation flow, account-management endpoints, or team-scope persistence was added.
- [ ] `context/progress-tracker.md` reflects the actual Unit 20 implementation and deferred Unit 23 scope work.
