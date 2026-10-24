# Unit 35: Lineup and Appearances UI

## Goal

Build the real lineup and participation interface inside the match detail `Sastav` tab, using the atomic lineup contract from Unit 31. Add the minimal backend eligible-player query required to select players according to the match team and match date, then provide permission-aware preliminary-lineup editing, played-match appearances, minutes, and ordered substitutions without implementing statistics, report actions, GPS, media, or audit UI.

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
10. `context/feature-specs/28-player-team-assignment-backend.md`
11. `context/feature-specs/31-match-lineup-appearance-backend.md`
12. `context/feature-specs/32-match-report-workflow-backend.md`
13. `context/feature-specs/34-matches-ui-foundation.md`
14. `context/feature-specs/35-lineup-appearances-ui.md`

Use relevant project-local skills from `.agents/skills/` when applicable.

Skills and plugins may guide implementation but must not override project context, architecture rules, code standards, or this spec.

Before creating custom UI primitives or interaction patterns:

- check `context/references/shadcn-components.md`;
- prefer suitable shadcn/ui components;
- install shadcn components just in time;
- compose app-level components outside `frontend/src/components/ui`;
- do not manually modify generated shadcn/ui primitive files.

This unit is primarily frontend work, with one narrowly scoped backend addition:

```txt
GET /api/matches/{matchId}/lineup/eligible-players
```

Do not change the existing atomic lineup read/save semantics from Unit 31.

Do not add unrelated backend models, mutations, migrations, or workflow changes.

### Why the eligible-player query is required

The Unit 28 player list uses current team assignments for normal player browsing and team-scope visibility.

That query is not sufficient for lineup selection because a match may be:

- historical;
- scheduled in the future;
- played after a player changed selections;
- associated with a player whose current assignment differs from the assignment that covered the match date.

The frontend must not infer historical eligibility from current assignments or fetch every player's full assignment history to reproduce backend rules.

The backend therefore must expose the authoritative set of players who may be newly added to this match lineup according to:

- the match's immutable `TeamId`;
- the match kickoff date;
- player archive state;
- assignment ranges;
- caller role and team scope.

This endpoint is a support query for the Unit 35 UI, not a new player-management feature.

### Scope

This unit introduces:

- real `Sastav` tab content on `/matches/:matchId`;
- lineup view mode;
- lineup edit mode;
- optional formation;
- starters;
- substitutes/bench;
- starting captain;
- played-match appearances;
- manual minutes played;
- ordered substitution events;
- client-side snapshot validation for usability;
- atomic save through `PUT /api/matches/{matchId}/lineup`;
- authoritative backend validation and conflict handling;
- workflow-lock awareness;
- loading, empty, error, read-only, and save states;
- the minimal eligible-player backend query.

This unit does not introduce:

- tactical positions;
- pitch coordinates;
- shirt numbers;
- a visual football-pitch formation editor;
- a fixed formation catalog;
- a hardcoded requirement for exactly 11 starters;
- a hardcoded substitute limit;
- statistics;
- goalkeeper designation or goalkeeper statistics;
- report submit/review/verify actions;
- GPS;
- media;
- imports;
- audit history;
- automatic minute calculation;
- automatic lineup generation.

### Existing aggregate contract remains authoritative

Continue using:

```txt
GET /api/matches/{matchId}/lineup
PUT /api/matches/{matchId}/lineup
```

The frontend edits one complete logical snapshot.

Do not introduce child-level mutation endpoints for:

- starters;
- substitutes;
- captain;
- appearances;
- minutes;
- substitution events.

The UI may split the experience into sections, but save must remain atomic.

The frontend must not generate database child IDs.

Stable `PlayerMatchAppearance` identifiers remain backend-owned and must be preserved by Unit 31 persistence behavior when the same player remains in the saved appearance set.

### Minimal eligible-player backend endpoint

Add:

```txt
GET /api/matches/{matchId}/lineup/eligible-players
```

Authorization:

- authenticated active user required;
- `ADMIN` may query for any accessible match;
- `DATA_OPERATOR` may query only for matches inside authorized team scope;
- other roles do not need this editor-only candidate query;
- inaccessible match behavior follows existing safe non-disclosure conventions;
- archived or cancelled matches may return a workflow/lifecycle conflict rather than an editable candidate list.

Eligibility behavior:

- player exists;
- player is not archived;
- player has an assignment to the match's immutable `TeamId`;
- assignment covers the match date using the same date convention as Unit 31;
- assignment to another team alone does not qualify;
- multiple assignments remain valid;
- historical or future match eligibility is evaluated against the match date, not today's date.

Return a compact candidate read model containing at minimum:

- player ID;
- display name fields needed by the UI;
- preferred name when present;
- player lifecycle status;
- matching assignment summary for the match team/date;
- a stable sort value or deterministic server ordering.

Do not return medical data, notes, statistics, unrelated assignments, or restricted profile fields.

Existing lineup players must remain displayable from `GET /lineup` even if they would no longer qualify as a new addition because they were later archived or their assignments changed. The candidate query governs new additions; it must not cause historical saved references to disappear.

Add focused backend tests for:

- historical eligibility;
- future eligibility;
- current eligibility;
- assignment outside match team;
- assignment range boundaries;
- archived player exclusion for new additions;
- admin access;
- in-scope data-operator access;
- out-of-scope denial;
- read-only role denial;
- safe inaccessible match behavior.

No EF Core migration should be required for this endpoint.

### Match-status modes

The UI must adapt to Unit 31 match-status rules.

#### `SCHEDULED`

Allow authorized editors to save a preliminary lineup containing:

- formation;
- starters;
- substitutes;
- optional captain.

Do not show:

- appearance minutes;
- substitution events;
- concrete participation controls.

Clearly label the content as a preliminary lineup.

#### `POSTPONED`

Use the same preliminary-lineup behavior as `SCHEDULED`.

Show a visible postponed status context, but do not clear an existing preliminary lineup.

#### `PLAYED`

Allow authorized editors to manage:

- formation;
- starters;
- substitutes;
- required captain when starters exist;
- derived concrete appearance list;
- manual minutes for each appearance;
- ordered substitution events.

The appearance set must equal:

- all starters; plus
- every player who enters through at least one valid substitution.

Unused substitutes must not receive an appearance or minutes input.

A returning player still has one appearance and one minutes value.

#### `CANCELLED`

Show any previously saved preliminary lineup in read-only mode.

Do not offer edit/save actions.

Do not create appearances or substitutions.

#### Archived match

Show saved lineup/participation in read-only mode.

Do not offer edit/save actions.

### Report-workflow edit locks

Use Unit 32 workflow behavior.

For users who otherwise have lineup mutation permission:

- no report: editable according to match status;
- `DRAFT`: editable;
- `NEEDS_CORRECTION`: editable;
- `READY_FOR_REVIEW`: locked;
- `VERIFIED`: locked;
- `ARCHIVED`: locked.

When practical, query report workflow metadata for authorized editors so the UI can explain why editing is unavailable before a save attempt.

The backend remains authoritative.

If `PUT /lineup` returns a workflow-lock `409`:

- show a clear Bosnian Latin message;
- exit or disable stale edit mode;
- refetch lineup, match detail, and report workflow data;
- do not locally override the backend;
- do not add correction-request actions in this unit.

Suggested lock explanation:

```txt
Sastav je zaključan zbog trenutnog statusa izvještaja utakmice.
```

### Permission-aware UI behavior

Use session role/team-scope data only as a UI hint.

#### `ADMIN`

May edit any non-archived, non-cancelled, backend-editable match.

#### `DATA_OPERATOR`

May edit only matches inside authorized team scope when the backend permits it.

#### Other roles

Read-only lineup/participation view.

Do not show edit controls to clearly read-only roles.

Still handle backend `401`, `403`, `404`, and `409` responses safely because permissions, assignments, match state, or report workflow may change after rendering.

### `Sastav` tab integration

Replace the Unit 34 placeholder in the existing match detail shell.

Do not create a second match detail page or duplicate the header/tabs.

The tab should have:

- compact section header;
- match status context;
- edit/read-only state;
- last loaded/saved state only if already supported by existing query patterns;
- formation summary;
- captain summary;
- starters section;
- substitutes section;
- appearance/minutes section for `PLAYED`;
- substitutions section for `PLAYED`.

Keep `Statistika`, `GPS / Fizički podaci`, `Video`, and `Revizija` placeholders unchanged.

### View mode

View mode must work for all authorized roles.

Show:

#### Formation

Display the saved free-text formation or a clear empty value.

Do not parse formation into tactical positions.

#### Starting lineup

Show starters in deterministic order.

The backend contract should preserve or expose a deterministic lineup order according to Unit 31 implementation. If the existing contract does not expose explicit order, use the backend-returned collection order and do not invent tactical positions.

