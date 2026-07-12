# Unit 40: File Storage Abstraction

## Goal

Build the provider-neutral backend file-storage foundation required by future media and import modules. Add an Application-layer storage contract, a safe local-development adapter, persistent stored-file metadata, validated configuration, and compensation rules for partial failures without adding public upload/download endpoints, media entities, import jobs, production object-storage SDKs, or frontend UI.

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
9. `context/feature-specs/07-backend-configuration-baseline.md`
10. `context/feature-specs/12-backend-persistence-foundation.md`
11. `context/feature-specs/13-backend-domain-shared-primitives.md`
12. `context/feature-specs/20-backend-staff-roles-authorization-foundation.md`
13. `context/feature-specs/40-file-storage-abstraction.md`

Use relevant installed backend Codex skills/plugins when applicable.

Skills/plugins may guide implementation workflow, but must not override the project context files, architecture rules, code standards, or this spec.

This unit is backend-only.

Do not add or change frontend routes, pages, components, shadcn/ui components, navigation, API wrappers, or browser upload flows.

### Scope

This unit introduces:

- provider-neutral Application-layer file-storage contracts;
- opaque server-generated storage keys;
- a local filesystem adapter for development;
- streaming write/read behavior;
- safe temporary-file and finalization behavior;
- internal compensation deletion for failed higher-level use cases;
- persistent `StoredFile` metadata;
- archive metadata without physical deletion;
- typed storage configuration;
- startup configuration validation;
- local storage directory ignore protection;
- EF Core mapping and migration;
- focused unit and integration tests.

This unit does not introduce:

- public upload endpoints;
- public download endpoints;
- static-file serving;
- media assets;
- external media references;
- entity attachment/link tables;
- import jobs;
- CSV/XLSX parsing;
- player images UI;
- video streaming;
- thumbnails;
- generated reports;
- presigned URLs;
- S3-compatible implementation;
- AWS, MinIO, Azure Blob, or other provider SDKs;
- virus scanning;
- image/video transcoding;
- background cleanup jobs;
- storage quotas;
- multipart/chunked resumable upload protocols;
- frontend UI.

Unit 41 will consume this foundation for media. Unit 43 will consume it for import source files.

### Architectural boundary

The Application layer defines what storage operations the application needs.

Infrastructure implements those operations.

Domain must not depend on:

- filesystem APIs;
- cloud-storage SDKs;
- ASP.NET Core upload types;
- local paths;
- storage-provider options.

API endpoint handlers must not:

- create storage keys;
- write files directly;
- build filesystem paths;
- decide provider behavior;
- persist file metadata directly.

Future media/import use cases must orchestrate storage through the Application contracts.

### No generic public file API

Do not add routes such as:

```txt
POST /api/files
GET /api/files/{id}
GET /files/{key}
```

A generic file endpoint would lack the entity-specific authorization and lifecycle rules required by media, imports, player images, and generated reports.

Future modules must expose authorized feature-specific operations that:

1. validate their own file type and business rules;
2. call the storage abstraction;
3. persist `StoredFile` metadata together with the owning entity;
4. serve content only after feature-specific authorization.

### Storage contract

Define a provider-neutral Application abstraction such as:

```txt
IFileStorage
```

The exact naming may follow existing conventions.

The contract must support at minimum:

```txt
WriteAsync
OpenReadAsync
DeleteIfExistsAsync
```

Conceptual behavior:

#### `WriteAsync`

Accepts:

- a readable `Stream`;
- an opaque server-generated storage key;
- optional declared length for early validation;
- cancellation token.

Returns authoritative write information including:

- storage key;
- actual bytes written.

Rules:

- actual bytes written are counted by the implementation;
- the caller's declared size is not the final source of truth;
- content is streamed;
- content is not loaded fully into memory;
- writing does not overwrite an existing object;
- cancellation is respected;
- partial output is cleaned up.

#### `OpenReadAsync`

Accepts:

- storage key;
- cancellation token.

