# UI Context

## Theme

The application uses a light-only FK Velež Mostar visual identity. Do not implement dark mode, dark theme tokens, theme toggles, or dark-specific layout behavior unless explicitly requested later.

The visual language should feel like a professional football performance and club operations dashboard: clean, structured, data-dense, and fast to scan. The identity is red, white, and gold, matching FK Velež's public-facing brand direction. The UI should feel serious and operational rather than decorative.

Primary characteristics:

- Light-only interface.
- Bosnian Latin default UI language, with optional English UI support.
- Red, white, and gold club identity.
- Dense but readable sports analytics layouts.
- Strong use of tables, filters, stat cards, status badges, forms, dashboards, and detail pages.
- Semantic design tokens only; no hardcoded visual styling.
- shadcn/ui primitives as the base component system.
- Desktop-first responsive layout with clean tablet and mobile rendering.


## UI Language

The default application language is Bosnian Latin (`bs`). English (`en`) may be supported as an optional selectable UI language.

Rules:

- Bosnian Latin is the default language for navigation, page titles, actions, form labels, validation messages, status labels, empty states, and operational UI copy.
- English can be added as a secondary UI language through the localization system.
- Do not mix Bosnian and English in the same visible UI screen unless the English text is a domain term, imported value, code, acronym, or user-entered content.
- Internal code, API contracts, enum names, route names, filenames, and translation keys remain in English.
- Display labels for internal statuses must be localized for the selected UI language. For example, `READY_FOR_REVIEW` is an internal status, not the final user-facing label.
- User-entered content such as player names, staff names, team names, opponent names, venues, notes, and imported source values is displayed as entered and is not automatically translated.
- Use 24-hour time and clear day-month-year date formatting for Bosnian UI unless a feature spec defines a more specific format.

When localization infrastructure is implemented, new long-lived user-facing strings should be added through translation resources instead of being hardcoded directly in feature components.

Recommended language switch behavior:

- Default to Bosnian Latin when no preference exists.
- Allow English selection only after English translations exist for the relevant UI area.
- Persist the language preference locally at first, then move it to a server-side user preference when account settings are implemented.

## Design Token Source of Truth

The source of truth for frontend design tokens is `frontend/src/index.css`.

The project uses Tailwind CSS v4 with `@theme inline`, shadcn/ui Tailwind integration, and CSS custom properties. All app UI must use semantic token utilities generated from these variables.

The approved token file imports:

```css
@import "tailwindcss";
@import "tw-animate-css";
@import "shadcn/tailwind.css";

@import "@fontsource-variable/manrope";
@import "@fontsource-variable/jetbrains-mono";
```

Do not replace this theme system with raw Tailwind palette classes or one-off color utilities.

## Colors

All color usage must flow through semantic CSS variables defined in `index.css` and exposed through Tailwind v4 theme tokens.

### Club Tokens

| Role | CSS Variable | Purpose |
| --- | --- | --- |
| Club red | `--club-red` | FK Velež primary brand accent and primary action color |
| Club gold | `--club-gold` | Focus rings, highlights, secondary accent, and selected emphasis |
| Soft club gold | `--club-gold-soft` | Subtle highlight background and secondary surfaces |

### Core shadcn/ui Tokens

| Role | CSS Variable | Purpose |
| --- | --- | --- |
| Page background | `--background` | Main page background |
| Primary text | `--foreground` | Main text color |
| Card background | `--card` | Cards and contained panels |
| Card text | `--card-foreground` | Text on card surfaces |
| Popover background | `--popover` | Menus, dropdowns, popovers |
| Popover text | `--popover-foreground` | Text in overlays |
| Primary action | `--primary` | Main buttons and primary interactive elements |
| Primary action text | `--primary-foreground` | Text on primary actions |
| Secondary surface | `--secondary` | Secondary buttons and selected neutral highlights |
| Secondary text | `--secondary-foreground` | Text on secondary elements |
| Muted surface | `--muted` | Muted backgrounds and quiet sections |
| Muted text | `--muted-foreground` | Secondary/help text |
| Accent surface | `--accent` | Hover, active, and subtle emphasis surfaces |
| Accent text | `--accent-foreground` | Text on accent surfaces |
| Destructive action | `--destructive` | Deletion, dangerous actions, serious errors |
| Destructive text | `--destructive-foreground` | Text on destructive actions |
| Border | `--border` | Borders and dividers |
| Input border | `--input` | Form controls and input borders |
| Focus ring | `--ring` | Focus state and accessible interaction ring |

### Dashboard Surface Tokens

