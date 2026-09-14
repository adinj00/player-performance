# Player analysis

These features turn reliable match data into player centered understanding. They follow the manual data path so the club analyzes its own reviewed information.

## Slice 2: FK Velež player analysis

### 12. Player profile foundation · existing

The player detail view shows identity, lifecycle, current selections, assignment history, safe availability, restricted medical shortcuts where permitted, and linked media.

**Done when:** staff can open one persistent player record and understand the supported club history and current context without seeing data outside their role and team scope.

Legacy specs Units 29, 42, 49, and 51 · code in `frontend/src/features/players/`

### 13. Player performance history and trends · needs a decision

Give staff a detailed player oriented view of match appearances and reviewed performance over time, with useful filters and comparisons that remain grounded in recorded club data.

**Done when:** an authorized staff member can open a player, inspect match by match statistics, filter a period or competition, and understand trends without mixing incompatible or unverified data.

- [ ] Design it (spec): `/architect player performance history and trends`

### 14. Team dashboard overview · existing

The dashboard provides team and season context, recent matches, report workflow, safe availability, verified report leaders, and data quality alerts. Its current implementation also contains workload summaries, which are outside the active direction recorded by this scope.

**Done when:** each authorized role receives one bounded overview that respects team scope and does not reconstruct hidden workflow, medical, or incompatible workload data.

Legacy specs [Unit 52](../../context/feature-specs/52-dashboard-backend-read-models.md) and [Unit 53](../../context/feature-specs/53-dashboard-ui.md) · code in `backend/src/Api/Endpoints/DashboardEndpoints.cs` and `frontend/src/features/dashboard/`

## Slice 3: Upcoming opponent preparation

### 15. Opponent player analysis and match preparation · needs a decision

Extend the existing opponent team records only where opponent players and their performance help staff prepare for an upcoming match. This must remain secondary to FK Velež player analysis.

**Done when:** authorized staff can maintain relevant opponent players, connect them to upcoming opponents, and review preparation focused information without turning the product into a broad scouting platform.

- [ ] Design it (spec): `/architect opponent player analysis and match preparation`

Existing opponent team code in `backend/src/Application/Settings/` and `frontend/src/features/settings/`. No opponent player implementation was found.
