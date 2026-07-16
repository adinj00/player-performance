# Unit 46: Gpexe Sample Review and Mapping Gate

## Goal

Define and enforce the evidence gate required before any Gpexe-specific import mapping or processor is implemented. Produce a reproducible review method, required evidence inventory, mapping-document structure, unblock criteria, privacy rules, destination-model checks, and implementation handoff requirements. Keep the Unit 45 generic CSV/XLSX raw preview as the only active Gpexe behavior until real Gpexe exports or authoritative documentation prove the file structure, row grain, identifiers, units, null semantics, and available metrics.

Current state:

```txt
BLOCKED
```

Blocking reason:

```txt
No real Gpexe CSV/XLSX export sample or authoritative field specification is present in the project context.
```

This unit is documentation and review-gate work only in its current state.

Do not implement a Gpexe exact processor, mapping, validation, confirmation, database model, migration, or official-data mutation from assumptions.

## Design

### Required reading

Before performing the sample review, read:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/code-standards.md`
5. `context/ai-workflow-rules.md`
6. `context/progress-tracker.md`
7. `context/feature-specs/00-build-plan.md`
8. `context/feature-specs/43-import-workflow-backend-foundation.md`
9. `context/feature-specs/44-import-ui-foundation.md`
10. `context/feature-specs/45-generic-csv-xlsx-parsing-foundation.md`
11. `context/feature-specs/46-gpexe-sample-review-mapping-gate.md`

The project rules remain authoritative:

- do not guess Gpexe export columns;
- do not treat possible metrics listed in architecture as confirmed export fields;
- do not infer units from column names alone;
- do not infer row grain from one visible row;
- do not enable validation or confirmation without a documented exact processor contract;
- do not mutate official data without a confirmed destination model and provenance rules.

### Scope

This gate defines:

- what Gpexe evidence must be obtained;
- how real samples are handled securely;
- how the generic Unit 45 preview is used during review;
- how export structure is documented;
- how row grain and identity are proven;
- how units, nulls, zeroes, thresholds, timestamps, and duplicates are documented;
- how source columns are classified;
- how confirmed mappings are separated from unsupported or unknown fields;
- how a future exact Gpexe processor is versioned;
- which capabilities may be enabled after each evidence level;
- how the destination GPS/workload model is checked;
- what documentation must exist before coding begins;
- what tests and sanitized fixtures are required;
- how build-plan blocking is recorded and later removed.

This gate does not introduce:

- parser packages beyond Unit 45;
- Gpexe column mappings;
- Gpexe validation code;
- Gpexe confirmation code;
- official GPS/workload mutations;
- training sessions;
- match GPS records;
- new database tables;
- migration files;
- frontend changes;
- Gpexe API integration;
- credentials;
- scheduled synchronization;
- automatic export download;
- assumptions based on product marketing pages;
- guessed field names from screenshots or internet examples.

### Evidence hierarchy

Use evidence in this order:

1. Real export files generated from the club's actual Gpexe environment.
2. Official Gpexe export documentation matching the actual product/version/export workflow.
3. Written confirmation from an authorized Gpexe representative tied to the exact export.
4. Repeated real exports proving stability across sessions, players, and date ranges.
5. Generic Unit 45 preview observations.

Generic preview observations alone do not prove semantic meaning.

Third-party blog posts, screenshots, forum posts, public marketing pages, and unrelated clubs' exports may help form questions, but they must not be recorded as confirmed mapping evidence.

### Minimum evidence required to begin review

Obtain at least one real export for each workflow intended to be supported:

```txt
MATCH_GPS
TRAINING_GPS
```

If Gpexe provides different export templates or layouts, obtain a sample for each intended template.

For every sample, record:

```txt
export date
Gpexe product/module when known
export screen/report name
selected session/match/training context
selected date range
selected team/squad
selected players
selected filters
selected units/settings
file format
original filename
whether headers were customized
whether thresholds were customized
whether the file was edited after export
```

One sample may reveal a format but is not enough to claim long-term stability.

Before implementation, obtain repeated evidence sufficient to compare:

- more than one player;
- more than one session;
- a case containing zero values;
- a case containing missing values where possible;
- a match export and training export when both are in scope;
- repeated exports using the same settings;
- a sample after any known Gpexe configuration change that could alter columns or units.

Do not put an artificial numeric sample count into code.

The reviewer must document why the available set is sufficient for each confirmed rule.

### Secure handling of real exports

Real player-performance exports may contain personal and sensitive internal data.

Rules:

- do not commit raw real exports to the Git repository;
- do not place them in public project artifacts;
- do not include real player names, device identifiers, account identifiers, or raw performance values in feature specs;
- store review originals only in an approved restricted location;
- preserve original files unchanged;
- calculate a checksum when the review process supports it so the reviewed source can be identified without publishing it;
- record only safe file metadata in project documentation;
- never paste complete real rows into source code, tests, logs, issues, or chat messages;
- do not send samples to external services without authorization;
- do not upload real files to public AI, conversion, or spreadsheet-preview services.

Testing fixtures must be synthetic or irreversibly sanitized.

Replacing player names alone is not necessarily sufficient anonymization if timestamps, device IDs, session names, or rare metric combinations remain identifiable.

### Review workspace

Use the existing Unit 43–45 import workflow for review where practical:

1. upload the original as:
   - source system `GPEXE`;
   - intended import type;
   - correct team/match context;
2. retain the original source securely;
3. run the Unit 45 generic preview;
4. inspect:
   - file format;
   - encoding;
   - delimiter or worksheet;
   - headers;
   - row counts;
   - representative value shapes;
   - generic detected types;
5. download the retained source only through authorized import access;
6. record findings in the required mapping document.

The generic preview must remain raw.

Do not change Unit 45 generic behavior to accommodate one Gpexe sample before the exact mapping has been approved.

### Required review document

Create:

```txt
context/references/gpexe-export-review.md
```

Only create the completed review document after real evidence is available.

Until then, the project may keep a clearly marked template containing no guessed values.

The completed review document must include:

```txt
Review status
Evidence inventory
Export procedure
Observed file structure
Row-grain determination
Identity fields
Session/context fields
Timestamp and timezone behavior
Units and thresholds
Missing/zero behavior
Duplicate behavior
Column mapping matrix
Unsupported/ignored columns
Open questions
Canonical destination mapping
Processor capability decision
Processor key/version
Sanitized fixture inventory
Approval record
```

Do not mark the review `CONFIRMED` while material questions remain unresolved.

### Review statuses

Use one of:

```txt
NOT_STARTED
EVIDENCE_INCOMPLETE
UNDER_REVIEW
BLOCKED_BY_VENDOR_QUESTION
CONFIRMED_FOR_PREVIEW
CONFIRMED_FOR_VALIDATION
CONFIRMED_FOR_CONFIRMATION
```

Meaning:

#### `NOT_STARTED`

No real sample has been reviewed.

#### `EVIDENCE_INCOMPLETE`

Some evidence exists, but it does not prove the required structure or semantics.

#### `UNDER_REVIEW`

Evidence is actively being compared and documented.

#### `BLOCKED_BY_VENDOR_QUESTION`

A material semantic, unit, identity, or export-setting question requires authoritative clarification.

#### `CONFIRMED_FOR_PREVIEW`

An exact Gpexe preview presenter/mapping can safely label confirmed columns, but domain validation and official mutation remain blocked.

#### `CONFIRMED_FOR_VALIDATION`

Required fields and semantic validation rules are confirmed, but official destination mutation is still blocked or not yet approved.

#### `CONFIRMED_FOR_CONFIRMATION`

Format, mapping, validation, target model, transaction, provenance, duplicate policy, and audit behavior are all confirmed.

Do not skip directly from raw evidence to confirmation capability.

### File-structure review

Document exact observed behavior for every supported export variant:

```txt
CSV or XLSX
encoding
BOM
delimiter
worksheet count
selected worksheet
leading blank rows
header row number
data start row
footer/summary rows
empty trailing rows
repeated headers
merged cells
formula cells
hidden rows/columns where relevant
number formatting
locale-dependent values
```

Confirm whether Unit 45 generic rules are compatible.

If Gpexe requires a different rule, document it for the future exact processor.

Examples of material differences:

- a preamble before headers;
- multiple non-empty worksheets;
- semicolon decimals;
- non-UTF-8 encoding;
- repeated section headers;
- totals/footer rows;
- separate player and session tables;
- custom export templates.

Do not modify the generic reader globally for a vendor-specific difference.

### Row-grain determination

The mapping cannot be implemented until the row grain is proven.

Determine whether one row represents:

```txt
one player per session
one player per half/period
one player per drill
one player per device segment
one player per interval
one team aggregate
one timestamp sample
another explicitly documented unit
```

Record:

- natural key candidate;
- whether multiple rows may legitimately belong to the same player/session;
- whether rows must be aggregated;
- whether rows are already aggregates;
- whether period rows overlap;
- whether team totals are mixed with player rows;
- whether staff/non-player devices appear;
- whether deleted/invalid device rows appear.

Do not sum rows until overlap and aggregation semantics are proven.

Do not interpret high-frequency tracking samples as session aggregates without an explicit transformation spec.

### Player identity review

Determine the exact fields available for matching a Gpexe row to a club player.

Potential evidence categories may include:

```txt
vendor athlete ID
device ID
player name
shirt number
team/squad
email
another stable identifier
```

These are questions, not assumed fields.

For every observed identity field, document:

- exact source header;
- stability across exports;
- uniqueness;
- whether it can change;
- whether it is personal/sensitive;
- whether formatting is normalized;
- whether duplicate players can share a display name;
- whether it refers to athlete, device, vest, or account;
- whether historical assignments affect interpretation.

Preferred matching must use a stable confirmed vendor identity when available.

Do not make player name the silent permanent key.

If only names are available:

- confirmation remains blocked until an explicit manual mapping workflow or approved deterministic matching policy exists;
- ambiguous matches must be validation errors;
- no fuzzy automatic matching.

### Team and target context

Confirm whether the file contains:

- team/squad identity;
- match/session identity;
- date;
- opponent;
- session type;
- training name;
- match home/away context;
- external session ID.

Even when context exists in the file, the `ImportJob.TeamId` and optional `MatchId` remain trusted application context.

Document how source context is cross-checked against the selected import target.

Do not silently switch the import team or match based on file values.

Mismatch must be:

- a blocking validation issue; or
- an explicitly approved warning when the source does not provide reliable context.

### Date, time, duration, and timezone review

Document exact semantics for every temporal field:

```txt
session date
session start
session end
duration
active duration
period duration
timestamp
timezone
UTC offset
locale/date format
decimal representation
```

Questions that must be answered:

- Is a timestamp local, UTC, or timezone-free?
- Which timezone was used during export?
- Does duration include pauses?
- Are values seconds, minutes, milliseconds, or time strings?
- Are period durations overlapping with total duration?
- Is stoppage time represented?
- Does midnight crossing occur?
- Are dates Excel serial values or formatted text?

Do not infer timezone from the user's current browser or server timezone.

Do not convert values before the source semantics are documented.

### Units and thresholds review

Every metric mapping must specify an exact source unit.

Possible architecture metrics remain unconfirmed until observed:

```txt
total distance
high-speed running distance
sprint distance
number of sprints
maximum speed
accelerations
decelerations
player load
session duration
```

For every observed metric, document:

```txt
exact source header
source data type
source unit
target canonical unit
conversion formula
rounding rule
precision
minimum/maximum plausible range
whether zero is valid
whether blank means missing
whether value is aggregate or interval
whether threshold configuration affects the value
```

Threshold-dependent metrics require additional evidence:

```txt
high-speed threshold
sprint threshold
acceleration threshold
deceleration threshold
individualized or team-wide thresholds
threshold unit
threshold version/date
```

Do not compare or aggregate threshold-based metrics across sessions unless threshold compatibility is defined.

Do not infer kilometers per hour versus meters per second from typical-looking values.

### Null, blank, zero, and sentinel values

For every column, document:

- blank-cell meaning;
- empty-string meaning;
- zero meaning;
- textual null values such as `N/A`, `-`, or vendor-specific markers;
- invalid sensor marker;
- unavailable metric marker;
- not-applicable marker;
- whether zero can be a real performance value;
- whether the export substitutes zero for missing data.

The future processor must preserve:

```txt
missing != zero
```

Do not normalize every blank or sentinel to zero.

Do not discard rows merely because one optional metric is missing.

### Decimal and locale rules

Confirm:

- decimal separator;
- thousands separator;
- percentage representation;
- unit suffixes inside cells;
- localized date strings;
- text casing;
- leading/trailing spaces;
- negative signs;
- scientific notation;
- Excel numeric formatting.

Record exact invariant conversions.

Do not use the server's current culture implicitly.

Do not remove commas/dots without a confirmed source-locale rule.

### Duplicate and re-import semantics

Before confirmation capability is implemented, define:

- natural uniqueness key;
- duplicate rows inside one file;
- repeated export of the same session;
- corrected/re-exported session;
- partial export versus full export;
- multiple devices for one player;
- multiple periods for one player;
- import of the same source file twice;
- whether existing official values are inserted, updated, replaced, skipped, or rejected.

The default safe policy is:

```txt
reject ambiguous duplicates
```

Do not implement last-write-wins without explicit approval.

Do not use filename as the uniqueness key.

A source checksum may detect identical files but does not define semantic duplicate rows.

### Mapping matrix

The completed review must contain one row for every observed source column.

Required matrix columns:

```txt
Exact source header
Normalized source header
Observed file variants
Observed generic type
Confirmed semantic meaning
Source unit
Canonical unit
Conversion
Row-grain role
Required/optional
Null/zero behavior
Validation rules
Canonical destination field
Import types
Preview display label
Confirmation behavior
Evidence references
Status
Notes
```

Mapping status values:

```txt
CONFIRMED
IGNORED_CONFIRMED
UNKNOWN
BLOCKED
```

#### `CONFIRMED`

Meaning, unit, and destination are proven.

#### `IGNORED_CONFIRMED`

The field is understood but intentionally not imported.

Document why.

#### `UNKNOWN`

Observed but not understood.

It may remain visible in raw preview but must not participate in validation or confirmation.

#### `BLOCKED`

A material question prevents safe use.

A future exact processor must fail safely if a required blocked field is needed.

Do not omit unknown source columns from the review.

### Canonical destination check

Gpexe source fields must map into an approved canonical GPS/workload model.

Before `CONFIRMED_FOR_CONFIRMATION`, document:

- destination aggregate/entity;
- match versus training ownership;
- relationship to `Player`;
- relationship to match/training participation;
- source provenance fields;
- canonical units;
- nullable fields;
- duplicate/upsert policy;
- archive/correction behavior;
- team scope;
- report/training workflow lock behavior;
- audit events;
- import result summary.

At the current build stage, the generic canonical GPS/workload model is planned for Unit 48.

Therefore:

- Gpexe sample review may proceed before Unit 48;
- an exact preview/validation processor may be specified when semantics are proven;
- official confirmation must remain disabled until the destination model exists and the Unit 46 implementation spec explicitly depends on it.

If match GPS and training GPS need different destination models, document them separately.

Do not force vendor columns directly into a wide vendor-specific database table merely to unblock confirmation.

### Exact processor contract after unblocking

After evidence is approved, replace this blocked gate with or append a new implementation spec defining an exact processor.

Expected exact processor key pattern:

```txt
gpexe-{import-type}-{file-format}
```

Use a deliberate semantic processor version.

The implementation spec must define:

```txt
supported ImportType
supported SourceSystem = GPEXE
supported FileFormat
preview capability
validation capability
confirmation capability
exact header aliases
required fields
optional fields
unknown-field behavior
row filtering
row grain
player matching
target-context checks
unit conversion
null handling
duplicate policy
official mutation
transaction boundary
provenance
result summary
audit events
tests
```

Do not use one generic `gpexe` processor for semantically different export layouts unless the review proves they share one contract.

Exact processor resolution must continue to override Unit 45 generic preview fallback.

### Capability activation rules

#### Preview capability

Unit 45 already provides raw generic preview.

An exact Gpexe preview may replace it only when:

- the file variant can be recognized deterministically;
- header/worksheet rules are documented;
- known columns can be safely labeled;
- unknown columns remain visible and non-destructive;
- parser failures have safe codes;
- sanitized fixtures exist.

#### Validation capability

Enable only when:

- required fields are confirmed;
- identity matching is defined;
- units and conversions are confirmed;
- target-context checks are defined;
- null/zero rules are confirmed;
- duplicate rules are defined;
- every blocking issue has a stable validation code;
- unknown required semantics do not remain.

#### Confirmation capability

Enable only when:

- validation capability is approved;
- canonical destination model exists;
- official mutation behavior is specified;
- full transaction behavior is tested;
- provenance is persisted;
- audit events are specified;
- re-import/upsert behavior is approved;
- result summary is bounded;
- report/training workflow locks are enforced;
- confirmation rollback tests pass.

Do not activate all three stages merely because one sample parsed successfully.

### Unknown columns

Future exact Gpexe processors must not fail simply because an optional unknown column appears, unless the file variant can no longer be recognized safely.

Policy:

- preserve unknown columns in raw preview;
- classify them as `UNKNOWN`;
- do not map them;
- do not silently treat them as known;
- optionally emit a bounded warning;
- do not include raw values in logs/audit;
- document newly observed columns for review;
- increment processor version only when behavior changes.

Missing required known columns must be a blocking validation or format error.

### Sanitized test fixtures

After real evidence is available, create minimal synthetic fixtures that reproduce structure without real data.

Include, where applicable:

```txt
valid minimal CSV
valid minimal XLSX
valid multi-player sample
zero-value sample
missing-value sample
duplicate identity sample
ambiguous player sample
wrong team/session context
unknown optional column
missing required column
invalid unit/value
duplicate source row
corrected re-export scenario
```

Rules:

- no real names;
- no real vendor/account/device identifiers;
- no real session titles;
- no real metric values copied wholesale;
- fixtures remain small;
- fixture provenance is documented as synthetic;
- expected mappings and validations are explicit.

Do not use a raw club export as a committed integration fixture.

### Validation-code catalogue

The completed mapping review must define stable codes for every approved error/warning.

Expected categories:

```txt
unrecognized export variant
missing required column
ambiguous duplicate header
unsupported unit
invalid numeric value
missing player identity
ambiguous player match
unknown player
team mismatch
match/session mismatch
duplicate source row
duplicate existing import
missing required metric
out-of-range value
threshold incompatibility
```

Do not finalize exact code names until the corresponding rules are confirmed.

Messages must be safe and must not contain complete source rows.

### Audit and provenance requirements

A future confirmed Gpexe import must preserve:

```txt
ImportJobId
SourceSystem = GPEXE
processor key
processor version
imported by
imported at
target player
target match/training session
source identity reference where safe
canonical values
validation result
```

Audit metadata may include:

```txt
confirmed metric count
inserted/updated/skipped row counts
match/training target IDs
processor key/version
```

Do not include:

- full source rows;
- raw file content;
- device credentials;
- storage key/path;
- sensitive vendor tokens;
- complete high-frequency tracking series unless a future domain explicitly requires it.

### Documentation approval

The completed review must identify:

```txt
reviewed by
review date
evidence set identifier
open questions
approved import types
approved formats
approved capabilities
destination dependency
```

Approval means the evidence supports the documented behavior.

It does not mean every observed Gpexe field must be imported.

A deliberately small confirmed mapping is preferable to a broad guessed mapping.

### Build-plan behavior while blocked

Unit 46 and Unit 47 are optional blocked branches in the main implementation sequence.

Their blocked status must not stop independent work that does not depend on vendor mappings.

After this gate document is added:

- keep Unit 46 marked blocked;
- keep Unit 47 marked blocked;
- continue to Unit 48;
- revisit Unit 46 when real Gpexe evidence is available;
- update Unit 46 dependencies when the confirmed destination model is known;
- do not mark Unit 46 implementation complete.

`context/progress-tracker.md` must keep the Gpexe sample requirement as an open question.

### No package or schema changes while blocked

While the gate remains blocked:

- add no package;
- add no Gpexe configuration;
- add no Gpexe database field;
- add no migration;
- add no exact processor registration;
- add no frontend mapping labels;
- add no vendor-specific validation;
- add no confirmation capability.

Unit 45 generic preview remains the only Gpexe-capable code path.

## Implementation

### 1. Record the blocked state

Update `context/progress-tracker.md` to state:

```txt
Unit 46 mapping implementation is blocked until real Gpexe export samples or authoritative matching documentation are reviewed.
```

Preserve the existing open question.

Do not mark implementation started.

### 2. Add the review template

Create a template at:

```txt
context/references/gpexe-export-review-template.md
```

The template should contain the required evidence inventory, structure review, row grain, identity, units, nulls, duplicates, mapping matrix, destination, capability, fixture, and approval sections.

The template must be visibly marked:

```txt
TEMPLATE — NOT CONFIRMED MAPPING
```

Do not populate guessed column names.

### 3. Obtain authorized real evidence

The project owner must provide:

- real restricted export samples; or
- authoritative documentation matching the actual export.

Record safe metadata only.

Do not commit the originals.

### 4. Run generic previews

Use Unit 43–45 to retain and preview each sample.

Record structural findings.

Do not alter the generic reader based on one sample.

### 5. Complete the review document

Create:

```txt
context/references/gpexe-export-review.md
```

Populate only evidence-backed findings.

Keep unresolved rows/statuses explicit.

### 6. Confirm destination-model dependencies

Compare confirmed metrics against the canonical GPS/workload model available at that time.

If the model is not yet implemented, stop before confirmation capability.

Update the future implementation dependencies.

### 7. Write the exact implementation spec

Only after the appropriate review status is reached, create or revise the Unit 46 implementation spec with exact:

- mappings;
- validation;
- capability stages;
- destination mutations;
- tests;
- audit/provenance;
- migration impact.

Do not implement directly from the review notes without a scoped approved spec.

### 8. Implement only approved capabilities

Possible safe staged delivery:

1. exact preview;
2. exact validation;
3. exact confirmation after destination readiness.

Do not enable unavailable stages.

### 9. Update project documentation

After confirmation, update:

- `context/architecture.md`;
- `context/project-overview.md`;
- `context/progress-tracker.md`;
- `context/feature-specs/00-build-plan.md`;
- the Unit 46 implementation spec.

Replace “possible” metric language only with confirmed facts.

## Dependencies

No new software dependency is added by this blocked gate.

Existing review tools:

- Unit 43 import job/source retention;
- Unit 44 import workflow UI;
- Unit 45 generic CSV/XLSX preview.

Implementation dependencies remain unresolved until evidence proves the target.

At minimum:

```txt
Unit 45
real Gpexe export evidence
```

If confirmation targets the canonical training/match GPS model, the implementation must also depend on the unit that owns that destination, currently expected to include Unit 48.

Do not add vendor SDKs or API clients without a separate confirmed requirement.

## Verification checklist

- [ ] Unit 46 remains explicitly marked blocked.
- [ ] No claim is made that current project files contain confirmed Gpexe columns.
- [ ] No Gpexe field name is invented in this spec.
- [ ] Possible architecture metrics are not treated as confirmed export fields.
- [ ] `context/progress-tracker.md` retains the real-sample open question.
- [ ] A clearly marked Gpexe review template is created.
- [ ] The template contains no guessed mapping rows.
- [ ] Raw real exports are not committed to Git.
- [ ] Real samples are handled in an approved restricted location.
- [ ] Real player names, device IDs, session titles, and raw performance rows are not copied into specs/tests.
- [ ] Generic Unit 45 preview is used as structural evidence only.
- [ ] Marketing pages, screenshots, or unrelated public exports are not treated as authoritative mapping proof.
- [ ] Evidence inventory records export procedure and settings.
- [ ] File format, encoding, delimiter/worksheet, header, and data-start behavior are documented.
- [ ] Row grain is proven before aggregation/mapping.
- [ ] Player identity fields and stability are documented.
- [ ] Name-only matching does not silently become the permanent key.
- [ ] Team/match/training context cross-check behavior is documented.
- [ ] Date/time/timezone semantics are documented.
- [ ] Every mapped metric has a confirmed unit and conversion.
- [ ] Threshold-dependent metrics document threshold configuration.
- [ ] Null, blank, zero, and sentinel behavior is documented.
- [ ] Decimal/locale behavior is documented.
- [ ] Duplicate and re-import policy is documented.
- [ ] Every observed source column appears in the mapping matrix.
- [ ] Each column is classified as `CONFIRMED`, `IGNORED_CONFIRMED`, `UNKNOWN`, or `BLOCKED`.
- [ ] Unknown columns are not silently mapped.
- [ ] Destination aggregate/entity and canonical units are documented before confirmation.
- [ ] Confirmation remains disabled until the destination model exists.
- [ ] Exact processor key/version and supported combination are defined before coding.
- [ ] Exact processor overrides generic fallback only for the confirmed combination.
- [ ] Preview, validation, and confirmation capability gates are assessed independently.
- [ ] One successful parse does not automatically enable validation/confirmation.
- [ ] Sanitized committed fixtures contain no real sensitive data.
- [ ] Validation codes are evidence-backed and stable.
- [ ] Provenance and audit requirements are documented.
- [ ] Review approval identifies evidence, reviewer, date, capabilities, and open questions.
- [ ] No package, Gpexe configuration, schema field, migration, processor, or frontend mapping is added while blocked.
- [ ] Unit 45 generic preview remains the only active Gpexe behavior.
- [ ] Unit 47 remains independently blocked.
- [ ] The build plan continues to Unit 48 while vendor mapping gates are blocked.
- [ ] Unit 46 is not marked implementation-complete.