| Role | CSS Variable | Purpose |
| --- | --- | --- |
| Surface | `--surface` | Dashboard background sections and page panels |
| Surface text | `--surface-foreground` | Text on custom surfaces |
| Muted surface | `--surface-muted` | Filter bars, subtle table containers, empty-state panels |
| Elevated surface | `--surface-elevated` | Raised cards, stat cards, overlay panels |

### State Tokens

| Role | CSS Variable | Use Cases |
| --- | --- | --- |
| Success | `--success` | Verified, imported, available, completed |
| Success text | `--success-foreground` | Text on success backgrounds |
| Warning | `--warning` | Ready for review, limited, needs attention |
| Warning text | `--warning-foreground` | Text on warning backgrounds |
| Info | `--info` | Draft, informational states, neutral workflow hints |
| Info text | `--info-foreground` | Text on info backgrounds |
| Destructive | `--destructive` | Failed, unavailable, delete, critical correction |
| Destructive text | `--destructive-foreground` | Text on destructive backgrounds |

### Chart Tokens

Use `--chart-1` through `--chart-5` for charts. These are mapped to Tailwind as `chart-1`, `chart-2`, `chart-3`, `chart-4`, and `chart-5`.

Charts must use theme-aware token colors. Do not hardcode hex, OKLCH, RGB, HSL, or raw Tailwind palette classes inside chart components unless a task explicitly changes the design system.

## Typography

| Role | Font | CSS Variable |
| --- | --- | --- |
| UI text | Manrope Variable | `--font-sans` |
| Headings | Manrope Variable | `--font-heading` |
| Numeric/technical data | JetBrains Mono Variable | `--font-mono` |

Typography rules:

- Use Manrope for normal UI text, headings, navigation, labels, forms, and dashboard copy.
- Use JetBrains Mono only for numeric, tabular, import-preview, code-like, or technical data where alignment and scanning matter.
- Do not introduce additional fonts without updating this file.
- Use Tailwind text utilities semantically and consistently.
- Avoid decorative typography. This is an operational staff dashboard.

## Border Radius

The base radius is defined by `--radius: 0.875rem` and exposed through Tailwind radius tokens.

| Context | Token/Class |
| --- | --- |
| Small controls | `rounded-sm` or `rounded-md` |
| Inputs and buttons | shadcn/ui default radius |
| Cards and panels | `rounded-lg` or `rounded-xl` |
| Dialogs and larger overlays | `rounded-xl` or `rounded-2xl` |

Do not use arbitrary radius values unless a component-specific task explicitly requires it.

## Shadows

Custom shadow tokens are defined through `--shadow-app-*` source variables and mapped to Tailwind shadow tokens:

```css
--shadow-2xs: var(--shadow-app-2xs);
--shadow-xs: var(--shadow-app-xs);
--shadow-sm: var(--shadow-app-sm);
--shadow: var(--shadow-app);
--shadow-md: var(--shadow-app-md);
--shadow-lg: var(--shadow-app-lg);
--shadow-xl: var(--shadow-app-xl);
--shadow-2xl: var(--shadow-app-2xl);
```

Use shadows sparingly. The application should feel clean and structured, not overly floating. Prefer borders and subtle surfaces for dense data areas.

## Component Library

The frontend uses shadcn/ui on top of Tailwind CSS v4.

Generated shadcn/ui primitive components live in:

```txt
frontend/src/components/ui/
```

The existing shadcn structure must remain in place, including:

```txt
frontend/src/components/ui/button.tsx
frontend/src/lib/utils.ts
```

Rules:

- Add shadcn components through the shadcn CLI when needed.
- Do not move generated shadcn/ui primitive files out of `src/components/ui`.
- Do not modify generated shadcn/ui primitive files unless a task explicitly requires it.
- App-specific reusable components should compose shadcn primitives and live outside `components/ui`, usually in `components/common`, `components/layout`, or a feature-specific `components` folder.
- Prefer component variants and semantic props over ad-hoc styling.

## shadcn/ui Usage Rules

Use shadcn/ui primitives through their documented variants and default behavior.

Do not apply ad-hoc color, border, shadow, or typography overrides directly to shadcn/ui component usages.

Do not write component usages like:

```tsx
<Button className="bg-red-700 text-white border-yellow-400">
  Save
</Button>
```

Prefer documented variants:

```tsx
<Button variant="default">Save</Button>
<Button variant="destructive">Delete</Button>
<Button variant="outline">Cancel</Button>
```

Layout-only utility classes are allowed on shadcn/ui usages when needed for width, spacing, grid placement, flex behavior, alignment, responsive behavior, or visibility.

Allowed examples:

```tsx
<Button className="w-full">Create Match</Button>
<Card className="col-span-2">...</Card>
<div className="grid gap-4 md:grid-cols-2">...</div>
```

If a repeated visual variation is needed, create a documented component variant or an app-level reusable component instead of styling individual usages.

## Tailwind Utility Rules

Use semantic token utilities generated from `index.css`.

Allowed examples:

```txt
bg-background
text-foreground
bg-card
text-card-foreground
border-border
text-muted-foreground
bg-primary
text-primary-foreground
bg-surface
bg-surface-muted
text-success
bg-warning
```

Avoid raw Tailwind palette classes in application UI:

```txt
bg-zinc-950
text-slate-400
border-gray-800
bg-red-700
text-yellow-500
```

Raw palette classes may only appear inside the design token source file or in a task explicitly dedicated to changing the theme system.

## Layout Patterns

### App Shell

The authenticated application uses a desktop-first dashboard shell:

```txt
App Shell
├── Left Sidebar
├── Top Bar
└── Main Content Area
```

The shell must support responsive behavior:

- Desktop: persistent or collapsible sidebar.
- Tablet: collapsible sidebar, clean content scaling.
- Mobile: sidebar opens as a drawer/sheet; essential viewing and lightweight actions remain usable.

Dense data entry, imports, and analysis workflows are optimized for desktop and tablet screens.

### Left Sidebar

The sidebar is the primary navigation area.

Recommended navigation groups:

```txt
Overview
- Dashboard

Performance
- Matches
- Players
- Training GPS
- Imports

Club
- Teams / Selections
- Medical / Availability
- Media Library

Administration
- Users & Roles
- Settings
```

Rules:

- Navigation must be role-aware and team-scope aware.
- Users must not see navigation entries they cannot access.
- Sidebar active states must use semantic tokens and shadcn-compatible styling.
- The sidebar may support collapsed icon-only mode on desktop.

### Top Bar

The top bar provides page context and high-level actions.

Common elements:

- Page title.
- Breadcrumbs when useful.
- Selected season.
- Selected team/selection.
- User menu.
- Primary page action.
- Optional global search later if needed.

Season and team selection should be treated as important dashboard context. When the selected season/team affects shareable page state, it should be stored in URL search params using `nuqs`.

### Main Content Area

Most pages should follow this pattern:

```txt
PageHeader
FilterBar
Main Content
```

Common page structures:

- KPI/stat-card grids.
- Filter bars.
- Tabs for status or detail sections.
- TanStack Table rendered with shadcn table primitives.
- Split detail panels for review workflows.
- Dialogs/sheets for focused creation and editing flows.
- Empty states for missing data.
- Skeletons for loading states.

## Page Patterns

### Dashboard

Dashboard pages provide season/team-level overview.

Common sections:

- KPI cards.
- Recent matches.
- Match report review status.
- Team form summary.
- Top performers.
- Availability summary.
- Physical workload summary when GPS data exists.
- Data quality alerts.

### Matches

The matches module is a central workflow area.

Common layout:

```txt
Matches
├── Page header
├── Filter bar
├── Status tabs
├── Matches table/list
└── Primary action: New Match
```

Recommended status tabs:

```txt
All
Draft
Ready for Review
Verified
Needs Correction
Archived
```

### Match Detail

Match detail pages should expose the report workflow clearly.

Recommended tabs:

```txt
Overview
Lineup
Player Stats
GPS / Physical
Video
Audit
```

The page must show:

- Match identity and status.
- Backend-provided allowed actions.
- Review/verification state.
- Data source indicators.
- Media/external references.
- Audit visibility for authorized users.

### Player Profile

Player profile pages show a persistent club player record.

Recommended sections:

```txt
Player Profile
├── Player header
├── Current team/status/availability
├── Development path through selections
└── Tabs
    ├── Overview
    ├── Matches
    ├── Performance
    ├── Physical
    ├── Availability
    └── Notes
```

The profile should make movement through the club visible, such as:

```txt
U17 → U19 → First Team
```

### Imports

Import workflows should be clear, safe, and reviewable.

Recommended flow:

```txt
Step 1: Upload file
Step 2: Select import type
Step 3: Preview and map columns
Step 4: Validate
Step 5: Confirm import
```

Import screens must show validation errors clearly and should never silently import unknown fields.

### Media Library

Media must support uploaded files and external references.

Common actions:

- Upload media.
- Add external link.
- Filter by type, linked entity, team, and season.
- Attach media to match, training session, player, import job, or report where relevant.

### Medical / Availability

Medical and availability screens must be clear and permission-aware.

Common sections:

- Team filter.
- Availability summary cards.
- Player availability table.
- Restricted notes area for authorized roles.

