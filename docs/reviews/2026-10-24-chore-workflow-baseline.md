# Review, chore/workflow-baseline, 2026-10-24

**Reviewed by**: GPT-6 Astra (author on GPT-6.1 Sol, confirmed by the user)
**Scope**: 213 files, chore/workflow-baseline vs main, including working tree changes and 13 untracked files; merge base `a65251624a222997c37847ff6553cced0438c54d`
**Verdict**: Changes requested

## Summary

The archive rollout preserves the recorded source exactly and separates current product scope, design, decisions, and implementation evidence. The follow-up correctly makes architect a scope reader and gives develop a coordinated split scope write contract, but the receiving scope workflow still assigns the transferred work to architect. The current tests pass, although their reference interpreter supplies that missing handoff and changes sync's execution order, so they do not establish that the installed workflows complete the same transitions.

## Major

### 🟠 Complete the receiving workflow for the architect handoff, `.agents/skills/scope/SKILL.md:19`

**Problem**: Architect now forbids every scope write and directs the user to `/scope <feature>` to link the confirmed spec, clear the decision marker, tick its decision box, and prepare milestones and closing boxes (`architect/internal/after-subagent.md:62`). Scope still explicitly assigns that work to architect here and in `scope/scope-template.md:102`. The selected add mode only extends/enrolls a feature with one entry checkbox; neither add nor replan defines consuming a confirmed-spec handoff. For current feature 13, following architect leaves the original decision-only section intact, and following scope's own lifecycle instructions sends preparation back to an actor that now cannot perform it.

**Why it matters**: The next planned feature cannot reliably obtain its active spec link and build/verification checklist through the documented pipeline. This also means the current passing transition tests do not cover the integration being repaired: `tests/helpers/scope-writer-harness.mjs:94` implements `prepareScopeOwner` directly, never loads the scope skill, and fixtures already contain the execution boxes absent from the real planned feature.

**Suggested fix**: Give scope an explicit confirmed-spec preparation path for an existing or newly enrolled feature, remove the stale architect ownership statements from its skill/template, preserve lifecycle status on design confirmation, and handle both index and area files together. Validate the transition starting with the real decision-only shape, including the effective tier's closing boxes, rather than preinstalling that shape in the fixture helper.

## Minor

### 🟡 Mirror specification status after scope reconciliation, `.agents/skills/sync/agent-prompt.md:126`

**Problem**: Numbered Step 4 derives and writes spec status from the current scope; scope validation and completion updates occur later in Step 6. Starting with both scope locations `in-progress`, an `In Progress` spec, and newly available passing evidence, one literal pass leaves the spec unchanged in Step 4 and then sets scope to `done` in Step 6. A second identical run finally accepts the spec. The phrase “reconciled scope” in Step 4 does not specify running the later step first. The new test interpreter does run scope reconciliation before mirroring the spec, so its immediate acceptance and second-run idempotence assertions cannot expose this ordering defect.

**Why it matters**: One sync can return contradictory lifecycle state and needs a second run to finish. Its early status write also precedes the new split scope validation gate, leaving the intended no-write behavior for malformed input unclear.

**Suggested fix**: Explicitly validate and reconcile the relevant scope first, then mirror only the resulting safe feature states into linked specs. Preserve `Assumed` and standalone decisions. Make the tests check the documented orchestration order as well as the modeled end state.

### 🟡 Remove automatic conflict winners from the build instructions, `.agents/skills/develop/flow/build.md:28`

**Problem**: The build flow says a feature spec wins over AGENTS.md and sync should reconcile later. The UI path similarly selects implemented styling values over conflicting design values (`develop/ui/existing.md:13`). Root AGENTS.md and accepted spec 0001 instead assign authority by concern and require the disputed boundary to be resolved through its named owner. The repository explicitly records both design and backend boundary drift, making these reachable conflicts.

**Why it matters**: Higher priority repository guidance still governs, but the installed workflow gives the opposite operational instruction where this rollout is intended to remove ambiguity.

**Suggested fix**: Defer these branches to the repository's authority and conflict rules. Keep drift visible and require the coordinated owner update before changing the disputed implementation.

### 🟡 Include staged and untracked work in runtime verification scope, `.agents/skills/check/modes/verify.md:71`

**Problem**: Feature mode combines the committed branch diff with plain `git diff --name-status`. Neither includes staged-only changes, and neither lists untracked implementation files. On main with a staged-only implementation change, both inventories can be empty.

**Why it matters**: When verification derives behavior from the diff instead of a complete governing spec, it can omit the behavior actually being reviewed. The completion evidence subsequently consumed by sync is then incomplete.

**Suggested fix**: Include working tree changes against HEAD and untracked implementation paths, combined with the committed branch inventory. Check both a staged-only case and a new untracked implementation file.

## Strengths

- Source `cfeab02514a2f16b3a245f18c55c34e8552be38a:context` and destination `HEAD:docs/legacy/context` have identical Git tree ID `2ee402844a0749f9dd96900e5567347ed5e608d2`, proving preservation of all archived names, modes, and blobs. No archive working tree changes were present.
- Current authorities retain the known design and API boundary differences and excluded training direction without silently treating implementation as product authorization.
- Develop's new paired scope validation and sync's requirement for current passing verification/test evidence directly address the prior one-sided updates and premature completion problems.

## Test coverage

Ran `node --test tests/scope-writers-workflow.test.mjs tests/sync-workflow.test.mjs`: 58 passed, 0 failed. `git diff --check main` passed, with line-ending warnings only. The tests protect instruction phrases and a test-only deterministic interpreter; they do not execute the skills through an agent. The scope owner handoff is manually supplied, and sync ordering differs from the numbered instructions as described above. Product builds and xUnit/Vitest suites were not run because no application implementation changed.

## Scope and limitations

The full branch and untracked path inventories were collected, with focused direct inspection of current authority documents, the accepted rollout contract, migration record, scope areas, workflow consumers, current follow-up diff, and regression tests. Archive content was checked by Git tree equality rather than rereading unchanged historical documents. This is not a claim of runtime execution of every bundled workflow or a new exhaustive Markdown link audit; the complete branch diff exceeds a single tool output and was examined through targeted reads and searches.

No current scope feature matches the standalone project context authority decision. Spec 0001 explicitly permits that standalone decision lifecycle, so there is no feature status to close and no missing-scope finding. Canonical activation on main and preparation checkout cleanup remain merge bookkeeping. Only this findings file was written.
