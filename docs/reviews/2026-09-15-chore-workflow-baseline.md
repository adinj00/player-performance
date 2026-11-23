# Review, chore/workflow-baseline, 2026-09-15

**Reviewed by**: gpt-6-astra, High reasoning (author on gpt-5.6-sol, Medium reasoning)
**Scope**: 200 files, chore/workflow-baseline vs main, including the complete working tree diff from merge base `a65251624a222997c37847ff6553cced0438c54d`
**Verdict**: Changes requested

## Summary

The branch introduces shared workflow skills and a current documentation authority model, with the former context tree preserved as historical evidence. The archive boundary, loader links, and current documentation links are sound, and application behavior is unchanged. Two workflow defects need correction before this becomes the normal development process: reconciliation can mistake a verification plan for completed verification, and its scope validation does not support the split scope format installed by this branch.

## Major

### 🟠 Require passing evidence before ticking verification, `.agents/skills/sync/agent-prompt.md:163`

**Problem**: The reconciliation rule treats either the existence of a `verify.md` file or a recorded passing runtime verification as sufficient evidence to tick `Verify it`. However, `/develop` creates that file as an unchecked plan before `/check verify` runs (`.agents/skills/develop/flow/build.md:108`). The adjacent `Test it` rule likewise checks for covering test files without requiring a passing result, although `/test` can deliberately write tests without running them or leave tests failing on real application bugs.

**Why it matters**: A normal sequence of saving verification steps and writing tests without running them can be marked verified and tested by `/sync`. The status rule at line 171 can then close the feature, and subsequent spec reconciliation can promote its decision to `Accepted`. This undermines the branch's distinction between implementation presence and demonstrated completion, especially for the authentication and permission workflows whose scope deliberately leaves verification open.

**Suggested fix**: Require a recorded passing result for the relevant feature and reviewed code state before ticking either box. Treat an unchecked verification checklist and unexecuted test files as pending work. Preserve failed or blocked outcomes, and only honor an explicitly recorded user decision to skip a stage. Check the reconciliation path with an unchecked `verify.md`, an unexecuted suite, and a failing suite; none should become a passing verification or test result.

### 🟠 Make scope reconciliation understand index plus area files, `.agents/skills/sync/agent-prompt.md:156`

**Problem**: The malformed scope gate expects an At a glance table and feature sections in the supplied scope file, and line 171 requires updating both there. The actual supported shape separates those responsibilities: `docs/scope/index.md` holds the table, while `docs/scope/core-player-match-loop.md` and the other area files hold the feature sections. `/sync` is told to operate only on the supplied relevant scope files, and `/develop` similarly restricts its updates to the one Step 0 scope file (`.agents/skills/develop/flow/build.md:98`). There is no explicit companion index resolution or coordinated update path.

**Why it matters**: On the next feature change, a valid area file can be rejected as malformed. A consumer that proceeds with only the area file instead leaves the central table stale. The baseline therefore cannot reliably keep its own authoritative status and resume instructions synchronized using the workflow it installs.

**Suggested fix**: Resolve scope as either a single file or an index plus its linked area file. Validate the two parts together and permit surgical updates to the matching index row and area heading/checkboxes. Apply that contract to the other status writers as well. Trace one feature such as manual match statistics through an update and confirm both representations agree without changing unrelated rows.

## Minor

### 🟡 Detect installed test runners before declaring a missing setup, `.agents/skills/check/modes/review.md:97`

**Problem**: Test signal detection only recognizes a configured runner through `test-preferences.json`; its absence becomes `none-yet` at line 99. This repository already has Vitest in `frontend/package.json`, a `vitest.config.ts`, and two backend test projects with the .NET test SDK and xUnit. The supplied signal for this review was consequently `none-yet` despite an established test setup.

**Why it matters**: Every review before the first `/test` preference write receives an incorrect coverage classification and is instructed to report a nonexistent setup gap. It also misses the rubric's explicit configured-suite coverage checks.

**Suggested fix**: Inspect the relevant package manifests, test configuration, and applicable AGENTS.md commands before falling back to `none-yet`. Treat preferences as workflow configuration, not proof that a runner exists or is absent; resolve the signal per affected area.

