# Official Agent Skills Migration and Repository Review

Review date: 2026-07-20. Scope: inventory, root-skill migration, documentation, and evidence-backed review only. No production dependencies, endpoint contracts, migrations, or design changes were added.

## Inventory and provenance

`npx --yes skills@latest list --agent codex` reports 32 project skills under `.agents/skills`, all enabled for Codex. `skills-lock.json` records official provenance: 26 skills from `dotnet/skills`, one from `shadcn/ui`, three from `vercel-labs/agent-skills`, and one from `anthropics/skills`. See [official-agent-skills.md](official-agent-skills.md) for the complete per-skill inventory.

## Old frontend copy

`frontend/.agents/skills/shadcn` contained only the upstream shadcn skill. Its 15 files have the same relative paths as `.agents/skills/shadcn`; both `SKILL.md` files have SHA-256 `227C5EC4D92FEA9A350495AA212094D4E79D2B0BEF19278B1F6729897F5FC6A3`. No unique or modified project content was found. The duplicate directory was removed after the root copy was verified.

## Plugin parity

`codex plugin list --json` reports no installed or available plugins. Copied skills therefore replace no active plugin capability. If official plugins are installed later, retain `dotnet` for any C# LSP capability and retain `dotnet-test` until its custom agents are reviewed; skill-only bundles may then be considered for removal only with owner approval. `Default templates` remains unrelated and should be left unchanged.

## Findings

| Source skill | File/area | Finding | Classification | Rationale and action |
| --- | --- | --- | --- | --- |
| shadcn | `frontend/src/components/ui`, UI conventions | Generated primitives and semantic-token rules already govern composition | ALREADY_COMPLIANT | Existing standards prohibit local visual overrides; no component rewrite warranted. |
| vercel-react-best-practices | React/Vite frontend | Next.js/RSC, server actions, `next/dynamic`, and SWR guidance | NOT_APPLICABLE | The application is a Vite SPA using TanStack Query; no framework substitution. |
| vercel-react-best-practices | Client data/query patterns | Existing feature hooks and TanStack Query address caching and state separation | ALREADY_COMPLIANT | No specific waterfall or unstable-query defect evidenced by static review. |
| vercel-composition-patterns | Feature/app component APIs | No confirmed boolean-prop proliferation requiring a broad refactor | DEFER | Re-evaluate when a concrete component API becomes difficult to extend. |
| web-design-guidelines | Frontend accessibility/UX | Latest official checklist fetched and compared to existing Unit 56 safeguards | ALREADY_COMPLIANT | Skip link, landmarks, route headings, reduced motion, URL state, and token rules are documented; no high-confidence defect found in static scan. |
| frontend-design | FK VeleÅ¾ visual system | Its re-theme/novel-visual-direction instructions conflict with approved light-only identity | CONFLICTS_WITH_PROJECT | Use only for constrained critique; retain red/white/gold tokens and existing design language. |
| dotnet-webapi | Minimal API endpoints | Advice to introduce .NET 9+/10 API conventions or generic endpoint rewrites | CONFLICTS_WITH_PROJECT | The project is .NET 8 and has established contracts/architecture. Existing endpoints use cancellation tokens and service boundaries where required. |
| minimal-api-file-upload | Imports/media streaming upload | MultipartReader, antiforgery, rate limits, and storage handling are already intentionally implemented | ALREADY_COMPLIANT | No confirmed streaming defect remains after the recorded empty-stream fix. |
| optimizing-ef-core-queries | Read paths | Read queries consistently use `AsNoTracking`, projections, and bounded pagination | ALREADY_COMPLIANT | No logged N+1 or slow-query evidence; do not introduce compiled queries speculatively. |
| convert-blazor-server-to-webapp | Frontend architecture | Blazor migration | NOT_APPLICABLE | Frontend is React/Vite. |
| convert-to-cpm, mtp-hot-reload, setup-local-sdk | Tooling | Package/test/SDK migration guidance | DEFER | Requires a separately approved tooling task. |
| configuring-opentelemetry-dotnet | Observability | New telemetry/provider setup | DEFER | No approved observability/provider decision. |
| Test-analysis skills | Test suite | Broad test-quality or coverage audit | DEFER | No requested test-audit scope; running it would create a separate analysis task. |

## Applied changes

- Centralized official copied skills under `.agents/skills/` (already installed before this review) and removed the verified duplicate `frontend/.agents` copy.
- Updated `AGENTS.md`, workflow rules, code standards, and historical feature-spec tooling-location wording.
- Added the official source inventory and this review record.

## Dependency changes

None.

## Remaining risks

- Static review cannot prove runtime performance or authenticated end-to-end accessibility; follow the existing Unit 56 handoff matrix.
- Re-run the targeted EF/query review when production telemetry or database evidence identifies a slow path.
- New skills or plugin removal require explicit owner approval.
