# Unit 57: Production Configuration Readiness

## Goal

Prepare the Player Performance Data System for a controlled production deployment without choosing a hosting provider, container platform, email vendor, database vendor, or object-storage vendor that has not been approved.

Add a production configuration profile and startup validation, explicit public-origin/deployment-topology rules, trusted reverse-proxy handling, secure cookie/CORS/CSRF production checks, persistent ASP.NET Core Data Protection requirements, production health endpoints, safe build/runtime metadata, built-in rate-limiting policies for sensitive endpoints, production logging and error behavior, frontend production-environment validation, database migration and rollback runbooks, backup/restore requirements, first-admin and invitation operational procedures, static-frontend/security-header guidance, object-storage provider decision and implementation gates, release smoke tests, and a signed production-readiness checklist.

Do not select or integrate a production hosting provider or object-storage SDK in this unit. Do not introduce Docker, CI/CD, Kubernetes, a managed secrets product, an email provider, monitoring vendor, CDN, or cloud-specific infrastructure without an explicit approved decision.

Production deployment must remain marked `BLOCKED` while any mandatory infrastructure decision—especially the production object-storage provider—is unresolved.

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
8. `context/feature-specs/07-backend-configuration-baseline.md`
9. `context/feature-specs/12-backend-persistence-foundation.md`
10. `context/feature-specs/14-backend-auth-identity-foundation.md`
11. `context/feature-specs/15-backend-csrf-session-api.md`
12. `context/feature-specs/17-first-admin-bootstrap.md`
13. `context/feature-specs/18-backend-login-required-password-change.md`
14. `context/feature-specs/40-file-storage-abstraction.md`
15. `context/feature-specs/41-media-backend-foundation.md`
16. `context/feature-specs/43-import-workflow-backend-foundation.md`
17. `context/feature-specs/45-generic-csv-xlsx-parsing-foundation.md`
18. `context/feature-specs/50-medical-availability-backend.md`
19. `context/feature-specs/52-dashboard-backend-read-models.md`
20. `context/feature-specs/56-responsive-accessibility-hardening.md`
21. `context/feature-specs/57-production-configuration-readiness.md`

Use relevant project-local skills from `.agents/skills/` when applicable. Skills may guide the workflow but must not override project context, this active specification, implemented contracts, or architecture boundaries.

This is a backend-heavy full-stack configuration and operations unit.

Do not change:

- domain behavior;
- role, permission, or team-scope semantics;
- match/report/import/medical/workload workflows;
- API response contracts except production health/build metadata specifically defined here;
- file metadata ownership;
- object-storage abstraction;
- Bosnian-only UI decision;
- database schema unless implementation proves a small operational table is absolutely necessary and the spec is updated first.

No EF Core migration is expected.

### Production readiness is not provider selection

Unit 57 distinguishes:

```txt
Application production-readiness
Infrastructure decision readiness
Deployment execution
```

#### Application production-readiness

Can be completed in this unit:

- validated production configuration;
- safe startup behavior;
- proxy/origin/cookie rules;
- health endpoints;
- logging;
- migration policy;
- operational documentation;
- release checklist;
- deployment smoke tests.

#### Infrastructure decision readiness

Must be explicitly approved outside assumptions:

- hosting provider;
- PostgreSQL provider;
- object-storage provider;
- TLS/DNS/reverse-proxy topology;
- email delivery mode/provider;
- backup retention;
- recovery objectives;
- single-instance or multi-instance backend;
- Docker/container usage;
- monitoring/alerting destination.

#### Deployment execution

Occurs only after mandatory decisions and credentials exist.

Unit 57 must not report:

```txt
Production ready
```

when a mandatory decision is unresolved.

Use readiness statuses:

```txt
READY
BLOCKED
ACCEPTED_LIMITATION
NOT_APPLICABLE
```

An `ACCEPTED_LIMITATION` requires:

- named owner;
- approval date;
- operational impact;
- expiry/review date;
- fallback procedure.

Security, medical privacy, secret handling, database backup, TLS, or production file-storage blockers cannot be waived casually.

### Scope

This unit introduces:

- a centralized production configuration inventory;
- strongly typed deployment/public-origin options;
- production-only startup validation;
- frontend production environment validation;
- exact CORS origin configuration;
- secure production cookie validation;
- trusted proxy/network configuration;
- HTTPS/HSTS behavior;
- persistent Data Protection key requirements;
- production health endpoints:
  - liveness;
  - readiness;
- database readiness and pending-migration checks;
- file-storage readiness integration point;
- temporary-working-directory readiness checks;
- safe build/version metadata;
- production OpenAPI/detailed-error restrictions;
- structured production logging rules;
- correlation/trace behavior;
- built-in endpoint rate limiting for authentication and expensive mutation entry points;
- release-time database migration policy;
- database backup/restore runbook requirements;
- object-storage decision record and production adapter gate;
- frontend static-hosting/cache/security-header requirements;
- first-admin bootstrap procedure;
- invitation/setup-link delivery procedure;
- release/deployment/rollback procedure;
- production smoke-test checklist;
- final readiness sign-off document.

This unit does not introduce:

- AWS SDK;
- MinIO SDK;
- Azure Blob SDK;
- Google Cloud Storage SDK;
- a generic cloud SDK;
- public buckets;
- presigned browser-direct uploads;
- a production storage adapter before provider approval;
- Dockerfile;
- Docker Compose;
- Kubernetes manifests;
- Terraform/Pulumi/CDK;
- CI/CD pipelines;
- cloud monitoring SDKs;
- SMTP/provider SDKs;
- centralized log vendor;
- Redis;
- distributed cache;
- background workers;
- automatic database migration on production startup;
- automatic database rollback;
- database schema changes;
- multilingual UI;
- a public diagnostics page;
- public Swagger/OpenAPI by default.

### Required operational documents

Create:

```txt
context/operations/production-configuration.md
context/operations/deployment-runbook.md
context/operations/backup-restore-runbook.md
context/operations/production-readiness-checklist.md
context/operations/object-storage-decision.md
```

Use the supplied Unit 57 templates where provided.

Rules:

- never put real secrets or full connection strings in documentation;
- use environment-variable names and secret-source descriptions;
- mark unresolved decisions clearly;
- include owner and verification date;
- update documents when the actual provider/topology is selected;
- keep deployment-specific values out of feature specs;
- do not commit provider credentials, DNS tokens, certificates, or initial passwords.

### Environment profiles

The application must distinguish:

