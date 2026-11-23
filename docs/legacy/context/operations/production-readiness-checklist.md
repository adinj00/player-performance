# Production readiness checklist

Overall status: **BLOCKED**. Mandatory unresolved decisions: hosting/TLS/proxy, PostgreSQL provider, approved object-storage adapter, email delivery mode, backup/restore policy, monitoring/alerting destination, and single- versus multi-instance deployment.

Use only: `READY`, `BLOCKED`, `ACCEPTED_LIMITATION`, `NOT_APPLICABLE`. An accepted limitation requires owner, approval date, impact, expiry/review date, and fallback; security, medical privacy, secrets, backups, TLS, and production storage are not casually waivable.

| Area | Status | Evidence / owner |
| --- | --- | --- |
| Application production configuration, origin validation, cookies, CSRF, proxy order | READY | Technical owner to attach build/test evidence |
| Health, rate limits, safe diagnostics, security headers | READY | Technical owner to attach deployed verification |
| Frontend HTTPS/relative API build configuration | READY | Frontend owner to attach build output |
| Database provider, remote TLS, migration identity, backup/restore | BLOCKED | Database/backup owner |
| Object-storage provider adapter and health integration | BLOCKED | Technical/security owner |
| Hosting, TLS, DNS, reverse-proxy and final headers/CSP | BLOCKED | Technical owner |
| Email/manual trusted-channel invitation delivery | BLOCKED | Club operational owner |
| Monitoring/alerting and release stop authority | BLOCKED | Deployment executor |
| Single-instance persistence / shared multi-instance key ring | BLOCKED | Technical owner |

Sign-off: Product owner: ____; Technical owner: ____; Database/backup owner: ____; Security/privacy owner: ____; Club operational owner: ____; Deployment executor: ____; Verification date: ____; Release version: ____.

Required release evidence: build/test logs, migration output, backup identifier, restore-test record, security-header result, health checks, smoke-test record, accessibility audit, and object-storage decision. Do not place secrets in this file.
