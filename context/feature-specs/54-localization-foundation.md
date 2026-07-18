# Unit 54: Localization Foundation

## Goal

Add the typed frontend localization foundation for the Player Performance Data System.

Install and configure `i18next` and `react-i18next`, preserve Bosnian Latin (`bs`) as the unconditional default UI language, add complete English (`en`) resources for foundation-owned keys, define safe local language-preference persistence, synchronize the HTML language metadata, introduce typed translation namespaces and shared enum/validation/error/date/number helpers, add reusable language-selector integration points, and prove language switching through tests.

Do not perform the full migration of existing feature UI strings in this unit. Do not make English globally selectable while most existing screens still contain hardcoded Bosnian copy. Unit 55 will migrate the long-lived UI and enable English selection only after the required coverage gate passes.

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
10. `context/feature-specs/19-frontend-sign-in-password-change-wiring.md`
11. `context/feature-specs/26-settings-ui-foundation.md`
12. `context/feature-specs/53-dashboard-ui.md`
13. `context/feature-specs/54-localization-foundation.md`

Use relevant frontend skills from `frontend/.agents/` when applicable.

Before creating custom UI primitives:

- check `context/references/shadcn-components.md`;
- reuse the existing user-menu, auth-page, `Dropdown Menu`, `Select`, `Button`, and `Tooltip` patterns;
- install a missing shadcn primitive only through the approved CLI workflow and only when it is actually used;
- do not manually modify generated `frontend/src/components/ui/*` files.

This unit is frontend-only.

Do not change:

- backend APIs;
- authentication/session contracts;
- user database fields;
- permission flags;
- team scope;
- domain enum values;
- route names;
- database schema;
- EF Core migrations;
- existing feature business logic.

### Package selection

Add only:

```txt
i18next
react-i18next
```

Install the current stable versions compatible with the existing React, Vite, and TypeScript versions, and pin the resolved versions through the normal package lock.

At the time this spec was prepared, the expected stable versions are:

```txt
i18next 26.3.6
react-i18next 17.0.8
```

Verify package compatibility at implementation time before committing the lockfile.

Do not add:

```txt
@types/react-i18next
i18next-browser-languagedetector
i18next-http-backend
i18next-resources-to-backend
i18next-cli
i18next-parser
Locize/Phrase SDKs
a second localization framework
a date/number localization library
```

Rationale:

- `react-i18next` already includes its own TypeScript declarations;
- resources are small and known at build time, so they should be statically bundled;
- Bosnian must remain the default when no explicit preference exists, so browser-language auto-detection is intentionally not used;
- extraction and full coverage automation belong to the Unit 55 migration only if proven necessary;
- `Intl` provides the required date/number formatting without another dependency.

### Supported, available, and selectable languages

Define stable language codes:

```txt
bs
en
```

Use a centralized language registry equivalent to:

```txt
AppLanguage
LanguageDefinition
```

Each definition contains:

```txt
code
nativeLabel
englishLabel
intlLocale
htmlLang
direction
resourceStatus
isSelectable
```

Required initial definitions:

#### Bosnian Latin

```txt
code = bs
nativeLabel = Bosanski
englishLabel = Bosnian
intlLocale = bs-BA
htmlLang = bs
direction = ltr
resourceStatus = DEFAULT_COMPLETE
isSelectable = true
```

#### English

```txt
code = en
nativeLabel = English
englishLabel = English
intlLocale = en-GB
htmlLang = en
direction = ltr
resourceStatus = FOUNDATION_ONLY
isSelectable = false
```

Meaning:

- `bs` is the default and only globally selectable language in Unit 54;
- `en` resources exist for all keys introduced by Unit 54;
- `en` can be used by tests and isolated foundation stories/components;
- ordinary users cannot globally switch to English yet because existing feature screens are not fully migrated;
- Unit 55 changes `en.isSelectable` to `true` only after its translation-coverage checklist passes.

Do not use an environment variable to bypass this product gate.

Do not expose an incomplete global English mode that mixes English shell text with hardcoded Bosnian feature text.

### Default language resolution

Resolve the initial application language in this order:

1. a valid locally persisted language that is currently selectable;
2. `bs`.

Explicit rules:

- do not use `navigator.language` when no preference exists;
- do not use operating-system locale detection;
- do not use IP/geolocation;
- do not use an authentication claim;
- do not infer language from browser timezone;
- do not use the current route;
- an invalid, unsupported, or currently non-selectable persisted value is removed/ignored and resolves to `bs`;
- failure to access local storage must not prevent application startup;
- default resolution must be deterministic in tests.

