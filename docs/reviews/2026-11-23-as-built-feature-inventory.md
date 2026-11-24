# Player Performance as-built feature inventory

**Date** 2026-11-23

**Branch and inspected revision** chore/feature-verification-inventory at b4cb6d92f364f4819de966b991dabf5245576e90

**Artifact** Preliminary source and evidence assessment for scope reconciliation.

## Verdict and evidence rules

| Verification status | Use when |
|---|---|
| **UNVERIFIED / NOT RUN** | The check has yet to be attempted. |
| **PASS** | An executed check succeeded and the result is supported by recorded evidence. |
| **FAIL** | An executed check failed and the observed result is recorded. |
| **BLOCKED** | A specific missing prerequisite prevents the check. Name the prerequisite. |

- **PASS for documentation coverage.** The inventory was checked against all 22 scope entries. The index records 11 existing, six in progress, two planned, and three dropped.
- **UNVERIFIED / NOT RUN for current application runtime and acceptance.** This reconnaissance was limited to source and existing evidence. Planned and dropped features have no current acceptance verdict. For feature 19, production deployment verification is **BLOCKED** pending approved operating decisions and a production storage adapter.
- **Source findings to reproduce at runtime.** Feature 3's final-admin guard can veto changes to inactive administrators. Feature 9 resets unsaved input after a save conflict and lacks field-level server feedback. Feature 11 has missing mutation coverage and an inaccurate prior status in a staff audit payload. Record a runtime verdict after exercising each affected flow.

Current intent and status come from the [scope index](../scope/index.md) and its five area files. [Root guidance](../../AGENTS.md), [backend guidance](../../backend/AGENTS.md), [frontend guidance](../../frontend/AGENTS.md), [DESIGN.md](../../DESIGN.md), and [Accepted spec 0001](../specs/0001-project-context-authority.md) govern the applicable boundaries. Spec 0001 governs project authority. The active spec directory contains no feature-level build or acceptance contract for these entries. The [legacy progress tracker](../legacy/context/progress-tracker.md) and numbered units are historical reports. For example, repository history reports 112 unit and 48 integration tests for Unit 57 and 20 Vitest tests for the later shell redesign. Those counts belong to the recorded runs. The [November workflow baseline review](2026-11-23-chore-workflow-baseline-final.md) covers workflow and documentation changes.

Source entries describe wiring and implementation at the inspected revision. Existing tests are cited as coverage evidence. Most feature API tests obtain their database through the Identity test factory, which replaces PostgreSQL with EF Core InMemory. The base factory retains Npgsql with an unreachable test connection for host and configuration checks. Neither establishes real PostgreSQL query translation, migration application, or transaction behavior. References to tests below use `I` for [backend integration tests](../../backend/tests/IntegrationTests), `U` for [backend unit tests](../../backend/tests/UnitTests), and `FE` for [frontend source](../../frontend/src), including colocated tests. Migration names refer to the [EF migrations directory](../../backend/src/Infrastructure/Persistence/Migrations).

### Workflow ownership

The nine checked-in JSMastery skill instructions were consulted for ownership. The /audit baseline already exists. /scope owns product reconciliation and the next feature queue.

For each existing feature, follow its dependencies in the work order below.

1. Use /check verify to establish the runtime baseline and record the observed outcome against current scope.
2. Use /debug to reproduce, isolate and minimally fix a bug. Use /architect when proceeding requires an important unresolved product or technical decision. Use /develop for scoped missing functionality or justified refactoring.
3. Close Beta with /check verify and /test. Re-exercise the affected behavior and add focused, durable regression coverage.
4. For GA, follow the Beta steps with /check review for independent review and /document for the relevant change record.
5. Use /sync when changes require reconciliation of durable project state within the skill's ownership boundaries.

### Verification tooling

- Use Playwright CLI or Playwright MCP to drive the browser during /check verify or /debug. Capture observable results from the running staff application.
- Use the existing xUnit and Vitest suites for automated regression coverage. Use real PostgreSQL where the claim depends on migrations, relational constraints, query translation, transactions or concurrency.
- Add persistent Playwright E2E tests through /test selectively when a critical user journey and its regression risk justify maintaining browser coverage.

**Reconciliation item.** The index declares GA as the project default and uses that tier for existing open checklists. The task sets Beta as the recommendation default, with GA for high-risk work. Use Beta recommendations here and ask /scope to reconcile the declared default and the feature-level GA exceptions in the next workflow. Preserve the existing statuses, completed boxes, and requested scope. Beta means runtime verification and focused tests. GA adds independent review and documentation for subsequent changes. A tier on a dropped feature is a risk classification only.

## Foundations and access

Current authority for 1–4 is [Foundations and access](../scope/foundations-and-access.md).

### 1. Application foundation · existing · Beta