```txt
Development
Test
Production
```

Staging may be supported when the hosting decision introduces it, but must use production-safe defaults unless explicitly documented.

Rules:

- `.env` loading remains Development-only;
- Production must use hosting environment variables or a managed secret store;
- Production must not load or depend on `backend/.env`;
- frontend production values are build-time public values only;
- no environment profile may silently downgrade a production safety rule;
- unknown environment names follow the safest production-like behavior for storage, secrets, cookies, and diagnostics where practical;
- tests may use explicit test-only overrides.

### Production deployment topology

Add a typed option equivalent to:

```txt
DeploymentOptions
```

Stable topology values:

```txt
SAME_ORIGIN
SPLIT_ORIGIN
```

Required fields:

```txt
Topology
PublicWebOrigin
PublicApiOrigin
AllowedFrontendOrigins
TrustedProxyMode
KnownProxies
KnownNetworks
BuildVersion
CommitSha
```

#### `SAME_ORIGIN`

Conceptual deployment:

```txt
https://performance.example.ba/
https://performance.example.ba/api/
```

Rules:

- preferred operational simplicity for cookie authentication;
- frontend may use a relative API base path;
- no cross-origin credential flow is needed;
- exact deployment still depends on the chosen host/reverse proxy;
- this is not a hosting-provider selection.

#### `SPLIT_ORIGIN`

Conceptual deployment:

```txt
https://app.example.ba/
https://api.example.ba/
```

Rules:

- exact frontend origin must be allowed;
- credentialed CORS is required;
- cookie SameSite behavior must match the real site/origin relationship;
- frontend fetches must include credentials through the existing API wrapper;
- CSRF remains required for unsafe methods;
- both origins require HTTPS;
- wildcard origins are forbidden;
- the topology decision must be tested in the final deployed environment.

Do not derive topology from request headers at runtime.

### Origin validation

In Production:

- `PublicWebOrigin` is required;
- `PublicApiOrigin` is required;
- both must be absolute HTTPS origins;
- no path, query, fragment, username, or password;
- localhost, loopback, `.local`, and development ports are rejected unless an approved non-production environment is being validated;
- allowed frontend origins must be exact normalized origins;
- duplicate origins normalize safely;
- wildcard values are rejected;
- trailing slash differences normalize consistently;
- an origin is never accepted from an untrusted request header as configuration.

Development may continue using HTTP localhost origins.

### Frontend production environment

Keep the existing frontend environment contract small.

At minimum validate:

```txt
VITE_API_BASE_URL
```

Rules:

- may be a same-origin relative base path such as `/api`;
- otherwise must be an absolute HTTPS URL in production builds;
- must not contain credentials, query strings, fragments, or secrets;
- must match the documented deployment topology;
- no private backend connection string or storage credential may use `VITE_*`;
- invalid production environment fails the frontend build clearly;
- development localhost remains supported;
- frontend does not need runtime access to secret or provider configuration;
- do not add a public environment dump.

If an existing frontend environment schema already exists, extend it rather than creating a second validator.

### Backend production configuration inventory

Document all actual required sections introduced by earlier units.

At minimum inventory:

```txt
ASPNETCORE_ENVIRONMENT

PlayerPerformance__ServiceName
PlayerPerformance__FrontendOrigin or approved replacement

ConnectionStrings__DefaultConnection

Bootstrap__FirstAdmin__Enabled
Bootstrap__FirstAdmin__Email
Bootstrap__FirstAdmin__TemporaryPassword

FileStorage__Provider
FileStorage__LocalRootPath
FileStorage__MaxObjectSizeBytes

Imports__MaxUploadSizeBytes
Imports__PreviewRowLimit
Imports__ProcessingLeaseTimeoutMinutes
Imports__HeaderScanRowLimit
Imports__CsvDetectionRowLimit
Imports__MaxRowsPerFile
Imports__MaxColumnsPerFile
Imports__MaxCellLengthCharacters
Imports__MaxXlsxEntryCount
Imports__MaxXlsxUncompressedSizeBytes
Imports__MaxXlsxCompressionRatio
Imports__TemporaryWorkingDirectory

deployment/public-origin options introduced by Unit 57
Data Protection options introduced by Unit 57
rate-limit options introduced by Unit 57
logging options supported by the host
```

Use the exact implemented names rather than duplicating aliases.

The inventory must classify each value as:

```txt
PUBLIC_CONFIG
NON_SECRET_CONFIG
SECRET
OPERATIONAL_PATH
DECISION_REQUIRED
```

And record:

```txt
required environments
source
rotation owner
validation rule
safe example
restart/rebuild requirement
```

Do not include real values.

### Production startup validation

Production startup must fail clearly and safely when mandatory configuration is missing or unsafe.

At minimum reject:

- missing/invalid HTTPS public origins;
- wildcard credentialed CORS;
- Local file storage provider;
- unknown file-storage provider;
- missing database connection string;
- database connection with explicitly disabled transport security when the selected provider requires remote TLS;
- missing persistent Data Protection configuration;
- production OpenAPI enabled without an approved restricted-access decision;
- detailed developer errors enabled;
- invalid trusted proxy configuration;
- temporary import directory inside `wwwroot` or frontend public output;
- temporary path equal to the committed local-storage root;
- invalid upload/storage/parser cross-limits;
- unsafe first-admin configuration when no users exist;
- empty production service name;
- non-positive rate-limit values;
- a split-origin topology without exact allowed origins;
- a SameSite=None cookie configuration without Secure;
- a configured local frontend origin in Production.

Errors:

- identify the configuration section/key safely;
- never print secret values;
- never dump the complete environment;
- never include connection strings, passwords, storage keys, or certificates.

Do not introduce a production bypass such as:

```txt
DisableProductionValidation=true
```

Tests may use explicit test environment configuration.

### Secure cookies

Production authentication cookies must remain:

```txt
HttpOnly = true
Secure = true
Path = /
```

Prefer an approved host-only cookie name compatible with the `__Host-` prefix when the existing auth implementation permits it.

Rules:

- no cookie domain unless an approved topology requires it and the security impact is documented;
- authentication cookie is not readable by JavaScript;
- no auth token in localStorage/sessionStorage;
- SameSite value is explicit and topology-compatible;
- `SameSite=None` always requires `Secure`;
- cookie expiration/sliding behavior follows the existing auth spec;
- disabling/reactivating/recovering accounts preserves existing session invalidation rules;
- no production option may set `Secure=false`;
- cookie names and non-secret policy values may be documented, not cookie contents.

