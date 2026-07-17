# Unit 52: Dashboard Backend Read Models

## Goal

Build one bounded, team-scope-aware dashboard backend snapshot for a selected team and season.

Compose recent played matches and team form, role-aware match-report workflow counts, current coach-safe availability counts, confirmed match-statistics leaders, exact-comparability physical workload summaries, and role-aware data-quality alerts through reusable Application queries.

Add a safe dashboard context-options query so non-admin staff can select authorized teams and seasons without weakening settings mutation authorization.

Do not expose restricted injury details, draft/correction report information to roles that cannot read those reports, import failures to users without import access, unverified statistics, incompatible workload aggregation, frontend UI, cached/materialized dashboard tables, background refresh jobs, or an EF Core migration.

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
8. `context/feature-specs/00-build-plan.md`
9. `context/feature-specs/21-seasons-competitions-backend.md`
10. `context/feature-specs/22-teams-selections-settings-backend.md`
11. `context/feature-specs/23-staff-users-team-scope-account-lifecycle-backend.md`
12. `context/feature-specs/30-matches-backend-foundation.md`
13. `context/feature-specs/32-match-report-workflow-backend.md`
14. `context/feature-specs/33-manual-match-statistics-backend.md`
15. `context/feature-specs/43-import-workflow-backend-foundation.md`
16. `context/feature-specs/48-training-gps-backend-foundation.md`
17. `context/feature-specs/50-medical-availability-backend.md`
18. `context/feature-specs/52-dashboard-backend-read-models.md`

Use relevant installed backend Codex skills/plugins when applicable.

Skills/plugins may guide implementation workflow but must not override project context, authorization, privacy, statistics completeness, workload comparability, or this spec.

This unit is backend-only.

Do not add or change:

- frontend routes or components;
- dashboard charts;
- match/report/statistics/workload/availability domain models;
- report visibility rules;
- medical disclosure rules;
- import permissions;
- canonical metric definitions;
- vendor processors;
- background workers;
- database tables or columns.

No EF Core migration is expected in Unit 52.

### Scope

This unit introduces:

- a feature-specific dashboard context-options endpoint;
- one composite dashboard overview endpoint;
- required team and season context validation;
- selected-team authorization;
- safe season summaries for non-admin dashboard use;
- recent played matches;
- a deterministic five-match form summary;
- role-aware match-report status counts;
- current coach-safe availability counts;
- confirmed statistics leader groups;
- comparison-safe physical workload groups;
- role-aware data-quality alerts;
- stable empty-reason and alert codes;
- bounded section limits;
- database-side aggregation/projection;
- focused query services reusable by later reporting work;
- performance, authorization, privacy, and integration tests.

This unit does not introduce:

- dashboard UI;
- dashboard mutation endpoints;
- user-configurable widgets;
- saved dashboard layouts;
- automatic current/default season persistence;
- standings or league-table calculations;
- expected goals;
- player rating formulas;
- composite “best player” scores;
- percentages requiring unconfirmed denominator rules;
- medical injury counts;
- diagnosis/body-area/note data;
- workload aggregation across different comparability keys;
- workload aggregation across training and match contexts;
- unverified/draft statistics in leaderboards;
- historical availability reconstruction;
- materialized views;
- a cache server;
- background snapshot generation;
- dashboard export;
- arbitrary client-defined sorting/metric selection;
- frontend dependencies.

### API surface

Add:

```txt
GET /api/dashboard/context-options
GET /api/dashboard/overview
```

`GET /api/dashboard/overview` requires:

```txt
teamId
seasonId
```

Do not add one endpoint per card/widget.

Do not make the frontend orchestrate many existing feature endpoints to build the dashboard.

The dashboard Application query may reuse internal query services and repositories, but it must not call the application's own HTTP endpoints.

### Dashboard context options

Add:

```txt
GET /api/dashboard/context-options
```

Purpose:

- provide safe team and season selection data to every authenticated dashboard reader;
- avoid requiring non-admin users to call Unit 21 admin settings endpoints;
- preserve Unit 21 mutation authorization;
- keep Unit 53 from hardcoding or inferring season/team values.

Authorization:

- authenticated active staff;
- required-password-change/session gates remain active;
- teams are restricted through current Unit 23 team scope;
- admin may see all eligible non-archived teams;
- other users see only current accessible non-archived teams.

Return:

```txt
teams:
  id
  name
  status
  displayOrder

seasons:
  id
  name
  startDate
  endDate
  isArchived
```

Season rules:

- return all seasons as safe operational reference data;
- include archived seasons because historical dashboards must remain selectable;
- expose only ID, display name, date range, and archive state;
- do not expose settings mutation actions or administrative metadata;
- deterministic order by start date descending, then ID;
- expected-small settings collection; no pagination required;
- no `isCurrent`, `isDefault`, or persisted recommendation is introduced.

Team rules:

- exclude archived teams from ordinary dashboard selection;
- include active and inactive accessible teams because historical data may still be reviewed;
- preserve configured display order;
- do not return teams outside current scope;
- do not return permission internals.

If no accessible teams exist, return an empty team list rather than a different user's data.

Add tests proving that this feature-specific read does not expose settings mutation capability.

### Dashboard overview context

Add:

```txt
GET /api/dashboard/overview?teamId={teamId}&seasonId={seasonId}
```

Validation:

- caller is authenticated and active;
- `teamId` is required;
- `seasonId` is required;
- team exists and is not archived;
- team is in current scope;
- season exists, including archived historical seasons;
- explicit out-of-scope team returns `403`;
- missing/safely inaccessible context follows established `404` behavior;
- no implicit team or season substitution;
- no automatic current-season lookup.

Return one response equivalent to:

```txt
context
recentMatches
teamForm
reportWorkflow
availability
statisticsLeaders
physicalWorkload
qualityAlerts
generatedAtUtc
```

Every section must be bounded and independently truthful.

Do not return placeholder/sample values.

### Shared dashboard context

Return:

```txt
context:
  team:
    id
    name
    status
  season:
    id
    name
    startDate
    endDate
    isArchived
  generatedAtUtc
```

Rules:

- use one captured UTC timestamp for the response;
- use the backend operational date derived from the approved clock/timezone convention where a date snapshot is needed;
- do not return caller role/permission flags as a substitute for section authorization;
- do not expose internal query timing, SQL, or cache keys.

### Recent matches

Return at most:

```txt
5
```

recent matches.

Population:

- selected team;
- selected season;
- match status `PLAYED`;
- match not archived;
- final score present through Unit 30 invariants.

Ordering:

- kickoff descending;
- match ID descending.

Each item contains at minimum:

```txt
id
kickoffAtUtc
competition summary
opponent summary
locationType
teamScore
opponentScore
result
reportStatus when caller may read that exact report state
```

Stable result values:

```txt
WIN
DRAW
LOSS
```

Rules:

- result derives from FK Velež perspective using `TeamScore` and `OpponentScore`;
- do not infer home/away score fields;
- report status is omitted when the caller cannot read that report status under Unit 32;
- verified/archived report status may be returned to final-only readers;
- hidden draft/review/correction status must not be returned as `null`, `UNKNOWN`, or another inferable placeholder indicating a hidden report exists;
- do not include lineup, statistics rows, media, workload values, or medical data.

### Team form summary

Derive form from the same recent played-match population, capped at the latest five matches.

Return:

```txt
consideredMatchCount
wins
draws
losses
goalsFor
goalsAgainst
form:
  matchId
  result
```

Rules:

- `form` order is newest to oldest;
- wins/draws/losses sum to considered count;
- score corrections are reflected live;
- no league points, table position, expected goals, strength-of-opponent weighting, or unbeaten-streak claim;
- empty season data returns zero counts and an empty form array;
- no synthetic fixtures.

### Report workflow visibility modes

Dashboard report data must reuse Unit 32 visibility rules.

Stable visibility modes:

```txt
FULL_WORKFLOW
FINAL_ONLY
```

#### `FULL_WORKFLOW`

Used for roles that Unit 32 permits to read non-final report states for the selected team, including the established admin, data-entry, and analyst cases.

May return counts for:

```txt
DRAFT
READY_FOR_REVIEW
NEEDS_CORRECTION
VERIFIED
ARCHIVED
```

May also return:

```txt
playedMatchCount
missingReportCount
```

#### `FINAL_ONLY`

Used for roles limited by Unit 32 to final reports, including ordinary coach/medical/viewer behavior.

May return only counts for:

```txt
VERIFIED
ARCHIVED
```

Rules:

- do not return hidden statuses with zero or `null` values;
- do not return `missingReportCount` to final-only readers;
- do not let count subtraction reveal hidden workflow states;
- `playedMatchCount` may be omitted from the report section for final-only readers because recent-match data already exposes permitted match history;
- response DTO/projection should make the disclosure boundary explicit rather than projecting every status and redacting later.

### Report workflow summary

Return a role-appropriate shape equivalent to:

```txt
visibilityMode
statusCounts:
  status
  count

playedMatchCount          # full workflow only
missingReportCount        # full workflow only
```

Population:

- selected team;
- selected season;
- non-archived matches;
- current report status only.

Rules:

- `missingReportCount` counts played non-archived matches with no report;
- a report in `NEEDS_CORRECTION` is not considered verified for dashboard statistics;
- `ARCHIVED` remains final and countable;
- match archive excludes the match/report from dashboard sections;
- no report IDs or correction reasons are returned in this summary;
- authorization is evaluated before query projection.

### Current coach-safe availability summary

Reuse Unit 50's current assigned-player population and synthetic unknown behavior through an internal Application query/service.

Do not call the availability HTTP endpoint.

Return:

```txt
scope = CURRENT_TEAM_SNAPSHOT
asOfDate
totalPlayers
availableCount
limitedCount
unavailableCount
rehabCount
unknownCount
```

Rules:

- availability is current operational state for the selected team;
- selected season does not alter availability population or status;
- `asOfDate` makes this distinction explicit, especially for historical seasons;
- counts use assignments covering the backend current operational date;
- synthetic `UNKNOWN` is included;
- counts are mutually exclusive and sum to total;
- no player rows;
- no coach-visible notes;
- no injury count;
- no diagnosis, body area, restricted notes, injury existence, or medical-detail permission state;
- no restricted injury query is executed;
- all ordinary scoped dashboard readers may receive this safe count summary.

### Confirmed statistics leaders

Build leader groups only from trusted current final report states:

```txt
VERIFIED
ARCHIVED
```

Population:

- selected team;
- selected season;
- non-archived played matches;
- current report status `VERIFIED` or `ARCHIVED`;
- concrete `PlayerMatchAppearance` statistics;
- field enabled by the report's persisted `AppliedTrackingLevel`;
- non-null complete values guaranteed by Unit 33 final-report workflow.

Do not include:

- `DRAFT`;
- `READY_FOR_REVIEW`;
- `NEEDS_CORRECTION`;
- missing/null statistics;
- statistics disabled by the applied tracking profile;
- team statistics not defined by Unit 33;
- GPS/workload metrics;
- manual ratings;
- composite scores.

### Dashboard leader metric catalogue

Use one centralized dashboard leader definition set:

```txt
GOALS
ASSISTS
SHOTS_ON_TARGET
KEY_PASSES
DUELS_WON
BALL_RECOVERIES
SAVES
CLEAN_SHEETS
```

Map to Unit 33 fields:

```txt
GOALS -> Goals
ASSISTS -> Assists
SHOTS_ON_TARGET -> ShotsOnTarget
KEY_PASSES -> KeyPasses
DUELS_WON -> DuelsWon
BALL_RECOVERIES -> BallRecoveries
SAVES -> Saves
CLEAN_SHEETS -> count of goalkeeper rows where CleanSheet = true
```

Rules:

- use stable dashboard metric codes separate from localized labels;
- do not include cards, fouls, possession losses, or goals conceded as “top performer” metrics;
- do not calculate pass accuracy, shot accuracy, duel percentage, per-match rates, or per-90 values because minutes/denominator semantics are not confirmed in Unit 33;
- do not create an overall player ranking;
- future metrics require a reviewed spec/context change.

### Leader group shape

Return at most:

```txt
8 metric groups
3 leader rows per group
```

Each group contains:

```txt
metricCode
valueKind = INTEGER
eligibleReportCount
leaders:
  rank
  player:
    id
    displayName
  value
  appearanceCount
  matchCount
hasAdditionalTies
```

Rules:

- aggregate database-side by player;
- sort value descending;
- deterministic tie order by player display name then ID;
- rank uses competition ranking semantics for equal values;
- cap output at three rows;
- `hasAdditionalTies` is true when an equal-value row exists beyond the cap;
- include a group only when its highest value is greater than zero;
- do not label the first row “best player”;
- `appearanceCount`/`matchCount` provide context but are not used to normalize values;
- an archived/transferred player remains represented through the safe player summary;
- no restricted player information.

### Statistics leader empty state

Return a stable empty-reason code when no groups exist:

```txt
NO_FINAL_REPORTS
NO_POSITIVE_LEADER_VALUES
NO_ENABLED_LEADER_METRICS
```

Choose the most accurate reason based on the query result.

Do not fabricate zero-valued leaders.

### Comparison-safe physical workload summary

Use only Unit 48 official current workload revisions.

Population:

- selected team;
- `OccurredOn` inside the selected season date range, inclusive;
- current revision only;
- training and match contexts;
- canonical `PhysicalMetricValue` values;
- no import preview rows;
- no failed/unconfirmed import data.

Group by exact:

```txt
contextType
metricCode
comparabilityKey
unitCode
threshold context
method context
```

Stable context types:

```txt
TRAINING
MATCH
```

Do not aggregate training and match values into one group even when their metric and comparability keys match.

Do not aggregate values with different comparability keys.

### Workload aggregation semantics

Reuse the Unit 48 metric catalogue.

Dashboard workload groups include definitions whose aggregation hint is:

```txt
SUM
MAX
```

Exclude `LATEST` metrics such as session duration from team-level dashboard aggregation in Unit 52.

For each exact group, return:

```txt
contextType
metricCode
comparabilityKey
unitCode
thresholdContext
methodContext
aggregationKind
aggregateValue
workloadCount
playerCount
contextCount
firstOccurredOn
lastOccurredOn
```

Aggregation:

- `SUM` -> sum exact canonical values;
- `MAX` -> maximum exact canonical value.

Rules:

- use exact decimal/numeric aggregation;
- preserve zero;
- absence of a metric is not zero;
- `playerCount` is distinct players;
- `contextCount` is distinct training sessions or matches according to context type;
- `workloadCount` is distinct current workload snapshots contributing a value;
- threshold/method fields are returned exactly as canonical context;
- no unit conversion in the backend response;
- no average, percentile, ranking, load score, acute/chronic ratio, rolling trend, or injury interpretation;
- no vendor header/source row.

### Workload output bounds

Return at most:

```txt
24 exact comparability groups
```

Ordering:

1. stable canonical metric catalogue order;
2. context type;
3. workload count descending;
4. comparability key.

When more groups exist:

```txt
truncated = true
availableGroupCount
returnedGroupCount
```

Rules:

- aggregation still occurs database-side;
- do not load all metric rows into memory merely to cap the response;
- Unit 53 must be able to show that additional contexts exist;
- do not silently merge omitted groups.

Also return:

```txt
hasMultipleComparabilityContexts
splitMetricContextCount
```

`splitMetricContextCount` counts distinct `(contextType, metricCode)` pairs that have more than one comparability key.

### Workload empty state

When no confirmed official values exist for the selected team/season, return:

```txt
emptyReason = NO_CONFIRMED_WORKLOADS
```

Do not treat retained source files, generic import previews, or unsupported vendor mappings as confirmed workload data.

### Data-quality alerts

Return a bounded list of stable role-aware alerts.

Each alert contains:

```txt
code
severity
count
destination
isSeasonScoped
```

Stable severities:

```txt
INFO
WARNING
ERROR
```

Stable destinations:

```txt
MATCH_REPORTS
IMPORTS
AVAILABILITY
PHYSICAL_WORKLOADS
```

Return at most:

```txt
10 alerts
```

Omit zero-count alerts.

Order by:

1. severity (`ERROR`, `WARNING`, `INFO`);
2. stable code order.

Do not return localized messages; Unit 53 maps stable codes to Bosnian copy.

### Report quality alerts

Return only for `FULL_WORKFLOW` readers:

```txt
PLAYED_MATCH_WITHOUT_REPORT
REPORT_DRAFT
REPORT_READY_FOR_REVIEW
REPORT_NEEDS_CORRECTION
```

Suggested severity:

```txt
PLAYED_MATCH_WITHOUT_REPORT -> ERROR
REPORT_NEEDS_CORRECTION -> WARNING
REPORT_READY_FOR_REVIEW -> WARNING
REPORT_DRAFT -> INFO
```

Counts use the same selected team/season/non-archived population as report workflow.

Do not return these alerts to final-only report readers.

### Import quality alerts

Return only when the caller has Unit 43 import workflow access:

- `ADMIN`; or
- in-scope `DATA_OPERATOR` with `canImportData = true`.

Stable codes:

```txt
IMPORT_FAILED
IMPORT_VALIDATION_FAILED
```

Suggested severity:

```txt
IMPORT_FAILED -> ERROR
IMPORT_VALIDATION_FAILED -> WARNING
```

Season association rules:

- match-linked import belongs to the selected season when its match does;
- training-linked import belongs to the selected season when `TrainingSession.SessionDate` falls inside the season range;
- team-only/unlinked import belongs to the selected season when `CreatedAtUtc` operational date falls inside the season range;
- an import with a match/training target is not additionally classified by creation date;
- selected team must match import team;
- cancelled/imported/ready/uploaded states are not failure alerts.