Indicate the captain with:

- readable text;
- a suitable `Badge` or icon;
- not color alone.

#### Substitutes

Show named substitutes separately from starters.

Clearly distinguish unused substitutes from substitutes who entered.

#### Appearances and minutes

For `PLAYED` matches, show every concrete appearance with:

- player name;
- starter/substitute origin;
- minutes played;
- appearance ID only internally, not as user-facing copy.

Do not show unused substitutes in the appearance list.

#### Substitution events

For `PLAYED` matches, show events in ascending sequence.

Display:

- match minute;
- stoppage time when present;
- player out;
- player in.

Suggested display:

```txt
67'  Izašao: Igrač A · Ušao: Igrač B
90+2'  Izašao: Igrač C · Ušao: Igrač D
```

Use proper Bosnian Latin copy.

If there are no substitutions, show a concise empty state rather than an empty table shell.

### Editing surface

This is a dense operational workflow.

Use a responsive shadcn `Sheet` or a large `Dialog` based on the existing app pattern and available viewport behavior.

Prefer `Sheet` when it provides more usable vertical space for desktop/tablet editing.

The editing surface must:

- preserve the match detail context;
- support keyboard navigation;
- prevent accidental dismissal when unsaved changes exist, using an explicit confirmation pattern if needed;
- expose one primary save action;
- expose cancel/close;
- not auto-save every field change.

Do not build a full-screen custom modal from scratch when an appropriate shadcn primitive exists.

### Form architecture

Use React Hook Form with Zod.

Use `useFieldArray` or an equivalent standard React Hook Form pattern for dynamic collections.

The form represents:

- optional formation;
- starter player IDs;
- substitute player IDs;
- optional/required captain player ID depending on status;
- ordered substitution rows;
- derived appearance rows with minutes.

Keep one source of truth inside the form.

Do not duplicate the same mutable lineup state in Zustand or React Context.

Convert the form into the Unit 31 atomic request only on submit.

### Player selection

Use the eligible-player endpoint for new additions.

Recommended shadcn composition:

- `Combobox` for searchable player selection;
- `Command` inside `Popover` if that is the installed/approved implementation;
- `Item`, `Badge`, or `Card` for selected-player rows;
- `Button` and `Dropdown Menu` for role/move/remove actions.

Rules:

- a player may appear only once in the lineup;
- adding a starter removes that player from substitute availability;
- adding a substitute removes that player from starter availability;
- selected existing lineup players remain visible even when absent from the new-addition candidate query;
- archived/historically preserved players already in the lineup should display a clear non-editable warning badge when relevant;
- removing a player also removes dependent captain selection, substitution references, and derived appearance/minutes data after explicit confirmation when data would be lost;
- do not silently discard dependent data.

Do not fetch the full club player list and reproduce assignment eligibility in the browser.

### Starters and substitutes editing

Provide separate sections:

```txt
Početni sastav
Klupa
```

Allow:

- adding eligible players;
- moving a selected player between starter and substitute roles;
- removing a player;
- deterministic ordering using accessible move-up/move-down actions when order is represented by the backend contract.

Do not install a drag-and-drop package.

Do not enforce exactly 11 starters or a fixed bench size.

Show current counts as informational text only.

### Formation editing

Formation is optional free text.

Use shadcn `Input`.

Normalize user experience through normal trimming on submit.

Do not:

- parse formation;
- validate against a fixed list;
- draw a tactical pitch;
- assign player positions.

Suggested label:

```txt
Formacija
```

Suggested placeholder:

```txt
npr. 4-3-3
```

### Captain editing

Captain choices must come only from the current starter list.

For `SCHEDULED` and `POSTPONED`:

- captain is optional.

For `PLAYED`:

- captain is required when at least one starter exists.

Use shadcn `Select` or `Combobox`.

If the selected captain is moved to the bench or removed:

- clear the captain field;
- show validation feedback;
- do not silently select another captain.

### Substitution editor

Show only for `PLAYED`.

Represent substitutions as an ordered list.

Each row includes:

- player out;
- player in;
- match minute;
- optional stoppage-time minute;
- move up;
- move down;
- remove.

Do not ask the user to type the backend `sequence`.

Derive:

```txt
sequence = row index + 1
```

when constructing the save payload.

This guarantees positive unique sequences while preserving explicit event order.

Use eligible lineup members only:

- both players must already exist in the match lineup;
- player out and player in must differ.

