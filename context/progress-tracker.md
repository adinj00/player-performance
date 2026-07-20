# Progress Tracker

## Unit 57: Production Configuration Readiness

- Status: complete for application production-readiness; **production deployment remains BLOCKED** pending required infrastructure/provider decisions.
- Added typed deployment, Data Protection, and rate-limit options; production-safe startup validation; Development-only `.env` loading; production key-ring persistence; exact split-origin CORS; trusted forwarded-header ordering; HTTPS/HSTS outside Development; health liveness/readiness checks; endpoint rate-limit policies; safe security headers; and frontend API-base validation for relative same-origin or absolute HTTPS production URLs.
- Added operations configuration, deployment, backup/restore, object-storage decision, and sign-off checklist documents. They explicitly preserve unresolved hosting, PostgreSQL, object-storage, email delivery, backup/RPO/RTO, monitoring, TLS/proxy, and single/multi-instance decisions; no provider or secret is assumed.
- No migration or provider SDK was added. `FileStorage__Provider=Local` remains Development-only, so deployment cannot be marked ready until an approved production adapter is implemented, health-checked, and integration-tested.
- Verification in this environment: isolated `dotnet restore`, zero-warning `dotnet build`, 112 unit tests, and 48 integration tests passed. `dotnet format ... --verify-no-changes`, frontend format check, lint (two pre-existing warnings only), and production build passed.

Update this file after every meaningful implementation change.

This file intentionally starts lightweight. It should become more detailed as build units are added under `/feature-specs` and implementation work begins.

## Current Phase

- Feature implementation kickoff

## Current Goal

- Unit 54: deferred. Unit 55: deferred. Unit 56: complete. Bosnian Latin remains the only implemented/selectable UI language; no localization package, user setting, or backend language change was added.
- Unit 56 implementation update: added protected-shell landmarks, a keyboard-visible skip link, a named primary nav, a focus-managed mobile navigation Sheet, and route title/focus management that runs only when `pathname` changes. Shared PageHeader heading focus targets and responsive actions, focused error summaries, semantic-token focus/reduced-motion behavior, a reusable named contained-table region, import upload progress announcements, dashboard chart descriptions/reduced motion, and safe availability-tab focus restoration after medical-detail permission loss are now in place. Generated shadcn primitives and all backend files remain unchanged.
- Unit 56 verification: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, and `npm.cmd run build` passed. Lint retains only the existing React Compiler compatibility warnings for TanStack Table in Medical and React Hook Form in Training. The audit documents browser evidence and approved MEDIUM follow-up for unavailable screen-reader/device environments and processor-capable workflow fixtures.
- Unit 56 public-route browser follow-up: an isolated Chrome profile verified sign-in at 320×568 and at 667×375 with forced high contrast and reduced motion after the local API/session check settled. No public-route clipping or horizontal overflow was found. Authenticated shell, workflow, privacy, and assistive-technology checks still require an authorized test staff session.
- Dashboard runtime fix: moved dashboard team/season URL normalization from render into an effect. This prevents `useQueryStates` from navigating/updating the active route while React is rendering `DashboardPage`, which was producing the cross-component `Cannot update a component (MatchesPage)` console warning.
- Unit 56 authenticated-verification blocker: the populated local database no longer accepts the configured bootstrap credential, and the sandboxed API cannot access its ASP.NET Data Protection key store for CSRF/session cookies. An isolated outside-sandbox API confirmed the application health path, but no authorized browser session can be created without resetting/changing a real staff account; that action was intentionally not taken. All temporary verification processes, browser profiles, screenshots, and logs were removed. Unit 56 requires temporary valid admin and non-medical staff credentials (or an explicitly supplied test session) before its remaining manual completion gate can be truthfully marked complete.
- Unit 56 authenticated-verification correction: the bootstrap account does authenticate when the UTF-8 `.env` file is parsed correctly. An isolated Chrome verification confirmed the protected dashboard landmarks/heading/skip link/named navigation at 320×568, modal mobile navigation title/close control, and route-heading focus after navigation to Matches. A temporary non-medical viewer was created and verified (`VIEWER`, no medical-detail permission), then disabled to preserve its audit history. The only remaining completion work is data-dependent manual workflows; creating the required player/match/import/training/injury fixtures would persist operational test records and needs explicit authorization before proceeding.
- Unit 56 medical-browser verification: the temporary viewer was reactivated, signed in through the real frontend, and confirmed as `VIEWER` with `canViewMedicalDetails=false`. The Medical route did not expose the injuries tab or restricted injury wording even when requested by query; the viewer was disabled again after the check.
- Unit 52: complete. Added `/api/dashboard/context-options` and `/api/dashboard/overview` with authenticated active-staff access, current team-scope filtering, archived-team exclusion, explicit team/season context, bounded recent/form (5), role-aware workflow and alert disclosure, current safe availability counts, final-report leaders (8 groups × 3 rows), database-side exact-comparability workloads (24 groups), and up to 10 stable alerts. Leader metrics respect each report's persisted tracking level; final-only workflow fields are omitted from JSON. No migration, frontend change, cache, snapshot, or background job was added. Added focused dashboard integration coverage for authentication, context options, required query context, final-only privacy, and restricted medical-field serialization. Verification: isolated zero-warning solution build; full backend suite (112 unit, 38 integration); focused dashboard suite (2 integration); both required whitespace formatter commands; and `git diff --check` passed. UI, advanced analytics, standings, ratings, caching, and export remain intentionally deferred.
- Unit 51: Medical Availability UI is complete. `/availability` provides a team-scoped, coach-safe availability summary, URL-backed filters/table/detail/audit, append-only availability editing, and a permission-gated restricted injuries workspace with date-eligible candidate search, create/update/resolve, immutable revisions, audit, pagination, and zero-retention restricted caching. Availability history and expected-return presentation follow the domain rules; player profiles expose safe per-team availability plus only permission-appropriate injury shortcuts and player-context create flows. No new migration was added.
- Unit 51 final verification: backend whitespace format and verify, isolated solution build, and the four focused `MedicalAvailabilityEndpointsTests` passed. Frontend Prettier format/check, lint (only the existing React compiler compatibility warnings for TanStack Table and the training form), production build, and `git diff --check` passed. The local source-output directory was locked by an existing process, so backend validation used an isolated `.artifacts-unit51` output directory; no source or runtime behavior was deferred.
- Local database remediation: applied migration `20260717140512_AddMedicalAvailabilityBackend` after the API reported missing `player_availabilities`; the migration created the availability and injury tables, revisions, and indexes successfully.
- Local injury-list remediation: replaced non-translatable `ReadScope` calls inside EF Core queries with SQL-translatable scoped-team filters; restarted the local API on port 5051 after successful build and focused integration tests.
- Unit 51 UI follow-up: filter triggers now render their selected labels, `Datum važenja` uses the shared calendar picker, and synthetic `Nepoznato` rows no longer open an empty availability-history Sheet. Frontend format/check and production build pass.
- Unit 51 availability-history follow-up: expected-return information is now presented only for `Ograničeno dostupan`, `Nedostupan`, and `Rehabilitacija`. `Dostupan` and `Nepoznato` show no expected-return row in revision history and a neutral dash in the table, matching the update form's domain rules. Frontend production build passes.
- Unit 50: Medical Availability Backend is complete. Added separate coach-safe availability and restricted injury domain models, append-only revision mappings, scoped endpoint surface, synthetic unknown reads, CSRF-protected mutations, and medical audit entity/action codes. Migration `20260717140512_AddMedicalAvailabilityBackend` was generated under `backend/src/Infrastructure/Persistence/Migrations/`; `dotnet ef database update` completed and reported the configured local database already up to date. Focused domain invariants plus safe-DTO privacy and medical-detail authorization integration coverage were added. Verification passed: whitespace format and verify, `dotnet build`, `dotnet test` (112 unit / 34 integration), and `git diff --check`.
- Unit 48: Training GPS Backend Foundation is in progress. The implementation is present, but the required fake exact-processor integration suite proving atomic workload/import/audit confirmation is still outstanding.

## In Progress

- Unit 48 implementation update: added `TrainingSession` (`PLANNED`/`COMPLETED`/`CANCELLED` lifecycle), immutable-team session metadata rules, participant history, and the nine specified canonical physical metrics. Added explicit training-participant/match-appearance workload context, append-only revisions, queryable metric values, threshold/method context, constraints, workload reads, and team-scoped training APIs. `ImportJob.TrainingSessionId` is a nullable non-cascading target relation and is carried safely through import creation/read contracts. The internal writer validates canonical values and defers commit to the surrounding import-confirmation transaction; confirmation commits processor writes, import completion, and audit atomically. No vendor processor was activated. Migration `20260716190749_AddTrainingGpsBackendFoundation` was generated and applied to local PostgreSQL. Verification passed: build, whitespace verification, `git diff --check`, 109 unit tests, and 32 integration tests. Required fake exact-processor atomic confirmation integration coverage remains outstanding.

- Unit 45: Generic CSV/XLSX parsing foundation is complete. Infrastructure pins `CsvHelper` 33.1.0 and `ExcelDataReader` 3.9.0 only; `ExcelDataReader.DataSet` is not referenced. Application owns provider-neutral tabular contracts, deterministic Unicode Form-C header normalization, conservative `EMPTY`/`TEXT`/`BOOLEAN`/`INTEGER`/`DECIMAL`/`DATE`/`DATETIME`/`MIXED` hints, and stable safe parser failure codes. Infrastructure provides forward CSV/XLSX readers, CSV UTF-8 (with/without BOM) and UTF-16 BOM handling, comma/semicolon/tab/pipe detection, OpenXML ZIP preflight, exactly-one-non-empty-worksheet handling, bounded preview storage, and generated temporary files in the configured private working directory for non-seekable sources. Generic fallbacks `generic-csv-preview` and `generic-xlsx-preview`, both version `1.0.0`, now provide preview-only capability for every stable import/source combination; exact processors take precedence. Generic preview never validates, confirms, maps vendor fields, or mutates official data.
  - Added `PreviewMetadataJson` (`jsonb`) on `ImportJob`, included as safe detail metadata, with clean EF migration `20260716184020_AddImportPreviewMetadata`. `backend/.env.example` documents configurable parser safety ceilings (header/delimiter scans, rows, columns, cells, XLSX ZIP limits, and working directory), not vendor requirements.
  - Verification passed: `dotnet build PlayerPerformance.sln --no-restore --no-incremental`; VSTest `dotnet test PlayerPerformance.sln --no-build` (104 unit tests and 32 integration tests); `dotnet format PlayerPerformance.sln whitespace --no-restore` and its `--verify-no-changes` variant; prohibited DataSet usage scan; and `dotnet ef database update`, which applied the migration to the configured local PostgreSQL database. No frontend files were changed.

- Unit 43 backend foundation: added centralized `PLAYER_ROSTER`, `MATCH_PLAYER_STATISTICS`, `MATCH_GPS`, and `TRAINING_GPS` types; `GENERIC`, `GPEXE`, `ZONE14`, and labeled `OTHER` sources; CSV/XLSX allowlisted extension/content-type pairs; and the `UPLOADED`/`PARSING`/validation/confirmation/terminal workflow aggregate. Import jobs uniquely own a Unit 40 `StoredFile`; source files are never media items and remain retained for every job status. Upload is metadata-first multipart, requires import-authorized admins or scoped data operators with `canImportData`, validates immutable team/match context, streams to Unit 40 storage, compensates storage on persistence failure, and does not process automatically.
  - Added `/api/imports` capabilities, upload, list/detail, attachment-only source download with `nosniff`, preview, validation-issues, preview/validate/confirm actions, cancellation, and per-job audit endpoints. DTOs do not expose storage keys or paths. Direct processing requests safely return a conflict while Unit 43 has no registered real processor; the provider-neutral registry and short lease/finalization orchestration support fake/test and future processors without CSV/XLSX packages, vendor mappings, workers, or official-data mutations.
  - Added typed `Imports__MaxUploadSizeBytes`, `Imports__PreviewRowLimit`, and `Imports__ProcessingLeaseTimeoutMinutes` configuration. The validator enforces the Unit 40 global storage ceiling; `.env.example` documents development defaults. Import audit entity type and safe lifecycle actions were added without storing source bytes, preview rows, or issue data in audit payloads.
  - Generated and applied EF migration `20260715233148_AddImportWorkflowBackendFoundation` under `backend/src/Infrastructure/Persistence/Migrations/`; it creates `import_jobs`, `import_preview_columns`, `import_preview_rows`, and `import_validation_issues` with unique stored-file ownership, restrictive foreign keys, `jsonb` structured fields, and query indexes. `dotnet ef database update` succeeded against the configured local database (EF tools reported only the existing 8.0.0-tools/8.0.10-runtime version advisory).
  - Follow-up completion: confirmation can acquire a lease only from `READY_TO_CONFIRM`; stale `PARSING` leases can be replaced while active leases cannot; preview row counts are persisted; and a source written above the import-specific limit is compensated before returning failure. Added focused domain workflow tests covering target/source invariants, active/stale leases, stale finalization rejection, validation, explicit confirmation, terminal behavior, and cancellation. Final verification passed: `dotnet format PlayerPerformance.sln whitespace --no-restore`, its `--verify-no-changes` variant, `dotnet build PlayerPerformance.sln --no-restore`, and VSTest `dotnet test PlayerPerformance.sln --no-build` (92 unit, 32 integration).

