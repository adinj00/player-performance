# Deployment runbook

Status: BLOCKED pending provider decisions. Owners and verification date must be entered before release.

1. Confirm the exact source commit and matching frontend, backend, and migration artifacts. Do not package `.env` files or secrets.
2. Confirm approved hosting, TLS termination/certificate renewal owner, PostgreSQL provider/network/TLS, object-storage adapter, email delivery mode (`EMAIL` or `MANUAL_TRUSTED_CHANNEL`), backup policy, and monitoring ownership.
3. Complete the readiness checklist, including current backup evidence and restore-test record.
4. Run the controlled migration once with the authorized migration identity. Ordinary API startup never applies migrations automatically; do not assume an EF `Down` migration is a safe rollback.
5. Deploy the backend without traffic as approved. Verify `/health/live`, then `/health/ready`; stop on failure.
6. Deploy the static frontend. Verify HTTPS, headers, cache policy (`index.html` not immutable; hashed assets may be immutable), no public source maps unless separately approved, and the selected topology/CORS behavior.
7. Shift traffic only after readiness is healthy. Run the smoke tests below and record release version, trace IDs, and evidence links without secrets.

Application-owned headers include `X-Content-Type-Options`, frame denial, referrer policy, permissions policy, and private API response caching. The final proxy/static-host CSP, HSTS, content-type, cache, and header policy must be tested after host selection; CSP must not use wildcard sources or `unsafe-eval`.

Smoke tests use approved synthetic least-privilege data where allowed: authentication, CSRF mutation, authorization/team scope, database read/write, authorized media and import streaming, dashboard, availability privacy, restricted medical access, audit history, rate limits, restart session continuity, and backup evidence. Never add real medical notes or publish passwords in a runbook.

Rollback: stop traffic/deployment, preserve safe logs and migration outcome, restore service artifact if compatible, and use a rehearsed database restore only under the database owner. Do not automatically downgrade schema.

