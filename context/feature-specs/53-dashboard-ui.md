# Unit 53: Dashboard UI

## Goal

Replace the `/` dashboard placeholder with a complete, URL-driven, team- and season-scoped operational overview using the bounded Unit 52 dashboard response.

Add safe context selection, recent matches and five-match form, role-aware report workflow cards, a clearly current coach-safe availability snapshot, confirmed statistics leader groups, exact-comparability physical workload summaries, role-aware data-quality alerts, truthful section-specific empty states, and token-based shadcn/Recharts visualizations.

Use the Unit 52 composite response as the source of truth. Do not orchestrate dashboard cards through many feature endpoints, reconstruct hidden counts, aggregate statistics or workload values in the browser, combine incompatible comparability contexts, expose restricted medical data, or add backend/API/schema changes.

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
10. `context/feature-specs/19-frontend-app-shell-navigation.md` or the actual implemented shell/navigation spec
11. `context/feature-specs/29-players-ui-foundation.md`
12. `context/feature-specs/34-matches-ui-foundation.md`
13. `context/feature-specs/37-match-report-review-ui.md`
14. `context/feature-specs/44-import-ui-foundation.md`
15. `context/feature-specs/49-training-gps-ui-foundation.md`
16. `context/feature-specs/51-medical-availability-ui.md`
17. `context/feature-specs/52-dashboard-backend-read-models.md`
18. `context/feature-specs/53-dashboard-ui.md`

Use relevant project-local skills from `.agents/skills/` when applicable.

Before creating custom UI primitives or interaction patterns:

- check `context/references/shadcn-components.md`;
- prefer suitable shadcn/ui components;
- install required shadcn components just in time through the approved workflow;
- compose application-level dashboard components outside `frontend/src/components/ui`;
- do not manually modify generated shadcn primitive files;
- do not hand-build equivalents of existing shadcn primitives without a documented reason.

This unit is frontend-only.

Do not change:

- Unit 52 endpoint contracts;
- report visibility rules;
- report statuses;
- availability population or privacy behavior;
- statistics leader catalogue;
- physical metric catalogue;
- workload comparability rules;
- import permissions;
- team-scope rules;
- authentication/session behavior;
- database schema;
- EF Core migrations.

### Scope

This unit introduces:

- a real dashboard on `/`;
- a safe dashboard context-options query;
- URL-driven team and season selection;
- deterministic context normalization without claiming a business “current season”;
- one Unit 52 overview query per selected context;
- context and data-generation timestamps;
- recent played-match list;
- five-match result strip;
- five-match goals-for/goals-against chart;
- role-aware report workflow cards;
- current coach-safe availability cards;
- confirmed statistics leader selector;
- top-three leader chart and accessible table;
- exact-comparability physical workload selector and summary;
- role-aware quality-alert list;
- safe destination links;
- archived/inactive context indicators;
- section-level loading, empty, truncation, and compatibility states;
- full-page overview error/retry state;
- responsive desktop/tablet/mobile layout;
- accessible chart/table equivalents;
- focused query behavior and invalidation integration.

This unit does not introduce:

- dashboard mutation endpoints;
- new backend read endpoints;
- a dashboard-specific database table;
- materialized dashboard snapshots;
- background refresh;
- websocket/live updates;
- saved dashboard layouts;
- user-configurable widgets;
- drag-and-drop dashboard cards;
- global season/team persistence;
- a server-defined current/default season;
- league tables;
- standings;
- xG;
- composite player rating;
- “best player” labels;
- per-90 or percentage metrics;
- hidden report/import counts;
- injury counts or restricted medical fields;
- historical availability reconstruction;
- workload averages, percentiles, rankings, trends, ACWR, or injury interpretation;
- cross-comparability workload charts;
- source import rows or preview data;
- CSV/PDF dashboard export;
- a new charting library;
- frontend-side cross-section aggregation.

### Route and navigation

Use the existing dashboard route:

```txt
/
```

Keep the existing visible navigation label:

```txt
Kontrolna ploča
```

The route remains protected through the existing authenticated app shell.

Navigation visibility:

- all authenticated active staff;
- team scope and section disclosure are applied by Unit 52;
- no mutation permission is required.

Do not add:

```txt
/dashboard
/overview
/home
```

as duplicate routes.

If a previous placeholder component exists for `/`, replace it rather than layering another route around it.

### Required UI-context synchronization

Update `context/ui-context.md`.

Replace the dashboard placeholder description with the real pattern:

```txt
Kontrolna ploča
- Selekcija i sezona
- Upozorenja kvaliteta podataka
- Posljednje utakmice i forma
- Status izvještaja
- Trenutna dostupnost igrača
- Potvrđeni statistički lideri
- Potvrđeni fizički workload
```

Document:

- dashboard context is URL-driven;
- availability is explicitly a current-team snapshot even for historical seasons;
- statistics leaders come only from current `VERIFIED`/`ARCHIVED` report data;
- workload groups are shown only within exact comparability contexts;
- charts use `--chart-1` through `--chart-5`;
- dashboard never exposes restricted injury details;
- report/import alert visibility follows backend response disclosure.

Do not hardcode implementation-specific component paths in UI context.

### API clients and query contracts

Add typed frontend clients for:

```txt
GET /api/dashboard/context-options
GET /api/dashboard/overview?teamId={teamId}&seasonId={seasonId}
```

Keep contracts in:

```txt
frontend/src/features/dashboard/
```

Suggested structure:

```txt
frontend/src/features/dashboard/
├── api/
├── components/
├── hooks/
├── types/
├── utils/
└── index.ts
```

Use explicit TypeScript types for:

```txt
DashboardContextOptions
DashboardOverview
DashboardRecentMatch
DashboardTeamForm
DashboardReportWorkflow
DashboardAvailabilitySummary
DashboardStatisticsLeaderGroup
DashboardWorkloadGroup
DashboardQualityAlert
```

Do not use:

```txt
any
object
Record<string, unknown>
```

for core section contracts.

Safe bounded metadata structures may use reviewed explicit types.

### Query keys

Use TanStack Query.

