You are working in the root of the Player Performance Data System repository.

Your task is to centralize the project's official agent skills in the repository root, update the project documentation to use that location, compare the installed guidance against the current codebase, and apply only safe, evidence-backed improvements.

## Authority and scope

Read these files first, in this order:

1. `AGENTS.md`
2. `context/project-overview.md`
3. `context/architecture.md`
4. `context/ui-context.md`
5. `context/code-standards.md`
6. `context/ai-workflow-rules.md`
7. `context/progress-tracker.md`
8. the currently active feature spec, if one exists

The project context, active spec, implemented contracts, architecture, and code standards are authoritative.

Agent skills are advisory. They must never override the project.

Preserve these project decisions:

- ASP.NET Core and target framework remain .NET 8 unless a separately approved upgrade spec exists.
- Frontend remains React + Vite + TypeScript, not Next.js.
- UI remains light-only with FK Velež red, white, and gold identity.
- Bosnian Latin remains the current only UI language.
- shadcn/ui primitives and semantic design tokens remain authoritative.
- Clean Architecture, vertical slices, backend authorization, team scope, privacy boundaries, audit rules, and workload comparability remain unchanged.
- Do not add product features, backend endpoints, migrations, packages, or visual redesigns unless directly required by a confirmed defect.
- Do not implement deferred localization Units 54/55.
- Do not choose hosting, object storage, email, or another provider.

## Official-source whitelist

Install and use skills only from these official repositories:

- `https://github.com/dotnet/skills`
- `https://github.com/shadcn/ui`
- `https://github.com/vercel-labs/agent-skills`
- `https://github.com/anthropics/skills`

Do not install community forks, copied variants, marketplace mirrors, or “originally from” repositories.

Do not install any additional skill without explicit owner approval.

## Required root layout

All project-local skills must live under:

```text
.agents/skills/<skill-name>/SKILL.md
```

Use project scope, not global scope.

Use copied files, not symlinks:

```text
--agent codex --copy --yes
```

Commit the copied skill files so the project has a reproducible reviewed skill set.

Do not edit vendored upstream `SKILL.md` files to add project-specific rules. Put project overrides in `AGENTS.md` and `context/*`.

## Inventory before modification

Before changing anything:

1. Run `git status`.
2. Inspect:
   - `.agents/`
   - `frontend/.agents/`
   - `.codex/`
   - current installed Codex plugins through `codex plugin list --json`
3. Record every skill/plugin name and its source.
4. Search all active documentation:
   ```bash
   rg -n 'frontend/\.agents|installed backend Codex plugins|Codex skills/plugins|\.agents/skills' \
     AGENTS.md context
   ```
5. Determine whether `frontend/.agents` contains:
   - only an upstream shadcn skill;
   - modified upstream files;
   - any unique/custom files.

Do not delete unique or modified files silently. Stop and report them before deletion.

## Installation

Run the approved project-local installation script supplied with this task, or execute its equivalent commands.

Required frontend skills:

- `shadcn` from `shadcn/ui`
- `vercel-react-best-practices` from `vercel-labs/agent-skills`
- `vercel-composition-patterns` from `vercel-labs/agent-skills`
- `web-design-guidelines` from `vercel-labs/agent-skills`
- `frontend-design` from `anthropics/skills`

Required .NET skills are the individual official skills corresponding to the currently installed:

- `dotnet`
- `dotnet-aspnetcore`
- `dotnet-data`
- `dotnet-nuget`
- `dotnet-test`

Use direct official GitHub skill paths.

Set:

```bash
export DISABLE_TELEMETRY=1
```

Do not use `-g`.

After installation verify:

```bash
npx --yes skills@latest list --agent codex
find .agents/skills -mindepth 2 -maxdepth 3 -name SKILL.md -print | sort
git check-ignore -v .agents/skills/*/SKILL.md || true
```