- **Intent and as built.** ASP.NET Core composes application services, EF Core/Npgsql, ProblemDetails, validation, and health endpoints. Vite starts a React Router shell with Query providers and a shared API client. PostgreSQL mappings and private filesystem storage support later features. Start at [API Program](../../backend/src/Api/Program.cs), [health endpoints](../../backend/src/Api/Endpoints/HealthEndpoints.cs), [DbContext](../../backend/src/Infrastructure/Persistence/AppDbContext.cs), [infrastructure registration](../../backend/src/Infrastructure/DependencyInjection.cs), [frontend providers](../../frontend/src/app/providers.tsx), and [API client](../../frontend/src/lib/api/api-client.ts). Migrations include `20260710120000_InitialPersistenceBaseline` and `20260712232513_AddFileStorageAbstraction`. Tests include I `Api/HealthEndpointsTests.cs`, U `Architecture/ProjectDependencyTests.cs`, and U `Files/*Tests.cs`.
- **Historical evidence.** Units 01–13 record foundation checks. Unit 40 records local storage tests and migration application. The tracker records a later fresh PostgreSQL migration and bootstrap after the assignments ordering fix.
- **Current verification and gaps.** Prove explicit migrations into a disposable empty PostgreSQL database, inspect the resulting schema, launch both processes, check liveness/readiness and structured API errors, and render the client against the configured origin. The venue repair discovery and assignment migration identity warrant explicit checking under 5 and 6. No current startup failure is established.
- **Next.** `/check verify application foundation` with the minimum gates in the work order.

### 2. Staff authentication and sessions · existing · GA

- **Intent and as built.** Staff sign in with an Identity cookie, complete any required password change, access protected routes and sign out. CSRF protects unsafe authenticated actions. [Auth endpoints](../../backend/src/Api/Endpoints/Auth/AuthEndpoints.cs), [API authentication](../../backend/src/Api/Authentication), [Identity services and bootstrap](../../backend/src/Infrastructure/Identity), and [frontend auth](../../frontend/src/features/auth) implement this over Identity/users and access profiles. Migrations include `AddIdentityFoundation`, `AddStaffAccessProfiles` and `AddStaffAccessProfileUserForeignKey`. See I `Api/AuthEndpointsTests.cs`, I `Configuration/AuthenticationConfigurationTests.cs`, U `Identity/FirstAdminBootstrapOptionsTests.cs`, and FE `features/auth/route-decisions.test.ts` and `components/protected-route.test.tsx` within that feature.
- **Historical evidence.** Units 14–20 record auth and policy work. The legacy tracker records Development API health, CSRF and login returning 200, a focused 9/9 auth suite, and a separate keyboard browser login to the shell. The apparent local credential failure was corrected by reading the configuration as UTF-8.
- **Current verification and gaps.** Use freshly bootstrapped disposable staff to prove CSRF, temporary password enforcement, session persistence across reload, logout, and protected routes across roles. Check denied/missing-CSRF responses and stale session behavior. Forgot/reset routes are explicit placeholders outside this definition of done. Cookie policy, matching origins, and local key persistence require a real browser check.
- **Next.** `/check verify staff authentication and sessions` immediately after the foundation smoke.

### 3. Staff access and account administration · in-progress · GA

- **Intent and as built.** Admins invite staff, reissue setup, assign one primary role and explicit flags, set all/selected team scope, and manage disable/reactivate with a final-admin safeguard. [Staff endpoints](../../backend/src/Api/Endpoints/Users/StaffUserEndpoints.cs), [staff service](../../backend/src/Infrastructure/Staff/IdentityStaffUsersService.cs), [scope/access resolvers](../../backend/src/Infrastructure/Authorization), [Users](../../frontend/src/features/users/users-page.tsx), and [invitation acceptance](../../frontend/src/pages/accept-invitation-page.tsx) connect manual setup links and Identity to `20260710152754_AddStaffUsersTeamScopeLifecycle`. I `Authorization/StaffAuthorizationTests.cs` covers policies, effective permission flags and bootstrap handoff. I `Api/AuthEndpointsTests.cs` and U `Users/UserAccountStatusTests.cs` exercise unavailable accounts and lifecycle rules respectively.
- **Historical evidence.** Units 23–24 record applied migration, 34 unit and 24 integration tests, frontend quality checks and an incomplete full browser walkthrough. Unit 56 records a temporary non-medical viewer created, reactivated and disabled. This is partial lifecycle evidence.
- **Current verification and gaps.** Walk invite, reissue, accept, sign in, role/scope edits, permission revocation, disable/reactivate and final-admin checks with fresh cookies and two teams. **Confirmed guard mismatch:** `IsFinalActiveAdminAsync` counts active administrators globally, then checks only the target's role. With exactly one active admin, it also vetoes disabling an invited admin or demoting an inactive admin although neither operation would remove the last active admin. **Suspected concurrency risk:** the count and the write are separate operations. Check concurrent final-admin changes in PostgreSQL, along with cross-team denial. No dedicated invitation-to-reactivation endpoint suite was located. Production email delivery remains a deployment decision.
- **Next.** `/check verify staff access and account administration`, then /debug for observed failures and /test for focused lifecycle coverage.

