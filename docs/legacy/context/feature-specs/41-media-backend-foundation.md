# Unit 41: Media Backend Foundation

## Goal

Build the backend foundation for uploaded media assets and external media references using the Unit 40 storage abstraction. Provide a unified team-scoped media catalog, secure streamed upload/content access, explicit relational links to existing matches, match reports, and players, lifecycle operations, authorization, and semantic audit coverage without adding media UI, transcoding, thumbnails, public file URLs, training/import links, or production storage providers.

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
9. `context/feature-specs/23-staff-users-team-scope-account-lifecycle-backend.md`
10. `context/feature-specs/28-player-team-assignment-backend.md`
11. `context/feature-specs/30-matches-backend-foundation.md`
12. `context/feature-specs/32-match-report-workflow-backend.md`
13. `context/feature-specs/38-audit-backend-foundation.md`
14. `context/feature-specs/40-file-storage-abstraction.md`
15. `context/feature-specs/41-media-backend-foundation.md`

Use relevant project-local skills from `.agents/skills/` when applicable.

Skills/plugins may guide implementation workflow, but must not override the project context files, architecture rules, code standards, or this spec.

This unit is backend-only.

Do not add or change frontend routes, pages, components, navigation, shadcn/ui components, or frontend API wrappers.

### Required architecture synchronization

Unit 40 intentionally keeps `StoredFile` provider-neutral and does not place generic target fields such as:

```txt
LinkedEntityType
LinkedEntityId
```

on the stored-file record.

Before or alongside Unit 41 implementation, update the object/file-storage metadata example in `context/architecture.md` so it reflects the implemented design:

- `StoredFile` contains storage metadata only;
- owning modules reference `StoredFile` explicitly;
- media entity links use explicit relational link records;
- future import jobs reference their original stored files through explicit foreign keys.

Do not reintroduce polymorphic target columns on `StoredFile`.

Keep the production object-storage direction unchanged.

### Scope

This unit introduces:

- a unified persistent media identity;
- uploaded `MediaAsset` records backed by `StoredFile`;
- `ExternalMediaReference` records backed by validated HTTP/HTTPS URLs;
- shared title, description, team scope, category, lifecycle, creator, and timestamps;
- server-side media catalog list/detail queries;
- streamed multipart upload through the Unit 40 storage abstraction;
- secure authorized uploaded-content streaming;
- external-reference creation and correction;
- metadata correction;
- explicit relational links to:
  - matches;
  - match reports;
  - players;
- link/unlink history without hard-deleting link records;
- archive and restore lifecycle;
- role/team-scope authorization;
- report-workflow-aware attachment locks;
- semantic audit records for media mutations;
- EF Core mapping, migration, and tests.

This unit does not introduce:

- media frontend UI;
- a public or anonymous media library;
- static file serving;
- public object URLs;
- presigned URLs;
- thumbnails;
- image resizing;
- video transcoding;
- video scene detection;
- media processing jobs;
- playlists;
- annotations;
- comments;
- tags;
- full-text search infrastructure;
- training-session links;
- import-job links;
- generated-report links;
- player profile-image designation;
- media replacement/versioning;
- production S3/MinIO/Azure implementation;
- antivirus scanning;
- file-content sniffing;
- background orphan cleanup;
- hard deletion of media or stored files.

Training sessions and import jobs do not yet exist. Their explicit media/file relationships remain for their own later units.

### Unified media aggregate

Use one stable media identifier for catalog, lifecycle, links, and authorization.

Model a shared aggregate/root equivalent to:

```txt
MediaItem
```

with two explicit source variants:

```txt
MediaAsset
ExternalMediaReference
```

The exact EF inheritance/table strategy may follow the repository's established persistence conventions, but the domain/API invariants are required:

- every media item has exactly one source type;
- an uploaded media item references exactly one `StoredFile`;
- an external media item references exactly one validated URL;
- one media ID is used for list/detail/link/lifecycle operations;
- shared metadata and team scope are not duplicated inconsistently across source records;
- source type cannot change after creation.

A clean implementation may use:

- a shared `MediaItem` table plus one-to-one source-detail tables; or
- an equivalent constrained inheritance mapping.

Do not create six duplicated catalogs or unrelated IDs for the same conceptual item.

### Shared media fields

The shared media record contains at minimum:

```txt
Id
TeamId
SourceType
Category
Title
Description
CreatedByUserId
CreatedAtUtc
UpdatedAtUtc
IsArchived
ArchivedAtUtc
ArchivedByUserId
```

Use existing project identifier and UTC timestamp conventions.

Rules:

- `TeamId` is required;
- `TeamId` is immutable after creation;
- `SourceType` is required and immutable;
- `Category` is required;
- `Title` is required, trimmed, and bounded;
- `Description` is optional, trimmed, and bounded;
- creator comes from authenticated current-user context;
- archive actor/time are both present or both absent;
- no normal hard-delete behavior exists.

