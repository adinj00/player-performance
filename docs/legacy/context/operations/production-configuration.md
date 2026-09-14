# Production configuration inventory

Status: BLOCKED until hosting, PostgreSQL, object storage, TLS/proxy, email delivery, backup, and instance decisions are approved. Owner: Technical owner. Verification date: pending deployment approval.

Production never loads `backend/.env`; use the hosting environment or approved secret store. `VITE_*` values are public build-time configuration and must never contain a secret.

| Key/section | Classification | Production rule | Source/restart |
| --- | --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | NON_SECRET_CONFIG | `Production` | host; restart |
| `PlayerPerformance__ServiceName` | PUBLIC_CONFIG | non-empty service label | host; restart |
| `ConnectionStrings__DefaultConnection` | SECRET | PostgreSQL runtime identity; encrypted transport for remote DB | secret owner; restart |
| `Deployment__*` | DECISION_REQUIRED | HTTPS origins, topology, proxy trust, build version and commit | approved deployment record; restart |
| `DataProtection__ApplicationName`, `DataProtection__KeysPath` | OPERATIONAL_PATH | stable name and writable persistent non-public key ring | technical owner; restart |
| `FileStorage__*` | DECISION_REQUIRED | `Local` is Development-only; approved production adapter required | storage owner; restart |
| `Imports__*`, `Media__*` | NON_SECRET_CONFIG / OPERATIONAL_PATH | bounded limits; private temporary directory outside public/source paths | technical owner; restart |
| `Bootstrap__FirstAdmin__*` | SECRET | enabled only for first bootstrap; remove secret immediately after use | security owner; restart |
| `RateLimits__*` | NON_SECRET_CONFIG | positive policy values | technical owner; restart |
| `VITE_API_BASE_URL` | PUBLIC_CONFIG | `/api` for same-origin or absolute HTTPS API URL for split-origin | frontend rebuild |

Topology values are `SAME_ORIGIN` and `SPLIT_ORIGIN`. Both public origins must be absolute HTTPS origins without credentials, paths, query, fragments, wildcards, localhost, loopback, or `.local` hosts. Split-origin CORS is exact-origin plus credentials; same-origin does not enable CORS. Trusted proxy modes are `DIRECT`, `KNOWN_PROXIES`, and `KNOWN_NETWORKS`; do not trust arbitrary forwarded headers.

Data Protection keys must be outside source, web root, and import temporary storage. A node-local persistent path is only acceptable for an approved single backend instance. Multi-instance deployment remains BLOCKED until all instances use a tested shared key ring.

The application exposes safe release metadata through health responses only; it does not expose configuration or environment dumps. Logs must retain trace IDs while excluding request bodies, cookies, tokens, connection strings, storage keys, source rows, and medical notes.