- Unit 42 media UI foundation is complete: `/media` is protected and available through the `Medijateka` navigation item, with URL-driven server-side filters, pagination, table states, and a URL-addressed media detail Sheet. It consumes Unit 41 media APIs, has centralized media display/content helpers, safe external-reference creation, authorized uploaded-content preview/open/download, and runtime `/api/media/capabilities`. Upload uses metadata-first `XMLHttpRequest` multipart, existing CSRF credentials, server-driven type/size validation, progress, and cancellation. The media list supports authoritative match/report/player linked-media queries; player detail and the match `Mediji` tab render those groups. The detail Sheet provides metadata editing, archive/restore confirmation, and candidate-backed match/report/player linking for users with mutation scope. Attachment cards provide role-aware unlink controls. Contextual create-and-attach uses explicit create then link requests and preserves the created media on link failure. Focused endpoint tests for capabilities and link-candidate missing-media behavior pass (2 tests). No Unit 42 migration was added.
  - Follow-up: applied the existing Unit 41 media migrations (`20260713063758_AddMediaBackendFoundation` and `20260713071719_AddMediaLinksAndAssets`) to the configured local database after the media library exposed PostgreSQL `42P01` for the missing `media_items` relation. Media filters and creation dialogs now render team/category/source labels instead of GUIDs or internal enum values. Frontend build and lint pass.
  - UI follow-up: media search and its selection/category/source controls now use the same shared `FilterSelect` behavior and responsive widths as the Users page: one row on desktop and one control per row on mobile. Upload failures now display the safe backend ProblemDetails message instead of always reporting a file-format/size issue.
  - Empty-state follow-up: when active media-library filters return no records, the page now uses the same `Nema rezultata za odabrane filtere` state and `Poništi filtere` action as Utakmice and Izvještaji. It resets only media filters and pagination, preserving any selected-media or contextual-attachment URL state.
  - Authorization follow-up: uploaded-file and external-reference creation team lists are intersected with the signed-in data operator's mutation scope (or all active teams for admins), preventing a user from selecting an unauthorized team and receiving a media `403`.
  - Upload runtime fix: multipart upload metadata now uses the API web JSON conventions (camel-case fields and string enums), and the endpoint no longer consumes the file stream while validating the multipart shape. This fixes the false team-access `403`, the subsequent generic file error, and prevents malformed metadata from becoming a `500`. An admin upload integration test now covers the full active-team multipart flow.
  - Content runtime fix: the optional `download` query parameter on `GET /api/media/{mediaId}/content` now defaults to `false`. Inline image previews and `Otvori` therefore reach the handler without requiring a query value; the upload integration test verifies the created image is returned as `200 image/png`.
  - Detail-sheet UX follow-up: media actions are grouped by intent. Match/report/player attachment actions form one labelled responsive row, metadata editing is separate, and the administrator-only archive/restore lifecycle action is isolated under media status. Frontend format, lint, and build pass.
  - Link/archive UX follow-up: attachment dialogs now use their actual target name, show loading/error/empty states, and explain that player candidates must be assigned to the media selection. The media filter bar now includes the administrator-only `Prikaži arhivirane` control used on Matches. Archive/restore invalidates both list and detail queries and closes its confirmation, so the current Sheet immediately shows the correct restore action. Frontend lint and build pass.
  - Link feedback follow-up: successful linking now closes the candidate dialog with an explicit success toast and marks the Sheet's player action as `Dodatni igrač`, reflecting that one player is already linked while still allowing the supported many-player attachment. Failed linking now surfaces the backend ProblemDetails reason. Frontend lint and build pass.
  - Candidate-cache follow-up: removed the misleading `Dodatni igrač` label. After a successful attachment the matching candidate-query cache is removed, so reopening the same target selector immediately fetches authoritative candidates and never offers the just-linked player again. Frontend lint and build pass.
  - Unlink confirmation follow-up: replaced the browser-native `window.confirm` with the application’s shadcn Dialog confirmation, including Bosnian cancel/destructive action labels and pending-mutation protection. Frontend lint and build pass.
  - Match/report candidate follow-up: match and match-report candidate searches now keep filtering in PostgreSQL and format kickoff dates only after materialization, avoiding a provider translation failure in the attachment dialog. Empty states explain the respective workflow constraints. Backend whitespace formatting and 3 focused media integration tests pass; the local API was restarted and health-checked.

- Unit 41 media backend foundation:
  - Added the shared `MediaItem` aggregate with immutable team/source type, `UPLOADED_FILE`/`EXTERNAL_REFERENCE` source types, `VIDEO`/`IMAGE`/`DOCUMENT`/`OTHER` categories, bounded shared metadata, and explicit active/archive lifecycle metadata.
  - Added one-to-one `MediaAsset`/`ExternalMediaReference` persistence model, safe HTTP(S)-only external URL validation (including credential rejection), team-scoped catalog/detail reads, external-reference creation, metadata correction, and administrator-only archive/restore operations.
  - Added `MEDIA_ITEM` audit entity support and created/updated/archived/restored semantic actions. Audit payloads intentionally contain only safe metadata and never storage keys or raw file data.
  - Added `Media__MaxUploadSizeBytes` validation against Unit 40 storage limits (with a safe 512 MiB default) and migration `20260713063758_AddMediaBackendFoundation` generated through `dotnet ef`.
  - Architecture documentation now explicitly describes `StoredFile` as storage metadata only, with owning-module foreign keys instead of generic linked-entity columns.
  - Remaining before completion: streamed multipart asset creation/content delivery, explicit match/report/player link histories and workflow locks, media audit-history endpoint, focused tests, database update and full formatting/test verification.

- Unit 38 audit backend foundation:
  - Added the append-only `AuditLog` domain entity, centralized stable `STAFF_USER` and `MATCH_REPORT` entity types, and initial staff/report/statistics action codes.
  - Added `audit_logs` EF mapping with PostgreSQL `jsonb` payload columns, non-cascading actor FK, and entity/actor/action history indexes. The migration is not yet generated because the local API process is locking the API build outputs needed by EF tooling.
  - Added safe structured audit contracts, writer/history repository, deterministic newest-first bounded history queries, and admin-only staff audit plus report-scoped audit routes. Audit payloads do not include invitation tokens, passwords, hashes, or Identity security metadata.
  - Integrated initial staff invitation, reissue, acceptance, profile, access, disable/reactivate audit writes; report creation/workflow transitions; and report-level statistics-save audit writes through the shared DbContext.
  - Generated `20260712220859_AddAuditBackendFoundation` under `backend/src/Infrastructure/Persistence/Migrations/` and applied it successfully to the configured local PostgreSQL database after stopping the locked API process.
  - Verification passed: solution build (zero warnings/errors), 61 unit tests, 29 integration tests, `dotnet format ... whitespace --no-restore`, `dotnet format ... --verify-no-changes`, and `git diff --check`.
  - Intentionally deferred: audit scopes for imports, medical, media, players, match metadata/lineups, GPS, login-email recovery, exports, and notifications; no global audit browser or mutation endpoints were added.

## Completed

- Unit 41 implementation update (pending its focused integration suite): streamed uploaded-media creation and authenticated content streaming are implemented through Unit 40; explicit match, report, and player link lifecycle records, workflow-aware mutation guards, media audit history, and link candidates are implemented. Backend regression verification passed with 87 unit and 29 integration tests; the generated media-link migration is present. The Unit 42 frontend remains intentionally removed until Unit 41’s focused endpoint coverage and final database update are completed.

- Unit 40 completed:
  - Added `IFileStorage`, opaque server-side key generation/validation, filename normalization, safe storage result categories, and explicit compensation-delete support. Upload streams remain caller-owned; read streams are caller-disposed.
  - Added the Development-only `LocalFileStorage` adapter with contained path resolution, asynchronous streaming to internal temporary files, actual-byte counting, zero/oversize rejection, no overwrite, cleanup, idempotent delete, and no paths/URLs exposed through the contract. The isolated `Testing` environment is permitted only for the test host's temporary storage root.
  - Added metadata-only `StoredFile` persistence with archive metadata, strict user foreign keys, unique storage keys, positive-size/archive-consistency constraints, and EF migration `20260712232513_AddFileStorageAbstraction`; `dotnet ef database update` applied it successfully to local PostgreSQL.
  - Added safe Local development configuration examples and Git ignore protection. No public storage endpoints, static-file exposure, media/import records, provider SDKs, or frontend changes were introduced.
  - Unit 40 was re-audited against its feature specification on 2026-07-13. Added configuration protection against roots under `wwwroot` or `frontend/public`, plus focused tests for declared-size rejection, cancellation cleanup, missing-object reads, and Development/Production/public-root options validation.
  - Verification passed: whitespace formatting and verification, zero-warning solution build, 87 unit tests, 29 integration tests, and `git diff --check`. Storage tests use unique system temporary roots; the test factory configuration is per-host to avoid process-environment races. Local filesystem symlink traversal remains platform-dependent and is not intentionally supported; the adapter never creates symlinks and retains normalized-path containment checks.

- Unit 28 completed:
  - Added the persistent `PlayerTeamAssignment` model with immutable player/team identity, required `DateOnly` start date, optional inclusive `DateOnly` end date, and UTC created/updated timestamps. Current/upcoming/past timing state is derived from the approved clock and is never persisted.
  - Added admin-only create and end routes beneath `/api/players/{playerId}/assignments`; assignment history reads are available to authenticated active staff only when the player is readable under the canonical team scope. There is no assignment delete, reopen, team-change, or generic update route.
  - Active players can hold simultaneous assignments to different teams. Application-level overlap validation rejects inclusive overlapping periods for the same player/team, including duplicate open-ended assignments; adjacent periods and later returns are supported. Only active teams may receive assignments.
  - Player deactivate/archive now return a safe conflict while the player has a current assignment. Assignment mutations remain audit-ready application use cases; audit persistence is deferred to Unit 38 and UI work is deferred to Unit 29.
  - Player reads now use assignment-aware scope rules: `ADMIN` reads all, `ALL_TEAMS` staff read all lifecycle-allowed players, and `SELECTED_TEAMS` staff read only players with a current assignment in scope. List reads accept an optional database-side `teamId` current-assignment filter and include compact current-assignment summaries without duplicate player rows.
  - Added assignment persistence configuration, history-safe foreign keys, supporting indexes, a unique filtered open-assignment index, and the EF-generated focused `20260711095718_AddPlayerTeamAssignments` migration. After the local API process was stopped, EF migration generation and `dotnet ef database update` completed successfully against the configured PostgreSQL development database; the migration contains only the assignment table, foreign keys, and related indexes.
  - Added focused domain and integration coverage for assignment date/end behavior, overlap conflicts, assignment list/end flow, and updated player read authorization behavior. Verification passed: restore, whitespace format/check, zero-warning build, and tests (43 unit, 28 integration). `git diff --check` passed.

