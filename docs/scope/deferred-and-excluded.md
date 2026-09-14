# Deferred and excluded

These items are not part of the active manual entry and player analysis direction. They remain visible so existing code and earlier plans are not mistaken for current priority.

## Deferred

### 19. Production deployment · in-progress · needs a decision

Application level production safeguards and runbooks exist, but real deployment remains blocked by hosting, storage, database, backup, email, monitoring, and operating decisions. No provider should be selected through this scope.

**Done when:** approved infrastructure decisions are recorded, required production adapters and operations are implemented and verified, and the readiness checklist can truthfully move from blocked to ready.

- [ ] Decide the deployment boundary (spec): `/architect production deployment`

Legacy spec [Unit 57](../../context/feature-specs/57-production-configuration-readiness.md) · code and operations guidance in `backend/src/Api/`, `frontend/src/lib/`, and `context/operations/`

## Excluded from the active direction

### 20. Training and physical workload · dropped

Training session and physical workload code exists, including backend models, routes, and interface surfaces. The current product direction excludes GPS and training data integration, so this capability is recorded for history and is not active work.

Code in `backend/src/Api/Endpoints/Training/`, `backend/src/Application/Physical/`, and `frontend/src/features/training/`

### 21. Vendor data integrations · dropped

GPeXe, GPS, Zone14, and other vendor integrations are outside the current scope. No vendor processor should be activated, no export fields should be guessed, and the generic preview foundation does not count as an integration.

The legacy [Unit 46](../../context/feature-specs/46-gpexe-sample-review-mapping-gate.md) remains evidence of an earlier blocked plan, not active scope.

### 22. Video analysis · dropped

Video analysis, automated event detection, camera integration, and similar workflows are outside the current scope. Existing media references remain simple supporting records and do not imply video analysis capability.