Returns a readable stream or the project's safe missing-object result/exception abstraction.

Rules:

- no public URL is returned;
- no local filesystem path is returned;
- the caller is responsible for disposing the stream;
- the implementation does not make authorization decisions;
- feature use cases must authorize before opening content.

#### `DeleteIfExistsAsync`

Used only for:

- compensation after a failed higher-level business operation;
- controlled test cleanup;
- explicitly approved future maintenance workflows.

Rules:

- idempotent when the object is already missing;
- not exposed through a public product endpoint;
- not used as normal archive behavior;
- does not delete a committed `StoredFile` record through Unit 40 APIs.

Do not add provider-specific methods to the Application interface.

Do not add:

- bucket names;
- S3 clients;
- presigned URLs;
- local root paths;
- filesystem handles;
- `IFormFile`.

### Stream ownership

Define stream ownership clearly.

Recommended rules:

- the caller owns and disposes the upload/input stream;
- the storage implementation does not dispose the caller's input stream;
- the caller owns and disposes the stream returned by `OpenReadAsync`;
- storage implementation-created internal streams are disposed by the implementation;
- tests verify the contract.

Do not leave stream ownership ambiguous.

### Opaque storage keys

Storage keys must be generated by trusted application code.

Requirements:

- clients never submit or choose the storage key;
- original filenames are never used as storage paths;
- keys are unique and provider-neutral;
- keys use `/` as the logical segment separator;
- keys do not begin with `/`;
- keys contain no `..`;
- keys contain no backslashes;
- keys contain no drive letters;
- keys contain no URL scheme;
- keys contain no user-supplied directory segment.

A suitable conceptual format is:

```txt
objects/{yyyy}/{MM}/{generated-guid}
```

The exact prefix may follow repository conventions.

Use UTC time from the approved clock abstraction when date segments are included.

Do not append the original filename.

A file extension is not required in the storage key because content type and original filename are metadata.

### Storage-key generator

Add an Application-safe storage-key generator abstraction/service or a deterministic helper that can be tested without Infrastructure path knowledge.

Requirements:

- generated keys pass storage-key validation;
- generated keys are independent of provider;
- collision probability is negligible;
- keys do not reveal user IDs, emails, player names, match names, or original filenames;
- callers cannot inject path segments.

Do not generate keys in Minimal API endpoint handlers.

### Local-development adapter

Add an Infrastructure implementation such as:

```txt
LocalFileStorage
```

It is a development adapter only.

Requirements:

- all files remain under one configured root directory;
- the root is not `wwwroot`;
- the root is not the frontend `public` directory;
- the root is not exposed through static-file middleware;
- the adapter resolves the logical storage key under the root;
- full-path containment is verified before every filesystem operation;
- rooted paths and traversal attempts are rejected;
- necessary parent directories are created safely;
- existing files are never overwritten;
- asynchronous stream copy is used;
- a temporary file is written first;
- the temporary file is moved/renamed to the final path only after a complete successful write;
- cancellation or write failure removes the temporary file;
- no partial final file remains;
- delete is idempotent;
- open-read does not expose the absolute path.

Do not trust storage-key validation alone. The local adapter must independently verify resolved-path containment.

### Temporary-file behavior

Write temporary files inside the configured storage root or an internal temporary subdirectory under that root.

Temporary names must:

- be server-generated;
- not use the original filename;
- not collide with final keys;
- be removed after failure/cancellation;
- be finalized only after successful byte-count and size validation.

Do not use the operating system's global temporary directory for committed upload content unless implementation constraints require it and the reason is documented.

Do not leave predictable `.tmp` names derived from user input.

### Symlink and path safety

The configured root must be normalized once during adapter construction.

For each operation:

- combine the normalized root with a validated logical key;
- resolve the full path;
- verify it remains inside the normalized root;
- reject traversal/rooted-path attempts before I/O.

Do not intentionally create symlinks inside the storage root.

If the local environment already contains unsafe symlinked directories that escape the root, fail safely rather than following them for write operations where practical with the target framework/filesystem APIs.

