# Frontend

## Overview

This area contains the React 19 and Vite staff application. It owns route composition, shared UI, feature modules, browser data access, and the FK Velež visual system.

## Key files

| File | Owns |
|---|---|
| `package.json` | Dependencies and frontend commands |
| `src/app/app-routes.tsx` | Router composition, lazy route loading, and access boundaries |
| `src/app/providers.tsx` | Stable application providers and TanStack Query defaults |
| `src/index.css` | Semantic design tokens, fonts, focus styles, and implemented theme values |
| `components.json` | shadcn configuration and source aliases |
| `vite.config.ts` | Vite, React, Tailwind, and the `@/` alias |
| `vitest.config.ts` | Vitest and browser test setup |

## Commands

Run these from `frontend/`.

```powershell
npm ci
npm run dev
npm run format
npm run format:check
npm run lint
npm run test
npm run build
```

## Conventions

- Keep application setup and routing in `src/app/`, route wrappers in `src/pages/`, feature code in `src/features/`, and genuinely shared code in `src/components/` or `src/lib/`.
- Use `@/` for source imports. Same folder relative imports are acceptable when clearer.
- Keep server state in TanStack Query. Use the existing Nuqs adapter only for URL search state that the documented ownership rules say should be shareable.
- Use React Hook Form with Zod for validated forms.
- Treat `src/components/ui/` as generated shadcn primitives. Add primitives with the CLI, then compose application behavior outside that folder.
- Use only semantic token utilities backed by `src/index.css`. Write visible product copy in Bosnian Latin with the correct characters.
- Keep protected route pages lazy loaded through `src/app/app-routes.tsx` unless the route is part of the initial authentication shell.

## Gotchas

- `VITE_*` values are public browser configuration. Never put a secret in `frontend/.env.local` or a Vite variable.
- When a shadcn `Button` renders a React Router link or an anchor, preserve link semantics with the supported Base UI composition and `nativeButton={false}`.
- Route focus management should react to path changes. Search, filter, tab, and pagination updates must not steal focus.
- Charts need a text description and an equivalent bounded table or list. Workload charts must not combine incompatible comparison groups.
- Root `DESIGN.md` is present and defines the intended dark stadium night and deep slate design system. The current `src/index.css` and frontend implementation appear light only, so this remains unresolved design drift. Do not resolve it during `/audit`.
- `context/code-standards.md` says Nuqs is not installed, while `package.json` and current frontend code install and use it. This is unresolved documentation and code drift. Do not treat either side as the new convention without an explicit decision.
- The context rules place high frequency Users filters in local React state, while `src/features/users/users-page.tsx` currently stores them in Nuqs URL state. This is unresolved documentation and code drift. Do not treat either side as the new convention without an explicit decision.

_Drafted by /audit from the repo, worth a quick human pass. Edit freely: once a line stops matching this draft, later runs treat it as curated and will flag rather than overwrite it._