- Unit 27 completed:
  - Added the persistent club-level `Player` aggregate with only first name, last name, optional preferred name, optional `DateOnly` date of birth, `PlayerRecordStatus`, and UTC created/updated timestamps. There is no team/current-selection foreign key or other future player metadata.
  - Added explicit create/profile-update/lifecycle use cases with trimmed Unicode-safe names, future-date rejection, ACTIVE creation, archive/restore behavior (restore returns INACTIVE), and archived-record update/activation/deactivation conflicts.
  - Added admin-only, authenticated, password-change-gated `/api/players` list, detail, create, update, activate, deactivate, archive, and restore routes. Non-administrators receive `403`; no hard-delete route exists.
  - Added database-side filtering, deterministic last-name/first-name/id ordering, paging metadata, archived-default exclusion, status/search support, EF configuration, and the focused `20260711120000_AddPlayersBackendFoundation` migration. Team assignment history remains deferred to Unit 28, non-admin team-scoped player access remains deferred until those assignments exist, and player mutation audit persistence remains deferred to Unit 38.
  - Added domain and integration coverage for name normalization, date validation, lifecycle constraints, authorization, duplicate names, filtering, archive/restore, archived update conflicts, validation, and missing/delete routes. The API `.http` file contains player request examples.
  - Verification passed in isolated outputs: `dotnet restore backend/PlayerPerformance.sln`, `dotnet build backend/PlayerPerformance.sln --no-restore` with zero warnings/errors, `dotnet test backend/PlayerPerformance.sln --no-build` (40 unit and 27 integration tests), repository whitespace formatting/check, and `git diff --check`.
  - After the authorized API process was stopped, EF migration discovery showed `20260711120000_AddPlayersBackendFoundation (Pending)` and `dotnet ef database update` applied it successfully to the configured PostgreSQL development database. The EF CLI reported only the existing tools/runtime version warning (`8.0.0` tools vs `8.0.10` runtime).