This preserves the product requirement that Bosnian Latin is the default interface language.

### Language preference persistence

Use one centralized storage adapter.

Suggested versioned key:

```txt
ppds.uiLanguage.v1
```

Rules:

- store only the language code;
- use `localStorage`;
- wrap every read/write/remove operation in `try/catch`;
- storage failure falls back to in-memory language state;
- no medical, user, team, or session data is stored with the language;
- preference is browser/device-local in V1;
- preference is not written to cookies;
- preference is not sent to the backend;
- logout does not clear it;
- account disable/session expiration does not clear it;
- invalid values are removed when practical;
- no JSON object is needed for one language code.

Future server-side preference support requires a separate account-settings spec and must define client/server precedence explicitly.

### Cross-tab synchronization

Listen to the browser `storage` event for the language preference key.

Rules:

- a valid selectable language written in another tab updates the current tab;
- a removed/invalid/non-selectable value resolves the current tab to `bs`;
- do not write back recursively during a storage-event update;
- detach the listener during application teardown/test cleanup;
- no `BroadcastChannel` dependency is required;
- a cross-tab language change must not reload the page.

### i18next instance ownership

Create one application-owned i18next instance through:

```txt
createInstance()
```

Do not rely on a mutable imported global singleton shared implicitly across tests.

Initialize the instance before the React root renders.

Recommended bootstrap:

```txt
bootstrapLocalization()
bootstrapApplication()
```

Requirements:

- bundled resources make startup local and deterministic;
- no HTTP translation request;
- no translation-loading spinner;
- localization initialization failure falls safely to Bosnian resources where possible;
- application startup errors use a minimal static Bosnian fallback outside i18next if initialization itself cannot complete;
- tests can create isolated i18next instances without mutating the production instance.

### i18next configuration

Configure at minimum:

```txt
fallbackLng = bs
defaultNS = common
supportedLngs = [bs, en]
load = languageOnly
returnNull = false
saveMissing = false
interpolation.escapeValue = false
react.useSuspense = false
```

Use the resolved selectable initial language as `lng`.

Rules:

- React owns output escaping, so translations must still render as normal React text;
- do not use `dangerouslySetInnerHTML` for translations;
- do not enable runtime missing-key submission;
- do not send translation strings to an external service;
- do not use a network backend;
- do not use English as fallback for Bosnian;
- Bosnian remains the final fallback;
- missing keys must remain visible in development/test through a clear diagnostic rather than silently returning an unrelated sentence;
- production must not crash due to one unknown future key.

If a current i18next option has changed in the installed stable version, use its documented equivalent and record the actual configuration in `context/progress-tracker.md`.

### HTML document synchronization

Whenever the resolved language changes, update:

```txt
document.documentElement.lang
document.documentElement.dir
```

Required values:

```txt
bs -> lang="bs", dir="ltr"
en -> lang="en", dir="ltr"
```

Also localize the base application document title through foundation resources:

```txt
bs -> Sistem podataka o učinku igrača
en -> Player Performance Data System
```

Rules:

- route-specific title migration remains Unit 55 work;
- do not leave the HTML language fixed to English in `index.html`;
- the static `index.html` fallback should use:
  - `lang="bs"`;
- changing language must update the title without a reload;
- no RTL styles are added in Unit 54;
- the registry must keep a `direction` field so a future language addition cannot ignore directionality.

### Resource organization

Create a localization structure equivalent to:

```txt
frontend/src/i18n/
├── config.ts
├── instance.ts
├── bootstrap.ts
├── languages.ts
├── resources.ts
├── i18next.d.ts
├── storage.ts
├── formatters.ts
├── enum-translator.ts
├── validation.ts
├── problem-details.ts
├── test-utils.ts
├── components/
│   └── language-selector.tsx
└── locales/
    ├── bs/
    │   ├── common.ts
    │   ├── errors.ts
    │   ├── language.ts
    │   └── validation.ts
    └── en/
        ├── common.ts
        ├── errors.ts
        ├── language.ts
        └── validation.ts
```

Exact filenames may follow current frontend conventions, but preserve:

- one clear localization ownership boundary;
- one file per namespace/language;
- no feature translation files mixed into shadcn primitives;
- no localization files under backend folders;
- no generated unreviewable translation dump.

