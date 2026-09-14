## Stack

- **Language and runtime**: C# on .NET 8, TypeScript targeting browser ES2023
- **Frameworks**: ASP.NET Core 8, React 19 with Vite 8
- **Key dependencies**: EF Core 8 with Npgsql for PostgreSQL, Tailwind CSS 4 with shadcn/ui, TanStack Query and Table, React Router 7 with Nuqs, xUnit, and Vitest
- **Package managers**: NuGet and npm with `package-lock.json`

## Build approach

<TBD, set by /scope>

## Application Building Context

Read the following files in order before implementing or making any architectural decision:

1. `context/project-overview.md` — product definition, goals, features, and scope
2. `context/architecture.md` — system structure, boundaries, storage model, and invariants
3. `context/ui-context.md` — theme, colors, typography, and component conventions
4. `context/code-standards.md` — implementation rules and conventions
5. `context/ai-workflow-rules.md` — development workflow, scoping rules, and delivery approach
6. `context/progress-tracker.md` — current phase, completed work, open questions, and next steps

The `context/` files are the primary source of truth.

Use relevant Codex skills or plugins when applicable:

- Project-local skills are in `.agents/skills/`
- Installed Codex plugins may provide additional optional capabilities, but they are not repository requirements

For frontend UI work, read `DESIGN.md` as the design system source of truth and also read `context/references/shadcn-components.md` before creating new UI primitives or interaction patterns.

Skills and plugins may guide implementation workflow, but must not override the project context files, active feature spec, architecture rules, code standards, or documented scope.

Update `context/progress-tracker.md` after each meaningful implementation change.

If implementation changes the architecture, scope, or standards documented in the context files, update the relevant file before continuing.

## Context files

- [backend/AGENTS.md](backend/AGENTS.md) (ASP.NET Core projects, tests, persistence, and commands)
- [frontend/AGENTS.md](frontend/AGENTS.md) (React application structure, UI rules, tests, and commands)
