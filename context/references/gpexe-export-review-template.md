# Gpexe Export Review

> **TEMPLATE — NOT CONFIRMED MAPPING**
>
> Do not populate this file from assumptions, public screenshots, marketing pages, or unrelated exports.
> Real restricted export evidence or authoritative documentation matching the club's export is required.

## Review status

```txt
NOT_STARTED
```

Allowed statuses:

```txt
NOT_STARTED
EVIDENCE_INCOMPLETE
UNDER_REVIEW
BLOCKED_BY_VENDOR_QUESTION
CONFIRMED_FOR_PREVIEW
CONFIRMED_FOR_VALIDATION
CONFIRMED_FOR_CONFIRMATION
```

## Evidence inventory

For each evidence item record only safe metadata:

| Evidence ID | Type | Export date | Product/module | Export/report name | Format | Workflow | Settings known | Restricted location/checksum reference | Notes |
|---|---|---|---|---|---|---|---|---|---|

Do not put raw player data or storage paths in this document.

## Export procedure

Document the exact steps used to create the export:

- selected team/squad;
- selected players;
- selected session/match/training;
- date range;
- report/template;
- filters;
- unit settings;
- threshold settings;
- customized columns;
- post-export edits.

## Observed file structure

### CSV

- encoding:
- BOM:
- delimiter:
- leading blank rows:
- header row:
- data start row:
- repeated headers:
- footer/summary rows:
- locale behavior:

### XLSX

- worksheet count:
- non-empty worksheets:
- selected worksheet:
- leading blank rows:
- header row:
- data start row:
- formulas:
- merged/hidden content:
- footer/summary rows:

## Row-grain determination

One row represents:

```txt
UNCONFIRMED
```

Evidence:

- natural key:
- overlapping periods:
- aggregation required:
- team aggregates mixed with player rows:
- device-segment behavior:
- duplicate row behavior:

## Player identity

| Exact header | Meaning | Athlete/device/account | Stable | Unique | Can change | Sensitive | Matching decision | Evidence |
|---|---|---|---|---|---|---|---|---|

## Team and target context

- team/squad field:
- session/match identifier:
- date:
- opponent:
- session type:
- external session ID:
- application target cross-check:
- mismatch severity:

## Date, time, duration, and timezone

| Exact header | Meaning | Format | Timezone | Unit | Includes pauses | Conversion | Evidence | Status |
|---|---|---|---|---|---|---|---|---|

## Units and thresholds

| Exact header | Meaning | Source unit | Canonical unit | Conversion | Precision | Threshold-dependent | Threshold evidence | Status |
|---|---|---|---|---|---|---|---|---|

## Null, blank, zero, and sentinel values

| Exact header | Blank | Zero | Sentinel values | Missing vs not applicable | Evidence | Status |
|---|---|---|---|---|---|---|

## Duplicate and re-import semantics

- source-row natural key:
- duplicate rows inside one file:
- repeated export:
- corrected export:
- partial versus full export:
- insert/update/replace/skip/reject:
- identical source checksum behavior:
- ambiguous duplicate policy:

## Mapping matrix

| Exact source header | Normalized source header | Variants | Generic type | Confirmed meaning | Source unit | Canonical unit | Conversion | Row-grain role | Required | Null/zero | Validation | Destination field | Import type | Preview label | Confirmation behavior | Evidence | Status | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|

Allowed mapping status:

```txt
CONFIRMED
IGNORED_CONFIRMED
UNKNOWN
BLOCKED
```

## Unsupported and ignored columns

Document understood but intentionally unsupported fields and the reason.

## Open questions

| Question | Why it matters | Required evidence/owner | Blocking capability |
|---|---|---|---|

## Canonical destination mapping

### Match GPS

- destination aggregate:
- player relationship:
- match relationship:
- canonical units:
- duplicate policy:
- workflow lock:
- provenance:
- audit:
- result summary:

### Training GPS

- destination aggregate:
- player relationship:
- training-session relationship:
- canonical units:
- duplicate policy:
- workflow lock:
- provenance:
- audit:
- result summary:

## Processor capability decision

- processor key:
- processor version:
- import type:
- source system:
- file format:
- exact variant recognition:
- can preview:
- can validate:
- can confirm:
- remaining destination dependency:

## Sanitized fixture inventory

| Fixture | Synthetic scenario | Expected parser behavior | Expected validation | Contains no real data |
|---|---|---|---|---|

## Approval record

- reviewed by:
- review date:
- evidence set:
- approved formats:
- approved import types:
- approved capabilities:
- open questions:
- destination dependency:
- approval notes:
