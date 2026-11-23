# Unit 39: Audit UI Foundation

## Goal

Build the first authorized audit-history UI for match reports and staff accounts using the entity-specific Unit 38 audit endpoints. Add a readable, paginated match-report history inside the existing `Revizija` experience and an administrator-only staff audit Sheet from `/users`, with localized semantic change summaries and safe structured before/after presentation without adding a global audit browser, audit mutations, exports, retention controls, backend changes, or coverage for unaudited modules.

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
10. `context/feature-specs/23-staff-users-team-scope-account-lifecycle-backend.md`
11. `context/feature-specs/24-staff-users-roles-ui.md`
12. `context/feature-specs/31-match-lineup-appearance-backend.md`
13. `context/feature-specs/37-match-report-review-ui.md`
14. `context/feature-specs/38-audit-backend-foundation.md`
15. `context/feature-specs/39-audit-ui-foundation.md`

Use relevant project-local skills from `.agents/skills/` when applicable.

Before creating custom UI primitives or interaction patterns:

- check `context/references/shadcn-components.md`;
- prefer suitable shadcn/ui components;
- install required shadcn components just in time through the approved workflow;
- compose app-level components outside `frontend/src/components/ui`;
- do not manually modify generated shadcn/ui primitive files;
- do not hand-build equivalents of existing shadcn primitives without a documented reason.

This unit is frontend-only.

Do not change:

- backend audit persistence;
- audit action/entity-type codes;
- audit query contracts;
- authorization rules;
- role/team-scope behavior;
- migrations;
- existing audited mutation behavior;
- report workflow transitions;
- staff lifecycle rules.

### Scope

This unit introduces:

- typed frontend contracts for Unit 38 audit responses;
- TanStack Query hooks for match-report and staff-user audit history;
- localized labels for all initial Unit 38 audit action codes;
- safe actor/time/action presentation;
- readable before/after change summaries;
- action and date filters;
- backend pagination;
- deterministic empty/loading/error states;
- a `Historija promjena` sub-view inside the existing match `Revizija` tab;
- an admin-only `Historija promjena` action and Sheet on `/users`;
- report statistics change presentation using appearance-to-player mapping from the existing lineup snapshot;
- staff selected-team ID presentation using existing team settings data;
- safe fallback behavior for unknown future audit action codes or fields.

This unit does not introduce:

- a global `/audit` page;
- a global `/api/audit` query;
- audit record creation from the frontend;
- audit update;
- audit delete;
- audit archive;
- audit restore;
- audit export;
- CSV/PDF download;
- retention or cleanup settings;
- actor impersonation;
- rollback/revert actions;
- full event-sourcing views;
- audit comparison across unrelated entities;
- audit coverage for players, matches, lineup, media, medical, GPS, imports, or future modules;
- charts or dashboards;
- raw database/EF metadata display;
- frontend changes to business mutation behavior.

### Existing backend contracts remain authoritative

Use only:

```txt
GET /api/match-reports/{reportId}/audit
GET /api/users/{userId}/audit
```

Supported query parameters:

```txt
page
pageSize
action
dateFrom
dateTo
```

Use the existing response pagination convention.

Do not add:

- frontend-generated audit entries;
- audit mutation endpoints;
- a combined audit endpoint;
- direct database access;
- client-side filtering of an unbounded history;
- client-side reconstruction of events that are absent from the backend.

Historical audit data begins when Unit 38 became active.

Do not invent or infer earlier history from:

- created/updated timestamps;
- current workflow metadata;
- current staff access state;
- report status;
- statistics state.

### Authorization and disclosure rules

Backend authorization remains authoritative.

#### Match-report audit

The match-report audit endpoint follows Unit 32 report visibility and team-scope rules.

Frontend behavior:

- show report history only after the caller can already access the report;
- do not show a history section that reveals an inaccessible report;
- handle safe `404`/inaccessible behavior without exposing whether hidden audit entries exist;
- do not infer additional audit access from role labels;
- display only the data returned by the backend.

#### Staff-user audit

The staff-user audit endpoint is `AdminOnly`.

Frontend behavior:

- show the `Historija promjena` staff action only to a resolved administrator session;
- keep `/users` administrator route protection from Unit 24;
- never request staff audit history for non-admin users;
- handle backend `401`/`403` safely because the current user's access may change after rendering;
- close the audit Sheet and refetch session/navigation state when a stale self-access change removes administrator access.

Do not expose staff audit history through public invitation routes, profile menus, or ordinary user session screens.

### No global audit browser

Do not add:

```txt
/audit
/audit-logs
/admin/audit
```

Do not add a global sidebar item.

The first audit UI is intentionally entity-specific:

- match-report history is shown where the report is reviewed;
- staff-account history is shown from staff administration.