Define stable keys equivalent to:

```txt
dashboardContextOptions
dashboardOverview(teamId, seasonId)
```

Rules:

- overview key includes both IDs;
- no role/permission values in the query key;
- auth/session changes already invalidate/refetch through existing session behavior;
- do not store dashboard server data in Zustand or React Context;
- do not clear the full TanStack Query cache from the dashboard;
- do not call matches/reports/availability/import/workload endpoints separately to assemble cards.

### Context-options loading

Load:

```txt
GET /api/dashboard/context-options
```

before enabling the overview query.

Context options provide:

- authorized non-archived active/inactive teams;
- all safe seasons, including archived seasons.

Rules:

- no admin settings endpoint is used;
- no settings mutation action is inferred;
- context options are expected-small and may use a reasonable stale time;
- context options remain available while the selected overview is refreshed;
- context-options failure prevents overview context resolution and shows a page-level retry state;
- an empty team list and an empty season list are truthful non-error states.

### URL-driven dashboard context

Use `nuqs`.

Required query parameters:

```txt
teamId
seasonId
```

Rules:

- IDs use the repository's established parser/serializer;
- invalid or inaccessible values normalize safely;
- selected context is shareable through the URL;
- changing either value updates the URL and requests a new overview;
- no dashboard context is stored in localStorage/sessionStorage;
- no medical/restricted data is placed in the URL;
- no hidden role state is encoded in search params.

### Deterministic initial context selection

Unit 52 intentionally provides no `isCurrent` or `isDefault`.

When URL context is absent or stale, use the following UI convenience rule:

#### Team

1. first accessible `ACTIVE` team in Unit 52 display order;
2. otherwise the first returned accessible team.

#### Season

1. the first season returned by Unit 52's deterministic newest-start-date ordering.

Rules:

- write the normalized IDs into the URL using replace semantics;
- do not add a browser history entry solely for normalization;
- do not label the chosen season as `Trenutna sezona`;
- do not persist the choice outside the URL;
- do not infer current season from today's date;
- do not silently substitute context after a user has selected a still-valid value.

Show the selected season's explicit date range so the user understands the context.

### Empty context states

#### No accessible teams

Show:

```txt
Nemate dostupnu selekciju za prikaz kontrolne ploče.
```

Do not show another team's data.

Do not show a team-management action unless the current role already has a supported settings route/action and the app's existing empty-state convention permits it.

#### No seasons

Show:

```txt
Još nema definisanih sezona.
```

An admin may receive a link to the existing season settings page when that route/action is already available.

Ordinary roles receive a neutral message.

Do not call the overview endpoint without both valid IDs.

### Context selector UI

Place one prominent context bar below the page header.

Use shadcn:

- `Card` or `Field` composition;
- `Select` or searchable `Combobox` according to existing option volume;
- `Badge`;
- `Tooltip`.

Fields:

```txt
Selekcija
Sezona
```

Show:

- team name;
- team status:
  - `Aktivna`;
  - `Neaktivna`;
- season name;
- season date range;
- archived season badge:
  - `Arhivirana`.

Rules:

- do not hide inactive teams returned for historical review;
- archived team is never returned by Unit 52 and must not be offered;
- selector changes are keyboard accessible;
- changing context immediately replaces overview content with a matching loading state;
- do not keep previous-context cards visible without a clear stale overlay;
- no global app-shell team/season store is introduced;
- the top bar may display selected labels only if the existing shell supports route-provided context without adding duplicate state.

### Overview request behavior

Enable:

```txt
GET /api/dashboard/overview
```

only when:

- context options succeeded;
- selected team ID is valid;
- selected season ID is valid.

Rules:

- do not send requests with placeholder IDs;
- no automatic repeated polling;
- refetch on window focus may follow existing TanStack Query defaults;
- provide a manual refresh button;
- manual refresh refetches only the selected dashboard overview;
- show the backend `generatedAtUtc` after success;
- do not use the browser time as the data-generation timestamp.

Suggested action:

```txt
Osvježi
```

### Dashboard page header

Use the existing `PageHeader`.

Suggested title:

```txt
Kontrolna ploča
```

Suggested description:

```txt
Pregled utakmica, izvještaja, dostupnosti, potvrđenih statistika i fizičkog opterećenja za odabranu selekciju i sezonu.
```

Secondary metadata:

```txt
Ažurirano: {generatedAtUtc}
```

Header action:

```txt
Osvježi
```

Use a loading indicator while refreshing.

Do not display backend query timing or cache details.

### Dashboard layout

Desktop layout should use a bounded 12-column application grid.

Recommended order:

```txt
Page header
Context selector
Quality alerts
Recent matches and form
Report workflow
Current availability
Statistics leaders
Physical workload
```

Suggested desktop composition:

```txt
Quality alerts                    12 columns

Recent matches                    7 columns
Five-match form                   5 columns

Report workflow                   6 columns
Current availability              6 columns

Statistics leaders                6 or 12 columns
Physical workload                 6 or 12 columns
```

Adapt based on actual content density.

Rules:

- sections use application-level `Card` compositions;
- no masonry layout;
- stable reading order remains meaningful on mobile;
- cards do not use fixed heights that cut off truthful empty/truncation content;
- no decorative hero banner;
- no background imagery;
- no raw Tailwind palette colors;
- use semantic surface/state/chart tokens.

### Full-page loading

When valid context is selected and no overview is cached for that exact context:

- show dashboard section Skeletons;
- preserve the context selector;
- preserve page title;
- do not show fake counts or chart axes with sample values;
- use approximate card/table shapes matching the real layout.

Do not reuse prior-context values as if they belonged to the new selection.

### Full-page error

If the composite overview query fails unexpectedly:

- keep context selectors available;
- show one clear `Alert`;
- provide retry;
- do not independently fetch sections as a fallback;
- do not render a partially fabricated dashboard;
- use existing ProblemDetails mapping.

Suggested copy:

```txt
Kontrolna ploča nije mogla biti učitana za odabranu selekciju i sezonu.
```

On `403`:

- refetch session/context options;
- normalize stale team state;
- do not reveal whether an out-of-scope team exists.