Unit 55 adds feature namespaces beside these resources.

### Foundation namespaces

Add exactly these initial namespaces:

```txt
common
language
validation
errors
```

#### `common`

Foundation-owned keys such as:

```txt
app.name
actions.save
actions.cancel
actions.close
actions.retry
actions.refresh
actions.confirm
actions.back
states.loading
states.notAvailable
states.noResults
values.yes
values.no
values.unknown
```

Do not attempt to move all existing feature labels here.

#### `language`

Keys such as:

```txt
selector.label
selector.current
selector.change
names.bs
names.en
status.default
status.foundationOnly
```

#### `validation`

Reusable generic validation messages such as:

```txt
required
invalid
invalidEmail
invalidDate
dateInFuture
endAfterStart
minLength
maxLength
minValue
maxValue
positive
integer
```

Use interpolation for numeric limits and field labels where appropriate.

#### `errors`

Reusable generic error UI such as:

```txt
generic.title
generic.description
network.title
network.description
unauthorized
forbidden
notFound
conflict
validation
server
sessionExpired
retry
```

Feature-specific statuses, fields, and workflows remain in feature namespaces created during Unit 55.

### Resource source format

Use reviewed TypeScript resource objects with:

```txt
as const
```

rather than remote JSON files.

Derive the key/type structure from the default Bosnian resources.

Add a recursive type helper equivalent to:

```txt
TranslationShape<T>
```

so English resources must match the Bosnian key structure while allowing different string values.

Requirements:

- missing English foundation key fails TypeScript/build/tests;
- extra accidental English key fails where practical;
- Bosnian resource keys are the structural source of truth;
- translation values remain plain strings or supported nested objects;
- do not place React elements/functions inside resource objects;
- do not create arrays of JSX in translations;
- no duplicate resource object maintained solely for types.

### TypeScript module augmentation

Augment `i18next` through one declaration file.

Configure typed:

```txt
defaultNS
resources
returnNull
```

Use the official `CustomTypeOptions` pattern compatible with the installed i18next version.

Rules:

- `t()` calls for foundation namespaces are key-checked;
- feature namespace additions in Unit 55 extend the single centralized resource shape;
- do not scatter conflicting `declare module "i18next"` blocks across feature folders;
- no `@types/react-i18next`;
- no broad `string` cast to bypass missing keys;
- no `as any` around translation keys.

### Translation-key conventions

Use semantic, stable English keys.

Good:

```txt
actions.save
availability.status.available
imports.validation.ready
```

Avoid copy-shaped keys:

```txt
Save
ClickHereToSaveChanges
DostupnostIgraca
```

Rules:

- keys describe meaning, not exact sentence text;
- namespaces own feature copy;
- internal enum values remain unchanged;
- do not construct translation keys from arbitrary server/user text;
- do not translate:
  - player names;
  - team names;
  - competition names;
  - opponents;
  - filenames;
  - imported spreadsheet values;
  - user-entered notes;
  - processor keys;
  - vendor names;
- acronyms and domain terms may remain unchanged where appropriate;
- translation resources use proper Bosnian characters:
  - `č`;
  - `ć`;
  - `š`;
  - `ž`;
  - `đ`.

### Translation usage conventions

Inside React components:

- use `useTranslation(namespace)`;
- call `t()` during render or event handling;
- do not call `t()` once at module scope;
- do not store translated strings as long-lived server/client state;
- derive translated table columns through a hook/factory that responds to language changes;
- translate toast copy at the moment the toast is created;
- keep stable machine values in form/server state;
- use `Trans` only when React elements must be embedded in one translated sentence;
- prefer plain `t()` for ordinary labels;
- do not split one grammatical sentence into multiple independently translated fragments.

Outside React:

- pass `TFunction` into factories where appropriate; or
- use the initialized application instance only in controlled infrastructure helpers;
- tests should prefer an isolated test instance.

### Pluralization

Use i18next count-based pluralization.

Bosnian resources must support the plural categories required by `Intl.PluralRules` for `bs`.

Use modern suffixes supported by the installed i18next version, expected to include:

```txt
_one
_few
_other
```

Rules:

- pass numeric `count`;
- do not concatenate singular/plural words manually;
- English resources provide their required plural variants;
- add tests for representative Bosnian counts:
  - `1`;
  - `2`;
  - `5`;
  - `21`;
- feature-specific plural migration is Unit 55 work;
- no text assumes English two-form plural rules.

### Interpolation

Use named interpolation values:

```txt
{{count}}
{{name}}
{{date}}
{{min}}
{{max}}
```

Rules:

- do not concatenate translated sentence fragments;
- do not put raw HTML in interpolation;
- user-entered values remain escaped by React;
- translation resources do not contain secrets or server data;
- values should be formatted before interpolation when they require locale-aware date/number behavior;
- do not use interpolation to construct route paths or API values.

### Language selector component

Create one reusable application component equivalent to:

```txt
LanguageSelector
```

Integration points:

- authenticated user menu;
- sign-in page;
- required-password-change page.

Behavior:

- when fewer than two languages are selectable, render nothing;
- Unit 54 therefore introduces no visible one-option selector;
- after Unit 55 enables English, the existing integration points become visible without another shell/auth refactor;
- use native language names:
  - `Bosanski`;
  - `English`;
- indicate current language accessibly;
- language change happens without page reload;
- persist only after a successful in-memory language change;
- update HTML metadata/title;
- preserve current route, form state, TanStack Query cache, and session;
- do not sign the user out;
- do not invalidate server queries;
- do not change API enum values.

Use the existing user-menu and auth-layout styling.

Do not put the language selector in administrator-only Settings; language preference is personal/browser-local and must be available before login once English is enabled.

### Language-change service

Expose one controlled operation equivalent to:

```txt
changeAppLanguage(language)
```

Rules:

1. validate the requested code;
2. require the language to be currently selectable;
3. call the app i18next instance;
4. update document metadata/title;
5. persist the code;
6. return a typed success/failure result.

On failure:

- keep the previous language;
- do not persist the failed value;
- show a localized safe error when invoked through UI;
- do not reload;
- do not leave document language and i18next language inconsistent.

Direct calls to `i18n.changeLanguage()` outside localization infrastructure should be prohibited by convention.

### Shared locale metadata

Expose a helper equivalent to:

```txt
getLocaleDefinition(language)
```

Required mapping:

```txt
bs -> bs-BA
en -> en-GB
```

Use the resolved language, not raw `i18n.language`, when variants are possible.

Keep both UI languages:

```txt
direction = ltr
hourCycle = h23
```

Do not assume every future language is LTR.

### Date-only formatting

Add centralized helpers for date-only values.

Examples of source fields:

```txt
SessionDate
MatchDate
OccurredOn
EffectiveOn
ExpectedReturnOn
season start/end
```

Rules:

- parse date-only values without converting through UTC midnight;
- prevent timezone day shifts;
- use the active locale;
- preserve day-month-year semantics;
- never use `new Date("YYYY-MM-DD")` as the sole date-only formatter;
- provide at minimum:
  - short date;
  - medium date;
  - date range;
- invalid/missing input returns the translated `not available` value or a typed empty result according to existing utility conventions;
- do not show English month names in Bosnian mode.

### UTC instant formatting

Add centralized helpers for UTC timestamps.

Examples:

```txt
CreatedAtUtc
UpdatedAtUtc
RecordedAtUtc
generatedAtUtc
```

Use:

```txt
timeZone = Europe/Sarajevo
hour12 = false
```

Provide at minimum:

- date and time;
- time only;
- date only for an instant;
- optional seconds where operationally needed.

Rules:

- use `Intl.DateTimeFormat`;
- do not rely on browser-local timezone;
- do not mutate source values;
- display `bs-BA` or `en-GB` formatting according to UI language;
- use 24-hour time for both initial languages;
- invalid timestamp uses a safe translated fallback;
- relative-time formatting is out of scope unless already required by an existing feature.

### Number formatting

Add centralized helpers using:

```txt
Intl.NumberFormat
```

Provide at minimum:

- integer;
- decimal with configurable min/max fraction digits;
- compact value only if an existing screen truly requires it;
- percent only as a formatting primitive, not a new business calculation.

Rules:

- Bosnian mode uses `bs-BA`;
- English mode uses `en-GB`;
- preserve numeric zero;
- missing/null is not zero;
- never parse localized display strings back into API values;
- API request values remain invariant numbers;
- decimal separators are presentation only;
- do not change Unit 49 canonical unit conversions;
- physical workload formatters may delegate numeric output to the shared locale helper in Unit 55.

### Duration formatting

Add one shared duration formatter for existing second-based durations.

Rules:

- input is a numeric duration, not a date;
- preserve zero duration;
- output remains unambiguous:
  - `HH:MM:SS` where hours exist;
  - `MM:SS` where appropriate and consistent with existing use;
- optional translated unit-label mode may be added:
  - hours;
  - minutes;
  - seconds;
- do not use `Date` or timezone APIs for durations;
- do not infer duration units from arbitrary source values.

### Enum/status translation helper

Add a typed utility pattern equivalent to:

```txt
createEnumLabelResolver(valueToTranslationKey)
```

Requirements:

- the map is explicit;
- keys are stable backend enum/string values;
- values are typed translation keys;
- known values return localized labels;
- unknown future values return a safe technical fallback without crashing;
- development mode emits a bounded diagnostic;
- no dynamic key construction from arbitrary values;
- no conversion of backend values;
- no fallback that exposes restricted/hidden semantics.

Example conceptual use:

```txt
const availabilityStatusKeys = {
  AVAILABLE: "availability:status.available",
  LIMITED: "availability:status.limited",
}
```

Actual feature mappings are migrated in Unit 55.

Do not centralize every domain enum into one giant unowned file. Feature namespaces own feature mappings; the helper is shared.

### Validation translation foundation

Add reusable translation-aware validation helpers.

Preferred patterns:

- schema factory receiving `TFunction`; or
- shared functions returning localized Zod messages at schema creation time.

Examples:

```txt
createRequiredMessage(t)
createMaxLengthMessage(t, max)
createInvalidDateMessage(t)
```

Rules:

- do not install another validation library;
- do not add a global mutable Zod error map that captures a stale language;
- language-changing forms must recreate/re-resolve localized schemas where practical;
- stable validation logic remains separate from visible copy;
- server-side validation remains authoritative;
- field names used inside messages come from translation keys, not backend property names;
- no feature-schema migration is required in Unit 54 beyond a small proof/test;
- Unit 55 migrates existing long-lived Zod messages.

### ProblemDetails translation foundation

Extend the existing frontend ProblemDetails presentation through a localization-aware helper.

Resolution order:

1. known stable backend/application error code mapped explicitly to a translation key;
2. generic HTTP/status category translation;
3. existing safe server-provided user-facing title/detail when no stable code mapping exists;
4. localized generic error.

Rules:

- do not use arbitrary server text as a translation key;
- do not translate internal exception strings;
- do not expose stack traces/provider details;
- do not discard field-level validation errors;
- field names may be localized by feature mappings in Unit 55;
- stable machine codes remain English;
- unknown future code uses a safe fallback;
- restricted medical values remain prohibited from toast/error output;
- do not change backend ProblemDetails contracts.

Add only foundation/common error mappings in Unit 54.

Feature-specific error-code migration belongs to Unit 55.

### Base title and minimal proof migration

To prove the integration without performing the full pass, migrate only foundation-owned visible copy:

- base document title;
- language selector labels;
- minimal localization initialization failure copy;
- one small shared/common action or test harness component if needed.

Do not partially migrate a major screen in a way that creates mixed translation ownership.

Existing hardcoded Bosnian feature UI remains valid until Unit 55.

After Unit 54, all newly introduced long-lived visible copy must use translation resources.

### New-code rule after Unit 54

Update project standards so any new frontend unit after Unit 54 must:

- add visible long-lived copy through the appropriate translation namespace;
- provide Bosnian translation;
- provide English translation when English is selectable for that area/global UI;
- keep backend/internal values unchanged;
- use shared date/number helpers;
- avoid module-scope `t()` calls;
- add/update translation-key tests.

Temporary developer diagnostics, test fixture labels, and imported/user-entered values are exempt when they are not user-facing product copy.

### English enablement gate for Unit 55

English must remain `isSelectable = false` until Unit 55 verifies:

- all long-lived app-shell/auth/navigation copy is migrated;
- all existing V1 route page titles/actions/forms/statuses/empty/error states are migrated;
- existing centralized enum-label registries are translation-backed;
- validation messages are migrated;
- ProblemDetails presentation is migrated where stable mappings exist;
- dashboard, imports, training, availability, matches, players, users, settings, media, and reports have complete English resources;
- no major visible screen mixes hardcoded Bosnian and selected English;
- translation resource shape checks pass;
- language switch manual verification passes on all major routes.

Only then may Unit 55 set:

```txt
en.isSelectable = true
```