Future global audit browsing requires a separate backend query, authorization decision, product need, and feature spec.

### Audit response contract

Create explicit TypeScript contracts aligned with Unit 38.

Each audit item includes at minimum:

```txt
id
actor:
  id
  displayName
action
entityType
entityId
occurredAtUtc
previousValues
newValues
metadata
```

Treat structured JSON fields as untrusted external data.

Use:

```ts
unknown
```

at the API boundary and validate/narrow before presentation.

Do not use `any`.

Do not assume every action contains every property.

Do not mutate the received audit objects.

### Stable action labels

Create one centralized audit-action presentation registry.

Initial action codes and Bosnian Latin labels:

#### Staff actions

```txt
STAFF_INVITATION_CREATED -> Kreirana pozivnica za korisnika
STAFF_INVITATION_REISSUED -> Ponovo izdata pozivnica
STAFF_INVITATION_ACCEPTED -> Pozivnica prihvaćena
STAFF_PROFILE_UPDATED -> Ažuriran profil korisnika
STAFF_ACCESS_REPLACED -> Ažuriran pristup korisnika
STAFF_ACCOUNT_DISABLED -> Korisnički račun deaktiviran
STAFF_ACCOUNT_REACTIVATED -> Korisnički račun ponovo aktiviran
```

#### Match-report actions

```txt
MATCH_REPORT_CREATED -> Kreiran izvještaj utakmice
MATCH_REPORT_SUBMITTED -> Izvještaj poslan na pregled
MATCH_REPORT_VERIFIED -> Izvještaj verificiran
MATCH_REPORT_CORRECTION_REQUESTED -> Zatražena korekcija izvještaja
MATCH_REPORT_ARCHIVED -> Izvještaj arhiviran
MATCH_REPORT_STATISTICS_UPDATED -> Ažurirana statistika izvještaja
```

Registry metadata may include:

- visible label;
- concise description;
- action group;
- suitable Lucide icon;
- presentation emphasis category.

Do not persist or send these localized labels back to the server.

Do not scatter action-label switches across components.

### Unknown action codes

The audit foundation must remain forward-compatible.

If the backend returns an unknown action code:

- preserve and display the audit item;
- show a localized fallback such as `Nepoznata audit radnja`;
- display the stable backend action code in a compact technical label;
- do not show mutation controls;
- do not silently discard the item;
- use the generic safe structured-value renderer for its values;
- document the frontend/backend contract mismatch in `context/progress-tracker.md` during implementation if it occurs.

Unknown actions must not break the complete history list.

### Entity-type handling

Initial entity types:

```txt
STAFF_USER
MATCH_REPORT
```

Do not display raw entity type prominently when the surrounding page already defines the entity.

Use it only for:

- validation;
- safe fallback;
- technical details when an unexpected value is returned.

If a response item entity type does not match the endpoint entity:

- show a compatibility/error warning;
- do not merge it silently into normal history presentation;
- preserve the rest of valid items where safe;
- document the mismatch.

### Audit item visual structure

Each audit event should show:

```txt
Action label
Actor
Date and time
Short semantic summary
Changed values
Optional metadata/context
```

Use an app-level reusable component such as:

```txt
AuditHistoryList
AuditHistoryItem
AuditChangeSet
```

These are application components, not new shadcn primitives.

Prefer composing:

- `Item`;
- `Card`;
- `Badge`;
- `Separator`;
- `Collapsible`;
- `Alert`;
- `Tooltip`;
- `Kbd` only when actual keyboard input is shown, which is not expected here.

Do not use chat components such as:

- `Message`;
- `Bubble`;
- `Message Scroller`.

Audit history is not a conversation.

Do not install a timeline package.

A simple semantic vertical list with separators is sufficient.

### Ordering

Display items in the exact backend order:

```txt
OccurredAtUtc descending
Id descending
```

Do not reverse the list into oldest-first.

Do not re-sort by action, actor, or frontend-formatted time.

If grouped date headings improve scanning, group visually without changing event order.

Suggested date groups:

```txt
Danas
Jučer
dd. MM. yyyy.
```

Use the application's current date-formatting utilities.

### Actor presentation

Show:

- actor display name;
- action timestamp;
- actor ID only as a fallback/technical identifier when the display summary is unavailable.

Suggested fallback:

```txt
Nepoznat korisnik
```

When actor summary is missing:

- do not hide the event;
- show the preserved actor ID in a tooltip or secondary technical text;
- do not attempt to infer the actor from target entity or metadata.

Do not show actor email in match-report audit.

For staff-user audit, show actor email only when the backend actually returns it through the approved admin-safe summary and the existing staff display pattern needs it. Display name is normally sufficient.

