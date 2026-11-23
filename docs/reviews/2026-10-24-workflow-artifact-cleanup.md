# Abandoned workflow artifact cleanup

Date: 2026-10-24. Branch: `chore/workflow-baseline`.

## Boundary and preservation

This cleanup removes project-owned artifacts for abandoned local changes to the
jsmastery workflow. It does not repair or customize the restored upstream skills.

Before removal, all 19 files below were copied outside the repository to
`C:\Users\ThinkPad\AppData\Local\Temp\player-performance-workflow-cleanup-0o404nra\artifacts`.
Every backup was verified against the source SHA-256. The parent backup directory
contains `before.json` (hashes and symlink targets for all initially present,
nonignored tracked/untracked files), `removed-files.json`, and
`initial-status.txt`. This is a local external backup in Temp; retain or relocate
it before routine temporary-directory cleanup.

All removed files were untracked at task start. Their removal therefore does not
appear as tracked deletions in `git diff`. The pre-existing vendor restoration,
`skills-lock.json` changes, and six untracked historical review reports were
preserved exactly as found. No application code, existing product test, project
guidance, live scope, accepted specification, or Claude symlink was edited.

## Exact cleanup inventory

Removed after external backup:

| Path | Reason |
|---|---|
| `test-preferences.json` | Root-wide `node:test`, `tests`, and `*.test.mjs` preferences described this custom workflow suite, not frontend Vitest or backend xUnit. |
| `tests/architect-decision-fixtures.test.mjs` | Hypothetical decision examples and modeled architect confirmation using the abandoned read-only scope contract. |
| `tests/decision-scope-preparation.test.mjs` | Instruction probes and decision handoff traces requiring the removed `scope/modes/prepare.md` contract. |
| `tests/scope-preparation.test.mjs` | Confirmed-spec preparation, tier, and preservation tests for the abandoned preparation route. |
| `tests/scope-writers-workflow.test.mjs` | Local architect/develop/sync writer contract probes and modeled split-scope transitions. |
| `tests/sync-workflow.test.mjs` | Local split-scope, execution-proof, and completion guards absent from official upstream. |
| `tests/helpers/scope-writer-harness.mjs` | Test-only interpreter of the abandoned contracts; it does not execute an agent. |
| `tests/confirmed-spec-handoff.md` | Handoff for the abandoned confirmed-spec preparation repair. |
| `tests/decision-scope-handoff.md` | Handoff for the abandoned decision preparation repair. |
| `tests/scope-writers-handoff.md` | Handoff documenting intentional local divergence from upstream. |
| `tests/fixtures/scope-writers/README.md` | Instructions and limitations for the obsolete fixture suite. |
| `tests/fixtures/scope-writers/decisions/architecture.md` | Hypothetical decision fixture, not an accepted project decision. |
| `tests/fixtures/scope-writers/decisions/cross-cutting.md` | Hypothetical decision fixture, not an accepted project decision. |
| `tests/fixtures/scope-writers/single/docs/scope/product.md` | Disposable single-file scope fixture. |
| `tests/fixtures/scope-writers/single/docs/specs/0013-player-trends.md` | Miniature test specification. |
| `tests/fixtures/scope-writers/split/docs/scope/index.md` | Disposable split-scope index fixture. |
| `tests/fixtures/scope-writers/split/docs/scope/other.md` | Unrelated-content preservation fixture. |
| `tests/fixtures/scope-writers/split/docs/scope/player-analysis.md` | Disposable feature fixture copied from product scope. |
| `tests/fixtures/scope-writers/split/docs/specs/0013-player-trends.md` | Miniature test specification. |

Added: `docs/reviews/2026-10-24-workflow-artifact-cleanup.md` (this report).
No other original repository file was modified by this cleanup.

The five suites exclusively cover workflow instruction text or a reference model
of the local modifications. Even tests that only validate fixture completeness
belong to that obsolete harness. None imports application behavior. Keeping them
in active discovery would assert abandoned contracts against immutable upstream.
All supporting assets were removed with them to avoid leaving stale run commands
and handoff instructions. Existing historical review reports remain intact.

## Spec/workflow drift remaining

These are compatibility findings, not changes to accepted decisions. The
repository's authority rules govern over skill instructions.