### 4. Responsive shell and accessibility · existing · Beta

- **Intent and as built.** Bosnian Latin shell, responsive navigation, landmarks, skip link, route focus, and accessible table/chart alternatives. [App routes](../../frontend/src/app/app-routes.tsx), [layout](../../frontend/src/components/layout), [route focus](../../frontend/src/components/common/route-focus-manager.tsx), [CSS](../../frontend/src/index.css), and [generated UI primitives](../../frontend/src/components/ui) provide these, with protected routes using the session API. No feature-specific persistence. FE `app/route-paths.test.ts` and `features/auth/components/protected-route.test.tsx` cover routing and access decisions.
- **Historical evidence.** Unit 56 records a keyboard sign-in, protected landmarks and focus, seven viewport sizes, zoom and emulated media preferences. The maintainer accepted remaining real screen-reader/device and complete-workflow checks as follow-up. The subsequent shell redesign records build, lint and Vitest evidence without a new interactive viewport run. [Legacy responsive audit](../legacy/context/references/responsive-accessibility-audit.md)
- **Current verification and gaps.** Repeat keyboard, mobile navigation, focus, zoom, reflow and restricted UI checks against the redesigned shell with representative records. Document real assistive-technology/device coverage separately. **Design drift:** current CSS implements a light theme, while DESIGN.md calls for dark stadium-night styling. Spec 0001 also records differences within the design palette. Treat this as a scoped design decision.
- **Next.** `/check verify responsive shell and accessibility` alongside the first authenticated slices. Use /architect for any design reconciliation.

## Core player and match loop

Current authority for 5–11 is [Core player and match loop](../scope/core-player-match-loop.md).

### 5. Club reference data · existing · Beta

- **Intent and as built.** Admin settings manage seasons, competitions, selections with tracking levels, venues and opponents. Match/player workflows consume scoped reference data and preserve archived history. [Settings endpoints](../../backend/src/Api/Endpoints/Settings/SettingsEndpoints.cs), [team endpoints](../../backend/src/Api/Endpoints/Teams/TeamEndpoints.cs), [team initialization](../../backend/src/Infrastructure/Teams/TeamStartupInitializer.cs), and [settings UI](../../frontend/src/features/settings) use EF settings/team mappings. Migrations are `AddSettingsSeasonsAndCompetitions`, `AddTeamsSelections`, `RepairVenuesAndOpponentsSchema`, and the original `AddVenuesAndOpponents` source file. See U `Settings` and `Teams/TeamTests.cs`, plus I `Settings/VenueAndOpponentEndpointsTests.cs` and `Teams/TeamEndpointsTests.cs`.
- **Historical evidence.** Units 21, 22, 25 and 26 report CRUD, archive and seed tests, frontend gates, a local schema/history mismatch, and application of the repair migration.
- **Current verification and gaps.** Confirm fresh-schema and supported upgrade behavior, active selectors, uniqueness, archive/restore and historical references. **Migration risk:** the original venue/opponent migration declares an ID but has no DbContext attribute in source, whereas the earlier idempotent repair has both attributes and creates both tables. Record the IDs actually discovered/applied and establish the live schema before attributing any failure to migration order. The current checkout has no observed migration failure.
- **Next.** `/check verify club reference data`, carrying the foundation migration evidence forward.

### 6. Player records and selection history · existing · Beta

- **Intent and as built.** One persistent player with lifecycle status and dated team assignments. [Player endpoints](../../backend/src/Api/Endpoints/Players/PlayerEndpoints.cs), [application services](../../backend/src/Application/Players), [repositories and mappings](../../backend/src/Infrastructure/Players), and [player list and detail](../../frontend/src/features/players/players-page.tsx) wire scoped reads, admin writes and assignment conflict guidance. `20260711120000_AddPlayersBackendFoundation` precedes the effective migration ID `20260711120001_AddPlayerTeamAssignments`, despite the latter's source filename prefix `20260711095718`. See I `Players/PlayerEndpointsTests.cs`, U `Players/PlayerTests.cs` and `Players/PlayerTeamAssignmentTests.cs`, and FE `features/players/lifecycle.test.ts`.
- **Historical evidence.** Units 27–29 report domain and API checks, local migration work and frontend build gates. Unit 56 records real fixture creation and ending the assignment. Commit `a652516` added conflict handling and endpoint/UI helper coverage.
- **Current verification and gaps.** Verify one longitudinal record across past/current/future assignment dates, overlapping ranges, scoped reads, blocked lifecycle updates while assigned, and later deactivation/archive with preserved history. Verify both a clean migration history and a supported existing history where relevant. Player and assignment audit coverage is tracked under 11.
- **Next.** `/check verify player records and selection history`.

### 7. Match management · existing · Beta