Do not enable English based only on foundation resource completeness.

### Testing foundation

Use the existing frontend test framework when present.

If no frontend test framework exists, do not introduce one solely for Unit 54 unless the repository's current roadmap has already approved it. In that case, prove through:

- TypeScript compilation;
- a small Node/Vite-compatible resource-shape check using existing tooling;
- build/lint;
- manual verification.

Create an isolated test helper equivalent to:

```txt
createTestI18n(language, namespaceOverrides?)
```

Rules:

- each test receives its own instance;
- no production singleton mutation;
- synchronous bundled resources;
- easy language override;
- no network/backend plugin.

### Required tests/checks

When the approved test foundation exists, cover:

- default language resolves to `bs`;
- browser locale does not override the Bosnian default;
- valid stored selectable language resolution;
- invalid stored value fallback/removal;
- non-selectable stored `en` resolves to `bs` in Unit 54;
- storage access failure;
- language-change validation;
- failed change does not persist;
- HTML `lang`/`dir` synchronization;
- base title synchronization;
- storage-event cross-tab change;
- English and Bosnian foundation resource shape parity;
- typed known foundation keys;
- missing/invalid key compile-time or resource check;
- Bosnian plural forms for representative counts;
- English plural forms;
- interpolation;
- enum helper known/unknown behavior;
- validation message interpolation;
- ProblemDetails known/generic fallback;
- date-only timezone safety;
- UTC instant formatting in `Europe/Sarajevo`;
- Bosnian and English number separators;
- zero versus null;
- duration formatting;
- language selector hidden with only one selectable language;
- language selector appears in an isolated test when English is temporarily marked selectable;
- language change does not invalidate TanStack Query/server state.

### Manual verification

Verify at minimum:

- app starts with empty local storage and Bosnian UI;
- app still starts when local storage throws/is unavailable;
- manually inserted invalid preference resolves to Bosnian;
- manually inserted `en` preference remains blocked in Unit 54 and resolves to Bosnian;
- HTML document starts with `lang="bs"`;
- base title is Bosnian;
- isolated English test instance renders foundation English resources;
- sign-in page and user menu do not show a one-option language selector;
- no visible English/Bosnian mixed mode is introduced;
- cross-tab Bosnian preference synchronization behaves safely;
- date-only values do not shift by timezone;
- UTC timestamps use Sarajevo time;
- number formatting differs correctly between `bs-BA` and `en-GB`;
- route, form, session, and query state survive an isolated language switch;
- existing frontend routes still build/render;
- no translation network request occurs;
- no backend request includes a UI language preference.

### Accessibility

Requirements:

- future-visible language selector has a clear accessible label;
- current language is announced;
- native language names are used;
- keyboard operation works in user menu and auth layouts;
- changing language does not unexpectedly move focus;
- `document.lang` updates for screen readers;
- the selector does not use flags as the only language label;
- disabled/non-selectable English is not exposed as a misleading control;
- status messages from a failed language change are accessible;
- no text relies on color to indicate selected language.

### Performance

Requirements:

- foundation resources are statically bundled;
- no translation HTTP waterfall;
- no duplicate i18next instance in production;
- no full-app rerender loop beyond the normal react-i18next language context update;
- no server-query invalidation on language change;
- no repeated `Intl.*Format` construction in large tables when a memoized/cached formatter helper is appropriate;
- formatter caches are keyed by locale and options;
- caches contain no user/server data;
- no premature namespace lazy loading is required for two bundled languages.

Unit 55 may revisit namespace splitting only if measured bundle size justifies it.

### Security and privacy

Rules:

- translation resources contain no secrets;
- language preference is non-sensitive;
- no authentication token is placed in localization storage;
- no user-entered or restricted medical text is sent to translation services;
- no external translation backend/service is configured;
- no raw HTML translation rendering;
- do not translate imported/user-entered content automatically;
- language changes do not alter authorization or API values;
- local preference is not trusted as a permission;
- restricted query cache rules from Unit 51 remain unchanged.

### Documentation synchronization

Update:

```txt
context/architecture.md
context/ui-context.md
context/code-standards.md
context/progress-tracker.md
```

Record:

- installed package versions;
- resource structure;
- language registry;
- Bosnian default resolution;
- no browser-language auto-detection;
- local storage key/precedence;
- English `FOUNDATION_ONLY` and non-selectable state;
- HTML/title synchronization;
- formatting locales/timezone;
- translation-key and namespace conventions;
- validation/ProblemDetails helper conventions;
- Unit 55 English enablement gate;
- verification results.