- Unit 26 completed:
  - Replaced the settings placeholder with administrator-only, nested routes for seasons, competitions, teams/selections, venues, and opponents; `/settings` redirects to seasons and the sidebar hides `Postavke` for non-administrators.
  - Added typed, CSRF-capable settings API clients and TanStack Query hooks for the existing Unit 21, Unit 22, and Unit 25 contracts. Archived visibility is URL-backed with `archived=include`; server ordering is preserved.
  - Added reusable settings navigation, RHF/Zod forms, lifecycle confirmations, table states, centralized Bosnian Latin team/tracking-level labels, date-only season formatting, and accessible move-up/move-down ordering controls that submit the complete non-archived team ID list.
  - No packages, shadcn primitives, backend files, or API contracts were added or changed.
  - Verification passed: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint` (two pre-existing/non-blocking React Compiler compatibility warnings, including React Hook Form usage in this unit), `npm.cmd run build`, and `git diff --check`.

- Settings list endpoint correction:
  - Made the `includeArchived` query value optional with a `false` default for seasons, competitions, venues, opponents, and teams. This matches the documented default-list contract and the frontend behavior, which only sends `includeArchived=true` when archived visibility is selected.
  - Configured an explicit invalid enum sentinel for `StaffAccessProfile.TeamScopeType`, removing EF Core's generated-default warning while preserving explicit `ALL_TEAMS` and `SELECTED_TEAMS` values and the existing database default.
  - Verification passed: isolated `dotnet restore`, `dotnet build PlayerPerformance.sln --no-restore`, and `dotnet test PlayerPerformance.sln --no-restore --no-build` (36 unit and 25 integration tests), `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes`, and `git diff --check`.
  - Replaced the archived-record native checkbox with the established shadcn checkbox primitive.
  - Added and applied `20260711082629_RepairVenuesAndOpponentsSchema`, an idempotent repair migration that creates the Unit 25 `venues` and `opponents` tables plus normalized-name indexes only when missing. It repairs local databases where the original Unit 25 migration was present in history but its schema was absent.
  - Follow-up verification passed: backend whitespace check, isolated restore/build/test (36 unit and 25 integration tests), frontend formatting/format check/lint/build, and `git diff --check`. Frontend lint retains its two non-blocking React Compiler compatibility warnings.
  - Removed duplicate create actions from settings empty states; each resource now keeps its single persistent create action in the top-right page controls.

- Unit 25 implementation is ready for final EF migration verification:
  - Added separate `Venue` and `Opponent` aggregates with only the approved name, normalized-name, archive, and UTC timestamp fields. Both use the canonical settings normalization and soft archive/restore lifecycle; archived records cannot be normally updated.
  - Extended the existing Settings application slice, repository, EF Core model, and admin-only/password-change-gated `/api/settings/venues` and `/api/settings/opponents` endpoints. Lists sort by normalized name then identifier, exclude archived records by default, and accept `includeArchived=true`.
  - Added a focused `20260711110000_AddVenuesAndOpponents` migration that creates only `venues` and `opponents`, including required names, archive/timestamp columns, and unique normalized-name indexes. No seed records, match relationships, deletion endpoints, or speculative metadata were added.
  - Added domain and integration coverage for lifecycle behavior, administrator authorization, CSRF-protected mutations, independent venue/opponent namespaces, and archived-list filtering. The API `.http` file now documents every new route.

- Unit 22 completed:
  - Added the canonical `Team` aggregate with server-owned normalized names, display order, and UTC timestamps; `TeamTrackingLevel` is fixed to `BASIC`, `STANDARD`, and `FULL`, while `TeamStatus` is fixed to `ACTIVE`, `INACTIVE`, and `ARCHIVED`.
  - Added admin-only, password-change-gated `/api/settings/teams` list, get, create, update, reorder, activate, deactivate, archive, and restore operations. Team names are trimmed and normalized with the established deterministic rule; the database unique index protects names across all lifecycle states and duplicate names return safe `409` responses.
  - Added contiguous non-archived ordering, complete-list reorder validation, archive normalization, and restore append behavior. Restored teams always return as `INACTIVE`; administrators explicitly activate them when appropriate.
  - Added the `20260710143906_AddTeamsSelections` migration, string-persisted tracking/status enums, non-negative display-order constraint, relevant indexes, and idempotent startup seeding. An empty teams table seeds `First Team` (`FULL`), `U19`/`U17` (`STANDARD`), and `U15`/`U13`/`U11` (`BASIC`) in that order; populated tables are not supplemented or reset.
  - Added domain and integration coverage for aggregate lifecycle rules, default seeding, admin authorization, CSRF-protected mutations, and archived-list filtering. Staff team-scope assignments intentionally remain deferred to Unit 23.

- Unit 21 completed:
  - Added `Season` (`Id`, display/normalized name, `DateOnly` start/end dates, archive flag, and UTC created/updated timestamps) and `Competition` (the same fields except dates) as EF-free Domain entities with explicit update, archive, and restore behavior.
  - Added focused Application settings use cases and FluentValidation contracts for list, get, create, update, archive, and restore. Names are trimmed and normalized with `Trim().ToUpperInvariant()`; unique normalized-name indexes remain the final concurrency authority while provider unique violations return a safe `409 duplicate_name` response.
  - Added admin-only, password-change-gated routes: `GET/POST /api/settings/seasons`, `GET/PATCH /api/settings/seasons/{id}`, `POST /api/settings/seasons/{id}/archive`, `POST /api/settings/seasons/{id}/restore`, plus the equivalent `/api/settings/competitions` routes. Lists exclude archived records by default and accept `includeArchived=true`; archive/restore are idempotent and archived records cannot be normally updated.
  - Added Infrastructure mappings, settings repository, and the `20260710131639_AddSettingsSeasonsAndCompetitions` migration. It creates only `seasons` and `competitions`, with `date` season columns, archive defaults, server-owned timestamps, and unique normalized-name indexes; no seed data or unrelated settings entities were added.
  - Added `FluentValidation.DependencyInjectionExtensions` 11.11.0 and focused domain/validator tests for date range, lifecycle idempotency, normalization, blank values, and maximum length.

- Unit 20 completed:
  - Added the canonical `StaffRole` model with `ADMIN`, `DATA_OPERATOR`, `ANALYST`, `COACH`, `MEDICAL_STAFF`, and `VIEWER`, plus explicit `CanVerifyReports`, `CanImportData`, and `CanViewMedicalDetails` permissions.
  - Added the authoritative one-to-one `staff_access_profiles` persistence model keyed by the existing Identity user ID. Roles persist as their stable uppercase string values and non-admin permission flags default to `false`.
  - Added the Application-safe `ICurrentUserAccess` abstraction and a scoped, request-cached Infrastructure resolver. It reads current persisted account/profile data, treats unavailable or profile-less users as non-authorized, and leaves an extension point for Unit 23 team scope.
  - Registered centralized `AdminOnly`, `CanVerifyReports`, `CanImportData`, and `CanViewMedicalDetails` policies. `ADMIN` satisfies all policies; non-admin users require the matching persisted flag; unauthenticated callers remain `401` and insufficient/profile-less callers receive `403`.
  - Added the idempotent first-admin role handoff after the existing bootstrap operation. It uses the configured bootstrap email when available, otherwise permits the documented single-user/no-profile fallback, and fails safely on ambiguous fallback state. It does not alter credentials, account status, lockout state, or password-change state.
  - Enriched authenticated `GET /api/auth/session` users with `primaryRole` and effective `permissions` (`canVerifyReports`, `canImportData`, `canViewMedicalDetails`). No team IDs, persistence identifiers beyond the existing compatible user ID, or Identity security metadata are exposed.
  - Added the `AddStaffAccessProfiles` and `AddStaffAccessProfileUserForeignKey` EF Core migrations and verified them against the configured local PostgreSQL database. The access profile has a unique one-to-one foreign-key reference to the Identity user. Selected-team scope remains intentionally deferred to Unit 23.

- Unit 19 completed:
  - Connected the React Hook Form and Zod sign-in form to `POST /api/auth/login` and added the protected `/change-password` form for `POST /api/auth/change-password`.
  - Confirmed the frontend auth contract: login and password change return `SessionResponse`; session is read from `GET /api/auth/session`; unsafe requests first acquire `GET /api/auth/csrf` and send its returned request token in the `X-CSRF-TOKEN` header while the API-issued `XSRF-TOKEN` cookie is included automatically; logout remains `POST /api/auth/logout` with the same CSRF convention.
  - Kept session server state in TanStack Query. Login and password-change mutations replace the cache with the direct response, invalidate it, then refetch the session before navigation. Logout replaces the cache with the unauthenticated shape only after backend success.
  - Added explicit route decisions for loading, session error, unauthenticated, required-password-change, and completed-password-change states. Safe return paths are internal-only, reject protocol-relative/backslash paths, and reject authentication-loop routes.
  - Added a central `password_change_required` ProblemDetails signal that invalidates session state and routes to `/change-password` without retrying the rejected request.
  - Kept error messages safe and Bosnian Latin, avoided browser storage for session/CSRF/credentials, and retained honest unavailable forgot/reset-password surfaces.
  - Added accessible show/hide controls for every password input, using an icon button with Bosnian Latin accessible labels and no change to password storage or submission behavior.
  - Corrected CSRF wiring to use the backend-issued antiforgery request token rather than the companion cookie value, preventing the `400 Invalid CSRF token` login failure. Added an opt-out for shared-button press translation and applied it only to password visibility controls, then corrected the unavailable-password-flow link to render as a native link rather than a Base UI button.

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
- Unit 12 completed:
  - Added EF Core, EF Core design-time, Npgsql, and EF Core DbContext health-check package references only to `backend/src/Infrastructure`.
  - Added `Microsoft.EntityFrameworkCore.Design` to `backend/src/Api` as a design-time-only dependency so `dotnet ef` can use the API startup project reliably.
  - Added Infrastructure-owned persistence setup with `AppDbContext`, PostgreSQL `DbContext` registration, and a required `ConnectionStrings:DefaultConnection` startup check.
  - Added a database readiness health check exposed through `/health/ready` while preserving the existing safe `/health` endpoint behavior.
  - Added an initial empty persistence baseline migration and model snapshot under `backend/src/Infrastructure/Persistence/` without introducing speculative business tables.
  - Updated backend startup logging to a safe explicit provider set so unhealthy database readiness checks do not crash on Windows Event Log permission issues in local environments.
  - Updated `backend/.env.example` with a placeholder `ConnectionStrings__DefaultConnection` value following the existing local configuration convention.
- Unit 13 completed:
  - Added shared backend primitives under `backend/src/Domain/Common/` for entity identity, value-based equality, developer-facing error objects, result wrappers, and universal guard helpers.
  - Added framework-independent audit metadata contracts under `backend/src/Domain/Abstractions/Auditing/` without introducing audit entities, persistence hooks, or schema changes.
  - Added an application-facing `ISystemClock` abstraction and an Infrastructure `SystemClock` UTC implementation registered through the existing Infrastructure dependency injection extension point.
  - Added focused unit tests covering entity equality, value object equality, result invariants, guard helpers, and clock registration/UTC behavior.
  - Kept the unit product-agnostic: no FK Velež entities, API routes, auth behavior, migrations, or frontend changes were introduced.

- Unit 14 completed:
  - Added a framework-independent `UserAccountStatus` enum in `backend/src/Domain/Users/` with `INVITED`, `ACTIVE`, `DISABLED`, and `LOCKED`.
  - Added an Infrastructure-owned `ApplicationUser` Identity model with account status, password-change requirement, and created/updated timestamps using Guid identifiers.
  - Integrated the existing `AppDbContext` with user-only ASP.NET Core Identity persistence and stable staff-only table names: `staff_users`, `staff_user_claims`, `staff_user_logins`, and `staff_user_tokens`.
  - Registered IdentityCore, EF stores, token providers, and a custom sign-in manager hook that blocks future sign-in for disabled or locked accounts.
  - Added API-owned cookie authentication and authorization wiring with explicit password/lockout options, `RequireUniqueEmail`, `HttpOnly` cookies, explicit `SameSite=Lax`, environment-sensitive secure-cookie policy, and non-redirecting `401`/`403` behavior for API callers.
  - Generated the `AddIdentityFoundation` EF Core migration without adding roles, user-management tables, seed users, or any auth/session endpoints.
  - Added focused tests for the new account-status enum, Identity service resolution, and cookie-auth security behavior while preserving the existing health endpoint and architecture coverage.
  - Follow-up cleanup: moved EF Core migration artifacts and the model snapshot into `backend/src/Infrastructure/Persistence/Migrations/` so the `Persistence` area separates the DbContext from generated migration files more clearly.

- Unit 15 completed:
  - Added API-owned antiforgery configuration with a stable `X-CSRF-TOKEN` request header and explicit cookie behavior for the staff cookie-auth flow.
  - Added `GET /api/auth/csrf` to mint antiforgery tokens for browser clients and return the stable CSRF header name in a minimal safe response.
  - Added `GET /api/auth/session` with a stable `200 OK` unauthenticated payload and a minimal authenticated-session payload that excludes Identity internals, roles, team scopes, and permission data.
  - Added `POST /api/auth/logout` as an authenticated API endpoint that validates CSRF before clearing the auth cookie and returns no HTML redirects.
  - Upgraded cookie-auth challenge/forbid behavior from bare status codes to API-friendly ProblemDetails responses for `401` and `403`.
  - Added a reusable API helper pattern for future unsafe cookie-authenticated endpoints to validate CSRF without enabling global CSRF checks on safe endpoints.
  - Added integration coverage for the unauthenticated session path, CSRF bootstrap endpoint, logout unauthorized behavior, antiforgery option wiring, and the new auth ProblemDetails responses.

- Unit 16 completed:
  - Added a frontend `auth` feature area with session types, auth API helpers, a query-owned `useSession()` hook, and a logout mutation built on the existing shared API client.
  - Added public auth route shells for `sign-in`, `forgot-password`, and `reset-password`, keeping all visible copy in Bosnian Latin and clearly marking login/password-reset actions as unavailable until backend endpoints exist.
  - Split routing into public auth routes and protected app-shell routes so existing dashboard/module placeholders now require a valid backend session.
  - Added protected-route loading, unauthenticated redirect, and safe session-error handling without introducing fake users, local token storage, role logic, or permission logic.
  - Replaced the topbar placeholder with a session-aware user menu that shows the authenticated email when present and performs CSRF-protected logout.

- Unit 17 completed:
  - Added typed Infrastructure-owned `Bootstrap:FirstAdmin` configuration for first-admin bootstrap, with placeholder-only values in `backend/.env.example`.
  - Added startup bootstrap wiring that checks the persisted Identity user store first, exits without side effects when any user exists, and otherwise fails safely when bootstrap is disabled or incomplete.
  - Added first-user creation through `UserManager`, which applies the configured Identity normalization and password policy; the created account is `ACTIVE`, requires a password change, and receives clock-based audit timestamps.
  - Kept the flow backend-only: no public setup route, login/reset/invitation endpoints, frontend changes, roles, team scopes, permissions, or sample data were added.
  - Bootstrap configuration is skipped explicitly in the automated `Testing` environment so existing API integration tests remain isolated from persistence requirements.
  - Added focused tests for empty-store bootstrap configuration validation and safe validation messages. Full database-backed bootstrap behavior requires manual empty-database verification with a real configured PostgreSQL instance.
  - The temporary password is never committed, logged, returned, or displayed; it must be supplied by local or production environment configuration.

- Unit 18 completed:
  - Added Application-layer login, current-session, logout, and current-user password-change contracts, implemented by the Infrastructure Identity adapter without exposing Identity types outside Infrastructure.
  - Added `POST /api/auth/login` and `POST /api/auth/change-password`. Successful login and password change return the safe session contract: `isAuthenticated` and `user` with `id`, `email`, `accountStatus`, and `mustChangePassword`.
  - Extended `GET /api/auth/csrf` to return the request token alongside the stable `X-CSRF-TOKEN` header name, enabling validated unsafe requests.
  - Added centralized middleware that returns `403` ProblemDetails with code `password_change_required` for authenticated users whose `RequiresPasswordChange` flag is set, while allowing session, CSRF, change-password, logout, and anonymous health endpoints.
  - Login uses Identity normalization and lockout-on-failure. Invited, disabled, explicitly locked, and Identity-locked accounts cannot receive an auth cookie. Successful password changes use `UserManager.ChangePasswordAsync`, clear `RequiresPasswordChange` only after success, and refresh the sign-in cookie.
  - Added focused integration coverage using an isolated EF Core in-memory test host and a test-environment-only protected route; no production demo endpoint was added. The official first-admin role handoff remains Unit 20.

## In Progress

- Unit 23: Staff Users, Team Scope, and Account Lifecycle Backend is in progress. `TeamScopeType` contains `ALL_TEAMS` and `SELECTED_TEAMS`; the authoritative staff profile now stores display name and scope type, while `staff_team_scopes` uses a composite user/team key with restricted foreign keys and a team lookup index. Migration `20260710152754_AddStaffUsersTeamScopeLifecycle` was generated and applied locally.
  - Initial staff endpoints are mapped at `/api/users` (list, detail, invitation creation/reissue, profile/access update, disable, reactivate) and anonymous setup acceptance is mapped at `/api/auth/invitations/accept`. Staff responses expose only safe profile/access/scope/lifecycle fields; setup credentials are URL-safe and returned only from create/reissue.
  - The current session response now includes effective `teamScope`; administrators report `ALL_TEAMS` with no selected IDs. A reusable server-side team-access service fails closed for unavailable or malformed current access.
  - Invitation acceptance uses the Identity token provider, one-time security-stamp invalidation on reissue/acceptance, and does not sign users in. Disable invalidates the security stamp; reactivation preserves profile/scope and selects `ACTIVE` or `INVITED` from password presence. The final-active-admin safeguard is applied to demotion and disable.
  - Verification: isolated `dotnet build PlayerPerformance.sln --no-restore` passed with zero warnings/errors; isolated `dotnet test PlayerPerformance.sln --no-build --no-restore` passed (34 unit, 24 integration). The migration was applied to the configured local PostgreSQL development database. Focused Unit 23 integration and lifecycle/team-scope test coverage is still pending before this unit can be marked complete.

## Next Up

- Start the next scoped feature spec on top of the shared UI primitive baseline.
- Build the first real frontend feature or backend login flow on top of the new auth/session foundation.
- Build the next backend foundation unit on top of the new configuration-ready, testable persistence baseline.
- Build the next backend module on top of the new shared primitives and persistence baseline.
- Build the next auth-focused backend unit on top of the new Identity persistence, CSRF/session foundation, auth utility endpoints, and frontend auth shell.

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
- Backend Identity persistence uses a user-only ASP.NET Core Identity setup on the shared `AppDbContext`, with staff-only table naming and no role tables in the foundation unit.
- Frontend uses React, Vite, TypeScript, Tailwind CSS v4, shadcn/ui, TanStack Query, Zustand for limited global UI state, Zod, React Hook Form, and shadcn/Recharts charts. React Router owns routing; current Players and Users filters use local React state, with no nuqs dependency.
- The UI is light-only and follows FK Velež red, white, and gold identity.
- Bosnian Latin is the default UI language; English may be supported as an optional selectable UI language.
- Local development uses `backend/.env` and `frontend/.env.local`; production uses real environment variables or a managed secret store.
- Docker is out of scope for the initial local development setup and may be introduced later.
- Exact Gpexe and Zone14 import mappings must not be guessed before real export samples are reviewed.
- Backend cookie authentication uses the Identity application scheme, `HttpOnly` cookies, `SameSite=Lax`, production `SecurePolicy=Always`, development `SecurePolicy=SameAsRequest`, and API-friendly non-redirecting unauthorized/forbidden responses.
- Backend auth utility endpoints use `GET /api/auth/csrf`, `GET /api/auth/session`, and `POST /api/auth/logout` as the stable initial cookie-auth API surface.
- First-admin bootstrap is a single-instance startup operation that only creates an account when the persisted user store is empty. A multi-instance deployment will need a distributed lock or equivalent database-safe coordination if concurrent initial startup becomes a supported deployment mode.
- Unit 20 will assign or migrate the bootstrap-created account into the official `ADMIN` role when the staff role and team-scope model is introduced.
- The frontend-facing CSRF request header name is `X-CSRF-TOKEN`; the browser-readable CSRF token cookie currently uses `XSRF-TOKEN`.
- Frontend auth routing now treats `/sign-in`, `/forgot-password`, and `/reset-password` as public routes, while the existing app-shell routes require a successful session query before rendering.
- Frontend session state is owned by TanStack Query through the auth feature hook and is not duplicated in Zustand or browser storage.

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
- Unit 12 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts` passed after allowing network access for the new EF Core/PostgreSQL packages.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts` passed.
  - `backend`: integration tests now verify `/health` still returns the safe service response and `/health/ready` returns a safe unhealthy response when the configured database is unreachable.
  - `backend`: the repository's default backend `obj/bin` paths were locally locked in this environment, so verification used the .NET SDK `--artifacts-path` option to keep restore/build/test isolated from those locked outputs.
  - `backend`: the baseline migration files were added manually in EF Core format because the local EF CLI metadata path was blocked by the same locked default build-output issue; the migration files compile successfully in the verified solution build.
  - `backend`: follow-up local verification succeeded after stopping the running API process, rebuilding normally, and running `dotnet ef database update --project src/Infrastructure/PlayerPerformance.Infrastructure.csproj --startup-project src/Api/PlayerPerformance.Api.csproj`.
  - `backend`: the EF update created the `player_performance` database, created `__EFMigrationsHistory`, and applied the `20260710120000_InitialPersistenceBaseline` migration.
- Unit 13 verification results:
  - `backend`: creating the new shared source/test folders required an escalated directory-creation step because the sandbox denied creating nested directories inside the workspace.
  - `backend`: default restore/build paths under the repository `obj/bin` folders remained locked in this environment, so verification again used `--artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit13`.
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit13` passed after allowing temporary NuGet network access.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit13` passed.
  - `backend`: an initial parallel `dotnet test --no-build` run failed because it raced the build outputs in the shared artifacts directory, so the test step was rerun sequentially.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit13` passed with 18 unit tests and 3 integration tests.
  - `frontend`: no frontend files were changed for Unit 13, so no frontend verification commands were required.