### Date and time presentation

Use local display time with:

- clear Bosnian day-month-year format;
- 24-hour clock;
- consistent timezone conversion from UTC.

Show an exact timestamp.

A relative label such as `prije 5 minuta` may be supplementary but must not replace the exact timestamp.

Do not scatter date formatting across components.

### Structured change presentation

Do not dump raw JSON as the default UI.

Present known action payloads through explicit, action-specific formatters.

Each formatter may return:

- changed field label;
- previous display value;
- new display value;
- added/removed list values;
- safe contextual metadata.

Use a compact before/after layout:

```txt
Polje
Prije
Poslije
```

or a responsive stacked equivalent.

For creation actions with no previous values:

- show the created/current values as `Postavljeno`;
- do not show meaningless empty `Prije` cells.

For transition actions:

- emphasize status change;
- show associated actor/time metadata only when it adds information beyond the event header.

For unknown fields:

- use a generic safe fallback formatter;
- display the stable field key in a technical label;
- render only JSON-native safe values;
- never use `dangerouslySetInnerHTML`.

### Generic safe value renderer

Create a bounded generic value renderer for unknown but structured audit values.

Supported display:

- `null` -> `Nije postavljeno`;
- boolean -> `Da` / `Ne`;
- number -> exact value, including `0`;
- string -> escaped plain text;
- string/number arrays -> badges or comma-separated values;
- small objects -> nested key/value rows;
- large/complex objects -> collapsed technical details with bounded depth/item count.

Rules:

- preserve `null` versus `0`;
- never treat HTML strings as markup;
- never execute links/scripts from values;
- never recursively render without depth bounds;
- do not stringify huge objects into the page;
- use the known Unit 38 size limits as a safety assumption but still render defensively.

Do not expose a `Kopiraj cijeli JSON` button in this unit.

### Sensitive-value defense in depth

Unit 38 must not persist secrets, but the frontend must still avoid encouraging unsafe display.

Do not add special labels or renderers for:

- passwords;
- password hashes;
- tokens;
- security stamps;
- cookies;
- CSRF values;
- connection strings.

If a returned key matches a clearly forbidden sensitive pattern:

- do not render the value;
- show a blocking redaction warning for that item;
- document the backend contract/security defect in `context/progress-tracker.md`;
- do not print the value to console, toast, error boundary, or analytics.

Do not log entire audit response objects in development or production code.

### Staff-field labels

Create centralized staff audit field labels.

At minimum:

```txt
displayName -> Ime i prezime
email -> Email za prijavu
status -> Status računa
primaryRole -> Uloga
canVerifyReports -> Može verificirati izvještaje
canImportData -> Može importovati podatke
canViewMedicalDetails -> Može pregledati medicinske detalje
teamScopeType -> Opseg pristupa selekcijama
selectedTeamIds -> Odabrane selekcije
setupCompleted -> Postavljanje računa završeno
mustChangePassword -> Potrebna promjena lozinke
sessionsInvalidated -> Sesije poništene
reactivationResult -> Rezultat reaktivacije
invitationIssued -> Pozivnica izdata
credentialReissued -> Pristupni podaci ponovo izdati
```

Use the same role, account-status, permission, and team-scope label mappings from Unit 24.

Do not duplicate conflicting mappings.

### Staff access diff presentation

For `STAFF_ACCESS_REPLACED`, present normalized previous/new access clearly.

Show:

- role change;
- explicit permission changes;
- team-scope type change;
- selected team additions/removals.

Use real team names from existing teams/settings queries.

Do not show raw team IDs as the primary UI.

When a referenced team name cannot be resolved:

- preserve the team ID in a fallback badge/tooltip;
- label it `Nepoznata selekcija`;
- do not remove it from the history.

Selected-team arrays are sets.

Present:

```txt
Dodano
Uklonjeno
```

rather than relying only on reordered before/after comma-separated lists.

Do not treat deterministic ordering differences as business changes.

### Staff invitation and lifecycle presentation

#### Invitation created

Show:

- display name;
- login email;
- initial status;
- role;
- permissions;
- team scope.

Do not imply the one-time token is available in history.

#### Invitation reissued

Show a concise explanation:

```txt
Izdata je nova jednokratna pozivnica. Sigurnosni token nije pohranjen u audit historiji.
```

Do not show a copy-link action.

#### Invitation accepted

Show account transition to active/setup-complete state.

Do not show password/security internals.

#### Profile updated

Show changed profile fields only.

#### Account disabled/reactivated

Show previous and new account status and safe lifecycle metadata such as session invalidation when returned.

### Match-report field labels

Create centralized report audit field labels.

At minimum:

```txt
matchId -> Utakmica
status -> Status izvještaja
submittedByUserId -> Poslao na pregled
submittedAtUtc -> Vrijeme slanja
verifiedByUserId -> Verificirao
verifiedAtUtc -> Vrijeme verifikacije
lastCorrectionRequestedByUserId -> Korekciju zatražio
lastCorrectionRequestedAtUtc -> Vrijeme zahtjeva
lastCorrectionReason -> Razlog korekcije
appliedTrackingLevel -> Primijenjeni nivo praćenja
archive metadata -> Arhiviranje
teamId -> Selekcija
affectedPlayerAppearanceIds -> Promijenjeni nastupi igrača
affectedGoalkeeperAppearanceIds -> Promijenjeni golmanski nastupi
goalkeeperRowsAdded -> Dodane golmanske statistike
goalkeeperRowsRemoved -> Uklonjene golmanske statistike
```

Reuse report status and tracking-level labels from Units 36–37.

Do not duplicate conflicting mappings.

### Report workflow event presentation

#### Report created

Show:

- report created;
- initial status `Nacrt`;
- safe match/team context when returned.

#### Submitted

Emphasize:

```txt
Nacrt / Potrebna korekcija -> Spremno za pregled
```

Use actual previous/new values.

#### Verified

Emphasize:

```txt
Spremno za pregled -> Verificirano
```

#### Correction requested

Show:

- previous/new status;
- correction requester/time when returned;
- full safe correction reason;
- plain text only.

#### Archived

Show terminal transition to `Arhivirano`.

Do not show restore actions.

### Statistics audit presentation

`MATCH_REPORT_STATISTICS_UPDATED` may contain nested changed-field maps keyed by `PlayerMatchAppearanceId`.

Use the existing Unit 31 lineup snapshot to build:

```txt
appearanceId -> player display name
```

Rules:

- map changed player and goalkeeper rows to player names where possible;
- preserve appearance ID as technical fallback;
- do not use the current player team list;
- do not infer names from audit metadata;
- do not call one API request per appearance;
- reuse the already available lineup snapshot/query.

Present statistics changes grouped by player.

Example:

```txt
Igrač: Marko Marić

Golovi
Prije: 0
Poslije: 1

Tačna dodavanja
Prije: Nije uneseno
Poslije: 24
```

Preserve:

- `null` as `Nije uneseno`;
- `0` as `0`;
- booleans as `Da` / `Ne`.

Use the Unit 36 centralized statistics field registry for labels.

Do not recreate statistics label mappings.

For goalkeeper row addition/removal:

- show `Dodana golmanska statistika` or `Uklonjena golmanska statistika`;
- identify the appearance/player;
- show changed enabled values when present.

If the lineup snapshot fails to load:

- keep the audit history visible;
- show appearance IDs as safe fallback;
- show a non-blocking warning that player names could not be resolved;
- do not falsely label IDs as player names.

### Match-report audit integration

Inside the existing match `Revizija` tab from Unit 37, add a nested view selector:

```txt
Tok izvještaja
Historija promjena
```

Use shadcn `Tabs`.

`Tok izvještaja` retains the complete Unit 37 workflow UI.

`Historija promjena` contains Unit 38 report audit history.

Do not:

- remove workflow actions;
- duplicate the match detail shell;
- create a separate report detail route;
- mix audit events into the workflow metadata list;
- claim current metadata is complete historical audit.

The nested view state should be shareable through URL state using the established route/`nuqs` convention.

Suggested query value:

```txt
reviewView=workflow
reviewView=audit
```

Use the actual naming pattern that best fits existing route parameters.

### Match-report audit filters and URL state

Use `nuqs` for report audit state:

- audit page;
- action filter;
- date from;
- date to.

Use names scoped to the review/audit area to avoid collisions with match list/report queue parameters.

Example concept:

```txt
auditPage
auditAction
auditDateFrom
auditDateTo
```

Rules:

- changing action/date resets page to 1;
- invalid values normalize safely;
- filters are sent to the backend;
- do not filter loaded results client-side;
- clear audit filters when the user chooses `Očisti filtere`;
- preserve workflow/report route context.

Action options must include only match-report action codes from Unit 38.

### Match-report history states

Required states:

#### No audit entries

Show an honest empty state:

```txt
Još nema zabilježenih promjena za ovaj izvještaj.
```

Explain that audit history starts from the point when audit logging was introduced.

Do not infer older events.

#### Loading

Use `Skeleton`.

#### Query error

Use `Alert` with retry.

Do not hide the complete workflow tab because audit loading failed.

#### Inaccessible/missing report

Use existing safe report behavior.

Do not display a separate audit-specific disclosure.

#### Partial lineup-name-resolution failure

Keep history usable with technical appearance identifiers and a warning.