Add tests for the production cookie options.

### CORS and credential behavior

For split-origin deployment:

- allow only exact configured HTTPS frontend origins;
- enable credentials;
- allow only required methods/headers;
- include the existing CSRF header;
- do not use:
  - `AllowAnyOrigin`;
  - wildcard subdomains;
  - reflected arbitrary origins;
- preflight responses contain no secret data;
- authorization still applies after CORS;
- CORS is not treated as an authorization boundary.

For same-origin deployment:

- do not add unnecessary permissive CORS;
- development Vite origin support remains development configuration.

Add integration tests for:

- allowed production origin;
- rejected unknown origin;
- credentials behavior;
- unsafe mutation with missing/invalid CSRF;
- safe methods.

### CSRF

Keep Unit 15's CSRF contract authoritative.

Production verification must cover:

- unsafe methods require valid CSRF protection;
- cross-origin topology uses the configured credential and header behavior;
- safe methods do not mutate;
- token/cookie values are never logged;
- expired/invalid CSRF results use safe ProblemDetails;
- file uploads and import/media mutations remain protected;
- no endpoint is exempted merely because it is large or streaming.

Do not replace the existing mechanism.

### HTTPS and HSTS

Production must:

- use HTTPS at the public boundary;
- enable HSTS with an approved safe duration;
- avoid HSTS in Development;
- redirect HTTP to HTTPS only when proxy forwarding is configured correctly;
- never create an infinite redirect loop behind a proxy;
- document where TLS terminates;
- document certificate ownership and renewal;
- reject a public HTTP origin.

Do not manage certificates inside application code unless the selected hosting model later requires it.

### Reverse proxy and forwarded headers

Add typed trusted-proxy behavior.

Stable modes:

```txt
DIRECT
KNOWN_PROXIES
KNOWN_NETWORKS
```

Rules:

- forwarded headers middleware runs before HTTPS redirection, auth, rate-limit partitioning by IP, and request logging that depends on client IP/scheme;
- trust only configured proxy IPs/networks;
- never clear known proxies/networks to trust all forwarders;
- reject invalid IP/CIDR configuration at startup;
- document proxy hop count/forward limit;
- public scheme/host reconstruction is tested;
- direct deployment disables forwarded-header trust;
- proxy configuration contains no DNS guessing;
- request Host is validated by hosting/proxy configuration;
- no user-supplied `X-Forwarded-*` header is trusted from the open internet.

### ASP.NET Core Data Protection

Cookie authentication and CSRF continuity require persistent Data Protection keys.

Add typed options equivalent to:

```txt
DataProtectionOptions
- ApplicationName
- KeysPath
```

Production requirements:

- `ApplicationName` is required and stable across releases;
- keys use persistent storage outside ephemeral application files;
- path is not under source code, `wwwroot`, or temporary import storage;
- path is writable only by the backend service identity;
- key material is never logged or served;
- backups and access controls are documented;
- application restarts must not invalidate all active sessions solely because keys disappeared.

Multi-instance rule:

- all backend replicas must share the same Data Protection key ring;
- a node-local path is acceptable only for an explicitly approved single-instance deployment with persistent volume;
- multi-instance deployment is `BLOCKED` until a shared key provider/path is approved and tested;
- sticky sessions are not a substitute for a durable shared key ring.

Unit 57 does not select a cloud key-management provider.

### Database connection safety

Production database requirements:

- PostgreSQL;
- least-privilege runtime credentials;
- transport encryption for remote database connections;
- secret supplied through environment/managed secret store;
- no connection string logging;
- connection pooling configured through Npgsql defaults/options;
- command timeout/retry behavior documented and bounded;
- server timezone/database timestamp behavior remains consistent with existing UTC rules;
- database host is not reachable publicly beyond approved network controls where the provider supports it.

Prefer distinct identities:

```txt
migration identity
runtime application identity
backup/restore operator identity
```

when the selected provider supports them.

The runtime identity should not own the database or have unrelated administrative permissions.

### Database migration policy

Production must not apply EF Core migrations automatically during ordinary API startup.

Use a release step:

1. verify target environment;
2. confirm a current successful database backup/snapshot;
3. build the exact release commit;
4. inspect pending migrations;
5. apply migrations once using an authorized migration identity;
6. verify migration success;
7. start/upgrade application instances;
8. run readiness and smoke tests.

Rules:

- one controlled migration executor;
- no simultaneous migration from multiple replicas;
- no `EnsureCreated`;
- no automatic downgrade;
- no handwritten production SQL unless explicitly reviewed;
- migration logs contain migration names and safe status, not connection strings;
- app readiness reports unhealthy when required migrations are pending;
- runtime startup may continue only according to the approved rollout design, but traffic must not be sent while schema is incompatible;
- irreversible migrations require an explicit backup/restore or forward-fix plan.

Document exact commands after the deployment environment is selected.

### Rollback policy

Application rollback and database rollback are separate.

#### Application rollback

May deploy the previously verified application artifact only when its schema compatibility is confirmed.

#### Database rollback

Do not assume an EF `Down` migration is a safe production rollback.

Preferred order:

1. stop traffic/writes;
2. assess compatibility;
3. apply an approved forward fix when safest;
4. restore from a verified backup when required;
5. validate database and object-storage consistency;
6. resume traffic only after smoke tests.

Every release with schema changes must document:

```txt
backward compatibility
rollback boundary
backup identifier
restore procedure
data-loss risk
owner
```

### Backup and restore requirements

The production-readiness checklist must not pass without a backup plan.

#### PostgreSQL

Require:

- automated backups from the selected provider or controlled tooling;
- encrypted backups;
- documented retention;
- pre-migration backup/snapshot;
- point-in-time recovery when the selected provider supports it;
- restricted backup access;
- restore testing;
- monitoring of backup success.

Do not invent final RPO/RTO values.

Record them as decisions requiring club/operations approval:

```txt
RPO
RTO
retention
restore-test frequency
backup owner
```

#### Object storage

Require provider decision coverage for:

- private bucket/container;
- encryption at rest;
- TLS in transit;
- durability;
- versioning or equivalent recovery protection;
- retention/lifecycle behavior;
- backup/replication where required;
- accidental deletion recovery;
- storage usage monitoring;
- restore/recovery testing;
- consistency with database metadata.

Normal product archive remains metadata-only and must not physically delete committed objects.

Compensation deletion for failed uncommitted writes remains required.

#### Data Protection keys

Document backup/access behavior so restoring the application/database does not unintentionally invalidate every session unless that is an intentional security response.

### Production object-storage gate

Unit 40 deliberately supports only Development `Local`.

Production deployment is `BLOCKED` until an object-storage provider is approved and implemented.

The decision record must identify:

```txt
provider/product
region/location
endpoint model
bucket/container
private access
credential model
TLS
server-side encryption
versioning/recovery
retention/lifecycle
maximum object size
multipart/streaming behavior
read-stream behavior
range/seek support
health-check behavior
temporary failure/retry semantics
cost owner
backup/recovery
data residency
service limits
```

Required adapter capabilities:

- implement the existing provider-neutral `IFileStorage`;
- server-generated opaque keys;
- streaming writes;
- bounded memory;
- streamed reads;
- compensation delete;
- object collision behavior;
- cancellation;
- safe provider error translation;
- private objects;
- no public storage keys/URLs;
- media content authorization remains through the API;
- import source authorization remains through the API;
- support framework range behavior where feasible, or document video-seeking limitations truthfully;
- no complete-file buffering;
- provider-specific health check;
- configuration validation;
- integration tests against an approved emulator/test account where practical.

Browser-direct uploads and presigned public access are out of scope unless a later spec changes the security model.

Once the provider is selected:

- create a separate provider-specific implementation spec;
- add only its official/mature SDK;
- update architecture and configuration inventory;
- keep `Local` Development-only;
- do not silently repurpose Unit 57 into an unreviewed cloud integration.

### File-storage startup behavior

Existing rules remain:

- `FileStorage__Provider=Local` is rejected outside Development;
- unknown providers fail;
- no fake/no-op provider;
- missing provider fails;
- no automatic fallback.

Unit 57 additionally requires:

- readiness health check for the configured provider;
- clear `BLOCKED` checklist state while no production adapter exists;
- no claim that the application can serve uploaded media/import files in Production until the adapter passes integration tests.

### Temporary import working directory

Production import parsing may require bounded temporary files.

Requirements:

- explicit configured directory;
- outside source code, static frontend output, public web roots, and object-storage mount;
- service-identity-only permissions;
- enough capacity for configured bounded operations;
- startup/readiness checks for existence or safe creation, write access, and free-space policy where practical;
- cleanup on success/failure/cancellation remains Unit 45 behavior;
- no assumption that temporary files survive restart;
- no backup requirement for temporary data;
- deployment runbook documents cleanup and disk-pressure response;
- path is never returned or logged in full to users.

Container/ephemeral storage details remain provider-specific.

### Health endpoints

Add or formalize:

```txt
GET /health/live
GET /health/ready
```

Keep `GET /health` only as a backward-compatible safe alias if already used by local tooling; document which check it represents.

#### Liveness

Checks only that the process can serve requests.

Must not depend on:

- database;
- object storage;
- external email;
- provider network.

Response:

- anonymous;
- minimal;
- no secrets;
- stable `200` when live;
- safe `503` when not live.

#### Readiness

Checks:

- production configuration validation completed;
- PostgreSQL connectivity;
- no required pending EF migrations;
- configured file-storage provider health;
- temporary import working directory readiness;
- required persistent Data Protection path/configuration;
- any other strictly required local dependency.

Rules:

- readiness may return `503`;
- response does not reveal:
  - hostnames;
  - connection strings;
  - bucket names;
  - paths;
  - exception messages;
  - usernames;
- detailed health data is logged safely for operators, not returned publicly;
- checks are bounded with timeouts/cancellation;
- health endpoints are excluded from noisy request logs where appropriate;
- readiness is used by the load balancer/orchestrator only after the infrastructure is selected.

Email delivery should not make the whole API unready unless email is an approved mandatory synchronous dependency.

### Build metadata

Expose safe build metadata through health response headers or a small safe component of the health payload:

```txt
service
version
```

`CommitSha` may be:

- logged at startup;
- included in an internal-safe health field when approved;
- truncated to a safe identifier.

Rules:

- no branch names containing sensitive information;
- no repository URL;
- no build-machine path;
- no environment dump;
- missing optional version metadata does not crash Development;
- Production readiness checklist records the deployed artifact/version.

Do not add a public configuration endpoint.

### OpenAPI and diagnostics

Production defaults:

- Swagger/OpenAPI UI disabled;
- developer exception page disabled;
- detailed EF/provider errors disabled;
- stack traces never returned;
- ProblemDetails remains safe;
- health remains minimal.

If OpenAPI is later required operationally in Production:

- it needs a separate approved access-control decision;
- public unauthenticated enablement is not allowed by default.

Do not add a secret query-string switch.

### Structured logging

Use the existing ASP.NET Core logging abstraction.

Production requirements:

- structured console logging suitable for the selected host;
- UTC timestamps from the host/log platform;
- environment, service, version, trace/correlation IDs as safe properties;
- configurable log levels;
- no complete request/response body logging;
- no full query-string logging where it may contain opaque IDs or filters unless safely reviewed;
- no secrets;
- no auth/CSRF cookies;
- no passwords/tokens;
- no connection strings;
- no storage keys/paths;
- no source rows/files;
- no coach-visible or restricted medical notes;
- no diagnosis/body-area values;
- no complete physical workload/import payloads.

At startup log only safe readiness facts such as:

```txt
service name
environment
build version
deployment topology
configured provider names without credentials
```

Do not add a vendor logging SDK in Unit 57.

### Correlation and trace behavior

Preserve Unit 08 trace IDs in ProblemDetails.

Requirements:

- accept/generate a safe request correlation identifier according to existing middleware;
- reject or normalize excessively long/invalid external correlation headers;
- include trace ID in safe logs and errors;
- do not use correlation ID as authorization;
- do not include secrets/user notes in tracing baggage;
- health checks may use reduced logging;
- proxy request IDs may be mapped only through a documented trusted behavior.

### Rate limiting

Use ASP.NET Core's built-in rate limiting.

Add typed options with validated positive limits and windows.

At minimum create named policies:

```txt
AUTH_SENSITIVE
TOKEN_SETUP
UPLOAD_CREATE
```

Apply to:

#### `AUTH_SENSITIVE`

- login;
- forgot/reset password when implemented;
- other unauthenticated credential-validation entry points.

Partition primarily by trusted client IP for unauthenticated requests.

#### `TOKEN_SETUP`

- invite/setup token validation and password setup;
- required password-change attempts where appropriate.

Use a combination of trusted client IP and safe user/token partitioning without logging the raw token.