- Unit 14 verification results:
  - `backend`: creating the new Domain/Infrastructure/test folders for the Identity foundation required an escalated directory-creation step because the sandbox denied nested folder creation inside the workspace.
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit14` passed after allowing temporary NuGet network access for the added Identity EF Core package.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit14` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit14` passed with 19 unit tests and 6 integration tests.
  - `backend`: `dotnet ef migrations add AddIdentityFoundation --project src/Infrastructure/PlayerPerformance.Infrastructure.csproj --startup-project src/Api/PlayerPerformance.Api.csproj --context PlayerPerformance.Infrastructure.Persistence.AppDbContext --output-dir Persistence` succeeded after allowing normal workspace build-output access for EF tooling.
  - `backend`: `dotnet ef database update --project src/Infrastructure/PlayerPerformance.Infrastructure.csproj --startup-project src/Api/PlayerPerformance.Api.csproj` succeeded after allowing normal workspace build-output access for EF tooling and applied the `20260709232300_AddIdentityFoundation` migration to the local database.
  - `backend`: an intermediate generated migration was removed and recreated so the final schema also enforces unique normalized email at the database level.
  - `backend`: the local EF tools reported that version `8.0.0` is older than the runtime `8.0.10`, but migration generation still completed successfully and the warning did not block verification.
  - `backend`: default repository `obj` paths remain sensitive in this environment, so normal solution verification continued to use `--artifacts-path` even though `dotnet ef` needed temporary unrestricted workspace output access.
  - `backend`: follow-up cleanup created `backend/src/Infrastructure/Persistence/Migrations/` and moved the generated migration files plus `AppDbContextModelSnapshot` there; solution verification still passed afterward.
  - `frontend`: no frontend files were changed for Unit 14, so no frontend verification commands were required.
- Unit 15 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit15` initially failed in the sandbox because NuGet network access was blocked, then passed after allowing temporary network access.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit15` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit15` passed with 19 unit tests and 10 integration tests.
  - `backend`: antiforgery token generation in the integration host required explicit ephemeral data-protection registration in `TestApplicationFactory` so CSRF issuance tests remain stable without changing runtime behavior.
  - `backend`: the test environment now uses `CookieSecurePolicy.SameAsRequest` for auth and antiforgery cookies so the integration host can exercise cookie behavior safely while production remains `SecurePolicy=Always`.
  - `backend`: authenticated logout with a missing or invalid CSRF token is implemented through the shared validation helper, but full end-to-end integration coverage for that path is still deferred because the repository does not yet have a test-auth sign-in helper that can establish a real cookie-authenticated session.
  - `frontend`: no frontend files were changed for Unit 15, so no frontend verification commands were required.
- Unit 16 verification results:
  - `frontend`: `npm.cmd run format` passed.
  - `frontend`: `npm.cmd run format:check` passed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
  - `frontend`: protected routing now depends on the backend session endpoint and redirects unauthenticated requests toward `/sign-in` without introducing fake auth state or browser token storage.
  - `frontend`: the sign-in, forgot-password, and reset-password pages are intentionally honest shells only; they do not submit credentials or call non-existent backend endpoints.
  - `frontend`: browser-level manual verification of a successful authenticated session and logout round-trip is still limited because the repository does not yet expose a real login/first-admin bootstrap flow for creating a staff session interactively.
- Local CORS bugfix verification results:
  - `backend`: the browser session error was traced to missing API CORS middleware even though `GET http://localhost:5051/api/auth/session` returned a healthy unauthenticated payload directly.
  - `backend`: added a configured-frontend CORS policy that allows the documented frontend origin and credentials for the cookie-auth API surface.
  - `backend`: added an integration test that verifies a preflight request for `/api/auth/session` returns `204 No Content` with `Access-Control-Allow-Origin` and `Access-Control-Allow-Credentials`.
  - `backend`: `dotnet build PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-cors-fix` passed after allowing temporary NuGet network access.
  - `backend`: `dotnet test PlayerPerformance.sln --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-cors-fix` passed with 19 unit tests and 11 integration tests.
- Local auth-page scrollbar bugfix verification results:
  - `frontend`: the public auth routes were producing unnecessary page scroll because the auth shell combined viewport-height sizing with vertical padding.
  - `frontend`: fixed the auth shell sizing so the public auth layout no longer adds height beyond the viewport.
  - `frontend`: `npm.cmd run format` passed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
- Local auth sign-in layout bugfix verification results:
  - `frontend`: the sign-in shell originally rendered the email and password placeholders side by side, which was not an appropriate default layout for this auth page.
  - `frontend`: updated the sign-in shell to use a standard vertical field stack.
  - `frontend`: corrected the progress note for the earlier scrollbar fix so it no longer attributes the issue to a body-margin change that was not part of the final fix.
  - `frontend`: `npm.cmd run format` passed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
  - `frontend`: `npm.cmd run format` passed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.
- Local auth shared-shell width refinement verification results:
  - `frontend`: finalized the shared auth shell at `max-w-xl`, keeping the header and auth card at a focused single-column width without a sign-in-specific override.
  - `frontend`: retained the standard vertical sign-in field stack.
  - `frontend`: `npm.cmd run format:check` passed.
  - `frontend`: `npm.cmd run lint` passed.
  - `frontend`: `npm.cmd run build` passed.

- Unit 17 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit17` initially failed because sandbox network access to NuGet was blocked, then passed after temporary NuGet network access was allowed.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit17` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit17` passed with 25 unit tests and 11 integration tests.
  - Manual follow-up: start the API against an empty local PostgreSQL database with `Bootstrap__FirstAdmin__Enabled=true`, a real email, and a password that meets the configured Identity requirements; confirm exactly one active user is created with `RequiresPasswordChange=true`, then restart with changed bootstrap values and confirm the user is untouched.

- Unit 18 verification results:
  - `backend`: added `Microsoft.EntityFrameworkCore.InMemory` to the integration-test project only so cookie/Identity flows can be verified without developer-specific PostgreSQL credentials or Testcontainers.
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit18` passed after temporary NuGet network access was allowed.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit18` passed with zero warnings and zero errors.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit18` passed with 25 unit tests and 17 integration tests.

- Unit 19 verification results:
  - `frontend`: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, and `npm.cmd run build` passed.
  - `backend`: `dotnet test PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit19` passed with 25 unit tests and 17 integration tests.
  - Manual browser end-to-end verification was not run in this environment because it requires a configured local API, PostgreSQL database, bootstrap admin credentials, and cookie-capable browser session. The backend integration suite verifies the login, CSRF, required-password-change, password-change, session, and logout contracts used by the frontend.

