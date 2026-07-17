# Unit 51: Medical Availability UI

## Goal

Build the complete first frontend experience for coach-safe player availability and restricted injury management using Unit 50.

Add a protected team-scoped `Dostupnost igrača` workspace, availability summary cards, server-filtered player status table, safe availability revision and audit history, authorized append-only availability editing, a separately authorized restricted `Povrede` workspace, injury creation/update/resolution, immutable injury revision history, injury audit history, and player-profile availability integration.

Add one narrowly scoped backend read endpoint for injury-player candidates on the injury occurrence date:

```txt
GET /api/medical/injury-player-candidates
```

Do not mix safe and restricted response contracts, persist restricted medical data in browser storage, expose medical details through coach-safe tables or player overview, add medical documents/treatment workflows, or weaken Unit 50 authorization and privacy boundaries.

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
11. `context/feature-specs/28-player-team-assignment-backend.md`
12. `context/feature-specs/29-players-ui-foundation.md`
13. `context/feature-specs/39-audit-ui-foundation.md`
14. `context/feature-specs/50-medical-availability-backend.md`
15. `context/feature-specs/51-medical-availability-ui.md`

Use relevant frontend skills from `frontend/.agents/` and installed backend Codex skills/plugins when applicable.

Before creating custom UI primitives or interaction patterns:

- check `context/references/shadcn-components.md`;
- prefer suitable shadcn/ui components;
- install required shadcn components just in time through the approved workflow;
- compose app-level components outside `frontend/src/components/ui`;
- do not manually modify generated shadcn primitive files;
- do not hand-build equivalents of existing shadcn primitives without a documented reason.

This unit is primarily frontend work with one narrowly scoped backend read endpoint.

Do not change:

- availability statuses;
- availability revision semantics;
- injury lifecycle;
- medical permissions;
- team-scope rules;
- safe/restricted DTO boundaries;
- audit action codes;
- optimistic concurrency;
- player assignment semantics;
- database schema.

No EF Core migration is expected in Unit 51.

### Scope

This unit introduces:

- protected `/availability` route;
- `Dostupnost igrača` Performance navigation;
- required team selection;
- URL-driven safe availability filters and pagination;
- current-team availability summary cards;
- coach-safe player availability table;
- append-only safe availability edit dialog;
- clear coach-visible-note privacy warning;
- availability revision-history Sheet;
- availability audit-history view;
- conditional restricted `Povrede` tab;
- injury-player candidate search using assignment coverage on `OccurredOn`;
- restricted injury list and filters;
- restricted injury create dialog;
- restricted injury detail Sheet;
- append-only injury detail update;
- explicit injury resolution;
- immutable injury revision history;
- injury audit history;
- safe current-revision conflict handling;
- restricted-query cache removal after permission/scope loss;
- player-detail `Dostupnost` section;
- permission-aware player injury shortcut/surface;
- loading, empty, synthetic-unknown, conflict, inaccessible, and terminal states.

This unit does not introduce:

- diagnosis/body-area fields in safe availability responses or components;
- injury-existence indicators in safe availability;
- medical-document upload;
- scans, images, test results, treatment plans, medication, appointments, or provider information;
- wellness, sleep, RPE, pain scales, or return-to-play protocols;
- automatic availability changes after injury operations;
- automatic injury operations after availability changes;
- availability/injury hard delete;
- injury reopen;
- bulk medical mutation;
- note-content search;
- public medical routes;
- medical data export;
- medical dashboard;
- notifications;
- frontend persistence of restricted responses;
- client-side authorization as the source of truth;
- frontend changes to Unit 50 DTOs or backend privacy rules;
- a new table, column, or migration.

### Required UI-context synchronization

Update `context/ui-context.md`.

Replace the generic visible label:

```txt
Medical / Availability
```

with the Bosnian default label:

```txt
Dostupnost igrača
```

Document two separate surfaces:

```txt
Dostupnost
Povrede
```

Clarify:

- `Dostupnost` is coach-safe and team-scoped;
- `Povrede` is restricted and appears only when the current user has medical-detail permission;
- coach-visible notes are not medical notes;
- restricted diagnosis/body-area/notes never appear in safe tables, cards, player overview, dashboard-safe summaries, or ordinary audit history.

Update the player-detail example so `Dostupnost` is a real section rather than a future placeholder.

Do not rename backend enum values or route segments.

### Minimal injury-player-candidate endpoint

Add:

```txt
GET /api/medical/injury-player-candidates
```

Required query parameters:

```txt
teamId
occurredOn
```

Optional:

```txt
search
page
pageSize
```

Purpose:

- provide players eligible for a restricted injury record on the selected occurrence date;
- support historical injury entry;
- avoid using the existing player team filter, which represents current assignment membership;
- prevent the frontend from reproducing assignment date-range logic.

Authorization:

- authenticated active user;
- caller must currently have injury mutation access:
  - `ADMIN`; or
  - `MEDICAL_STAFF` with `canViewMedicalDetails = true`;
- target team must be in scope;
- explicit out-of-scope team returns `403`;
- other roles, including detail-enabled read-only roles, receive `403`;
- inactive/archived team follows Unit 50 injury-create behavior.

Candidate rules:

- player exists;
- player is not archived;
- one `PlayerTeamAssignment` to `teamId` covers `occurredOn`;
- date boundaries are inclusive;
- historical/current/future assignment records are evaluated relative to `occurredOn`;
- current-day assignment state is not substituted;
- normalized server-side search;
- bounded database pagination;
- deterministic order by display name and player ID;
- no N+1 assignment/player queries.

Return at minimum:

```txt
id
displayName
preferredName
dateOfBirth
eligibleAssignment:
  id
  teamId
  startDate
  endDate
```

Rules:

- no availability status;
- no injury existence;
- no diagnosis/body area/notes;
- no unrestricted player history;
- response is candidate support only;
- `POST /api/medical/injuries` remains authoritative and rechecks all rules;
- a candidate becoming ineligible before create returns the established safe conflict.

Add focused backend tests.

No migration is required.

### Route and navigation

Add:

```txt
/availability
```

Visible navigation label:

```txt
Dostupnost igrača
```

Place it in the Performance group.

Navigation visibility:

- all authenticated active staff who can read coach-safe availability may see it;
- visibility does not imply availability mutation or medical-detail access;
- selected-team users see only scoped teams;
- active state applies to `/availability` regardless of selected nested view;
- collapsed sidebar retains an accessible tooltip/label.

Use a suitable Lucide availability/medical-cross/activity icon following existing icon rules.

Do not create separate top-level routes such as:

```txt
/medical
/injuries
/medical-notes
```

The restricted workspace is nested under the same protected route but remains separately authorized and queried.

### Page structure

Recommended structure:

```txt
PageHeader
Required team selector
Nested view tabs
Selected view content
```

Suggested title:

```txt
Dostupnost igrača
```

Suggested description:

```txt
Pratite operativnu dostupnost igrača i, kada imate dozvolu, vodite zaštićenu evidenciju povreda.
```

Nested views:

```txt
Dostupnost
Povrede
```

Use URL state:

```txt
medicalView=availability
medicalView=injuries
```

Rules:

- default is `availability`;
- `injuries` is rendered and queried only when the session grants `canViewMedicalDetails`;
- unauthorized `medicalView=injuries` normalizes to `availability`;
- clear injury-specific URL state when normalizing;
- team selection applies to both views;
- changing team closes selected availability/injury detail and resets relevant pages;
- do not show a disabled restricted tab that reveals permission configuration;
- do not display `Nemate medical permission` to ordinary users as a tab placeholder.

### Required team selection

Unit 50 safe list/summary require `teamId`.

Use:

```txt
teamId
```

in `nuqs`.

Rules:

- options come from accessible teams/selections;
- admin/all-team users may choose any active accessible team;
- selected-team users see only selected scope;
- when one team is available, it may be selected automatically according to the established route-filter convention;
- when several teams exist and there is no valid URL value, use the existing preferred/default team convention;
- never select an out-of-scope team from stale URL state;
- no team means a neutral team-selection empty state rather than an invalid request;
- team changes reset:
  - safe status/search/page;
  - injury filters/page;
  - selected availability player;
  - selected injury;
  - audit/revision subview state.

Do not store selected team in a medical-specific Zustand store.

### Safe availability view

The default coach-safe view contains:

```txt
Availability summary cards
Filter controls
Player availability table
Availability history Sheet
```

It uses only:

```txt
GET /api/player-availability/summary
GET /api/player-availability
GET /api/players/{playerId}/availability
GET /api/player-availability/{availabilityId}/audit
POST /api/players/{playerId}/availability
```

Do not call injury endpoints from the safe view.

Do not enrich the safe table with restricted data from another query.

### Availability status labels

Centralize:

```txt
AVAILABLE -> Dostupan
LIMITED -> Ograničeno dostupan
UNAVAILABLE -> Nedostupan
REHAB -> Rehabilitacija
UNKNOWN -> Nepoznato
```

Use readable badges and text.

Do not rely only on color or icons.

Suggested short labels for cards may be:

```txt
Dostupni
Ograničeni
Nedostupni
Rehabilitacija
Nepoznato
```

Internal values remain English.

### Availability summary cards

Load:

```txt
GET /api/player-availability/summary?teamId={teamId}
```

Show cards for:

```txt
Ukupno igrača
Dostupni
Ograničeno dostupni
Nedostupni
Rehabilitacija
Nepoznato
```

Rules:

- show exact backend counts;
- do not recalculate from the currently paginated table;
- do not add injury/povreda/diagnosis counts;
- do not infer medical severity;
- counts must be accessible text;
- card clicks may apply the corresponding safe status filter, except total clears it;
- active status filter is visibly indicated using existing token patterns;
- loading uses card skeletons;
- summary failure does not hide the table;
- table failure does not hide an already loaded summary;
- no status pie chart is needed in Unit 51.

### Safe availability URL filters

Use `nuqs`.

Support:

```txt
teamId
availabilityStatus
availabilitySearch
availabilityPage
availabilityPlayerId
availabilityDetailView
availabilityAuditPage
availabilityAuditAction
availabilityAuditDateFrom
availabilityAuditDateTo
```

Suggested detail values:

```txt
availabilityDetailView=revisions
availabilityDetailView=audit
```

Rules:

- search is debounced;
- status updates immediately;
- filter changes reset availability page;
- invalid enum/page values normalize safely;
- selected player detail is URL-addressable;
- opening another player resets detail audit filters;
- synthetic unknown items have no availability audit ID;
- do not put coach-visible note text in the URL;
- do not put expected-return date in the URL.

### Player availability table

Use TanStack Table and shadcn table primitives.

Recommended columns:

- player;
- current selection/team;
- availability status;
- effective date;
- expected return;
- coach-visible note;
- last recorded by/time;
- actions.

Rules:

- player list population and synthetic unknown come from the backend;
- do not fetch one history request per table row;
- do not infer unknown from missing fields client-side when backend already returns status;
- expected return shows:
  - localized date; or
  - `Nije uneseno`;
- coach-visible note may be shown in a bounded/truncated cell with tooltip/expanded safe text;
- label the column:
  - `Napomena vidljiva stručnom štabu`;
- do not label it `Medicinska napomena`;
- no injury icon/count/status;
- no diagnosis/body area;
- no medical-detail permission marker;
- row opens the safe history Sheet;
- update action appears only when row `allowedActions` contains `UPDATE`;
- synthetic unknown row can still be updated with `expectedCurrentRevisionId = null`.

Do not use restricted injury data to determine a safe status badge.

### Safe availability states

#### No assigned players

Show:

```txt
Nema aktivno raspoređenih igrača za odabranu selekciju.
```

Do not offer player creation or assignment changes from this module.

#### All players synthetic unknown

Show the normal table with `Nepoznato`.

Optionally show a neutral informational Alert:

```txt
Za ovu selekciju još nije evidentirano potvrđeno stanje dostupnosti.
```

Do not call this an error.

#### No filter matches

Show:

```txt
Nema igrača koji odgovaraju odabranim filterima.
```

Provide `Očisti filtere`.

#### Loading

Use table Skeleton.

#### Error

Use Alert and retry.

Do not switch to restricted queries as a fallback.

### Availability edit dialog

Use shadcn `Dialog`, React Hook Form, and Zod.

Render only when backend/session state permits safe availability mutation.

Fields:

```txt
status
effectiveOn
expectedReturnOn
coachVisibleNote
```

Hidden trusted context:

```txt
playerId from selected row
teamId from selected team
expectedCurrentRevisionId from current safe response
```

Rules:

- status required;
- effective date required;
- no future date client-side hint;
- backend clock remains authoritative;
- expected return field appears/enables only for:
  - `LIMITED`;
  - `UNAVAILABLE`;
  - `REHAB`;
- selecting `AVAILABLE` or `UNKNOWN` clears expected return;
- expected return cannot precede effective date;
- coach-visible note is optional and bounded;
- no injury selector;
- no body area;
- no diagnosis;
- no restricted notes;
- no medical detail query is loaded for this dialog;
- do not use one form schema shared with injury forms.

### Coach-visible note warning

The form must clearly label:

```txt
Napomena vidljiva stručnom štabu
```

Show an Alert/helper text:

```txt
Ovu napomenu mogu vidjeti svi ovlašteni članovi stručnog štaba za selekciju. Ne unosite dijagnozu, rezultate pregleda, terapiju ili druge osjetljive medicinske detalje.
```

Rules:

- warning is always visible when the field is present;
- do not prefill from injury details;
- do not offer copy-from-injury;
- do not attempt unreliable medical-text detection in the browser;
- backend remains authoritative for bounds and status/date rules.

### Availability mutation and concurrency

Call:

```txt
POST /api/players/{playerId}/availability
```

Send the full safe snapshot and expected revision.

On success:

- close dialog;
- show generic success feedback:
  - `Dostupnost igrača je ažurirana.`;
- invalidate:
  - safe availability list;
  - summary;
  - player safe history;
  - availability audit;
  - player detail safe availability section;
  - future dashboard availability queries where key composition already exists;
- do not invalidate or refetch restricted injuries automatically;
- open or refresh history Sheet when appropriate.

Do not include the note contents in toast messages.

### Availability revision conflict

On `409` current-revision mismatch:

- keep the dialog open;
- preserve the user's entered values;
- refetch current safe row/history;
- show:
  - `Stanje dostupnosti je u međuvremenu promijenjeno. Pregledajte najnoviju reviziju prije ponovnog čuvanja.`;
- provide:
  - `Učitaj najnovije stanje`;
  - `Odustani`;
- updating the expected revision must require the user to review/reconfirm the form;
- do not automatically resubmit;
- do not silently overwrite newer data.

Other conflicts, such as assignment eligibility changes, use clear safe feedback and authoritative refetch.

### Availability history Sheet

Use shadcn `Sheet`.

Selected state:

```txt
availabilityPlayerId
```

Load:

```txt
GET /api/players/{playerId}/availability?teamId={teamId}
```

Show:

- player summary;
- selected team;
- current safe status;
- expected return;
- coach-visible note;
- current revision;
- revision list;
- update action when allowed.

Use nested views:

```txt
Revizije dostupnosti
Historija promjena
```

Do not call them medical records.

Revision list shows:

```txt
revision number
status
effective date
expected return
coach-visible note
recorded by
recorded time
```

Rules:

- safe operational history only;
- no injury ID or existence;
- no restricted diagnosis/body area/notes;
- newest-first according to backend;
- pagination uses backend response where provided;
- preserve zero/null/date semantics;
- archived/transferred historical player/team references use returned safe summaries.

For a synthesized unknown row without an aggregate:

- show:
  - `Još nema evidentiranih revizija dostupnosti.`;
- no audit tab/request;
- update may still be available.

### Availability audit view

When the selected safe aggregate has an ID, load:

```txt
GET /api/player-availability/{availabilityId}/audit
```

Reuse Unit 39 audit components.

Add action label:

```txt
PLAYER_AVAILABILITY_RECORDED -> Zabilježena dostupnost igrača
```

Add safe field labels:

```txt
playerId -> Igrač
teamId -> Selekcija
previousStatus -> Prethodni status
newStatus -> Novi status
previousEffectiveOn -> Prethodni datum važenja
newEffectiveOn -> Novi datum važenja
previousExpectedReturnOn -> Prethodni očekivani povratak
newExpectedReturnOn -> Novi očekivani povratak
coachVisibleNoteChanged -> Izmijenjena napomena za stručni štab
previousRevisionId -> Prethodna revizija
newRevisionId -> Nova revizija
```

Rules:

- note contents never appear;
- injury data never appears;
- no raw JSON by default;
- unknown fields use Unit 39 bounded safe fallback;
- audit filters/page are scoped to availability detail state;
- no global availability audit page.

### Restricted injuries view

Render only when the session has:

```txt
canViewMedicalDetails = true
```

and the selected team is in scope.

Use only restricted Unit 50 endpoints:

```txt
GET /api/medical/injuries
GET /api/medical/injuries/{id}
GET /api/medical/injuries/{id}/revisions
GET /api/medical/injuries/{id}/audit
POST /api/medical/injuries
PATCH /api/medical/injuries/{id}
POST /api/medical/injuries/{id}/resolve
```

Do not use restricted responses to enrich safe availability cards/table/history.

### Restricted view header

Suggested heading:

```txt
Povrede
```

Suggested description:

```txt
Zaštićena evidencija povreda dostupna je samo korisnicima sa posebnom medicinskom dozvolom.
```

Do not display the user's permission flag value in the UI.

Primary action:

```txt
Evidentiraj povredu
```

Show only when the current session indicates an injury mutation role:

- `ADMIN`; or
- `MEDICAL_STAFF` with `canViewMedicalDetails = true`.

Backend remains authoritative.

Read-only detail-enabled roles see no create/update/resolve actions.

### Restricted URL state

Use `nuqs`.

Support:

```txt
medicalView=injuries
teamId
injuryPlayerId
injuryStatus
injuryOccurredFrom
injuryOccurredTo
injuryPage
injuryId
injuryDetailView
injuryRevisionPage
injuryAuditPage
injuryAuditAction
injuryAuditDateFrom
injuryAuditDateTo
```

Suggested detail views:

```txt
injuryDetailView=details
injuryDetailView=revisions
injuryDetailView=audit
```

Rules:

- do not place diagnosis, body area, note text, or occurrence description in URL;
- changing team resets all injury state;
- opening another injury resets revision/audit pages;
- losing permission clears all injury-specific URL parameters;
- logout/account disable/session loss follows global auth behavior;
- stale direct injury URL uses safe inaccessible handling.