#### `UPLOAD_CREATE`

- media upload creation;
- import source upload creation.

Partition by authenticated user ID and use a concurrency/bounded request policy appropriate for long streaming operations.

Rules:

- exact values are configurable and documented;
- queueing is disabled or tightly bounded;
- `429` returns safe ProblemDetails and `Retry-After` where supported;
- health endpoints are not accidentally blocked;
- authenticated ordinary reads are not broadly throttled in a way that breaks tables/dashboard;
- reverse-proxy IP trust is configured before IP partitioning;
- rate limits are defense-in-depth, not brute-force detection/account lockout replacement;
- tests use deterministic low values.

Do not add a rate-limiting package.

### Request and upload limits

Existing domain limits remain authoritative:

- Unit 40 object size;
- media upload capability limits;
- import upload limits;
- CSV/XLSX parser limits.

Production runbook must align:

```txt
reverse-proxy body limit
hosting request timeout
proxy buffering behavior
backend server limit
application upload limit
temporary-disk capacity
object-storage multipart behavior
```

Rules:

- proxy/host limits must not silently be lower than advertised application capabilities;
- application still rejects oversize streams;
- do not raise limits to unbounded values;
- do not buffer large files in memory;
- cancellation/disconnect propagates;
- timeout behavior is documented for large uploads;
- upload failure leaves no committed metadata/partial object according to existing compensation rules;
- no Base64 transport.

### Security headers

Document and implement application-owned headers where appropriate.

Backend/API baseline:

```txt
X-Content-Type-Options: nosniff
Referrer-Policy: no-referrer or approved strict policy
frame-ancestors protection through CSP at the web boundary
```

Frontend/static host baseline should document:

```txt
Strict-Transport-Security
Content-Security-Policy
X-Content-Type-Options
Referrer-Policy
Permissions-Policy
```

CSP must account for:

- self-hosted Vite assets;
- API connection origin;
- authorized image/video/PDF content endpoints;
- blob/data usage only where actually required;
- shadcn/Radix inline style behavior where applicable;
- no arbitrary third-party script origin;
- `frame-ancestors 'none'` unless an approved embedding use case exists;
- `base-uri 'self'`;
- `form-action 'self'` or topology-compatible approved value.

Rules:

- final CSP is tested in the deployed topology;
- do not add `unsafe-eval` in Production;
- do not add broad `*`;
- report-only mode may be used during verification but enforcement is required before readiness;
- no external analytics/font/CDN origin is assumed;
- static-host configuration remains provider-specific and is recorded after provider selection.

Do not rely only on `<meta http-equiv>` when the required header must be sent by the host/proxy.

### Static frontend caching

Document host behavior:

- `index.html`:
  - no long immutable cache;
  - revalidate/no-cache according to host capabilities;
- hashed Vite assets:
  - long cache;
  - immutable;
- source maps:
  - disabled publicly by default or protected according to an approved debugging policy;
- API/auth/medical responses:
  - never cached publicly;
- media content:
  - authorization remains checked per request;
  - cache policy must not allow one user's protected content to become public/shared;
- service worker/offline caching is not introduced.

Do not add a PWA in Unit 57.

### First-admin production procedure

Use the existing Unit 17 bootstrap.

Runbook:

1. ensure no users exist only for first deployment;
2. set:
   - enabled;
   - admin email;
   - strong temporary password;
3. deploy/start one controlled backend instance;
4. verify exactly one admin was created;
5. sign in;
6. require immediate password change;
7. remove/rotate the temporary password secret;
8. set bootstrap enabled to false;
9. restart/redeploy if required by the host;
10. verify bootstrap cannot create/overwrite another user.

Rules:

- temporary password never enters Git/docs/logs/chat screenshots;
- do not leave the secret configured indefinitely;
- do not run several first-start replicas concurrently before bootstrap behavior is verified;
- if users exist, bootstrap never resets them;
- production startup with empty user store and disabled/incomplete bootstrap remains a clear blocker according to Unit 17.

### Invitation and account-setup delivery

Production readiness must document the actual mode:

```txt
EMAIL
MANUAL_TRUSTED_CHANNEL
```

#### `EMAIL`

Requires an implemented/tested email provider and:

- sender/domain configuration;
- TLS;
- credential storage;
- delivery/retry behavior;
- bounce/failure operational handling;
- safe templates;
- no token logging.

Unit 57 does not implement a provider.

#### `MANUAL_TRUSTED_CHANNEL`

May be used only if the existing V1 flow supports admin-visible one-time setup links.

Requirements:

- explicit operational approval;
- links shared only through an approved trusted channel;
- one-time token;
- expiry;
- no logs/analytics;
- admin guidance;
- compromised-link response;
- documented limitation.

If neither mode is operationally usable, staff onboarding is `BLOCKED`.

Password-reset/recovery availability must be documented truthfully.

### Secrets management

Classify and protect at minimum:

```txt
database credentials
first-admin temporary password
email credentials
object-storage credentials
Data Protection protection material when applicable
future vendor tokens
```

Rules:

- no Git;
- no frontend `VITE_*`;
- no build artifact;
- no logs;
- no screenshots/docs;
- least privilege;
- rotation owner;
- revocation procedure;
- separate production from development/test;
- access audit where the chosen platform supports it;
- no shared personal credentials;
- deploy process references secret names, not values.

Unit 57 does not select a managed secret store.

### Service identity and filesystem permissions

The backend process should run as a non-root/non-administrator service identity where the hosting model supports it.

It needs access only to:

- network/database;
- persistent Data Protection key location;
- bounded temporary import directory;
- provider credentials supplied by the host;
- application binaries read-only where practical.

It must not need write access to:

- frontend static assets;
- source code;
- broad host filesystem;
- unrelated secrets.

Document ownership/permissions after host selection.

### Single-instance and multi-instance readiness

#### Single instance

Requires:

- persistent Data Protection keys;
- external PostgreSQL;
- production object storage;
- health checks;
- restart behavior;
- no critical committed file on ephemeral local disk.

#### Multiple instances

Additionally requires:

- shared Data Protection keys;
- external object storage;
- external PostgreSQL;
- no in-memory authoritative job/session state;
- database-backed leases/concurrency as already designed;
- migration executed separately;
- load balancer health routing;
- compatible cookie behavior;
- upload timeout/body settings;
- verified concurrent startup/bootstrap behavior.

Multi-instance deployment remains `BLOCKED` until these are tested.