Do not return failed-import counts to callers who cannot access imports.

### Availability quality alert

Return to all safe dashboard readers when count is positive:

```txt
AVAILABILITY_UNKNOWN
```

Severity:

```txt
WARNING
```

Rules:

- count equals Unit 50 synthetic/explicit unknown count for the current selected-team snapshot;
- `isSeasonScoped = false`;
- no player IDs or medical information;
- no injury alert.

### Workload comparability alert

Return when:

```txt
splitMetricContextCount > 0
```

Code:

```txt
WORKLOAD_COMPARABILITY_SPLIT
```

Severity:

```txt
INFO
```

Rules:

- count is the split metric/context pair count;
- it is informational, not a data error;
- no vendor claim;
- no attempt to merge groups.

### Role-aware section behavior

The endpoint is readable by all authenticated active staff for authorized teams.

Section disclosure differs by existing feature authorization:

#### Recent matches and form

Available to all scoped dashboard readers.

#### Report workflow and report alerts

Use Unit 32 report visibility.

#### Availability

Use Unit 50 safe summary only.

#### Statistics leaders

Available to all scoped readers because data comes only from currently final report states they may safely consume.

#### Workload summary

Available to all scoped readers under Unit 48 read rules.

#### Import alerts

Available only to Unit 43 import-authorized readers.

Do not expose a boolean such as `hasHiddenReportAlerts` or `hasHiddenImportAlerts`.

Omitted data must not reveal inaccessible counts.

### Stable response shape and empty states

Use explicit section DTOs.

Recommended conceptual response:

```txt
DashboardOverviewResponse
- context
- recentMatches
- teamForm
- reportWorkflow
- availability
- statisticsLeaders
- physicalWorkload
- qualityAlerts
- generatedAtUtc
```

Each collection is a non-null bounded array.

Use stable empty-reason codes where specified.

Do not use `object`, `dynamic`, or arbitrary JSON dictionaries for core sections.

Do not embed raw domain entities.

### Query composition

Create one Application query/use case equivalent to:

```txt
GetDashboardOverview
```

Create focused internal readers/components equivalent to:

```txt
DashboardContextReader
RecentMatchesDashboardReader
ReportWorkflowDashboardReader
AvailabilityDashboardReader
StatisticsLeadersDashboardReader
PhysicalWorkloadDashboardReader
DashboardQualityAlertReader
```

Exact names may follow repository conventions.

Rules:

- reuse established domain/Application predicates and catalogues;
- do not duplicate report-visibility or metric-comparability logic in endpoint handlers;
- do not call HTTP endpoints internally;
- do not instantiate another `DbContext` per card without a documented reason;
- use `AsNoTracking` or existing read conventions;
- database-side filtering/grouping/projection;
- no N+1 player/opponent/team queries;
- no unbounded `ToListAsync` before aggregation;
- cancellation token propagated through all operations.

### Consistency and failure behavior

Capture one:

```txt
generatedAtUtc
```

and one current operational date for the whole response.

Rules:

- use those shared values in availability and date-sensitive logic;
- minor cross-query database timing differences are acceptable for a live read model;
- do not hold a long repeatable-read transaction solely to render a dashboard;
- return either a valid complete response or safe ProblemDetails;
- do not return partially fabricated sections after an unexpected query failure;
- truthful empty sections are valid and not errors;
- authorization is checked before expensive section queries.

### Performance bounds

Hard response bounds:

```txt
recent matches: 5
form matches: same 5
leader metric groups: 8
leaders per metric: 3
workload groups: 24
quality alerts: 10
context teams/seasons: expected-small settings lists
```

Rules:

- no nested unbounded player/statistic/workload arrays;
- no complete report list;
- no complete import list;
- no complete availability player list;
- no complete workload history;
- no query per player;
- no query per metric definition;
- inspect generated SQL/query plans where practical in integration/performance tests;
- log only safe timing and section row/group counts.

Do not add a Redis/distributed cache in Unit 52.

### Caching

The dashboard is private authenticated data.

Rules:

- no public caching headers;
- no static JSON artifact;
- no persisted server snapshot table;
- a short private client query cache will be handled by Unit 53;
- backend authorization is evaluated on every request;
- restricted medical data is never queried, so no restricted-cache concern is introduced here.

### Data privacy

Dashboard responses must never contain:

- injury IDs;
- injury counts;
- body area;
- diagnosis;
- restricted notes;
- coach-visible note contents;
- medical-detail permission flags;
- source import rows/files;
- storage keys/paths;
- correction reasons;
- hidden report statuses;
- hidden import counts;
- workload vendor headers;
- audit payloads.

