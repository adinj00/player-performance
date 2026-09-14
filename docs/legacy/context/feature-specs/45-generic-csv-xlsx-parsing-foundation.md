# Unit 45: Generic CSV/XLSX Parsing Foundation

## Goal

Add the real generic tabular-reading foundation behind the Unit 43 import workflow. Introduce mature CSV and XLSX reader packages, provider-neutral tabular reader contracts, safe format verification, bounded streaming row processing, deterministic header normalization, conservative type detection, persisted preview metadata, and a generic raw-preview fallback processor for every accepted import/source combination. Do not add vendor-specific mappings, domain validation, confirmation capability, official data mutations, frontend parsing, or speculative Gpexe/Zone14 fields.

## Design

### Required reading and implementation boundaries

Before implementation, read:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/code-standards.md`
5. `context/ai-workflow-rules.md`
6. `context/progress-tracker.md`
7. `context/feature-specs/00-build-plan.md`
8. `context/feature-specs/40-file-storage-abstraction.md`
9. `context/feature-specs/43-import-workflow-backend-foundation.md`
10. `context/feature-specs/44-import-ui-foundation.md`
11. `context/feature-specs/45-generic-csv-xlsx-parsing-foundation.md`

Use relevant project-local skills from `.agents/skills/` when applicable.

Skills/plugins may guide implementation workflow but must not override project context, architecture rules, code standards, or this spec.

This unit is backend-only.

Do not add or change frontend routes, components, tables, dialogs, navigation, or browser-side file parsing.

The existing Unit 44 UI must become useful automatically through:

- updated import capabilities;
- backend `allowedActions`;
- the existing preview endpoint and preview read model.

### Scope

This unit introduces:

- the mature `CsvHelper` package for CSV reading;
- the mature `ExcelDataReader` base package for low-level XLSX reading;
- provider-neutral tabular reader contracts in Application;
- Infrastructure CSV and XLSX reader implementations;
- tabular reader resolution by stored `FileFormat`;
- a generic raw-preview import processor;
- deterministic exact-processor versus generic-fallback resolution;
- content-level CSV/XLSX format verification;
- safe CSV encoding and delimiter detection;
- safe XLSX ZIP/container preflight;
- bounded row, column, cell, worksheet, and decompression limits;
- forward-only row processing without retaining the complete dataset;
- deterministic header normalization and duplicate handling;
- conservative generic column-type detection;
- persistence of safe preview parser metadata;
- stable parser failure codes and safe failure messages;
- Unit 43 preview/audit integration;
- configuration, migration, and comprehensive tests.

This unit does not introduce:

- Gpexe mappings;
- Zone14 mappings;
- domain column mappings;
- generic column-mapping endpoints;
- generic column-mapping UI;
- player roster import confirmation;
- match-statistics import confirmation;
- GPS metric import confirmation;
- training GPS confirmation;
- `VALIDATE` capability for the generic fallback;
- `CONFIRM` capability for the generic fallback;
- official data mutations;
- automatic preview after upload;
- automatic validation;
- automatic confirmation;
- CSV/XLSX writing;
- `.xls`, `.xlsb`, or `.xlsm` support;
- formula evaluation;
- macro execution;
- browser-side parsing;
- background workers;
- a DataSet-based full-workbook loader;
- vendor field assumptions.

### Package selection

Add packages only to the Infrastructure project that implements the readers.

Use:

```txt
CsvHelper
ExcelDataReader
```

Use the current stable versions compatible with the repository's .NET target and pin them through the repository's normal NuGet package-management approach.

Do not add:

```txt
ExcelDataReader.DataSet
ClosedXML
EPPlus
NPOI
DocumentFormat.OpenXml
Sylvan.Data.Csv
a second CSV reader
a second XLSX reader
```

Rationale:

- CSV reading needs robust quoted-field, multiline-field, delimiter, and malformed-data handling;
- XLSX preview needs a low-level forward reader rather than a workbook editing object model;
- the DataSet extension would encourage loading complete workbooks into memory;
- writing and workbook manipulation are out of scope.

Do not expose package-specific types outside Infrastructure.

### Generic preview only

Unit 45 registers a generic fallback processor with:

```txt
CanGeneratePreview = true
CanValidate = false
CanConfirm = false
```

The generic processor exists to show the source tabular structure and raw values safely.

It does not decide whether values are valid for:

- players;
- matches;
- statistics;
- GPS metrics;
- training sessions;
- vendor-specific semantics.

A successful raw preview means only:

```txt
The source file could be read as a bounded tabular file.
```

It does not mean:

```txt
The file is ready to mutate official data.
```

Therefore:

- generic preview success returns the job to `UPLOADED`;
- generic preview never moves a job to `READY_TO_CONFIRM`;
- generic preview never makes `VALIDATE` available;
- generic preview never makes `CONFIRM` available;
- Unit 44 continues to show validation and confirmation as unavailable until an exact processor exists.

### Processor fallback resolution

Extend the Unit 43 processor registry so it can resolve:

1. an exact processor for:
   - import type;
   - source system;
   - file format;
2. otherwise, the generic preview fallback for the file format.

Exact processors always take precedence.

Conceptual resolution:

```txt
Exact Gpexe/Zone14/domain processor
    ↓ when absent
