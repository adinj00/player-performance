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

## Confirmed feature spec handoff

For an existing planned feature, use this handoff under [spec 0001](docs/specs/0001-project-context-authority.md).

1. `/architect` owns the decision and spec and leaves scope edits to `/scope`. After the engineer confirms the design, leave the unbuilt feature spec `Proposed` and return the feature number, name, and spec path under `docs/specs/` for `/scope link confirmed spec for feature <number>: <spec path>`.
2. `/scope` treats that request as an update to the existing feature. Match its number and name in `docs/scope/index.md` and the area file, and verify that the confirmed spec covers that feature. In the area file, add the relative spec link, tick the decision checkbox labeled `(spec)`, and add an unchecked `Build it` box for `/develop <feature>` with a short rollup of milestones and their acceptance criteria from the spec's build plan. Add unchecked closing steps for the feature's workflow tier (`/check verify` for Alpha and above, `/test` for Beta and above, and `/check review` and `/document` for GA).
3. In the same scope update, set both the index row and the area heading to `in-progress`, replacing `needs a decision` in the heading. Update the area's status summary in the index to match. Preserve the feature's number, name, phase, intent, and definition of done.
4. `/develop` uses the spec as the build contract for this feature only after `/scope` completes the update and the linked spec passes the build gate. Feature specs reach `Accepted` after the feature is built and verified.

## Area guidance

- [backend/AGENTS.md](backend/AGENTS.md) (ASP.NET Core projects, tests, persistence, and commands)
- [frontend/AGENTS.md](frontend/AGENTS.md) (React application structure, UI rules, tests, and commands)
