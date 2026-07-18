# Production Object Storage Decision

> Status: `UNDECIDED`
>
> Complete this record before writing a production provider adapter.
> Never place real access keys, secret keys, tokens, or full credential-bearing endpoints in this document.

## Decision metadata

| Field | Value |
|---|---|
| Status | UNDECIDED |
| Owner | |
| Reviewers | |
| Decision date | |
| Review date | |
| Related implementation spec | |
| Region/data residency | |

Allowed status:

```txt
UNDECIDED
UNDER_REVIEW
APPROVED
REJECTED
SUPERSEDED
```

## Candidate providers

| Candidate | Product/service | Region | Reason considered | Rejection/approval evidence |
|---|---|---|---|---|

## Required security properties

| Requirement | Candidate evidence | Result |
|---|---|---|
| Private objects by default | | |
| No public bucket/container | | |
| TLS in transit | | |
| Encryption at rest | | |
| Least-privilege service credentials | | |
| Credential rotation/revocation | | |
| Access logging/audit support | | |
| Data residency acceptable | | |
| No browser exposure of storage keys | | |

## Required application capabilities

| Capability | Candidate evidence | Result |
|---|---|---|
| Official mature .NET SDK | | |
| Streaming upload without full buffering | | |
| Large-object/multipart support | | |
| Streamed read | | |
| Cancellation | | |
| Idempotent compensation delete | | |
| Object collision handling | | |
| Private object metadata/head operation | | |
| Health-check strategy | | |
| Retry/transient-error semantics | | |
| Range/video seeking compatibility | | |
| Test environment/emulator | | |
| Existing `IFileStorage` contract fit | | |

## Limits and operations

| Item | Value/evidence |
|---|---|
| Maximum object size | |
| Request/part limits | |
| Storage quota/alerts | |
| Egress considerations | |
| Multipart cleanup | |
| Versioning | |
| Retention/lifecycle | |
| Accidental-delete recovery | |
| Backup/replication | |
| Restore test | |
| Cost owner | |
| Support/SLA | |

## Media behavior

- Browser receives content only through authorized application endpoints.
- Storage keys and provider URLs remain hidden.
- Image/video/PDF streaming:
- Range request/video seeking behavior:
- Browser-direct/presigned access decision: `OUT OF SCOPE` unless separately approved.
- Cache behavior:

## Import behavior

- Original CSV/XLSX retention:
- Stream open behavior:
- Non-seekable stream implications:
- Temporary materialization behavior:
- Compensation behavior:
- Audit/provenance implications:

## Configuration contract

List names only, never real values:

```txt
FileStorage__Provider=
# provider-specific names after approval
```

Classify each provider-specific value:

| Environment name | Secret? | Required | Validation | Rotation owner |
|---|---:|---:|---|---|

## Integration-test plan

- Write/open/read/delete uncommitted object.
- Large streamed upload.
- Cancellation.
- Collision.
- Missing object.
- Provider outage.
- Transient retry.
- Authorized media content.
- Import source download.
- Range behavior where supported.
- Health check.
- No key/URL leakage.
- Cleanup after failed database commit.

## Decision

```txt
Selected provider:
Decision:
Why:
Known limitations:
Production block removed: NO
```

Production block may change to `YES` only after:

- status is `APPROVED`;
- provider-specific implementation spec is approved;
- adapter is implemented;
- configuration validation passes;
- health check passes;
- integration tests pass;
- backup/recovery behavior is approved.