### Injury filters

Use:

```txt
teamId
playerId
status
occurredFrom
occurredTo
page
```

No search input over:

- diagnosis;
- body area;
- restricted notes.

Player filter uses an existing scoped player selector appropriate for read filtering.

The filter is not proof of historical assignment eligibility because it only narrows already-authorized injury records.

Status labels:

```txt
OPEN -> Otvorena
RESOLVED -> Riješena
```

Use server pagination and ordering.

Provide `Očisti filtere`.

### Restricted injury table

Use TanStack Table and shadcn table primitives.

Recommended columns:

- player;
- team;
- occurred date;
- status;
- body area;
- diagnosis;
- revision count;
- resolved date;
- actions.

Rules:

- data comes only from the restricted list DTO;
- restricted notes are not shown in the compact table;
- no note snippet or tooltip;
- no client-side joining with safe availability;
- no availability status column inferred from injury;
- diagnosis/body area are displayed only inside this restricted view;
- long values truncate safely with access-preserving tooltip/detail opening;
- row opens injury detail Sheet;
- mutation actions use detail `allowedActions` where practical;
- no bulk resolve/delete;
- no CSV export.

### Restricted injury list states

#### No injury records

Show:

```txt
Nema evidentiranih povreda za odabrane filtere.
```

Authorized mutation roles may see `Evidentiraj povredu`.

Do not imply that every player is medically healthy.

#### Loading

Use restricted table Skeleton.

#### Error

Use a generic restricted-data error:

```txt
Zaštićeni medicinski podaci nisu mogli biti učitani.
```

Do not echo diagnosis/note-containing server text.

#### Permission loss

Immediately:

- stop restricted queries;
- close injury Sheet/dialogs;
- clear injury URL state;
- remove restricted query cache;
- switch to safe availability view;
- show:
  - `Pristup zaštićenim medicinskim podacima je promijenjen.`;
- do not keep cached diagnosis/notes visible.

### Restricted query-cache rules

Medical-detail responses require stricter frontend cache handling.

Requirements:

- never persist restricted queries to:
  - localStorage;
  - sessionStorage;
  - IndexedDB;
  - persisted TanStack cache;
  - Zustand;
  - React Context;
- query keys contain only opaque IDs and filters, never medical text;
- do not prefetch injury detail on row hover/focus;
- use `staleTime: 0`;
- use `gcTime: 0` or the shortest repository-approved equivalent for unmounted restricted queries;
- on loss of `canViewMedicalDetails`, team scope, active account, or `403`:
  - cancel restricted queries;
  - remove restricted query keys;
  - clear selected injury state;
- ordinary safe availability cache remains unaffected;
- do not log query responses to console;
- do not expose restricted data through dev-only debug UI committed to the app.

Authorization must still be checked by the backend on every request.

### Injury create dialog

Use shadcn `Dialog`, React Hook Form, and Zod.

Fields:

```txt
team
occurredOn
player
bodyArea
diagnosis
restrictedNotes
```

Recommended workflow:

1. select team;
2. select occurrence date;
3. load eligible player candidates;
4. select player;
5. enter restricted details;
6. confirm creation.

Rules:

- team is required and limited to injury-mutation scope;
- occurrence date required and non-future client-side hint;
- player selector uses the new candidate endpoint;
- changing team/date clears selected player;
- body area optional and bounded;
- diagnosis optional and bounded;
- restricted notes optional and bounded;
- at least one detail field required;
- no availability status;
- no expected return;
- no coach-visible note;
- no automatic safe availability update;
- no file upload;
- no free-text player identity;
- mutation uses existing CSRF behavior.

### Injury candidate selector

Call:

```txt
GET /api/medical/injury-player-candidates
```

only when:

- restricted view is authorized;
- team exists;
- valid occurrence date exists;
- create dialog is open.

Provide:

- debounced search;
- server pagination;
- loading/empty/error states;
- assignment date summary;
- one explicit player selection.

Do not:

- use current-team player filter;
- fetch all players;
- fuzzy-match names;
- allow manual player ID entry;
- infer eligibility from current assignment badges;
- keep candidates cached after permission loss.

### Injury create success

On success:

- close dialog;
- show generic feedback:
  - `Povreda je evidentirana.`;
- invalidate:
  - restricted injury list;
  - created injury detail;
  - player-filtered injury queries;
  - injury audit;
  - player-detail restricted injury section;
- open the created injury detail when the response includes ID;
- do not change or invalidate safe availability as though its status changed;
- do not mention diagnosis in toast.

### Injury detail Sheet

Use shadcn `Sheet`.

Store selected opaque ID in:

```txt
injuryId
```

Show a clear restricted header:

```txt
Zaštićeni medicinski detalji
```

Sections/views:

```txt
Detalji
Revizije
Historija promjena
```

Header shows:

- player;
- team;
- occurred date;
- status;
- resolved date when present;
- creator;
- allowed actions.

Current restricted detail shows:

- body area;
- diagnosis;
- restricted notes;
- current revision number;
- recorded by/time.

Rules:

- do not show coach-visible availability note automatically;
- do not infer current availability;
- no player-wide unrelated medical data;
- no raw IDs as primary display;
- no download/copy/export action for notes;
- sheet closes and cache clears on permission loss.

### Injury update dialog

Render only when detail `allowedActions` contains:

```txt
UPDATE
```

Use a dedicated restricted form.

Fields:

```txt
bodyArea
diagnosis
restrictedNotes
```

Trusted concurrency value:

```txt
expectedCurrentRevisionId
```

Rules:

- full current restricted snapshot is loaded from injury detail;
- at least one field remains present;
- no player/team/occurred date/status editing;
- no availability fields;
- no note content in toast/log;
- exact no-op submission is disabled where practical;
- backend remains authoritative.

On success:

- close dialog;
- show:
  - `Medicinski detalji su ažurirani.`;
- invalidate detail/revisions/list/audit;
- do not update safe availability.

### Injury update conflict

On `409`:

- keep form open;
- preserve user values;
- refetch restricted detail/revisions;
- show:
  - `Zapis je u međuvremenu izmijenjen. Pregledajte najnoviju reviziju prije ponovnog čuvanja.`;