If `.agents` is ignored, update `.gitignore` narrowly so reviewed project skill files can be committed without unignoring unrelated private agent state.

## Move shadcn from frontend to root

After the official root `shadcn` skill is installed:

1. Compare the root and old frontend copies.
2. Preserve/report any unique project content.
3. Confirm `.agents/skills/shadcn/SKILL.md` exists.
4. Remove `frontend/.agents` only when it contains no unique required files.
5. Verify there are no remaining references to `frontend/.agents`.

Do not keep duplicate shadcn copies.

## Documentation updates

Update `AGENTS.md` so it states:

- project-local skills are under `.agents/skills/`;
- only official allowlisted sources are permitted;
- context files and active specs override skills;
- skill applicability must be checked against the actual stack;
- project-specific overrides belong in context files, not vendored skill files.

Update `context/ai-workflow-rules.md` so it states:

- root project skills are the normal source for frontend and backend workflow guidance;
- globally installed plugins are optional capability extensions, not repository requirements;
- new skills require explicit owner approval;
- official provenance must be recorded;
- skills must not be applied mechanically.

Update `context/code-standards.md` with these boundaries:

- Vercel React guidance: apply only rules relevant to React/Vite/client-side code; ignore Next.js-only or server-component guidance.
- Vercel composition guidance: apply only patterns supported by the installed React version and existing architecture.
- Anthropic frontend-design: use for critique and refinement within the existing FK Velež design system; do not re-theme or redesign the product.
- shadcn: use the official CLI and documented component composition; do not manually edit generated primitives for project-specific visual overrides.
- .NET skills: project remains .NET 8; do not perform framework, test-platform, package-management, or architecture migrations without an approved spec.
- Skills never authorize new dependencies or product scope on their own.

Update path-only stale references in:

```text
context/feature-specs/*.md
```

Change references from `frontend/.agents/` or globally required backend plugins to the root `.agents/skills/` workflow.

For completed historical specs, change only tooling-location wording. Do not rewrite business requirements, acceptance criteria, dependencies, or implemented history.

Create:

```text
context/references/official-agent-skills.md
```

Record:

- skill name;
- official repository;
- installed path;
- purpose;
- project applicability;
- important caveats;
- installation/review date;
- update procedure;
- whether the skill is mandatory or optional.

Create:

```text
context/references/official-agent-skills-review.md
```

## Plugin handling

Do not automatically uninstall all Codex plugins.

First report plugin-only capabilities.

In particular:

- the official `dotnet` plugin may include C# LSP integration that copied skills do not reproduce;
- `dotnet-test` may include custom agents in addition to skills.

Recommended default:

- keep `dotnet` installed for LSP capability;
- keep `dotnet-test` until its plugin-only agents are reviewed;
- after root skill parity is verified, duplicate skill-only bundles such as `dotnet-aspnetcore`, `dotnet-data`, and `dotnet-nuget` may be removed with owner approval;
- leave `Default templates` unchanged because it is unrelated to application coding skills.

Do not remove a marketplace while an intentionally retained plugin still depends on it.

Include exact proposed removal commands in the final report, but execute them only if the owner explicitly authorized plugin removal.

## Skill-guided repository review

After installation, read the relevant skills and compare them against the actual repository.

Do not perform a broad speculative rewrite.

Classify each finding as:

```text
APPLY_NOW
DEFER
NOT_APPLICABLE
CONFLICTS_WITH_PROJECT
ALREADY_COMPLIANT
```

### Frontend review boundaries

Use:

- `shadcn`
- `vercel-react-best-practices`
- `vercel-composition-patterns`
- `web-design-guidelines`
- `frontend-design`

Review for high-confidence issues such as:

- unnecessary rerenders or data waterfalls;
- unstable query/component patterns;
- excessive boolean prop APIs;
- duplicated component state;
- inaccessible controls;
- semantic HTML/focus/keyboard problems;
- broken responsive overflow;
- inconsistent use of shadcn composition;
- raw colors or design-token bypasses;
- clearly generic or inconsistent visual composition.