1. **Split-scope writes remain unsafe without project-aware interpretation.**
   `docs/scope/index.md` owns the summary table; its five linked areas own feature
   headings and checklists. Upstream `architect/internal/after-subagent.md:62`
   opens only the single matching scope file, then at line 72 instructs updating
   both the table and heading. `develop/SKILL.md` similarly selects one scope
   file; `sync/agent-prompt.md:156` expects a table and feature sections together.
   An area alone can be rejected as malformed or updated without its index.
   Removing the local tests does not resolve this upstream limitation.
2. **Scope ownership differs.** Root `AGENTS.md` and accepted
   `docs/specs/0001-project-context-authority.md` direct scope changes through
   `/scope`. Upstream architect completion directly links specs, checks decision
   boxes, advances scope status, and can enroll follow-ups. Its scope template
   explicitly assigns designed-state preparation to architect. The abandoned
   `/scope` confirmed-spec receiving route no longer exists. Future runs must
   obey project ownership and surface a disputed handoff rather than assume the
   removed preparation mode is installed.
3. **Completion evidence is weaker in upstream sync.** Current scope explicitly
   says code presence alone is not completion proof and uses GA verification.
   Upstream `sync/agent-prompt.md:160-165` permits a saved `verify.md` or test
   files as checkbox evidence. Its spec status reconciliation (step 4) also
   precedes scope reconciliation (step 6), so a scope transition can leave a
   linked spec status behind until a later pass. Require actual relevant run
   evidence under current project guidance; no accepted lifecycle was rewritten.
4. **Skill authority wording conflicts with project precedence.** Upstream
   `architect/SKILL.md:171` calls consulted community-skill conventions
   authoritative. Root `AGENTS.md` and accepted specification 0001 explicitly
   subordinate skills to project authorities. Consultation cannot override scope,
   design, or accepted decisions.
5. **Test setup is area-specific.** Removing the global preference preserves
   the actual frontend `vitest run` configuration and backend xUnit projects.
   A future `/test` setup must use those area commands rather than recreate one
   root `node:test` preference. No replacement global preference was invented.

The standalone accepted authority specification is supported by upstream's
standalone-decision exception; it does not need conversion into a feature spec.
Existing frontend design drift and backend persistence-boundary drift remain
recorded in their area `AGENTS.md` files and are outside this cleanup.

## Validation

- Backend: `dotnet test PlayerPerformance.sln --no-restore` from `backend/`;
  **112 unit tests and 49 integration tests passed**, zero failures or skips.
  Detection found .NET SDK 10.0.303, net8.0 targets, xUnit 2, and VSTest through
  `Microsoft.NET.Test.Sdk`; no global runner or central props override was found.
- Vendor integrity: compared the exact file sets and every file's bytes in all
  nine installed jsmastery directories against Git blobs from upstream
  `43b69e44c9ca905fe3a3418ccdf4102255e20d40`: **71 files matched**, no extras or
  missing files. This checks Git object content, not a potentially modified
  upstream working tree. No vendor file was written.
- Claude symlinks: **41 valid directory symlinks**, each resolving to its
  corresponding `.agents/skills/<name>` directory; all targets remain unchanged.
- Upstream portability: official `scripts/check-portability.mjs` passed in an
  external disposable copy of the verified upstream Git content. Only the copied
  validator's Windows URL/path handling was adapted; copied text used LF.
  Six upstream budget warnings remain. Since the installed nine directories are
  byte-identical to upstream, this validates their upstream content without
  editing the installed skills. Log: external backup `portability-check.txt`.
- `git diff --check`: passed; pre-existing LF-to-CRLF warnings remain.
- Before/after SHA-256 and symlink comparison: only the 19 listed original files
  were removed; every other initial file, including all historical reviews,
  vendor assets, lockfile, application sources/tests, scope, and specifications,
  retained its original content or link target.
- Frontend: `npm.cmd run test` from `frontend/`; **24 tests passed in five
  files**, zero failures. PowerShell blocked the `npm.ps1` shim, so the Windows
  command shim was used. Dependencies were initially missing; an offline restore
  lacked a cached package and the sandboxed registry request timed out. A restore
  with network access using `npm.cmd ci --ignore-scripts --no-audit --no-fund
  --fetch-retries=0 --fetch-timeout=15000` succeeded before the passing run.
  Dependency installation affected ignored `frontend/node_modules` only; the
  package manifest, lockfile, Vitest config, and product tests remain unchanged.

No commit, push, reset, or PR was performed.