- Unit 20 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit20` passed after temporary NuGet network access was allowed.
  - `backend`: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit20` passed with zero warnings and zero errors.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --no-build --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit20` passed with 25 unit tests and 22 integration tests.
  - `backend`: generated `20260710124807_AddStaffAccessProfiles` and `20260710125149_AddStaffAccessProfileUserForeignKey` with EF Core tooling and applied them successfully using `dotnet ef database update`; the local development database now contains `staff_access_profiles` with its Identity-user foreign key.
  - `backend`: test-host-only policy probes verify unauthenticated `401`, missing-profile/non-admin `403`, matching permission access, all `ADMIN` overrides, safe session enrichment, and first-admin handoff idempotency/ambiguity behavior. No production demonstration endpoint was added.

- Unit 21 verification results:
  - `backend`: `dotnet restore PlayerPerformance.sln` passed after NuGet access was allowed; `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit21` passed with zero warnings and errors.
  - `backend`: `dotnet test PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit21` passed with 30 unit tests and 22 integration tests.
  - `backend`: generated and applied `20260710131639_AddSettingsSeasonsAndCompetitions` with EF Core tooling against the configured local PostgreSQL database. The migration was generated after a stale no-build attempt; the local development migration history also contains the harmless empty `20260710131358_AddSeasonsAndCompetitions` record from that attempt, while the source-controlled migration set contains only the intended Unit 21 migration.

## Confirmed Decisions

- Unit 25 preserves venue and opponent names across archive states; archived names remain reserved by their respective unique normalized-name indexes. Venue and opponent namespaces are independent, so the same display name is valid once in each table.

## Known Blockers

- Unit 30: `dotnet ef database update` reached the local PostgreSQL database but could not apply because its migration history is out of sync with existing tables (`competitions` already exists while EF attempts an earlier migration). The focused match migration was generated through EF tooling, but EF's snapshot rollback caused it to include existing tables; it was replaced with the equivalent focused match-only migration. This tooling exception must be resolved against the local database history before applying the migration there.

- Unit 25: `dotnet ef migrations add AddVenuesAndOpponents` could not run because the developer's running `PlayerPerformance.Api` process (PID 2564) locks the normal API build outputs. The focused migration was prepared from the verified EF model instead. The isolated restore, whitespace formatting/check, zero-warning build, and full test suite pass; generating/applying the migration with EF tooling remains the final local verification step once that process is stopped.

## Unit 30: Matches Backend Foundation

- Status: complete, with the local database-update blocker documented above.
- Added the persistent `Match` aggregate with immutable team selection, `HOME`/`AWAY`/`NEUTRAL` context, explicit `SCHEDULED`/`PLAYED`/`POSTPONED`/`CANCELLED` transitions, paired non-negative final-score invariants, and archive/restore lifecycle.
- Added team-scope-aware match list/detail and create/update/archive/restore APIs. Reads are server-scoped; admins have full access, data operators can mutate only their scope, and archive/restore remain admin-only. Creation and updates validate active references, season date range, exact active duplicates, and immutable team identity.
- Added EF mapping, restrictive historical foreign keys, query indexes, the focused `20260711153539_AddMatchesBackendFoundation` migration, API request examples, and match domain coverage.
- Verification passed: isolated restore; whitespace format and verify; zero-warning solution build; 47 unit and 28 integration tests; `git diff --check`. `dotnet ef database update` could not complete only because the existing local database migration history is inconsistent with its already-present settings tables.

## Unit 24: Staff Users and Roles UI

- Status: complete.
- Added protected `/users` and public `/accept-invitation` routes. The users page is gated by resolved `ADMIN` session role and the sidebar hides the navigation item until an administrator session is known.
- Staff filters are URL-backed with `nuqs`: `q`, `role`, `status`, `scope`, and `team`.
- Added real staff/team API wrappers, TanStack Query mutations, role/scope/permission mappings, invitation/reissue one-time setup links built from `window.location.origin`, and transient invitation credential state. Setup query values are captured in component memory and removed from the public invitation URL.
- Self-disable clears session and returns to sign-in; self-access replacement invalidates the session before route re-evaluation.
- Added `@tanstack/react-table`, `nuqs`, and generated shadcn primitives: table, badge, select, checkbox, dropdown-menu, input, and label. The dialog generator could not add dialog/alert-dialog without overwriting the existing generated Button, so accessible feature-level modal semantics are used instead.
- Verification: frontend formatting, build, format check passed. Lint passed except its known TanStack Table React Compiler compatibility warning; manual end-to-end verification requires local API, PostgreSQL, and a cookie-capable browser session.
- Deferred: email delivery, user deletion, login-email recovery, manual unlock, audit UI, localization infrastructure, and unrelated modules.

## Unit 29: Players UI Foundation

- Status: complete.
- Replaced the players placeholder with protected `/players` and `/players/:playerId` views backed by the implemented Unit 27/28 player and assignment APIs.
- Added feature-owned player API contracts, TanStack Query cache keys, server-side URL-backed search/status/team/page filtering, 300 ms search debounce, table pagination, and explicit loading, empty, filtered-empty, and error states.
- Added admin-only create, edit, lifecycle, assignment-create, and assignment-end flows. Non-admin users can view only the player data returned by the API; mutation failures, including assignment conflicts, render safe Bosnian Latin feedback.
- Player details show supported identity data, lifecycle status, all current selection badges, and backend-ordered assignment history including past, current, and future timing states.
- Verification: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, and `npm.cmd run build` passed. Lint retains only existing React Compiler compatibility warnings for TanStack Table and React Hook Form. Manual browser verification requires the configured API, PostgreSQL database, and cookie-capable authenticated session.
- The shadcn registry/docs request was blocked by the environment network policy. Existing generated Dialog, table, badge, select, dropdown, field, and common loading/empty/error components were reused; no generated primitives were modified.
- Follow-up UI refinement: removed the duplicate empty-state player creation button, added a filtered-result reset action to Users, and replaced every native date input in settings, player, and assignment forms with the shared shadcn Calendar/Popover date picker.
- Date-picker follow-up: aligned the shared picker with the controlled Popover/Calendar pattern (including close-on-select and default month), removed the unused `date-fns` dependency, and fixed the assignment selection trigger to render the selected team value.
- Assignment selection follow-up: resolved the remaining GUID display by explicitly mapping the selected team id back to its team name in the trigger.
- Calendar installation follow-up: replaced the staged calendar workaround with the standard `npx shadcn@latest add calendar` result in `frontend/src/components/ui/calendar.tsx`; the CLI was explicitly answered `No` when it asked to overwrite the existing Button component. The generated component was formatted only to meet the repository formatter convention.
- Players list URL-state follow-up: replaced React Router `useSearchParams` with `nuqs` for the `q`, `status`, `team`, and `page` filters. The search debounce now skips identical URL values, preventing repeated URL updates and the observed loading loop when search text is deleted.
- Players search follow-up: aligned the nuqs debounce lifecycle with the proven Users-page pattern by storing the query setter in a ref. URL updates no longer recreate the debounce effect; only a user text change schedules a query update.
- Players search final follow-up: removed nuqs from the Players list at user request and adopted the working Users-page `useSearchParams`/`useCallback`/ref-backed debounce implementation verbatim in structure.
- Search UX follow-up: removed debounce effects from both Players and Users after the same typing failure appeared in both. Both pages now use direct, per-input-event nuqs updates for their URL-backed filters; no search effect can reschedule requests.
- Search stability follow-up: standardized both Players and Users on local text entry with URL/server search committed only on blur or Enter. This keeps router navigation and server-query state outside the keystroke path while preserving shareable nuqs filters.
- Search architecture finalization: both Players and Users now bind input/URL state directly through nuqs and debounce only the value supplied to TanStack Query/API calls via the shared `useDebouncedValue` hook. The app retains `nuqs/adapters/react-router/v7` because routing is owned by React Router v7 rather than the framework-agnostic React adapter.
- Frontend page organization standardization: moved the Users implementation into `features/users/users-page.tsx`, added the feature barrel export, and reduced `pages/users-page.tsx` to the same thin route adapter used by Players.
- Nuqs adapter correction: official nuqs documentation confirms that this `BrowserRouter`/React Router v7 app must use `nuqs/adapters/react-router/v7` inside router context; the generic Vite adapter is only for router-less React SPAs. Restored the React Router adapter, with immediate input/URL state and API-only debounce.
- Filter freeze isolation: removed nuqs, its adapter/package, and all debounce code from the frontend at user request. Players and Users now use plain local React filter state and feed TanStack Query directly, ensuring input/select changes do not depend on router history integration. Also removed the unused empty-column TanStack Table instances and package highlighted by the external review.
- Context synchronization: updated architecture, code standards, UI context, workflow rules, and the current frontend stack note to document the removal of nuqs and debounce from Players/Users.
- Filter UI standardization: added the shared `components/common/filter-select.tsx` composed from shadcn Select primitives. Users and Players now share the same menu-label, clear-option, selected-label, and empty-label behavior (`Sve`, `Sve selekcije`, `Sve uloge`, and related labels).
- Filter trigger refinement: empty filters now show their field label in the closed trigger (`Status`, `Selekcija`, etc.); the clear option remains localized as `Sve`/`Sve selekcije` inside the opened menu, while selected values show their actual label.

- Backend formatting baseline: the root `.editorconfig` defines four-space C# indentation, multiline braces/statements, and whitespace conventions. Backend implementation work must run `dotnet format PlayerPerformance.sln whitespace --no-restore` followed by its `--verify-no-changes` check before final build/test verification. A repository-wide backend whitespace formatting pass was completed; it introduced no behavioral changes.
- Future EF Core migration creation and database update work should use `dotnet ef` tooling by default instead of handwritten migration files whenever the local environment supports the CLI workflow.
- Future EF Core migrations should be generated into `backend/src/Infrastructure/Persistence/Migrations/` using `--output-dir Persistence/Migrations` so the `AppDbContext` remains separated from generated migration artifacts.
- When a backend task requires a new EF Core migration, the preferred verification flow is to run both `dotnet ef migrations add ... --output-dir Persistence/Migrations` and `dotnet ef database update` unless the environment prevents it or the task explicitly says otherwise.

## Unit 31: Match Lineup and Appearance Backend

- Status: complete.
- Added the match-lineup, lineup-entry, appearance, and ordered-substitution domain model. Played-match validation enforces captain, unique lineup/appearance identities, non-negative minutes, and ordered on-field transitions while allowing return substitutions and without fixed squad or minute-total rules.
- Added team-scope-authorized `GET` and atomic `PUT /api/matches/{matchId}/lineup` endpoints. The save operation supports preliminary scheduled/postponed lineups, validates new-player assignment eligibility at the match date, rejects archived/cancelled mutation, and retains appearance IDs when the same player remains in the appearance set.
- Added PostgreSQL mappings and EF-generated `20260711184101_AddMatchLineupAppearances` migration with restrictive historical player foreign keys and required match/player and substitution-sequence uniqueness constraints.
- Verification passed: isolated restore; zero-warning solution build; 52 unit and 28 integration tests; whitespace formatting and verify; `git diff --check`; and `dotnet ef database update`, which applied both the Unit 30 match migration and the Unit 31 lineup/appearance migration to the configured local PostgreSQL database.

## Unit 32: Match Report Workflow Backend

- Status: complete.
- Added the persistent `MatchReport` aggregate and explicit `DRAFT`, `READY_FOR_REVIEW`, `VERIFIED`, `NEEDS_CORRECTION`, and terminal `ARCHIVED` transitions. It records creation, latest submission, verification, correction-request reason/metadata, and archive metadata.
- Added report create, detail, filtered/paginated list, submit, verify, request-correction, and archive endpoints. Authorization, team scope, verification permission, report visibility, and backend-owned `allowedActions` are enforced in the Application layer; no generic status mutation or report delete endpoint was added.
- Added shared workflow guards so `READY_FOR_REVIEW`, `VERIFIED`, and `ARCHIVED` reports lock match metadata and lineup mutations with a workflow-specific `409` response; correction re-enables normal authorized edits. Match archive now rejects active report workflows and permits archive after the report is archived.
- Added EF Core report mapping and the EF-generated `20260711194432_AddMatchReportWorkflow` migration with one-report-per-match enforcement, restrictive match foreign key, and workflow indexes.
- Verification passed: isolated restore; whitespace format and verify; zero-warning solution build; 56 unit and 28 integration tests; `git diff --check`; and `dotnet ef database update`, which applied the Unit 32 migration to the configured local PostgreSQL database. The EF CLI noted its installed `8.0.0` tools are older than the `8.0.10` runtime, but migration generation and application completed successfully.

## Unit 33: Manual Match Statistics Backend

- Status: complete.
- Added persistent `PlayerMatchStats` and `GoalkeeperMatchStats` records bound to concrete appearances, with restrictive report/appearance foreign keys and unique per-appearance constraints. The report now freezes `AppliedTrackingLevel` on its first statistics save or submission readiness check.
- Added centralized `BASIC`/`STANDARD`/`FULL` statistics profiles, tracking-level-aware read and atomic snapshot save endpoints at `/api/match-reports/{reportId}/statistics`, nullable edit values, disabled-field rejection, non-negative and cross-field validation, and goalkeeper snapshot replacement behavior.
- Statistics reads honor report visibility and team scope. Writes require an admin or in-scope data operator and are locked outside `DRAFT`/`NEEDS_CORRECTION`. Submission now requires complete enabled player fields for every appearance and at least one complete goalkeeper row.
- Lineup removals clean up linked player and goalkeeper statistics within the same shared persistence transaction before the appearance is removed.
- Added focused statistics domain/profile coverage. Verification passed with isolated artifacts: `dotnet build PlayerPerformance.sln --no-restore --artifacts-path C:\Users\Jugo\AppData\Local\Temp\player-performance-artifacts-unit33` (zero warnings/errors), `dotnet test ... --no-build` (61 unit and 28 integration tests), and `git diff --check`.
- EF tooling follow-up: the source-tree intermediate files remained locked by a running process, so the same approved `dotnet ef migrations add AddManualMatchStatistics --output-dir Persistence/Migrations` command was run against an isolated copy of the backend. It succeeded and the generated migration, designer, and updated `AppDbContextModelSnapshot` were copied back into the repository. `dotnet ef database update --no-build` then applied `20260711213433_AddManualMatchStatistics` successfully to the configured local PostgreSQL database.

## Unit 34: Matches UI Foundation

- Status: complete, with the explicitly requested deferred create-form experience noted below.
- Added the protected `/matches` and `/matches/:matchId` route foundation, feature-owned typed match API client, TanStack Query list/detail calls, and `nuqs` URL state for status, date, archive visibility, and page filters.
- Replaced the Matches placeholder with a server-paginated TanStack Table, localized match/location labels, loading, empty, and query-error states. The detail page provides the required metadata-only overview shell and future-section placeholders without fetching later-unit data.
- Restored the explicitly required `nuqs` and `@tanstack/react-table` dependencies, added generated shadcn `Alert`, `Empty`, `Skeleton`, `Sonner`, and `Tabs` primitives, and configured the router adapter and toast provider.
- Current limitation: full settings selector filters, React Hook Form/Zod create/edit dialogs, field-level mutation error mapping, and Alert Dialog-confirmed archive/restore are not yet implemented. This work must be completed before Unit 34 can be considered done.
- Verification so far: `npm.cmd run format` and `npm.cmd run build` passed. The Vite bundle-size warning remains informational.
- Backend query-fix follow-up: corrected the match-list repository query after the frontend exposed an EF Core translation failure. Match filters, including archive visibility, now apply to the `Match` entity query before the joined projection is created; this preserves server-side filtering and pagination without client evaluation. Isolated restore, zero-warning build, and the full backend suite passed (61 unit and 28 integration tests). The developer must restart the currently running API process to load the rebuilt repository assembly.
- UX consistency follow-up: aligned the active Matches list with the Players/Settings visual baseline: shared `PageHeader`, card-based filter bar, `FilterSelect` controls, shared Popover/Calendar date pickers, Settings-style archive checkbox, standard table header treatment, and `Poništi filtere` copy. `context/ui-context.md` now explicitly requires this shared workflow baseline for comparable routes.
- Date-format follow-up: added a shared `date-fns` formatting utility. All currently rendered date-only values in Settings and Players, plus picker labels, now display as `dd.MM.yyyy.`; match date-times display as `dd.MM.yyyy. HH:mm`. Transport values remain ISO/UTC, including explicit UTC start/end-of-day boundaries for Matches date filters.
- Settings navigation refinement: replaced the five route-link controls with controlled shadcn Tabs that retain the existing nested routes and browser navigation. Removed the redundant Matches-specific date-time formatting wrapper; the feature utility now owns only match domain labels and date/time conversion, while the shared formatter owns display formatting.
- Responsive-navigation and date-range follow-up: Settings now uses a compact Select on mobile and tabs from tablet width upward, avoiding wrapped or scrollable navigation. The shared DatePicker accepts disabled-date rules; Matches date filters prevent selecting an invalid range and defensively omit a malformed range from the request.
- Mobile filter refinement: Players, Users, and Matches filter panels no longer use horizontal scrolling or mobile flex wrapping. They use full-width stacked controls on phones, with compact flex layouts retained from the desktop breakpoint upward; search inputs occupy the first row naturally.
- Completion follow-up: added the real React Hook Form + Zod match edit dialog using live active season, competition, opponent, and venue data; it preserves the immutable team context, converts the local date/time to UTC, constrains status transitions, requires both scores for played matches, maps field/API errors, and refreshes the specific match plus list queries after mutation. Edit visibility is limited to administrators and in-scope data operators. Archive/restore is administrator-only, uses an explicit confirmation dialog, explains that archiving is neither deletion nor cancellation, handles report-workflow locks, and refreshes stale list/detail data. These actions are available from both the list and metadata detail shell.
- Create-form completion: `Nova utakmica` now opens the full React Hook Form + Zod dialog. It uses active settings data, restricts `DATA_OPERATOR` choices to their authorized teams, creates only `SCHEDULED` matches, converts local date/time to UTC, maps backend validation/conflict feedback, invalidates match-list queries, shows success feedback, and opens the created match detail.\r\n- Verification after completion: `npm.cmd run format`, `npm.cmd run format:check`, and `npm.cmd run build` passed. `npm.cmd run lint` completed with only the existing React Compiler compatibility warnings for React Hook Form/TanStack Table, plus the same warning for the new edit dialog's standard React Hook Form `watch` use. The Vite bundle-size notice remains informational. Manual authenticated browser coverage still requires the local API/PostgreSQL session.
- UTF-8 follow-up: repaired incorrectly decoded Bosnian Latin strings within the Matches feature. User-facing copy now renders proper characters such as `Poništi filtere`, `Sljedeća`, `Takmičenje`, and `Prikaži arhivirane`.
- Dialog selector follow-up: create and edit dialogs now resolve the selected stored identifier or enum to its Bosnian display label in the trigger. IDs/enums remain the submitted API values; the UI shows labels such as a selection name and `Domaćin`.
- Time-format follow-up: documented the shared `HH:mm` 24-hour Bosnian time rule and constrained create/edit kickoff inputs to minute precision while preserving ISO/UTC API transport.
- Archive workflow query follow-up: corrected the report repository guard used by match archive/restore. It now filters `MatchReports` by ID or match ID before constructing `MatchReportAggregate`, avoiding EF Core's non-translatable post-projection predicate and allowing the intended archive conflict handling to run.
- Mobile match-dialog follow-up: the create and edit dialogs now follow the existing invitation dialog and shadcn sticky-footer composition. They use a viewport-safe maximum height with a separately scrollable form body, while cancel/save actions remain visible.

## Unit 35: Lineup and Appearances UI

- Status: complete. Manual browser verification remains dependent on a locally configured API/PostgreSQL session; automated coverage and all project quality gates pass.
- Lineup-editor usability follow-up: widened the responsive dialog at the desktop breakpoint, separated its scrollable form body from a persistent shadcn dialog footer, and render selected player names rather than GUID values for captain and substitution selectors.
- Match-detail responsive-navigation and lint follow-up: its section selector now follows the Settings mobile Select/desktop Tabs pattern. Replaced direct React Hook Form watches with `useWatch` in affected components and documented the two necessary TanStack Table React Compiler opt-outs.
- Added `GET /api/matches/{matchId}/lineup/eligible-players`. It uses the match's immutable team and kickoff date, excludes archived players, returns deterministic compact candidate data, and limits access to admins or in-scope data operators. No migration was introduced.
- Replaced the match-detail `Sastav` placeholder with lineup read mode and a React Hook Form/Zod atomic editor. It supports preliminary scheduled/postponed lineups, played-match captain/appearances/minutes/substitutions, role movement, duplicate prevention, manual non-negative minutes, ordered substitution sequences, report/lifecycle locks, server candidate selection, and focused query invalidation after a successful save.
- The editor protects dirty close/cancel operations with an explicit discard dialog. Browser-level navigation is not intercepted; sheet/dialog dismissal is protected.
- Added focused integration coverage for historical/current/future match-date eligibility, range boundaries, excluded archived and other-team players, admin access, read-only role denial, and safe missing-match behavior.
- Verification passed: backend whitespace format and verify, zero-warning `dotnet build`, 61 unit tests, 29 integration tests, frontend Prettier format/check, frontend build, and frontend lint (warnings only: existing React Compiler compatibility notices plus React Hook Form watch usage in the new editor). `git diff --check` passed.

## Unit 36: Manual Match Statistics UI

- Status: partial. The implemented workflow is usable and the project quality gates pass, but the Unit 36 audit below identifies remaining specification requirements before this unit can be marked complete.
- The UI supports draft report initialization for admin/data-operator sessions, played/archived/unavailable/no-report/no-appearance/loading/error/read-only states, player and goalkeeper sub-tabs, multiple manually selected goalkeeper appearances, nullable clean-sheet selection, local completeness feedback, and workflow-conflict reload feedback.
- Numeric input preserves empty-to-null and zero values, rejects invalid decimal/negative entries before saving, and validates the backend-supported shots/passes/duels relationships. Edit cancellation and goalkeeper-row removal require discard/confirmation dialogs; browser-level navigation remains outside the current router guard pattern.
- Added shadcn `Progress` and `Tooltip` primitives through the CLI. No backend files, workflow transitions, or API contracts were changed.
- Verification passed: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint` (existing React Compiler compatibility warnings only), and `npm.cmd run build`. The existing Vite bundle-size warning remains informational. Manual authenticated API/browser verification remains dependent on the local API/PostgreSQL session.