On `404`:

- refetch context options;
- normalize stale team/season selection;
- show safe feedback.

### Quality alerts section

Render Unit 52:

```txt
qualityAlerts
```

near the top of the dashboard.

Suggested title:

```txt
Kvalitet podataka
```

Each alert contains:

```txt
code
severity
count
destination
isSeasonScoped
```

Use one centralized presentation registry.

### Quality-alert labels and descriptions

Map stable codes:

#### `PLAYED_MATCH_WITHOUT_REPORT`

Title:

```txt
Odigrane utakmice bez izvještaja
```

Description:

```txt
Odigrane utakmice u odabranoj sezoni još nemaju kreiran izvještaj.
```

#### `REPORT_DRAFT`

Title:

```txt
Izvještaji u nacrtu
```

Description:

```txt
Izvještaji su započeti, ali još nisu poslani na pregled.
```

#### `REPORT_READY_FOR_REVIEW`

Title:

```txt
Izvještaji spremni za pregled
```

Description:

```txt
Izvještaji čekaju pregled ovlaštenog korisnika.
```

#### `REPORT_NEEDS_CORRECTION`

Title:

```txt
Izvještaji zahtijevaju ispravku
```

Description:

```txt
Izvještaji su vraćeni na doradu prije verifikacije.
```

#### `IMPORT_FAILED`

Title:

```txt
Neuspjeli importi
```

Description:

```txt
Import jobovi za odabrani kontekst završili su tehničkom greškom.
```

#### `IMPORT_VALIDATION_FAILED`

Title:

```txt
Importi sa greškama validacije
```

Description:

```txt
Importi su obrađeni, ali sadrže blokirajuće greške validacije.
```

#### `AVAILABILITY_UNKNOWN`

Title:

```txt
Nepoznata dostupnost igrača
```

Description:

```txt
Za dio trenutno raspoređenih igrača nije evidentirano potvrđeno stanje dostupnosti.
```

#### `WORKLOAD_COMPARABILITY_SPLIT`

Title:

```txt
Više konteksta fizičkih metrika
```

Description:

```txt
Ista metrika postoji sa različitim pragovima ili metodologijama i prikazuje se odvojeno.
```

Rules:

- use proper Bosnian Latin;
- `count` is shown explicitly;
- do not invent player/match/import identities;
- unknown future alert code uses a safe technical fallback and never crashes;
- do not add text for alerts absent from the response;
- do not show hidden alerts as zero-count rows;
- no restricted medical alert exists.

### Alert severity presentation

Map:

```txt
ERROR
WARNING
INFO
```

to existing semantic tokens and shadcn `Alert`/`Badge` variants.

Rules:

- use readable severity labels:
  - `Greška`;
  - `Upozorenje`;
  - `Informacija`;
- do not rely only on color;
- no hardcoded palette classes;
- preserve backend ordering;
- do not reorder by client-defined business importance.

### Alert destination mapping

Create one centralized safe destination mapper.

Use only routes and filter parameters already supported by target modules.

Recommended behavior:

#### `MATCH_REPORTS`

Navigate to:

```txt
/match-reports
```

Include supported `teamId` and season/date filters only when those parameters already exist.

Do not create unsupported `alertCode` query params.

#### `IMPORTS`

Navigate to:

```txt
/imports
```

Include selected team and the corresponding status when safely supported:

```txt
IMPORT_FAILED -> FAILED
IMPORT_VALIDATION_FAILED -> VALIDATION_FAILED
```

Do not pretend that this destination reproduces the exact Unit 52 season classification when the imports page cannot express target-season semantics.

#### `AVAILABILITY`

Navigate to:

```txt
/availability
```

with:

```txt
teamId
availabilityStatus=UNKNOWN
```

#### `PHYSICAL_WORKLOADS`

Prefer:

- focus/scroll to the dashboard physical workload section; or
- navigate to `/training-sessions` with supported team filter.

Do not invent a universal physical-workload route.

Rules:

- destination links are navigation aids, not guaranteed exact drilldowns;
- use explanatory accessible labels;
- unknown destination disables navigation but preserves alert text;
- no inaccessible destination is inferred from a hidden alert.

### No-alert state

When `qualityAlerts` is empty:

Show:

```txt
Nema aktivnih upozorenja kvaliteta podataka za podatke koje možete vidjeti.
```

This wording acknowledges role-aware disclosure without revealing hidden alert categories.

Do not claim that all underlying system data is globally complete.

### Recent matches section

Render Unit 52:

```txt
recentMatches
```

Suggested title:

```txt
Posljednje utakmice
```

Each item shows:

- date/time;
- competition;
- opponent;
- location:
  - domaćin;
  - gost;
  - neutralni teren;
- FK Velež-perspective score;
- result badge:
  - pobjeda;
  - remi;
  - poraz;
- report status only when present in the response;
- open match action.

Localized result labels:

```txt
WIN -> Pobjeda
DRAW -> Remi
LOSS -> Poraz
```

Rules:

- do not infer home/away score order;
- render `teamScore : opponentScore`;
- report badge is omitted when the property/status is absent;
- do not show `Nepoznat izvještaj` as that could reveal hidden workflow state;
- row/card links to the existing match detail route;
- no lineup/statistics/media/workload detail is fetched here.

### Recent matches table/list

Use either:

- compact semantic `Table`; or
- shadcn `Item`/`Card` list.

Prefer a list/card composition on mobile and a compact table on desktop only if the existing responsive table pattern supports it without duplicate markup.

Required empty state:

```txt
Nema odigranih utakmica za odabranu selekciju i sezonu.
```

Do not show future scheduled matches in this section.

### Team form section

Render Unit 52:

```txt
teamForm
```

Suggested title:

```txt
Forma u posljednjih pet utakmica
```

Show:

- considered match count;
- wins;
- draws;
- losses;
- goals for;
- goals against;
- newest-to-oldest result strip.

Suggested cards:

```txt
Pobjede
Remiji
Porazi
Golovi za
Golovi protiv
```

Use JetBrains Mono for numeric values when consistent with existing stat-card patterns.

