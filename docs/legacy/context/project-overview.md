# Player Performance Data System

## Overview

Player Performance Data System is an internal football performance platform for FK Velež Mostar. It gives club staff a structured, club-owned database for match performance, player development, physical workload, media references, imports, review workflows, and reporting across the first team and youth selections. The application is not a public statistics site; it is an operational tool for coaches, analysts, data operators, medical staff, and administrators who need reliable internal performance data where public league data is incomplete or unavailable.

## Goals

1. Build a reliable internal performance database for FK Velež Mostar players, teams, matches, and training workload.
2. Support match-centered analysis in V1, including match metadata, lineups, player appearances, player statistics, goalkeeper statistics, GPS/physical metrics, review status, and media references.
3. Track players as persistent club entities as they move through selections, instead of recreating player records for each team.
4. Support configurable club selections and tracking levels so different teams can collect different levels of data.
5. Provide controlled access for staff through role-based permissions, team scopes, and audit logging.
6. Support manual data entry first, with CSV/XLSX imports for known data sources where real export formats are confirmed.
7. Keep the import architecture extensible because exact Gpexe and Zone14 export fields are not fully confirmed yet.
8. Provide a professional, light-only FK Velež user interface using red, white, and gold club identity.
9. Use Bosnian Latin as the default application language, with architecture that can support English as an optional selectable UI language.

## Core User Flow

1. An administrator seeds or creates the first admin account through secure environment-based configuration.
2. The administrator signs in and configures seasons, selections, tracking levels, competitions, venues, opponents, and staff users.
3. The administrator invites staff users and assigns each user one primary role and either all-team access or selected team scopes.
4. Staff users accept their invite, set their password, and access only the teams and workflows allowed by their role and scope.
5. A data operator, analyst, coach, or administrator creates a match for a specific season, competition, and team selection.
6. Authorized staff enter match metadata, lineup, substitutions, player minutes, technical statistics, goalkeeper statistics, notes, and media references.
7. Authorized staff import CSV/XLSX data when a supported and validated import format is available.
8. The match report remains in draft until the data operator submits it for review.
9. An analyst reviews the report, requests corrections if needed, or verifies the report if they have verification permission.
10. Coaches and authorized staff view verified reports, dashboards, player profiles, availability summaries, GPS metrics, and media references.
11. Administrators and authorized staff can inspect audit history for important data and workflow changes.

## Features

### Authentication and User Management

- Closed staff-only authentication with no public self-registration.
- Secure login, logout, session validation, forgot password, and password reset.
- Environment-seeded first admin account when no admin exists.
- Admin-created staff invitations using one-time setup links.
- User account statuses: invited, active, disabled, and locked.
- User disable/reactivate flows that preserve audit history instead of physically deleting accounts.
- Audited login email recovery for returning staff when needed.

### Roles, Team Scopes, and Permissions

- One primary role per user in V1.
- Supported roles: admin, data operator, coach, analyst, medical staff, and viewer.
- Non-admin access is restricted by team scope.
- Team scope supports all teams or selected teams.
- Analysts can review match reports by default.
- Analysts can verify reports only when explicitly granted report verification permission.
- Admin users have full system access by default.
- Backend authorization is the source of truth for all protected actions.

### Teams and Selections

- Configurable club selections managed by administrators.
- Default selections: First Team, U19, U17, U15, U13, and U11.
- Configurable tracking levels: basic, standard, and full.
- Default tracking levels: First Team full, U19 standard, U17 standard, U15 basic, U13 basic, and U11 basic.
- Tracking levels control which workflows, data entry fields, and reports are available for each selection.
- Active, inactive, archived, renamed, and reordered selections.

### Players and Club Development Path

- Players are persistent club entities across selections.
- Player movement through the club is tracked with time-bound team assignment history.
- A player may have multiple active assignments if they train or play with more than one selection.
- Match statistics are linked to concrete match appearances, not only to a player's current team assignment.
- Player profile pages show current status, team assignment history, match appearances, performance data, physical metrics, availability, and notes.

### Matches and Match Reports

- Match metadata: season, competition, round, date/time, opponent, home/away/neutral, venue, score, and match status.
- Team lineup: starting XI, substitutes, formation, captain, substitutions, and player minutes.
- Player statistics: goals, assists, cards, shots, shots on target, passes attempted/completed, key passes if tracked, duels attempted/won, fouls committed/won, offsides, ball recoveries, possession losses, and similar tracked metrics.
- Goalkeeper statistics: saves, goals conceded, clean sheet, punches/claims if tracked, and penalty saves if tracked.
- Match report workflow statuses: draft, ready for review, verified, needs correction, and archived.
- Backend-provided allowed actions based on role, team scope, report status, and explicit permissions.

### GPS and Physical Metrics