The UI should calculate on-field state in row order to improve option choices:

- initial on-field set = starters;
- outgoing choices = players currently on field;
- incoming choices = lineup players currently off field;
- after each valid row, update the derived on-field set;
- return substitutions remain possible when a player is off field again.

When an earlier event changes and makes later events invalid:

- preserve the later row long enough to show a clear validation error, or clear only the invalid player reference with explicit feedback;
- do not silently rewrite the entire substitution history.

The backend remains authoritative for ordered-state validation.

Use integer inputs:

- minute must be non-negative;
- stoppage time is optional and non-negative;
- do not enforce a universal 90/120-minute maximum.

### Derived appearances and minutes

For `PLAYED`, derive the appearance player set from the current form:

```txt
all starters
+
every distinct player who enters through a substitution
```

Do not provide a manual “appeared” checkbox.

For each derived appearance, show:

- player;
- starter/substitute origin;
- manual `MinutesPlayed` input.

Rules:

- minutes are required for every derived appearance before save;
- integer;
- non-negative;
- zero allowed;
- do not calculate minutes from substitution events;
- do not enforce a fixed maximum;
- do not enforce a fixed total team-minute sum.

Preserve entered minutes while the player remains in the derived appearance set.

If a form change removes a player from the appearance set and they have entered minutes:

- require explicit confirmation before discarding those minutes;
- clear them only after confirmation.

Do not send appearance records for unused substitutes.

### Local validation

Use Zod and form-level validation to provide immediate feedback for obvious issues.

Validate at minimum:

- no duplicate lineup player;
- captain belongs to starters;
- played match with starters has a captain;
- substitution player out/in differ;
- substitution players belong to lineup;
- minute/stoppage/minutes-played values are non-negative integers;
- every derived appearance has minutes;
- no concrete participation data is sent for scheduled/postponed matches.

Where practical, validate ordered on-field substitution state in a reusable feature utility.

Do not attempt to duplicate all backend assignment or workflow rules in Zod.

Map backend ProblemDetails validation errors to:

- fields;
- substitution rows;
- form-level error summary.

Use the existing `FormErrorSummary` and form helper patterns.

### Unsaved-change protection

The atomic editor may contain substantial data.

When the form is dirty and the user attempts to close the sheet/dialog or navigate away:

- show an explicit confirmation using an approved shadcn confirmation primitive;
- explain that unsaved lineup changes will be lost;
- allow continue editing or discard changes.

Do not add a global navigation-blocking framework if the existing router can support a focused feature-level guard.

If browser-level navigation cannot be safely intercepted with the existing router, at minimum protect sheet/dialog dismissal and document the limitation in `context/progress-tracker.md`.

### Query and mutation behavior

Use TanStack Query.

Define/reuse stable query keys for:

- match detail;
- lineup;
- eligible players;
- report workflow metadata when queried for editor lock state.

On successful save:

- close edit mode;
- show Bosnian success feedback through the approved toast system;
- invalidate/refetch lineup;
- invalidate match detail only if the existing contract includes derived lineup state there;
- invalidate report readiness/detail data when necessary because appearance completeness may affect later submit readiness;
- avoid broad cache clearing.

On error:

- keep unsaved form values;
- show mapped validation/conflict feedback;
- refetch authoritative state after authorization/workflow conflicts;
- do not optimistically replace the saved aggregate before backend success.

### Shadcn-first component selection

Before implementation, review the new interactions against the component catalog.

Prefer suitable existing components such as:

- `Sheet` or `Dialog`;
- `Alert Dialog`;
- `Combobox`;
- `Command`;
- `Popover`;
- `Input`;
- `Field`;
- `Select`;
- `Badge`;
- `Item`;
- `Card`;
- `Separator`;
- `Scroll Area`;
- `Skeleton`;
- `Empty`;
- `Alert`;
- `Sonner`;
- `Tooltip`;
- `Dropdown Menu`;
- `Button Group` where appropriate.

Do not install or use components merely because they exist.

Use only components that improve the required workflow.

### Frontend organization

Keep lineup UI code inside the Matches feature because lineup/participation belongs to a match workflow.

Suggested organization:

```txt
frontend/src/features/matches/
├── api/
├── components/
│   └── lineup/
├── hooks/
├── schemas/
├── types/
├── utils/
└── index.ts
```

Potential app-level components should be extracted only when a real repeated pattern already exists.

Do not create a separate global lineup store.