Do not add Redis only because multiple instances are possible.

### Release artifact rules

Frontend:

- exact lockfile install;
- production build;
- build-time environment validated;
- artifact contains no `.env` or source secrets;
- hashed assets;
- build output immutable after verification.

Backend:

- exact restore/build/test;
- Release publish artifact;
- no local `.env`;
- no Development settings reliance;
- version/commit metadata recorded;
- migration assembly included;
- no temporary upload/storage directories packaged with real files.

Use one exact source commit for:

```txt
frontend artifact
backend artifact
migration set
documentation/checklist
```

Do not rebuild different artifacts during rollback without recording the new identity.

### Deployment runbook

The runbook must cover:

#### Pre-deployment

- approved infrastructure decisions;
- readiness checklist;
- release commit/tag;
- package lock;
- tests/builds;
- secrets present;
- DNS/TLS;
- database backup;
- object-storage health;
- Data Protection persistence;
- migration review;
- rollback owner;
- maintenance/communication decision.

#### Deployment

- apply migration once;
- deploy backend without traffic or as approved;
- verify `/health/live`;
- verify `/health/ready`;
- deploy frontend;
- verify security headers/static caching;
- send traffic;
- run smoke tests.

#### Post-deployment

- inspect safe logs;
- verify auth;
- verify CSRF mutation;
- verify role/team scope;
- verify database read/write;
- upload/open/download a small authorized media test object;
- upload/download/preview a small import source;
- verify dashboard;
- verify availability safe privacy;
- verify restricted medical authorization;
- verify audit writes;
- verify rate limiting;
- verify restart session continuity;
- record artifact/version and sign-off.

No real player medical/performance data should be used solely for smoke testing when synthetic test data can be used.

### Smoke-test account/data

Use controlled non-production-like synthetic records in production only when club operations approves the cleanup/retention policy.

Prefer:

- designated test staff account with least privilege;
- designated test team/player/match labels clearly marked;
- small harmless media/import files;
- no real medical notes;
- no real passwords shared in documentation.

If production test records are not allowed, define equivalent safe verification using approved existing records without mutation.

### Rollout and rollback monitoring

Without selecting a monitoring vendor, require operational observation of:

- readiness failures;
- HTTP 5xx;
- auth failures/rate limits;
- database connectivity;
- migration errors;
- object-storage errors;
- upload failures;
- disk pressure on temporary storage;
- backup status;
- unusual forbidden/CSRF failures.

Define:

```txt
who watches
where logs/metrics are viewed
how alerting occurs
how deployment is stopped
```

before release.

Do not claim automated alerting exists until configured.

### Readiness checklist ownership

The checklist must include sign-off fields for:

```txt
Product owner
Technical owner
Database/backup owner
Security/privacy owner
Club operational owner
Deployment executor
Verification date
Release version
```

One person may hold more than one role, but ownership must be explicit.

Checklist evidence may link to:

- build/test logs;
- migration output;
- backup identifier;
- restore-test record;
- security-header result;
- health checks;
- smoke-test record;
- accessibility audit;
- object-storage decision.

Do not include secret values.

### Tests

Add focused unit/integration tests.

#### Production option validation

Cover:

- valid same-origin profile;
- valid split-origin profile;
- missing origins;
- HTTP production origin;
- wildcard origin;
- localhost production origin;
- duplicate normalized origins;
- invalid topology;
- split-origin missing allowed origin;
- invalid proxy mode/IP/network;
- missing Data Protection path/application name;
- Local storage in Production;
- invalid temporary directory relationship;
- invalid rate-limit values;
- unsafe OpenAPI/detailed-errors setting;
- secret values excluded from validation exceptions.

#### Cookie/CORS/CSRF tests

Cover:

- Production Secure/HttpOnly cookie;
- explicit SameSite behavior;
- SameSite=None requires Secure;
- allowed exact origin;
- rejected unknown origin;
- credentials;
- safe/unsafe CSRF behavior;
- upload mutation protection;
- no wildcard.

#### Forwarded-header tests

Cover:

- trusted proxy scheme/IP;
- untrusted forwarded headers ignored;
- HTTPS redirect behavior;
- rate-limit client partition receives trusted client IP;
- safe direct mode.

#### Health tests

Cover:

- liveness without dependencies;
- readiness success;
- database failure;
- pending migration;
- storage failure;
- temp-directory failure;
- timeout/cancellation;
- response redaction;
- anonymous safe response;
- backward-compatible `/health` behavior.

#### Rate-limit tests

Cover:

- auth policy limit;
- token/setup policy;
- upload concurrency/limit;
- `429`;
- `Retry-After` where supported;
- user/IP partitioning;
- health not limited;
- no raw token partition/log.

#### Frontend environment tests

Cover:

- relative same-origin API path;
- valid HTTPS API URL;
- HTTP production rejection;
- credentials/query/fragment rejection;
- no secret values;
- build failure on invalid config.

#### Logging/redaction tests

Where existing test infrastructure permits, cover:

- safe configuration validation messages;
- no secret values;
- no storage keys;
- no medical/source/request-body logs;
- trace ID retained.

#### Operational document verification

Add a simple repository check or manual verification ensuring required operation files exist and contain no obvious secret placeholders such as real passwords/tokens.

Do not build a custom secrets scanner in this unit.

### Documentation synchronization

Update:

```txt
context/architecture.md
context/code-standards.md
context/progress-tracker.md
context/operations/*
```

Record:

- production environment behavior;
- deployment topology options;
- origin/cookie/proxy rules;
- Data Protection persistence;
- health endpoint semantics;
- migration policy;
- backup/restore requirements;
- object-storage deployment block;
- invitation delivery decision;
- first-admin procedure;
- rate limiting;
- frontend build configuration;
- security headers;
- single/multi-instance constraints;
- actual test/build commands and results;
- unresolved provider/hosting decisions.

Keep Unit 54/55 deferred and Bosnian as the current only UI language.

Do not mark Unit 57 or V1 deployment `READY` while mandatory checklist items are `BLOCKED`.

## Implementation

### 1. Create operations documentation

Create:

```txt
context/operations/production-configuration.md
context/operations/deployment-runbook.md
context/operations/backup-restore-runbook.md
context/operations/production-readiness-checklist.md
context/operations/object-storage-decision.md
```

Populate known application facts.

Keep provider-specific fields unresolved rather than guessing.

### 2. Add deployment and Data Protection options

Implement typed options and startup validation for:

- topology;
- public origins;
- exact frontend origins;
- proxy mode/proxies/networks;
- build metadata;
- persistent Data Protection application name/path.

Reuse Unit 07's validation pattern.

### 3. Harden production startup

Ensure:

- no `.env` production dependency;
- unsafe origins/cookies/proxies fail;
- Local storage fails outside Development;
- developer diagnostics/OpenAPI are disabled by default;
- temporary paths and cross-limits validate;
- secret values are redacted.

### 4. Configure production cookies, CORS, CSRF, HTTPS, and proxy behavior

Wire middleware in the correct order.

Add focused integration tests.

Preserve Development Vite behavior.

### 5. Persist Data Protection keys

Use the configured persistent path for the provider-neutral initial production profile.

Document single-instance versus shared multi-instance constraints.

Do not add a cloud key provider.

### 6. Add production health endpoints

Implement:

```txt
/health/live
/health/ready
```

Add bounded checks for:

- database;
- pending migrations;
- storage;
- temporary directory;
- required configuration.

Keep responses minimal and safe.

### 7. Add built-in rate limiting

Configure and apply:

```txt
AUTH_SENSITIVE
TOKEN_SETUP
UPLOAD_CREATE
```

Use trusted client identity/IP and safe `429` responses.

Do not broadly throttle normal dashboard/table reads.

### 8. Validate frontend production configuration

Extend the existing frontend environment schema.

Support relative same-origin or absolute HTTPS API base URL.

Fail production build on unsafe values.

### 9. Apply production logging and diagnostics rules

Configure safe structured output and production log levels.

Preserve trace IDs.

Do not add an external logging vendor.

### 10. Add security-header and static-host requirements

Implement application-owned headers.

Document provider-owned frontend/proxy headers and caching.

Test the final policy after host selection.

### 11. Formalize migration and rollback policy

Ensure automatic production startup migration is disabled.

Document the controlled release migration command/process and backup gate.

### 12. Formalize first-admin and invitation procedures

Document bootstrap secret removal and onboarding delivery mode.

Do not implement SMTP without a provider decision.

### 13. Enforce the object-storage deployment gate

Keep Local Development-only.

Populate the decision record.

Mark production deployment `BLOCKED` until a provider-specific adapter is approved, implemented, health-checked, and integration-tested.

### 14. Add production smoke-test checklist

Cover authentication, CSRF, authorization, database, storage, imports, media, dashboard, availability privacy, restricted medical access, audit, rate limits, restart continuity, and backup evidence.

### 15. Add focused tests

Implement all applicable option, middleware, health, rate-limit, frontend environment, and redaction tests using existing test foundations.

### 16. Update project documentation

Update architecture, standards, progress tracker, operations documents, and build plan.

Do not claim a hosting/provider decision that has not been made.

## Dependencies

None.

Use existing platform/framework capabilities:

- ASP.NET Core configuration/options;
- ASP.NET Core Data Protection;
- ASP.NET Core Health Checks;
- ASP.NET Core Rate Limiting;
- ASP.NET Core Forwarded Headers;
- ASP.NET Core CORS;
- ASP.NET Core cookie authentication;
- existing CSRF implementation;
- EF Core/Npgsql;
- existing file-storage abstraction;
- existing frontend environment/Zod tooling;
- existing backend/frontend test infrastructure.

Do not add:

- cloud/object-storage SDKs;
- Docker/container packages;
- CI/CD tooling;
- secrets-manager SDKs;
- SMTP/email SDKs;
- external health-check packages unless an existing dependency gap is documented and approved;
- logging/monitoring vendor SDKs;
- Redis/distributed cache;
- another configuration framework;
- localization packages;
- a new test framework solely for Unit 57.

A provider-specific object-storage SDK is added only in a later approved provider implementation spec.

## Verification checklist