- provide explicit reload-latest action;
- do not auto-merge restricted notes;
- do not auto-resubmit;
- do not display the conflicting values in a toast.

### Injury resolve dialog

Render only when detail `allowedActions` contains:

```txt
RESOLVE
```

Use shadcn `Alert Dialog` with a date field or a preceding small Dialog plus confirmation according to existing form conventions.

Required field:

```txt
resolvedOn
```

Trusted:

```txt
expectedCurrentRevisionId
```

Explain:

- injury record becomes terminal `Riješena`;
- record cannot be reopened;
- resolving does not automatically set availability to `Dostupan`;
- availability must be updated separately when operationally appropriate.

On success:

- refetch detail/list/revisions/audit;
- show:
  - `Povreda je označena kao riješena.`;
- do not trigger safe availability mutation;
- do not expose resolve action again.

### Injury revision history

Load:

```txt
GET /api/medical/injuries/{id}/revisions
```

only for authorized users and when the revision view is active.

Display:

- revision number;
- body area;
- diagnosis;
- restricted notes;
- recorded by;
- recorded time.

Rules:

- newest-first backend order;
- pagination;
- complete restricted snapshots;
- no client-generated diff required;
- no copy/export action;
- no note-content URL/search;
- no raw JSON;
- old revisions remain visibly immutable;
- factual correction after resolution appears as another revision without reopening status.

### Injury audit history

Load:

```txt
GET /api/medical/injuries/{id}/audit
```

Reuse Unit 39 audit components.

Action labels:

```txt
INJURY_RECORD_CREATED -> Kreiran zapis o povredi
INJURY_RECORD_UPDATED -> Ažuriran zapis o povredi
INJURY_RECORD_RESOLVED -> Povreda označena kao riješena
```

Safe field labels:

```txt
playerId -> Igrač
teamId -> Selekcija
occurredOn -> Datum nastanka
status -> Status
resolvedOn -> Datum rješavanja
previousRevisionId -> Prethodna revizija
newRevisionId -> Nova revizija
changedFieldKeys -> Izmijenjena polja
```

Map changed keys:

```txt
bodyArea -> Dio tijela
diagnosis -> Dijagnoza
restrictedNotes -> Zaštićena medicinska napomena
```

Rules:

- audit shows changed field names only;
- no old/new restricted values;
- no diagnosis/note body copied into audit;
- revision history is the detailed authoritative history;
- filters/page are scoped to selected injury;
- no global medical audit page.

### Player-detail integration

Extend the existing `/players/:playerId` composition with a real:

```txt
Dostupnost
```

section or tab.

Do not rebuild the player shell.

The safe section is available to all ordinary scoped readers.

Load:

```txt
GET /api/players/{playerId}/availability
```

Show:

- current safe availability per accessible team;
- status;
- effective date;
- expected return;
- coach-visible note;
- revision history;
- safe update action when allowed.

Rules:

- multi-team history respects current user scope;
- do not merge statuses from different teams;
- no global “medical status” derived from multiple team records;
- no injury existence indicator;
- no body area/diagnosis/restricted notes;
- archived player safe history follows Unit 50 visibility;
- update dialog may be opened with player/team locked.

### Restricted player injury shortcut

When:

```txt
canViewMedicalDetails = true
```

show a visually separate restricted action/section:

```txt
Otvori zaštićenu evidenciju povreda
```

Behavior:

- navigate to:
  - `/availability?medicalView=injuries&injuryPlayerId={playerId}`;
- preserve/select an authorized team when possible;
- do not preload injury data on player detail;
- do not show injury count in the safe player overview;
- do not show latest diagnosis/body area;
- mutation roles may also receive:
  - `Evidentiraj povredu`;
- player-context create keeps player fixed but still requires:
  - team;
  - occurred date;
  - backend eligibility validation.

If the user has no detail permission, the restricted action/section does not exist.

Do not show a locked-card placeholder that reveals hidden injury capability.

### Player-context injury creation

For admin or authorized medical staff:

- open the shared injury create dialog;
- lock `playerId`;
- user selects team and occurred date;
- validate the locked player through the candidate endpoint or backend create contract;
- if the player is not eligible for the selected date/team, show a safe field error;
- do not silently switch player or team;
- no safe availability change occurs.

### Role-aware UI behavior

#### All authenticated active scoped roles

May:

- see `Dostupnost igrača`;
- read safe summary;
- read safe table;
- read safe player availability history;
- read availability audit according to Unit 50.

#### `ADMIN`

May:

- update safe availability;
- access restricted injuries across scoped/all teams;
- create/update/resolve injuries.

#### `MEDICAL_STAFF` without detail permission

May:

- read safe availability;
- update safe availability in scope;
- cannot see the `Povrede` tab;
- cannot load restricted queries;
- cannot create/update/resolve injury records.

#### `MEDICAL_STAFF` with detail permission

May:

- read/update safe availability in scope;
- read restricted injuries in scope;
- create/update/resolve injuries in scope.

#### Other roles with `canViewMedicalDetails = true`

May:

- read restricted injury list/detail/revisions/audit in scope;
- cannot create/update/resolve injuries;
- safe availability remains read-only unless another backend rule permits it.

#### Other roles without detail permission

See only safe availability.

Backend endpoint authorization remains authoritative for every operation.

### Permission and scope changes

React to session changes immediately.

When:

- `canViewMedicalDetails` becomes false;
- selected team leaves scope;
- account becomes disabled;
- session expires;
- role changes remove injury mutation;

perform the appropriate subset:

- cancel in-flight restricted requests;
- close restricted dialogs/Sheet;
- clear injury URL state;
- remove restricted query cache;
- hide restricted tab/actions;
- refetch safe availability/session/navigation;
- preserve only non-sensitive filters that remain valid.

Do not continue rendering cached restricted data while refetching authorization.

### Query keys

Use TanStack Query.

Define separate namespaces:

```txt
availabilitySummary(teamId)
availabilityList(filters)
playerAvailabilityHistory(playerId, teamId, page)
availabilityAudit(availabilityId, filters)

injuryPlayerCandidates(filters)
injuryList(filters)
injuryDetail(injuryId)
injuryRevisions(injuryId, page)
injuryAudit(injuryId, filters)
```

