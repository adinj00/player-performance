# Review, chore/workflow-baseline, 2026-10-24

**Reviewed by**: GPT-6 Astra (author identified by the user as GPT/Codex; prior report records GPT-6.1 Sol)
**Scope**: 217 files, chore/workflow-baseline vs main, including working tree changes and 17 untracked files; merge base `a65251624a222997c37847ff6553cced0438c54d`
**Verdict**: Changes requested

## Summary

The archive preservation and project authority boundary remain sound. The new scope preparation route fixes the earlier missing receiving handoff for ordinary feature specs, including the actual planned feature 13 shape. One supported decision-only handoff now loops back to architect instead of reaching develop; three previously reported minor issues also remain.

## Major

### 🟠 Preserve the decision-only specification handoff, `.agents/skills/scope/modes/prepare.md:11`

**Problem**: Preparation refuses a decision-only spec without an implementation plan and sends it back to architect. Architect explicitly requires Architecture and Cross-cutting decision specs to omit `## Build plan`, even instructing the author to strip scaffold plans (`architect/internal/after-subagent.md:15` and `architect/internal/design-conversation.md:122`). Develop explicitly derives scaffold steps from `## Proposed stack` (`develop/SKILL.md:40`). Thus a confirmed Stack and architecture scope feature cannot pass the new mandatory preparation route: architect hands linking to scope, scope demands the section architect forbids, and the whole preparation is withheld. This is a supported shape in `scope/scope-template.md:37`, not an incomplete feature design.

**Why it matters**: The new scope owner cannot link and clear the decision for a valid foundation/scaffold contract, leaving the workflow unable to reach the builder without manual bypass or violating architect's format contract. The ordinary feature-13 tests do not cover this distinction; they treat every missing build plan as invalid.

**Suggested fix**: Restore an explicit decision-only preparation branch: link the confirmed decision, resolve only its decision marker/box, preserve its scaffold or execution command, and let develop derive the permitted implementation steps. Continue rejecting genuinely incomplete feature build contracts. Add a decision-only fixture that follows architect confirmation through scope preparation to the scaffold handoff without inventing a Build plan.

## Minor

### 🟡 Mirror specification status after scope reconciliation, `.agents/skills/sync/agent-prompt.md:126`

**Problem**: Numbered Step 4 derives and writes the spec status before Step 6 validates and reconciles scope. Starting with an `In Progress` spec and an `in-progress` scope whose closing evidence has just passed, the first run leaves the spec unchanged, then marks scope `done`; a second run is needed to accept the spec. The early write also precedes the malformed split-scope gate. The test helper instead validates scope first and mirrors its resulting status (`tests/helpers/scope-writer-harness.mjs:168` and `:192`), so the passing tests do not cover the installed order.

**Why it matters**: A single sync leaves contradictory lifecycle state, and its documented no-write boundary for malformed input does not clearly include the already attempted spec write. This is the still-open ordering finding from the preceding review.

**Suggested fix**: Explicitly validate and reconcile scope before mirroring the resulting safe states to specs. Preserve standalone and Assumed decisions. Add an orchestration-order check alongside modeled end-state assertions.

### 🟡 Remove automatic conflict winners from build instructions, `.agents/skills/develop/flow/build.md:28`

**Problem**: The build flow says a feature spec wins over AGENTS.md and sync should reconcile later. The existing UI path similarly gives implemented styling values precedence over conflicting design values (`develop/ui/existing.md:13`). Root AGENTS.md and accepted spec 0001 instead assign authority by concern and stop the disputed boundary until the owning workflow resolves it. The repository already records both design and backend boundary drift.

**Why it matters**: Higher-priority repository guidance still governs, but these installed instructions supply contrary operational rules precisely where the authority rollout aims to remove ambiguity. This remains unresolved from the preceding review.

**Suggested fix**: Defer to the repository's authority and conflict rules, keep drift visible, and require the coordinated owner update before implementing the disputed change.

### 🟡 Include staged and untracked implementation in verification scope, `.agents/skills/check/modes/verify.md:71`

**Problem**: The inventory combines the committed branch diff with plain `git diff --name-status`. Neither includes staged-only changes or untracked implementation files. A staged-only change on main can therefore produce an empty inventory.

**Why it matters**: Verification that derives behaviors from the diff can omit the actual implementation being reviewed, leaving incomplete evidence for subsequent reconciliation. This remains unresolved from the preceding review.

**Suggested fix**: Combine the committed branch inventory with changes against HEAD and untracked implementation paths. Cover staged-only and newly created implementation cases.

## Strengths

- Source `cfeab02514a2f16b3a245f18c55c34e8552be38a:context` and destination `HEAD:docs/legacy/context` have identical tree ID `2ee402844a0749f9dd96900e5567347ed5e608d2`. All archived names, modes and blobs are preserved, with no archive working tree changes.
- The new preparation route starts from the real decision-only feature-13 section, links the confirmed feature spec, adds tier-specific unchecked execution steps, and preserves lifecycle status and unrelated content. Architect remains a scope reader.
- Current authority documents retain design/API drift and excluded product direction explicitly; preparation and reconciliation distinguish confirmation, execution, passing evidence and authorized skips.

## Test coverage

Ran `node --test tests/scope-preparation.test.mjs tests/scope-writers-workflow.test.mjs tests/sync-workflow.test.mjs`: **108 passed, 0 failed**. `git diff --check a65251624a222997c37847ff6553cced0438c54d` passed with line-ending warnings only. No application implementation changed; product builds and xUnit/Vitest suites were not run.

The suite checks installed instruction clauses and a deterministic test-only reference model. It covers the ordinary preparation path, tiers, repeated execution, concurrency, malformed scope, and evidence refusal, but does not execute the skills through an agent. The decision-only spec exception lacks coverage, and the sync model still runs a different order from the numbered instructions. These limitations prevent treating the passing suite as proof that every documented handoff succeeds.

## Scope and limitations

Collected the full branch and untracked inventories and inspected current authority documents, the accepted rollout specification and migration record, scope areas, downstream workflow consumers, follow-up edits, and regression tests. Archive preservation was verified by Git tree equality rather than rereading historical documents. This was focused review of project authority and workflow handoffs, not runtime execution of every bundled skill or a new exhaustive Markdown link audit.

No scope feature matches standalone project context authority; spec 0001 permits that standalone lifecycle, so no feature status needs closing. The earlier receiving-handoff Major is resolved for ordinary feature specs. Both historical review files are preserved. Only this findings file was written.
