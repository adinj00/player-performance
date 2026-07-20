# Official Agent Skills

Installation/review date: 2026-07-20. All copies live in `.agents/skills/<skill-name>/` and are tracked with `skills-lock.json`. They were copied from approved official repositories; the installed copy is advisory, while `AGENTS.md`, context files, and active feature specs remain authoritative.

| Skill | Official repository | Installed path | Purpose and project applicability | Caveat | Status |
| --- | --- | --- | --- | --- | --- |
| shadcn | `shadcn/ui` | `.agents/skills/shadcn/` | shadcn CLI and composition for the Vite UI | Use official CLI; do not visually override generated primitives | Available when applicable |
| vercel-react-best-practices | `vercel-labs/agent-skills` | `.agents/skills/vercel-react-best-practices/` | React client performance reviews | Ignore Next.js/RSC/server-only rules | Available when applicable |
| vercel-composition-patterns | `vercel-labs/agent-skills` | `.agents/skills/vercel-composition-patterns/` | React component API/composition review | Only supported React patterns; no speculative refactors | Available when applicable |
| web-design-guidelines | `vercel-labs/agent-skills` | `.agents/skills/web-design-guidelines/` | UI, accessibility, and UX review | Fetch its official checklist before a review; preserve light FK VeleÅ¾ design | Available when applicable |
| frontend-design | `anthropics/skills` | `.agents/skills/frontend-design/` | Design critique and refinement | Existing tokens and visual identity override redesign guidance | Available when applicable |
| setup-local-sdk | `dotnet/skills` | `.agents/skills/setup-local-sdk/` | Local SDK setup | Not needed for the committed .NET 8 setup unless approved | Optional |
| configuring-opentelemetry-dotnet | `dotnet/skills` | `.agents/skills/configuring-opentelemetry-dotnet/` | OpenTelemetry configuration | No telemetry/provider package additions without a spec | Optional |
| convert-blazor-server-to-webapp | `dotnet/skills` | `.agents/skills/convert-blazor-server-to-webapp/` | Blazor migration | Not applicable: frontend is React/Vite | Optional |
| dotnet-webapi | `dotnet/skills` | `.agents/skills/dotnet-webapi/` | Minimal API endpoint guidance | Follow established contracts and Clean Architecture | Available when applicable |
| minimal-api-file-upload | `dotnet/skills` | `.agents/skills/minimal-api-file-upload/` | Secure/streaming Minimal API uploads | Cookie-authenticated CSRF and existing storage rules remain mandatory | Available when applicable |
| optimizing-ef-core-queries | `dotnet/skills` | `.agents/skills/optimizing-ef-core-queries/` | EF Core query review | Require evidence; preserve scopes and query contracts | Available when applicable |
| convert-to-cpm | `dotnet/skills` | `.agents/skills/convert-to-cpm/` | Central Package Management migration | Not authorized without an approved spec | Optional |
| assertion-quality | `dotnet/skills` | `.agents/skills/assertion-quality/` | Assertion-depth analysis | Analysis only; use language extension guidance first | Optional |
| code-testing-agent | `dotnet/skills` | `.agents/skills/code-testing-agent/` | Test generation/coverage scaffolding | Do not add tests or packages outside task scope | Optional |
| code-testing-extensions | `dotnet/skills` | `.agents/skills/code-testing-extensions/` | Language testing references | Support skill for testing workflows | Optional |
| coverage-analysis | `dotnet/skills` | `.agents/skills/coverage-analysis/` | Coverage/CRAP analysis | Run only when coverage work is requested | Optional |
| crap-score | `dotnet/skills` | `.agents/skills/crap-score/` | Targeted CRAP analysis | Requires named code target and coverage data | Optional |
| detect-static-dependencies | `dotnet/skills` | `.agents/skills/detect-static-dependencies/` | Static dependency scan | Analysis only | Optional |
| filter-syntax | `dotnet/skills` | `.agents/skills/filter-syntax/` | Test filter reference | Support skill for the test runner | Optional |
| find-untested-sources | `dotnet/skills` | `.agents/skills/find-untested-sources/` | Untested-source inventory | Analysis only | Optional |
| generate-testability-wrappers | `dotnet/skills` | `.agents/skills/generate-testability-wrappers/` | DI wrappers for static dependencies | Requires explicit testability change scope | Optional |
| grade-tests | `dotnet/skills` | `.agents/skills/grade-tests/` | Per-test quality review | Review-only | Optional |
| migrate-static-to-wrapper | `dotnet/skills` | `.agents/skills/migrate-static-to-wrapper/` | Static-to-wrapper migration | Not authorized without a scoped change | Optional |
| mtp-hot-reload | `dotnet/skills` | `.agents/skills/mtp-hot-reload/` | Microsoft Testing Platform test iteration | Do not migrate testing platform by implication | Optional |
| platform-detection | `dotnet/skills` | `.agents/skills/platform-detection/` | Test platform detection | Support skill for the test runner | Optional |
| run-tests | `dotnet/skills` | `.agents/skills/run-tests/` | Correct `dotnet test` command selection | Detect VSTest/MTP before filtered runs | Available when applicable |
| test-analysis-extensions | `dotnet/skills` | `.agents/skills/test-analysis-extensions/` | Test-analysis language references | Support skill for test analysis | Optional |
| test-anti-patterns | `dotnet/skills` | `.agents/skills/test-anti-patterns/` | Test anti-pattern audit | Review-only | Optional |
| test-gap-analysis | `dotnet/skills` | `.agents/skills/test-gap-analysis/` | Mutation-style test gap analysis | Review-only | Optional |
| test-smell-detection | `dotnet/skills` | `.agents/skills/test-smell-detection/` | Test smell audit | Review-only | Optional |
| test-tagging | `dotnet/skills` | `.agents/skills/test-tagging/` | Test trait inventory | Do not auto-tag outside approved scope | Optional |
| writing-mstest-tests | `dotnet/skills` | `.agents/skills/writing-mstest-tests/` | MSTest test authoring | Use only if this is the detected framework and tests are requested | Optional |

## Update procedure

1. Obtain explicit owner approval for a new or updated skill.
2. Confirm its source is one of the official repositories above and install project-locally with copied files (`--agent codex --copy --yes`); never use global scope or symlinks.
3. Review `skills-lock.json`, `npx --yes skills@latest list --agent codex`, ignore status, and the diff. Do not modify vendored upstream files.
4. Update this inventory and `official-agent-skills-review.md` with provenance, date, applicability, and verification.