Rules:

- counts come directly from the backend;
- do not recalculate W/D/L from recent match rows for primary values;
- result strip uses `teamForm.form`;
- no league points;
- no unbeaten streak label;
- no strength-of-opponent interpretation;
- no sample form when count is zero.

### Five-match goals chart

Use existing shadcn Chart/Recharts infrastructure.

Chart type:

```txt
Grouped Bar Chart
```

Source:

```txt
recentMatches
```

Series:

```txt
Golovi FK Velež
Golovi protivnika
```

X-axis:

- compact opponent label;
- accessible full opponent/date in tooltip.

Y-axis:

- non-negative integer goals.

Rules:

- chart uses exact match scores only;
- no client-side season aggregation;
- preserve newest-to-oldest or oldest-to-newest order consistently and state it;
- preferred chart chronology is oldest to newest for scanning;
- result list remains newest to oldest as returned;
- reversing the bounded array for chart presentation is allowed and is not business aggregation;
- use `--chart-1` and `--chart-2`;
- no hardcoded color values;
- no smooth trend line implying continuous performance;
- tooltip includes:
  - date;
  - opponent;
  - location;
  - score;
  - result;
- chart is shown only when at least two matches exist;
- with one match, show stat cards/list only;
- provide an accessible hidden/visible data table equivalent.

### Report workflow section

Render Unit 52:

```txt
reportWorkflow
```

Suggested title depends on mode.

#### `FULL_WORKFLOW`

Title:

```txt
Tok izvještaja utakmica
```

Show:

- played match count;
- missing report count;
- returned status counts.

Localized status labels:

```txt
DRAFT -> Nacrt
READY_FOR_REVIEW -> Spreman za pregled
NEEDS_CORRECTION -> Potrebna ispravka
VERIFIED -> Verifikovan
ARCHIVED -> Arhiviran
```

Use cards or a compact status list.

Provide an action:

```txt
Otvori pregled izvještaja
```

to the existing `/match-reports` route.

#### `FINAL_ONLY`

Title:

```txt
Završeni izvještaji utakmica
```

Show only returned:

```txt
VERIFIED
ARCHIVED
```

Rules:

- do not render empty cards for hidden statuses;
- do not calculate or display played match count/missing report count;
- do not show a message that hidden workflow states exist;
- do not offer report-review mutation language;
- a link to accessible final reports/matches may be shown using supported routes.

### Report workflow visualization

Prefer status cards or a horizontal accessible list.

Do not use a donut/pie chart when status categories differ by role and may be intentionally omitted.

Do not calculate percentages because the denominator differs by disclosure mode.

### Report empty states

#### Full workflow with zero played matches

Show:

```txt
Nema odigranih utakmica za praćenje toka izvještaja u odabranoj sezoni.
```

#### Full workflow with played matches but no reports

Show:

- missing report count card;
- no fabricated status rows.

#### Final-only with no returned final reports

Show:

```txt
Nema verificiranih ili arhiviranih izvještaja za odabranu selekciju i sezonu.
```

Do not imply whether draft/correction reports exist.

### Current availability section

Render Unit 52:

```txt
availability
```

Suggested title:

```txt
Trenutna dostupnost igrača
```

Show:

- `asOfDate`;
- total;
- available;
- limited;
- unavailable;
- rehab;
- unknown.

Localized labels reuse Unit 51.

Rules:

- exact backend counts;
- no player rows;
- no coach-visible notes;
- no injury count/existence;
- no diagnosis/body area/restricted notes;
- no `canViewMedicalDetails` behavior;
- no historical reconstruction from selected season;
- no recalculation through Unit 51 list endpoint.

### Availability snapshot warning

Always show context text:

```txt
Presjek dostupnosti na {asOfDate}. Ovi podaci predstavljaju trenutno stanje selekcije i nisu historijski presjek odabrane sezone.
```

This message is especially important for archived or historical seasons.

Do not hide it when the selected season includes today's date.

### Availability cards

Use compact stat cards.

Rules:

- `Ukupno` clears any destination filter;
- each status card may link to `/availability` with supported team/status params;
- cards use semantic state tokens;
- unknown is visibly neutral/warning according to existing Unit 51 patterns;
- do not add a medical chart;
- zero counts remain visible;
- count sum is not recalculated client-side as an authorization check.

### Statistics leaders section

Render Unit 52:

```txt
statisticsLeaders
```

Suggested title:

```txt
Potvrđeni statistički lideri
```

Suggested description:

```txt
Zbirni rezultati koriste samo trenutne verificirane ili arhivirane izvještaje.
```

Use URL state:

```txt
leaderMetric
```

Rules:

- options come only from returned leader groups;
- invalid value normalizes to the first returned metric group;
- normalization uses replace semantics;
- if no groups exist, clear `leaderMetric`;
- do not persist the selection outside the URL;
- selection does not trigger a new backend query because the complete bounded groups are already returned;
- selecting among bounded returned groups is presentation filtering, not aggregation.

### Leader metric labels

Centralize:

```txt
GOALS -> Golovi
ASSISTS -> Asistencije
SHOTS_ON_TARGET -> Šutevi u okvir
KEY_PASSES -> Ključna dodavanja
DUELS_WON -> Dobijeni dueli
BALL_RECOVERIES -> Osvojene lopte
SAVES -> Odbrane
CLEAN_SHEETS -> Utakmice bez primljenog gola
```

Value kind is integer.

Do not add:

- cards/fouls/possession-loss leaderboards;
- percentages;
- per-match/per-90;
- overall rating.

### Leader presentation

For the selected metric group, show:

- eligible report count;
- top-three rows;
- rank;
- player;
- value;
- appearance count;
- match count;
- additional-tie message.

Player links open existing player detail.

Suggested context labels:

```txt
Nastupi
Utakmice
Potvrđeni izvještaji
```

Rules:

- use returned rank;
- do not recalculate tie ranks;
- do not label rank 1 as `Najbolji igrač`;
- preserve transferred/archived player summaries;
- `hasAdditionalTies` displays:
  - `Postoje dodatni igrači sa istom vrijednošću izvan prikazanog top-3.`;