- **Intent and as built.** Staff list and maintain team-scoped match context, opponent, venue, season/competition, kickoff, score and lifecycle. [Match endpoints](../../backend/src/Api/Endpoints/Matches/MatchEndpoints.cs), [application workflow](../../backend/src/Application/Matches), [EF repositories](../../backend/src/Infrastructure/Matches), and [match list](../../frontend/src/features/matches/matches-page.tsx) plus [dialogs](../../frontend/src/features/matches/components/match-dialogs.tsx) connect to `20260711153539_AddMatchesBackendFoundation`. U `Matches/MatchTests.cs` exercises domain rules.
- **Historical evidence.** Units 30/34 report builds and tests, the original local schema blocker later cleared by Unit 31's update, and a corrected archive-guard query. Unit 56 records local API-backed match fixture creation.
- **Current verification and gaps.** Check staff create/edit/search/archive/restore with validation, controlled references, two-team boundaries and report-state locks. Add focused real-provider coverage for filter/archive and workflow-lock queries if current evidence exposes a gap. No dedicated match CRUD integration suite was located. Audit omissions are recorded under 11.
- **Next.** `/check verify match management`.

### 8. Lineups and appearances · existing · Beta

- **Intent and as built.** Match-date eligibility, starters and substitutes, captain, appearances with minutes and ordered substitutions. [Participation domain](../../backend/src/Domain/Matches), [match endpoints](../../backend/src/Api/Endpoints/Matches/MatchEndpoints.cs), [lineup repository](../../backend/src/Infrastructure/Matches/MatchLineupRepository.cs), and [lineup tab](../../frontend/src/features/matches/components/lineup-tab.tsx) connect to `20260711184101_AddMatchLineupAppearances`. See U `Matches/MatchParticipationRulesTests.cs` and I `Matches/LineupEligiblePlayersEndpointsTests.cs`. The statistics service depends on appearance identity, and lineup mutation consults the report workflow guard.
- **Historical evidence.** Units 31/35 report migration application, domain and eligibility checks and frontend build gates. Unit 56 reports saving played-match lineup/appearance fixtures through the API.
- **Current verification and gaps.** Exercise preliminary and played snapshots, eligibility at date boundaries, duplicate/captain/substitution constraints, stable appearance IDs, removal cleanup and report-state locks. Existing focused endpoint tests cover candidate selection. Cover atomic writes, rollback and cleanup with PostgreSQL. Dialog discard confirmation exists, while navigation away from the page remains unverified.
- **Next.** `/check verify lineups and appearances`, with report-lock assertions repeated during 10.

### 9. Manual match statistics · in-progress · GA