Availability contains counts only.

Player leader summaries contain only safe identity/display information.

### API error handling

Use existing Result and ProblemDetails conventions.

Expected behavior:

- `400` for missing/invalid team or season query syntax;
- `401` unauthenticated;
- `403` explicit out-of-scope team;
- `404` missing/safely inaccessible team or season;
- safe `5xx` for unexpected query/persistence failures.

Do not leak:

- hidden team existence;
- hidden report/import status counts;
- SQL/provider details;
- restricted medical data;
- stack traces;
- query plans.

### Observability

Safe logs may include:

```txt
teamId
seasonId
caller user ID
report visibility mode
section durations
returned group/count sizes
generatedAtUtc
correlation ID
```

Do not log:

- complete response bodies;
- player leader arrays;
- medical data;
- import filenames;
- workload values;
- source rows;
- authorization claims beyond safe diagnostics.

### Tests

Add focused unit, Application, authorization, privacy, SQL/integration, and endpoint tests.

#### Context-options tests

Cover:

- admin teams;
- all-teams user;
- selected-teams user;
- inaccessible teams absent;
- archived teams excluded;
- inactive accessible teams included;
- active and archived seasons included;
- season ordering;
- safe fields only;
- no settings mutation actions;
- unauthenticated/disabled/password-change gates.

#### Dashboard context tests

Cover:

- required team/season;
- accessible team;
- explicit out-of-scope `403`;
- archived season accepted;
- archived team rejected;
- empty season data;
- one captured clock instant/date;
- no implicit context substitution.

#### Recent-match/form tests

Cover:

- selected team/season only;
- played/non-archived only;
- latest five;
- deterministic tie order;
- win/draw/loss;
- goals for/against;
- form order;
- score correction reflected;
- no report detail leakage;
- hidden report status omitted for final-only roles;
- verified/archived status visible when permitted.

#### Report workflow tests

Cover:

- full workflow role;
- final-only role;
- every status count;
- missing report count;
- archived match exclusion;
- current status only;
- verified-to-correction exclusion from final count;
- no hidden status fields/zeroes in serialized final-only JSON;
- no subtraction disclosure.

#### Availability tests

Cover:

- same population/counts as Unit 50 summary;
- synthetic unknown;
- selected team scope;
- current date independent of season;
- historical season still returns current snapshot with `asOfDate`;
- safe fields only;
- no injury query/projection;
- serialized JSON has no restricted properties.

#### Statistics leader tests

Cover:

- verified reports;
- archived reports;
- draft/review/correction exclusion;
- archived match exclusion;
- applied tracking-level field gating;
- each supported metric mapping;
- clean-sheet counting;
- aggregate by player;
- appearance/match counts;
- zero top value omitted;
- tie ranking/order/cap/additional-ties;
- maximum group/row bounds;
- no rate/composite metrics;
- no null-as-zero;
- transferred/archived player safe summary;
- empty-reason codes.

#### Workload summary tests

Cover:

- selected team/season range;
- current revision only;
- training/match separation;
- exact comparability-key separation;
- threshold-context split;
- method-version split;
- SUM behavior;
- MAX behavior;
- LATEST exclusion;
- zero preservation;
- absent value exclusion;
- distinct player/context/workload counts;
- output ordering/bound/truncation;
- split count;
- no import preview/unconfirmed data;
- no vendor rows/headers;
- empty reason.

#### Quality-alert tests

Cover:

- report alerts for full readers;
- report alerts absent for final-only readers;
- import alerts for admin;
- import alerts for in-scope permitted data operator;
- import alerts absent for unauthorized roles;
- match-linked import season association;
- training-linked import season association;
- team-only import creation-date association;
- failed versus validation-failed;
- availability unknown current/non-season scope;
- workload split alert;
- zero alerts omitted;
- severity/code order;
- maximum bound;
- no IDs/restricted data.

#### Query/performance tests

Cover where practical:

- no N+1 query pattern;
- no unbounded entity materialization;
- section limits;
- database-side grouping;
- cancellation propagation;
- response size under a documented realistic fixture;
- safe failure behavior.

#### Privacy serialization tests

Assert dashboard JSON contains none of:

```txt
injury
bodyArea
diagnosis
restrictedNotes
coachVisibleNote
correctionReason
storageKey
```

Also verify final-only response does not serialize hidden report workflow keys.

### Documentation synchronization

Update:

```txt
context/architecture.md
context/progress-tracker.md
```

Architecture should document:

- one composite dashboard read model;
- selected team/season context;
- current availability snapshot semantics;
- final-only versus full report disclosure;
- final-report-only statistics leaders;
- exact comparability workload aggregation;
- role-aware quality alerts;
- no restricted medical fields.

Progress tracker should record:

- endpoint routes;
- response bounds;
- leader metric set;
- workload aggregation behavior;
- report/import alert authorization;
- no-migration status;
- verification/performance results;
- intentionally deferred UI, advanced analytics, caching, standings, ratings, and export.

If implementation reveals that an existing feature query cannot be reused without changing its authorization or privacy contract, update the relevant context/spec before continuing.

Do not silently duplicate or weaken those rules.

## Implementation

### 1. Add dashboard context-options query

Implement:

```txt
GET /api/dashboard/context-options
```

Return safe scoped teams and safe season references.

Do not expose settings mutation behavior.

### 2. Add dashboard response contracts

Create explicit DTOs for:

- context;
- recent match;
- team form;
- report workflow visibility/counts;
- availability counts;
- statistics leader groups;
- workload groups;
- quality alerts;
- empty reasons.

Avoid arbitrary JSON/dynamic objects.

### 3. Build recent match and form reader

Implement latest-five played-match projection and deterministic form calculation.

Reuse Unit 30 score/location semantics.

### 4. Build role-aware report workflow reader

Reuse Unit 32 visibility rules.

Return full or final-only DTOs without hidden-field projection.

### 5. Build safe availability reader

Reuse Unit 50 current-team summary logic internally.

Return current snapshot counts and `asOfDate` only.

### 6. Build confirmed statistics leaders

Centralize the eight approved leader definitions.

Aggregate only current verified/archived report statistics with applied field gating.

Enforce output/tie bounds.

### 7. Build comparison-safe workload reader

Use Unit 48 current revisions/catalogue.

Group and aggregate by exact context/metric/comparability key.

Enforce output bounds and split metadata.

### 8. Build role-aware quality alerts

Compose report, import, availability, and workload alerts only when the caller may access their source feature.

Keep codes stable and payloads bounded.

### 9. Compose the overview query and endpoint

Validate context once, capture clock values once, execute focused readers, and map one complete response.

Do not perform self-HTTP calls.

### 10. Add tests and performance verification

Implement all required authorization, privacy, aggregation, bound, SQL, and integration coverage.

No migration should be generated.

### 11. Synchronize documentation

Update `context/architecture.md` and `context/progress-tracker.md` with actual implementation and verification results.

Do not mark Unit 52 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use existing backend packages and infrastructure:

- ASP.NET Core 8;
- EF Core/Npgsql;
- existing Result/ProblemDetails conventions;
- approved UTC clock/current-user services;
- Unit 21 season data;
- Unit 22 team data;
- Unit 23 team-scope authorization;
- Unit 30 matches;
- Unit 32 report workflow/visibility;
- Unit 33 statistics catalogue and applied tracking levels;
- Unit 43 import permissions/statuses;
- Unit 48 workload catalogue/comparability/current revisions;
- Unit 50 safe availability summary logic;
- existing test infrastructure.

Do not add:

- analytics/BI packages;
- OLAP engines;
- cache servers/clients;
- background-job packages;
- materialized-view frameworks;
- ranking/statistics libraries;
- medical packages;
- charting/frontend dependencies.

## Verification checklist