Do not:

- convert Vite to Next.js;
- introduce React Server Components;
- add Vercel hosting assumptions;
- replace TanStack Query, nuqs, React Hook Form, Zod, TanStack Table, Recharts, or shadcn;
- re-theme the app;
- add dark mode;
- introduce decorative motion that conflicts with reduced-motion/accessibility rules;
- change working code merely to match a stylistic preference.

`web-design-guidelines` may require network access to fetch its current upstream checklist. If network access is unavailable, mark that review as deferred; do not substitute an unofficial copy.

### Backend review boundaries

Use only applicable official .NET skills.

Review for high-confidence issues such as:

- ASP.NET Core middleware ordering;
- Minimal API composition;
- streaming upload correctness;
- cancellation propagation;
- EF Core query shape and N+1 risks;
- no-tracking/read projections;
- bounded pagination;
- test quality and coverage gaps;
- package/reference problems;
- incorrect async/disposal patterns.

Do not:

- upgrade from .NET 8;
- convert testing frameworks;
- adopt Central Package Management;
- add OpenTelemetry/provider packages;
- change architecture;
- introduce new package versions;
- change database schema;
- alter API contracts;
- weaken authorization/privacy/audit rules;

unless an existing confirmed defect requires it and the change remains inside current scope.

A skill named for migration, upgrade, telemetry, Blazor, or package management is not automatically applicable.

## Applying improvements

Apply only `APPLY_NOW` findings that are:

- high confidence;
- supported by current project tests/context;
- behavior-preserving or a confirmed bug/accessibility/performance fix;
- reasonably reviewable in this branch.

For every applied change:

- explain the skill/rule that motivated review;
- explain why it applies to this React/Vite or .NET 8 project;
- preserve contracts;
- add/update tests where practical;
- update context only when a real standard/architecture decision changes.

Do not create large unrelated diffs.

Put deferred findings into the review document with reason and suggested future scope.

## Required review document

Populate:

```text
context/references/official-agent-skills-review.md
```

with:

1. Installed official skill inventory
2. Old `frontend/.agents` comparison and cleanup result
3. Codex plugin parity and plugin-only capability report
4. Repository findings table:
   - source skill
   - file/area
   - finding
   - classification
   - rationale
   - action
5. Applied changes
6. Deferred/not-applicable/conflicting guidance
7. Package/dependency changes, ideally none
8. Verification commands and results
9. Remaining risks

Do not claim a skill was used unless it was actually read.

## Verification

Run the repository's actual configured commands.

At minimum:

```bash
npx --yes skills@latest list --agent codex

rg -n 'frontend/\.agents|installed backend Codex plugins' AGENTS.md context
rg -n '\.agents/skills' AGENTS.md context

dotnet format PlayerPerformance.sln whitespace --no-restore
dotnet format PlayerPerformance.sln whitespace --no-restore --verify-no-changes
dotnet build
dotnet test

cd frontend
npm run format
npm run format:check
npm run lint
# run the configured typecheck/build command
# run existing frontend tests when present
```

Use the repository's exact solution and script names when they differ.

Also run:

```bash
git status --short
git diff --check
git diff --stat
```

Inspect the final diff for:

- accidental vendored skill edits;
- community/unapproved sources;
- stale `frontend/.agents` references;
- Next.js-only code;
- .NET upgrade changes;
- new dependencies;
- architecture/scope changes;
- secrets;
- generated build artifacts.

Update `context/progress-tracker.md` with the actual result.

## Final response

Report:

- installed root skills;
- official sources;
- removed old frontend skill path;
- documentation files updated;
- plugins retained and why;
- optional plugin removal commands;
- applied code improvements;
- deferred findings;
- exact verification results;
- commits to create.

Do not say the repository is improved or compliant without concrete evidence.
