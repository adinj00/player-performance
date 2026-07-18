# Production Readiness Checklist

> This document contains readiness status and evidence references only.
> Never place real passwords, tokens, connection strings, certificates, storage credentials, or secret values here.

## Release

| Field | Value |
|---|---|
| Status | BLOCKED |
| Release version | |
| Source commit | |
| Frontend artifact | |
| Backend artifact | |
| Migration set | |
| Planned deployment date | |
| Checklist updated at | |

Allowed status:

```txt
READY
BLOCKED
ACCEPTED_LIMITATION
NOT_APPLICABLE
```

## Owners and approvals

| Responsibility | Owner | Approval/date |
|---|---|---|
| Product owner | | |
| Technical owner | | |
| Database/backup owner | | |
| Security/privacy owner | | |
| Club operational owner | | |
| Deployment executor | | |

## Mandatory infrastructure decisions

| Decision | Status | Approved value/evidence | Owner | Review date |
|---|---|---|---|---|
| Hosting provider/topology | BLOCKED | | | |
| DNS and TLS ownership | BLOCKED | | | |
| PostgreSQL provider | BLOCKED | | | |
| Object-storage provider | BLOCKED | See `object-storage-decision.md` | | |
| Data Protection persistence | BLOCKED | | | |
| Email/manual invitation mode | BLOCKED | | | |
| Backup retention/RPO/RTO | BLOCKED | | | |
| Single or multi-instance API | BLOCKED | | | |
| Log/monitoring destination | BLOCKED | | | |
| Docker/container decision | NOT_APPLICABLE | Not required unless separately approved | | |

## Application configuration

| Check | Status | Evidence/notes |
|---|---|---|
| Production uses real environment variables/managed secret store | BLOCKED | |
| No production `.env` dependency | BLOCKED | |
| Public web/API origins are exact HTTPS origins | BLOCKED | |
| Deployment topology selected/tested | BLOCKED | |
| Exact CORS origins configured | BLOCKED | |
| Secure HttpOnly cookie verified | BLOCKED | |
| CSRF mutation verified | BLOCKED | |
| Trusted proxy/network configured | BLOCKED | |
| HSTS/HTTPS verified | BLOCKED | |
| Data Protection keys persist across restart | BLOCKED | |
| OpenAPI/developer errors disabled | BLOCKED | |
| Rate-limit values approved | BLOCKED | |
| Frontend API base validated | BLOCKED | |
| No secrets in frontend artifact | BLOCKED | |

## Database

| Check | Status | Evidence/notes |
|---|---|---|
| Least-privilege runtime identity | BLOCKED | |
| Migration identity/process | BLOCKED | |
| TLS/network restriction | BLOCKED | |
| Current backup successful | BLOCKED | |
| Pre-migration snapshot identified | BLOCKED | |
| Pending migrations reviewed | BLOCKED | |
| Migration applied once | BLOCKED | |
| Restore procedure tested | BLOCKED | |
| RPO approved | BLOCKED | |
| RTO approved | BLOCKED | |

## File and object storage

| Check | Status | Evidence/notes |
|---|---|---|
| Production provider selected | BLOCKED | |
| Provider-specific adapter implemented | BLOCKED | |
| Private bucket/container | BLOCKED | |
| TLS and at-rest encryption | BLOCKED | |
| Streaming upload/read verified | BLOCKED | |
| Range/video behavior verified | BLOCKED | |
| Compensation delete verified | BLOCKED | |
| Health check verified | BLOCKED | |
| Versioning/recovery policy | BLOCKED | |
| Retention/lifecycle policy | BLOCKED | |
| Storage limits/cost owner | BLOCKED | |
| Local provider rejected in Production | BLOCKED | |

## Temporary working storage

| Check | Status | Evidence/notes |
|---|---|---|
| Private directory configured | BLOCKED | |
| Not inside source/web/static/storage root | BLOCKED | |
| Capacity aligned to import limits | BLOCKED | |
| Cleanup verified | BLOCKED | |
| Disk-pressure response documented | BLOCKED | |

## Security and privacy

| Check | Status | Evidence/notes |
|---|---|---|
| Secrets inventory complete | BLOCKED | |
| Rotation/revocation owners named | BLOCKED | |
| Security headers verified | BLOCKED | |
| CSP enforced | BLOCKED | |
| No public caching of protected responses | BLOCKED | |
| Medical privacy smoke test passed | BLOCKED | |
| Team-scope authorization smoke test passed | BLOCKED | |
| Audit write/read privacy verified | BLOCKED | |
| Accessibility audit has no blocking/high findings | BLOCKED | |

## First admin and onboarding

| Check | Status | Evidence/notes |
|---|---|---|
| First-admin bootstrap procedure approved | BLOCKED | |
| Temporary password handled as secret | BLOCKED | |
| Required password change verified | BLOCKED | |
| Bootstrap disabled/secret removed afterward | BLOCKED | |
| Invitation delivery mode works | BLOCKED | |
| Setup links are one-time/expiring | BLOCKED | |
| Recovery limitation documented | BLOCKED | |

## Build and test evidence

| Check | Status | Evidence/notes |
|---|---|---|
| Backend format/build/tests | BLOCKED | |
| Frontend format/lint/typecheck/build/tests | BLOCKED | |
| Production config tests | BLOCKED | |
| Cookie/CORS/CSRF tests | BLOCKED | |
| Forwarded-header tests | BLOCKED | |
| Health tests | BLOCKED | |
| Rate-limit tests | BLOCKED | |
| Frontend environment tests | BLOCKED | |
| Audit/accessibility document complete | BLOCKED | |

## Deployment and smoke tests

| Check | Status | Evidence/notes |
|---|---|---|
| Release runbook reviewed | BLOCKED | |
| Rollback owner/plan | BLOCKED | |
| `/health/live` | BLOCKED | |
| `/health/ready` | BLOCKED | |
| Sign in/password change/sign out | BLOCKED | |
| CSRF-protected mutation | BLOCKED | |
| Role/team-scope authorization | BLOCKED | |
| Database write/read | BLOCKED | |
| Media upload/open/download | BLOCKED | |
| Import upload/download/preview | BLOCKED | |
| Dashboard | BLOCKED | |
| Safe availability privacy | BLOCKED | |
| Restricted injury authorization | BLOCKED | |
| Audit event/query | BLOCKED | |
| Rate-limit response | BLOCKED | |
| Restart/session continuity | BLOCKED | |
| Static caching/security headers | BLOCKED | |
| Safe logs reviewed | BLOCKED | |

## Accepted limitations

| Limitation | Impact | Owner | Approval date | Review/expiry date | Fallback |
|---|---|---|---|---|---|

## Blocking items

- Production object-storage provider and adapter are unresolved.
- Add other active blockers here.

## Final sign-off

```txt
Final status: BLOCKED
```

| Role | Name | Date | Decision |
|---|---|---|---|
| Product | | | |
| Technical | | | |
| Security/privacy | | | |
| Operations | | | |