### Staff audit integration on `/users`

Add a row action:

```txt
Historija promjena
```

Show it only to resolved administrators.

Use shadcn `Sheet` for the history surface.

The Sheet should include:

```txt
User summary
Audit filters
Audit history list
Pagination
```

Do not add a new global user-audit page in this unit.

The selected staff audit surface should be URL-addressable through `nuqs`, for example:

```txt
auditUserId
userAuditPage
userAuditAction
userAuditDateFrom
userAuditDateTo
```

Rules:

- opening the action writes the selected user ID to URL state;
- refreshing restores the Sheet when the admin can still access it;
- closing clears audit-specific URL state;
- opening another user resets audit page/filter state;
- invalid/missing selected user closes safely or shows the existing not-found behavior;
- do not store selected audit user in Zustand.

### Staff audit Sheet header

Show:

- display name;
- login email;
- current role;
- current account status;
- close action.

Use the existing `GET /api/users/{userId}` detail query and Unit 24 label mappings.

Do not treat current values as historical values.

Label the summary clearly as current account state.

Suggested title:

```txt
Historija promjena korisnika
```

### Staff audit filters

Support:

- action;
- date from;
- date to;
- page.

Use only staff action options from Unit 38.

Use:

- shadcn `Select` for action;
- approved Date Picker/Popover + Calendar pattern for dates;
- existing pagination.

Do not include entity-type selector because the endpoint is already entity-specific.

Do not add actor filter unless the backend supports it later.

### Staff audit empty/loading/error states

#### No audit entries

Show:

```txt
Još nema zabilježenih promjena za ovog korisnika.
```

Do not imply that no historical changes ever happened before audit logging existed.

#### Loading

Use a compact Sheet skeleton.

#### Error

Show `Alert` and retry.

#### Stale admin access

On `403`:

- close or disable the Sheet;
- refetch session;
- let the existing `/users` route guard become authoritative;
- do not keep showing cached audit values to a user who lost admin access.

#### Disabled or invited target user

History remains viewable to administrators.

Do not hide the action based on target account status.

### Pagination

Use backend pagination for both audit surfaces.

Default page size should be a practical bounded value consistent with the backend default, for example 20.

Do not expose an arbitrary large page-size selector unless the existing common pagination pattern already requires it.

Show:

- previous;
- next;
- current page;
- total items/pages when returned.

Match-report audit page is stored in URL state.

Staff Sheet audit page is stored in its scoped URL state.

Do not concatenate every page into an unbounded client-side list.

### Query keys and data access

Use TanStack Query.

Define stable keys for:

```txt
matchReportAudit(reportId, filters)
staffUserAudit(userId, filters)
staffUserDetail(userId)
matchLineup(matchId)
teams/options
```

Reuse existing:

- report detail query;
- lineup query;
- user detail query;
- team settings query;
- role/status/statistics label registries.

Do not duplicate API clients or contracts already present.

Do not store audit server data in:

- Zustand;
- React Context;
- localStorage;
- sessionStorage.

Avoid broad query invalidation.

Audit history is immutable after creation, but new events appear after business mutations.

After existing audited mutations succeed:

- invalidate the relevant entity audit query prefix when practical;
- do not block the business success UI if audit refetch fails;
- do not optimistically create a fake audit item.

Examples:

- report submit/verify/correction/archive/statistics save should invalidate report audit queries;
- staff invitation/profile/access/disable/reactivate actions should invalidate the selected user's audit queries when relevant.

Do not reconstruct the event client-side from the mutation request.

### Caching behavior

Audit records are immutable, but pages can gain newer entries.

Use normal TanStack Query caching with focused invalidation.

Do not use infinite stale time for the first page.

When the history surface opens:

- fetch the requested page;
- respect existing freshness conventions;
- allow manual retry/refetch.

Do not poll continuously.

### Loading and progressive rendering

For match reports:

- render Unit 37 workflow normally while audit history is not selected;
- do not fetch all audit pages eagerly;
- fetch audit only when the audit nested view is active, unless the established query pattern prefers a harmless prefetched first page;
- do not fetch lineup solely for statistics-name mapping until audit entries requiring that mapping are present.

For staff:

- fetch audit only when the Sheet is open;
- fetch target user detail through the existing user-detail query;
- reuse cached team options when available.

Avoid waterfall requests where parallel fetching is safe.

### Filter date semantics

Send UTC-compatible date filters according to the Unit 38 API contract.

Visible date pickers use local Bosnian-friendly dates.

Define deterministic start/end conversion:

- date from -> start of selected local date converted to UTC;
- date to -> end/exclusive start of following local date according to the established API convention.

Use existing date utilities.

Do not hand-code timezone conversions separately in both audit surfaces.