Update `context/feature-specs/00-build-plan.md` if implementation reveals a migration dependency for Unit 55.

Do not claim English UI support is complete after Unit 54.

## Implementation

### 1. Install localization packages

Install:

```txt
i18next
react-i18next
```

Verify compatibility and commit the package lock.

Do not add detector/backend/extractor packages.

### 2. Add the language registry and storage adapter

Implement:

- stable `bs`/`en` types;
- locale metadata;
- selectability/resource status;
- safe local storage;
- deterministic initial resolution;
- storage-event synchronization.

### 3. Add typed foundation resources

Create Bosnian and English resources for:

```txt
common
language
validation
errors
```

Enforce shape parity.

### 4. Configure TypeScript i18next types

Add one centralized `CustomTypeOptions` augmentation.

Prove typed foundation keys without casts.

### 5. Bootstrap the application i18next instance

Initialize one `createInstance()` instance before React render.

Configure Bosnian fallback, bundled resources, no Suspense, and no missing-key/network submission.

### 6. Synchronize the document

Implement:

- HTML `lang`;
- HTML `dir`;
- localized base title;
- updates after language change.

Keep static `index.html` Bosnian.

### 7. Add the controlled language-change service

Implement validation, in-memory switch, persistence, document sync, typed result, and safe failure behavior.

Prevent direct scattered language changes by convention.

### 8. Add language selector integration points

Create one reusable selector.

Mount it in:

- user menu;
- sign-in layout;
- required-password-change layout.

It renders nothing while only Bosnian is selectable.

### 9. Add locale formatting helpers

Implement and document:

- date-only;
- Sarajevo UTC timestamps;
- number;
- duration;
- locale lookup;
- formatter caching.

Do not migrate every existing formatter in this unit.

### 10. Add translation helper conventions

Implement:

- explicit enum-label resolver;
- generic validation-message helpers;
- localization-aware ProblemDetails foundation;
- isolated test i18n factory.

### 11. Add focused tests/checks

Cover language resolution, storage, resource shape, document sync, plurals, formatters, selector gate, and safe helper behavior using the existing test foundation/tooling.

### 12. Synchronize project documentation

Update architecture, UI context, code standards, progress tracker, and build plan where needed.

Do not mark Unit 54 complete until the required checks pass or failures are explicitly documented.

## Dependencies

Add:

```txt
i18next
react-i18next
```

Use their current stable versions compatible with the repository and pin them in the existing package lock.

Use built-in browser/JavaScript APIs:

```txt
Intl.DateTimeFormat
Intl.NumberFormat
Intl.PluralRules through i18next
localStorage
storage events
document.documentElement
```

Use existing:

- React;
- Vite;
- TypeScript;
- React Router;
- TanStack Query;
- Zod;
- React Hook Form;
- shadcn/ui;
- auth/user-menu/app-shell infrastructure;
- ProblemDetails utilities.

Do not add:

- language detector packages;
- translation HTTP backend packages;
- translation SaaS SDKs;
- extractor/parser CLI packages;
- date/number libraries;
- another localization framework;
- backend dependencies;
- a new test framework solely for this unit.

## Verification checklist

