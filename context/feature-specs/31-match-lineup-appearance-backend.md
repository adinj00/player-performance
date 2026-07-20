# Unit 31: Match Lineup and Appearance Backend

## Goal

Build the backend foundation for match lineup, starters, substitutes, captain, substitution events, player minutes, and concrete player match appearances. Keep participation linked to the persistent player and match records, validate player eligibility against team assignment history, and provide a stable appearance identity that later match statistics can reference.

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
9. `context/feature-specs/31-match-lineup-appearance-backend.md`

Use relevant project-local skills from `.agents/skills/` when applicable. Skills may guide implementation workflow but must not override project context, architecture rules, code standards, or this spec.

This unit is backend-only.

Do not add or change frontend routes, pages, components, navigation, shadcn/ui components, or frontend API wrappers in this unit.

### Scope

This unit extends the existing Match module with the concrete participation structures needed before report workflow and match statistics can exist.

Implement:

- one lineup record per match;
- optional formation text;
- starters;
- substitutes/bench players;
- starting captain;
- ordered substitution events;
- concrete player match appearances;
- manually recorded player minutes;
- player eligibility validation against assignment history;
- team-scope-aware read access;
- authorized lineup mutation;
- stable appearance IDs for future statistics.

This unit does not implement:

- player or goalkeeper statistics;
- team statistics;
- match report status or review workflow;
- report verification or correction requests;
- GPS/physical metrics;
- media;
- imports;
- audit persistence;
- frontend lineup editor;
- configurable competition squad-size rules;
- fixed starting-player count rules;
- shirt numbers;
- player positions;
- tactical positions on a pitch;
- captain changes during the match;
- match-event timeline beyond substitutions.

### Domain model

Add the minimum persistent structures needed for a complete match lineup and concrete appearances.

Use domain names consistent with the architecture context, including `MatchLineup` and `PlayerMatchAppearance`.

A practical model should include:

#### `MatchLineup`

One lineup record per match.

Store at minimum:

- match identity;
- optional normalized `Formation` text;
- optional starting captain player reference;
- timestamps consistent with project conventions.

Use the match identity as the one-to-one ownership boundary according to the project's existing EF Core conventions.

Do not store duplicate match metadata on the lineup.

#### `MatchLineupEntry`

Represents a player named in the match squad.

Store at minimum:

- entry identity;
- `MatchId`;
- `PlayerId`;
- lineup role;
- timestamps if required by existing persistence conventions.

Use an internal lineup-role enum with:

```txt
STARTER
SUBSTITUTE
```

A player may appear only once in the lineup for a given match.

Do not hardcode exactly 11 starters or a fixed number of substitutes. The system supports multiple club selections, and competition-specific squad-size rules are not yet modeled.

#### `PlayerMatchAppearance`

Represents concrete participation in a played match.

Store at minimum:

- stable appearance identity;
- `MatchId`;
- `PlayerId`;
- `MinutesPlayed`;
- timestamps consistent with project conventions.

Enforce one appearance per `MatchId + PlayerId`.

The appearance ID must remain stable across ordinary lineup/participation saves for the same match and player. Do not delete and recreate unchanged appearance rows on every update.

Future Unit 33 player and goalkeeper statistics will link to concrete `PlayerMatchAppearance` records rather than only to the current player-team assignment.

#### `MatchSubstitution`

Represents an ordered substitution event.

Store at minimum:

- substitution identity;
- `MatchId`;
- `PlayerOutId`;
- `PlayerInId`;
- non-negative regulation/event minute;
- optional non-negative stoppage-time minute;
- positive event sequence/order within the match;
- timestamps consistent with project conventions.

The sequence value exists so multiple substitution events at the same displayed minute have deterministic order and can be validated as an ordered on-field state transition.

Do not model goals, cards, shots, possession events, or other event types in this unit.

### Formation

`Formation` is optional free text in this unit.

Normalize whitespace and enforce a reasonable length limit according to existing validation conventions.

Examples may include values such as `4-3-3` or `4-2-3-1`, but do not parse the string into tactical positions and do not hardcode an allowed formation catalog.

### Starting captain

The starting captain is optional while a preliminary lineup is being prepared for a `SCHEDULED` or `POSTPONED` match.

When a played-match participation state is saved:

- a captain is required when the lineup contains starters;
- the captain must be one of the lineup's `STARTER` entries.

This unit models only the starting captain. Mid-match captain changes are out of scope.

### Player eligibility

The lineup must use persistent Player records and the team-assignment history introduced in Unit 28.

When adding a player who is not already part of the existing lineup:

- the player must exist;
- the player must not be archived;
- the player must have a team assignment covering the match date for the match's immutable `TeamId`;
- an assignment to a different team does not make the player eligible for this match;
- multiple simultaneous assignments remain valid when one of them covers the match team.

Use the project's established date comparison convention when checking the match kickoff against date-only assignment ranges. Do not introduce an unrelated timezone model in this unit.

Historical lineup and appearance records remain valid if the player is later archived or their current assignment changes after the match. Do not cascade-delete historical participation when player or assignment lifecycle changes.

When a full lineup is saved, existing unchanged historical references must not fail only because the player's current state changed after the original participation was recorded. New additions or replacements must satisfy current eligibility validation for the match date.

### Preliminary lineup versus played-match participation

Lineup preparation and concrete participation have different rules.

For `SCHEDULED` and `POSTPONED` matches:

- lineup entries may be saved;
- formation may be saved;
- captain may be saved when known;
- appearances must be empty;
- substitutions must be empty;
- player minutes must not be recorded because no concrete appearance has occurred yet.

For `PLAYED` matches:

- lineup entries may be saved and corrected;
- concrete appearances may be saved;
- player minutes may be saved;
- substitution events may be saved;
- full participation consistency rules apply.

For `CANCELLED` matches:

- an already prepared preliminary lineup may remain readable for history;
- appearances and substitutions must remain empty;
- new participation data must not be created;
- lineup mutation is not allowed in this unit.

For archived matches:

- lineup and participation remain readable to authorized callers according to existing archive/read conventions;
- mutation is not allowed.

Unit 32 may add additional report-workflow-based edit restrictions later. Do not pre-implement those restrictions here.

### Appearance and substitution consistency

For a `PLAYED` match, treat the saved participation state as one consistent snapshot.

Use the initial `STARTER` entries as the initial on-field player set.

Process substitutions in ascending event sequence.

For each substitution event:

- `PlayerOutId` and `PlayerInId` must be different;
- both players must be named in the match lineup;
- the outgoing player must currently be on the field at that point in the ordered sequence;
- the incoming player must not currently be on the field at that point;
- after the event, remove the outgoing player from the on-field set and add the incoming player.

This state validation intentionally supports a player returning later after previously leaving the field, because youth or competition rules may permit return substitutions. Do not hardcode a universal no-reentry rule.

The concrete appearance set for a played match must equal:

- all initial starters; plus
- every player who enters through at least one valid substitution event.

Therefore:

- every starter has one `PlayerMatchAppearance`;
- an unused substitute has no appearance;
- a substitute who enters has one appearance;
- a returning player still has only one appearance record for the match;
- no player outside the lineup may have an appearance.

`MinutesPlayed` is authoritative manual match data in this unit.

Rules:

- minutes must be a non-negative integer;
- zero is allowed for edge cases such as an extremely late appearance;
- do not derive minutes automatically from substitution minute values;
- do not enforce a fixed 90-minute or 120-minute maximum because match duration rules are not yet modeled;
- do not enforce that the sum of all player minutes equals a fixed team total.

Substitution minute fields are chronology/display data, while `MinutesPlayed` is the stored participation metric.

### Atomic lineup save contract

Use one aggregate-oriented save operation for the complete lineup/participation state rather than multiple unrelated child mutation endpoints.

Required API:

```txt
GET /api/matches/{matchId}/lineup
PUT /api/matches/{matchId}/lineup
```

`GET` returns the complete current state:

- match identity and minimal match status/team context needed by the consumer;
- formation;
- captain player summary when present;
- lineup entries with player summaries and `STARTER`/`SUBSTITUTE` role;
- stable appearance IDs and minutes for players who appeared;
- ordered substitution events.

`PUT` replaces the desired logical lineup/participation snapshot atomically.

The request should carry at minimum:

- optional formation;
- optional captain player ID;
- complete lineup entry collection;
- complete appearance collection for a played match;
- complete ordered substitution collection for a played match.