### Specification audit — 2026-07-12

- Implemented and verified in code: report lookup/creation, statistics GET and atomic PUT calls, backend-driven field rendering, central presentation registry, numeric null-versus-zero handling, supported relationship validation, player and goalkeeper read/edit flows, duplicate-goalkeeper prevention, selected-row add/remove behavior, loading/no-report/no-appearance/unavailable/unsupported-field states, local completeness, and focused query invalidation.
- Required before exact-spec completion: dynamic Zod schemas are not used; player-grid headers are not grouped by the registry's presentation group; backend validation is displayed only as a form summary rather than mapped to cells/rows; `409` responses do not preserve recoverable unsaved values or distinguish workflow locks from appearance-snapshot mismatches; dirty edits do not guard top-level match-tab changes or in-app navigation; and no approved frontend test foundation exists, so the recommended parser/payload tests are absent.
- Intentional user-directed variance: goalkeeper-statistics rows now use the lineup editor's immediate add/remove interaction. Removal is persisted only by `Sačuvaj statistiku`, rather than the feature spec's confirmation dialog for populated goalkeeper rows.
- Audit verification passed: `npm.cmd run format:check`, `npm.cmd run lint`, `npm.cmd run build`, and `git diff --check`. The Vite bundle-size notice remains informational; no manual authenticated end-to-end run was performed in this environment.
- Audit follow-up: report-query failures now render a retryable error state rather than the no-report state, and goalkeeper numeric input names include the selected player's name. `npm.cmd run format`, `npm.cmd run lint`, and `npm.cmd run build` passed.
- Statistics header follow-up: moved the report-status badge beside the statistics title so the right-aligned `Uredi statistiku` action can appear or disappear without shifting the status indicator.
- Match-detail navigation follow-up: installed the official shadcn Breadcrumb component through the CLI and replaced the visible hand-built route text with semantic `Utakmice / {selekcija} – {protivnik}` navigation.

## Unit 37: Match Report Review UI

- Status: implemented with an explicitly documented component limitation below. The frontend has a protected `/match-reports` queue with server-side `nuqs` filters and pagination, report-status tabs, scoped team options, localized status labels, TanStack Table rendering, role-safe empty/error states, and navigation to the existing match `Revizija` tab.
- Replaced the `Revizija` placeholder with report detail, advisory lineup/statistics readiness, available workflow metadata, latest correction reason, existing lineup/statistics navigation, and backend-`allowedActions`-only workflow controls. Submit, verify, correction request, and archive use the explicit endpoints, focused query invalidation, no optimistic status assignment, stale-state refetch, pending-state protection, and Bosnian feedback.
- Synchronized `context/architecture.md`: archived reports now have `ARCHIVED: view`; no restore endpoint, transition, or UI was introduced.
- Added the generated shadcn `Textarea` through the CLI. Limitation: the current CLI Alert Dialog addition requires overwriting the existing generated `Button` primitive. Per the shadcn workflow, that overwrite was not performed without explicit user approval, so submit/verify/archive currently use the existing accessible shadcn `Dialog` confirmation pattern rather than the requested Alert Dialog wrapper.
- Verification: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, `npm.cmd run build`, and `git diff --check` passed. The Vite bundle-size notice remains informational. Manual authenticated browser/API checks remain dependent on the local API/PostgreSQL session.

### Queue responsiveness and query-fix follow-up

- Replaced the wide, horizontally scrolling workflow-status tabs on phones with the same full-width Select/mobile and Tabs/tablet-plus pattern used by Settings. The status tabs now retain their natural width on larger screens, and `Poništi filtere` is available in the filter card whenever a report filter is active.
- Fixed the report queue's EF Core translation failure. `MatchReportsRepository.ListAsync` now applies backend-visible report statuses to the `MatchReport` entity query before the `MatchReportReadModel` projection, allowing `GET /api/match-reports` to execute server-side filtering and pagination successfully.
- Verification: frontend Prettier, lint, and build passed. Isolated backend restore/build passed with zero warnings/errors, and all tests passed: 61 unit and 29 integration tests. The source-tree `dotnet format` remains blocked by files held by the running local API process; no formatter changes were needed for the focused source edit.

### Status-filter refinement

- Removed report workflow status tabs entirely. Status is now a regular `Status izvještaja` Select filter in the queue filter card alongside season, selection, competition, and date filters; `Poništi filtere` remains available whenever any filter is active.
- Verification: frontend Prettier, lint, and build passed. The currently running API process must be restarted to serve the already-verified backend query fix.

### Report queue runtime recovery

- Rewrote `MatchReportsRepository.ListAsync` so all report visibility, status, team, season, competition, and date predicates run against EF entity joins before the response projection. This removes the remaining projection-shape risk in the report queue query.
- Rebuilt the normal API output after stopping the stale port-5051 process that held the Infrastructure assembly lock, then restarted the API with the `http` launch profile. `/health` returns 200 and the unauthenticated report endpoint now returns the expected 401 rather than a server error. Authenticated queue verification remains a browser-session check.
- Verification: normal API build passed with zero warnings/errors; the isolated build and full suite remain green (61 unit and 29 integration tests).

### Unit 37 audit remediation

- Corrected selected-team filter visibility by using the backend session's `SELECTED_TEAMS` scope value. The queue now avoids offering inaccessible team filters.
- Added a safe queue `Otvori reviziju` dropdown action and latest-correction column, distinguished report-load errors from the no-report state, and gave eligible data-entry users a direct `Statistika` navigation action to start report input without duplicating the Unit 36 creation control.
- Expanded the advisory readiness summary with match state, lineup roles, captain, appearances, substitutions, applied tracking level, player/goalkeeper completeness, and workflow state. Current metadata now identifies the supplied actor IDs alongside timestamps; human-readable staff names require a future approved backend contract expansion.
- Match-detail tab selection now remains URL-driven after navigation. Submit `422` failures now clearly instruct users to check Sastav and Statistika instead of showing the generic action-failure message.
- Verification: `npm.cmd run format`, `npm.cmd run lint`, `npm.cmd run build`, and `git diff --check` passed. The Vite bundle-size warning remains informational.

### Report workflow actor display names

- Replaced report-workflow GUID display with staff `DisplayName` values. The report detail contract now supplies a creator display name and optional display names for submit, verify, correction, and archive actors; the queue remains unchanged. No migration or workflow rule changed.
- Verification: frontend format, lint, and build passed; isolated backend build passed with zero warnings/errors; 61 unit and 29 integration tests passed. Rebuilt and restarted the local API on port 5051; `/health` returns 200.

## Unit 36 report-loading fix

- Fixed the `GET /api/matches/{matchId}/report` 500 exposed by the Statistics tab. `MatchReportsRepository.GetReadByMatchAsync` now applies the match-id predicate before projecting to `MatchReportReadModel`, which EF Core can translate to SQL. This preserves server-side scope filtering and avoids client evaluation.
- Verification: isolated restore, zero-warning build, and the full backend test suite passed using `backend/.artifacts/report-query-fix` (61 unit tests and 29 integration tests). `git diff --check` passed. The normal build-output path remains locked by the running local API process; the isolated build avoids that process without changing it. The repository-wide whitespace verification still reports pre-existing formatting/line-ending issues outside this focused fix.

## Match detail and goalkeeper-statistics UX follow-up

- Match-detail routes now resolve to the `Utakmice` application-header title rather than the fallback `Nepoznata stranica` label.
- The empty goalkeeper-statistics state now clearly explains that a user must enter statistics edit mode before selecting an existing appearance for goalkeeper statistics. Player positions and lineup goalkeeper roles remain intentionally out of scope; a goalkeeper is manually identified only by adding goalkeeper statistics for that match appearance.
- Verification passed: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, and `npm.cmd run build`. The Vite bundle-size notice remains informational.

## Goalkeeper-statistics interaction follow-up

- `Dodaj golmana` is now available directly in the `Golmani` tab whenever the backend permits statistics editing; choosing it enters edit mode and adds an unsaved goalkeeper row to the existing atomic statistics snapshot.
- Goalkeeper selection and clean-sheet values use player names and Bosnian `Da`/`Ne` labels instead of raw appearance IDs and boolean values. Read mode uses the same labels. Goalkeeper statistics remain additional to the required player-statistics row for the same match appearance.
- Verification passed: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, and `npm.cmd run build`. The Vite bundle-size notice remains informational.

## Goalkeeper-statistics removal follow-up

- Empty goalkeeper draft rows are excluded from both the atomic save payload and the displayed goalkeeper completeness count. Removing the final selected goalkeeper now sends an empty goalkeeper snapshot, allowing the backend to remove the persisted goalkeeper-statistics record without a validation `422`.
- Verification passed: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, and `npm.cmd run build`. The Vite bundle-size notice remains informational.

