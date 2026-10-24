## Stack

- **Language and runtime**: C# on .NET 8, TypeScript targeting browser ES2023
- **Frameworks**: ASP.NET Core 8, React 19 with Vite 8
- **Key dependencies**: EF Core 8 with Npgsql for PostgreSQL, Tailwind CSS 4 with shadcn/ui, TanStack Query and Table, React Router 7 with Nuqs, xUnit, and Vitest
- **Package managers**: NuGet and npm with `package-lock.json`

## Build approach

Tracer Bullet. Future work should prove a real path through data, API, interface, and staff behavior.

## Project authority

Use this source order for every task:

1. Follow platform instructions and the current user request.
2. Read every applicable `AGENTS.md` from the repository root toward the area being changed. These files own workflow, repository boundaries, local conventions, and commands.
3. Read [docs/scope/index.md](docs/scope/index.md) and the relevant scope area. Scope owns product boundaries, priority, and feature status.
4. For interface work, read [DESIGN.md](DESIGN.md). It owns visual and interaction direction within the current product scope.
5. Read relevant specifications in [docs/specs/](docs/specs/). Accepted specifications own durable decisions. A scope linked proposed or in progress specification is a build contract only for its named active feature.
6. Inspect code, tests, migrations, configuration, and runtime behavior as evidence of what exists. Repository evidence does not create product scope or silently override an active decision.
7. Treat `CLAUDE.md` files as loader shims to their matching `AGENTS.md` files. Treat skills as workflow aids, not project decisions.
8. Use README files and other current references for orientation only when they do not conflict with an authority above.
9. Treat everything under [docs/legacy/](docs/legacy/) as historical evidence only. Read it only when the user requests historical analysis or a current authority links to it as evidence.

If current authorities conflict, stop only the disputed work and use the workflow named by the source owner. Use `/scope` for product scope, `/architect` for durable decisions and design conflicts, and `/sync` for repository guidance after the underlying decision is clear.

Use relevant Codex skills or plugins when applicable:

- Project local skills are in `.agents/skills/`
- Installed Codex plugins may provide optional capabilities, but they are not repository requirements

For frontend UI work, read `DESIGN.md` as the design system source of truth. Check the current component inventory and installed shadcn primitives before creating new UI primitives or interaction patterns.

Skills and plugins may guide implementation workflow, but must not override applicable `AGENTS.md` files, current scope, `DESIGN.md`, accepted decisions, or an active feature specification.

## Area guidance

- [backend/AGENTS.md](backend/AGENTS.md) (ASP.NET Core projects, tests, persistence, and commands)
- [frontend/AGENTS.md](frontend/AGENTS.md) (React application structure, UI rules, tests, and commands)