If the backend expects date-only strings instead of timestamps in the implemented contract, align with the actual typed API contract and document it.

### Shadcn-first component selection

Review the interactions against:

```txt
context/references/shadcn-components.md
```

Prefer suitable components such as:

- `Tabs`;
- `Sheet`;
- `Item`;
- `Card`;
- `Badge`;
- `Separator`;
- `Collapsible`;
- `Alert`;
- `Empty`;
- `Skeleton`;
- `Select`;
- `Date Picker`;
- `Popover`;
- `Calendar`;
- `Pagination`;
- `Tooltip`;
- `Scroll Area`;
- `Dropdown Menu`;
- `Button`.

Do not use:

- `Message`;
- `Bubble`;
- `Message Scroller`;
- a third-party timeline;
- a JSON viewer package.

Do not install components merely because they are available.

Do not modify generated shadcn primitive files.

Do not apply ad-hoc visual overrides.

### Frontend organization

Create reusable audit display code in a focused shared application area because it is used by both Matches and Users.

Suggested organization:

```txt
frontend/src/features/audit/
├── api/
├── components/
├── hooks/
├── types/
├── utils/
└── index.ts
```

Entity-specific presenters may remain near their owner:

```txt
frontend/src/features/matches/components/audit/
frontend/src/features/users/components/audit/
```

A good split:

#### Shared audit feature

- API contracts;
- paginated response types;
- actor/event header;
- generic safe value renderer;
- audit list;
- filters;
- pagination adapter;
- unknown-action fallback.

#### Matches

- report action mappings;
- report workflow/status field mappings;
- statistics change presenter;
- appearance-to-player resolution.

#### Users

- staff action mappings;
- role/status/permission/scope presenter;
- team-ID-to-name resolution;
- user audit Sheet.

Do not create one enormous component with entity-type switches for all future modules.

Do not create a generalized plugin framework.

### Localization

Visible copy is Bosnian Latin with proper characters.

Suggested copy includes:

```txt
Historija promjena
Tok izvještaja
Radnja
Datum od
Datum do
Očisti filtere
Još nema zabilježenih promjena
Nepoznata audit radnja
Nepoznat korisnik
Prije
Poslije
Postavljeno
Dodano
Uklonjeno
Nije postavljeno
Tehnički detalji
Trenutno stanje korisnika
Historija promjena korisnika
```

Use consistent terminology.

Prefer `Historija promjena` as the visible label rather than mixing `Audit`, `Dnevnik`, and `Evidencija` arbitrarily.

The technical word `audit` remains valid in code, routes, query keys, and developer-facing diagnostics.

### Accessibility

Requirements:

- nested report tabs are keyboard navigable;
- the staff Sheet traps and restores focus correctly;
- each audit event has a readable action heading;
- actor and exact timestamp are programmatically associated with the event;
- before/after values have clear labels;
- added/removed states do not rely only on color;
- collapsed technical details expose expanded state;
- filter controls have labels;
- pagination buttons have accessible names;
- unknown-action warnings are readable;
- icon-only controls use accessible labels/tooltips;
- long correction reasons remain readable;
- horizontal overflow is contained for change tables;
- the Sheet remains usable with keyboard and screen readers.

### Responsive behavior

Desktop/tablet are primary.

Match-report history:

- use a responsive list;
- stack before/after values on smaller screens;
- avoid page-wide horizontal overflow;
- keep event headings and timestamps readable.

Staff Sheet:

- use a wide but bounded desktop Sheet;
- use full available width on smaller screens;
- keep filters and pagination reachable;
- use `Scroll Area` only when it improves contained behavior without hiding native accessibility.

Do not create a separate mobile audit product.

### Error handling

Use existing ProblemDetails handling.

Expected frontend behavior:

- `401`: use existing auth/session behavior;
- `403`: show safe access feedback, refetch session when relevant;
- `404`: use safe entity-missing/inaccessible behavior;
- invalid filter response: normalize/reset offending filter and show safe feedback;
- network error: retain existing loaded page when appropriate and offer retry;
- malformed structured audit value: show an item-level compatibility warning without crashing the whole page.

Do not include raw backend exception details.

Do not log audit payloads to console.

### Tests and verification approach

Add frontend tests only if the repository already has an approved frontend testing foundation.

When tests exist, prioritize:

- action-label registry;
- unknown action fallback;
- safe value renderer;
- `null` versus `0`;
- sensitive-key redaction;
- staff access set diff;
- appearance-to-player mapping;
- statistics change grouping;
- audit query parameter serialization;
- staff Sheet URL state;
- report nested-view URL state.

Do not introduce a new testing framework solely for Unit 39.

Manual verification must cover:

- admin staff audit action visibility;
- non-admin absence of staff audit action;
- staff Sheet open/close/refresh URL behavior;
- staff action/date/page filters;
- invited/active/disabled target users;
- staff role/permission/team-scope changes;
- unresolved team IDs;
- match-report audit visibility for each role/status combination;
- nested `Tok izvještaja` / `Historija promjena` navigation;
- report filters and pagination;
- report creation/submit/verify/correction/archive events;
- statistics changes with `null` and `0`;
- player-name mapping;
- missing lineup mapping fallback;
- unknown action fallback;
- empty pre-audit history;
- loading/error/retry;
- stale `403`;
- responsive behavior;
- keyboard/focus behavior.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

Record:

- the two implemented audit surfaces;
- current initial action mappings;
- intentionally absent global audit page;
- any unknown action/field encountered;
- verification results;
- any testing/environment limitation.

If the implemented Unit 38 API differs from this spec, update the relevant context/spec before continuing.

Do not silently add a global audit browser, export, rollback, or coverage for unaudited modules.

## Implementation

### 1. Add shared audit frontend contracts

Create typed contracts and safe validation/narrowing for:

- audit actor;
- audit item;
- structured values;
- paginated response;
- query filters.

Use `unknown` at the JSON boundary.

### 2. Add audit API clients and query hooks

Implement:

```txt
getMatchReportAudit(reportId, filters)
getStaffUserAudit(userId, filters)
```

Add TanStack Query hooks and stable keys.

Keep entity endpoints separate.

### 3. Build the shared audit display components

Create reusable:

- event list;
- event header;
- action badge/label;
- before/after change rows;
- generic safe fallback renderer;
- loading/empty/error states;
- filter bar;
- pagination adapter.

Keep entity-specific semantics outside the generic renderer.

### 4. Add action and field presentation registries

Create:

- shared action registry shape;
- staff action registry;
- match-report action registry;
- staff field labels;
- report field labels.

Reuse existing role/status/tracking/statistics label utilities.

### 5. Build the match-report audit presenter

Implement action-specific presentation for:

- report creation;
- submission;
- verification;
- correction request;
- archive;
- statistics update.

Map appearance IDs to player names using the existing lineup snapshot.

Handle unresolved appearances safely.

### 6. Integrate match audit into `Revizija`

Add nested tabs:

```txt
Tok izvještaja
Historija promjena
```

Preserve all Unit 37 workflow behavior.

Use scoped `nuqs` state for:

- nested view;
- audit filters;
- audit page.

Fetch audit data only when needed.

### 7. Build the staff audit presenter

Implement semantic presentation for:

- invitation creation;
- invitation reissue;
- invitation acceptance;
- profile changes;
- access replacement;
- disable;
- reactivate.

Map team IDs to names using existing team queries.

Reuse Unit 24 labels.

### 8. Add the staff audit Sheet

Add `Historija promjena` to `/users` row actions.

Use URL-backed `nuqs` state for:

- selected user;
- action filter;
- date filters;
- page.

Show current user summary separately from historical events.

Keep the Sheet admin-only.

### 9. Wire focused invalidation after audited mutations

Update existing frontend mutation success handlers where practical to invalidate relevant audit query prefixes.

Do not synthesize audit events locally.

Do not block normal mutation success when audit history refetch is unavailable.

### 10. Add defensive rendering and security checks

Implement:

- unknown action fallback;
- unexpected entity-type warning;
- malformed value warning;
- sensitive-key redaction;
- bounded recursive rendering;
- no console logging of payloads.

### 11. Verify shadcn-first implementation

Review all new components against the reference catalog.

Remove unnecessary custom low-level primitives.

Confirm generated `frontend/src/components/ui/*` files remain unmodified.

### 12. Update progress documentation

Update `context/progress-tracker.md` with actual implementation and verification state.

Do not mark Unit 39 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use existing frontend packages:

- TanStack Query;
- `nuqs`;
- shadcn/ui;
- Lucide React;
- existing date utilities;
- existing Zod infrastructure where runtime narrowing schemas are useful.

Add required shadcn components just in time through the approved CLI workflow.

Do not add:

- a timeline package;
- a JSON viewer package;
- a diff library;
- a data-grid package;
- an export package;
- another query/state library;
- a global event-store client;
- a new frontend testing framework solely for this unit.

## Verification checklist