- **Intent and as built.** Quickly enter player and goalkeeper fields by tracking level for actual appearances, with field feedback, conflict recovery and protection for unsaved work. [Statistics service](../../backend/src/Application/Matches/MatchStatisticsService.cs), [statistics repository](../../backend/src/Infrastructure/Matches/MatchStatisticsRepository.cs), [report endpoints](../../backend/src/Api/Endpoints/Matches/MatchReportEndpoints.cs), [statistics editor](../../frontend/src/features/matches/components/statistics-tab.tsx) and [payload helpers](../../frontend/src/features/matches/utils/statistics.ts) implement report-scoped reads and snapshot saves. Backend owns tracking profile, allowed values, submission completeness and report locks. `20260711213433_AddManualMatchStatistics` persists values bound to appearances. See U `Matches/MatchStatisticsTests.cs`.
- **Historical evidence.** Unit 33 records its local migration and 61 unit/28 integration tests. Unit 36 records frontend gates and an incomplete audit, with subsequent goalkeeper editing fixes. [Legacy Unit 36 audit and follow-ups](../legacy/context/progress-tracker.md#specification-audit--2026-07-12)
- **Current verification and gaps.** **Confirmed in source:** a save 409 ends editing, refetches and resets the form. One message covers both workflow locks and changed appearances, with no recovery of the discarded inputs. Validation feedback is a shared message and the editor has no route blocker for dirty edits. Current code handles null versus zero and explicit goalkeeper rows. Grouped headers and schema choice are historical design details to reassess only where needed for current acceptance. Prove BASIC/STANDARD/FULL, null/zero and goalkeeper entry, invalid relationships, concurrent changes, dirty tab/navigation and PostgreSQL snapshot integrity. No focused frontend parser/payload or statistics write integration suite was located.
- **Next.** Baseline `/check verify manual match statistics` after 8. /debug should reproduce draft-loss behavior and isolate its cause. /scope must confirm the remaining scoped work and whether /architect is needed for a recovery/navigation contract before /develop completes missing behavior. Follow with /check verify and /test. The current scope's build action is `/develop manual match statistics`.

### 10. Match report review · in-progress · GA

- **Intent and as built.** Operators submit, analysts review/request correction, permitted staff verify, and archive is terminal. Backend owns allowed actions, visibility and change locks for match metadata/lineup/statistics. [Report service](../../backend/src/Application/Matches/MatchReportsService.cs), [report repository](../../backend/src/Infrastructure/Matches/MatchReportsRepository.cs), [report queue](../../frontend/src/features/matches/match-reports-page.tsx) and [review tab](../../frontend/src/features/matches/components/report-review-tab.tsx) use `20260711194432_AddMatchReportWorkflow`. See U `Matches/MatchReportTests.cs`; I `DashboardEndpointsTests.cs` provides indirect final-only privacy evidence.
- **Historical evidence.** Units 32 and 37 report migration, transition tests, frontend checks, a report-queue EF translation fix, and live health/unauthenticated 401 after restart. The archived record leaves authenticated queue behavior incomplete.
- **Current verification and gaps.** The confirmation UI uses the shared Dialog, matching the documented historical Alert Dialog limitation. Its actual keyboard and focus behavior still needs verification. Exercise the role/permission matrix, scoped queue reads, state-specific actions, correction reason, archive, stale updates, and lock/unlock behavior with real PostgreSQL. No dedicated report transition endpoint suite was located.
- **Next.** `/check verify match report review` after 9, then /debug where observed and /test for transition, role, and query coverage.

### 11. Audit history · in-progress · GA

- **Intent and as built.** Important active mutations record actor, action, entity, time and safe meaningful changes. [Audit contracts](../../backend/src/Application/Auditing/AuditContracts.cs), [audit store](../../backend/src/Infrastructure/Auditing/AuditStore.cs), and [shared history UI](../../frontend/src/features/audit) implement append-only app writes and bounded, entity-specific reads through `20260712220859_AddAuditBackendFoundation`. Writers exist for staff, reports/statistics, media, imports, availability/injuries and training. The role/visibility rules sit with the owning endpoints.
- **Historical evidence.** Units 38/39 describe initial staff/report history. Unit 38 records a successful migration and 61 unit/29 integration tests. Later media, import and medical units extend audit coverage. Unit 56 records retaining audit history after fixture cleanup. These records establish partial historical coverage.
- **Current verification and gaps.** **Confirmed coverage omissions:** no audit writer in player-record/assignment, match metadata or lineup application/repository paths. Whether particular reference-data mutations are deemed important remains a scope question. **Confirmed payload defect:** `DisableAsync` records `ACTIVE` as the prior status even when disabling an invited non-admin. The Users UI offers disable for invited accounts. Verify the resulting history and route the defect through /debug. No dedicated audit integration suite was located. Check persistence and rollback, no-op/retry behavior, before/after fidelity, actor identity across account changes, pagination and denied access. Review sensitive field allowlists with the staff and medical owners. [Staff service](../../backend/src/Infrastructure/Staff/IdentityStaffUsersService.cs) · [Users UI](../../frontend/src/features/users/users-page.tsx)
- **Next.** Record the mutation/visibility matrix during 3 and 5–10, then run `/check verify audit history` with the existing media/import/medical writers as those slices are exercised. Reconcile missing coverage through /scope and use /architect only if the mutation or payload contract is unresolved. The scope's completion step is `/develop audit history`.

## Player analysis

Current authority for 12–15 is [Player analysis](../scope/player-analysis.md).

### 12. Player profile foundation · existing · Beta

- **Intent and as built.** One scoped record shows player identity, lifecycle, assignments, safe availability, permission-dependent medical shortcuts, and media. The [player detail page](../../frontend/src/features/players/players-page.tsx) combines [player API](../../frontend/src/features/players/api.ts), [medical API](../../frontend/src/features/medical/api.ts), [media links](../../frontend/src/features/media/media-links-section.tsx), and legacy physical workload data. Persistence is shared with 6, 16, 17 and 20. I `Players/PlayerEndpointsTests.cs`, I `Players/MedicalAvailabilityEndpointsTests.cs` and FE `features/players/lifecycle.test.ts` cover constituent behavior.
- **Historical evidence.** Units 29, 42, 49 and 51 document successive profile surfaces and automated checks. The Unit 56 viewer check exercised the Medical route and supplies partial privacy evidence.
- **Current verification and gaps.** Use the same player across accessible/inaccessible teams, media and availability states, and sessions with and without medical permission. Verify focus and cache behavior on a role change, plus profile composition and absent optional data. **Scope drift:** the workload section and its API client remain wired to dropped functionality. No focused profile composition test was located.
- **Next.** `/check verify player profile foundation` after 6 and the relevant 16/17 reads. Ask /scope to disposition existing workload exposure if cleanup is proposed.

### 13. Player performance history and trends · planned · Beta

- **Intent and current extent.** Provide match-by-match appearance and reviewed statistics history with time/competition filters and comparable trends. [Player details](../../frontend/src/features/players/players-page.tsx), [statistics persistence](../../backend/src/Infrastructure/Matches/MatchStatisticsRepository.cs), and [dashboard leaders](../../backend/src/Infrastructure/DashboardService.cs) are reusable inputs. No dedicated route, API, data model, migration, integration, or acceptance tests for this player history were located.
- **Historical evidence.** The legacy dashboard and player UI units describe related bounded reads. They supply no evidence of completed player history or trends.
- **Acceptance and gaps.** No feature runtime flow is available. Decide reviewed-data eligibility, per-appearance identity, metric comparability across tracking levels, and how filters affect results before writing a feature contract.
- **Next.** `/architect player performance history and trends` after 8–10 establish trustworthy source data.

### 14. Team dashboard overview · existing · GA

- **Intent and as built.** A team/season overview combines recent results, report workflow, safe availability, verified leaders and quality alerts. [Dashboard endpoints](../../backend/src/Api/Endpoints/DashboardEndpoints.cs), [read model service](../../backend/src/Infrastructure/DashboardService.cs), [client](../../frontend/src/features/dashboard/api.ts) and [dashboard page](../../frontend/src/pages/dashboard-page.tsx) use existing match/report/statistics/medical/workload tables. No dedicated migration. I `DashboardEndpointsTests.cs` covers context options, explicit context and final-only privacy.
- **Historical evidence.** Units 52/53 report backend integration and frontend build checks. Unit 56 captured authenticated dashboard shell behavior. The full authenticated multi-role data matrix is still owed.
- **Current verification and gaps.** Check one team/season response per allowed context, role and permissions, bounds/ties/tracking comparability, missing data, and refresh after writes. Verify live PostgreSQL aggregation and absence of restricted workflow/medical fields. **Scope drift:** the service still groups physical workload and the UI still renders it. Existing coverage leaves real-provider aggregations and full role/data combinations open.
- **Next.** `/check verify team dashboard overview` after 5–10 and 16. Route the workload disposition to /scope.

### 15. Opponent player analysis and match preparation · planned · Beta

- **Intent and current extent.** Maintain relevant opponent players in the context of upcoming matches. Team-level [opponent persistence](../../backend/src/Infrastructure/Settings/OpponentConfiguration.cs), [settings UI](../../frontend/src/features/settings), and [matches](../../backend/src/Api/Endpoints/Matches/MatchEndpoints.cs) provide the existing prerequisites. No opponent-player route, entity, persistence, migration, external integration, or feature-specific test was located.
- **Historical evidence.** Units 25/26 and 30/34 cover opponent teams and match reference selection. No verification of opponent-player analysis was found.
- **Acceptance and gaps.** No feature runtime flow is available. Set the preparation boundary, identity and match linkage, role access, and source of player data before implementation.
- **Next.** `/architect opponent player analysis and match preparation` after the first-team player history direction has been established.

## Supporting capabilities

Current authority for 16–18 is [Supporting capabilities](../scope/supporting-capabilities.md).

### 16. Availability and restricted medical details · existing · GA

- **Intent and as built.** Coach-safe team availability is separated from restricted injury details and history. [Medical endpoints](../../backend/src/Api/Endpoints/Players/MedicalEndpoints.cs), [medical mappings](../../backend/src/Infrastructure/Players/MedicalConfiguration.cs) and [Medical workspace](../../frontend/src/features/medical/medical-page.tsx) implement scoped access and revisions, including restricted query cleanup and zero cache retention on restricted queries. The player profile and dashboard consume bounded context. Migration `20260717140512_AddMedicalAvailabilityBackend` creates records/revisions. See U `Players/MedicalRecordsTests.cs` and I `Players/MedicalAvailabilityEndpointsTests.cs`.
- **Historical evidence.** Units 50/51 record migration application, a corrected PostgreSQL injury-list translation failure, and four focused endpoint tests. Unit 56 records a real viewer session with no injuries tab or restricted wording, including a query directly requesting that tab.
- **Current verification and gaps.** Verify assignment eligibility, record/edit/resolve and revision history, team separation, coach-safe payloads, and access lost while restricted data is active, including mounted dialogs, cached responses and route changes. **Architectural drift:** handlers use AppDbContext directly, as documented by the current backend guidance and spec 0001. Boundary changes should be considered only with evidence and behavior-preserving checks.
- **Next.** `/check verify availability and restricted medical details` after 3 and 6. Use /debug for observed privacy or behavior faults. Use /architect for a changed persistence boundary if evidence justifies it.

### 17. Media references · existing · GA

- **Intent and as built.** Team-scoped external references and uploaded files can be linked to players, matches and reports. [Media endpoints](../../backend/src/Api/Endpoints/Media/MediaEndpoints.cs), [media service](../../backend/src/Infrastructure/Media/MediaService.cs), [private storage](../../backend/src/Infrastructure/Files/LocalFileStorage.cs), [media workspace](../../frontend/src/features/media) and shared link sections implement stream, metadata, audit and workflow-aware linkage. Migrations include `AddFileStorageAbstraction`, `AddMediaBackendFoundation`, and `AddMediaLinksAndAssets`. I `Media/MediaSupportEndpointsTests.cs` covers capabilities, missing candidates, multipart creation and inline retrieval. U `Files/LocalFileStorageTests.cs` covers local storage operations.
- **Historical evidence.** Units 40–42 report applied migrations and successful upload/stream test evidence after multipart binding and optional query fixes. The archive also records repairs for absent media schema and provider translation in candidate searches.
- **Current verification and gaps.** With local storage, prove upload/cancel, stream vs attachment, external reference, attach/unlink/archive/restore and audit history. Cover denied team/role, workflow-locked reports, private paths and file cleanup after a persistence/link failure. Production object storage belongs to 19.
- **Next.** `/check verify media references` after 5–8 and with 10 for report links.

### 18. Generic import preview · in-progress · GA

- **Intent and as built.** Authorized staff retain source files and inspect bounded raw CSV/XLSX preview and history. [Import endpoints](../../backend/src/Api/Endpoints/Imports/ImportEndpoints.cs), [import workflow](../../backend/src/Infrastructure/Imports/ImportService.cs), [processor registration](../../backend/src/Infrastructure/DependencyInjection.cs), [tabular readers](../../backend/src/Infrastructure/Imports/TabularSourceReaders.cs), and [import workspace](../../frontend/src/features/imports/imports-page.tsx) connect storage, lease states, safe retrieval, capability-driven actions and audit. Registered CSV/XLSX fallbacks expose preview only. Migrations are `AddImportWorkflowBackendFoundation` and `AddImportPreviewMetadata`, sharing stored files with 17. U `ImportsTests.cs` and `TabularContractsTests.cs` cover workflow/domain and format-neutral contracts.
- **Historical evidence.** Units 43–45 record local migration application, backend suite counts up to 104 unit/32 integration, frontend gates and incomplete authenticated browser verification. Unit 45 adds preview capability after Unit 44's earlier no-processor state.
- **Current verification and gaps.** Use small synthetic files of both types, representative encoding/shape and parser limits, invalid files, scoped source access, history and cancellation, rollback/lease consistency, and safe download. Confirm allowed actions and persistence make validation and confirmation unavailable, and official statistics remain untouched. No dedicated authenticated import/parser integration suite was located. Retained upload/lease boundaries justify GA.
- **Next.** `/check verify generic import preview` after 3, 5 and the local storage check in 17. Include 7 when exercising match-targeted preview. Use /debug for observed failures and /test for reader/provider/browser gaps.

## Deferred and excluded

Current authority for 19–22 is [Deferred and excluded](../scope/deferred-and-excluded.md).

### 19. Production deployment · in-progress, needs a decision · GA

- **Intent and as built.** Application safeguards exist across [production configuration](../../backend/src/Api/Configuration), [API startup](../../backend/src/Api/Program.cs), [health endpoints](../../backend/src/Api/Endpoints/HealthEndpoints.cs), [file storage validation](../../backend/src/Infrastructure/Files/FileStorageOptionsValidator.cs), and [frontend environment rules](../../frontend/src/lib/env.ts). The app has production gatekeeping around origins, trusted proxies/HTTPS, key storage, rate limiting, liveness/readiness, and frontend API configuration. It uses the existing migration chain through an explicit command. Infrastructure registration supplies local filesystem storage, which the production validator disallows. I `Configuration/ProductionOptionsValidatorTests.cs`, I `Configuration/CorsConfigurationTests.cs`, and U `Files/FileStorageOptionsValidatorTests.cs` cover configuration behavior.
- **Historical evidence.** Unit 57 records application safeguards checked by build and automated suites. Operations records document unresolved hosting, storage, PostgreSQL, backup/recovery, email, monitoring, TLS and instance topology. [Legacy readiness checklist](../legacy/context/operations/production-readiness-checklist.md)
- **Current verification and gaps.** **BLOCKED** on approved operating decisions and a production storage adapter. No deployment, backup restore, ingress/cookie behavior, restart/key persistence, mail delivery or operational ownership was verified. Earlier automated readiness-option checks cannot establish those outcomes.
- **Next.** When the deferred feature is prioritized, `/architect production deployment`. Keep local verification of active staff workflows independent of the provider decision.

### 20. Training and physical workload · dropped · Beta classification

- **Intent and as built.** Historical training sessions, participant lifecycle and comparable revisioned physical metrics. [Training endpoints](../../backend/src/Api/Endpoints/Training/TrainingSessionEndpoints.cs), [physical endpoints](../../backend/src/Api/Endpoints/Training/PhysicalMetricEndpoints.cs), [physical workload writer](../../backend/src/Infrastructure/Physical/PhysicalWorkloadWriter.cs) and [training UI](../../frontend/src/features/training) still link to player/match/dashboard and import targets. `20260716190749_AddTrainingGpsBackendFoundation` adds sessions, participants and workload persistence. See U `Training/TrainingSessionTests.cs` and `Physical/PhysicalMetricCatalogTests.cs`.
- **Historical evidence.** Units 48/49 report migration and 109 unit/32 integration tests plus an API fixture exercise in Unit 56. The tracker leaves fake exact-processor atomic confirmation coverage open.
- **Current verification and gaps.** No dropped-feature acceptance run is queued. **Scope drift:** routes and cross-feature workload surfaces remain reachable in code. **Architectural drift:** training endpoints access AppDbContext directly. Neither warrants a standalone refactor during active-feature verification. Active surfaces should be checked for privacy and correct behavior with optional workload data.
- **Next.** Preserve dropped status. Include surviving exposure in /scope reconciliation only where it affects active features. Any change to that exposure needs explicitly scoped work.

### 21. Vendor data integrations · dropped · GA classification if ever reopened

- **Intent and current extent.** GPeXe/GPS, Zone14 and other vendor processing are excluded. Vendor labels remain in [import enums](../../backend/src/Domain/Imports/ImportEnums.cs) and the [import UI](../../frontend/src/features/imports/imports-page.tsx). The [processor registrations](../../backend/src/Infrastructure/DependencyInjection.cs) contain generic readers only. No exact vendor processor, integration-specific migration, or tested mapping was located. Existing import/workload tables support the earlier generic workflow.
- **Historical evidence.** Unit 46 was blocked on authentic samples or authoritative field definitions. Unit 45 establishes bounded generic raw preview. [Legacy Unit 46 mapping gate](../legacy/context/feature-specs/46-gpexe-sample-review-mapping-gate.md)
- **Current verification and gaps.** No vendor acceptance flow is queued. During 18, verify that a vendor label still produces generic preview capabilities and cannot confirm official data.
- **Next.** No build, mapping or sample collection scheduled. Use /scope only if the club changes this exclusion.

### 22. Video analysis · dropped · Beta classification

- **Intent and current extent.** Video analysis, automated event detection and camera integration are excluded. [Media endpoints](../../backend/src/Api/Endpoints/Media/MediaEndpoints.cs) and [media UI](../../frontend/src/features/media) implement file/reference handling. Inspection of registered routes, domain and persistence located no video-analysis pipeline, event model, migration, player-review UI, or dedicated tests.
- **Historical evidence.** Media Units 40–42 supply evidence of reference handling only. No historical delivery or verification evidence for video analysis was located.
- **Current verification and gaps.** No video-analysis acceptance flow is queued. Verify supported media linking and streaming under 17.
- **Next.** Preserve dropped status. A change in direction belongs to /scope.

## Dependency-aware work order

1. **Reconcile the baseline for verification.** Use this inventory in /scope replan to record the Beta recommendation and GA exceptions, confirm the first verification slice, and surface currently wired workload features. Preserve the planned/dropped boundaries and the current evidence state.
2. **Start with 1, then 2 and the shell portion of 4.** Use a disposable PostgreSQL database and private local storage root, appropriate Development API/SPA origins, and test-only staff credentials. Start from `backend/.env.example`, the API launch profile and `frontend/.env.example`. Align the frontend API URL to the actual launch profile. Verify explicit migration application and the history/schema entries for settings, venues/opponents, Identity and player assignments. Check /health/live, /health/ready and client-to-API/ProblemDetails wiring. Then exercise bootstrap, CSRF, first-password-change, protected refresh and logout. Recheck current shell focus and navigation from that session. Capture command output, HTTP status and safe response fields, schema queries and browser observations.
3. **Build the verification fixtures through existing staff workflows.** Exercise 3 to create disposable staff accounts with distinct roles and selected-team scope. Exercise 5 to establish two teams and active/historical reference records. Run 6 → 7 → 8 to create a player, dated assignment, played match, and eligible lineup/appearances. Preserve one known valid fixture and a distinct out-of-scope context for denial checks.
4. **Prove reviewed data.** Baseline 9 against those appearances, use /debug to reproduce the draft-loss behavior identified in source, and settle any design decisions before finishing the scoped work. Verify 10 with operators, analysts and users with/without verification permission. Check that correction reopens allowed edits and that verified/archived reports stay locked. Capture 11 audit observations at each mutation and exercise its history UI once enough records exist.
5. **Prove bounded read views and supporting flows.** Verify 16 with safe and restricted fixtures before 12/14 consume it. Verify 17 in the existing storage and player/match/report contexts. Verify 18 with synthetic files after local storage is exercised. Complete 12/14 with their dependencies and run the remaining 4 keyboard/mobile workflows as the representative records become available. Close 11's authorization, redaction and atomicity evidence across its owners.
6. **Advance only the scoped follow-ups.** Use /test for focused regressions and go beyond InMemory where the claim depends on PostgreSQL. Use GA closure for the high-risk slices when changes are made. If a reproducible fault or a scoped decision requires a refactor, capture before/after behavior with /check verify and keep the change narrow. Leave 13, then 15 for their own design decisions. Feature 19 waits for its deferred decision. Features 20–22 have no verification campaign.

**Exact next JSMastery workflow command after this assessment is reviewed**

`/scope`

Use bare /scope in replan mode, with this inventory as evidence. **First feature selected for runtime verification** is **1. Application foundation**. Its exact verification command following reconciliation is `/check verify application foundation`. The first unfinished staff workflow is feature 3, after features 1 and 2 supply a working environment and authenticated session. The manual-statistics build remains the next unfinished core build recorded by the current scope.
