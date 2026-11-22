# Review, chore/workflow-baseline, 2026-11-22

**Reviewed by**: GPT-6.2 Sol (author on GPT-6.1 Sol)

**Scope**: 208 files, branch vs `origin/main`, plus `skills-lock.json` reviewed separately. PR #1 compares `a65251624a222997c37847ff6553cced0438c54d` with `4c34c9a47a50a7652ac01fe60487814852304286`.

**Verdict**: Changes requested

## Summary

The PR establishes repository authority, installs the shared JSMastery workflows, and preserves all 73 archived files from the recorded rollout source. The nine imported skills match the declared upstream revision. Two integration gaps remain when using the baseline as documented. A Windows checkout can leave every project-local Claude skill unavailable, and the first design-to-build handoff for an existing planned feature has no defined path through the scope owner.

## Major

### 🟠 Document the Windows checkout requirement for Claude skills, `.claude/skills/check:1`

**Problem**: All 41 entries are valid relative Git symlinks, and all resolve in this checkout with repository-local `core.symlinks=true`. The installed Git for Windows configuration sets `core.symlinks=false`. A disposable Windows Git checkout with the default setting materialized all 41 entries as plain files. In that checkout, `.claude/skills/check/SKILL.md` was unavailable. The repository identifies the shared skill location without documenting the checkout prerequisite for Claude. See `AGENTS.md:30`.

**Why it matters**: A developer using that Git configuration can follow the repository setup and find none of the 41 project-local Claude skills. The authority rules apply after discovery and cannot repair the checkout.

**Suggested fix**: Add a short Windows setup note in `README.md` that requires OS permission to create symlinks and `core.symlinks=true` before checkout, for example through `git clone -c core.symlinks=true`. Verify that a fresh checkout following the documented steps exposes `.claude/skills/check/SKILL.md` and resolves all 41 directory links. Keep the tracked relative targets.

### 🟠 Define the scope-owned handoff for a confirmed feature spec, `AGENTS.md:26`

**Problem**: Spec 0001 requires `/scope` to change `docs/scope/` and requires an explicit scope link before a `Proposed` feature specification governs a build. The root stop-on-conflict rule correctly prevents the architect completion flow from writing scope directly. The installed `/scope` routes provide no matching operation for linking a confirmed spec to an existing feature and updating its checklist. A named feature routes to enrollment, and replan reconciles shipped features and queues the next work. See `docs/specs/0001-project-context-authority.md:99`, `:113`, `.agents/skills/architect/internal/after-subagent.md:62`, `.agents/skills/scope/SKILL.md:19`, `:75`, and `.agents/skills/scope/modes/replan.md:7`.

**Why it matters**: Player performance history and trends already has a planned summary row and a `Design it (spec)` step in another file. Once `/architect` confirms its design, following repository authority leaves the spec unlinked. `/develop` cannot treat it as the build contract, and following the vendor's single-file write path puts the summary row and feature heading at risk of diverging. See `docs/scope/index.md:28` and `docs/scope/player-analysis.md:15`.

**Suggested fix**: Record a repository-specific handoff in root `AGENTS.md`. Have `/architect` return the confirmed spec path and feature, then have `/scope` link that spec, update the feature's decision and execution checklist, and update the summary row and area heading together. Verify the handoff against an existing split-scope feature using a disposable checkout. Keep the vendored instructions intact.

## Minor

### 🟡 Require runtime evidence before sync advances a feature, `.agents/skills/sync/agent-prompt.md:163`

**Problem**: The upstream sync procedure accepts the existence of a saved `verify.md` as verification evidence. A saved checklist can still be entirely unchecked. It also reconciles linked specification status before updating scope in the same run. See `.agents/skills/sync/agent-prompt.md:126` and `:150`.

**Why it matters**: Spec 0001 requires a scope-linked `Accepted` feature to be built and verified. That higher authority keeps an unchecked checklist from qualifying. A future sync run could still leave its report or the scope and spec status out of step when it relies on the shortcut. The current single standalone specification falls outside feature status mirroring. See `docs/specs/0001-project-context-authority.md:124` and `.agents/skills/sync/agent-prompt.md:136`.

**Suggested fix**: When syncing a future scope-linked feature, use recorded passing runtime results for verification and reconcile its spec status from the final scope state.

## Strengths

- All 71 files in the nine JSMastery skill directories match the upstream Git tree at the declared revision. The comparison covered the full path set, Git modes, and blob IDs. [JSMastery upstream tree at `43b69e4`](https://github.com/jsmastery-pro/skills/tree/43b69e44c9ca905fe3a3418ccdf4102255e20d40/skills)
- The recorded source `cfeab02:context` and PR destination `4c34c9a:docs/legacy/context` share tree ID `2ee402844a0749f9dd96900e5567347ed5e608d2`. All 73 manifest hashes reproduce from the Windows checkout bytes. The five added lines in the PR-base progress tracker comparison predate the recorded source.
- The ratified spec precedes the focused documentation rollout commit `e30bd77`. Reversing its complete patch in a disposable index restored every rollout path to its pre-rollout state. GitHub allows a merge commit, and auto-merge is unset. A merge commit preserves `e30bd77` and its source revision in `main` ancestry for the activation and rollback procedure. See `docs/specs/0001-project-context-authority.md:73` and `:164`.
- Root and area Claude shims import their sibling `AGENTS.md`. The current authority rules also resolve the upstream build-order and design-token precedence differences. The area guidance identifies the existing design and API boundary drift and documents the actual xUnit and Vitest commands.

## Test coverage

Reviewed the committed PR diff and lockfile, compared archive and upstream Git objects, checked the migration manifest, verified symlink targets and checkout behavior, and ran `git diff --check`. A static review of current Markdown found no unresolved repository link or heading anchor. The current PR contains workflow and documentation changes. No application tests or live Claude discovery were run. No scope row matches the workflow baseline.