Rules:

- safe and restricted keys are never mixed;
- restricted keys contain no diagnosis/note/body-area values;
- do not store server data in Zustand or Context;
- no persisted cache for restricted keys;
- existing Unit 39 audit generic components may receive restricted data only within authorized mounted surfaces.

### Focused invalidation

After availability mutation:

- invalidate summary;
- safe list;
- selected player safe history;
- availability audit;
- player-detail safe section;
- relevant future dashboard availability query keys.

Do not invalidate injuries as if coupled.

After injury create/update/resolve:

- invalidate restricted list/detail/revisions/audit;
- relevant player-filtered injury queries;
- player restricted shortcut/surface if loaded.

Do not invalidate safe availability as though changed.

On team scope/session changes:

- remove restricted queries, not the full TanStack cache.

Do not optimistically invent revisions or statuses.

### API client organization

Create or extend:

```txt
frontend/src/features/availability/
frontend/src/features/medical/
```

Suggested structure:

```txt
frontend/src/features/availability/
├── api/
├── components/
├── hooks/
├── schemas/
├── types/
├── utils/
└── index.ts

frontend/src/features/medical/
├── api/
├── components/
├── hooks/
├── schemas/
├── types/
├── utils/
└── index.ts
```

Availability owns:

- safe route view;
- summary;
- safe table;
- edit dialog;
- safe revisions/audit;
- player safe integration.

Medical owns:

- restricted tab;
- candidate selector;
- injury forms;
- restricted list/detail/revisions/audit;
- restricted cache cleanup.

Do not import restricted injury DTOs into safe availability table/card components.

Avoid circular imports.

Use explicit TypeScript DTOs.

Avoid `any`.

### Safe and restricted TypeScript types

Define separate types mirroring Unit 50.

Safe types must not contain optional restricted fields.

Do not use:

```ts
type Availability = {
  diagnosis?: string | null
  restrictedNotes?: string | null
}
```

Preferred separation:

```txt
SafeAvailability...
RestrictedInjury...
```

Add compile-time and test-level safeguards where practical.

Do not create a broad `MedicalRecord` union passed throughout ordinary player pages.

### Shadcn-first component selection

Review all new interactions against:

```txt
context/references/shadcn-components.md
```

Prefer:

- `Tabs`;
- `Table`;
- `Card`;
- `Dialog`;
- `Alert Dialog`;
- `Sheet`;
- `Dropdown Menu`;
- `Badge`;
- `Alert`;
- `Empty`;
- `Skeleton`;
- `Input`;
- `Textarea`;
- `Select`;
- `Combobox`;
- `Command`;
- `Popover`;
- `Calendar`;
- `Tooltip`;
- `Collapsible`;
- `Pagination`;
- `Scroll Area`;
- `Sonner`;
- `Button`.

Do not add:

- a medical UI library;
- a timeline package;
- a data-grid package;
- an editor/rich-text package;
- a full-text search package;
- a form state library;
- another query/state library.

Do not modify generated shadcn primitives.

Do not apply ad-hoc visual overrides.

### Localization

Visible copy is Bosnian Latin with proper characters.

Suggested labels:

```txt
Dostupnost igrača
Dostupnost
Povrede
Ukupno igrača
Dostupni
Ograničeno dostupni
Nedostupni
Rehabilitacija
Nepoznato
Ažuriraj dostupnost
Datum važenja
Očekivani povratak
Napomena vidljiva stručnom štabu
Revizije dostupnosti
Historija promjena
Evidentiraj povredu
Otvorena
Riješena
Datum nastanka
Dio tijela
Dijagnoza
Zaštićena medicinska napomena
Ažuriraj medicinske detalje
Označi kao riješenu
Revizije medicinskog zapisa
Zaštićeni medicinski detalji
```

Internal API values remain English.

Do not show raw action/status codes.

### Accessibility

Requirements:

- route tabs are keyboard accessible;
- summary cards used as filters are real buttons or contain accessible controls;
- safe status includes readable text;
- availability table remains semantic;
- edit form labels clearly identify coach-visible disclosure;
- privacy warning is announced/readable;
- date fields have associated labels;
- restricted tab and injury Sheet maintain focus correctly;
- candidate selector is keyboard navigable;
- restricted notes textarea has a clear label and description;
- resolve/update confirmations explain consequences;
- icon-only actions have accessible names/tooltips;
- status does not rely only on color;
- tables use contained horizontal scrolling;
- Sheet/dialog close restores focus;
- permission loss removes focus from restricted content safely;
- no destructive button receives automatic focus.

### Responsive behavior

Desktop/tablet are primary.

Requirements:

- summary cards wrap cleanly;
- safe/injury tables use contained horizontal scrolling;
- coach-visible note does not force page-wide overflow;
- filters stack on mobile;
- availability/injury detail Sheets use full mobile width;
- forms remain usable on smaller screens;
- restricted notes remain readable but bounded;
- nested detail tabs remain keyboard/mobile usable;
- player-detail safe section stacks;
- no separate mobile product.

### Error handling

Use existing ProblemDetails handling.

Expected behavior:

- `401`:
  - existing auth/session flow;
  - restricted cache removed;
- `403` safe endpoint:
  - scope/session refetch and safe feedback;
- `403` restricted endpoint:
  - immediately close/clear restricted surfaces and cache;
- `404`:
  - safe missing/inaccessible behavior;
- `409`:
  - revision conflict;
  - assignment eligibility conflict;
  - lifecycle conflict;
  - concurrent create;
  - refetch authoritative state;
- `422`:
  - map form-level/field-level validation;
- network error:
  - preserve form values where safe;
  - retry without leaking contents into logs.

Do not display:

- restricted backend values in toast messages;
- raw exception text;
- hidden injury existence;
- medical content in console logs;
- request bodies;
- current restricted revision IDs in generic error messages.

### Security and browser privacy checks

Required manual/code review:

- no restricted response in local/session storage;
- no query persistence;
- no injury data in safe component props;
- no diagnosis/note in route/query string;
- no medical content in toast;
- no console logging;
- no analytics event payload;
- no clipboard/export action;
- no injury prefetch for unauthorized users;
- no restricted fetch before permission check;
- restricted cache removed on permission/scope loss;
- safe browser back navigation does not reveal removed restricted content after permission loss;
- safe availability remains usable after restricted cache cleanup.