Generic CSV or XLSX preview fallback
    ↓ when absent
No processor capability
```

Rules:

- resolution is deterministic;
- duplicate exact registrations fail startup;
- duplicate generic fallback registrations for the same file format fail startup;
- an exact processor may reuse the generic tabular reader internally;
- an exact processor replaces the effective capability record for that combination;
- the registry must not merge two unrelated processors into an ambiguous workflow;
- current processor key/version in capabilities reflects the effective processor;
- future Unit 46/47 processors can override the generic fallback without changing import jobs or UI routes.

### Effective capability matrix

For every stable Unit 43 combination using `CSV` or `XLSX`, the generic fallback exposes:

```txt
canPreview = true
canValidate = false
canConfirm = false
```

This includes source systems:

```txt
GENERIC
GPEXE
ZONE14
OTHER
```

and all currently stable import types.

This does not claim vendor mapping support.

The capability response must make clear that the effective processor is a generic raw-preview processor.

Suggested keys:

```txt
generic-csv-preview
generic-xlsx-preview
```

Use stable explicit parser/processor versions such as a manually maintained semantic contract version.

Do not use the application assembly build timestamp or random deployment identifier as the processor version.

Changing parsing behavior that can change preview/validation interpretation must increment the relevant processor/reader version deliberately.

### Application tabular reader contracts

Add provider-neutral contracts equivalent to:

```txt
ITabularSourceReader
ITabularSourceReaderResolver
```

The exact names may follow repository conventions.

The contracts must not reference:

- `CsvReader`;
- `IExcelDataReader`;
- `DataSet`;
- filesystem paths;
- ASP.NET Core request types;
- storage provider clients;
- `IFormFile`.

Conceptual request:

```txt
TabularReadRequest
- FileFormat
- PreviewRowLimit
- MaxRows
- MaxColumns
- MaxCellLengthCharacters
- CancellationToken
```

Conceptual result:

```txt
TabularReadResult
- ReaderKey
- ReaderVersion
- DetectedFileFormat
- SourceDescriptor
- Columns
- TotalRowCount
- PreviewRows
- PreviewWasTruncated
- StructuralIssues
```

The implementation may use a row callback or an asynchronous row consumer so it never retains the full file in memory.

### Row-consumer contract

Prefer a single-pass bounded contract equivalent to:

```txt
ReadAsync(
    Stream source,
    TabularReadOptions options,
    Func<TabularRow, CancellationToken, ValueTask> onRow,
    CancellationToken cancellationToken)