Recommended bounds unless an established repository convention already defines stricter values:

```txt
Title: 200 characters
Description: 2,000 characters
```

Do not add speculative fields such as:

- tactical labels;
- player shirt numbers;
- Zone14 IDs;
- Gpexe IDs;
- camera IDs;
- match-event timestamps;
- clip start/end time;
- thumbnail key;
- duration;
- resolution;
- codec;
- checksum.

### Media source types

Use stable internal values:

```txt
UPLOADED_FILE
EXTERNAL_REFERENCE
```

Do not localize persisted/API enum values.

An uploaded source owns:

```txt
StoredFileId
```

An external source owns:

```txt
Url
ProviderLabel
```

`ProviderLabel` is optional free text for operational identification, for example a known video platform or internal provider name.

Do not add provider-specific schemas.

Do not assume an external reference is downloadable or that the backend may fetch it.

### Media categories

Use a small V1 content category enum:

```txt
VIDEO
IMAGE
DOCUMENT
OTHER
```

Category describes intended presentation, not the source type.

Examples:

- uploaded MP4 -> `VIDEO`;
- external Zone14 page -> `VIDEO`;
- uploaded JPEG -> `IMAGE`;
- uploaded PDF -> `DOCUMENT`;
- an uncategorized external resource -> `OTHER`.

The backend validates uploaded content type/extension compatibility with the selected category.

For external references, category is chosen by the authorized user and is not verified by fetching the URL.

Do not add audio-specific, playlist, scene, or clip enums until a real product requirement exists.

### Media lifecycle

Use a simple lifecycle:

```txt
ACTIVE
ARCHIVED
```

The implementation may use a status enum or archive fields consistent with existing project conventions.

Allowed transitions:

```txt
ACTIVE -> ARCHIVED
ARCHIVED -> ACTIVE
```

Rules:

- lifecycle transitions use explicit methods/use cases;
- no direct endpoint assignment;
- archive and restore are `ADMIN` only;
- archive does not physically delete uploaded bytes;
- archive does not delete source details;
- archive does not delete link history;
- active links remain persisted but archived media is excluded from normal linked-entity queries;
- restore re-exposes the same active links;
- no processing/failed status is added because Unit 41 performs no asynchronous processing.

The broader architecture may leave room for future `PROCESSING`/`FAILED` states, but do not implement unused workflow states now.

### Uploaded media source

`MediaAsset` references one Unit 40 `StoredFile`.

Rules:

- `StoredFileId` is required and unique for media ownership;
- the same stored file cannot back multiple media assets;
- file bytes are immutable;
- original filename, content type, size, uploader, and storage key remain authoritative on `StoredFile`;
- media API read models may expose safe file metadata but never the storage key;
- replacing uploaded bytes is out of scope;
- correcting a wrong upload requires creating a new media item and archiving the old one;
- archive/restore of the media item does not physically delete or recreate stored bytes.

Do not store a second copy of original filename, content type, or file size on the media item unless a denormalized projection is proven necessary later.

### External media reference

`ExternalMediaReference` contains:

```txt
Url
ProviderLabel
```

URL rules:

- required;
- trimmed;
- absolute URI;
- scheme must be `http` or `https`;
- reject `javascript`, `data`, `file`, and other schemes;
- reject embedded username/password credentials;
- bounded to a reasonable URL limit such as 2,048 characters;
- stored in normalized absolute form without fetching the target;
- backend does not follow redirects;
- backend does not validate availability;
- backend does not download or mirror the external content.

`ProviderLabel`:

- optional;
- trimmed;
- bounded, for example 100 characters;
- not used for authorization;
- not used to choose integration behavior.

The external URL may be corrected through the metadata update use case while the item is active and the caller has mutation access.

### Uploaded file allowlist

Unit 41 owns the first feature-specific upload allowlist.

Support these initial file types:

#### Video

```txt
.mp4  -> video/mp4
.webm -> video/webm
.mov  -> video/quicktime
```

#### Image

```txt
.jpg / .jpeg -> image/jpeg
.png         -> image/png
.webp        -> image/webp
```

#### Document

```txt
.pdf -> application/pdf
```

Rules:

- extension comparison is case-insensitive;
- content type comparison is case-insensitive after normalization;
- extension and declared content type must form an approved pair;
- the selected category must match the approved pair;
- `OTHER` is not accepted for uploaded files in Unit 41;
- arbitrary binary uploads are rejected;
- no executable, archive, office document, SVG, HTML, or script upload is accepted;
- content type/extension validation is not claimed to be content sniffing;
- antivirus and deep file inspection remain out of scope.

Future import source files use Unit 43's own CSV/XLSX allowlist and must not be uploaded through the media endpoint merely to bypass import rules.

