# Unit 56: Responsive and Accessibility Hardening

## Goal

Harden the complete implemented V1 frontend for responsive behavior and practical WCAG 2.2 AA accessibility without changing product scope, backend contracts, permissions, workflow rules, or the Bosnian-only language decision currently in force.

Audit and correct the authenticated shell, auth pages, dashboard, settings, users, players, matches, match reports, media, imports, training/workload, and availability/medical surfaces. Add consistent landmarks, skip navigation, route-level focus management, keyboard-complete interactions, semantic headings/tables/forms, accessible async feedback, dialog/Sheet focus behavior, status text independent of color, chart table equivalents, reduced-motion handling, controlled responsive overflow, mobile-safe dialogs/actions, and a documented viewport/role/workflow verification matrix.

Units 54 and 55 are intentionally deferred. Unit 56 must preserve the current Bosnian Latin UI and must not install or depend on localization infrastructure.

This unit is a hardening pass, not a formal accessibility certification.

## Design

### Required reading and implementation boundaries

Before implementation, read:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/ui-context.md`
5. `context/code-standards.md`
6. `context/ai-workflow-rules.md`
7. `context/progress-tracker.md`
8. `context/references/shadcn-components.md`
9. `context/feature-specs/00-build-plan.md`
10. the implemented frontend shell/auth specs;
11. `context/feature-specs/37-match-report-review-ui.md`
12. `context/feature-specs/44-import-ui-foundation.md`
13. `context/feature-specs/49-training-gps-ui-foundation.md`
14. `context/feature-specs/51-medical-availability-ui.md`
15. `context/feature-specs/53-dashboard-ui.md`
16. `context/feature-specs/56-responsive-accessibility-hardening.md`

Use relevant project-local skills from `.agents/skills/` when applicable.

Before changing a component:

- inspect the generated shadcn primitive and its documented behavior;
- prefer correcting application composition and props;
- do not manually modify generated `frontend/src/components/ui/*` files;
- do not replace accessible shadcn behavior with custom low-level interaction code;
- keep layout-only responsive classes at the usage/application-component level;
- preserve semantic design tokens;
- do not add raw Tailwind palette colors or hardcoded color values.

This unit is frontend-only unless implementation reveals a small factual accessibility metadata defect in an existing backend response. Such a backend change is out of scope unless separately documented before implementation.

Do not change:

- API endpoints or DTO semantics;
- role, permission, or team-scope behavior;
- workflow statuses or transitions;
- import processor capabilities;
- medical privacy boundaries;
- workload comparability rules;
- database schema;
- EF Core migrations;
- route paths;
- Bosnian visible labels except where wording must be clarified for accessibility;
- Unit 54/55 localization status.

### Deferred localization decision

Record the current release decision:

```txt
Unit 54: DEFERRED
Unit 55: DEFERRED
Current selectable UI language: Bosnian Latin only
```

Rules:

- do not install `i18next` or `react-i18next`;
- do not add a language selector;
- do not migrate existing copy into translation resources;
- do not change backend language behavior;
- do not introduce a new user-language field;
- keep all new visible hardening copy in proper Bosnian Latin;
- centralize any new repeated labels in existing feature constants where practical;
- leave the detailed Unit 54 spec available for future resumption;
- Unit 54/55 may resume after Unit 57 or in a post-V1 phase.

### Accessibility target

Use WCAG 2.2 AA as the practical engineering target for implemented workflows.

Prioritize:

- 1.3.1 Info and Relationships;
- 1.3.2 Meaningful Sequence;
- 1.3.5 Identify Input Purpose where applicable;
- 1.4.3 Contrast (Minimum);
- 1.4.10 Reflow;
- 1.4.11 Non-text Contrast;
- 1.4.12 Text Spacing;
- 1.4.13 Content on Hover or Focus;
- 2.1.1 Keyboard;
- 2.1.2 No Keyboard Trap;
- 2.1.4 Character Key Shortcuts where relevant;
- 2.2.1 Timing Adjustable where relevant;
- 2.3.3 Animation from Interactions;
- 2.4.1 Bypass Blocks;
- 2.4.2 Page Titled;
- 2.4.3 Focus Order;
- 2.4.4 Link Purpose;
- 2.4.6 Headings and Labels;
- 2.4.7 Focus Visible;
- 2.4.11 Focus Not Obscured;
- 2.5.3 Label in Name;
- 2.5.8 Target Size (Minimum);
- 3.2.1 On Focus;
- 3.2.2 On Input;
- 3.3.1 Error Identification;
- 3.3.2 Labels or Instructions;
- 3.3.3 Error Suggestion;
- 3.3.7 Redundant Entry where applicable;
- 4.1.2 Name, Role, Value;
- 4.1.3 Status Messages.

This spec does not claim external audit or legal certification.

### Scope

This unit introduces or hardens:

- a keyboard-visible skip link;
- shell landmarks;
- one main heading per route;
- route-change focus management;
- mobile sidebar focus behavior;
- visible focus tokens;
- keyboard-complete navigation and action menus;
- semantic button/link usage;
- accessible dialogs, Alert Dialogs, Sheets, popovers, selects, comboboxes, calendars, and dropdown menus;
- accessible loading, success, warning, error, and progress announcements;
- form label, description, validation, and error-summary associations;
- semantic and keyboard-usable TanStack/shadcn tables;
- contained table overflow;
- accessible sorting and pagination labels;
- file-upload progress and abort announcements;
- chart title/description/table equivalents;
- reduced-motion behavior;
- status labels independent of color;
- touch-target and mobile action sizing;
- responsive filter/action layouts;
- responsive dialogs and Sheets;
- safe overflow/reflow at narrow widths and 200% zoom;
- text-spacing resilience;
- forced-colors/high-contrast resilience where practical;
- medical restricted-content unmount/focus behavior;
- a repeatable manual audit document;
- targeted frontend tests when the existing test foundation supports them.

This unit does not introduce:

- new product workflows;
- new dashboard data;
- new mobile-only product behavior;
- a second component library;
- a second table/chart library;
- new backend endpoints;
- localization infrastructure;
- dark mode;
- formal screen-reader certification;
- automated browser testing infrastructure if none currently exists;
- a new frontend test framework solely for this unit;
- broad visual redesign;
- arbitrary responsive card alternatives for every table;
- accessibility overlays;
- third-party accessibility widgets;
- runtime DOM-rewriting plugins;
- automated translation;
- support for player login.

### Audit deliverable

Create and maintain:

```txt
context/references/responsive-accessibility-audit.md
```

Use the supplied Unit 56 audit template as the starting point.

The completed audit must include:

```txt
environment
browser/assistive technology
route/workflow
role/permission
team scope
viewport/zoom
keyboard result
focus result
semantics result
responsive/reflow result
status/error result
privacy/disclosure result
finding severity
fix reference
verification result
remaining limitation
```

Do not mark a route as verified based only on visual inspection.

### Finding severity

Use:

```txt
BLOCKING
HIGH
MEDIUM
LOW
```

#### `BLOCKING`

Examples:

- keyboard trap;
- inaccessible primary workflow;
- restricted medical content remains visible after permission loss;
- destructive action can be triggered accidentally without accessible confirmation;
- page is unusable at required narrow width;
- form cannot be completed without a mouse;
- focus disappears behind an overlay.

#### `HIGH`

Examples:

- unlabeled input or action;
- incorrect dialog focus;
- missing table semantics on a core workflow;
- status communicated only by color;
- validation error not associated with field;
- chart has no accessible data equivalent;
- key content clipped at 200% zoom.

#### `MEDIUM`

Examples:

- confusing heading order;
- non-descriptive link text;
- poor focus order without complete blockage;
- touch target below project target;
- secondary content overflow;
- excessive repetitive announcements.

#### `LOW`

Examples:

- minor wording improvement;
- redundant accessible name;
- non-critical spacing inconsistency.

Completion gate:

- no unresolved `BLOCKING`;
- no unresolved `HIGH`;
- `MEDIUM` findings on core V1 workflows must be fixed or explicitly approved/documented with owner and follow-up;
- `LOW` findings may be documented for later polish;
- no privacy or permission finding may be deferred.

### Verification environment matrix

Verify at minimum:

#### Viewports

```txt
320 x 568
375 x 812
667 x 375 landscape
768 x 1024
1024 x 768
1280 x 800
1440 x 900
```

Use CSS-pixel viewport dimensions.

#### Zoom and reflow

Verify:

```txt
100%
200%
400 CSS-pixel equivalent reflow
```

Rules:

- no page-wide horizontal scroll for ordinary content at 320 CSS pixels;
- data tables and previews may use a clearly labeled contained horizontal scroll region;
- actions, headings, errors, and required values must remain reachable;
- content must not overlap or disappear;
- sticky elements must not obscure focused controls.

#### Text-spacing override

Verify representative routes with:

```txt
line-height: 1.5
paragraph spacing: 2em
letter spacing: 0.12em
word spacing: 0.16em
```

Required content and controls must remain usable without clipping.

#### Input modalities

Verify:

```txt
keyboard only
mouse
touch-emulation
at least one screen reader when available
```

Recommended combinations when available:

```txt
NVDA + Chromium/Firefox
VoiceOver + Safari
```

If an environment is unavailable, document it honestly rather than claiming completion.

#### Color/contrast modes

Verify:

```txt
normal light theme
browser/OS increased contrast or forced-colors mode when available
prefers-reduced-motion
```

### Route and workflow audit matrix

Audit all implemented routes and critical workflows.

At minimum:

#### Public/authentication

- sign in;
- required password change;
- auth/session error state;
- sign out from user menu.

#### App shell

- desktop sidebar;
- collapsed sidebar;
- mobile sidebar Sheet;
- navigation groups;
- top bar;
- user menu;
- main content;
- unauthorized/not-found states.

#### Dashboard

- context selectors;
- alerts;
- recent matches;
- report cards;
- availability cards;
- leader chart/table;
- workload selectors/table;
- refresh/error states.

#### Settings and users

- settings tabs;
- table filters;
- create/edit dialogs;
- role/team-scope controls;
- invitations/account actions;
- audit Sheet.

#### Players

- list/search/filter/pagination;
- create/edit;
- player detail tabs;
- assignment history;
- media;
- physical history/chart;
- availability section;
- restricted medical shortcut visibility.

#### Matches and reports

- match list;
- create/edit;
- detail tabs;
- lineup;
- manual statistics matrix;
- report review;
- correction form;
- report queue;
- audit history;
- physical data;
- media.

#### Media

- list/filter/detail;
- upload progress;
- external reference;
- preview/open/download;
- link/unlink targets;
- archive/restore.

#### Imports

- list/filter;
- upload;
- XHR progress/abort;
- detail Sheet;
- preview table;
- validation issues;
- explicit confirmation;
- cancellation;
- processing state;
- audit history.

#### Training and workload

- session list/create/edit;
- complete/cancel;
- participant candidate/add/remove;
- physical metric/comparability selectors;
- workload table/detail;
- contextual import;
- player trend chart.

#### Availability and medical

- summary cards;
- safe status table;
- availability update;
- revision/audit Sheet;
- restricted tab gating;
- injury candidate/create;
- injury detail/update/resolve;
- restricted revisions/audit;
- permission/scope loss while restricted content is open.

### App-shell landmarks

Use native landmarks:

```txt
<header>
<nav>
<main>
<aside> where semantically appropriate
<footer> only when actual footer content exists
```

Requirements:

- one main content landmark per rendered route;
- primary navigation has an accessible name;
- repeated/nested navigation landmarks have distinct labels;
- page header is not used as the global shell header when it is only route content;
- decorative containers do not receive landmark roles;
- mobile and desktop navigation are not both exposed simultaneously to assistive technology;
- hidden navigation is unmounted or properly non-interactive.

### Skip link

Add a keyboard-visible skip link as the first focusable item:

```txt
Preskoči na glavni sadržaj
```

Behavior:

- hidden visually until focused;
- uses semantic anchor behavior;
- targets the main content;
- moves keyboard focus to the main content/route heading;
- remains visible above sticky shell layers;
- works in desktop, collapsed-sidebar, and mobile layouts;
- uses semantic tokens and approved focus ring;
- does not add a raw-color exception.

Suggested target:

```txt
#main-content
```

The target may use `tabIndex={-1}` when required for programmatic focus.

### Route-level focus management

Add an app-level route focus manager.

Rules:

- on pathname change caused by normal route navigation, move focus to:
  1. route `h1`; or
  2. main content fallback;
- announce the new page through its heading/title;
- do not move focus for ordinary search-parameter changes such as:
  - filters;
  - pagination;
  - tabs;
  - selected row/Sheet state;
- do not steal focus while a user types;
- browser back/forward should result in predictable route focus;
- closing a route-level overlay restores focus to the triggering control when it still exists;
- when the trigger disappeared due to mutation/filter change, focus a safe section heading/action fallback;
- permission loss that closes restricted medical UI must move focus to the safe availability heading and announce the access change.

Do not implement focus management separately in every feature.

### Heading hierarchy

Every route must have:

- exactly one meaningful `h1`;
- logical descending section headings;
- no heading level chosen for visual size alone;
- dialog/Sheet title through the relevant shadcn title primitive;
- cards with headings only when they represent sections;
- no empty headings;
- no hidden duplicate route headings exposed to assistive technology.

App-level text styles remain responsible for appearance.

### Page titles

Because localization is deferred, use Bosnian route titles when title management already exists or can be added without broad routing refactor.

At minimum:

- base title remains meaningful;
- critical auth and app routes should not all share an uninformative `Vite App` title;
- title changes do not expose restricted medical details or player names unless the existing product convention deliberately includes safe entity names;
- no localization package is introduced.

If route-specific title management is too broad for Unit 56, document the limitation as `MEDIUM` and ensure the base title is meaningful.

### Focus visibility

Audit all interactive controls.

Requirements:

- do not remove outline/focus ring without an equivalent;
- `:focus-visible` uses `--ring` or approved semantic focus tokens;
- focus indicator has sufficient contrast against adjacent surfaces;
- focus remains visible inside:
  - tables;
  - cards;
  - sticky headers;
  - dialogs;
  - Sheets;
  - dropdown menus;
  - charts' supporting controls;
- disabled controls are not focusable unless the primitive specifically requires it;
- keyboard focus is not indicated only through color change;
- no `outline-none`/`focus:outline-none` without an approved visible replacement;
- destructive action focus uses normal accessible focus, not reduced visibility.

Do not add per-component one-off focus colors.

### Focus not obscured

Verify:

- sticky top bar;
- sticky action bars;
- dialog/Sheet headers and footers;
- table sticky headers;
- mobile browser viewport;
- on-screen keyboard where practical.

When a focused field/action is obscured:

- adjust scroll margin;
- adjust sticky offsets;
- use internal overlay scrolling;
- avoid fixed footer overlap.

### Keyboard interaction

All primary workflows must be completable using keyboard only.

Requirements:

- native buttons/links for actions/navigation;
- `Enter` and `Space` behavior follows native control semantics;
- menus, tabs, selects, comboboxes, calendars, dialogs, Sheets, and popovers preserve shadcn/Radix keyboard behavior;
- no clickable `div`/`span` replacing a button/link;
- no action depends only on double-click, hover, drag, or context menu;
- row navigation has a real link/button;
- row action menus remain separately reachable;
- `Escape` closes dismissible overlays;
- `Tab`/`Shift+Tab` remain trapped only inside modal dialogs/Sheets while open;
- non-modal popovers do not create keyboard traps;
- destructive confirmations are not auto-focused when that could increase accidental activation;
- pending actions cannot be submitted repeatedly.

### Character shortcuts

Do not add global character-key shortcuts in Unit 56.

If an existing component uses a printable-character shortcut:

- verify it is scoped to the focused widget;
- ensure it does not trigger while typing in an input;
- document/disable it if needed.

Standard browser/Radix typeahead inside a focused menu/select is acceptable.

### Touch targets

At minimum:

- satisfy WCAG target-size requirements;
- aim for approximately 44 by 44 CSS pixels for primary mobile actions;
- preserve adequate spacing between icon-only controls;
- table row action menus remain operable on touch;
- pagination controls are not tiny;
- mobile sidebar controls are comfortably reachable;
- small inline badges are not interactive unless they meet target requirements;
- do not enlarge desktop dense-data cells unnecessarily when the action can use an accessible menu/button.

Use layout/size variants, not raw visual overrides.

### Status and color

Every status must include readable text.

Audit:

- report statuses;
- match result/status;
- import statuses;
- processing states;
- validation severities;
- training statuses;
- availability statuses;
- injury statuses;
- media lifecycle;
- user account states;
- success/warning/error alerts.

Rules:

- color is supplementary;
- icons are supplementary;
- status text remains visible at narrow widths;
- abbreviated status text must remain understandable;
- chart series are distinguished through labels/tooltips/table data, not color alone;
- forced-colors mode preserves borders/focus/state distinction where practical;
- do not introduce pattern fills or another chart package.

### Contrast audit

Audit semantic tokens and representative component states.

Targets:

- normal text: at least 4.5:1;
- large text: at least 3:1;
- meaningful UI boundaries, icons, focus indicators, and chart essentials: at least 3:1 against adjacent colors where required.

Audit:

- primary/destructive/outline/ghost buttons;
- muted text;
- placeholder text;
- input borders;
- disabled controls;
- status badges;
- alerts;
- links;
- focus ring;
- charts and tooltip text;
- table separators;
- empty-state icons.

Rules:

- adjust semantic tokens or approved variants centrally when a repeated issue exists;
- do not patch contrast through route-local raw colors;
- retain FK Velež red/white/gold identity;
- no dark mode;
- document token changes in `context/ui-context.md`.

### Reduced motion

Respect:

```txt
prefers-reduced-motion: reduce
```

Requirements:

- disable/reduce nonessential transitions and animations;
- charts avoid distracting entry animations;
- sidebar/dialog/Sheet transitions remain understandable and brief or reduced;
- loading indicators do not use excessive motion;
- no auto-scrolling animation;
- focus movement is immediate/predictable;
- functional progress indicators may continue with a low-motion presentation;
- no content depends on animation to convey state.

Do not remove all feedback; preserve state changes through text and structure.

### Async status messages

Use appropriate live-region behavior.

#### Polite status

Examples:

- data refreshed;
- item saved;
- upload progress milestone;
- processing started/completed;
- filters returned no results.

Use:

```txt
role="status"
aria-live="polite"
```

or the approved toast/status primitive.

#### Assertive errors

Examples:

- form submission failed;
- upload failed;
- permission changed;
- destructive action failed.

Use the approved error Alert/toast behavior without duplicating announcements.

Rules:

- do not announce every polling refresh;
- do not announce every percentage tick;
- avoid duplicate toast plus inline live-region text;
- loading skeletons are `aria-hidden` where appropriate;
- one meaningful hidden/loading status may announce:
  - `Učitavanje...`;
- async buttons expose loading text or an accessible loading name;
- spinners are decorative when adjacent text already names the state.

### Forms

Audit all forms.

Requirements:

- every input has a visible label except a clearly justified icon/search control with an accessible label;
- required state is announced;
- help text and error text use stable IDs;
- inputs use `aria-describedby`;
- invalid fields use `aria-invalid`;
- server field errors map to the correct field;
- form-level error summary appears when multiple/non-field errors exist;
- after failed submit, focus the first invalid field or error summary according to form complexity;
- placeholder is not the only label/instruction;
- grouped radio/checkbox controls use `fieldset`/`legend` or equivalent primitive semantics;
- destructive confirmation wording describes consequence;
- pending submit disables duplicate action while preserving readable state;
- cancel remains available unless cancellation would corrupt an active request;
- date/time controls have format/instruction text where needed;
- password inputs support browser password-manager/autocomplete conventions;
- search fields use appropriate input type and clear-button label;
- file inputs have visible labels and accepted-format guidance.

### Form error summary

For dense forms or forms with errors across collapsed sections/tabs, add an application-level error summary pattern.

Behavior:

- receives focus after failed submission when no single obvious first field is sufficient;
- contains a count and links to invalid fields;
- uses semantic Alert;
- does not duplicate restricted medical values;
- preserves user-entered values;
- remains responsive;
- is not required for simple one-field dialogs when field-level focus is clearer.

Do not build a new form framework.

### Dialog, Alert Dialog, and Sheet behavior

Audit every overlay.

Requirements:

- title and description are present through shadcn primitives;
- content has an accessible name;
- initial focus goes to:
  - first meaningful field;
  - neutral primary action;
  - explicit heading/description for high-risk confirmation;
- destructive action is not automatically focused when avoidable;
- `Escape` behavior matches dismissibility;
- clicking outside does not dismiss a form if data loss risk requires explicit cancellation;
- focus remains trapped while modal;
- closing restores focus;
- mobile height uses viewport-safe max height and internal scroll;
- header and action footer remain reachable;
- no nested modal trap without documented need;
- background is not keyboard-accessible while modal;
- Sheet content does not exceed viewport width;
- wide desktop Sheets have bounded max width;
- unsaved changes confirmation is added only where existing product behavior requires it; do not introduce broad new workflow.

### Mobile sidebar

Audit the mobile navigation Sheet.

Requirements:

- open button has an accessible name and state where appropriate;
- Sheet title identifies navigation;
- initial focus is predictable;
- navigation items follow DOM/visual order;
- selecting a route closes the Sheet;
- closing returns focus to the menu button unless route focus management moves to the new page;
- desktop sidebar is not exposed while mobile Sheet is active;
- collapsed desktop icons have accessible names/tooltips;
- current route is exposed through:
  - `aria-current="page"`;
- group labels remain readable;
- no hover-only navigation state.

### Tabs

Audit route/detail/status tabs.

Requirements:

- use shadcn/Radix semantics;
- arrow-key behavior works;
- selected tab is announced;
- tab panels are associated correctly;
- inactive panels are not focusable/exposed;
- URL-backed tab changes do not unexpectedly move focus;
- tab overflow on mobile uses a usable scroll/wrap strategy;
- tabs are not used only for visual button grouping when a real button group is more appropriate;
- restricted medical tab is not present in the accessibility tree without permission.

### Dropdown menus and popovers

Requirements:

- trigger has an accessible name;
- current expanded state is handled by primitive;
- menu items have clear labels;
- dangerous actions are clearly distinguished through text;
- disabled items include understandable context where needed;
- menu does not contain arbitrary complex forms unless the component supports it;
- focus returns to trigger;
- icon-only triggers are not hover-only;
- popover help content remains available on keyboard focus and dismissible.

### Comboboxes and selects

Audit team, season, player, match, metric, and status selectors.

Requirements:

- visible label;
- trigger accessible name;
- current value announced;
- search input label/instructions;
- empty-results message;
- loading state;
- clear action label;
- keyboard selection;
- selected option state;
- no inaccessible virtualized behavior unless already supported;
- changing value does not unexpectedly submit/destructively mutate;
- server-side candidate pagination remains usable without mouse;
- date-eligible participant/injury candidate selectors expose assignment summary as descriptive text.

### Calendars and dates

Requirements:

- date field has a visible label;
- calendar trigger includes selected date or purpose;
- keyboard navigation works;
- date format instruction is available;
- selected/current/today states are not color-only;
- min/max constraints are explained;
- invalid dates produce field-associated errors;
- date-only values do not shift through UTC conversion;
- mobile calendar fits viewport;
- occurrence/session/effective/resolved date semantics remain unchanged.

### Tables

Use semantic shadcn table markup with TanStack behavior.

Requirements:

- table has an accessible name through:
  - visible heading association;
  - caption;
  - `aria-label`;
  - or `aria-labelledby`;
- header cells use `<th>`;
- column headers use correct scope/semantics;
- sortable headers contain a real button;
- `aria-sort` reflects current state;
- sort direction is announced in button text/accessible description;
- row selection uses labeled checkboxes;
- pagination controls identify:
  - previous;
  - next;
  - current page;
  - total pages when known;
- empty table keeps context and explanation;
- cell action buttons have row-specific accessible names where needed;
- no interactive control is nested inside another interactive row wrapper;
- row navigation uses a link/button rather than only `onClick`;
- sticky headers do not obscure focus;
- important values remain text, not CSS pseudo-content;
- status cells contain readable labels;
- no sensitive hidden column remains exposed to assistive technology.

### Responsive table regions

Dense tables may scroll horizontally inside a contained region.

Requirements:

- no page-wide horizontal scrolling;
- wrapper has:
  - `role="region"` when helpful;
  - accessible label;
  - `tabIndex={0}` only when keyboard scrolling is necessary and tested;
- visible or screen-reader instruction indicates horizontal scrolling when content overflows;
- key identity/action columns remain usable;
- actions are not positioned off-screen without a way to reach them;
- mobile stacked-card alternatives are added only when explicitly beneficial and do not duplicate inconsistent business logic;
- import preview/statistics matrices preserve all required columns/data;
- do not hide validation or medical disclosure context to fit small screens;
- wide table remains usable at 200% zoom.

Create a small reusable application-level wrapper only if multiple tables need identical behavior.

Do not alter shadcn table primitives.

### Statistics matrix

The manual match-statistics matrix is a critical dense-data workflow.

Verify:

- each cell input has an accessible name including:
  - player;
  - metric;
- keyboard order follows row/column reading order;
- arrow-key enhancement may be added only if it does not break native input behavior and is documented;
- headers remain associated at narrow widths/scroll positions;
- current player/metric context remains visible;
- null versus zero remains clear;
- validation error identifies player and metric;
- focus moves to the invalid cell;
- save/pending/status feedback is announced;
- locked report states remove editing controls from the tab order;
- no horizontal page scroll;
- mobile supports review/lightweight correction but dense entry remains desktop/tablet optimized as documented.

Do not add spreadsheet packages.

### Import preview and validation tables

Verify:

- dynamic columns have readable source headers;
- source row number is announced;
- null/empty marker is understandable;
- horizontal scroll region is labeled;
- validation severity is text;
- issue row/column references are explicit;
- progressbar exposes:
  - min;
  - max;
  - current value when known;
- indeterminate progress has readable text;
- abort action remains reachable;
- processing state uses status announcement;
- confirmation/cancellation consequence is clear;
- no raw source rows appear in error announcements.

### Charts

Audit all shadcn/Recharts charts.

Requirements:

- visible section title;
- concise chart description;
- chart is supplementary;
- equivalent semantic table/list is available;
- axes/series have meaningful labels;
- tooltips are not the only data source;
- chart container is not inserted into tab order unless it has a deliberate interaction;
- no individual SVG element receives noisy focus;
- color is not the sole distinction;
- reduced motion disables/reduces animation;
- screen-reader text does not expose every decorative SVG element;
- hidden/restricted data is not included in tooltip/description;
- zero values are preserved;
- missing values are not plotted as zero;
- workload charts never merge incompatible comparability keys;
- chart resizes without clipped labels at supported viewports;
- long player/opponent labels have a readable table equivalent.

### Icon-only actions

Every icon-only button/link must have:

- accessible name;
- tooltip where it improves sighted understanding;
- adequate target size;
- visible focus;
- no duplicate accessible name from both title and label where it causes noise;
- row/entity context when ambiguity exists.

Examples:

```txt
Otvori akcije za Amara Hadžića
Zatvori detalje importa
Osvježi kontrolnu ploču
Preuzmi izvorni fajl
```

Do not rely on Lucide icon name or SVG title automatically.

### Links

Requirements:

- descriptive purpose from link text/context;
- avoid repeated ambiguous:
  - `Otvori`;
  - `Detalji`;
  - `Klikni ovdje`;
- when compact UI requires short visible text, provide a row-specific accessible name;
- external links communicate new-tab behavior where practical;
- external media uses safe `noopener`/`noreferrer`;
- disabled navigation is not represented as a fake link;
- `aria-current` is used for current navigation/page where appropriate.

### Loading and skeletons

Requirements:

- skeletons are decorative and hidden from assistive technology where appropriate;
- one surrounding status communicates loading;
- loading state does not remove the page heading/context selector;
- previous-context data is not mislabeled as current;
- avoid rapid repeated live announcements;
- button spinners have readable loading labels;
- table loading preserves column/context headings where useful;
- no fake sample data.

### Empty states

Every empty state must explain:

- what is empty;
- selected context/filter;
- next action when permitted;
- no action when the user lacks permission.

Requirements:

- proper heading/text order;
- icon decorative unless meaningful;
- action is a real button/link;
- no hidden entity existence disclosure;
- role-aware wording;
- no future-feature placeholder buttons.

### Error and unauthorized states

Requirements:

- readable title/description;
- retry when meaningful;
- field errors associated;
- `403` does not reveal hidden data;
- restricted medical `403` immediately removes restricted content/cache per Unit 51;
- focus moves to the error Alert when the current workflow becomes unusable;
- error announcement is not duplicated excessively;
- raw stack traces/IDs/source content are not shown;
- actions remain keyboard reachable.

### Toasts and transient feedback

Audit the approved Sonner/toast integration.

Requirements:

- success messages are polite;
- critical failures are assertive through the approved system;
- action result is also reflected in persistent UI after refetch;
- toast is not the only place where a validation error exists;
- toast text contains no:
  - medical details;
  - source rows;
  - passwords/tokens;
  - coach-visible note content;
- toasts remain visible/readable at mobile widths;
- dismiss action has an accessible name;
- no infinite toast duration without a user-controlled close unless required for a blocking state.

### Medical privacy and accessibility

Unit 51 privacy behavior is a mandatory hardening focus.

Verify:

- restricted `Povrede` tab is absent without permission;
- restricted data is unmounted, not visually hidden;
- permission/scope loss:
  - cancels requests;
  - removes restricted cache;
  - closes overlay;
  - clears URL state;
  - moves focus to safe availability heading;
  - announces safe access-change message;
- safe availability controls contain no injury existence clue;
- screen-reader accessible names do not expose diagnosis/body area in safe UI;
- hidden/inert overlay content is not reachable;
- restricted note text is not inserted into toast/error/URL;
- restricted tables/Sheets remain keyboard accessible for authorized users;
- resolved status and update/resolve actions are clearly distinguished;
- no destructive medical action is focused automatically.

No privacy finding may be deferred.

### Responsive shell behavior

#### Desktop

- persistent/collapsible sidebar;
- content width remains readable;
- tables/charts use available width;
- sidebar collapse preserves accessible names;
- no content hidden behind sidebar/top bar.

#### Tablet

- sidebar behavior remains predictable;
- context/filter/action rows wrap;
- dialog/Sheet widths remain bounded;
- dense table scroll remains contained;
- primary actions remain visible.

#### Mobile

- sidebar is a Sheet/drawer;
- page headers stack;
- primary action remains reachable without overlapping title;
- filters stack or use a clearly labeled filter Sheet only when existing patterns support it;
- cards use one-column flow;
- charts have readable height/table fallback;
- dialogs use viewport-safe sizing/internal scroll;
- Sheet actions remain visible;
- lightweight actions remain possible;
- dense analysis/data entry may require horizontal contained scroll but cannot become impossible.

Do not build separate mobile routes or duplicate feature logic.

### Page-header behavior

Audit all `PageHeader` compositions.

Requirements:

- `h1` remains visible;
- description wraps;
- action group wraps/stacks;
- no action overlaps breadcrumbs/title;
- mobile primary action remains reachable;
- secondary actions use menu when space is constrained;
- status metadata wraps;
- no page-wide overflow from long entity names;
- safe truncation provides full accessible text when necessary.

### Filter bars

Requirements:

- labels remain associated;
- controls wrap in meaningful DOM order;
- search is not squeezed below usable width;
- clear-filters action remains visible;
- filter changes do not move focus unexpectedly;
- filter Sheet, if used, has title/description/focus restoration;
- active filters are understandable without color;
- date ranges remain usable;
- responsive layout does not change logical keyboard order.

### Dialog/Sheet responsive sizing

Use modern viewport-safe sizing such as approved `dvh` behavior through existing Tailwind support.

Requirements:

- overlay content fits at 320 by 568;
- internal content scrolls;
- action footer does not cover form fields;
- virtual keyboard does not permanently hide submit/cancel where practical;
- textarea remains usable;
- file upload progress remains visible;
- wide preview/details use bounded desktop width;
- no fixed pixel height that clips content;
- no nested page scroll plus overlay scroll confusion.

### Truncation and long content

Audit:

- player names;
- team/opponent/venue names;
- filenames;
- processor keys;
- status labels;
- error messages;
- notes;
- restricted medical text;
- table headers.

Rules:

- truncation is visual only;
- full content is available through:
  - wrapped detail;
  - tooltip;
  - accessible name/description;
- do not use tooltip as the only access to critical values;
- long unbroken filenames/keys use safe wrapping;
- restricted text is not copied into accessible labels outside its authorized surface;
- mobile layout does not break.

### Zoom and browser text scaling

At 200% zoom:

- all controls remain reachable;
- no fixed-height clipping;
- headings/actions reflow;
- no required hover behavior;
- tables scroll in contained regions;
- dialogs remain operable;
- sticky content does not hide focus;
- chart table equivalent remains available;
- no content overlaps.

Do not solve zoom defects by disabling browser zoom.

### Forced colors/high contrast

Where supported:

- focus indicator remains visible;
- buttons/links remain distinguishable;
- status text remains;
- selected tabs/menu items remain identifiable;
- input boundaries remain visible;
- icons do not become the only state indicator;
- charts retain table equivalents;
- custom backgrounds do not erase text;
- use system-compatible borders/outlines where semantic tokens otherwise disappear.

Do not create a separate high-contrast theme.

### DOM and ARIA rules

Prefer native semantics.

Prohibited patterns:

- `role="button"` on a `div` when a button works;
- `tabIndex={0}` on non-interactive content without a deliberate keyboard function;
- redundant/conflicting roles;
- `aria-label` that contradicts visible text;
- focusable content inside `aria-hidden`;
- using `aria-live` on large frequently updating containers;
- manually reproducing Radix/shadcn ARIA;
- hidden restricted content left focusable;
- positive `tabIndex`;
- CSS-generated critical text;
- duplicate IDs.

Use ARIA only to complete native semantics.

### Automated and code-level checks

Do not introduce a new browser/E2E test framework solely for Unit 56.

Use existing:

- TypeScript;
- ESLint;
- frontend tests;
- Vite build;
- browser accessibility tree/devtools;
- browser Lighthouse/accessibility audit as supporting evidence only;
- manual keyboard/screen-reader verification.

Rules:

- a Lighthouse score is not the acceptance gate;
- automated scanners do not replace workflow testing;
- if `eslint-plugin-jsx-a11y` is already installed/configured, fix applicable findings;
- do not add it solely to satisfy this spec unless repository maintainers explicitly approve the dev dependency;
- if an existing E2E/test foundation supports axe integration, add targeted checks for critical routes;
- otherwise document manual results without pretending automated coverage exists.

### Frontend tests

When the existing frontend test foundation supports them, add targeted tests for:

- skip-link target/focus;
- route focus only on pathname change;
- no focus move on search-param changes;
- mobile navigation accessible labels/`aria-current`;
- dialog focus return;
- first-invalid-field/error-summary focus;
- sortable table `aria-sort`;
- row action accessible names;
- upload progress semantics;
- chart table equivalent;
- reduced-motion chart/animation configuration;
- restricted medical unmount/cache/focus behavior;
- status label text;
- responsive component class/state logic where practical.

Do not snapshot enormous DOM trees as the primary evidence.

### Manual keyboard workflow set

Complete at minimum:

1. sign in;
2. forced password change;
3. select dashboard team/season and navigate alerts;
4. create/edit player;
5. create/edit match;
6. enter lineup;
7. enter statistics;
8. submit/review/return/verify report;
9. upload and cancel/reconcile an import;
10. inspect preview/validation;
11. create/complete training and add participant;
12. inspect workload comparability;
13. update availability;
14. create/update/resolve restricted injury with permission;
15. open/close audit Sheets;
16. sign out.

Every step must be possible without a mouse.

### Manual screen-reader workflow set

With at least one available screen reader, verify representative:

- shell/navigation and skip link;
- dashboard heading/section structure;
- one dense table with sorting/pagination;
- one multi-field form with validation;
- one Dialog;
- one Sheet;
- one chart plus data table;
- import progress/status;
- safe availability versus restricted medical separation.

Document exact environment and limitations.

### Performance constraints

Hardening must not significantly degrade the application.

Rules:

- no duplicate data fetching for accessibility;
- no duplicate desktop/mobile full feature trees unless necessary and proven safe;
- avoid rendering both table and card alternatives when one responsive semantic structure works;
- chart table equivalents use the already bounded returned data;
- route focus manager is lightweight;
- no global MutationObserver accessibility layer;
- no accessibility overlay script;
- no repeated formatter/listener leaks;
- no excessive live-region updates.

### Documentation synchronization

Update:

```txt
context/ui-context.md
context/code-standards.md
context/progress-tracker.md
context/references/responsive-accessibility-audit.md
```

Record:

- Unit 54/55 deferred state;
- current Bosnian-only UI;
- WCAG 2.2 AA practical target;
- skip link/landmarks/route focus behavior;
- focus-ring and contrast token changes;
- responsive viewport matrix;
- table overflow pattern;
- dialog/Sheet mobile pattern;
- chart accessibility pattern;
- reduced-motion behavior;
- medical permission-loss focus behavior;
- audit findings and resolutions;
- unavailable assistive-technology environments;
- remaining approved limitations;
- verification commands/results.

Do not mark Unit 56 complete until completion-gate findings are resolved or explicitly documented according to this spec.

## Implementation

### 1. Record localization deferral

Update the build plan and progress tracker:

```txt
Unit 54: deferred
Unit 55: deferred
Unit 56: current
```

Keep Bosnian Latin as the only selectable/implemented UI language.

Do not merge unfinished localization implementation code.

### 2. Create the audit document

Copy the Unit 56 audit template to:

```txt
context/references/responsive-accessibility-audit.md
```

Populate route/workflow findings throughout implementation.

### 3. Audit and harden the app shell

Implement/fix:

- landmarks;
- skip link;
- primary navigation labels;
- mobile sidebar focus;
- `aria-current`;
- route heading/focus management;
- unauthorized/not-found focus behavior.

### 4. Harden shared interaction patterns

Audit/fix application compositions for:

- PageHeader;
- buttons/links;
- Dropdown Menu;
- Tabs;
- Dialog;
- Alert Dialog;
- Sheet;
- Popover;
- Select/Combobox;
- Calendar;
- Tooltip;
- Toast;
- loading/empty/error states.

Do not modify generated shadcn primitives.

### 5. Harden forms

Implement/fix:

- labels;
- descriptions;
- required state;
- `aria-invalid`;
- error associations;
- first-error/error-summary focus;
- pending submission;
- date/password/file input instructions;
- destructive confirmations.

### 6. Harden tables and dense data entry

Implement/fix:

- accessible names;
- header/sort semantics;
- pagination labels;
- row links/actions;
- contained horizontal scroll;
- statistics matrix cell naming/focus;
- import preview/validation table semantics.

Create only small reusable application wrappers when repeated behavior justifies them.

### 7. Harden charts and data visualization

Implement/fix:

- titles/descriptions;
- equivalent table/list;
- tooltip supplement;
- reduced motion;
- chart token/contrast behavior;
- no incompatible workload visualization;
- responsive labels/containers.

### 8. Harden responsive layouts

Verify and fix all audit routes at the required viewport matrix and zoom levels.

Prioritize:

- app shell;
- page headers;
- filters;
- tables;
- dialogs/Sheets;
- action bars;
- charts;
- long content;
- mobile keyboard viewport.

### 9. Harden async feedback

Implement/fix:

- progressbar semantics;
- polite status;
- assertive errors;
- loading text;
- no duplicate announcements;
- accessible pending/disabled buttons;
- permission-change announcement.

### 10. Verify medical privacy interactions

Test and fix:

- restricted tab absence;
- unmount behavior;
- cache cleanup;
- overlay closure;
- focus relocation;
- safe accessible labels;
- no hidden restricted content.

### 11. Apply centralized token fixes

When contrast/focus defects repeat, update semantic tokens or approved app-level variants centrally.

Document every token behavior change.

Do not add route-local raw colors.

### 12. Add focused tests

Use the existing frontend test foundation only.

Add targeted tests for the most failure-prone shared behaviors.

Do not introduce a new test/E2E framework solely for this unit.

### 13. Complete manual verification

Run:

- viewport matrix;
- zoom/reflow;
- text-spacing override;
- keyboard workflows;
- at least one screen-reader set when available;
- reduced motion;
- forced colors/high contrast when available;
- role/scope/privacy matrix.

Update the audit document with evidence and limitations.

### 14. Update project documentation

Update UI context, code standards, progress tracker, and build plan.

Do not mark Unit 56 complete while any `BLOCKING` or `HIGH` finding remains unresolved.

## Dependencies

None.

Use the existing frontend stack:

- React;
- Vite;
- TypeScript;
- Tailwind CSS v4;
- shadcn/ui/Radix primitives;
- TanStack Query;
- `nuqs`;
- React Hook Form;
- Zod;
- TanStack Table;
- Recharts through shadcn Chart;
- Lucide React;
- existing frontend test and lint tooling.

Do not add:

- localization packages;
- accessibility overlays/widgets;
- a second component library;
- a second table/chart library;
- a new E2E/browser test framework solely for Unit 56;
- responsive-layout frameworks;
- gesture libraries;
- accessibility runtime DOM-rewriting libraries;
- dark-mode packages;
- backend packages.

No backend dependency, migration, or API change is expected.

## Verification checklist

- [ ] Unit 54 is marked deferred rather than completed or cancelled.
- [ ] Unit 55 is marked deferred rather than completed or cancelled.
- [ ] Bosnian Latin remains the only implemented/selectable UI language.
- [ ] No localization package or backend language change is added.
- [ ] Unit 56 is the current build-plan unit.
- [ ] `context/references/responsive-accessibility-audit.md` is created and populated.
- [ ] Audit environment, role, scope, route, viewport, input method, finding, fix, and verification are recorded.
- [ ] No unresolved `BLOCKING` finding remains.
- [ ] No unresolved `HIGH` finding remains.
- [ ] No privacy/permission finding is deferred.
- [ ] App shell uses correct header/nav/main landmarks.
- [ ] One primary main landmark exists per route.
- [ ] Primary navigation has an accessible name.
- [ ] Mobile and desktop navigation are not simultaneously exposed.
- [ ] A visible-on-focus `Preskoči na glavni sadržaj` skip link exists.
- [ ] Skip link works in desktop, collapsed, and mobile shell modes.
- [ ] Every route has one meaningful `h1`.
- [ ] Heading order is logical.
- [ ] Pathname navigation moves focus to route heading/main.
- [ ] Search-param filter/tab/pagination changes do not steal focus.
- [ ] Browser back/forward focus is predictable.
- [ ] Overlay close restores focus or uses a safe fallback.
- [ ] Restricted medical permission loss moves focus to safe availability content.
- [ ] Visible focus exists on every interactive element.
- [ ] No outline is removed without an accessible replacement.
- [ ] Focus is not hidden under sticky headers/footers/overlays.
- [ ] All primary workflows can be completed keyboard-only.
- [ ] No keyboard trap exists.
- [ ] No clickable `div`/`span` replaces a native button/link.
- [ ] No action depends only on hover, drag, double-click, or context menu.
- [ ] Modal focus remains trapped while open.
- [ ] Destructive actions are not accidentally auto-focused.
- [ ] Pending actions cannot be submitted repeatedly.
- [ ] Primary mobile targets are comfortably operable.
- [ ] Minimum interactive target size/spacing is satisfied.
- [ ] Every status includes readable text and does not rely only on color.
- [ ] Focus, text, UI-boundary, badge, link, and chart contrast are audited.
- [ ] Repeated contrast fixes use semantic tokens/approved variants.
- [ ] No raw Tailwind palette or hardcoded color is introduced.
- [ ] Reduced-motion preference is respected.
- [ ] Charts reduce/disable nonessential animation.
- [ ] Loading/progress/success/error states use appropriate accessible status behavior.
- [ ] Live regions do not announce excessive repeated updates.
- [ ] Skeletons do not create misleading screen-reader content.
- [ ] Every input has a label or justified accessible name.
- [ ] Required state and instructions are announced.
- [ ] Help/error text is associated with fields.
- [ ] Invalid fields use `aria-invalid`.
- [ ] Server field errors focus/link to the relevant field.
- [ ] Dense forms use an accessible error summary where useful.
- [ ] File inputs include accepted-format/size guidance.
- [ ] Password inputs retain appropriate autocomplete behavior.
- [ ] Every Dialog/Sheet/Alert Dialog has an accessible title/description.
- [ ] Mobile overlays fit the supported viewport and scroll internally.
- [ ] Overlay action footers do not cover fields.
- [ ] Mobile sidebar has accessible open/close/current-page behavior.
- [ ] Tabs support keyboard operation and correct panel relationships.
- [ ] Restricted medical tab is absent from the accessibility tree without permission.
- [ ] Dropdown and popover triggers have accessible names.
- [ ] Combobox/select search, loading, empty, clear, and selected states are accessible.
- [ ] Date/calendar controls are labeled and keyboard usable.
- [ ] Every table has an accessible name/context.
- [ ] Table headers use semantic `<th>` behavior.
- [ ] Sort controls are real buttons and expose `aria-sort`.
- [ ] Pagination controls have descriptive labels/current state.
- [ ] Row navigation and row action controls are not nested incorrectly.
- [ ] Dense tables use contained labeled horizontal overflow rather than page-wide scrolling.
- [ ] Statistics matrix cells identify player and metric.
- [ ] Statistics validation can focus the invalid cell.
- [ ] Import preview and validation tables preserve semantic headers/row references.
- [ ] Upload progress exposes progressbar semantics and readable status.
- [ ] Every chart has a title/description and equivalent table/list.
- [ ] Charts are not the sole data source.
- [ ] Chart elements do not create noisy keyboard focus.
- [ ] Missing values are not presented as zero.
- [ ] Workload charts never merge incompatible comparability groups.
- [ ] Every icon-only action has a descriptive accessible name.
- [ ] Links have understandable purpose.
- [ ] `aria-current` is used for current navigation where appropriate.
- [ ] Empty/error/unauthorized states are readable and role-safe.
- [ ] Toasts contain no sensitive/medical/source/password data.
- [ ] Restricted medical content is unmounted and cache-cleared on permission/scope loss.
- [ ] Restricted content is not reachable through hidden DOM/focus.
- [ ] Safe availability UI exposes no injury clue through accessible names.
- [ ] All required viewport dimensions are verified.
- [ ] Landscape mobile is verified.
- [ ] 200% zoom is verified.
- [ ] 400 CSS-pixel-equivalent reflow is verified.
- [ ] No ordinary page-wide horizontal scroll exists at narrow widths.
- [ ] Necessary data regions use contained scroll.
- [ ] Required text-spacing override remains usable.
- [ ] Reduced-motion mode is verified.
- [ ] Forced-colors/high-contrast behavior is verified when available.
- [ ] Page headers, filters, cards, actions, tables, charts, dialogs, and Sheets reflow safely.
- [ ] Long names, filenames, labels, notes, and errors do not break layout.
- [ ] Mobile virtual-keyboard behavior is checked for key forms where practical.
- [ ] At least the required manual keyboard workflow set is completed.
- [ ] At least one screen-reader representative workflow set is completed when the environment is available.
- [ ] Unavailable assistive-technology environments are documented honestly.
- [ ] No duplicate feature endpoint fetching is added for accessibility.
- [ ] No accessibility overlay/runtime DOM rewriting is added.
- [ ] No duplicate desktop/mobile business-logic trees are introduced without need.
- [ ] Existing approved frontend tests pass.
- [ ] Focused accessibility tests pass when supported by the existing test foundation.
- [ ] `npm run format` completes successfully.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] Frontend typecheck/build passes using the configured project command.
- [ ] No backend files or EF migrations are changed.
- [ ] `context/ui-context.md` records the hardened patterns and any token changes.
- [ ] `context/code-standards.md` records the accessibility/responsive rules for future units.
- [ ] `context/progress-tracker.md` records localization deferral, audit results, remaining limitations, and verification commands.