The Application layer must validate the complete requested snapshot before persistence changes are committed.

Do not expose separate endpoints that allow substitutions, appearances, and lineup entries to drift into inconsistent states.

### Stable child identity and persistence update behavior

The aggregate save operation is logically replace-all from the client's perspective, but persistence must preserve stable identities where required.

For `PlayerMatchAppearance`:

- match an existing record by `MatchId + PlayerId`;
- preserve its existing appearance ID when the player remains in the appearance set;
- create a new appearance ID only when a player becomes a new concrete appearance;
- remove an appearance only when it is no longer present in a valid saved snapshot and no dependent data prevents removal.

Unit 33 will add statistics that reference appearance IDs. Design the persistence update path now so ordinary lineup edits do not churn appearance IDs.

For lineup entries, use a unique `MatchId + PlayerId` constraint and preserve existing row identity when practical.

For substitution events, use stable row identity according to the project's normal aggregate update conventions. The API does not need to expose arbitrary child-level CRUD endpoints.

### Match status interaction

Do not change the Unit 30 match status transition model in this unit.

A match may still transition to `PLAYED` through the existing match update use case before lineup/participation entry is complete.

This unit must not make the Unit 30 `SCHEDULED -> PLAYED` transition depend on a completed lineup, because staff may enter match metadata and participation in different orders.

Instead:

- concrete appearances/substitutions are allowed only after the match is `PLAYED`;
- report workflow and later verification rules will determine when incomplete participation blocks workflow progress.

### Authorization model

Backend authorization remains the source of truth.

Reuse Unit 30 match access rules and Unit 23 team-scope infrastructure.

Read behavior:

- authenticated active staff may read lineup/participation only when they can access the parent match's team;
- `ADMIN` and `ALL_TEAMS` users follow existing broad match-read rules;
- `SELECTED_TEAMS` users may read only matches within assigned team scope;
- inaccessible match resources must follow the existing safe not-found/inaccessible-resource behavior and must not leak player or lineup data.

Mutation behavior:

- `ADMIN` may save lineup/participation for any accessible match;
- `DATA_OPERATOR` may save lineup/participation only for matches inside authorized team scope;
- other roles do not receive lineup mutation permission in this unit;
- every save must re-check authorization server-side;
- archived matches cannot be mutated;
- cancelled matches cannot be mutated in this unit.

Do not rely on future frontend action hiding for security.

### Validation and conflicts

Use FluentValidation for request-shape validation where appropriate and domain/application validation for cross-record invariants.

Reject at minimum:

- duplicate player IDs in lineup entries;
- unknown lineup role values;
- captain not present in the lineup;
- captain not a starter when played participation is saved;
- duplicate appearance player IDs;
- appearance for a player outside the lineup;
- missing starter appearance in a played participation snapshot;
- appearance for an unused substitute who never enters through a substitution;
- substitution using the same player in and out;
- substitution player outside the lineup;
- duplicate or non-positive substitution sequence values;
- negative substitution minute;
- negative stoppage-time minute;
- invalid ordered on-field transition;
- negative player minutes;
- concrete appearance/substitution data on `SCHEDULED`, `POSTPONED`, or `CANCELLED` matches;
- new ineligible lineup players;
- mutation of archived or cancelled matches;
- caller without mutation permission or team scope.

Use the existing Result and ProblemDetails conventions.

Expected behavior:

- `400` for malformed request input;
- `401` for unauthenticated requests;
- `403` for authenticated callers without mutation permission or explicit team access where the existing API convention uses forbidden;
- `404` for missing or safely inaccessible match resources according to existing conventions;
- `409` for aggregate/domain conflicts such as invalid participation state or immutable/dependent-data conflicts;
- `422` for semantic validation errors if that is the established project convention.

Do not expose internal database details or restricted player data in errors.

### Clean Architecture boundaries

Keep responsibilities aligned with the existing architecture.

Domain owns:

- lineup/participation entities and enums where domain-owned by existing conventions;
- lineup uniqueness and captain invariants that do not require external data;
- appearance and substitution state invariants that operate on an already-loaded aggregate snapshot;
- stable business rules for concrete participation.

Application owns:

- get-lineup query;
- save-lineup command/use case;
- authorization orchestration;
- team-assignment eligibility checks;
- parent match state checks;
- complete snapshot validation;
- transaction boundary;
- mapping the logical replace-all request into stable persistence changes.

Infrastructure owns:

- EF Core configuration;
- PostgreSQL persistence;
- loading the complete aggregate state needed for validation;
- migration.

API owns:

- route mapping;
- HTTP request/response mapping;
- mapping Application results to existing ProblemDetails conventions.

Do not place EF Core queries, player eligibility logic, substitution state simulation, or business rules directly in endpoint handlers.

### EF Core persistence and migration

Add the lineup, lineup entry, appearance, and substitution persistence structures to the existing `AppDbContext`.

Create the migration through the approved `dotnet ef` workflow.

Keep migration files under:

```txt
backend/src/Infrastructure/Persistence/Migrations/
```

Use:

```txt
--output-dir Persistence/Migrations
```

Do not handwrite migration files unless the CLI workflow is genuinely blocked; document any exception in `context/progress-tracker.md`.

Configure at minimum:

- one lineup per match;
- unique `MatchId + PlayerId` for lineup entries;
- unique `MatchId + PlayerId` for player match appearances;
- unique positive substitution sequence within a match;
- foreign keys to Match and Player;
- restrictive/no-cascade behavior from Player to historical match participation;
- child cleanup behavior from Match only if consistent with the existing archive/no-hard-delete architecture and safe for future dependent statistics.

Do not configure player deletion to cascade into historical lineup or appearance data.

### Tests

Add focused tests for the behavior introduced by this unit.

Domain/unit tests should cover at minimum:

- duplicate lineup player rejection;
- captain starter invariant;
- played participation requires every starter appearance;
- unused substitute has no appearance;
- substitute entering creates the expected appearance-set requirement;
- returning player still maps to one appearance;
- ordered substitution state validation;
- outgoing player must be on field;
- incoming player must be off field;
- same player cannot be both in and out in one event;
- duplicate substitution sequence rejection;
- negative minute/stoppage/minutes-played rejection;
- zero minutes allowed;
- no fixed starter-count invariant;
- no fixed total-minute invariant.

Application/integration tests should cover at minimum:

- unauthenticated read/write behavior;
- team-scope-aware lineup read;
- inaccessible match does not leak lineup/player data;
- `ADMIN` save behavior;
- `DATA_OPERATOR` save inside assigned team scope;
- `DATA_OPERATOR` cannot save outside assigned team scope;
- other roles cannot mutate;
- scheduled/postponed lineup can save starters/substitutes without appearances or substitutions;
- concrete participation rejected for non-played matches;
- cancelled and archived match mutation rejected;
- new lineup player eligibility against match team and match date;
- player assigned only to another team is rejected;
- multiple active assignments work when one assignment covers the match team;
- existing historical lineup remains readable after later player lifecycle changes;
- atomic save rolls back completely when any participation invariant fails;
- stable appearance ID is preserved when the same player remains in the appearance set across saves;
- new appearance gets a new stable ID;
- read model returns formation, captain, lineup roles, appearance IDs/minutes, and ordered substitutions;
- migration/database behavior works in the existing integration-test environment.

Do not add frontend tests in this unit.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

If implementation reveals that lineup structure, appearance identity, substitution rules, player eligibility, role behavior, team-scope behavior, or API conventions must differ from documented project context, update the relevant context file before continuing.

Do not silently introduce fixed squad sizes, player positions, shirt numbers, match statistics, report workflow, or other future feature behavior.

## Implementation

### 1. Add lineup and participation domain structures

Create:

- `MatchLineup`;
- `MatchLineupEntry`;
- lineup role enum;
- `PlayerMatchAppearance`;
- `MatchSubstitution`.

Implement the domain invariants that can be validated against a complete in-memory snapshot.

### 2. Add persistence mapping

Add EF Core configuration for all new entities.

Configure:

- one-to-one match/lineup ownership;
- unique match/player constraints;
- substitution sequence uniqueness;
- restrictive historical player relationships;
- indexes needed for match-centric participation queries.

Generate the migration through the approved EF Core CLI workflow.

### 3. Add lineup read query

Implement the query for:

```txt
GET /api/matches/{matchId}/lineup
```