- no fourth row is fetched separately.

### Leader chart

Use shadcn Chart/Recharts horizontal bar chart.

Data:

```txt
selected leader group's returned leaders
```

Rules:

- all bars are one exact metric;
- chart uses returned integer values;
- no normalization;
- no combining metric groups;
- use theme chart tokens;
- player names are accessible;
- tooltip includes:
  - value;
  - appearances;
  - matches;
  - rank;
- chart and semantic table are both available;
- chart is supplementary, not the only presentation;
- if only one leader exists, the table/card may be shown without a decorative chart;
- zero-valued groups should never be returned by Unit 52.

### Statistics leader empty reasons

Map:

#### `NO_FINAL_REPORTS`

```txt
Nema verificiranih ili arhiviranih izvještaja iz kojih se mogu izračunati statistički lideri.
```

#### `NO_POSITIVE_LEADER_VALUES`

```txt
Potvrđeni izvještaji postoje, ali nema pozitivnih vrijednosti u podržanim leaderboard metrikama.
```

#### `NO_ENABLED_LEADER_METRICS`

```txt
Za primijenjene tracking nivoe nema dostupnih podržanih leaderboard metrika.
```

Unknown future reason:

```txt
Nema dostupnih potvrđenih statističkih lidera za odabrani kontekst.
```

Do not fabricate zero-valued players.

### Physical workload section

Render Unit 52:

```txt
physicalWorkload
```

Suggested title:

```txt
Potvrđeno fizičko opterećenje
```

Suggested description:

```txt
Prikazane su samo službene trenutne workload revizije, odvojene po tipu konteksta, pragu i metodologiji.
```

Do not use import preview/source files as official workload data.

### Workload URL state

Use:

```txt
workloadContext
workloadMetric
workloadComparison
```

Stable context values:

```txt
TRAINING
MATCH
```

Rules:

1. derive available context types from returned groups;
2. derive available metric codes for selected context;
3. derive exact comparability groups for selected context/metric;
4. normalize invalid selections using replace semantics;
5. clear values when no groups exist.

This is selection over a bounded authoritative response.

Do not:

- merge groups;
- sum groups;
- average groups;
- generate a new comparability key;
- normalize threshold precision;
- ignore method version.

### Workload metric labels and units

Reuse the Unit 49 shared physical-workload presentation registry.

Do not create a second independent metric/unit registry in dashboard code.

Use existing labels and formatters for:

```txt
TOTAL_DISTANCE_METERS
HIGH_SPEED_RUNNING_DISTANCE_METERS
SPRINT_DISTANCE_METERS
SPRINT_COUNT
MAX_SPEED_METERS_PER_SECOND
ACCELERATION_COUNT
DECELERATION_COUNT
PLAYER_LOAD_ARBITRARY_UNITS
```

`SESSION_DURATION_SECONDS` should not appear because Unit 52 excludes `LATEST`.

Unknown future metric codes:

- remain visible through safe fallback;
- are not charted/ranked automatically.

### Workload group selector

Use:

- context selector:
  - `Trening`;
  - `Utakmica`;
- metric selector;
- exact comparability selector when more than one group exists.

Comparability label must include:

- threshold value/unit/direction/scope where present;
- method key/version where present;
- canonical unit;
- aggregation kind.

Do not display only an opaque comparability key.

The technical key may appear in a tooltip or expandable details.

### Workload group card

For one selected exact group, show:

```txt
aggregate value
aggregation kind
workload count
player count
context count
first occurred date
last occurred date
threshold context
method context
canonical unit
```

Localized aggregation labels:

```txt
SUM -> Zbir
MAX -> Maksimum
```

Context-count label depends on type:

```txt
TRAINING -> Treninzi
MATCH -> Utakmice
```

Rules:

- aggregate value comes directly from Unit 52;
- no client-side recalculation;
- preserve exact decimal formatting through shared Unit 49 helpers;
- preserve zero;
- absence is not zero;
- do not divide by player/workload/context count;
- do not call the value an average;
- no ranking;
- no “high/low” performance interpretation;
- no injury/medical interpretation.

### Workload list/table

Alongside the selected card, render a compact table/list of all returned exact groups.

Recommended columns:

- context;
- metric;
- threshold/method summary;
- aggregation;
- value;
- workload count;
- player count;
- context count;
- date range.

Rules:

- this table makes split contexts transparent;
- do not sort by aggregate value across incompatible metrics/contexts;
- default order follows backend order;
- allow filtering through the selectors but not arbitrary cross-metric value sorting;
- selecting a row updates the exact URL selection;
- no one-line combined chart across rows.

### Workload comparability split state

When:

```txt
hasMultipleComparabilityContexts = true
```

show an informational Alert:

```txt
Neke fizičke metrike postoje u više pragova ili metodologija. Grupe su namjerno odvojene i ne sabiraju se međusobno.
```

Show:

```txt
splitMetricContextCount
```

Do not label this as corrupted data.

### Workload truncation state

When:

```txt
truncated = true
```

show:

```txt
Dashboard prikazuje {returnedGroupCount} od {availableGroupCount} kompatibilnih workload grupa. Dodatne grupe nisu spojene niti skrivene kao nula.
```

Provide a supported navigation action to:

```txt
/training-sessions
```

with team filter where possible.

Do not offer a fake `Učitaj još` action because Unit 52 endpoint is intentionally bounded and has no workload pagination contract.

### Workload empty reason

Map:

```txt
NO_CONFIRMED_WORKLOADS
```

to:

```txt
Nema potvrđenih službenih fizičkih workload vrijednosti za odabranu selekciju i sezonu.
```

Additional helper text:

```txt
Sačuvani import fajl ili generički preview ne predstavlja potvrđen workload.
```

Do not display generic import jobs as workload data.

### No cross-context workload chart

Do not render a chart that compares:

- training and match groups;
- different metric codes;
- different comparability keys;
- different thresholds;
- different method keys/versions.

Unit 52 returns one aggregate value per exact group, so a one-value decorative chart adds little analytical value.

Use:

- cards;
- selectors;
- table/list;
- context badges.

The dashboard already includes safe charts for recent goals and one leader metric.

### Shared section-header pattern

Use a reusable application-level dashboard section header with:

- title;
- description;
- optional metadata;
- optional action;
- no hidden permission indicators.

Do not create a generic widget framework.

### Shared empty-state pattern

Every section should use truthful feature-specific empty copy.

Rules:

- no generic `Nema podataka` when a more precise reason exists;
- no sample/demo values;
- no hidden placeholders;
- no future-feature buttons;
- no inference that inaccessible data exists;
- section empty state does not make the whole dashboard an error.

### Role-aware rendering

The dashboard component must render exactly what the Unit 52 response contains.

#### Report workflow

Branch only on:

```txt
visibilityMode
```

Do not inspect session role to reconstruct hidden statuses.

#### Import alerts

Render only returned alerts.

Do not check `canImportData` to fabricate missing counts.

#### Availability

Always safe summary only.

Do not check `canViewMedicalDetails`.

#### Statistics leaders

Render returned final-report aggregates to all scoped readers.

#### Workload

Render returned official groups to all scoped readers.

Role/session data may still control navigation to target modules according to existing routes, but not dashboard count reconstruction.

### Navigation and links

Use existing route helpers rather than hardcoded duplicated paths where they exist.

Expected links:

```txt
Recent match -> /matches/{matchId}
Report workflow -> /match-reports
Availability -> /availability
Statistics player -> /players/{playerId}
Workload -> /training-sessions
Imports alert -> /imports
```

Rules:

- include only supported query params;
- preserve selected team when target page supports it;
- preserve selected season only when target page supports it semantically;
- do not pass dashboard-only `leaderMetric`/`workloadComparison` to unrelated pages;
- all links are keyboard accessible;
- navigation labels explain destination.

### Chart implementation rules

Use existing shadcn Chart/Recharts.

Allowed dashboard charts:

```txt
Recent match goals grouped bar chart
Selected statistics leader horizontal bar chart
```

Rules:

- no new charting package;
- use `ChartContainer`, `ChartTooltip`, and established shadcn chart wrappers;
- use `--chart-1` through `--chart-5`;
- no raw hex/RGB/HSL/OKLCH values;
- no raw Tailwind palette classes;
- no gradients unless already approved by the design system;
- no 3D charts;
- no pie/donut for role-dependent omitted statuses;
- chart data is bounded;
- chart dimensions are responsive;
- charts have accessible titles/descriptions;
- an equivalent semantic table/list is present;
- tooltips contain no hidden/restricted data;
- animations respect reduced-motion preferences where supported.

### Number and date formatting

Reuse existing utilities.

Rules:

- Bosnian date formatting;
- 24-hour time;
- exact integers use locale formatting;
- decimal workload values use Unit 49 canonical formatters;
- do not use truthiness for zero;
- `null`/missing uses explicit unavailable copy;
- no percentage formatting is introduced;
- `generatedAtUtc` and `asOfDate` are distinct;
- season date range is visible.

### Localization preparation

Unit 54 will add localization infrastructure.

For Unit 53:

- centralize long-lived dashboard labels in feature-level registries/constants where practical;
- use proper Bosnian Latin;
- avoid scattering duplicate label mappings;
- keep internal codes English;
- do not introduce i18n packages early;
- document visible strings that Unit 54 must migrate.

### Responsive behavior

Desktop/tablet are primary.

Requirements:

- context selectors use one row on desktop and stack on mobile;
- quality alerts stack cleanly;
- recent matches and form cards reflow without horizontal page overflow;
- chart containers maintain readable minimum heights;
- report and availability cards wrap;
- leader chart/table use contained horizontal behavior;
- workload table uses contained `Scroll Area`/overflow;
- section actions remain reachable;
- mobile reading order follows the desktop semantic order;
- no fixed-width dashboard causing page-wide scrolling;
- no separate mobile dashboard product.

### Accessibility

Requirements:

- team/season selectors have visible labels;
- normalized/default selection is announced through normal form state, not surprise focus;
- manual refresh has accessible loading state;
- alert severities use text;
- summary cards used as links/filters are semantic buttons/links;
- result strip includes text/accessible labels;
- charts have accessible title/description and table equivalents;
- match/result/report/status badges contain readable text;
- all tables retain semantic markup;
- chart color is not the only distinction;
- workload threshold/method context is readable by assistive technology;
- truncation and empty states are announced as text;
- focus order follows section order;
- no chart captures keyboard focus unnecessarily;
- tooltips are supplementary;
- links have descriptive labels;
- loading skeletons do not create misleading announcements.

### Error handling

Use existing ProblemDetails and TanStack Query behavior.

#### Context options

- `401`: existing auth flow;
- `403`: session/scope refetch;
- unexpected failure: page-level retry;
- no options: truthful empty state.

#### Overview

- `401`: existing auth flow;
- `403`: refetch context options and normalize team;
- `404`: refetch options and normalize stale context;
- network/5xx: keep selectors and show overview retry;
- malformed unknown code: safe compatibility fallback for only the affected presentation registry.

Do not:

- fetch individual sections to hide a composite failure;
- show stale prior-team values without clear labeling;
- display raw backend exceptions;
- expose hidden report/import counts;
- log complete dashboard payloads to console.

### Query refresh and invalidation integration

Dashboard overview is a composite read.

After mutations in existing modules, focused feature invalidation may not automatically know every dashboard key.

Add a small shared helper equivalent to:

```txt
invalidateDashboardOverview(queryClient, teamId?)
```

Use it in relevant existing successful mutation flows where practical:

- match result/status changes;
- report create/status/statistics changes;
- import confirmation/cancellation/failure-retry outcomes that alter alert/workload state;
- availability revision;
- training/match workload revision confirmation;
- team/season settings changes affecting context options.

Rules:

- invalidation helper invalidates dashboard overview keys, optionally by team;
- context-options invalidation is separate and used only when team/season settings change;
- do not clear the full cache;
- do not add tight coupling from domain components to dashboard component internals;
- manual dashboard refresh remains available;
- no polling is required.

