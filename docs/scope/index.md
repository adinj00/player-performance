# Scope: Player Performance

Player Performance is the internal FK Velež Mostar system for collecting, managing, and analyzing football player performance data. It serves club staff, with the player and the player’s match performance as the center of the product.

**Build approach:** Tracer Bullet (future work should prove a real path through data, API, interface, and staff behavior).
**Workflow:** GA (`/check verify`, then `/test`, then `/check review`, then `/document` after `/develop`). This is the project default because authentication, permissions, medical details, and audit history need careful verification. `/architect` remains the recommended first stop when a real decision is still open.

_These are recommendations that keep future work orderly. You may skip a stage when it does not fit, and you decide when a feature is done._

This is a brownfield baseline. `existing` means the repository records explicit completion and verification evidence. It does not mean this scope run repeated that verification. `in-progress` means the implementation is partial, blocked, or not verified well enough to call complete. Code presence alone is not treated as proof.

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

[Foundations and access](foundations-and-access.md) has three existing features and one feature in progress. The next unfinished step is real application verification of staff access and account administration.

[Core player and match loop](core-player-match-loop.md) has four existing features and three features in progress. The recommended first resume point is `/develop manual match statistics`.

[Player analysis](player-analysis.md) has two existing features and two planned features. Detailed player performance over time is the next player centered capability after the manual data path is complete.

[Supporting capabilities](supporting-capabilities.md) has two existing features and one feature in progress. They support the core loop but do not replace manual entry or player analysis.

[Deferred and excluded](deferred-and-excluded.md) keeps one incomplete deployment concern and three capabilities outside the active direction. Existing training and physical workload code remains recorded without making it active scope.

## Current boundary

The active direction starts with manual match and player statistics entry, then builds player analysis from that reliable club owned data. Opponent information belongs only where it improves preparation for upcoming matches.

No current slice includes video analysis, Zone14 integration, GPeXe or GPS integration, training data integration, public registration, player login, multi club behavior, billing, or speculative external integrations.

English localization remains unscheduled. Bosnian Latin is the only current interface language.

## Legend

**Next step:** the first unticked box in a feature section.

**needs a decision:** `/architect` is the recommended next step because an important product or technical decision is still open.

**Status:** `planned` means approved scope that has not started. `in-progress` means partial or not verified enough. `existing` means it predates this scope and has explicit repository completion evidence. `dropped` means it is outside the active direction and stays visible for history.

**Workflow:** GA normally runs `/check verify`, `/test`, `/check review`, and `/document` after `/develop`. A feature can use a different level when its heading says so.

**Pointers:** existing feature specs are [legacy planning evidence](../legacy/context/feature-specs/00-build-plan.md) under `docs/legacy/context/feature-specs/`. They are not new `/architect` specifications under `docs/specs/`.
