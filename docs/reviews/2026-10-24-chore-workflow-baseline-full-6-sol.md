# Review, chore/workflow-baseline, 2026-10-24

**Reviewed by**: GPT-6 Sol (author model unknown)
**Scope**: 220 files, branch vs main at `a65251624a222997c37847ff6553cced0438c54d`, including untracked source and test files; prior review artifacts excluded
**Verdict**: Changes requested

## Summary

The branch establishes current project authority, preserves the former context tree as legacy evidence, adds workflow skills, and tests their scope and decision handoffs. The migration manifest matches its stated source revision, and all 148 Node workflow tests pass. Three workflow issues can record completion or select a test runner incorrectly during future feature work.

## Major

### 🟠 `/test` can close a GA feature before its remaining gates, `.agents/skills/test/SKILL.md:187`
**Problem**: After passing tests, `/test` offers to set the feature to `done` and its linked spec to `Accepted` at both Beta and GA. A GA feature still has `Review it` and `Document it` boxes, and the instruction does not require those boxes to be completed or explicitly recorded as skipped before changing status. The scope template recommends this same early completion point, while `/sync` requires all present boxes to be complete or explicitly skipped.
**Why it matters**: The index and feature section can show `done`, and the spec can become `Accepted`, while the current project GA workflow still has unfinished review and documentation. Later agents may treat that state as verified completion.
**Suggested fix**: At the `/test` handoff, leave GA work `in-progress` while later boxes remain open. If the engineer elects to finish early, record each skipped stage explicitly before setting `done`, and make `/sync` apply the same rule. Add a regression case for a GA feature after tests pass with review and documentation still open.

### 🟠 A blocked review can satisfy the completion gate, `.agents/skills/sync/agent-prompt.md:165`
**Problem**: `/check review` ticks `Review it` whenever a findings file exists, including `Changes requested` and `Blocked` verdicts (`.agents/skills/check/modes/review.md:137`). `/sync` also treats the mere findings file as completion evidence, then sets `done` when all present boxes are ticked, without checking the verdict or that blockers and majors were resolved against the current code.
**Why it matters**: A feature can be marked done after a review explicitly says it should not merge. The stale review artifact continues to satisfy the box after subsequent edits unless a new review is required.
**Suggested fix**: Keep the review run recorded, but distinguish a completed acceptable review from a review requiring changes. Require a current approving verdict or a specifically recorded user override before `/sync` treats `Review it` as satisfied for feature completion; rerun review after fixes. Test blocked and stale review scenarios.

### 🟠 Root test preferences select the wrong runner for application changes, `test-preferences.json:2`
**Problem**: The new root preferences set `tool` to `node:test`, `testDir` to `tests`, and `filePattern` to `*.test.mjs`. `/test` loads this root file as the configured runner and skips framework setup (`.agents/skills/test/SKILL.md:93-95`). The application has separate `frontend/` Vitest and `backend/` xUnit suites, with their commands documented in the respective `AGENTS.md` files. The repository root has no `package.json` for the npm command that the skill derives.
**Why it matters**: Running `/test` on a React or C# change will use the workflow test preference instead of the relevant application runner, so it can generate unusable tests, miss the real suite, or fail to derive a runnable command.
**Suggested fix**: Scope preferences by project or package root, and make the workflow Node tests use their own configuration. Resolve the effective runner for each changed area before Step 2 loads preferences. Exercise `/test` resolution for a frontend and a backend file.

## Strengths

- The 73 archived files match the migration record's source revision, including the progress tracker; the recorded destination hash matches the current file.
- The new scope handoff tests cover single and split scope files, concurrent edits, status preservation, and decision only specifications. `node --test tests/*.test.mjs` passed all 148 tests.

## Test coverage

The configured Node suite is a reference interpreter for written workflow instructions, not an agent executor. It covers the new scope preparation and split scope handoffs well. It does not cover the `/test` runner resolution or the effect of a blocked or stale review on scope completion, the paths behind the findings above.