If retrofitting every existing mutation is too broad for Unit 53, prioritize the major successful mutation hooks and document any remaining manual-refresh dependency in `context/progress-tracker.md`.

### Frontend testing

Add frontend tests only if the repository already has an approved test foundation.

When present, prioritize:

- context normalization;
- invalid/stale URL handling;
- overview query enablement;
- context change loading behavior;
- report `FULL_WORKFLOW` versus `FINAL_ONLY` rendering;
- absent report status omission;
- current availability snapshot warning;
- leader metric selection;
- leader empty-reason mapping;
- use of returned ranks/ties;
- exact workload selection;
- no comparability-key merging;
- workload truncation copy;
- alert label/destination mapping;
- hidden/absent alert behavior;
- chart data mapping without business aggregation;
- zero handling;
- unknown future code fallback;
- dashboard invalidation helper.

Do not introduce a new testing framework solely for Unit 53.

### Manual verification

Verify at minimum:

- admin;
- full-workflow data operator;
- analyst;
- coach;
- medical staff;
- viewer;
- all-team scope;
- selected-team scope;
- no accessible teams;
- no seasons;
- inactive team;
- archived season;
- absent URL params;
- stale team/season params;
- context switching;
- manual refresh;
- overview error;
- no played matches;
- one recent match;
- five recent matches;
- report full workflow;
- report final-only disclosure;
- no final reports;
- all availability statuses;
- historical season current-availability warning;
- no statistics leaders for each empty reason;
- top-three leader tie state;
- one/multiple workload comparability contexts;
- truncated workload groups;
- no confirmed workloads;
- alert-free state;
- role-aware report/import alerts;
- navigation destinations;
- responsive layout;
- keyboard operation;
- chart table equivalents;
- reduced-motion behavior where supported.

### Documentation synchronization

Update:

```txt
context/ui-context.md
context/progress-tracker.md
```

Record:

- real dashboard route and layout;
- context normalization rule;
- URL parameters;
- selected/inactive/archived context behavior;
- section disclosure behavior;
- chart choices;
- availability current-snapshot wording;
- statistics final-report-only behavior;
- exact workload comparability handling;
- quality-alert mappings/destinations;
- dashboard invalidation integration;
- verification results;
- intentionally deferred configurable widgets, exports, historical availability, advanced analytics, and polling.

If implementation reveals a mismatch in the Unit 52 response contract, update the relevant spec/context before continuing.

Do not silently reconstruct missing or hidden data.

## Implementation

### 1. Add dashboard frontend contracts and API clients

Create typed clients for:

```txt
GET /api/dashboard/context-options
GET /api/dashboard/overview
```

Add stable query keys and hooks.

### 2. Replace the `/` placeholder

Render the real protected `Kontrolna ploča`.

Preserve existing app-shell route/navigation behavior.

### 3. Add URL-driven context resolution

Implement:

- context-options loading;
- deterministic team selection;
- deterministic season selection;
- stale URL normalization;
- no-team/no-season states;
- overview query enablement.

### 4. Build the context selector and page states

Implement:

- team selector;
- season selector;
- inactive/archived badges;
- date range;
- manual refresh;
- generated timestamp;
- skeleton/error states.

### 5. Build quality alerts

Implement centralized:

- code labels/descriptions;
- severity presentation;
- destination mapping;
- empty state.

Render only backend-returned alerts.

### 6. Build recent matches and form

Implement:

- recent match list;
- result badges;
- optional permitted report status;
- form stat cards;
- result strip;
- goals-for/goals-against grouped bar chart;
- empty states.

### 7. Build role-aware report workflow

Implement distinct:

```txt
FULL_WORKFLOW
FINAL_ONLY
```

presentations.

Do not render or infer hidden statuses.

### 8. Build current availability summary

Implement safe status cards, snapshot date/warning, and links to Unit 51.

Do not load injury data.

### 9. Build confirmed statistics leaders

Implement:

- metric selector;
- localized labels;
- horizontal bar chart;
- accessible table;
- tie behavior;
- player links;
- empty-reason mapping.

Do not calculate new performance metrics.

### 10. Build physical workload summary

Reuse Unit 49 metric/unit/context helpers.

Implement:

- context selector;
- metric selector;
- exact comparability selector;
- selected group card;
- exact-group table;
- split-context Alert;
- truncation state;
- no-confirmed-workload state.

Do not chart or merge incompatible groups.

### 11. Add dashboard invalidation helper

Create a focused query invalidation utility.

Wire major existing successful mutation hooks where practical.

Keep context-options invalidation separate.

### 12. Verify shadcn-first and token-based design

Review all UI against the component catalogue.

Confirm:

- semantic tokens only;
- chart tokens only;
- generated primitive files unmodified;
- no custom chart library;
- no ad-hoc component overrides.

### 13. Update documentation

Update `context/ui-context.md` and `context/progress-tracker.md`.

Do not mark Unit 53 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use existing frontend packages and infrastructure:

- TanStack Query;
- `nuqs`;
- shadcn/ui;
- existing Recharts-backed shadcn Chart;
- Lucide React;
- existing app shell/navigation;
- existing PageHeader, Empty, Alert, Card, Table, Badge, Select/Combobox, Skeleton, Tooltip, Scroll Area, and Button patterns;
- Unit 49 physical metric presentation/formatting helpers;
- existing date/number/ProblemDetails utilities.

Add missing shadcn primitives only through the approved CLI workflow and only when used.

Do not add:

- another charting library;
- dashboard/widget layout packages;
- drag-and-drop packages;
- data-grid packages;
- analytics libraries;
- state libraries;
- caching servers;
- date libraries;
- unit-conversion libraries;
- export/PDF packages;
- localization packages before Unit 54;
- a new frontend testing framework solely for this unit.

## Verification checklist

