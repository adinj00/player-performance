# Review, chore/workflow-baseline, 2026-10-24

**Reviewed by**: GPT-6 Sol (author on GPT-6.1 Sol)
**Scope**: 223 files, chore/workflow-baseline vs main, including 200 tracked changes and 23 untracked files; merge base `a65251624a222997c37847ff6553cced0438c54d`
**Verdict**: Changes requested

## Summary

The branch establishes the new project authority documents, preserves the legacy context tree, and adds workflow handoffs for architect, scope, develop, and sync. The decision-only preparation instructions now handle the prior blocking case, but the test model uses specs that architect's own checks would reject, leaving that central handoff untested for valid input. Three smaller instruction conflicts remain.

## Major

### 🟠 Exercise the decision handoff with a valid architect spec, `tests/helpers/scope-writer-harness.mjs:112`

**Problem**: The new reference interpreter classifies a spec as decision only only when it has `## Proposed stack` or `## Standard definition` and lacks `## Requirements`. Architect requires `## Requirements` for all modes (`architect/internal/after-subagent.md:10`) and expressly forbids `## Build plan` for Architecture and Cross cutting decisions (`:14`). A compliant architecture spec therefore enters the model's feature branch and is rejected for a missing build plan at line 116. Both decision fixtures omit Requirements, so all the new passing handoff cases exercise a spec architect would mark incomplete.

**Why it matters**: The 138 passing tests do not protect the exact architect-to-scope decision-only route this follow-up repairs. A later change could break preparation of valid foundation or cross cutting decisions without failing these tests.

**Suggested fix**: Make both fixtures satisfy architect's required common sections and criteria while omitting Build plan. Classify decision contracts by their actual kind or decision section, not by absence of Requirements, and trace a valid spec through confirmation and preparation.

## Minor

### 🟡 Reconcile scope before mirroring linked spec status, `.agents/skills/sync/agent-prompt.md:126`

**Problem**: Numbered Step 4 writes a spec status from the current scope, then Step 6 validates and advances scope. With fresh passing evidence, one run can leave scope `done` and the linked spec `In Progress`; a second run is needed. The reference interpreter instead computes the new scope state before writing the spec, so its tests do not exercise the documented order.

**Why it matters**: One sync can leave contradictory lifecycle records. Its spec write also precedes the malformed split-scope validation gate.

**Suggested fix**: Validate and reconcile the scope pair first, then mirror the resulting safe lifecycle state to the linked spec. Test the documented orchestration order.

### 🟡 Follow the repository conflict owner in the build flow, `.agents/skills/develop/flow/build.md:28`

**Problem**: The build flow gives a feature spec automatic precedence over conflicting `AGENTS.md` and sends reconciliation to sync afterward. The UI path similarly favors implemented token values over conflicting `design.md` values (`develop/ui/existing.md:13`). Root `AGENTS.md` and accepted spec 0001 instead require work at a disputed boundary to stop until its owning source resolves the conflict.

**Why it matters**: The skill gives an operational instruction contrary to the authority model this branch establishes.

**Suggested fix**: Refer to the root authority and conflict rules, and route the disputed boundary to its named owner before implementing it.

### 🟡 Include staged and untracked changes in verification scope, `.agents/skills/check/modes/verify.md:71`

**Problem**: Feature mode inventories a committed branch diff and plain `git diff --name-status`. Neither includes staged-only changes or untracked implementation. On main with a staged-only change, the observable behavior inventory can be empty.

**Why it matters**: Verification may miss behavior that was just implemented, leaving incomplete evidence for scope reconciliation.

**Suggested fix**: Combine the committed branch inventory with changes against HEAD and untracked implementation paths. Cover staged-only and new-file cases.

## Strengths

- The archived source tree and `HEAD:docs/legacy/context` have the same Git tree ID, `2ee402844a0749f9dd96900e5567347ed5e608d2`. The migration record also provides a per-file preservation manifest.
- The scope preparation instructions now distinguish feature build contracts from decision-only contracts, preserve status and existing execution evidence, and require coordinated validation of split scope files.

## Test coverage

Ran `node --test tests/*.test.mjs`: 138 passed, 0 failed. `git diff --check a65251624a222997c37847ff6553cced0438c54d` passed, with line-ending warnings only. These are instruction guards and a test-only reference interpreter, not execution of the installed skills through an agent. The decision fixtures' invalid shape and the sync model's different order limit what their green result proves. No application source changed, so product builds and application suites were outside this review.

## Scope and limitations

Reviewed the full branch inventory and untracked paths, current authority and scope documents, the accepted rollout decision, workflow handoffs, earlier findings, and regression tests. Archive content was checked through Git tree equality rather than rereading historical documents. No current scope feature matches this standalone authority rollout, so no scope box was ticked. Only this findings file was written.