Document any platform-specific limitation in `context/progress-tracker.md`.

### Streaming and memory behavior

File content must be streamed.

Do not:

- use `ReadToEnd`;
- convert file content to Base64;
- buffer the entire file in a byte array;
- persist file bytes in PostgreSQL;
- return file bytes inside normal JSON DTOs.

Use a bounded copy buffer and asynchronous I/O.

The implementation must count actual bytes while streaming.

### Global storage size limit

Add a configurable global maximum object size.

This is an infrastructure safety ceiling, not a feature-specific product limit.

Future modules may impose lower limits for:

- images;
- spreadsheets;
- videos;
- generated documents.

Rules:

- value is configured in bytes;
- value must be positive;
- implementation rejects content larger than the configured maximum;
- if declared length exceeds the limit, reject before writing;
- if actual streamed bytes exceed the limit, stop, clean up temporary content, and fail safely;
- zero-byte content is rejected;
- exact configured maximum is allowed;
- size validation is tested around boundary values.

Do not hardcode one universal media/import business limit into feature code.

### Content type and extension

`StoredFile.ContentType` is metadata supplied by a trusted feature use case after feature-specific validation.

The generic storage layer:

- requires a non-empty bounded content type;
- stores it;
- does not trust it as proof of file content;
- does not infer authorization from it;
- does not maintain a global allowlist.

Future media/import modules must define their own accepted file types and extensions.

Original filename extension may be used by those modules as one validation signal, but the local storage adapter must not use it to build the storage path.

Do not implement MIME sniffing or antivirus scanning in Unit 40.

### Original filename normalization

Persist an original/display filename for user-facing history.

Rules:

- use only the final filename component;
- remove any submitted directory path;
- trim surrounding whitespace;
- reject empty normalized names;
- reject control characters;
- bound the length;
- do not use the value as a storage key;
- do not use it as a local path;
- do not assume it is unique;
- escape it normally in future UI.

A suitable maximum is 255 characters unless existing repository conventions define another bound.

Do not silently replace a dangerous path with an unrelated filename without validation feedback.

### Stored-file metadata model

Add a persistent entity such as:

```txt
StoredFile
```

Use existing entity/audit timestamp conventions.

Required fields:

```txt
Id
StorageKey
OriginalFileName
ContentType
SizeBytes
UploadedByUserId
CreatedAtUtc
IsArchived
ArchivedAtUtc
ArchivedByUserId
```

Rules:

- `Id` uses the normal project identifier convention;
- `StorageKey` is required and globally unique;
- `OriginalFileName` is required and bounded;
- `ContentType` is required and bounded;
- `SizeBytes` is greater than zero;
- `UploadedByUserId` is required and comes from the authenticated current user in future consuming use cases;
- timestamps are UTC;
- archive actor/time are either both present or both absent;
- normal lifecycle never hard-deletes the metadata record.

If the existing entity conventions use a status enum rather than `IsArchived`, follow that convention while preserving the same behavior.

### Stored-file archive semantics

Archiving a `StoredFile`:

- changes metadata only;
- does not delete physical bytes;
- preserves the storage key and all upload metadata;
- is not exposed through a public Unit 40 endpoint;
- must be invoked only by a future owning module with appropriate authorization and lifecycle rules.

Restoring generic stored-file metadata is not required in this unit.

Do not assume every consumer will archive files:

- media assets may archive later;
- original import files may require indefinite retention;
- generated artifacts may have separate lifecycle rules.

Unit 40 provides the metadata capability without imposing a product workflow.

### Ownership and entity links

Do not add generic:

```txt
LinkedEntityType
LinkedEntityId
```

fields to `StoredFile` in Unit 40.

Future modules should use explicit relational ownership/link models:

- Unit 41 `MediaAsset` references a stored file;
- Unit 43 `ImportJob` references its original stored file;
- later player-image/generated-report modules use explicit foreign keys or attachment records.

This avoids polymorphic foreign keys and keeps authorization inside the owning module.