Availability statuses:

```txt
AVAILABLE
LIMITED
UNAVAILABLE
REHAB
UNKNOWN
```

Sensitive medical notes must not be visible to unauthorized roles.

## Forms

Forms use React Hook Form with Zod schemas.

Rules:

- Use shadcn form primitives for consistent styling.
- Use Zod for frontend schema validation.
- Backend validation remains required and authoritative.
- Show field-level errors clearly.
- Keep dense forms split into logical sections.
- Prefer keyboard-friendly data entry for match statistics and import mapping.
- Do not store server data in form state longer than necessary.

## Tables

Use TanStack Table for table state and behavior, and shadcn/ui table primitives for rendering.

TanStack Table owns:

- Sorting.
- Filtering.
- Pagination.
- Row selection.
- Column visibility.
- Table state.

shadcn table primitives own:

- Markup.
- Styling.
- Semantic table structure.

Important table-heavy areas:

- Players.
- Matches.
- Match player stats.
- GPS metrics.
- Imports and preview validation.
- Users and roles.
- Audit logs.

Match-stat data entry should prioritize speed, clarity, keyboard support, and validation feedback over decorative layout.

## Charts and Data Visualization

Use shadcn/ui chart components built on Recharts for V1 charts.

Do not introduce Tremor, ECharts, visx, Evil Charts, or another charting library unless a specific requirement cannot be met with shadcn/Recharts and the context files are updated first.

Expected V1 chart types:

- Bar charts for goals, assists, minutes, cards, or team comparisons.
- Line charts for trends over time.
- Area charts for workload and physical trends.
- Simple categorical summaries for availability and report status.

Charts must use theme tokens and should remain readable in the light-only UI.

## Frontend State and UI Data Rules

- Use TanStack Query for server state, API fetching, caching, and mutations.
- Use `nuqs` for URL search parameter state such as filters, tabs, pagination, selected season, and selected team.
- Use Zustand only for global client-side UI state that is not server data and does not belong in the URL.
- Use React Context Providers for stable app-level provider composition and static context.
- Do not duplicate server-owned data in Zustand or React Context.

Examples:

| State Type | Tool |
| --- | --- |
| Players list | TanStack Query |
| Match detail from API | TanStack Query |
| Table filters in URL | nuqs |
| Sidebar collapsed state | Zustand |
| QueryClientProvider | React Context Provider |
| Auth/session query result | TanStack Query plus provider composition as needed |

## Icons

Use Lucide React icons unless a task explicitly introduces another icon source.

Recommended sizes:

| Context | Size |
| --- | --- |
| Inline icon | `h-4 w-4` |
| Button icon | `h-4 w-4` or `h-5 w-5` |
| Sidebar icon | `h-5 w-5` |
| Empty state icon | `h-8 w-8` or `h-10 w-10` |

Use stroke-based icons. Avoid mixing filled icon styles with Lucide unless the design system is updated.

## Responsive Behavior

The app is desktop-first but must remain usable on smaller screens.

Rules:

- Primary analysis, imports, and dense data entry are optimized for desktop and tablet.
- Mobile must support clean viewing and lightweight actions.
- Tables should use responsive wrappers and avoid breaking layout.
- Mobile table alternatives may use stacked cards only where a task explicitly calls for it.
- Dialogs may become sheets on mobile when it improves usability.

## Accessibility

- Preserve keyboard navigation for interactive components.
- Use visible focus states through `--ring`.
- Do not remove outlines globally beyond the approved base layer behavior.
- Buttons and links must have accessible names.
- Status badges must not rely only on color; include readable text.
- Form validation errors must be associated with fields where possible.
- Tables must preserve semantic table structure.

## Loading, Empty, and Error States

Every data-driven page should provide clear states:

- Loading: skeletons or compact loading indicators.
- Empty: clear explanation and next action when allowed.
- Error: readable message and retry action when appropriate.
- Unauthorized: clear access message without exposing restricted data.
- Validation failure: field-level and summary errors where useful.

## Approved UI Direction

This file approves the initial UI direction:

- Light-only FK Velež theme.
- Bosnian Latin default UI language with optional English UI support.
- Red, white, and gold identity.
- Tailwind v4 semantic token system.
- shadcn/ui component foundation.
- No dark mode.
- No raw Tailwind palette classes in application UI.
- No ad-hoc visual overrides on shadcn/ui usages.
- Layout-only utility classes are allowed.
- Desktop-first responsive dashboard layout.
- TanStack Query, nuqs, Zustand, Zod, React Hook Form, TanStack Table, and shadcn/Recharts as the frontend UI/data toolkit.
