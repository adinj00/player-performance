# Responsive and Accessibility Audit

> Unit 56 working document.
>
> This audit is an engineering verification record, not a formal legal/accessibility certification.

## Decision context

```txt
Current UI language: Bosnian Latin
Unit 54 localization foundation: DEFERRED
Unit 55 localization pass: DEFERRED
Target: practical WCAG 2.2 AA hardening
```

## Environment

| Item | Value |
|---|---|
| Frontend commit | |
| Operating system | |
| Browser/version | |
| Screen reader/version | |
| Input method | |
| Device/emulation | |
| Date | |
| Reviewer | |

## Severity definitions

| Severity | Meaning |
|---|---|
| BLOCKING | Workflow/privacy failure or keyboard/reflow blocker |
| HIGH | Major label/focus/semantic/status/access defect |
| MEDIUM | Significant but non-blocking usability/access defect |
| LOW | Minor polish or wording defect |

## Viewport and mode matrix

| Viewport/mode | Completed | Notes |
|---|---:|---|
| 320 × 568 | | |
| 375 × 812 | | |
| 667 × 375 landscape | | |
| 768 × 1024 | | |
| 1024 × 768 | | |
| 1280 × 800 | | |
| 1440 × 900 | | |
| 200% zoom | | |
| 400 CSS-pixel-equivalent reflow | | |
| Text-spacing override | | |
| Reduced motion | | |
| Forced colors/high contrast | | |

## Route and workflow audit

| ID | Route/workflow | Role/permission | Team scope | Viewport/zoom | Keyboard | Focus | Semantics | Responsive/reflow | Status/error | Privacy/disclosure | Severity | Finding | Fix/commit | Retest | Remaining limitation |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| A-001 | | | | | | | | | | | | | | | |

## Shared component audit

| Component/pattern | Keyboard | Focus | Name/role/value | Mobile/reflow | Reduced motion | Finding/fix |
|---|---|---|---|---|---|---|
| Skip link | | | | | | |
| App shell/nav | | | | | | |
| PageHeader | | | | | | |
| Button/link | | | | | | |
| Tabs | | | | | | |
| Dropdown Menu | | | | | | |
| Dialog | | | | | | |
| Alert Dialog | | | | | | |
| Sheet | | | | | | |
| Popover/Tooltip | | | | | | |
| Select/Combobox | | | | | | |
| Calendar/date input | | | | | | |
| Form/error summary | | | | | | |
| Table/pagination | | | | | | |
| Responsive table region | | | | | | |
| Toast/status/Alert | | | | | | |
| Chart + table equivalent | | | | | | |
| File upload/progress | | | | | | |

## Critical workflow keyboard verification

| Workflow | Completed | Result/limitations |
|---|---:|---|
| Sign in | | |
| Required password change | | |
| Dashboard context and links | | |
| Player create/edit | | |
| Match create/edit | | |
| Lineup entry | | |
| Statistics entry | | |
| Report submit/review/correction/verify | | |
| Media upload/link | | |
| Import upload/progress/preview/validation | | |
| Training create/participants/workload | | |
| Availability update | | |
| Restricted injury create/update/resolve | | |
| Audit Sheet navigation | | |
| Sign out | | |

## Screen-reader representative verification

| Workflow | Environment | Completed | Result/limitations |
|---|---|---:|---|
| Shell, skip link, navigation | | | |
| Dashboard headings and charts | | | |
| Dense table sorting/pagination | | | |
| Multi-field validation form | | | |
| Dialog and Sheet focus | | | |
| Import progress/status | | | |
| Safe availability vs restricted injury | | | |

## Contrast and token audit

| Token/component | Foreground/background | Result | Change |
|---|---|---|---|
| Primary button | | | |
| Destructive button | | | |
| Muted text | | | |
| Input border | | | |
| Focus ring | | | |
| Status badges | | | |
| Alerts | | | |
| Links | | | |
| Chart series | | | |
| Tooltip | | | |

## Completion summary

| Severity | Open | Resolved | Approved deferred |
|---|---:|---:|---:|
| BLOCKING | | | 0 |
| HIGH | | | 0 |
| MEDIUM | | | |
| LOW | | | |

## Verification commands

```bash
npm run format
npm run format:check
npm run lint
# add the configured frontend typecheck/build/test commands
```

## Final limitations

- 