Return one complete read model containing lineup metadata, player summaries, roles, stable appearance IDs and minutes, and ordered substitutions.

Apply parent-match team-scope authorization before returning data.

### 4. Add atomic lineup save use case

Implement:

```txt
PUT /api/matches/{matchId}/lineup
```

The use case must:

1. authenticate and authorize the caller;
2. load the parent match and current complete lineup/participation state;
3. validate match lifecycle/status rules;
4. validate new player eligibility against assignment history;
5. validate the requested complete lineup snapshot;
6. validate played-match appearance and ordered substitution consistency;
7. preserve stable appearance IDs for unchanged `MatchId + PlayerId` records;
8. apply all changes inside one transaction;
9. return the updated complete read model or the project's established success response.

Do not persist partial changes if any validation step fails.

### 5. Add Minimal API routes

Map the two required endpoints under the existing Matches route conventions.

Keep endpoint handlers thin and free of EF Core queries or business rules.

### 6. Add tests

Add unit and integration coverage for all critical lineup, appearance, substitution, player eligibility, authorization, atomicity, and stable-ID behavior described above.

### 7. Update progress documentation

Update `context/progress-tracker.md` to reflect actual implementation and verification results.

Do not mark Unit 31 complete until required checks pass or a failure is explicitly documented.

## Dependencies

None.

Use the packages and infrastructure already introduced by previous units.

Do not add new NuGet or npm packages unless an existing approved dependency is genuinely missing and the need is documented before installation.

## Verification checklist

- [ ] `MatchLineup` exists as one lineup record per match.
- [ ] `MatchLineupEntry` supports `STARTER` and `SUBSTITUTE` without a hardcoded fixed squad size.
- [ ] A player can appear only once in the lineup for a match.
- [ ] Optional formation is stored without introducing tactical-position modeling.
- [ ] The played-match captain is a starter.
- [ ] `PlayerMatchAppearance` exists as a stable concrete match participation record.
- [ ] Appearance identity is unique by `MatchId + PlayerId` and remains stable across ordinary saves.
- [ ] `MatchSubstitution` stores ordered substitution events with deterministic sequence.
- [ ] Substitution state validation supports valid return substitutions without imposing a universal no-reentry rule.
- [ ] Every starter has one appearance in a played participation snapshot.
- [ ] An unused substitute has no appearance.
- [ ] A substitute who enters has one appearance.
- [ ] A returning player still has only one appearance record.
- [ ] Player minutes are non-negative manual data and are not automatically derived from substitution times.
- [ ] No fixed 90/120-minute maximum or fixed total-minute sum is introduced.
- [ ] New lineup players are validated against the match team's assignment history at the match date.
- [ ] Multiple simultaneous team assignments remain supported when one covers the match team.
- [ ] Historical lineup/appearance data is not cascade-deleted by later player or assignment lifecycle changes.
- [ ] `SCHEDULED` and `POSTPONED` matches may store preliminary lineup data but no concrete appearances or substitutions.
- [ ] `PLAYED` matches may store complete lineup, appearances, minutes, and substitutions.
- [ ] `CANCELLED` and archived matches cannot be mutated in this unit.
- [ ] `GET /api/matches/{matchId}/lineup` returns the complete authorized read model.
- [ ] `PUT /api/matches/{matchId}/lineup` validates and saves the complete logical snapshot atomically.
- [ ] A failed save leaves the previous lineup/participation state unchanged.
- [ ] Team-scope authorization is enforced server-side for reads and writes.
- [ ] `ADMIN` can save lineup/participation for any team.
- [ ] `DATA_OPERATOR` can save only inside authorized team scope.
- [ ] Other roles cannot mutate lineup/participation in this unit.
- [ ] Endpoint handlers remain thin and contain no EF Core queries or business rules.
- [ ] Migration files are generated under `backend/src/Infrastructure/Persistence/Migrations/`.
- [ ] Relevant domain/unit tests pass.
- [ ] Relevant integration tests pass.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for the affected backend solution/projects.
- [ ] `dotnet test` passes for the relevant backend test projects.
- [ ] `dotnet ef database update` succeeds when the local database environment is available.
- [ ] No frontend files are changed unless a documentation-only sync is explicitly required.
- [ ] `context/progress-tracker.md` reflects the actual Unit 31 implementation and verification state.