Use explicit TypeScript interfaces aligned with backend contracts.

Avoid `any`.

### Localization

Visible UI copy is Bosnian Latin with proper characters.

Suggested labels include:

```txt
Sastav
Uredi sastav
Početni sastav
Klupa
Kapiten
Formacija
Izmjene
Minute
Izašao
Ušao
Sačuvaj sastav
Odustani
Nema unesenog sastava
Preliminarni sastav
Sastav je zaključan
```

Internal enum/API values remain English.

Do not display raw `STARTER` or `SUBSTITUTE` values.

Suggested display labels:

```txt
STARTER -> Starter
SUBSTITUTE -> Zamjena
```

Use established project terminology consistently. If “Početni sastav” is used in one place, do not mix it arbitrarily with unrelated synonyms across the same screen.

### Responsive and accessibility behavior

Desktop/tablet are primary for dense lineup editing.

Requirements:

- mobile remains usable for viewing and lightweight corrections;
- editor uses contained scrolling;
- selected-player rows remain keyboard reachable;
- add/remove/move controls have accessible names;
- icon-only controls have tooltips or `aria-label`;
- Combobox and Select preserve keyboard navigation;
- field errors are associated with controls;
- substitution rows remain understandable without relying only on visual position;
- move-up/move-down buttons announce event order;
- focus moves to newly added rows when practical;
- focus returns appropriately after confirmation dialogs;
- status/role badges include text;
- horizontal overflow is contained.

Do not build an inaccessible drag-only interaction.

### Tests

Add backend tests for the eligible-player endpoint as specified.

Add frontend tests only if the repository already has an approved frontend testing foundation by implementation time.

At minimum, manually verify:

- scheduled preliminary lineup;
- postponed preliminary lineup;
- played lineup;
- cancelled read-only behavior;
- archived read-only behavior;
- admin editor access;
- in-scope data-operator editor access;
- read-only role behavior;
- historical/future eligible-player selection;
- duplicate prevention;
- captain behavior;
- substitution ordering;
- return substitution scenario;
- appearance derivation;
- minutes validation;
- unsaved-change confirmation;
- workflow lock response;
- backend validation mapping;
- responsive and keyboard behavior.

Do not introduce a new frontend testing framework solely for this unit.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

If the eligible-player API, atomic lineup contract, match-status rules, report workflow locks, or UI interaction model must differ from this spec, update the relevant context/spec before continuing.

Do not silently introduce tactical positions, lineup limits, goalkeeper roles, statistics, or report actions.

## Implementation

### 1. Add the eligible-player backend query

Create:

```txt
GET /api/matches/{matchId}/lineup/eligible-players
```

Implement:

- match lookup;
- editor authorization;
- team-scope enforcement;
- match-date assignment filtering;
- archived-player exclusion for new additions;
- compact deterministic candidate read model;
- safe errors;
- focused backend tests.

Keep endpoint handlers thin.

Do not add a migration.

### 2. Add typed lineup frontend contracts and queries

Align frontend types with:

- `GET /lineup`;
- `PUT /lineup`;
- eligible-player query;
- report lock metadata used by the editor.

Add TanStack Query hooks and stable keys.

### 3. Replace the `Sastav` placeholder

Build real read mode inside the existing Unit 34 match detail shell.

Show:

- formation;
- captain;
- starters;
- substitutes;
- appearances/minutes for played matches;
- substitutions;
- empty/read-only/locked states.

### 4. Build the atomic lineup editor

Use shadcn `Sheet` or large `Dialog`, React Hook Form, Zod, and dynamic field arrays.

Implement:

- formation;
- starters;
- substitutes;
- captain;
- substitutions;
- derived appearances;
- minutes;
- save/cancel;
- unsaved-change protection.

### 5. Implement player-selection behavior

Use the eligible-player query and shadcn Combobox/Command patterns.

Prevent duplicates and preserve already-saved historical references.

Handle dependent-data confirmation when removing players.

### 6. Implement ordered substitutions

Build accessible ordered rows.

Derive sequence from row order.

Calculate current on-field state for client feedback while preserving backend authority.

Support return substitutions.

### 7. Implement derived appearances and minutes

Derive appearances from starters and substitution entries.

Require manual non-negative minutes for each appearance.

Preserve stable frontend form values by player ID and do not generate backend appearance IDs.

### 8. Wire atomic save and error handling

Convert form state into one complete Unit 31 request.

On success:

- show feedback;
- close editor;
- invalidate focused queries.

On failure:

- preserve form state;
- map validation;
- handle workflow/auth conflicts;
- refetch authoritative state when needed.

### 9. Verify shadcn-first implementation

Review all new primitives against:

```txt
context/references/shadcn-components.md
```

Remove unnecessary hand-built low-level controls.

Do not modify generated shadcn primitive files.

### 10. Update progress documentation

Update `context/progress-tracker.md` with the actual implementation and verification state.

Do not mark Unit 35 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use existing backend and frontend packages introduced by prior units.

Add required shadcn components just in time through the approved CLI workflow.

Do not add:

- drag-and-drop packages;
- a second form library;
- a second query/state library;
- a tactical-pitch library;
- a date-range/eligibility library;
- a new frontend testing framework solely for this unit.

## Verification checklist

- [ ] `GET /api/matches/{matchId}/lineup/eligible-players` exists.
- [ ] Eligible-player results use the match team and match date rather than current assignment only.
- [ ] Historical, current, and future assignment eligibility is tested.
- [ ] Assignment to another team alone does not qualify a player.
- [ ] Archived players are excluded from new additions.
- [ ] Existing historical lineup players remain readable even if later archived or no longer eligible for new addition.
- [ ] Eligible-player query enforces admin/data-operator role and team scope.
- [ ] No backend migration is introduced for the eligible-player query.
- [ ] The Unit 31 atomic `GET`/`PUT` lineup contract remains intact.
- [ ] The Unit 34 `Sastav` placeholder is replaced without duplicating the match detail shell.
- [ ] Scheduled matches support preliminary formation, starters, substitutes, and optional captain only.
- [ ] Postponed matches preserve the same preliminary-lineup behavior.
- [ ] Played matches support captain, appearances, minutes, and ordered substitutions.
- [ ] Cancelled matches are read-only.
- [ ] Archived matches are read-only.
- [ ] Report statuses `READY_FOR_REVIEW`, `VERIFIED`, and `ARCHIVED` lock editing.
- [ ] `DRAFT`, `NEEDS_CORRECTION`, and no-report states remain editable when other rules permit.
- [ ] Admin and authorized data operators see edit controls.
- [ ] Other roles see read-only content.
- [ ] Player selection uses the authoritative eligible-player endpoint.
- [ ] No duplicate player can exist across starters and substitutes.
- [ ] No fixed 11-player or substitute-count rule is hardcoded.
- [ ] Formation remains optional free text.
- [ ] No tactical positions or pitch editor are introduced.
- [ ] Captain choices come only from starters.
- [ ] Played lineup with starters requires a captain.
- [ ] Substitution sequence is derived from accessible row order.
- [ ] Player out/in must differ and belong to the lineup.
- [ ] Client-side ordered on-field validation supports return substitutions.
- [ ] Appearance set is derived from starters plus distinct entering players.
- [ ] Unused substitutes do not receive appearances.
- [ ] Returning players receive only one appearance/minutes record.
- [ ] Minutes are manual non-negative integers and zero is allowed.
- [ ] Minutes are not automatically derived from substitution events.
- [ ] Removing players with dependent data requires explicit confirmation.
- [ ] Dirty editor dismissal requires explicit discard confirmation.
- [ ] One atomic save request is used.
- [ ] The frontend does not generate persistence IDs for appearances.
- [ ] Backend validation errors map to fields/rows or a form summary.
- [ ] Workflow-lock conflicts preserve safety and refetch authoritative state.
- [ ] New UI follows shadcn-first component-selection guidance.
- [ ] Generated `frontend/src/components/ui/*` files are not manually modified.
- [ ] No raw Tailwind palette classes or hardcoded component colors are introduced.
- [ ] No ad-hoc visual overrides are applied to shadcn components.
- [ ] Frontend imports use `@/` instead of deep relative paths.
- [ ] Visible Bosnian copy uses proper Bosnian Latin characters.
- [ ] Keyboard navigation, labels, focus handling, and semantic controls are preserved.
- [ ] Mobile/tablet layouts remain usable.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes.
- [ ] Relevant backend tests pass.
- [ ] `npm run format` completes successfully.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] Frontend typecheck/build passes using the configured project command.
- [ ] Statistics, report actions, GPS, media, imports, audit UI, and unrelated backend changes are not added.
- [ ] `context/progress-tracker.md` reflects the actual Unit 35 implementation and verification state.