- [ ] `/` renders a real dashboard rather than a placeholder.
- [ ] Existing `Kontrolna ploča` navigation remains active for `/`.
- [ ] No duplicate `/dashboard` route is added.
- [ ] Dashboard uses `GET /api/dashboard/context-options`.
- [ ] Dashboard uses one `GET /api/dashboard/overview` request per selected context.
- [ ] Dashboard does not orchestrate cards through feature HTTP endpoints.
- [ ] Team and season are URL-driven with `nuqs`.
- [ ] Invalid/stale URL IDs normalize safely.
- [ ] Initial team prefers the first active accessible team, otherwise the first accessible team.
- [ ] Initial season uses Unit 52's first deterministic season option.
- [ ] The UI does not label the selected season as server-defined current/default.
- [ ] Context normalization uses URL replace semantics.
- [ ] Team/season selection is not stored outside the URL.
- [ ] No accessible-team and no-season empty states are implemented.
- [ ] Overview query is disabled until both valid IDs exist.
- [ ] Context change does not display previous-team values as current.
- [ ] Inactive team and archived season badges are shown.
- [ ] Selected season date range is visible.
- [ ] Manual refresh refetches only the current overview.
- [ ] Backend `generatedAtUtc` is displayed.
- [ ] Full dashboard loading uses skeletons without fake values.
- [ ] Composite query failure does not trigger per-section HTTP fallbacks.
- [ ] Quality alerts render only backend-returned entries.
- [ ] All stable alert codes have centralized Bosnian labels/descriptions.
- [ ] Alert severity uses semantic tokens and readable text.
- [ ] Zero-count/hidden alerts are not fabricated.
- [ ] Alert destination mapping uses only supported routes/params.
- [ ] Alert-free copy does not claim globally perfect data.
- [ ] Recent matches show at most the returned bounded set.
- [ ] Match score is rendered from FK Velež perspective.
- [ ] Result labels are localized.
- [ ] Missing report status is omitted rather than shown as unknown.
- [ ] Recent match links open existing match detail.
- [ ] Team form cards use backend counts.
- [ ] Result strip preserves backend newest-to-oldest meaning.
- [ ] Goals chart uses exact recent-match scores only.
- [ ] Goals chart uses shadcn/Recharts and chart tokens.
- [ ] Goals chart has a semantic table/list equivalent.
- [ ] Goals chart is omitted when it would be decorative/insufficient.
- [ ] `FULL_WORKFLOW` shows only its returned status/missing fields.
- [ ] `FINAL_ONLY` shows only verified/archived returned counts.
- [ ] Hidden report statuses are never rendered as zero/null placeholders.
- [ ] Report workflow does not calculate percentages from hidden denominators.
- [ ] Availability uses only Unit 52 safe summary.
- [ ] Availability displays `asOfDate`.
- [ ] Availability always explains that it is a current snapshot, not historical season state.
- [ ] Availability contains no player rows, notes, injury counts, diagnoses, body areas, or restricted details.
- [ ] Availability cards link only to supported safe Unit 51 filters.
- [ ] Statistics leaders use only returned groups.
- [ ] Leader metric selection is URL-driven.
- [ ] All stable leader metric labels are centralized.
- [ ] Leader chart compares only one selected metric group.
- [ ] Returned ranks/ties are not recalculated.
- [ ] `hasAdditionalTies` is explained.
- [ ] No “best player,” composite rating, percentage, per-match, or per-90 calculation is added.
- [ ] All three leader empty reasons have truthful copy.
- [ ] Workload presentation reuses Unit 49 metric/unit/context helpers.
- [ ] Workload context, metric, and comparability selection are URL-driven.
- [ ] Exact backend comparability key remains the grouping boundary.
- [ ] Dashboard never generates a weaker comparability key.
- [ ] Training and match workload groups remain separate.
- [ ] Different thresholds/method versions remain separate.
- [ ] Workload aggregate values are never recalculated, averaged, normalized, or ranked.
- [ ] Aggregation kind is clearly labeled as sum or maximum.
- [ ] Threshold and method context are visible.
- [ ] Workload group table follows backend ordering.
- [ ] No cross-context workload chart is added.
- [ ] Comparability split is shown as informational, not corrupted data.
- [ ] Workload truncation displays returned/available counts.
- [ ] No fake workload “load more” action exists.
- [ ] No-confirmed-workload state distinguishes imports/previews from official data.
- [ ] Unknown future alert/metric/reason codes use safe fallbacks without crashing.
- [ ] Role/session state is not used to reconstruct hidden report/import counts.
- [ ] Dashboard contains no restricted medical data or medical-detail permission state.
- [ ] Links use existing route helpers and supported parameters.
- [ ] Charts use only `--chart-1` through `--chart-5`.
- [ ] No hardcoded chart or component colors exist.
- [ ] Charts are supplementary to accessible tables/lists.
- [ ] Numbers preserve zero versus missing values.
- [ ] Dates/times use Bosnian/24-hour formatting.
- [ ] Dashboard server data is not stored in Zustand or React Context.
- [ ] Dashboard query invalidation is focused.
- [ ] Dashboard context-options invalidation is separate from overview invalidation.
- [ ] No polling, websocket, background refresh, or materialized frontend snapshot is added.
- [ ] New UI follows shadcn-first guidance.
- [ ] Generated `frontend/src/components/ui/*` files are not manually modified.
- [ ] No charting, dashboard-layout, drag/drop, grid, analytics, state, export, date, unit, or localization dependency is added.
- [ ] No raw Tailwind palette classes or arbitrary visual values are introduced.
- [ ] No ad-hoc shadcn component overrides are applied.
- [ ] Frontend imports use `@/`.
- [ ] Visible copy uses proper Bosnian Latin characters.
- [ ] Selectors, refresh, alerts, cards, links, tables, result strip, and charts are keyboard/screen-reader accessible.
- [ ] Desktop, tablet, and mobile layouts are usable without page-wide overflow.
- [ ] No backend files, endpoint contracts, domain models, or migrations are changed.
- [ ] `npm run format` completes successfully.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] Frontend typecheck/build passes using the configured project command.
- [ ] Existing approved frontend tests pass when present.
- [ ] `context/ui-context.md` reflects the real dashboard layout and disclosure rules.
- [ ] `context/progress-tracker.md` records actual Unit 53 implementation, invalidation coverage, and verification results.
