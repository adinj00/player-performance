# Object-storage production decision

Status: BLOCKED. Owner: Technical owner / security owner. No provider has been selected or integrated.

`FileStorage__Provider=Local` is Development-only. Production must not use a fake or no-op storage provider. Before deployment, approve a provider-specific adapter that implements the existing `IFileStorage` abstraction without exposing provider types to Application code, and integration-test writes, reads, compensation deletion, health, limits, streaming, range behavior, authorization, recovery, and failure behavior.

The decision record must cover private access, service identity/least privilege, encryption, data residency, lifecycle/versioning/retention, backup/recovery, object size and temporary-disk limits, malware/operational controls if approved later, observability, and cost/ownership. Browser-direct public object access and presigned browser uploads are not introduced; media/import authorization remains API-owned.
