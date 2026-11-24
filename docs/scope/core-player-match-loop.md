# Core player and match loop

This is the central Tracer Bullet path. Staff configure the football context, maintain players, create a match, record participation and statistics, then review the report.

## Slice 1: Manual player match data

### 5. Club reference data · existing

Administrators manage seasons, competitions, FK Velež selections, tracking levels, venues, and opponent teams needed by player and match records.

**Done when:** staff can select valid active reference data for players and matches while archived records remain available as history.

- [ ] Verify it: `/check verify club reference data`
- [ ] Test it: `/test club reference data`

Legacy specs Units 21, 22, 25, and 26 · code in `backend/src/Api/Endpoints/Settings/`, `backend/src/Api/Endpoints/Teams/`, and `frontend/src/features/settings/`

### 6. Player records and selection history · existing

Players are persistent club entities with lifecycle state and time bounded assignment history across FK Velež selections.

**Done when:** an authorized user can create and maintain one player record, track past, current, and future selection assignments, and preserve history when the player changes status.

- [ ] Verify it: `/check verify player records and selection history`
- [ ] Test it: `/test player records and selection history`

Legacy specs Units 27 to 29 · code in `backend/src/Application/Players/`, `backend/src/Api/Endpoints/Players/`, and `frontend/src/features/players/`

### 7. Match management · existing

Staff can manage match metadata for an FK Velež selection using season, competition, opponent, venue, location, score, lifecycle, and archive context.

**Done when:** an authorized user can create, find, update, and archive a match within team scope, with valid references and workflow locks enforced by the backend.

- [ ] Verify it: `/check verify match management`
- [ ] Test it: `/test match management`

Legacy specs Units 30 and 34 · code in `backend/src/Application/Matches/`, `backend/src/Api/Endpoints/Matches/`, and `frontend/src/features/matches/`

### 8. Lineups and appearances · existing

Each match can record starters, substitutes, captain, player appearances, minutes, and ordered substitutions using players eligible for that selection on the match date.

**Done when:** authorized staff can save a valid lineup and appearance snapshot, and report workflow locks prevent unsafe changes.

- [ ] Verify it: `/check verify lineups and appearances`
- [ ] Test it: `/test lineups and appearances`

Legacy specs Units 31 and 35 · code in `backend/src/Domain/Matches/`, `backend/src/Application/Matches/`, and `frontend/src/features/matches/`

### 9. Manual match statistics · in-progress · GA

Staff can manually record tracking level driven player and goalkeeper statistics for concrete match appearances. The backend is complete and the current interface is usable, but the Unit 36 audit records unfinished validation, conflict recovery, navigation protection, and test requirements.

**Done when:** the full player and goalkeeper statistics grid supports fast entry, clear field feedback, safe recovery from conflicts, protected unsaved work, and focused automated coverage.

Reproduce the reported draft loss and missing field feedback. Use `/architect` if conflict recovery or navigation protection needs a durable decision before completion.

- [x] Build the backend and the usable interface from legacy specs Units 33 and 36
- [ ] Baseline it: `/check verify manual match statistics`
- [ ] Complete it: `/develop manual match statistics`
- [ ] Verify it after changes: `/check verify manual match statistics`
- [ ] Test it: `/test manual match statistics`
- [ ] Review it: `/check review manual match statistics`
- [ ] Document it: `/document manual match statistics`

Legacy specs [Unit 33](../legacy/context/feature-specs/33-manual-match-statistics-backend.md) and [Unit 36](../legacy/context/feature-specs/36-manual-match-statistics-ui.md) · code in `backend/src/Application/Matches/MatchStatisticsService.cs` and `frontend/src/features/matches/`

### 10. Match report review · in-progress · GA

Reports move through draft, review, correction, verification, and archive states using backend allowed actions. The implementation and automated checks exist, while the repository still records a component limitation and incomplete authenticated browser verification.

**Done when:** data operators can submit, analysts can review or request correction, permitted staff can verify, and every role sees only allowed report data and actions in the real application.

- [x] Build the backend and interface from legacy specs Units 32 and 37
- [ ] Verify it: `/check verify match report review`
- [ ] Test it: `/test match report review`
- [ ] Review it: `/check review match report review`
- [ ] Document it: `/document match report review`

Legacy specs [Unit 32](../legacy/context/feature-specs/32-match-report-workflow-backend.md) and [Unit 37](../legacy/context/feature-specs/37-match-report-review-ui.md) · code in `backend/src/Application/Matches/MatchReportsService.cs` and `frontend/src/features/matches/`

### 11. Audit history · in-progress · GA

Important staff, report, statistics, media, import, and availability changes use append only audit records with permission aware history views. The initial audit foundation exists, but the progress record names coverage that was intentionally deferred and does not establish complete coverage for every important mutation.

**Done when:** every important active scope mutation records safe actor, action, entity, time, and meaningful change data, and authorized staff can inspect the relevant history.

Check the reported prior status defect and gaps for player, assignment, match and lineup changes. The set of important reference data mutations remains a scope decision.

- [x] Build the initial audit foundation from legacy specs Units 38 and 39
- [ ] Baseline it: `/check verify audit history`
- [ ] Complete it: `/develop audit history`
- [ ] Verify it after changes: `/check verify audit history`
- [ ] Test it: `/test audit history`
- [ ] Review it: `/check review audit history`
- [ ] Document it: `/document audit history`

Legacy specs [Unit 38](../legacy/context/feature-specs/38-audit-backend-foundation.md) and [Unit 39](../legacy/context/feature-specs/39-audit-ui-foundation.md) · code in `backend/src/Application/Auditing/`, `backend/src/Infrastructure/Auditing/`, and `frontend/src/features/audit/`