- [ ] `GET /api/dashboard/context-options` exists.
- [ ] Context options require authenticated active staff.
- [ ] Context teams are restricted by Unit 23 scope.
- [ ] Archived teams are excluded while accessible inactive teams remain available.
- [ ] Context seasons expose only safe ID/name/date/archive fields.
- [ ] Archived seasons remain available for historical dashboards.
- [ ] No settings mutation actions/admin metadata are exposed.
- [ ] No default/current season field is persisted or invented.
- [ ] `GET /api/dashboard/overview` exists.
- [ ] Dashboard overview requires explicit `teamId` and `seasonId`.
- [ ] Out-of-scope team returns `403`.
- [ ] Archived historical season is accepted.
- [ ] Archived team is rejected from ordinary dashboard context.
- [ ] No implicit team/season substitution occurs.
- [ ] One generated timestamp/current operational date is captured for the response.
- [ ] Recent matches include at most five selected-team/season played non-archived matches.
- [ ] Recent match result uses FK Velež score perspective.
- [ ] Team form uses the same five matches.
- [ ] Form W/D/L counts and goals totals are correct.
- [ ] No standings, points, xG, rating, or strength weighting is added.
- [ ] Hidden report status is omitted from recent matches for unauthorized roles.
- [ ] Report workflow uses `FULL_WORKFLOW` or `FINAL_ONLY` according to Unit 32.
- [ ] Full workflow counts current DRAFT/READY/NEEDS_CORRECTION/VERIFIED/ARCHIVED states.
- [ ] Full workflow counts played matches with no report.
- [ ] Final-only responses contain only VERIFIED/ARCHIVED status counts.
- [ ] Final-only serialized JSON contains no hidden status/missing-report fields.
- [ ] Match archive removes match/report from dashboard populations.
- [ ] Availability uses Unit 50's current assignment/synthetic unknown semantics.
- [ ] Availability explicitly reports `CURRENT_TEAM_SNAPSHOT` and `asOfDate`.
- [ ] Selected historical season does not rewrite current availability semantics.
- [ ] Availability counts sum to total.
- [ ] Dashboard availability contains no player rows, coach notes, injury count, diagnosis, body area, restricted notes, or permission state.
- [ ] Statistics leaders use only current VERIFIED/ARCHIVED reports.
- [ ] DRAFT/READY/NEEDS_CORRECTION reports never contribute to leaders.
- [ ] Applied tracking-level field gating is respected.
- [ ] Leader metrics are limited to the approved eight stable codes.
- [ ] No rate, per-90, percentage, or overall player score is calculated.
- [ ] Leader aggregation is player-based and database-side.
- [ ] Leader groups are capped at eight and rows at three.
- [ ] Tie ranking/order/additional-tie behavior is deterministic.
- [ ] Zero-only leader groups are omitted.
- [ ] Empty leader reason is truthful.
- [ ] Workload summary uses current official revisions only.
- [ ] Workload values are selected-team and within selected season dates.
- [ ] Training and match contexts remain separate.
- [ ] Exact comparability key is part of every group.
- [ ] Different threshold/method contexts are never merged.
- [ ] SUM and MAX catalogue semantics are respected.
- [ ] LATEST metrics are excluded from team aggregation.
- [ ] Zero is preserved and absent values are excluded.
- [ ] Workload response includes workload/player/context counts.
- [ ] Workload groups are capped at 24 with truthful truncation metadata.
- [ ] Multiple comparability contexts are surfaced, not normalized away.
- [ ] No raw vendor/import/source data is returned.
- [ ] Quality alerts are stable, bounded, and omit zero counts.
- [ ] Report alerts are returned only to full workflow readers.
- [ ] Import alerts are returned only to Unit 43 import-authorized readers.
- [ ] Import season association handles match, training, and team-only jobs explicitly.
- [ ] Current unknown availability alert is marked non-season-scoped.
- [ ] Workload split alert is informational.
- [ ] No hidden alert-presence flag reveals inaccessible data.
- [ ] Dashboard response includes no injury IDs/counts/details.
- [ ] Dashboard response includes no coach-visible note contents.
- [ ] Dashboard response includes no correction reasons, storage paths/keys, source rows, or audit payloads.
- [ ] Core response uses explicit DTOs rather than arbitrary dictionaries/domain entities.
- [ ] Dashboard composition reuses internal query services rather than self-HTTP calls.
- [ ] Authorization is checked before expensive section queries.
- [ ] No N+1 player/opponent/team queries exist.
- [ ] No unbounded entity materialization occurs before grouping.
- [ ] All section response bounds are enforced.
- [ ] Cancellation tokens propagate through dashboard queries.
- [ ] Unexpected section failure does not return fabricated partial data.
- [ ] No dashboard mutation endpoint exists.
- [ ] No cache server, materialized table/view, background job, or migration is added.
- [ ] Context/authorization tests pass.
- [ ] Recent match/form tests pass.
- [ ] Full/final report disclosure tests pass.
- [ ] Availability privacy/semantic tests pass.
- [ ] Statistics leader gating/tie/bound tests pass.
- [ ] Workload comparability/aggregation/bound tests pass.
- [ ] Role-aware quality alert tests pass.
- [ ] Query/performance tests pass where practical.
- [ ] Serialized privacy-property tests pass.
- [ ] No frontend files are changed.
- [ ] No analytics, cache, background-job, ranking, medical, or frontend package is added.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for affected backend projects/solution.
- [ ] `dotnet test` passes for relevant backend test projects.
- [ ] `context/architecture.md` reflects the bounded privacy-safe dashboard composition.
- [ ] `context/progress-tracker.md` records actual Unit 52 routes, bounds, aggregation, authorization, no-migration, and verification state.