- [ ] `i18next` is installed at a current stable compatible version.
- [ ] `react-i18next` is installed at a current stable compatible version.
- [ ] Resolved versions are pinned in the existing package lock.
- [ ] No `@types/react-i18next` package is added.
- [ ] No browser detector, HTTP backend, resource backend, extractor, parser, or SaaS localization package is added.
- [ ] A centralized `AppLanguage` registry exists.
- [ ] Stable language codes are exactly `bs` and `en`.
- [ ] Bosnian maps to `bs-BA`.
- [ ] English maps to `en-GB`.
- [ ] Both initial languages are marked LTR.
- [ ] Bosnian is the default complete selectable language.
- [ ] English resources are foundation-complete but globally non-selectable in Unit 54.
- [ ] Browser/OS language does not override the Bosnian default.
- [ ] Initial resolution uses valid selectable local preference, then Bosnian.
- [ ] Invalid/non-selectable stored values safely resolve to Bosnian.
- [ ] Local storage access failure does not block startup.
- [ ] Language preference uses one versioned non-sensitive key.
- [ ] Logout/session expiration does not clear the language preference.
- [ ] No language preference is sent to the backend or stored in cookies.
- [ ] Cross-tab storage synchronization works without reload loops.
- [ ] One application-owned i18next instance is created with `createInstance()`.
- [ ] Localization initializes before React root render.
- [ ] Production code does not mutate the test/global singleton.
- [ ] Resources are statically bundled.
- [ ] No translation network request occurs.
- [ ] `fallbackLng` is Bosnian.
- [ ] Missing English keys never become the Bosnian source of truth.
- [ ] Runtime missing-key submission is disabled.
- [ ] React Suspense is not required for bundled foundation resources.
- [ ] `document.documentElement.lang` follows the resolved language.
- [ ] `document.documentElement.dir` follows the language registry.
- [ ] Static `index.html` uses `lang="bs"`.
- [ ] Base application title is localized.
- [ ] Route-title migration remains deferred to Unit 55.
- [ ] Resource files are organized by language and namespace.
- [ ] Initial namespaces are `common`, `language`, `validation`, and `errors`.
- [ ] Bosnian resources use proper `č`, `ć`, `š`, `ž`, and `đ`.
- [ ] English foundation resources match the Bosnian key shape.
- [ ] Resource shape parity is build/test checked.
- [ ] Resource values do not contain React elements/functions/raw HTML.
- [ ] One centralized i18next TypeScript module augmentation exists.
- [ ] Foundation translation keys are type-checked.
- [ ] Missing keys are not bypassed with `as any` or broad string casts.
- [ ] Translation keys use stable semantic English naming.
- [ ] Player/team/opponent/user/imported/user-entered values are not translated.
- [ ] `t()` is not called at module scope.
- [ ] Translated strings are not stored as server/client state.
- [ ] Toast text is translated when the event occurs.
- [ ] Bosnian plural forms are tested for 1, 2, 5, and 21.
- [ ] English plural forms are tested.
- [ ] Interpolation uses named values without raw HTML.
- [ ] A reusable `LanguageSelector` exists.
- [ ] Selector integration points exist in user menu, sign-in, and required-password-change layouts.
- [ ] The selector renders nothing while only Bosnian is selectable.
- [ ] No incomplete mixed English mode is exposed.
- [ ] Language change preserves route, form, session, and server-query state.
- [ ] Language change does not reload the page.
- [ ] Language change does not invalidate TanStack Query data.
- [ ] Failed language change keeps and preserves the previous language.
- [ ] Failed language change is not persisted.
- [ ] Date-only helpers avoid UTC day shifts.
- [ ] UTC instant helpers use `Europe/Sarajevo`.
- [ ] Both initial locales use 24-hour time.
- [ ] Bosnian date/number output uses `bs-BA`.
- [ ] English date/number output uses `en-GB`.
- [ ] Numeric zero is distinct from null/missing.
- [ ] Display strings are never parsed back into API numeric values.
- [ ] Duration formatting does not use date/timezone semantics.
- [ ] Explicit typed enum-label resolver exists.
- [ ] Enum translation does not dynamically trust arbitrary backend values.
- [ ] Unknown enum values fail safely without changing API values.
- [ ] Generic validation translations and helpers exist.
- [ ] No global stale-language Zod error map is introduced.
- [ ] Localization-aware ProblemDetails foundation exists.
- [ ] Arbitrary server text is not used as a translation key.
- [ ] Backend contracts remain unchanged.
- [ ] Only foundation-owned copy is migrated in Unit 54.
- [ ] Existing major feature screens are not partially migrated into mixed ownership.
- [ ] New-code localization rules are added to project standards.
- [ ] English enablement gate for Unit 55 is documented.
- [ ] English is not marked complete/selectable before that gate passes.
- [ ] Isolated test i18n instances do not mutate production state.
- [ ] Existing routes/auth/session/query behavior still works.
- [ ] No backend file or migration is changed.
- [ ] `npm run format` completes successfully.
- [ ] `npm run format:check` passes.
- [ ] `npm run lint` passes.
- [ ] Frontend typecheck/build passes using the configured project command.
- [ ] Existing approved frontend tests pass when present.
- [ ] `context/architecture.md` reflects the implemented localization model.
- [ ] `context/ui-context.md` reflects Bosnian default and gated English selection.
- [ ] `context/code-standards.md` requires translation resources for new long-lived copy.
- [ ] `context/progress-tracker.md` records actual package versions, configuration, English gate, and verification results.