### 🟡 Include staged changes in runtime verification scope, `.agents/skills/check/modes/verify.md:71`

**Problem**: Feature mode gathers the committed branch diff and plain `git diff --name-status`. The latter contains only unstaged changes. A staged-only change on `main`, or staged work in addition to committed branch work, is absent from both commands. Untracked implementation files are omitted too.

**Why it matters**: When no governing spec supplies the complete checklist, `/check verify` can select no behaviors or miss the behavior introduced by the staged files. This differs from `/check review` and `/test`, which explicitly include staged and untracked changes.

**Suggested fix**: Include the full working tree change set against HEAD plus untracked source files, and combine it with the branch change set. Confirm that a staged-only implementation change appears in the observable behavior inventory.

### 🟡 Align bundled conflict instructions with the project authority contract, `.agents/skills/develop/flow/build.md:28`

**Problem**: The build flow says a feature spec wins over AGENTS.md and reconciliation can follow later. The UI route also says implemented styling values win when they disagree with the design document (`.agents/skills/develop/ui/existing.md:13`). Root AGENTS.md and accepted spec 0001 instead require the owning current source to govern and disputed work to stop until the conflict is resolved. The repository already has explicitly recorded design and API boundary drift, so these are reachable cases.

**Why it matters**: The higher priority root rules do govern today, so this does not change application behavior by itself. It nevertheless leaves the installed workflow giving the opposite operational instruction precisely where the new authority model is intended to remove ambiguity.

**Suggested fix**: Adapt these conflict branches to defer explicitly to the repository's authority and conflict rules. A spec or implemented token value must not automatically resolve a disagreement; use the named owner workflow and keep the documented drift visible until the coordinated change is approved.

## Strengths

- All 73 archived paths retain identical Git entry modes and blob IDs from the recorded clean rollout source `cfeab02514a2f16b3a245f18c55c34e8552be38a`. The archive is clearly labelled and no root context directory remains.
- The authority model distinguishes scope, design, durable decisions, and implementation evidence, while preserving known drift and excluded code rather than silently claiming conformance.
- All 41 Claude skill links resolve to their shared SKILL.md files. A static scan of 246 current tracked Markdown files found no unresolved actual local links or heading anchors among 201 links checked; illustrative paths inside code were excluded.

## Test coverage

The supplied `none-yet` signal reflects a workflow detection gap, not an absence of product tests; the existing Vitest and xUnit configuration was inspected. This branch changes documentation and agent workflows, so additional product unit tests or an application build would not exercise the defects above and were not run. Validation covered the complete changed-path inventory, Git preservation of the archive, current local Markdown links, skill loader resolution, retired-path references, and `git diff --check`, which passed. Workflow completion and split-scope transitions were assessed by tracing their instructions and consuming artifacts; no workflow was executed in a mode that could mutate project authority or application files.

---

# Current state reassessment, project context authority, 2026-09-15

**Reviewed by**: gpt-6-astra (author on gpt-5.6-sol)
**Scope**: 202 files, chore/workflow-baseline vs main, merge base `a65251624a222997c37847ff6553cced0438c54d`, including current uncommitted and untracked work. The enumerated inventory contains 203 paths; `skills-lock.json` is excluded from the rubric count but inspected.
**Verdict**: Changes requested

## Summary

The current authority documents and archive rollout establish the intended ownership boundary. The uncommitted sync changes now require actual passing verification and test evidence, and support a coordinated index plus area scope input. One integration problem remains: the other workflow status writers still restrict themselves to a single scope file, so they can create a disagreement that the corrected sync workflow must refuse to reconcile. This assessment is independent of, and supersedes the conclusions of, the earlier report for the current working tree; the earlier text is preserved above.

## Major

### 🟠 Apply the split scope contract to the workflows that produce its status, `.agents/skills/develop/SKILL.md:79`

**Problem**: Develop selects only the one feature scope file, and its build flow explicitly restricts updates to that file while requiring both the At a glance table and heading to change (`.agents/skills/develop/flow/build.md:69`, `:98`). Architect likewise opens only the feature file (`.agents/skills/architect/internal/after-subagent.md:62`) and then requires a table and heading status update (`:72`). In this repository the table is in `docs/scope/index.md` and the feature headings are in separate area files. Only sync received the coordinated input contract.