```

The reader:

- discovers the header;
- emits normalized columns;
- processes rows sequentially;
- counts every accepted source row;
- sends rows to the consumer;
- allows the consumer to retain only the first preview rows;
- produces final type and source metadata;
- respects cancellation.

If the implementation requires a slightly different shape, preserve these guarantees.

Do not return an unbounded `List<TabularRow>` for the complete file.

### Tabular column model

A generic column contains at minimum:

```txt
Ordinal
SourceHeader
NormalizedHeader
DetectedDataType
```

Rules:

- ordinal is zero- or one-based consistently across the reader and mapping layer;
- persisted Unit 43 preview column ordinal remains deterministic;
- source header is the trimmed source-facing label;
- normalized header is a deterministic unique machine-facing key;
- detected type is a conservative presentation hint;
- source header is not trusted as a domain field;
- normalized header is not a vendor mapping.

### Header-row behavior

For both CSV and XLSX:

- ignore leading completely blank rows within the configured scan limit;
- use the first non-empty row as the header;
- the next row is the first data row;
- do not scan arbitrary textual preambles looking for a “better” header;
- do not use vendor keywords to choose a header;
- fail safely when no non-empty header row exists;
- retain the original source row number.

Add configuration:

```txt
Imports__HeaderScanRowLimit
```

Rules:

- positive and bounded;
- exceeding the limit without a header produces a safe structural failure;
- this is a generic safety limit, not a vendor rule.

Future exact processors may define a different header policy after samples are confirmed.

### Header normalization

Create one centralized deterministic normalization function.

Recommended behavior:

1. Unicode-normalize to Form C.
2. Trim surrounding whitespace.
3. Collapse internal whitespace to one space for `SourceHeader`.
4. Build a lower-invariant normalized key.
5. Convert runs of non-letter/digit characters to `_`.
6. Trim leading/trailing `_`.
7. If empty, use:
   - `column_1`;
   - `column_2`;
   - etc.
8. If duplicated, append:
   - `__2`;
   - `__3`;
   - etc.

Examples:

```txt
"Player Name" -> player_name
"Player-Name" -> player_name__2 when duplicated
"" -> column_3
```

Rules:

- normalization is deterministic;
- no localized Bosnian translation occurs;
- original source labels remain available;
- duplicates do not overwrite prior values;
- every preview row uses the same unique normalized keys;
- normalization does not claim semantic equality between unrelated vendor columns.

Add extensive tests.

### Generic detected data types

Use stable generic type values:

```txt
EMPTY
TEXT
BOOLEAN
INTEGER
DECIMAL
DATE
DATETIME
MIXED
```

These are presentation/diagnostic hints only.

They are not domain types and do not confirm mapping validity.

#### CSV type inference

CSV values are textual.

Infer conservatively:

- empty/whitespace-only values do not force a type;
- boolean only for unambiguous `true`/`false`, case-insensitive;
- integer only for invariant optional-sign digits;
- decimal only for invariant forms using `.` as decimal separator;
- date/datetime only for strict ISO-compatible formats;
- otherwise `TEXT`;
- incompatible non-empty values produce `MIXED`.

Do not guess:

- comma decimal separators;
- local date formats;
- thousands separators;
- shirt numbers;
- GPS units;
- percentages;
- durations;
- vendor booleans such as `Y/N` or `1/0`.

Vendor/domain processors may interpret them later.

#### XLSX type inference

Use reader-provided cell types where safe:

- null;
- boolean;
- numeric;
- date/time;
- text.

When a column contains incompatible non-empty types, use `MIXED`.

Do not evaluate formulas.

If the reader returns a stored/cached formula result, treat only the returned value as a cell value and do not execute the formula.

### Preview value representation

Persist preview values as safe JSON-native values or safe invariant strings according to one centralized policy.

Required guarantees:

- null remains `null`;
- numeric zero remains `0`;
- boolean remains boolean when confidently known;
- integers/decimals do not silently lose precision;
- dates/datetimes use an unambiguous invariant/ISO representation;
- strings remain escaped plain data;
- formulas and HTML are never executed;
- very long values are rejected before persistence;
- values are keyed by unique normalized headers.

A safe preferred policy is:

- preserve null and boolean;
- preserve numbers only when they can be represented without precision loss;
- otherwise persist invariant strings;
- persist dates/datetimes as ISO strings;
- expose detected type separately.

Do not use locale-dependent `ToString()` behavior.

### Row shape behavior

Rows must never silently lose source cells.

Rules:

- determine the effective column count from the header and observed rows;
- shorter rows are padded with null values;
- longer rows add deterministic synthetic columns such as `column_n`, up to the configured maximum;
- synthetic columns are reflected in the final persisted column list;
- all persisted preview row objects align with the final column list;
- do not overwrite duplicate columns;
- do not discard extra cells;
- rows above the configured maximum column count fail safely;
- source row numbers remain accurate.

Because final columns may expand after early preview rows were read, normalize buffered preview rows against the final column set before persistence.

### Structural issues

Define an Application-level structure equivalent to:

```txt
TabularStructuralIssue
- Severity
- Code
- SafeMessage
- SourceRowNumber
- ColumnOrdinal
- Metadata
```

Stable generic severities:

```txt
ERROR
WARNING
```

Structural issues are parser/reader output for future exact processors.

Examples:

```txt
BLANK_HEADER_REPLACED
DUPLICATE_HEADER_RENAMED
ROW_PADDED_WITH_NULLS
ROW_ADDED_SYNTHETIC_COLUMNS
MULTIPLE_VALUE_TYPES
```

Rules:

- structural issue codes are stable English;
- safe messages contain no source row contents;
- metadata is bounded;
- no stack trace;
- no full raw cell value;
- no storage path/key;
- no vendor interpretation.

Unit 45 does not expose generic workflow `VALIDATE`.

Therefore, structural warnings are not converted into a claim that the import is valid.

Fatal reader conditions fail the preview operation.

Non-fatal structural issue persistence/display may be deferred until an exact validator consumes the result; the reader contracts must still expose them for Units 46/47.

### Generic validation result contracts

Add reusable Application structures equivalent to:

```txt
TabularValidationResult
TabularValidationIssue
TabularValidationSummary
```

They must support:

- blocking errors;
- warnings;
- row/column references;
- stable codes;
- safe messages;
- bounded metadata;
- valid/invalid/warning counts.

These are contracts for future exact processors.

Do not register a generic `ValidateAsync` implementation merely because the structure exists.

A generic structural read cannot prove that:

- required player identifiers exist;
- a match is correct;
- units are correct;
- a vendor column means the assumed metric;
- duplicates should insert/update/skip;
- official data is safe to mutate.

### CSV reader behavior

Implement the CSV reader with `CsvHelper`.

Required behavior:

- read from a stream;
- leave stream ownership according to Unit 43/reader contract;
- support RFC-style quoted fields;
- support delimiters inside quoted fields;
- support line breaks inside quoted fields;
- support escaped quotes;
- respect cancellation where package APIs permit;
- count source rows accurately;
- do not include raw source data in exception messages;
- do not auto-map to domain classes;
- do not use `GetRecords<TDomain>()`;
- process fields by ordinal into the generic row model.

Configure:

```txt
ExceptionMessagesContainRawData = false
```

or the package's current equivalent.

Do not place raw row contents in custom logs or ProblemDetails.

### CSV encoding policy

Supported generic encodings:

- UTF-8 with or without BOM;
- UTF-16 LE/BE only when a BOM is present.

Rules:

- BOM is authoritative when present;
- a BOM-less file is decoded as strict UTF-8;
- invalid strict UTF-8 fails with a safe encoding error;
- do not silently fall back to Windows-1250, Windows-1252, or ISO-8859 variants;
- do not add heuristic charset-detection packages;
- detected encoding is recorded in preview metadata;
- future exact vendor processors may add an explicitly documented encoding rule after sample review.

This avoids silent character corruption and vendor-specific guessing.

### CSV delimiter detection

Use a bounded candidate set:

```txt
,
;
TAB
|
```

Rules:

- inspect only a bounded initial sample;
- ignore delimiters inside valid quoted fields;
- choose a delimiter only when detection is deterministic;
- record the chosen delimiter in preview metadata;
- allow a valid single-column source when no candidate delimiter is present;
- reject ambiguous multi-column detection safely;
- do not accept arbitrary user-provided delimiter strings in Unit 45;
- do not select delimiter based on Bosnian locale alone;
- future exact processors may define explicit delimiter rules.

Use `CsvHelper` delimiter detection where it satisfies these constraints, with additional deterministic validation around the result.

### CSV safety limits

Add/configure:

```txt
Imports__MaxRowsPerFile
Imports__MaxColumnsPerFile
Imports__MaxCellLengthCharacters
Imports__HeaderScanRowLimit
Imports__CsvDetectionRowLimit
```

Rules:

- all positive and bounded;
- row count includes non-header data rows;
- column count applies after synthetic-column expansion;
- cell length uses decoded characters;
- detection sample remains small and bounded;
- limits are enforced during streaming;
- exceeding a limit stops processing, clears temporary result state, and returns a safe failure;
- no partial preview is committed.

### XLSX reader behavior

Implement the XLSX reader with the low-level `ExcelDataReader` API.

Do not use `AsDataSet()`.

Required behavior:

- accept only the already approved `.xlsx`/OpenXML input;
- inspect worksheets sequentially;
- identify non-empty worksheets;
- process rows sequentially;
- retain only bounded preview rows;
- count total rows;
- preserve source row numbers;
- map cell values into generic tabular values;
- respect configured row/column/cell limits;
- dispose reader and streams deterministically;
- no formula evaluation;
- no macros;
- no workbook editing.

### XLSX worksheet policy

Generic Unit 45 behavior:

- ignore completely empty worksheets;
- require exactly one non-empty worksheet;
- use that worksheet;
- use its first non-empty row as header;
- fail safely when no non-empty worksheet exists;
- fail safely when multiple non-empty worksheets exist.

Stable failures:

```txt
XLSX_NO_NON_EMPTY_WORKSHEET
XLSX_MULTIPLE_NON_EMPTY_WORKSHEETS
```

Do not guess which worksheet is the data sheet.

A future exact processor may select a specific worksheet by confirmed name/order.

Record the selected worksheet name in preview metadata.

### XLSX container preflight

An XLSX file is a compressed OpenXML package.

Before normal row reading, perform a bounded container preflight using built-in .NET ZIP APIs or an equivalent safe approach.

Add configuration:

```txt
Imports__MaxXlsxEntryCount
Imports__MaxXlsxUncompressedSizeBytes
Imports__MaxXlsxCompressionRatio
```

Required behavior:

- verify the source has the expected ZIP/OpenXML container shape;
- reject corrupted/non-XLSX content even when extension/MIME claimed XLSX;
- reject an excessive entry count;
- reject excessive declared total uncompressed size;
- reject excessive compression ratio;
- reject pathologically large entries;
- do not extract entries to arbitrary paths;
- do not execute embedded content;
- do not log entry contents;
- preserve stream/source ownership.

This is a decompression-bomb safety boundary, not a complete malware scanner.

Do not claim antivirus protection.

### Seekable source handling

XLSX ZIP and reader operations may require a seekable source.

Add an Infrastructure abstraction equivalent to:

```txt
ISeekableImportSourceFactory
```

Behavior:

- reuse the Unit 40 source stream directly when it is safely seekable;
- otherwise materialize it into a bounded temporary working file;
- never load the complete source into memory solely to gain seeking;
- preserve Unit 43 authorization by operating only after the import use case has opened the authorized source;
- delete temporary working files deterministically;
- clean up after success, failure, and cancellation;
- do not expose temporary paths outside Infrastructure;
- do not copy the temporary file into media/storage metadata;
- do not persist the working path.

Add typed configuration:

```txt
Imports__TemporaryWorkingDirectory
```

Rules:

- required when the environment/provider can return non-seekable streams;
- outside `wwwroot` and frontend public directories;
- not source controlled;
- normalized and validated;
- not returned through APIs or normal logs;
- per-operation temporary names are server-generated;
- file size remains bounded by import/global storage limits;
- orphaned working files are not handled by a background job in Unit 45.

In current local development, the Unit 40 local source stream may already be seekable, so no duplicate working file should be created unnecessarily.

Update `.gitignore` only if the chosen repository-local development working directory is not already covered.

### Content-level format verification

Do not trust extension/content type alone.

#### Declared CSV

Verify:

- supported encoding policy;
- text-like content without disallowed NUL/binary patterns;
- deterministic delimiter/single-column structure;
- parseable bounded rows.

If content is an XLSX ZIP or binary source, fail with:

```txt
SOURCE_FORMAT_MISMATCH
```

#### Declared XLSX

Verify:

- valid ZIP/OpenXML container;
- Excel reader can open it;
- worksheet policy succeeds.

If content is plain CSV/text or unrelated ZIP content, fail with:

```txt
SOURCE_FORMAT_MISMATCH
```

Do not change the immutable Unit 43 `FileFormat` automatically.

The user must create a new import job with correct source metadata if the uploaded content was mislabeled.

### Preview metadata persistence

Add an optional structured `jsonb` field or equivalent typed owned persistence on `ImportJob`:

```txt
PreviewMetadataJson
```

Persist safe metadata such as:

```txt
readerKey
readerVersion
detectedFileFormat
encodingName
delimiter
worksheetName
headerSourceRowNumber
firstDataSourceRowNumber
columnCount
totalRowCount
previewRowLimit
previewWasTruncated
structuralWarningCount
```

Rules:

- no storage key/path;
- no source values;
- no entire headers array duplicated when columns already persist;
- no raw exception details;
- deterministic property names;
- replaced atomically with a successful preview;
- cleared/replaced consistently when preview is regenerated;
- not treated as validation readiness;
- returned through import detail/preview DTOs as safe optional metadata.

This requires an EF Core migration.

Do not put package-specific object graphs into JSON.

### Preview persistence integration

The generic processor uses existing Unit 43 records:

```txt
ImportPreviewColumn
ImportPreviewRow
```

On successful preview:

- replace prior preview columns/rows atomically;
- persist normalized columns;
- persist at most `PreviewRowLimit` rows;
- persist total source row count;
- persist detected data types;
- persist preview metadata;
- set `PreviewGeneratedAtUtc`;
- set `PreviewRowCount`;
- return status to `UPLOADED`;
- clear safe prior preview-processing failure state;
- write `IMPORT_JOB_PREVIEW_GENERATED` audit;
- keep validation revision/readiness unchanged according to Unit 43 rules.

If the job was `READY_TO_CONFIRM` from a future exact processor and a generic preview regeneration would invalidate interpretation, the exact processor should own the combination instead. Generic fallback must not supersede an exact ready job.

### Preview truncation semantics

A preview is intentionally bounded.

Rules:

- `PreviewRowCount` is the number persisted/displayed;
- `TotalRowCount` is the number of data rows scanned;
- `PreviewWasTruncated = TotalRowCount > PreviewRowCount`;
- scanning continues through the bounded maximum to compute total rows and inferred types;
- do not stop after preview rows unless a later performance decision explicitly changes the contract;
- the UI must not interpret preview count as total row count;
- no full dataset is persisted.

### Failure codes

Map known parsing failures to stable safe codes.

At minimum:

```txt
SOURCE_FORMAT_MISMATCH
SOURCE_EMPTY
SOURCE_ENCODING_UNSUPPORTED
CSV_DELIMITER_AMBIGUOUS
CSV_MALFORMED
XLSX_INVALID_CONTAINER
XLSX_CONTAINER_LIMIT_EXCEEDED
XLSX_NO_NON_EMPTY_WORKSHEET
XLSX_MULTIPLE_NON_EMPTY_WORKSHEETS
HEADER_NOT_FOUND
ROW_LIMIT_EXCEEDED
COLUMN_LIMIT_EXCEEDED
CELL_LENGTH_LIMIT_EXCEEDED
SOURCE_READ_FAILED
```

Safe failure messages:

- are Bosnian-ready/user-presentable backend messages according to existing API conventions or stable code plus safe default;
- contain no raw row/cell contents;
- contain no storage path/key;
- contain no package exception text;
- contain no ZIP entry content;
- contain no stack trace.

Infrastructure logs may include:

- import job ID;
- reader key/version;
- safe failure code;
- source row/column number;
- correlation ID.

Do not log source values.

### Processing and lease integration

The generic preview processor uses Unit 43's `PREVIEW` processing lease.

Rules:

- parsing occurs outside a long database transaction;
- the original immutable source is opened after lease acquisition;
- temporary resources are disposed after processing;
- preview result is finalized only if the lease still matches;
- a stale/lost lease discards the generated result;
- no partial preview rows are persisted;
- an old parser cannot overwrite a newer preview;
- cancellation token is propagated through stream copy and reader loops;
- technical failure finalizes to `FAILED` only when the processor still owns the lease.

Do not introduce background processing.

### Audit integration

Reuse Unit 43 actions:

```txt
IMPORT_JOB_PREVIEW_GENERATED
IMPORT_JOB_PROCESSING_FAILED
```

Successful preview audit metadata may include:

```txt
readerKey
readerVersion
detectedFileFormat
encodingName
delimiter
worksheetName
columnCount
totalRowCount
previewRowCount
previewWasTruncated
structuralWarningCount
```

Do not include:

- source row values;
- headers duplicated in full;
- file bytes;
- storage key/path;
- temporary path;
- package exceptions.

Failed parsing audit contains only safe failure code and bounded context.

No audit event is created when:

- processor resolution fails before a lease;
- authorization fails;
- active lease conflict occurs;
- the operation loses its lease;
- database transaction rolls back.

### Clean Architecture boundaries

Application owns:

- tabular reader contracts;
- generic row/column/value/issue models;
- header normalization rules;
- conservative generic type model;
- generic preview processor;
- exact-versus-fallback processor resolution semantics;
- safe parser failure categories;
- preview result mapping;
- validation-result structures for future processors.

Infrastructure owns:

- CsvHelper implementation;
- ExcelDataReader implementation;
- ZIP/OpenXML preflight;
- encoding/delimiter mechanics;
- seekable source materialization;
- temporary working files;
- package registration;
- EF mapping/migration for preview metadata;
- package-specific exception translation.

API owns:

- no new endpoint;
- existing Unit 43 preview/capabilities/list/detail endpoint mapping remains unchanged except safe DTO additions;
- no package-specific types.

Domain owns:

- no CSV/XLSX package logic;
- existing import workflow and lease invariants remain authoritative.

Do not put package parsing calls in endpoint handlers or Domain.

### Configuration

Extend `ImportOptions` with:

```txt
Imports__HeaderScanRowLimit
Imports__CsvDetectionRowLimit
Imports__MaxRowsPerFile
Imports__MaxColumnsPerFile
Imports__MaxCellLengthCharacters
Imports__MaxXlsxEntryCount
Imports__MaxXlsxUncompressedSizeBytes
Imports__MaxXlsxCompressionRatio
Imports__TemporaryWorkingDirectory
```

Rules:

- values are typed;
- positive;
- bounded;
- cross-validated with:
  - import upload maximum;
  - global storage maximum;
- XLSX uncompressed limit must be greater than zero and may exceed upload size only within a deliberate safe bound;
- temporary working directory is normalized safely;
- startup fails clearly for invalid configuration;
- errors do not reveal secrets or source contents.

Update:

```txt
backend/.env.example
```

with safe development examples and comments.

Example values must be described as configurable safety ceilings, not confirmed vendor requirements.

Do not expose temporary directory or decompression limits through public capabilities unless the UI genuinely needs them. The existing capability response may expose only user-relevant upload/preview limits.

### EF Core migration

Add `PreviewMetadataJson` or its approved typed equivalent to the import job persistence model.

Use PostgreSQL `jsonb` when JSON is selected.

Generate the migration using the approved CLI workflow under:

```txt
backend/src/Infrastructure/Persistence/Migrations/
```

Use:

```txt
--output-dir Persistence/Migrations
```

No new official imported-data tables are added.

No vendor mapping tables are added.

### Tests

Add comprehensive tests with small committed fixtures that contain no real club/player/vendor-sensitive data.

#### Package and architecture tests

Cover:

- package references exist only where required;
- Application has no CsvHelper/ExcelDataReader type dependency;
- `ExcelDataReader.DataSet` is absent;
- no `DataSet`/`AsDataSet()` use in the parser implementation;
- no frontend parser dependency/file change.

#### Processor resolution tests

Cover:

- generic CSV fallback;
- generic XLSX fallback;
- all stable import types/source systems receive preview capability;
- generic fallback has no validate/confirm capability;
- exact processor overrides fallback;
- duplicate exact processor fails startup;
- duplicate fallback fails startup;
- deterministic capability ordering;
- stable processor key/version.

#### Header normalization tests

Cover:

- whitespace;
- Unicode Form C;
- punctuation;
- blank header;
- duplicate header;
- multiple duplicates;
- equivalent normalized names;
- stable suffixes;
- non-Latin letters;
- deterministic repeated runs.

#### CSV tests

Cover:

- UTF-8 without BOM;
- UTF-8 with BOM;
- UTF-16 LE/BE with BOM;
- invalid BOM-less UTF-8;
- comma;
- semicolon;
- tab;
- pipe;
- valid single-column source;
- ambiguous delimiter;
- quoted delimiter;
- escaped quote;
- multiline quoted field;
- blank leading rows;
- missing header;
- duplicate/blank headers;
- short rows padded with null;
- long rows create synthetic columns;
- malformed quoted data;
- binary/NUL source;
- format mismatch;
- row limit;
- column limit;
- cell-length limit;
- cancellation;
- raw data excluded from exception/log messages;
- source stream ownership.

#### XLSX tests

Cover:

- valid one-sheet workbook;
- leading blank rows;
- blank/duplicate headers;
- null, boolean, numeric, date, text, and mixed cells;
- zero preservation;
- large integer precision policy;
- formula cell does not execute;
- empty workbook;
- empty sheets plus one non-empty sheet;
- multiple non-empty worksheets;
- corrupt ZIP;
- unrelated ZIP;
- extension/content mismatch;
- excessive ZIP entry count;
- excessive uncompressed size;
- excessive compression ratio;
- row/column/cell limits;
- cancellation;
- no full DataSet allocation;
- stream and reader disposal.

#### Seekable materialization tests

Cover:

- seekable input is reused;
- non-seekable input is copied to a bounded temporary file;
- temporary file removed on success;
- removed on failure;
- removed on cancellation;
- no source-controlled directory pollution;
- generated temporary names;
- limit enforcement;
- no path leakage.

#### Generic type detection tests

Cover:

- empty;
- text;
- boolean;
- integer;
- decimal;
- strict date;
- strict datetime;
- mixed;
- null ignored for inference;
- comma decimal not guessed;
- local date not guessed;
- zero retained;
- precision-safe value serialization.

#### Preview integration tests

Cover:

- Unit 43 upload then generic preview;
- job transitions `UPLOADED -> PARSING -> UPLOADED`;
- preview columns/rows persisted;
- preview row limit;
- total row count;
- truncation flag;
- parser metadata persisted;
- regeneration replaces preview atomically;
- old preview remains when new processing fails before finalization;
- lost lease cannot overwrite;
- safe failure transitions to `FAILED`;
- source file retained;
- audit metadata safe;
- no validation issues/readiness created;
- `VALIDATE` absent;
- `CONFIRM` absent;
- Unit 44-compatible DTO shape.

#### Configuration tests

Cover:

- valid values;
- non-positive values;
- unsafe/bad temporary directory;
- cross-limit validation;
- excessive XLSX ratio/size values;
- startup failure without path/secret leakage.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

Record:

- package names and pinned versions actually installed;
- parser/processor keys and versions;
- effective fallback behavior;
- supported encodings;
- delimiter policy;
- XLSX worksheet policy;
- safety limits;
- temporary working-file behavior;
- preview metadata persistence/migration;
- verification results;
- intentionally absent validation/confirmation/vendor mappings.

If implementation discovers that a real Gpexe or Zone14 file requires a different delimiter, encoding, worksheet, or header policy, do not alter the generic reader based on guesswork.

Document the sample and handle it in Unit 46/47 through an exact processor/spec.

## Implementation

### 1. Add mature reader packages

Install:

```txt
CsvHelper
ExcelDataReader
```

in Infrastructure through the repository's approved NuGet workflow.

Pin current stable compatible versions.

Do not add the DataSet extension.

### 2. Add tabular Application contracts

Create:

- reader interface/resolver;
- read options;
- generic columns;
- generic rows;
- value representation;
- structural issues;
- detected types;
- generic validation-result structures;
- safe failure categories.

Keep package types outside Application.

### 3. Add header and value utilities

Implement:

- deterministic header normalization;
- duplicate/synthetic column handling;
- invariant value serialization;
- conservative type aggregation;
- row-shape normalization.

Add focused unit tests.

### 4. Implement the CSV reader

Use CsvHelper with:

- strict safe encoding policy;
- bounded delimiter detection;
- raw-data-free errors;
- streaming rows;
- limits;
- cancellation;
- generic values only.

### 5. Implement the XLSX reader

Use low-level ExcelDataReader with:

- container preflight;
- exactly one non-empty worksheet;
- first non-empty header row;
- sequential rows;
- limits;
- no formula execution;
- no DataSet.

### 6. Add seekable-source support

Implement safe reuse/materialization for parser operations.

Configure and validate the temporary working directory.

Guarantee deterministic cleanup.

### 7. Extend processor resolution

Add exact-first, generic-fallback resolution.

Register:

```txt
generic-csv-preview
generic-xlsx-preview
```

as preview-only fallbacks.

Update capabilities automatically.

### 8. Implement the generic preview processor

Integrate the generic readers with Unit 43:

- acquire preview lease;
- open source;
- parse and retain bounded preview;
- persist columns/rows/metadata atomically;
- return to `UPLOADED`;
- audit success/failure;
- no validation/confirmation.

### 9. Add preview metadata persistence

Extend the import model/DTOs.

Configure `jsonb` or approved typed persistence.

Generate and apply the migration.

### 10. Add configuration

Extend `ImportOptions`.

Update `.env.example`.

Add startup validation and tests.

### 11. Add comprehensive tests

Implement reader, resolver, safety, lease, persistence, and endpoint integration coverage.

Use synthetic fixtures only.

### 12. Update progress documentation

Update `context/progress-tracker.md` with actual implementation and verification state.

Do not mark Unit 45 complete until all required checks pass or failures are explicitly documented.

## Dependencies

Add only:

```txt
CsvHelper
ExcelDataReader
```

Use their current stable versions compatible with the repository's .NET target and pin them according to the repository package-management convention.

Use built-in .NET APIs for:

- streams;
- encodings/BOM handling;
- ZIP preflight;
- temporary files;
- Unicode normalization;
- JSON.

Do not add:

- `ExcelDataReader.DataSet`;
- a second CSV/XLSX reader;
- workbook editing packages;
- charset detection packages;
- spreadsheet formula engines;
- vendor SDKs;
- frontend packages.

## Verification checklist

- [ ] `CsvHelper` is installed only in the required backend Infrastructure project.
- [ ] `ExcelDataReader` is installed only in the required backend Infrastructure project.
- [ ] Current stable compatible versions are pinned through the approved package-management workflow.
- [ ] `ExcelDataReader.DataSet` is not installed.
- [ ] `DataSet`/`AsDataSet()` is not used for XLSX reading.
- [ ] Application contracts contain no CsvHelper or ExcelDataReader types.
- [ ] Provider-neutral tabular reader contracts exist.
- [ ] Generic column, row, value, structural issue, and validation-result contracts exist.
- [ ] Exact processors resolve before generic fallback processors.
- [ ] Duplicate processor/fallback registrations fail startup.
- [ ] Generic CSV and XLSX fallback processors have stable explicit keys/versions.
- [ ] Every current CSV/XLSX import/source combination receives generic preview capability.
- [ ] Generic fallback exposes `canPreview=true`.
- [ ] Generic fallback exposes `canValidate=false`.
- [ ] Generic fallback exposes `canConfirm=false`.
- [ ] No generic job becomes `READY_TO_CONFIRM`.
- [ ] No official domain data is mutated.
- [ ] No Gpexe or Zone14 field mapping is added.
- [ ] Header selection uses the first non-empty row within a bounded scan.
- [ ] Header normalization is deterministic and Unicode-safe.
- [ ] Blank headers receive synthetic names.
- [ ] Duplicate normalized headers receive stable suffixes.
- [ ] No source cell is silently discarded.
- [ ] Short rows are padded with null.
- [ ] Long rows create bounded synthetic columns.
- [ ] Preview rows align with the final column set.
- [ ] Generic detected types are limited to the documented values.
- [ ] CSV type inference does not guess local decimals/dates/vendor booleans.
- [ ] XLSX type inference uses safe reader-provided value types.
- [ ] Null, zero, boolean, precision, and ISO date behavior is verified.
- [ ] CSV supports quoted delimiters, escaped quotes, and multiline quoted fields.
- [ ] CSV exception messages exclude raw source data.
- [ ] CSV supports UTF-8 with/without BOM.
- [ ] CSV supports UTF-16 only with BOM.
- [ ] Invalid BOM-less UTF-8 is rejected rather than silently decoded.
- [ ] No heuristic legacy charset fallback is added.
- [ ] CSV delimiter candidates are comma, semicolon, tab, and pipe.
- [ ] Delimiter detection is bounded and deterministic.
- [ ] Valid single-column CSV is supported.
- [ ] Ambiguous delimiter detection fails safely.
- [ ] XLSX uses low-level streaming/forward reading.
- [ ] XLSX accepts exactly one non-empty worksheet in the generic reader.
- [ ] Empty and multiple-non-empty workbook cases fail safely.
- [ ] XLSX formulas/macros are not executed.
- [ ] XLSX container content is verified rather than trusting extension/MIME only.
- [ ] ZIP entry, uncompressed-size, and compression-ratio limits are enforced.
- [ ] No ZIP content is extracted to arbitrary paths.
- [ ] Row, column, cell, header-scan, and detection limits are configurable and enforced.
- [ ] Content-level CSV/XLSX format mismatch is detected.
- [ ] Seekable source streams are reused.
- [ ] Non-seekable streams are boundedly materialized to temporary working files.
- [ ] Temporary working paths are private, ignored, validated, and never exposed.
- [ ] Temporary files are removed on success, failure, and cancellation.
- [ ] Full source files are not loaded into memory.
- [ ] Complete datasets/workbooks are not persisted.
- [ ] Existing preview tables persist only the configured preview row limit.
- [ ] Full source data rows are scanned only within configured maximums.
- [ ] `TotalRowCount`, `PreviewRowCount`, and truncation semantics are correct.
- [ ] Safe parser metadata is persisted with successful previews.
- [ ] Parser metadata contains no storage/temp path or source data.
- [ ] Preview regeneration replaces columns/rows/metadata atomically.
- [ ] Failed regeneration cannot leave partial new preview data.
- [ ] Lost processing leases cannot finalize.
- [ ] Generic preview transitions through `PARSING` and returns to `UPLOADED`.
- [ ] Stable safe failure codes are used.
- [ ] Failure messages/logs contain no raw rows, cells, storage paths, or package exceptions.
- [ ] `IMPORT_JOB_PREVIEW_GENERATED` audit contains only bounded safe metadata.
- [ ] Generic parse failure audit contains only safe failure context.
- [ ] Unit 44 receives real preview actions through capabilities/allowed actions without frontend placeholder changes.
- [ ] Unit 44 preview DTO remains compatible with dynamic columns and rows.
- [ ] No frontend parser package or browser-side parsing is added.
- [ ] No background worker is added.
- [ ] `backend/.env.example` documents new parser safety configuration.
- [ ] EF Core migration is generated under `backend/src/Infrastructure/Persistence/Migrations/`.
- [ ] `dotnet ef database update` succeeds when the local database environment is available.
- [ ] CSV reader tests pass.
- [ ] XLSX reader and ZIP safety tests pass.
- [ ] Header/value/type tests pass.
- [ ] Processor fallback/override tests pass.
- [ ] Seekable materialization/cleanup tests pass.
- [ ] Preview lease/persistence/audit integration tests pass.
- [ ] Configuration tests pass.
- [ ] Synthetic fixtures contain no real club/vendor-sensitive data.
- [ ] No frontend files are changed.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for affected backend projects/solution.
- [ ] `dotnet test` passes for relevant backend test projects.
- [ ] `context/progress-tracker.md` records actual Unit 45 packages, parser versions, policies, migration, and verification state.