- [ ] Unit 54 and Unit 55 remain deferred.
- [ ] Bosnian Latin remains the only current UI language.
- [ ] Required operations documents exist under `context/operations/`.
- [ ] Operations documents contain no real secrets, credentials, tokens, or connection strings.
- [ ] Production readiness uses `READY`, `BLOCKED`, `ACCEPTED_LIMITATION`, and `NOT_APPLICABLE`.
- [ ] Hosting provider remains explicitly undecided unless separately approved.
- [ ] PostgreSQL provider remains explicitly undecided unless separately approved.
- [ ] Object-storage provider remains explicitly undecided unless separately approved.
- [ ] Email delivery mode/provider remains explicit rather than assumed.
- [ ] Production deployment is marked `BLOCKED` while mandatory provider decisions are unresolved.
- [ ] Typed deployment/public-origin options exist.
- [ ] Stable topology values are `SAME_ORIGIN` and `SPLIT_ORIGIN`.
- [ ] Production web/API origins are required absolute HTTPS origins.
- [ ] Production rejects localhost, loopback, wildcard, credential-bearing, query, and fragment origins.
- [ ] Exact allowed frontend origins are validated.
- [ ] No request header is used as trusted origin configuration.
- [ ] Frontend production environment validates `VITE_API_BASE_URL`.
- [ ] Frontend supports a safe relative same-origin API base.
- [ ] Absolute production API base must use HTTPS.
- [ ] Frontend build fails on unsafe production configuration.
- [ ] No backend secret is exposed through `VITE_*`.
- [ ] Backend production configuration inventory contains all actual earlier-unit options.
- [ ] Configuration inventory classifies public config, non-secret config, secrets, paths, and decisions.
- [ ] Production startup validation fails safely on missing/unsafe mandatory settings.
- [ ] Startup validation never prints secret values or full environment dumps.
- [ ] No production-validation bypass option exists.
- [ ] Authentication cookie is HttpOnly.
- [ ] Authentication cookie is Secure in Production.
- [ ] Cookie path and SameSite policy are explicit and topology-compatible.
- [ ] SameSite=None cannot be used without Secure.
- [ ] Auth tokens remain absent from browser local/session storage.
- [ ] Split-origin CORS allows only exact HTTPS origins.
- [ ] Credentialed CORS never uses wildcard origin.
- [ ] Same-origin production does not enable unnecessary permissive CORS.
- [ ] Existing CSRF protection covers all unsafe mutations, including streamed uploads.
- [ ] HTTPS is required at the public production boundary.
- [ ] HSTS is enabled only outside Development with approved settings.
- [ ] Reverse-proxy middleware order is correct.
- [ ] Forwarded headers trust only configured proxies/networks.
- [ ] Untrusted public `X-Forwarded-*` values are ignored.
- [ ] Persistent Data Protection application name/path are required in Production.
- [ ] Data Protection keys are outside source, web root, and temp storage.
- [ ] Data Protection keys are not logged or served.
- [ ] Restart does not invalidate all sessions because keys were ephemeral.
- [ ] Multi-instance deployment is blocked until key sharing is approved/tested.
- [ ] PostgreSQL credentials are secret and least-privilege.
- [ ] Remote production database transport encryption is required/documented.
- [ ] Connection strings are never logged.
- [ ] Runtime and migration identities are separated where provider support permits.
- [ ] Automatic EF migration on ordinary production API startup is disabled.
- [ ] Production migration runs once as a controlled release step.
- [ ] Pending required migrations make readiness unhealthy.
- [ ] Pre-migration backup is required.
- [ ] Rollback does not assume EF `Down` is automatically safe.
- [ ] Backup/restore runbook defines RPO/RTO as approval decisions rather than guesses.
- [ ] PostgreSQL backup encryption, retention, owner, and restore testing are documented.
- [ ] Object-storage recovery/versioning/retention requirements are documented.
- [ ] Data Protection key recovery/access behavior is documented.
- [ ] `FileStorage__Provider=Local` remains Development-only.
- [ ] No fake/no-op production storage provider exists.
- [ ] Production remains blocked until an approved provider adapter exists.
- [ ] Object-storage decision record covers security, streaming, range, health, limits, recovery, and data residency.
- [ ] Future provider adapter must implement existing `IFileStorage` without leaking provider types.
- [ ] Browser-direct public object access is not introduced.
- [ ] Media/import authorization remains through the API.
- [ ] Temporary import directory is private, bounded, writable, and outside public/source paths.
- [ ] Temporary directory readiness is checked.
- [ ] Temporary data is not backed up as authoritative data.
- [ ] `/health/live` exists and does not depend on database/storage.
- [ ] `/health/ready` exists and checks mandatory dependencies.
- [ ] Health responses expose no hostnames, connection strings, bucket names, paths, usernames, or exceptions.
- [ ] Health checks are bounded and cancellation-aware.
- [ ] Existing `/health` behavior remains safely compatible or is documented.
- [ ] Safe service/version metadata is available for release verification.
- [ ] No public configuration/environment dump endpoint exists.
- [ ] Swagger/OpenAPI UI is disabled in Production by default.
- [ ] Developer exception page and detailed provider errors are disabled in Production.
- [ ] Production logging is structured and configurable.
- [ ] Logs contain safe trace/version/environment context.
- [ ] Logs exclude request bodies, secrets, cookies, tokens, connection strings, storage keys, source rows, and medical notes.
- [ ] Correlation IDs are bounded/validated and remain non-authoritative.
- [ ] Built-in rate limiting is used without a new package.
- [ ] `AUTH_SENSITIVE`, `TOKEN_SETUP`, and `UPLOAD_CREATE` policies exist.
- [ ] Authentication rate limiting uses trusted client IP behavior.
- [ ] Upload rate limiting partitions by authenticated user and supports long streaming requests.
- [ ] `429` is safe and returns retry guidance where supported.
- [ ] Health endpoints and normal bounded reads are not unintentionally throttled.
- [ ] Proxy/body/time limits are aligned with application upload capabilities.
- [ ] Large files are never buffered fully in memory or encoded as Base64.
- [ ] Application-owned security headers are configured.
- [ ] Frontend/proxy CSP, HSTS, content-type, referrer, permissions, and frame policies are documented.
- [ ] Production CSP has no `unsafe-eval` or wildcard source.
- [ ] Final security headers are tested in the selected topology.
- [ ] `index.html` is not cached as immutable.
- [ ] Hashed frontend assets may use long immutable caching.
- [ ] Public source-map behavior is explicitly decided.
- [ ] Protected API/medical/media responses are not publicly cached.
- [ ] No service worker/PWA is introduced.
- [ ] First-admin production runbook requires immediate password change and secret removal.
- [ ] Bootstrap never overwrites existing users.
- [ ] Invitation delivery is explicitly `EMAIL` or `MANUAL_TRUSTED_CHANNEL`.
- [ ] Manual setup-link mode has one-time/expiry/trusted-channel guidance.
- [ ] Staff onboarding is marked blocked when no delivery mode works.
- [ ] Secret inventory includes database, bootstrap, email, storage, and protection material.
- [ ] Secret rotation/revocation ownership is documented.
- [ ] Backend service identity uses least filesystem/network privilege where supported.
- [ ] Single-instance requirements are documented.
- [ ] Multi-instance requirements are documented and tested before use.
- [ ] Frontend/backend/migration artifacts come from one exact source commit.
- [ ] Artifacts contain no real `.env` files or secrets.
- [ ] Deployment runbook covers pre-deploy, migration, deploy, health, traffic, smoke tests, and rollback.
- [ ] Smoke tests cover auth, CSRF, authorization, database, storage, media, imports, dashboard, availability privacy, medical restrictions, audit, rate limits, and restart continuity.
- [ ] Smoke tests avoid real sensitive data where synthetic records are sufficient.
- [ ] Operational monitoring ownership is explicit even without a selected vendor.
- [ ] Readiness checklist contains named technical/product/database/security/deployment owners.
- [ ] Production option validation tests pass.
- [ ] Cookie/CORS/CSRF integration tests pass.
- [ ] Forwarded-header tests pass.
- [ ] Liveness/readiness tests pass.
- [ ] Rate-limit tests pass.
- [ ] Frontend production-environment tests pass.
- [ ] Logging/redaction tests pass where supported.
- [ ] No EF migration is added.
- [ ] No cloud, Docker, CI/CD, secrets-manager, SMTP, logging-vendor, Redis, localization, or new test-framework dependency is added.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore` completes successfully.
- [ ] `dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes` passes.
- [ ] `dotnet build` passes for affected backend projects/solution.
- [ ] `dotnet test` passes for relevant backend tests.
- [ ] `npm run format` completes successfully.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] Frontend typecheck/build passes using the configured project command.
- [ ] Existing approved frontend tests pass when present.
- [ ] `context/architecture.md` reflects the production-readiness boundaries.
- [ ] `context/code-standards.md` records future production configuration/deployment rules.
- [ ] `context/progress-tracker.md` records actual implementation, verification, and unresolved deployment blockers.
- [ ] Unit 57 is not marked deployment-ready while mandatory checklist entries remain `BLOCKED`.