**Why it matters**: Designing planned feature 13 or closing an active feature cannot follow these instructions without either exceeding the permitted file scope or leaving the index stale. Once one writer changes only the area heading, sync's new mismatch gate (`.agents/skills/sync/agent-prompt.md:156`) correctly rejects both halves. Sync therefore cannot repair the disagreement produced by the normal preceding workflow. The authoritative product status and next work selection become inconsistent.

**Suggested fix**: Resolve and authorize the index plus touched area file as one logical scope in every workflow that writes feature status, including architect and develop. Keep the restriction to the matching feature and update both status locations together. Trace a planned feature through design, build, verification, and sync using this repository's split files; verify that the index and heading remain equal and unrelated features stay untouched.

## Minor

### 🟡 Remove automatic conflict winners from the bundled build flow, `.agents/skills/develop/flow/build.md:28`

**Problem**: The instruction that a feature spec wins over AGENTS.md, with sync reconciliation afterward, conflicts with root AGENTS.md and accepted spec 0001. The existing UI path also makes implemented token values authoritative when they disagree with the design document (`.agents/skills/develop/ui/existing.md:13`). The current authorities explicitly retain both backend boundary drift and design drift pending a scoped decision.

**Why it matters**: Higher priority repository instructions still govern, so this is not an application regression. However, the workflow gives the opposite conflict resolution instruction at the exact boundaries this change is intended to clarify.

**Suggested fix**: Explicitly defer these branches to the repository's source ownership and conflict rules. Keep disputed work pending the named owner workflow, and require the coordinated authority update rather than selecting the spec or implementation automatically.

### 🟡 Include staged and untracked work in runtime verification scope, `.agents/skills/check/modes/verify.md:71`

**Problem**: The committed branch diff plus plain `git diff --name-status` excludes staged-only and untracked implementation changes. On main with a staged-only change, both commands can return an empty behavior inventory.

**Why it matters**: Verification derived from the diff can miss the behavior being reviewed when a governing specification does not independently enumerate it. This weakens the runtime evidence subsequently consumed by scope reconciliation.

**Suggested fix**: Collect the complete working tree diff against HEAD, including staged changes, and add untracked files. Combine that with the branch inventory and check a staged-only and an untracked implementation scenario.

## Strengths

- The archived source and destination have the identical Git tree ID `2ee402844a0749f9dd96900e5567347ed5e608d2`. All archived paths, entry types, and blob contents match the recorded rollout source. The apparent progress tracker difference against the branch base predates the rollout; it is not an archive preservation failure.
- Root and area instructions clearly separate current ownership from historical evidence, preserve the explicit design and backend boundary differences, and keep excluded training code from becoming active product scope.
- Sync now distinguishes a verification plan from execution and requires current passing evidence for both verification and tests. Its coordinated scope update and malformed input protections are explicit.

## Test coverage

The parent ran `node --test tests/sync-workflow.test.mjs`: all 12 tests passed, exit 0. It also reported a passing `git diff --check` with line ending warnings only. The tests assert instruction text; they protect the written sync contract but do not execute an agent reconciliation or cover the consuming architect and develop flows. The first two earlier findings are resolved within sync itself, while the cross workflow scope integration remains incomplete as described above. The new preferences file provides a configured test signal for this review; the earlier missing preferences diagnosis is not a current setup failure.

## Scope and limitations

This is a whole branch inventory review focused on project context authority and workflow integration. Governing root and area instructions, all current scope areas, DESIGN, the accepted authority decision, migration evidence, sync instructions and tests, and the relevant workflow consumers were inspected directly. Archive preservation was checked by exact Git tree equality rather than rereading 73 unchanged historical documents. Other bundled skill material was not exhaustively behavior exercised, and this report does not claim runtime validation of every installed skill or repeat the earlier full Markdown link scan. No application code, tests, scope status, or specification was changed by this review. There is no current scope feature named project context authority to close; canonical activation on main and preparation checkout cleanup remain merge bookkeeping, not a newly discovered archive defect.