### Tests and verification approach

Add backend tests for the injury candidate endpoint.

Add frontend tests only if the repository already has an approved frontend testing foundation.

When frontend tests exist, prioritize:

- safe/restricted TypeScript contract separation;
- status labels;
- summary-card status filtering;
- synthetic unknown rendering;
- expected-return form rules;
- coach-visible privacy warning;
- availability revision conflict behavior;
- restricted tab permission gating;
- restricted query `enabled` conditions;
- restricted cache cleanup on session/scope change;
- injury candidate parameters;
- create form field separation;
- injury mutation role visibility;
- resolve warning;
- audit field/action redaction mappings;
- player-detail restricted shortcut absence/presence;
- URL-state clearing after permission loss.

Do not introduce a new frontend testing framework solely for Unit 51.

Manual verification must cover:

- all roles;
- all permission-flag combinations;
- selected/all-team scope;
- team switching;
- safe summary/list/search/status/page;
- synthetic unknown;
- availability create/update/no-op;
- concurrency conflict;
- assignment eligibility change;
- note warning and safe display;
- safe revision history;
- safe audit;
- restricted tab hidden;
- restricted read-only role;
- restricted medical mutation role;
- historical injury candidate selection;
- injury create;
- injury update/no-op/conflict;
- injury resolve;
- resolved factual revision update;
- injury revision history;
- injury audit redaction;
- permission removal while detail is open;
- team-scope removal while restricted data is loaded;
- player safe integration;
- player restricted shortcut;
- responsive behavior;
- keyboard/focus behavior.

### Documentation synchronization

Update:

```txt
context/ui-context.md
context/progress-tracker.md
```

Record:

- `/availability` route;
- Bosnian navigation label;
- safe/restricted nested views;
- injury candidate endpoint;
- summary/table/edit/history surfaces;
- coach-visible note warning;
- restricted cache policy;
- injury workflows;
- player-detail integration;
- authorization behavior;
- audit mappings;
- verification results;
- intentionally deferred documents, treatment plans, wellness, automation, export, and dashboard composition.

If implementation reveals a mismatch in Unit 50 safe/restricted DTOs or session permission flags, update the relevant context/spec before continuing.

Do not silently merge disclosure tiers.

## Implementation

### 1. Add the injury-player-candidate backend query

Implement:

```txt
GET /api/medical/injury-player-candidates
```

Reuse Unit 28 assignment date-range logic and Unit 50 injury-mutation authorization.

Add search/pagination and focused tests.

No migration.

### 2. Add safe availability frontend contracts

Create typed clients/hooks for:

- summary;
- list;
- player history;
- safe mutation;
- availability audit.

Keep types free of restricted fields.

### 3. Add restricted medical frontend contracts

Create separately typed clients/hooks for:

- player candidates;
- injury list;
- detail;
- revisions;
- audit;
- create;
- update;
- resolve.

Do not share broad DTOs with availability.

### 4. Add route and navigation

Create:

```txt
/availability
```

Add `Dostupnost igrača` to Performance navigation.

Add required team selection and nested permission-aware views.

### 5. Build availability summary and table

Implement:

- backend summary cards;
- safe URL filters;
- TanStack Table;
- synthetic unknown;
- loading/empty/error states;
- backend allowed actions.

### 6. Build append-only availability editing

Implement:

- safe form only;
- expected return rules;
- coach-visible note warning;
- expected revision;
- conflict/reload flow;
- focused invalidation.

### 7. Build availability history and audit

Implement:

- URL-addressable Sheet;
- safe revision history;
- synthetic-unknown state;
- Unit 39 audit reuse;
- safe action/field mappings.

### 8. Build restricted injuries workspace

Implement:

- permission-gated tab;
- restricted filters/table;
- create action visibility;
- no note snippets;
- restricted states.

### 9. Build injury candidate and create flow

Implement team/date-dependent candidate search and restricted injury creation.

Do not use current-assignment-only player filters.

### 10. Build injury detail, revision, update, and resolve flows

Implement:

- restricted Sheet;
- append-only update;
- expected revision conflict;
- explicit resolve;
- immutable revision history;
- safe focused invalidation.

### 11. Build injury audit history

Reuse Unit 39 audit components.

Show changed field names only, never restricted values.

### 12. Implement restricted cache cleanup

Add one centralized hook/service responding to:

- permission loss;
- team-scope loss;
- session expiration;
- restricted `403`.

Cancel and remove only restricted query namespaces and clear injury URL state.

### 13. Integrate player detail

Add safe `Dostupnost` content and permission-aware restricted injury shortcut/create flow.

Do not leak injury existence/details into safe overview.

### 14. Verify shadcn-first implementation

Review new UI against the component catalogue.

Remove unnecessary custom primitives.

Confirm generated `frontend/src/components/ui/*` files remain unmodified.

### 15. Update documentation

Update `context/ui-context.md` and `context/progress-tracker.md`.

Do not mark Unit 51 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use existing backend/frontend packages and infrastructure:

- ASP.NET Core 8;
- Unit 50 availability/injury APIs;
- Unit 39 shared audit components;
- Unit 29 player detail shell;
- TanStack Query;
- `nuqs`;
- React Hook Form;
- Zod;
- TanStack Table;
- shadcn/ui;
- Lucide React;
- existing session/team/player/date/ProblemDetails utilities.

Add required shadcn components just in time through the approved CLI workflow.

Do not add:

- medical UI libraries;
- rich-text editors;
- full-text search packages;
- data-grid packages;
- timeline packages;
- another form/query/state library;
- cache-persistence packages;
- analytics packages;
- frontend encryption packages;
- a new frontend testing framework solely for this unit.

## Verification checklist