- [ ] No global audit route or sidebar item is added.
- [ ] No audit mutation, delete, restore, rollback, or export UI exists.
- [ ] Match-report audit uses `GET /api/match-reports/{reportId}/audit`.
- [ ] Staff-user audit uses `GET /api/users/{userId}/audit`.
- [ ] Both histories use backend pagination.
- [ ] Action and date filters are sent to the backend.
- [ ] Audit filtering is not performed over an unbounded client-side history.
- [ ] Audit JSON fields are treated as `unknown` and narrowed safely.
- [ ] No `any` is used for audit contracts.
- [ ] All initial Unit 38 action codes have centralized Bosnian labels.
- [ ] Unknown action codes remain visible through a safe fallback.
- [ ] Unknown actions do not create mutation controls.
- [ ] Entity-type mismatches are handled safely.
- [ ] Audit items display actor, exact time, action, and semantic changes.
- [ ] Backend ordering is preserved.
- [ ] No fake pre-Unit-38 audit history is generated.
- [ ] Empty history explains that no audit entries are available without inventing older events.
- [ ] Raw JSON is not the default presentation.
- [ ] Known actions use explicit semantic presenters.
- [ ] Generic structured rendering is bounded and HTML-safe.
- [ ] `null` displays as not entered/not set.
- [ ] Numeric `0` displays as `0`.
- [ ] Booleans display as localized `Da`/`Ne`.
- [ ] Forbidden sensitive keys are redacted and reported safely.
- [ ] Audit payloads are not logged to console or analytics.
- [ ] Staff action labels reuse Unit 24 role/status/permission/scope mappings.
- [ ] Staff access changes show role, permission, scope, and selected-team diffs.
- [ ] Selected team IDs resolve to real team names.
- [ ] Unresolved team IDs remain visible through a safe technical fallback.
- [ ] Invitation audit never shows setup credentials or copy-link actions.
- [ ] Staff lifecycle audit shows previous/new account status.
- [ ] Match-report labels reuse Unit 37 workflow status mappings.
- [ ] Statistics labels reuse Unit 36 field registry.
- [ ] Statistics audit groups changes by appearance/player.
- [ ] Appearance IDs resolve through the existing lineup snapshot.
- [ ] Missing lineup-name mapping does not hide audit events.
- [ ] Statistics audit preserves `null` versus `0`.
- [ ] Goalkeeper row additions/removals are understandable.
- [ ] Existing match `Revizija` UI remains intact under `Tok izvještaja`.
- [ ] `Historija promjena` is added as a nested report-review view.
- [ ] Match audit nested view and filters use scoped `nuqs` URL state.
- [ ] Report audit access follows backend report visibility/team scope.
- [ ] Inaccessible report history does not leak existence.
- [ ] `/users` includes an admin-only `Historija promjena` row action.
- [ ] Staff audit opens in a shadcn Sheet.
- [ ] Selected staff audit Sheet state is URL-addressable through `nuqs`.
- [ ] Closing the Sheet clears audit-specific URL state.
- [ ] Opening another user resets audit filters/page.
- [ ] Refresh restores the Sheet only when access remains valid.
- [ ] Non-admin users do not request or see staff audit history.
- [ ] Stale admin `403` closes/disables the Sheet and refetches session state.
- [ ] Disabled and invited target-user histories remain visible to admins.
- [ ] Current staff summary is clearly separated from historical changes.
- [ ] Loading uses approved Skeleton patterns.
- [ ] Empty states use approved Empty patterns.
- [ ] Errors use readable Alert/retry behavior.
- [ ] Pagination is bounded and server-backed.
- [ ] Audit query keys are stable and entity-specific.
- [ ] Audit server data is not stored in Zustand, React Context, localStorage, or sessionStorage.
- [ ] Existing audited business mutations invalidate relevant audit query prefixes where practical.
- [ ] The frontend never synthesizes audit records from mutation requests.
- [ ] Audit history does not poll continuously.
- [ ] New UI follows shadcn-first component-selection guidance.
- [ ] Chat/conversation components are not used for audit history.
- [ ] No third-party timeline or JSON viewer is added.
- [ ] Generated `frontend/src/components/ui/*` files are not manually modified.
- [ ] No raw Tailwind palette classes or hardcoded component colors are introduced.
- [ ] No ad-hoc visual overrides are applied to shadcn components.
- [ ] Frontend imports use `@/` instead of deep relative paths.
- [ ] Visible Bosnian copy uses proper Bosnian Latin characters.
- [ ] Tabs, Sheet, filters, pagination, collapsibles, and event lists are keyboard accessible.
- [ ] Actor/action/change information does not rely only on color or icons.
- [ ] Desktop, tablet, and mobile layouts remain usable.
- [ ] No backend code, migrations, audit codes, authorization rules, or query contracts are changed.
- [ ] No audit coverage for unaudited modules is implied or fabricated.
- [ ] `npm run format` completes successfully.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] Frontend typecheck/build passes using the configured project command.
- [ ] Existing approved frontend tests pass when present.
- [ ] `context/progress-tracker.md` reflects the actual Unit 39 implementation and verification state.