A committed stored file should have one clear owning workflow. Do not use one physical object as a mutable shared file across unrelated aggregates unless a later feature explicitly defines that behavior.

### User relationship

Configure `UploadedByUserId` and optional `ArchivedByUserId` using the established staff-user identifier type.

Rules:

- normal account disable/reactivation does not remove stored-file metadata;
- no cascade delete from staff accounts;
- no credentials or security data are copied into file metadata;
- file metadata returns user IDs, not embedded duplicate user profiles;
- future read models may project safe actor summaries.

### Metadata and object consistency

Database and object storage cannot share a true distributed transaction.

Future consuming use cases must follow a compensation pattern:

1. validate authorization, metadata, feature limits, and references;
2. generate a trusted storage key;
3. stream the object to storage;
4. begin/use the existing database transaction;
5. add `StoredFile` metadata and the owning entity/link;
6. commit the database transaction;
7. if database persistence fails, call `DeleteIfExistsAsync` for the just-written uncommitted object;
8. do not return success unless both object write and database commit succeed.

Unit 40 must provide the contracts/helpers needed for this pattern.

Do not:

- claim object storage and PostgreSQL are atomically committed;
- swallow compensation failures;
- return success when metadata/link persistence fails;
- automatically delete an already committed object because a later unrelated action fails.

If compensation deletion fails:

- log the storage key and correlation ID safely;
- do not log file content or credentials;
- return the original operation as failed;
- document during development/testing;
- leave future orphan reconciliation as a separate operational concern.

Do not add a cleanup background job in Unit 40.

### Metadata construction

Do not trust file metadata supplied directly by future clients.

Future use cases must construct `StoredFile` from:

- normalized original filename;
- validated content type;
- actual bytes written returned by storage;
- server-generated storage key;
- authenticated current user;
- approved UTC clock.

Do not persist the client-declared size when it differs from actual bytes written.

### Configuration model

Add typed options such as:

```txt
FileStorageOptions
```

Recommended configuration keys:

```txt
FileStorage__Provider
FileStorage__LocalRootPath
FileStorage__MaxObjectSizeBytes
```

Supported provider value in Unit 40:

```txt
Local
```

Do not add fake S3 settings.

Do not add access key, secret key, bucket, region, endpoint, or presigned URL settings until a production provider is selected in a later unit.

### Local configuration example

Update:

```txt
backend/.env.example
```

with safe non-secret development examples, such as:

```txt
FileStorage__Provider=Local
FileStorage__LocalRootPath=./.local-storage
FileStorage__MaxObjectSizeBytes=1073741824
```

The example maximum is a configurable development ceiling, not a fixed V1 business requirement.

Use comments explaining:

- local root is development-only;
- real `.env` files remain uncommitted;
- production must not depend on the local filesystem adapter;
- future feature modules may use lower file-size limits.

Do not commit real machine-specific absolute paths.

### Local-root resolution

A relative local root may be resolved against a stable backend content/application root.

Rules:

- resolve once during startup/DI composition;
- store/use the normalized absolute path internally;
- do not expose it through API responses or health responses;
- create the directory when safe and required;
- fail clearly when the path cannot be created or accessed;
- do not place it under source-controlled content intended for publication.

Add the local storage directory to `.gitignore`, for example:

```txt
backend/.local-storage/
```

If existing ignore rules already cover it, do not add redundant entries.

### Environment safety

The local filesystem adapter is for development.

Prevent accidental production use.

Required behavior:

- `Provider=Local` is valid in the Development environment;
- application startup in Production must fail clearly if the only configured provider is `Local`;
- do not silently use local disk in Production;
- do not silently switch to an unconfigured provider;
- do not implement a fake no-op storage provider that returns success.

Production storage provider selection remains deferred to Unit 57 or an explicitly approved earlier infrastructure unit.

Staging behavior should follow the safest repository/environment convention. If no convention exists, treat non-Development use of `Local` as invalid and document the decision.

### Options validation

Validate on startup:

- provider is present;
- provider is recognized;
- local root is present for the Local provider;
- local root resolves safely;
- maximum size is positive;
- maximum size fits supported stream/file length types;
- local provider environment restrictions are satisfied.

Fail clearly without logging:

- secrets;
- full environment dumps;
- file contents.

Do not fall back to guessed defaults for missing required storage configuration after Unit 40 is implemented.

### Framework request limits

Do not globally configure ASP.NET multipart/form buffering or request-body limits in Unit 40 unless an existing API host limit would make future streaming storage impossible and the change can be made safely without exposing a generic upload endpoint.

Unit 41 and Unit 43 own their HTTP upload endpoint limits and feature-specific multipart handling.

The storage abstraction's actual-byte limit remains authoritative regardless of HTTP-layer limits.

Do not buffer future large video uploads through `IFormFile` by default merely because it is convenient.

### Authorization

Unit 40 exposes no product endpoints.

Therefore, it introduces no broad `CanUploadFiles` policy.

Future owning modules must authorize file operations according to their own rules:

- media permissions in Unit 41;
- import permissions/team scope in Unit 43;
- player image rules in a future player-profile feature.

`UploadedByUserId` must come from trusted current-user context, never from the client.

Do not interpret possession of a storage key as authorization.

### Error model

Use Application-safe storage error categories consistent with existing Result patterns.

At minimum distinguish:

- invalid storage key;
- empty content;
- object too large;
- object already exists;
- object not found;
- storage unavailable/I/O failure;
- operation cancelled.

Do not leak:

- absolute paths;
- operating-system exception details;
- machine names;
- filesystem permissions;
- stack traces.

Infrastructure logs may include:

- operation type;
- storage key;
- correlation ID;
- safe error category.

Do not log:

- file content;
- tokens;
- credentials;
- complete local paths in normal product logs.

### Clean Architecture boundaries

Domain owns:

- `StoredFile` metadata entity and archive invariants if consistent with current entity conventions;
- metadata value constraints that must hold regardless of provider.

Application owns:

- `IFileStorage`;
- provider-neutral requests/results;
- storage-key generation/validation;
- normalization helpers where appropriate;
- safe storage error contracts;
- orchestration guidance/contracts used by future feature use cases.

Infrastructure owns:

- `LocalFileStorage`;
- filesystem path resolution;
- stream copy implementation;
- temporary-file finalization;
- configuration binding/validation implementation;
- EF Core mapping;
- migration;
- dependency injection.

API owns:

- startup/DI composition;
- environment/configuration integration;
- no file product endpoint in this unit.

Do not put filesystem code in Domain or Application.

### EF Core persistence

Add `StoredFile` to the existing `AppDbContext`.

Configure:

- required fields;
- length bounds;
- unique index on `StorageKey`;
- positive-size database constraint where consistent with repository conventions;
- non-cascading user relationships;
- archive metadata consistency;
- useful indexes on `UploadedByUserId`, `CreatedAtUtc`, and archive state;
- no binary/blob content column.

Do not add generic linked-entity columns.

Do not add media/import tables.

### Migration

Create the migration through the approved `dotnet ef` workflow.

Keep files under:

```txt
backend/src/Infrastructure/Persistence/Migrations/
```

Use:

```txt
--output-dir Persistence/Migrations
```

Do not handwrite migration files unless the CLI workflow is genuinely blocked. Document any exception in `context/progress-tracker.md`.

### Tests

Add focused unit and integration tests.

#### Storage-key tests

Cover:

- generated key shape;
- uniqueness across many generated samples;
- no original filename/user data in key;
- rejection of rooted paths;
- rejection of `..`;
- rejection of backslashes;
- rejection of empty/invalid keys.

#### Filename normalization tests

Cover:

- plain filename;
- Windows submitted path;
- Unix submitted path;
- surrounding whitespace;
- empty filename;
- control characters;
- excessive length.

#### Local adapter tests

Use an isolated temporary root.

Cover:

- write and open round trip;
- actual byte count;
- input stream remains open;
- returned read stream ownership;
- nested directory creation;
- no overwrite;
- missing object;
- idempotent delete;
- path traversal rejection;
- rooted path rejection;
- partial write cleanup;
- cancellation cleanup;
- size limit early rejection;
- size limit streamed rejection;
- zero-byte rejection;
- exact limit acceptance;
- final file exists only after successful completion;
- no file written outside root.

Tests must not write into the repository's real `.local-storage` directory.

#### Configuration tests

Cover:

- valid Development Local configuration;
- missing provider;
- unknown provider;
- missing local root;
- invalid/non-positive maximum size;
- Production Local rejection;
- safe relative-root normalization;
- no path exposure in validation-facing errors where avoidable.

#### Metadata/domain tests

Cover:

- valid construction;
- positive size requirement;
- required normalized filename/content type/key;
- archive actor/time consistency;
- archive does not delete/change storage key;
- user relationship persistence;
- unique storage key;
- no cascade delete behavior.

#### Integration tests

Cover:

- EF Core migration/model;
- storing metadata without blob content;
- local storage write plus metadata persistence in a test orchestration;
- simulated metadata failure invokes compensation deletion;
- compensation failure is observable and does not return success;
- normal archive metadata leaves object bytes intact;
- no public file routes are mapped;
- no static-file exposure of local root.

Do not require a production object-storage provider.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

Record:

- selected contract names;
- local adapter behavior;
- configuration keys;
- development-only provider restriction;
- migration status;
- test root strategy;
- verification results;
- any filesystem/platform limitation.

If implementation reveals that the local adapter, metadata ownership model, or production-provider boundary must differ from this spec, update the relevant context before continuing.

Do not silently introduce a provider SDK or generic upload endpoint.

## Implementation

### 1. Add provider-neutral Application contracts

Create:

- storage write/open/delete abstraction;
- write request/result types;
- safe storage error/result types;
- stream ownership documentation;
- storage-key validation/generation.

Keep the contract independent of ASP.NET Core and provider SDKs.

### 2. Add filename and metadata validation

Implement:

- original filename normalization;
- content-type bounds;
- size invariants;
- storage-key constraints;
- reusable validation helpers required by future feature use cases.

Do not create media/import allowlists.

### 3. Add the `StoredFile` metadata entity

Implement:

- identity;
- key;
- original filename;
- content type;
- actual size;
- uploader;
- UTC creation timestamp;
- archive metadata;
- invariants.

Do not add binary content or polymorphic entity links.

### 4. Add EF Core mapping and migration

Configure:

- table/columns;
- unique storage key;
- positive size;
- non-cascading staff relationships;
- archive fields;
- indexes.

Generate/apply the migration through `dotnet ef`.

### 5. Add typed storage configuration

Add:

- provider;
- local root;
- maximum object size;
- startup validation;
- Development-only Local-provider enforcement.

Update `backend/.env.example` with safe values/comments.

Update `.gitignore` only when needed.

### 6. Implement the local filesystem adapter

Implement:

- normalized root;
- containment checks;
- asynchronous streaming;
- actual-byte counting;
- temporary-file write;
- final move without overwrite;
- cancellation/failure cleanup;
- open read;
- idempotent compensation delete.

Do not expose local paths.

### 7. Register storage infrastructure

Register the validated options and Local implementation through the existing Infrastructure/API dependency-injection composition.

Do not add a runtime provider factory with speculative providers.

A simple explicit provider selection for the currently supported `Local` value is sufficient.

### 8. Add focused tests

Add unit/integration coverage for:

- keys;
- filenames;
- metadata invariants;
- options;
- local I/O;
- limits;
- cleanup;
- persistence;
- compensation;
- route/static-file absence.

### 9. Update progress documentation

Update `context/progress-tracker.md` with actual implementation and verification state.

Do not mark Unit 40 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use built-in .NET stream and filesystem APIs plus the existing:

- ASP.NET Core configuration/options infrastructure;
- EF Core/Npgsql persistence;
- current-user identifier conventions;
- UTC clock abstraction;
- Result/error conventions;
- test infrastructure.

Do not add:

- AWS SDK;
- MinIO SDK;
- Azure Storage SDK;
- Google Cloud Storage SDK;
- file-type detection package;
- antivirus package;
- image/video processing package;
- resumable-upload package;
- generic retry package solely for storage;
- frontend dependencies.

## Verification checklist

- [ ] An Application-layer provider-neutral file-storage abstraction exists.
- [ ] The storage abstraction does not reference ASP.NET Core `IFormFile`.
- [ ] The storage abstraction does not expose local paths, bucket names, provider clients, or public URLs.
- [ ] Write, open-read, and compensation-delete operations are supported.
- [ ] Input/output stream ownership is explicitly documented and tested.
- [ ] Content is streamed rather than fully buffered.
- [ ] Actual bytes written are counted and returned.
- [ ] Zero-byte content is rejected.
- [ ] Declared and actual oversized content is rejected.
- [ ] Exact maximum-size content is accepted.
- [ ] Partial/cancelled writes leave no final object.
- [ ] Existing objects are never overwritten.
- [ ] Storage keys are server-generated and opaque.
- [ ] Storage keys contain no original filename, user identity, or business entity name.
- [ ] Rooted paths, traversal segments, and backslashes are rejected.
- [ ] The local adapter independently verifies full-path containment.
- [ ] The local adapter writes temporary content before finalization.
- [ ] Temporary files are cleaned after failure/cancellation.
- [ ] Local storage root is not served through static-file middleware.
- [ ] Local storage root is not `wwwroot` or the frontend public directory.
- [ ] Local storage directory is ignored by Git.
- [ ] `StoredFile` metadata exists in PostgreSQL.
- [ ] `StoredFile` contains no binary/blob content.
- [ ] `StorageKey` is unique.
- [ ] Original filename is normalized, safe, required, and bounded.
- [ ] Content type is required/bounded but not treated as proof of content.
- [ ] Persisted size uses actual written bytes.
- [ ] Uploader identity comes from trusted backend context in future consuming use cases.
- [ ] User relationships do not cascade-delete file metadata.
- [ ] Archive metadata does not physically delete object bytes.
- [ ] No generic linked-entity type/ID columns are added to `StoredFile`.
- [ ] No public generic upload/download/delete routes are added.
- [ ] No static possession-of-key authorization pattern is introduced.
- [ ] Future compensation workflow is documented and supported.
- [ ] Compensation delete is idempotent.
- [ ] Compensation failure is observable and never produces a false success.
- [ ] Typed storage options exist.
- [ ] Provider, local root, and max object size are validated at startup.
- [ ] `backend/.env.example` contains only safe Local-development storage examples.
- [ ] No real path, secret, credential, bucket, or provider key is committed.
- [ ] `Provider=Local` is rejected outside Development according to the safe environment rule.
- [ ] No production object-storage SDK or fake provider is added.
- [ ] No global multipart buffering behavior is introduced without a required endpoint.
- [ ] No media, import, player-image, thumbnail, or generated-report behavior is added.
- [ ] EF Core migration is generated under `backend/src/Infrastructure/Persistence/Migrations/`.
- [ ] `dotnet ef database update` succeeds when the local database environment is available.
- [ ] Storage-key tests pass.
- [ ] Filename-normalization tests pass.
- [ ] Local adapter round-trip, traversal, limit, collision, cancellation, and cleanup tests pass.
- [ ] Configuration validation tests pass.
- [ ] Metadata/domain tests pass.
- [ ] Integration tests verify metadata persistence and compensation behavior.
- [ ] Tests use isolated temporary storage roots and do not pollute the repository.
- [ ] No frontend files are changed.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for affected backend projects/solution.
- [ ] `dotnet test` passes for relevant backend test projects.
- [ ] `context/progress-tracker.md` records actual Unit 40 implementation, configuration, migration, and verification state.