- [ ] `GET /api/medical/injury-player-candidates` exists.
- [ ] Injury candidate query requires team and occurrence date.
- [ ] Candidate eligibility uses assignment coverage on `OccurredOn`.
- [ ] Current-assignment-only filtering is not used.
- [ ] Candidate endpoint is limited to actual injury mutation roles.
- [ ] Read-only detail-enabled roles cannot use the candidate endpoint.
- [ ] Candidate response contains no availability/injury/medical fields.
- [ ] Candidate query uses bounded server search/pagination and deterministic order.
- [ ] Injury create endpoint still revalidates authorization and eligibility.
- [ ] No Unit 51 migration is added.
- [ ] `/availability` exists.
- [ ] Performance navigation uses `Dostupnost igrača`.
- [ ] All safe availability readers may access the route for scoped teams.
- [ ] The route requires/normalizes an authorized team selection.
- [ ] Nested `Dostupnost` and `Povrede` views exist.
- [ ] `Povrede` is not rendered or queried without medical-detail permission.
- [ ] Unauthorized injury URL state normalizes to safe availability.
- [ ] Team changes clear selected safe/restricted detail state.
- [ ] Availability summary uses the backend summary endpoint.
- [ ] Summary counts are not recalculated from the current table page.
- [ ] Summary contains no injury/diagnosis counts.
- [ ] Summary cards can apply safe status filters accessibly.
- [ ] Safe availability filters/page/detail state use `nuqs`.
- [ ] Safe table uses TanStack Table and shadcn primitives.
- [ ] Safe table renders backend synthetic `UNKNOWN` correctly.
- [ ] Safe table contains no injury existence, diagnosis, body area, or restricted notes.
- [ ] Coach-visible note is explicitly labeled as visible to the coaching/staff audience.
- [ ] Safe availability status never depends on an injury query.
- [ ] Availability edit form contains only safe Unit 50 fields.
- [ ] Expected-return field follows status/date rules.
- [ ] Coach-visible note warning is always visible.
- [ ] Injury details cannot be copied into the coach-visible note through a UI action.
- [ ] Availability mutation sends expected current revision.
- [ ] Availability revision conflict preserves user input and never auto-resubmits.
- [ ] Availability success toast contains no note contents.
- [ ] Availability mutation does not trigger an injury mutation.
- [ ] Availability history Sheet is URL-addressable.
- [ ] Availability revision history contains only safe operational fields.
- [ ] Synthetic unknown shows no fake history/audit.
- [ ] Availability audit reuses Unit 39 components.
- [ ] Availability audit contains no note contents or injury data.
- [ ] Restricted injuries view uses only Unit 50 restricted endpoints.
- [ ] Restricted list supports team/player/status/date filters and server pagination.
- [ ] No diagnosis/restricted-note free-text search exists.
- [ ] Injury compact table shows no restricted note snippet.
- [ ] Injury table does not infer/show safe availability status.
- [ ] Injury create uses the date-eligible candidate endpoint.
- [ ] Injury create has no availability/expected-return/coach-note fields.
- [ ] Injury creation does not automatically change safe availability.
- [ ] Injury detail uses an authorized URL-addressable Sheet.
- [ ] Injury detail shows current restricted revision only within the restricted surface.
- [ ] Injury update creates a new revision through the backend rather than editing locally.
- [ ] Injury update conflict never auto-merges or auto-submits notes.
- [ ] Injury resolve explains that availability is not automatically changed.
- [ ] Resolved records expose no reopen action.
- [ ] Injury revision history is immutable, paginated, and restricted.
- [ ] Injury audit shows changed field names but no old/new restricted values.
- [ ] No global medical audit page exists.
- [ ] Restricted DTOs are not imported into safe availability card/table modules.
- [ ] Safe TypeScript types do not define optional diagnosis/body-area/restricted-note fields.
- [ ] Restricted queries are never persisted to browser storage.
- [ ] Restricted query keys contain no medical text.
- [ ] Restricted details are not prefetched on hover.
- [ ] Restricted queries use zero/shortest approved unmounted cache lifetime.
- [ ] Permission or scope loss cancels and removes restricted queries immediately.
- [ ] Permission loss closes restricted Sheet/dialogs and clears URL state.
- [ ] Cached restricted data is not kept visible during authorization refetch.
- [ ] Safe availability remains usable after restricted cache cleanup.
- [ ] No medical response/request body is logged to the browser console.
- [ ] No medical text appears in toast, URL, analytics, clipboard, or export features.
- [ ] Player detail contains a real safe `Dostupnost` section.
- [ ] Player safe section respects multi-team scope and does not merge team statuses.
- [ ] Player safe section contains no injury indicator/details.
- [ ] Restricted player injury shortcut exists only with medical-detail permission.
- [ ] Player detail does not prefetch restricted injuries.
- [ ] Player-context injury create keeps player identity explicit and validates team/date.
- [ ] Admin can use safe and restricted mutation flows.
- [ ] In-scope medical staff without detail permission can update safe availability but sees no injuries.
- [ ] In-scope medical staff with detail permission can use injury mutations.
- [ ] Other detail-enabled roles can read injuries but cannot mutate them.
- [ ] Other roles see only safe availability.
- [ ] Backend `401`, `403`, `404`, `409`, `422`, and network errors are handled safely.
- [ ] Safe and restricted server state is not stored in Zustand or React Context.
- [ ] Query invalidation remains focused.
- [ ] No optimistic fake revision, injury status, or availability status is created.
- [ ] New UI follows shadcn-first guidance.
- [ ] Generated `frontend/src/components/ui/*` files are not manually modified.
- [ ] No medical UI, editor, search, data-grid, timeline, cache-persistence, analytics, or new state package is added.
- [ ] No raw Tailwind palette classes or hardcoded component colors are introduced.
- [ ] No ad-hoc visual overrides are applied to shadcn components.
- [ ] Frontend imports use `@/`.
- [ ] Visible copy uses Bosnian Latin with proper characters.
- [ ] Tabs, cards, tables, forms, candidate selector, Sheet, confirmations, revisions, audit, and pagination are keyboard accessible.
- [ ] Desktop, tablet, and mobile layouts remain usable.
- [ ] No medical documents, treatment plans, medication, tests, wellness, automation, export, dashboard, hard delete, or reopen behavior is added.
- [ ] Backend injury-candidate tests pass.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for affected backend projects/solution.
- [ ] Relevant backend tests pass.
- [ ] `npm run format` completes successfully.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] Frontend typecheck/build passes using the configured project command.
- [ ] Existing approved frontend tests pass when present.
- [ ] `context/ui-context.md` reflects the safe/restricted medical UI boundary.
- [ ] `context/progress-tracker.md` records actual Unit 51 route, privacy/cache behavior, workflows, and verification results.