- Support aggregate GPS/physical metrics from Gpexe imports where export fields are confirmed.
- Match and training physical metrics may include total distance, high-speed running distance, sprint distance, number of sprints, max speed, accelerations, decelerations, player load, and session duration when available.
- Training support in V1 focuses on GPS/physical workload tracking, not full manual technical training-event analysis.
- Exact Gpexe fields must be finalized only after real export samples are reviewed.

### Imports

- CSV/XLSX import support for structured data where formats are known and validated.
- Import types may include match player stats, match GPS/physical stats, training GPS/physical stats, and player roster data.
- Import workflow includes upload, type selection, preview, column mapping when needed, validation, error reporting, and confirmation.
- Import source files are stored as media/import artifacts so original data can be audited later.
- Unknown or unconfirmed vendor fields must not be guessed in implementation.

### Media and External References

- Support uploaded media files and external media references.
- Media can be linked to matches, training sessions, players, import jobs, or generated reports where relevant.
- Zone14 is treated in V1 as a source for video, tagging/supporting references, and possibly running statistics only when confirmed by real exports.
- The system must not assume Zone14 provides full event data such as passes, shots, duels, or ball actions unless confirmed by real exports.

### Medical and Availability

- Availability statuses: available, limited, unavailable, rehab, and unknown.
- Medical staff can manage availability and injury-related records for assigned teams.
- Coaches can view availability summaries for assigned teams.
- Sensitive medical notes remain restricted to authorized roles.

### Dashboard and Reporting

- Dashboard views for selected season and team.
- Recent matches, report review status, player availability summary, top performers, and physical workload summaries where data exists.
- Player and match detail pages for staff analysis.
- Data quality alerts for reports waiting for review, reports needing correction, failed imports, and missing required data.


### UI Language and Localization

- The default application language is Bosnian Latin.
- English may be supported as an optional selectable UI language.
- The application should be built so user-facing UI copy can be localized without rewriting screens later.
- Domain data such as player names, team names, competitions, opponents, and notes remains user-entered content and is not automatically translated.
- Technical enum values may remain in English internally, but displayed labels must be localized for users.

### Audit and Review

- Audit logging for important mutations, including match report changes, player stats, imports, medical data, user role/scope changes, account disable/reactivate, and login email changes.
- Review workflows preserve who entered, reviewed, verified, corrected, and imported data.
- Verified data should not be casually overwritten without a workflow transition and audit trail.

## Scope

### In Scope

- Single-club internal application for FK Velež Mostar.
- First team and configurable youth selections.
- Staff-only authentication and role-scoped access.
- Admin user management, staff invitations, account lifecycle, and audited recovery flows.
- Configurable seasons, teams/selections, tracking levels, competitions, opponents, and venues.
- Player records with team assignment history.
- Match metadata, lineup, appearances, substitutions, player minutes, and match report workflow.
- Manual match data entry.
- CSV/XLSX imports for supported and validated formats.
- Aggregate GPS/physical metrics from confirmed Gpexe export fields.
- Uploaded files and external media references.
- Basic training-session support for GPS/physical workload tracking.
- Availability and injury-related data with restricted medical access.
- Dashboards and detail pages for match, player, team, and workload analysis.
- Audit logs for important data, workflow, import, and account changes.
- Light-only FK Velež visual identity using red, white, and gold.
- Bosnian Latin as the default UI language, with optional English UI support.

### Out of Scope

- Public-facing statistics site or public fan portal.
- Player login or player-facing self-service features.
- Public registration.
- Multi-club or multi-tenant SaaS support.
- Billing, subscriptions, payments, or commercial tenant management.
- Native mobile applications.
- Full scouting platform features.
- Automated event detection from video.
- Assumed full Zone14 event-data integration without real export confirmation.
- Direct external vendor API integrations unless access, documentation, licensing, and stability are confirmed.
- Full manual technical/event analysis for training sessions in V1.
- Advanced predictive analytics, AI recommendations, or machine learning models.
- Docker-based local development in the initial setup.
- Production hosting provider selection in the initial context phase.

## Success Criteria

1. An administrator can configure the club's seasons, selections, tracking levels, and staff access without hardcoded teams.
2. A staff user can sign in and only access the teams and workflows allowed by their role, team scope, and explicit permissions.
3. A player can move through multiple selections while preserving one persistent player profile and assignment history.
4. Authorized staff can create a match, enter lineup and player statistics, attach media references, and submit the match report for review.
5. An analyst with verification permission or an administrator can verify a match report through the defined workflow.
6. Coaches can view verified match reports, player profiles, dashboards, availability summaries, and physical metrics for their assigned teams.
7. CSV/XLSX imports can be previewed, validated, confirmed, and audited for supported formats without guessing unknown vendor fields.
8. Important changes to reports, imports, users, roles/scopes, medical records, and verification status are recorded in audit history.
9. The UI consistently follows the FK Velež light-only red, white, and gold identity using semantic design tokens.
10. User-facing interface text is available in Bosnian Latin by default and can support English where localization is implemented.
11. The system remains extensible for confirmed Gpexe, Zone14, object storage, Docker, and production hosting decisions added later.
