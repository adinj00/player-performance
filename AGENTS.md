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

- Frontend skills are in `frontend/.agents/`
- Backend skills/plugins are available through the installed Codex plugins

For frontend UI work, also read `context/references/shadcn-components.md` before creating new UI primitives or interaction patterns.

Skills and plugins may guide implementation workflow, but must not override the project context files, active feature spec, architecture rules, code standards, or documented scope.

Update `context/progress-tracker.md` after each meaningful implementation change.

If implementation changes the architecture, scope, or standards documented in the context files, update the relevant file before continuing.