### Media upload size configuration

Add typed media options such as:

```txt
MediaOptions
```

Recommended configuration:

```txt
Media__MaxUploadSizeBytes
```

Rules:

- value is positive;
- value must be less than or equal to Unit 40 `FileStorage__MaxObjectSizeBytes`;
- declared upload size and actual streamed bytes must both respect the limit;
- zero-byte files are rejected;
- exact configured maximum is allowed;
- the limit is a media endpoint ceiling, not an import-file limit;
- no hardcoded browser-visible value is the source of truth.

Update `backend/.env.example` with a safe development example and comment.

Do not add separate guessed video/image/document limits in Unit 41.

### Streamed multipart upload contract

Add a media-specific upload endpoint using streaming multipart parsing.

Required route:

```txt
POST /api/media/assets
```

The request contains exactly:

- one bounded metadata JSON section;
- one file section.

The metadata contains:

```txt
teamId
category
title
description
```

Rules:

- require a supported multipart content type;
- require exactly one metadata section and one file section;
- bound metadata section size, for example 64 KiB;
- reject extra file sections;
- reject duplicate metadata sections;
- validate and authorize metadata before committing the file as a media item;
- do not buffer the entire uploaded file;
- do not use `ReadToEnd`, Base64, or a full byte array;
- do not depend on `IFormFile` buffering for large video uploads;
- use Unit 40 streaming write behavior;
- actual bytes written are authoritative;
- original filename is normalized through Unit 40 rules;
- client does not provide a storage key;
- mutation requires CSRF protection under the existing cookie-authenticated API rules.

A deterministic contract may require the metadata section to appear before the file section so authorization and business validation complete before storage begins. If so, reject reversed order with a clear validation error and document the request format.

Do not silently buffer an early file section to work around invalid ordering.

### Upload orchestration and compensation

The upload use case must follow Unit 40's compensation pattern:

1. authenticate and authorize;
2. validate team, metadata, category, filename, content type, and declared size;
3. generate a trusted storage key;
4. stream the file through `IFileStorage`;
5. begin/use the database transaction;
6. create `StoredFile`;
7. create shared media record and `MediaAsset`;
8. write the semantic audit event;
9. commit;
10. if database/audit persistence fails, call `DeleteIfExistsAsync` for the uncommitted object.

Rules:

- do not return success until object write and database commit both succeed;
- if compensation fails, log the safe storage key/correlation context and still return failure;
- do not include file bytes or absolute local paths in errors/logs;
- do not delete a committed object when the HTTP response later fails;
- do not physically delete an old object during metadata correction/archive.

### External-reference creation

Add:

```txt
POST /api/media/external-references
```

Request fields:

```txt
teamId
category
title
description
url
providerLabel
```

Rules:

- no stored-file record is created;
- no outbound HTTP request occurs;
- URL validation uses the rules above;
- caller cannot choose media ID;
- caller cannot create an archived item directly;
- item starts active;
- creation and audit record commit atomically.

### Unified media list API

Add:

```txt
GET /api/media
```

Support server-side filters and bounded pagination:

```txt
teamId
sourceType
category
search
matchId
matchReportId
playerId
includeArchived
page
pageSize
```

Rules:

- default excludes archived items;
- `includeArchived=true` is `ADMIN` only;
- results are always team-scope restricted;
- explicit unauthorized `teamId` returns `403`;
- target filters validate target visibility and team scope;
- multiple target filters in one request are rejected unless the implementation explicitly defines AND semantics;
- search is bounded and normalized;
- search may cover title, description, original filename, and provider label;
- do not search file bytes;
- do not load all media into memory;
- deterministic ordering uses newest creation time then ID;
- archived items never appear to non-admin users;
- links that were unlinked are excluded from normal target filtering.

Return compact catalog items containing at minimum:

```txt
id
team summary
sourceType
category
title
description
source summary
createdBy summary
createdAtUtc
updatedAtUtc
archive state
active link counts/summaries as needed
```

For uploaded source summary:

```txt
originalFileName
contentType
sizeBytes
```

For external source summary:

```txt
url
providerLabel
```

Never return `StorageKey` or absolute path.

### Media detail API

Add:

```txt
GET /api/media/{mediaId}
```

Return:

- shared metadata;
- safe source details;
- team summary;
- creator summary;
- archive metadata when authorized;
- active match links;
- active match-report links;
- active player links.

Do not return unlinked historical link records through the normal detail endpoint.

Do not return raw stored-file storage key.

For an archived item:

- `ADMIN` may read detail;
- non-admin users receive safe inaccessible/missing behavior.

### Uploaded-content endpoint

Add:

```txt
GET /api/media/{mediaId}/content
```

Optional query:

```txt
download=true|false
```

Rules:

- uploaded assets only;
- external references do not redirect through this endpoint;
- authenticate and authorize before opening storage;
- active team-scoped media is readable by authorized staff;
- archived content is readable only by `ADMIN`;
- resolve content type and original filename from `StoredFile`;
- stream through the Unit 40 abstraction;
- never expose storage key or local path;
- set safe `Content-Disposition`;
- use inline disposition by default for approved image/video/PDF types;
- use attachment disposition when `download=true`;
- include `X-Content-Type-Options: nosniff`;
- enable framework range processing when the returned stream/provider supports seeking/ranges;
- otherwise stream normally without buffering the full file;
- missing object after valid metadata returns a safe storage-unavailable/missing response and is logged for operational investigation;
- do not convert file content to JSON/Base64.

Do not add a permanently public URL.

### Metadata update

Add:

```txt
PATCH /api/media/{mediaId}
```

Editable shared fields:

```txt
title
description
category
```

External-reference-only editable fields:

```txt
url
providerLabel
```

Immutable fields:

```txt
id
teamId
sourceType
storedFileId
createdByUserId
createdAtUtc
uploaded bytes
original filename
content type
size
```

Rules:

- item must be active;
- `ADMIN` or authorized in-scope `DATA_OPERATOR`;
- category correction for uploaded media must remain compatible with the existing file type;
- external URL uses full validation;
- no semantic no-op audit record;
- changes and audit commit atomically;
- no file replacement occurs.

### Explicit entity links

Create explicit relational link records for the currently implemented targets:

```txt
MediaMatchLink
MediaMatchReportLink
MediaPlayerLink
```

Each link record contains at minimum:

```txt
Id
MediaItemId
TargetId
LinkedByUserId
LinkedAtUtc
UnlinkedByUserId
UnlinkedAtUtc
```

Rules:

- an active link has no unlink actor/time;
- unlink actor/time are both present or absent;
- links are not hard-deleted through product APIs;
- a previously unlinked item may be linked again, creating a new link-history record;
- only one active link may exist for the same media/target pair;
- use database constraints/indexes appropriate for PostgreSQL to enforce active-link uniqueness;
- target foreign keys are explicit and non-cascading;
- archived media cannot gain new links;
- existing active links remain stored when media is archived;
- archived target entities cannot receive new links;
- target hard deletion is not expected and must not cascade-delete media link history.

Do not use a generic `TargetType + TargetId` table without relational foreign keys.

### Match linking

Add explicit operations equivalent to:

```txt
POST   /api/media/{mediaId}/matches/{matchId}
DELETE /api/media/{mediaId}/matches/{matchId}
```

`DELETE` semantically unlinks by setting unlink metadata. It does not hard-delete the link row.

Validation:

- media and match exist and are active/not archived;
- media `TeamId` equals match `TeamId`;
- caller has media mutation access for that team;
- cancelled/archived matches cannot receive new links;
- if the match has a report in `READY_FOR_REVIEW`, `VERIFIED`, or `ARCHIVED`, link and unlink are workflow-locked;
- no-report, `DRAFT`, and `NEEDS_CORRECTION` states allow authorized changes;
- exact active duplicate returns `409`;
- unlinking a missing/inactive relationship follows the established idempotency or not-found convention consistently.

Use the Unit 32 report-workflow guard/service rather than duplicating status logic in endpoint handlers.

### Match-report linking

Add explicit operations equivalent to:

```txt
POST   /api/media/{mediaId}/match-reports/{reportId}
DELETE /api/media/{mediaId}/match-reports/{reportId}
```

Validation:

- media and report exist;
- media `TeamId` equals the report match's `TeamId`;
- caller has mutation access;
- report must expose/permit `EDIT` under Unit 32 rules;
- normally allowed in `DRAFT` and `NEEDS_CORRECTION`;
- locked in `READY_FOR_REVIEW`, `VERIFIED`, and `ARCHIVED`;
- archived report cannot receive links;
- exact active duplicate returns `409`.

Do not allow media attachment to bypass report workflow locking.

### Player linking

Add explicit operations equivalent to:

```txt
POST   /api/media/{mediaId}/players/{playerId}
DELETE /api/media/{mediaId}/players/{playerId}
```

Validation:

- media exists and is active;
- player exists and is not archived;
- the player has at least one assignment to `MediaItem.TeamId`;
- the assignment may be historical, current, or future because the media link represents club/team development context rather than match-date eligibility;
- caller has media mutation access for the media team;
- exact active duplicate returns `409`.

Do not use only the player's current assignment.

Do not attach a player who has never belonged to the media item's team.

Existing links remain readable if the player is later archived or assignments change, subject to media visibility.

### Authorization model

Backend authorization is the source of truth.

#### Read active media

Authenticated active staff may read active media only for teams within their authorized scope:

- `ADMIN`: all teams;
- `ALL_TEAMS`: all teams;
- `SELECTED_TEAMS`: selected teams only.

The same rules apply to list, detail, and uploaded-content access.

#### Create/update/link/unlink

Allowed:

- `ADMIN` for any team;
- `DATA_OPERATOR` only for teams inside their authorized scope.

Not allowed by default:

- `ANALYST`;
- `COACH`;
- `MEDICAL_STAFF`;
- `VIEWER`.

Report/match workflow locks still apply after role/team authorization passes.

#### Archive/restore

`ADMIN` only.

#### Archived media reads

`ADMIN` only.

When a caller explicitly targets an out-of-scope team during create/list mutation, return `403`.

For inaccessible existing media/detail/content targets, use the established safe non-disclosure behavior.

Do not trust frontend action visibility.

### Archive and restore endpoints

Add:

```txt
POST /api/media/{mediaId}/archive
POST /api/media/{mediaId}/restore
```

Rules:

- `ADMIN` only;
- explicit lifecycle methods;
- archive does not delete stored bytes;
- archive does not unlink targets;
- archive hides the item from normal lists and linked-entity queries;
- restore reactivates the same item and active links;
- repeated/invalid transitions return a consistent conflict/idempotent result according to existing lifecycle conventions;
- source details remain unchanged;
- operations and audit records commit atomically.

Do not expose hard delete.

### Audit integration

Extend Unit 38 with:

```txt
EntityType = MEDIA_ITEM
```

Add centralized semantic action codes:

```txt
MEDIA_ITEM_CREATED
MEDIA_ITEM_UPDATED
MEDIA_ITEM_LINKED
MEDIA_ITEM_UNLINKED
MEDIA_ITEM_ARCHIVED
MEDIA_ITEM_RESTORED
```

Audit successful committed actions only.

Do not audit:

- rejected authorization;
- invalid upload;
- storage write failure;
- failed database transaction;
- duplicate link conflict;
- no-op metadata update;
- failed workflow lock.

Safe audit payloads may include:

```txt
teamId
sourceType
category
title
description
originalFileName
contentType
sizeBytes
providerLabel
targetType
targetId
archive status
```

Do not include:

- storage key;
- local path;
- file bytes;
- multipart body;
- uploaded stream;
- external URL query string or fragment;
- credentials;
- cookies;
- CSRF tokens.

For external URL changes, record a safe summary such as:

```txt
urlChanged = true
previousHost
newHost
```

Do not duplicate a potentially token-bearing full URL into audit JSON.

For upload creation:

- object write occurs first;
- stored file, media item, and audit commit together;
- database/audit failure triggers Unit 40 compensation deletion.

For metadata/link/lifecycle mutations:

- business change and audit commit in the same database transaction.

Add an entity-specific history endpoint:

```txt
GET /api/media/{mediaId}/audit
```

Authorization matches media detail:

- active media history is team-scope readable;
- archived media history is `ADMIN` only;
- safe inaccessible behavior;
- bounded pagination and optional action/date filters;
- structured Unit 38 response shape;
- no mutation endpoint.

Media audit UI remains out of scope for Unit 41 and Unit 42 unless Unit 42 explicitly adds it.

### Unified API shape

Required endpoint surface:

```txt
GET    /api/media
GET    /api/media/{mediaId}
GET    /api/media/{mediaId}/content
GET    /api/media/{mediaId}/audit

POST   /api/media/assets
POST   /api/media/external-references
PATCH  /api/media/{mediaId}

POST   /api/media/{mediaId}/matches/{matchId}
DELETE /api/media/{mediaId}/matches/{matchId}

POST   /api/media/{mediaId}/match-reports/{reportId}
DELETE /api/media/{mediaId}/match-reports/{reportId}

POST   /api/media/{mediaId}/players/{playerId}
DELETE /api/media/{mediaId}/players/{playerId}

POST   /api/media/{mediaId}/archive
POST   /api/media/{mediaId}/restore
```

Exact endpoint-group organization may follow repository conventions.

Do not add generic stored-file routes.

Do not add direct storage-key routes.

### Error handling

Use existing Result and ProblemDetails conventions.

Expected behavior:

- `400` for malformed multipart/request syntax;
- `401` for unauthenticated requests;
- `403` for explicit out-of-scope team creation/filter/mutation attempts;
- `404` for missing or safely inaccessible media/targets/content;
- `409` for duplicate links, lifecycle conflicts, workflow locks, immutable/source conflicts, or storage object collision;
- `413 Payload Too Large` when the HTTP/upload limit is exceeded where practical;
- `415 Unsupported Media Type` for unsupported multipart/file content types;
- `422` for semantic metadata/URL/file-pair validation when that is the established convention;
- safe `5xx` ProblemDetails for storage/database failures.

Do not leak:

- storage keys;
- local paths;
- database details;
- OS exceptions;
- authorization internals;
- full external URLs in logs/errors;
- file content.

### Clean Architecture boundaries

Domain owns:

- shared media aggregate/root;
- source-type/category/lifecycle values;
- uploaded/external source invariants;
- archive/restore behavior;
- explicit link lifecycle invariants.

Application owns:

- media list/detail queries;
- upload orchestration;
- external-reference creation;
- metadata update;
- link/unlink use cases;
- archive/restore use cases;
- file/category validation;
- URL validation;
- authorization/team-scope orchestration;
- report-workflow guards;
- audit payload construction;
- transaction/compensation orchestration.

Infrastructure owns:

- EF Core mappings;
- database queries;
- multipart infrastructure helper only when API/framework-specific;
- Unit 40 storage implementation usage;
- migration.

API owns:

- endpoint mapping;
- multipart HTTP parsing/composition;
- request/response mapping;
- safe streaming response;
- CSRF/auth policy application;
- ProblemDetails mapping.

Do not place business rules, target authorization, storage-key generation, or EF queries in endpoint handlers.

### EF Core persistence

Add the shared media/source/link model to `AppDbContext`.

Configure:

- required/optional fields;
- enum persistence consistent with repository conventions;
- one uploaded source per media item;
- one external source per media item;
- exactly one source variant invariant through domain and database constraints where practical;
- unique `StoredFileId` ownership;
- explicit foreign keys to team, stored file, match, match report, player, and staff users;
- non-cascading historical relationships;
- archive metadata;
- active-link uniqueness;
- URL/title/description/provider bounds;
- useful list/filter indexes.

Practical indexes include:

```txt
MediaItem(TeamId, IsArchived, CreatedAtUtc)
MediaItem(SourceType, Category, CreatedAtUtc)
MediaAsset(StoredFileId unique)
ExternalMediaReference(Url or normalized URL hash only if needed; no uniqueness required)
MediaMatchLink(MatchId, UnlinkedAtUtc)
MediaMatchReportLink(MatchReportId, UnlinkedAtUtc)
MediaPlayerLink(PlayerId, UnlinkedAtUtc)
```

Do not require external URL uniqueness. The same resource may legitimately be referenced in different team contexts or with different operational titles.

Use PostgreSQL partial unique indexes where appropriate for active link pairs.

### Migration

Create the migration through the approved `dotnet ef` workflow.

Keep migration files under:

```txt
backend/src/Infrastructure/Persistence/Migrations/
```

Use:

```txt
--output-dir Persistence/Migrations
```

Do not handwrite migration files unless tooling is genuinely blocked. Document any exception in `context/progress-tracker.md`.

### Tests

Add focused domain/unit, application, storage-orchestration, and integration tests.

#### Domain/unit tests

Cover:

- shared media construction;
- exactly one source variant;
- required immutable team/source;
- title/description bounds;
- category values;
- archive/restore transitions;
- external URL scheme/credential validation;
- provider-label normalization;
- uploaded category/file-pair validation;
- link/unlink lifecycle;
- duplicate active-link conflict behavior;
- unlink actor/time consistency.

#### Upload tests

Cover:

- admin upload;
- in-scope data-operator upload;
- out-of-scope data-operator rejection before committed media creation;
- read-only role rejection;
- supported video/image/PDF pairs;
- unsupported extension;
- unsupported content type;
- mismatched extension/content type/category;
- missing/duplicate metadata part;
- missing/multiple file parts;
- invalid multipart order when metadata-first is required;
- bounded metadata part;
- zero-byte file;
- declared oversize;
- streamed oversize;
- exact limit;
- storage collision;
- storage failure;
- database failure compensation;
- audit failure compensation;
- compensation failure observability;
- no storage key/path in response/error;
- CSRF enforcement for mutation.

#### External-reference tests

Cover:

- valid HTTP/HTTPS URL;
- invalid relative URL;
- rejected schemes;
- embedded credentials;
- URL length;
- provider label;
- no outbound HTTP request;
- admin/in-scope authorization;
- atomic audit creation.

#### List/detail/content tests

Cover:

- team-scope filtering;
- explicit unauthorized team filter;
- source/category/search filters;
- match/report/player target filters;
- target visibility validation;
- archive exclusion;
- admin `includeArchived`;
- safe detail access;
- no storage key in DTOs;
- uploaded content round trip;
- external item content endpoint rejection;
- `download` disposition;
- `nosniff`;
- archived content admin-only;
- missing physical object handling;
- range behavior where supported;
- bounded pagination and deterministic ordering.

#### Link tests

Cover:

- match team equality;
- report match team equality;
- player any-assignment-to-team eligibility;
- player current-only logic is not used;
- archived target rejection;
- duplicate active link;
- unlink and re-link history;
- non-cascading link history;
- admin/data-operator authorization;
- read-only role rejection;
- report `DRAFT`/`NEEDS_CORRECTION` link editing;
- `READY_FOR_REVIEW`/`VERIFIED`/`ARCHIVED` locks;
- match link lock when an existing report is locked;
- audit creation for link/unlink;
- rejected/failed link creates no audit.

#### Lifecycle tests

Cover:

- admin archive/restore;
- non-admin rejection;
- bytes remain after archive;
- active links remain persisted;
- archived hidden from normal queries;
- restore re-exposes active links;
- archived non-admin detail/content denial;
- audit records;
- no hard-delete endpoint.

#### Media audit tests

Cover:

- action/entity codes;
- safe payloads;
- no storage key;
- no full token-bearing URL;
- successful mutation only;
- no-op update creates no event;
- history authorization;
- pagination/filtering;
- archived history admin-only.

### Documentation synchronization

Update `context/progress-tracker.md` after meaningful implementation changes.

Record:

- media model/table strategy;
- source/category/lifecycle enums;
- upload allowlist;
- size configuration;
- multipart contract/order;
- explicit link targets;
- workflow-lock behavior;
- audit action codes;
- migration status;
- storage compensation verification;
- intentionally deferred media processing/provider/UI features.

If implementation reveals that the Unit 40 storage contract cannot support secure streaming or compensation as specified, update the relevant architecture/context before continuing.

Do not silently add generic polymorphic links, provider SDKs, or media processing.

## Implementation

### 1. Synchronize storage architecture documentation

Update `context/architecture.md` to remove the obsolete implication that `StoredFile` owns generic linked entity type/ID fields.

Document explicit owning-module references and media link records.

### 2. Add media domain model

Create:

- shared media root;
- uploaded source;
- external source;
- source type;
- category;
- lifecycle;
- explicit match/report/player link records;
- invariants and transitions.

Keep team and source immutable.

### 3. Add media options and validation

Add:

- media upload size option;
- startup validation against Unit 40 global maximum;
- uploaded allowlist;
- filename/content-type/category validation;
- external URL/provider validation;
- metadata bounds.

Update `backend/.env.example`.

### 4. Add persistence and migration

Configure the shared/source/link tables, constraints, relationships, partial indexes, and non-cascading behavior.

Generate and apply the migration.

### 5. Implement streamed uploaded-media creation

Add the streaming multipart endpoint and Application use case.

Integrate:

- authorization;
- storage key generation;
- Unit 40 write;
- actual size;
- `StoredFile`;
- media/source persistence;
- audit;
- compensation.

### 6. Implement external-reference creation

Add the validated JSON endpoint and atomic media/audit persistence.

Do not fetch the URL.

### 7. Implement list, detail, and content reads

Add:

- server-side catalog filters/pagination;
- team-scope enforcement;
- source-aware DTOs;
- safe detail;
- authorized file streaming;
- safe headers/range behavior.

Never expose storage keys.

### 8. Implement metadata update

Add active-item metadata correction with source-specific validation and no-op detection.

Keep immutable fields protected.

### 9. Implement explicit entity linking

Add match, report, and player link/unlink use cases and endpoints.

Reuse:

- match/report team context;
- Unit 32 workflow guards;
- player assignment history;
- team-scope authorization.

Persist unlink history rather than deleting link rows.

### 10. Implement archive and restore

Add administrator-only lifecycle operations.

Preserve bytes, source details, and link history.

### 11. Extend audit foundation

Add:

- `MEDIA_ITEM`;
- media action codes;
- safe payload builders;
- entity-specific media history query/endpoint;
- atomic integration in media mutations.

Do not expose raw URLs/storage details.

### 12. Add tests

Implement all required unit/integration coverage, including real local-storage compensation in isolated test roots.

### 13. Update progress documentation

Update `context/progress-tracker.md` with actual implementation and verification results.

Do not mark Unit 41 complete until required checks pass or failures are explicitly documented.

## Dependencies

None.

Use existing packages and infrastructure:

- ASP.NET Core 8;
- built-in streaming/multipart APIs;
- EF Core/Npgsql;
- FluentValidation;
- Unit 40 file-storage abstraction;
- Unit 38 audit foundation;
- current user/team-scope authorization;
- existing Result/ProblemDetails/test infrastructure.

Do not add:

- S3/MinIO/Azure SDKs;
- multipart upload libraries unless the built-in framework cannot meet the requirement and the need is documented first;
- MIME-sniffing packages;
- antivirus packages;
- FFmpeg/video-processing packages;
- image-processing packages;
- URL-preview/scraping packages;
- background job packages;
- frontend dependencies.

## Verification checklist

- [ ] `context/architecture.md` no longer places generic linked entity type/ID fields on `StoredFile`.
- [ ] A unified media identity exists with explicit uploaded and external source variants.
- [ ] Every media item has exactly one source type.
- [ ] `TeamId` and source type are immutable.
- [ ] Shared title, description, category, creator, timestamps, and archive lifecycle exist.
- [ ] Uploaded media references one unique `StoredFile`.
- [ ] External media references store one validated HTTP/HTTPS URL.
- [ ] Embedded URL credentials and unsafe schemes are rejected.
- [ ] The backend does not fetch external URLs.
- [ ] Media categories are `VIDEO`, `IMAGE`, `DOCUMENT`, and `OTHER`.
- [ ] Uploaded `OTHER` files are rejected in Unit 41.
- [ ] Initial video/image/PDF extension and content-type pairs are enforced.
- [ ] Arbitrary executable/archive/HTML/SVG/office uploads are rejected.
- [ ] Media upload size is configured and cannot exceed Unit 40's global storage maximum.
- [ ] Streamed multipart upload does not buffer the entire file or use Base64.
- [ ] Multipart metadata/file cardinality and ordering rules are enforced.
- [ ] Original filename is normalized and never used as a storage path.
- [ ] Client never supplies a storage key.
- [ ] Actual bytes written are persisted as the file size.
- [ ] Upload success requires both object write and database/audit commit.
- [ ] Database or audit failure triggers compensation deletion.
- [ ] Compensation failure is observable and never returns false success.
- [ ] `GET /api/media` supports bounded server-side filters and pagination.
- [ ] Team scope is applied to all media list queries.
- [ ] Explicit unauthorized `teamId` returns `403`.
- [ ] Target filters validate match/report/player visibility and scope.
- [ ] Default media lists exclude archived items.
- [ ] Only admins may include archived items.
- [ ] Media detail returns safe source/link summaries.
- [ ] Media DTOs never expose storage keys or absolute paths.
- [ ] Uploaded content is available only through the authorized media content endpoint.
- [ ] External references do not use the uploaded-content endpoint.
- [ ] Content responses use safe disposition and `nosniff`.
- [ ] Range processing is enabled where supported without full buffering.
- [ ] Archived content is admin-only.
- [ ] Metadata update cannot change team, source, stored file, or uploaded bytes.
- [ ] Uploaded category remains compatible with the stored file type.
- [ ] No-op updates create no audit event.
- [ ] Explicit relational match links exist.
- [ ] Explicit relational match-report links exist.
- [ ] Explicit relational player links exist.
- [ ] No generic polymorphic target table is used.
- [ ] Link records preserve linked/unlinked actor and UTC time.
- [ ] Product unlink does not hard-delete link history.
- [ ] Only one active link exists per media/target pair.
- [ ] Re-linking after unlink creates valid new history.
- [ ] Match/report links require team equality.
- [ ] Player links require at least one assignment to the media team, not current assignment only.
- [ ] Archived targets cannot receive new links.
- [ ] Report links respect Unit 32 `EDIT`/workflow locks.
- [ ] Match links are locked when an existing report is in a locked state.
- [ ] `ADMIN` can create/update/link/unlink across teams.
- [ ] In-scope `DATA_OPERATOR` can create/update/link/unlink.
- [ ] Other roles are read-only.
- [ ] Read access is team-scope aware for all active staff.
- [ ] Archive and restore are admin-only explicit transitions.
- [ ] Archive does not physically delete bytes or links.
- [ ] Restore re-exposes the same active links.
- [ ] No hard-delete media or stored-file endpoint exists.
- [ ] `MEDIA_ITEM` audit entity type exists.
- [ ] All required media action codes exist.
- [ ] Media audit payloads exclude storage keys, bytes, paths, and full token-bearing URLs.
- [ ] Media mutations and audit entries commit atomically.
- [ ] Failed/rejected mutations create no audit entry.
- [ ] `GET /api/media/{mediaId}/audit` exists with media visibility rules.
- [ ] No media frontend UI is added.
- [ ] No thumbnail, processing, transcoding, playlist, annotation, or scene model is added.
- [ ] No training-session/import-job/generated-report media links are added prematurely.
- [ ] No production storage provider SDK is added.
- [ ] EF Core migration is generated under `backend/src/Infrastructure/Persistence/Migrations/`.
- [ ] `dotnet ef database update` succeeds when the local database environment is available.
- [ ] Domain/unit tests pass.
- [ ] Upload/storage-compensation tests pass using isolated storage roots.
- [ ] Authorization/team-scope tests pass.
- [ ] Link/workflow-lock tests pass.
- [ ] Lifecycle/content tests pass.
- [ ] Audit tests pass.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for affected backend projects/solution.
- [ ] `dotnet test` passes for relevant backend test projects.
- [ ] `context/progress-tracker.md` reflects actual Unit 41 implementation, migration, audit, storage, and verification state.