## Goalkeeper-statistics save stability follow-up

- Statistics saving now resets the form from the authoritative response before leaving edit mode, eliminating the stale goalkeeper-row render that could crash the page after adding or removing a goalkeeper. Identity rendering also safely handles a transient missing appearance while data refreshes.
- The player-statistics tab now states that it includes goalkeepers, while the goalkeeper tab is labeled as additional statistics. This reflects the required API snapshot: all appearances retain player statistics and goalkeeper statistics add goalkeeper-only fields for the selected appearance.
- Verification passed: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, and `npm.cmd run build`. The Vite bundle-size notice remains informational.

## Goalkeeper draft-row removal follow-up

- Empty, newly added goalkeeper rows now disappear immediately when their remove action is clicked. The confirmation dialog remains for selected or populated goalkeeper rows, preserving the safeguard for meaningful changes.
- Verification passed: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, and `npm.cmd run build`. The Vite bundle-size notice remains informational.

## Goalkeeper-statistics selector follow-up

- Replaced blank goalkeeper-row creation with the same select-to-add interaction used by the lineup editor's starting lineup and substitute lists. Selecting an available appearance adds its goalkeeper-statistics row immediately; selected rows display the player identity and can be removed directly.
- Verification passed: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, and `npm.cmd run build`. The Vite bundle-size notice remains informational.

## Goalkeeper-statistics direct-removal follow-up

- Goalkeeper-statistics rows now use the same immediate remove behavior as lineup rows. The remove action updates only the in-memory atomic snapshot; `Sačuvaj statistiku` is the sole persistence confirmation. The separate goalkeeper-removal dialog was removed.
- Verification passed: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, and `npm.cmd run build`. The Vite bundle-size notice remains informational.

## Goalkeeper-statistics row-removal rendering fix

- The goalkeeper table now takes its row structure from React Hook Form's field array rather than the asynchronously watched form values. Removing a goalkeeper therefore removes its visible row immediately, without a transient `Nastup nije dostupan` placeholder.
- Verification passed: `npm.cmd run format`, `npm.cmd run format:check`, `npm.cmd run lint`, and `npm.cmd run build`. The Vite bundle-size notice remains informational.

## First-admin bootstrap configuration follow-up

## Unit 56 authenticated fixture verification follow-up

- Created labelled, local-only records needed for data-dependent accessibility checks: season, competition, opponent, player assignment, match, completed-match lineup/appearance, and training sessions/participants. API-backed create/edit/save operations all succeeded.
- Cancelled or archived all created records afterwards, ended the temporary player assignment, and retained only the application audit history. The temporary viewer without medical permission remains disabled.
- This provides real-data evidence but does not substitute for the required keyboard-only, viewport/zoom, or screen-reader workflow matrix. Unit 56 remains current rather than complete until those MEDIUM manual-verification rows are finished; the audit documents the exact distinction.

## Unit 56 shell keyboard regression follow-up

- Corrected the skip link's DOM order so it is the first Tab stop, rather than following the shell's interactive controls. Chrome verification at 320Ã—568 now confirms its focus and `Enter` target (`main#glavni-sadrzaj`); navigation to Matches focuses the route heading.
- Verified one main, one h1, and no viewport-width overflow at 320Ã—568, 375Ã—812, 667Ã—375, 768Ã—1024, 1024Ã—768, 1280Ã—800, and 1440Ã—900. The mobile navigation dialog has its required title and Bosnian close control.
- Frontend `format`, `format:check`, `lint`, and `build` all pass. Lint retains the two pre-existing React Compiler compatibility warnings; Vite retains the informational chunk-size notice.

## Unit 56 final local browser-matrix follow-up

- Verified the sign-in workflow with native keyboard interaction: Tab reached email, password, visibility, and submit controls; Space submitted the form and reached the authenticated dashboard.
- Verified all seven required viewport dimensions, 200% page scale, the prescribed text-spacing override, and emulated reduced-motion/forced-colors media preferences. No page-wide overflow occurred; protected routes retained one main landmark and one h1.
- A real screen reader and OS forced-colors environment remain unavailable in this local environment and are recorded in the Unit 56 audit rather than claimed. The remaining complete-workflow manual checks are still explicitly documented; no unavailable result is represented as a pass.

## Unit 56 completion decision

- Status: complete. The repository maintainer approved the remaining documented MEDIUM manual-verification items as release follow-up, owned by the repository maintainer. No BLOCKING, HIGH, or privacy/permission issue remains open.
- This completion records finished implementation, automated checks, and executable local browser verification. It does not represent formal accessibility certification or assert that unavailable screen-reader, device-keyboard, processor-state, or complete lifecycle walkthroughs were performed.

- Added `Bootstrap__FirstAdmin__Name=Administrator` to `backend/.env.example`. The initial bootstrap role handoff now uses this setting for the first administrator's staff display name, alongside the existing email and temporary-password settings. No migration is needed because `StaffAccessProfile.DisplayName` already exists.
- Repaired the fresh-database migration ordering so the player table is created before player-team assignments. Recreated the dropped local PostgreSQL database, applied all migrations successfully, and started the API with bootstrap enabled; the first admin and `Administrator` staff profile were created from the local `.env` values.

## Unit 49: Training GPS UI Foundation

- Status: complete. Added the database-backed `GET /api/training-sessions/{id}/participant-candidates` lookup, using assignment coverage on the session date and excluding active participants. The lookup is limited to users permitted to add participants.
- `GET /api/imports` now accepts the optional `trainingSessionId` filter; typed import clients carry the same field. No migration was added.
- Replaced the training placeholder with `/training-sessions` and `/training-sessions/:id`: URL-backed list filters/pagination, role-aware create action, create form, lifecycle actions, participant candidate search/add/remove, training audit history, and real GPS/physical workload queries are connected to the Unit 48 API foundation.
- Physical workload reads now return current revision, safe import provenance, player identity, canonical metric values, and exact threshold/method comparability context. Shared physical-data presentation is wired into training, match, and player detail surfaces; it does not combine differing threshold or methodology keys.
- Training and match physical views reuse the existing XHR import dialog with locked `TRAINING_GPS` or `MATCH_GPS` team/target context and show target-filtered import history with links to the import detail workspace. The generic processor capability gate remains authoritative; no unsupported preview, validation, confirmation, Gpexe, or Zone14 action is activated.
- Navigation and UI context now use `Treninzi i GPS`. Vendor processing remains deliberately capability-gated; no Gpexe or Zone14 mapping/confirmation behavior was invented.
- Verification: `dotnet build PlayerPerformance.sln --no-restore`, `dotnet test PlayerPerformance.sln --no-restore --no-build` (109 unit and 32 integration tests), both required `dotnet format ... whitespace` commands, frontend Prettier format/check, production build, lint, and `git diff --check` passed. Lint retains one React Hook Form compiler-compatibility warning in the training form; Vite retains its informational bundle-size warning. Authenticated manual browser verification is not executable in this environment and remains a documented deployment handoff check, not a known failure.
- Usability follow-up: compacted the training filter bar, changed training creation to the shared date picker, added footer separation below `Lokacija`, replaced participant GUID display with player display/preferred names, localized audit badges and hid raw identifier metadata, and removed the redundant global `Pregled importa` link from training detail.
- Physical workload query follow-up: replaced the non-translatable nested metric projection with two database-backed queries (workload/revision rows, then all matching metric rows) and in-memory response composition. This preserves scope filtering and avoids N+1 reads while allowing all workload endpoints to sort server-side.

## Unit 53: Dashboard UI

- Status: complete, with authenticated multi-role/browser verification recorded as a local API/PostgreSQL handoff below. Replaced the protected `/` placeholder with the URL-driven `Kontrolna ploča`, using only `GET /api/dashboard/context-options` and one enabled `GET /api/dashboard/overview` request for the selected team/season.
- Context normalizes URL IDs with replace semantics: first active accessible team, otherwise first accessible team, and the backend's first deterministic season. Empty context, loading, retry, inactive team, archived season, date range, generated timestamp, and manual current-overview refresh states are included.
- The dashboard renders only Unit 52 composite response content: centralized quality-alert labels/destinations; recent matches/form and accessible goal chart; visibility-mode-specific workflow; current safe availability snapshot; final-report leaders with returned ranks/ties; and exact physical workload comparability groups with no cross-context aggregation.
- Added focused `invalidateDashboardOverview` and separate context-options invalidation utilities. Dashboard overview refresh is wired into major match, report/status/statistics, import, availability, training, and team/season settings success flows; manual refresh remains available.
- Added the official shadcn Chart and Scroll Area primitives through the CLI. The required generated `Card` primitive was updated through the same approved CLI operation. Charts use `ChartContainer`, `ChartTooltip`, `--chart-*` tokens, and semantic table/list equivalents.
- Verification: frontend format, build, lint (existing two compiler-compatibility warnings only), format check, and `git diff --check` passed. The full authenticated admin/data-operator/analyst/coach/medical/viewer browser matrix remains a local API/PostgreSQL handoff check because this environment has no authenticated browser session; it is not a known automated failure.
- Console follow-up: dashboard navigation buttons rendered through React Router links now explicitly use Base UI `nativeButton={false}`, preserving link semantics and eliminating the development-console native-button warning.
- Alert presentation follow-up: stable dashboard alert codes now render localized Bosnian titles and descriptions. Their links use centralized supported application routes rather than raw backend destination identifiers, preventing uppercase underscore paths such as `AVAILABILITY_UNKNOWN` or `MATCH_REPORTS` from reaching the router.

## Unit 44: Import UI Foundation

- Status: implemented against the Unit 43 import API foundation. The `/imports` workspace replaces the placeholder and is visible in Performance navigation only to `ADMIN` and permitted `DATA_OPERATOR` sessions; backend authorization remains authoritative.
- Added capability-driven CSV/XLSX upload with runtime accepted extensions/size, file-first metadata selection, authorized team/match selectors, native XHR progress, CSRF credentials, abort/reconciliation feedback, immutable import context, URL-driven list filters/status tabs/pagination/selected job, server-side history, and URL-addressable detail Sheet.
- Detail uses backend `allowedActions` only, provides safe source download, displays terminal/processing/failure/result states, uses explicit confirmation/cancellation dialogs, and polls only an actively viewed `PARSING` job every five seconds. Its scoped URL state also drives preview pagination, validation severity/issues pagination, and import audit pagination. Preview columns/rows and validation issues come directly from Unit 43 endpoints; audit history reuses the Unit 39 components and import-safe labels.
- Unit 43 currently registers no real processors; consequently preview, validation, confirmation, parser, mapping, vendor field, and official-data mutation behavior intentionally remains unavailable until later processor units supply backend capabilities. No client-side parsing or new import dependencies were added.
- Verification: frontend Prettier format/check, lint, and production build were run after implementation. Manual authenticated browser/API verification remains dependent on a configured local backend/PostgreSQL session.
- Date-filter consistency follow-up: replaced native import date inputs with the shared `DatePicker`; its calendar now starts weeks on Monday throughout the application.
- Import-list alignment follow-up: replaced the separate status-tab row with the inline filter treatment used by Medijateka, and expanded the table with safe selection, match, format, and validation-summary columns. The Unit 43 list DTO does not provide creator display data; no client-side user lookup or guessed identity is shown.
- Filter-control follow-up: Imports now uses the shared compact `FilterSelect` pattern, with field names shown in empty triggers and full-width mobile sizing; persistent desktop labels were removed in favour of the established Medijateka layout.
- Upload reliability follow-up: the native XHR multipart request now sends metadata as its required first text field and preserves the selected source filename explicitly; the UI rejects zero-byte browser files before upload. The final submit action is labelled `Pokreni import`.
- Dialog usability follow-up: the new-import dialog uses a viewport-bounded, scrollable form body with a sticky action footer so cancel/upload actions remain visible on smaller screens.
- Import empty-stream fix: removed the import endpoint's premature third `MultipartReader.ReadNextSectionAsync` call. That call drained the selected file section before storage consumed it, producing `File content cannot be empty.` The API was rebuilt successfully and restarted on port 5051; `/health` returned 200 after restart.
- Audit presentation follow-up: import audit history now resolves selection/match identifiers, numeric or string enum payloads, source/format/status labels, content types, and byte counts into staff-readable values instead of exposing GUIDs, enum numbers, MIME strings, or raw byte totals.
