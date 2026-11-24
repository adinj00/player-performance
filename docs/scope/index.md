# Scope: Player Performance

Player Performance is the internal FK Velež Mostar system for collecting, managing, and analyzing football player performance data. It serves club staff, with the player and the player’s match performance as the center of the product.

**Build approach:** Tracer Bullet (future work should prove a real path through data, API, interface, and staff behavior).
**Workflow:** Beta (`/check verify`, then `/test`). Features tagged `· GA` add an independent `/check review` and `/document` for changes. GA applies to the shared migration and persistence baseline, authentication and permissions, official statistics and report integrity, audit history, restricted data and file handling, and production readiness.

_These are recommendations that keep future work orderly. You may skip a stage when it does not fit, and you decide when a feature is done._

The [as-built feature inventory](../reviews/2026-11-23-as-built-feature-inventory.md) records implementation and historical evidence. The Status column carries that implementation baseline. Current runtime verification is **UNVERIFIED / NOT RUN** for the 16 implemented or partially implemented active features. Production deployment (19) is **BLOCKED** pending approved operating decisions and a production storage adapter. The planned and dropped features have no current runtime verdict.

## At a glance

| # | Feature | Phase | Status |
|---|---------|-------|--------|
| 1 | Application foundation | Foundation | existing |
| 2 | Staff authentication and sessions | Foundation | existing |
| 3 | Staff access and account administration | Foundation | in-progress |
| 4 | Responsive shell and accessibility | Foundation | existing |
| 5 | Club reference data | Slice 1 | existing |
| 6 | Player records and selection history | Slice 1 | existing |
| 7 | Match management | Slice 1 | existing |
| 8 | Lineups and appearances | Slice 1 | existing |
| 9 | Manual match statistics | Slice 1 | in-progress |
| 10 | Match report review | Slice 1 | in-progress |
| 11 | Audit history | Slice 1 | in-progress |
| 12 | Player profile foundation | Slice 2 | existing |
| 13 | Player performance history and trends | Slice 2 | planned |
| 14 | Team dashboard overview | Slice 2 | existing |
| 15 | Opponent player analysis and match preparation | Slice 3 | planned |
| 16 | Availability and restricted medical details | Supporting | existing |
| 17 | Media references | Supporting | existing |
| 18 | Generic import preview | Supporting | in-progress |
| 19 | Production deployment | Deferred | in-progress |
| 20 | Training and physical workload | Deferred | dropped |
| 21 | Vendor data integrations | Deferred | dropped |
| 22 | Video analysis | Deferred | dropped |

## Areas

[Foundations and access](foundations-and-access.md) has three existing features and one in progress. Start with application foundation, then authenticate and verify staff access.

[Core player and match loop](core-player-match-loop.md) has four existing features and three in progress. Baseline manual statistics before its remaining build work and collect audit evidence while checking mutations.

[Player analysis](player-analysis.md) has two existing features and two planned. Verify the profile and dashboard once their data sources are proven. Player performance history is the next planned capability.

[Supporting capabilities](supporting-capabilities.md) has two existing features and one in progress. Verify availability and media before their dependent views, and generic import preview after local storage.

[Deferred and excluded](deferred-and-excluded.md) has one deferred feature in progress and three dropped features. Production verification is blocked. Surviving training and workload exposure is recorded as drift.

## Verification order

**Next:** `/check verify application foundation`.

1. **1 → 2 → 4 (shell smoke).** Apply migrations explicitly to disposable PostgreSQL, inspect the migration history and schema, and prove API health, structured errors, and browser connectivity. Then prove staff bootstrap, CSRF, first password change, protected reload and logout. Use that session to smoke check shell navigation and focus.
2. **3 → 5 → 6 → 7 → 8.** Start staff lifecycle checks, then use club reference workflows to establish two team contexts and finish testing selected team access. Create a player, dated assignment, played match, lineup and appearances. Reuse the fixtures for allowed and denied cases.
3. **9 → 10.** Baseline manual statistics and resolve gaps that block reliable reviewed data. Verify report transitions and permission boundaries, including correction and locks on match, lineup and statistics edits.
4. **16 → 17 → 18 → 12 and 14 → 4 (full workflows) → 11 (close).** Verify restricted availability and media before their consumers. Verify import preview after local storage and relevant match context. Complete profile and dashboard checks with the proven data, then finish keyboard and responsive checks with representative records. Collect audit evidence during 3, 5–10 and 16–18, then close the history check across its writers.
5. **Later:** design 13 after 8–10 establish reliable reviewed data, then design 15 after the player history direction is clear. Feature 19 waits for its deferred decision. Features 20–22 have no verification campaign.

Use Playwright CLI or MCP for browser observations and the existing xUnit and Vitest suites for regression coverage. Use real PostgreSQL for migrations, relational constraints, query translation, transactions and concurrency. Add maintained Playwright E2E tests selectively through `/test` when a critical journey and its regression risk justify them.

Record each runtime outcome and its evidence through `/check verify`. Use `/debug` for observed defects, `/architect` for unresolved contracts, and `/develop` for scoped gaps or justified refactoring. Capture before and after behavior when making changes, and rerun the affected checks before the tier's remaining steps.

## Current boundary

The active direction starts with manual match and player statistics entry, then builds player analysis from that reliable club owned data. Opponent information belongs only where it improves preparation for upcoming matches.

No current slice includes video analysis, Zone14 integration, GPeXe or GPS integration, training data integration, public registration, player login, multi club behavior, billing, or speculative external integrations.

English localization remains unscheduled. Bosnian Latin is the only current interface language.

## Legend

**Next step:** follow the verification order across features, then the first applicable unticked box in the feature section.

**needs a decision:** `/architect` is the recommended next step because an important product or technical decision is still open.

**Status:** `planned` means approved scope awaiting implementation. `in-progress` means partial, blocked, or historically incomplete. `existing` means the repository records earlier completion evidence. `dropped` means outside the active direction. Completed legacy boxes record historical work. Current acceptance requires fresh evidence.

**Workflow:** a heading without a tier tag inherits Beta. A GA tag on a dropped feature records the depth if it is reopened.

**Pointers:** existing feature specs are [legacy planning evidence](../legacy/context/feature-specs/00-build-plan.md) under `docs/legacy/context/feature-specs/`. They are not new `/architect` specifications under `docs/specs/`.
