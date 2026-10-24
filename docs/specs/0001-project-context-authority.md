# 0001. Establish project context authority

**Date**: 2026-09-14
**Status**: Accepted

## Summary

The repository will use `AGENTS.md`, `docs/scope/`, `DESIGN.md`, and accepted files in `docs/specs/` as its active guidance. The complete old `context/` tree will move to `docs/legacy/context/` as preserved history, with no file promoted into the new specification system. Ratification approves this target. One later verified documentation commit activates it in each Git checkout that adopts the commit, and a complete revert restores the previous authority model if that rollout fails.

## Context

This brownfield repository now has root and area `AGENTS.md` files, current product scope under `docs/scope/`, a root `DESIGN.md`, and a place for durable decisions under `docs/specs/`. The older `context/` tree still describes a different workflow and sometimes a different product or design direction. Root `AGENTS.md` currently tells agents to read that tree as the primary source, so the new and old systems are both active in practice.

The conflict is concrete. `docs/scope/` excludes training and vendor integrations from the active direction, while older context includes them in V1. `DESIGN.md` defines a dark stadium night direction with Oswald and Fira Sans, while older UI guidance and the current interface use a light theme with Manrope and JetBrains Mono. The repository also records code that does not follow every current area convention. None of these conflicts should be settled by whichever file an agent happens to read first.

This decision changes documentation authority only. It does not change application code, remove excluded application code, or claim that current code already conforms to the current design and architecture guidance. In any Git checkout, the present root `AGENTS.md` reading list and root `context/` tree remain active until that checkout advances to the verified rollout commit. This activation boundary prevents the specification from claiming authority over the very rule that still places it below the current context files.

## Options considered

### Option 1: Move the complete old context tree to one legacy location

Move `context/` as one preserved tree to `docs/legacy/context/`, add one archive notice, and update current links that intentionally point to historical evidence.

Pros:

1. The directory boundary is obvious and difficult to misread.
2. History, old specifications, audits, and runbooks remain available together.
3. No file by file judgment can accidentally leave part of the old workflow active.

Cons:

1. Links into `context/` must be updated in the same change.
2. A useful old document must be deliberately promoted through the current workflow before it can guide new work.

### Option 2: Move only files that currently contradict the new baseline

Retire the six root context files and old feature specifications, while leaving operations and reference material under `context/`.

Pros:

1. Fewer paths move.
2. Some useful references remain at their present locations.

Cons:

1. The meaning of `context/` stays mixed.
2. Future agents must decide which documents are active without a reliable directory boundary.
3. Later drift can turn a harmless reference into active contradictory guidance.

### Option 3: Leave every file in place and add warning text

Keep the old layout and add legacy banners plus new precedence rules.

Pros:

1. Existing links do not move.
2. Git history remains easy to follow at the original paths.

Cons:

1. Agents still discover retired instructions beside active work.
2. Every old file needs a durable warning, which creates repeated maintenance.
3. Search results continue mixing current and superseded guidance.

## Decision

**Chosen option**: Option 1: Move the complete old context tree to one legacy location

The repository will use one active documentation system and one clearly marked historical archive. The entire 73 file `context/` tree that exists on 2026-09-14 will move without content edits to `docs/legacy/context/`. There are no active exceptions inside that tree.

This decision is ratified when this standalone specification becomes `Accepted`. Ratification approves the target state. It does not activate that state by itself, and the rollout must not start while this specification is `Proposed`.

The new authority order takes effect separately in each Git checkout. It applies only when that checkout's `HEAD` includes the verified rollout commit, its working tree reflects the complete rollout, and no later full revert has restored the old model. The canonical project state activates when the rollout commit is merged into `main`. Other checkouts remain under the old model until they advance to that state. The rollout commit must satisfy all of these conditions:

1. This specification has status `Accepted` before rollout work starts.
2. Root `AGENTS.md` names the new authority order and no longer requires the old context reading list.
3. The complete root `context/` tree has moved to `docs/legacy/context/`.
4. `docs/legacy/context-migration-record.md` records the source revision, the exact preservation manifest, the legacy rule disposition, and every required check result.
5. Current links have been updated and every rollout check in this specification passes.

Before a checkout adopts that state, the present root `AGENTS.md` and `context/` files remain the active repository guidance in that checkout. There is no mixed transition state in which this specification overrides only selected old rules. An incomplete working tree is preparation only and must never be treated as the active authority model.

## Rationale

The directory boundary is the smallest reliable control. Selective moves require every future reader to understand which old files are safe, and banners repeat the same warning across many files. One coordinated move preserves all history while making accidental use less likely.

This choice also avoids laundering old feature specifications into `docs/specs/`. Those files record earlier plans and implementation evidence. They were not created or ratified under the current specification lifecycle, so they must not receive `Accepted` status by relocation.

## Standard definition

**Canonical pattern**:

After the activation conditions above pass, use this project source order for every task.

1. Follow the platform instructions and the current user request.
2. Read every applicable `AGENTS.md` from the repository root toward the file being changed. These files own workflow, repository boundaries, local conventions, and commands. A nearer file may add area detail but may not silently contradict an ancestor file.
3. Read `docs/scope/index.md` and the relevant scope area. `docs/scope/` owns current product boundaries, priority, and feature status.
4. For interface work, read root `DESIGN.md`. It owns visual and interaction direction, but its examples do not add product features that `docs/scope/` excludes.
5. Read relevant specifications in `docs/specs/`. An `Accepted` specification owns a durable project decision within the current scope and design direction. A feature specification in `Proposed` or `In Progress` status is a build contract only when current scope explicitly links to it. A standalone `Proposed` specification is a draft and is not actionable. No proposed specification becomes durable precedent.
6. Inspect application code, tests, migrations, configuration, and runtime behavior as evidence of what exists. Repository evidence does not create product scope or silently overrule an active decision.
7. Treat root and area `CLAUDE.md` files as loader shims to their matching `AGENTS.md` files. Treat skills as workflow aids, not project decisions.
8. Use README files and other current references for orientation only when they do not conflict with an authority above.
9. Treat everything under `docs/legacy/` as historical evidence only. Do not read it as active guidance and do not use it to break a conflict between current sources.

The authority sources own different concerns. The source that owns the concern governs that concern. `AGENTS.md` owns workflow, repository boundaries, local conventions, and commands. `docs/scope/` owns product boundaries, priority, and feature status. `DESIGN.md` owns visual and interaction direction. Accepted specifications own durable technical and product decisions inside those boundaries.

A specification never silently overrides another current owner. A later `Accepted` specification that intentionally changes scope, design, or repository guidance must update `docs/scope/`, `DESIGN.md`, or the applicable `AGENTS.md` in the same coordinated change. Until that synchronization occurs, the conflict is unresolved and implementation must stop only at the disputed boundary.

Apply these conflict rules.

1. Platform instructions and the current user request take precedence over repository documents.
2. Applicable `AGENTS.md` files are cumulative. If an ancestor and descendant file conflict, stop only the disputed work. Use `/sync` to correct current repository guidance after the underlying decision is clear. Use `/audit` only when repository context is missing and must be rebuilt. Do not choose by proximity alone.
3. Use `/scope` to change `docs/scope/`. Use `/architect` to change decisions in `docs/specs/`. Resolve a conflict involving `DESIGN.md` through `/architect`, then update `DESIGN.md` in the same approved change that applies the decision. Specification acceptance alone never changes another owner.
4. Within one decision area, supersession requires both links. The older specification uses `**Status**: Superseded by [NNNN](relative-path)`. The replacement names the older specification in its `**Replaces**` field or section. A newer date or larger number alone never supersedes an `Accepted` decision.
5. Supersession becomes durable when the replacement is `Accepted`. Before then, a scope linked replacement may govern only its named active feature as its build contract. The older accepted decision remains durable everywhere else. If the replacement is abandoned, remove the provisional supersession links and keep the older decision accepted.
6. If two current `Accepted` specifications conflict without an explicit relationship, stop only the disputed work and use `/architect` to resolve and record the relationship.
7. If code differs from a current authority, keep the difference visible in the current source that owns the work. Use the nearest applicable `AGENTS.md` for a local repository warning, `docs/scope/` for product status, or a relevant specification for decision follow up. Leave both the documented target and code unchanged until scoped work resolves the difference. Code presence does not invalidate the target or authorize adjacent cleanup.
8. If one current authority contradicts itself, stop only the disputed work and use the named workflow for that source to clarify it. Do not choose a value by convenience.

Apply specification status this way.

1. A standalone `Proposed` specification is a draft and is not authoritative until ratified as `Accepted`.
2. A scope linked `Proposed` or `In Progress` specification is the build contract only for its named active feature.
3. An `Accepted` specification is durable authority. For a standalone decision, `Accepted` means ratified. For a scope linked feature, `Accepted` means built and verified.
4. An `Assumed` specification records provisional build reasoning after the engineer explicitly chose to build before deciding. It may guide only that named feature. It is actionable only when current scope links the feature and records it as active. It is never durable precedent for other work and requires `/architect` ratification or replacement.
5. A specification marked `Superseded by` is historical evidence after its replacement becomes `Accepted`. While a linked replacement remains `Proposed` or `In Progress`, the older accepted specification remains durable outside the replacement feature.
6. Any specification stored under `docs/legacy/` is historical evidence only.

**Replaces**:

1. The rule that `context/` is the primary source of truth.
2. The mandatory reading list for `context/project-overview.md`, `context/architecture.md`, `context/ui-context.md`, `context/code-standards.md`, `context/ai-workflow-rules.md`, and `context/progress-tracker.md`.
3. The use of `context/feature-specs/` as an active specification queue.
4. The use of any document under the retired tree as an implicit answer to a new product, design, or architecture question.

**Enforcement**:

Repository structure is the primary enforcement. Root `AGENTS.md` states the authority order and does not instruct agents to read `docs/legacy/`. Nested `AGENTS.md` files contain only current area guidance and explicit drift warnings. `docs/legacy/README.md` states that archived material is never active guidance. No root `context/` compatibility directory or pointer remains.

The rollout review scans every tracked text file outside `docs/legacy/` for old `context/` references and active reading instructions. This includes source files, scripts, configuration, templates, structured data, and documentation. Binary files are excluded. Every retained match must be reviewed and recorded as an explicit exception in `docs/legacy/context-migration-record.md`.

Markdown validation is a separate check over every tracked current Markdown file outside `docs/legacy/`. Every local file link and heading anchor must resolve. External URLs are outside this activation check. Archived files are byte preserved evidence, so their internal and outbound links are not repaired and do not need to resolve after the move. `docs/legacy/README.md` explains that archived relative paths must be interpreted from their original `context/` location.

A search for old `context/` paths outside `docs/legacy/` may retain literal paths in this specification when they describe history, the migration source, or a search rule. It may not retain an active instruction to read from the retired location. Every current link to archived evidence must use its `docs/legacy/context/` destination and must visibly say that it is legacy evidence. A custom continuous integration rule is not required now because it cannot reliably distinguish valid historical discussion from an active instruction without more metadata and maintenance.

**Rollout**:

Use one documentation only commit. Prepare it in a dedicated Git worktree or an equivalent isolated checkout created from a clean revision that already contains this accepted specification. Keep the original checkout unchanged until verification succeeds. After the rollout commit is merged into `main` and the canonical checkout advances to it, remove the preparation worktree. Do not leave an unlabeled preparation checkout that could be mistaken for the canonical project state.

1. Confirm that `docs/legacy/context/` does not already exist. If it exists unexpectedly, stop and investigate. Do not merge or overwrite its contents.
2. Confirm that every file under `context/` is tracked and unchanged at the rollout source revision. Include hidden files and symbolic links in this check. If any entry is untracked or modified, preserve the work first and restart from a clean revision.
3. Record the source commit identifier. Build a preservation manifest with every source relative path, entry type, and SHA-256 content hash. For a symbolic link, hash and record its target text. The expected inventory is 73 files: six former core context files, 56 old feature specifications, five operations files, five reference files, and one completed migration prompt.
4. If the count, categories, paths, or entry types differ, investigate the cause. Correct factual inventory details only after the difference is understood. Run `/architect` again if the difference introduces a new class of active material or challenges the archive decision.
5. Create a load bearing rule disposition table in `docs/legacy/context-migration-record.md`. Each row records the legacy rule, its current owner and exact path, whether it is retained, replaced, or retired, and any unresolved issue. Map workflow, repository boundaries, local conventions, commands, and safeguards to applicable `AGENTS.md` files. Map product boundaries and status to `docs/scope/`. Map visual and interaction direction to `DESIGN.md`. Map architecture and durable decisions to accepted specifications.
6. Treat any unresolved load bearing disposition row as a rollout blocker. Use `/sync` for `AGENTS.md`, `/scope` for `docs/scope/`, and `/architect` for specifications. Resolve a `DESIGN.md` conflict through `/architect`, then update `DESIGN.md` in the same approved change. Do not copy whole legacy documents or mark an old feature specification as accepted.
7. Move the complete `context/` tree to `docs/legacy/context/` without content edits. Generate a destination manifest and require an exact match of every original relative path, entry type, and SHA-256 hash. Confirm that no root `context/` directory remains.
8. Add `docs/legacy/README.md` with the historical only rule, current authority pointers, and the original path interpretation for archived relative links. Complete `docs/legacy/context-migration-record.md` with the source revision, both manifest results, the disposition table, and the check results.
9. Update root `AGENTS.md` to use this source order, set the project build approach to Tracer Bullet from `docs/scope/index.md`, remove the old mandatory context reading list, and remove the old progress tracker update rule.
10. Update current `docs/scope/` links from `context/feature-specs/` and `context/operations/` to their new legacy paths. Keep their visible legacy labels.
11. Remove legacy comparison notes from nested `AGENTS.md` when the compared legacy source is no longer active. Keep genuine code drift warnings and rewrite their source when needed.
12. Scan every tracked text file outside `docs/legacy/` for old `context/` paths and active reading instructions. Repair active references. Current links to archived evidence must use `docs/legacy/context/` and must label the target as legacy evidence. Record every retained match, including allowed historical and migration path text in this specification, as a reviewed exception in the migration record. Exclude binary files and record how text files were identified.
13. Separately verify every local file link and heading anchor in tracked current Markdown outside `docs/legacy/`. Do not require external URL checks or repairs inside archived files.
14. Review the complete diff. Require documentation files only, exact source and destination manifests, no unresolved disposition rows, no unexpected destination collision, no application change, and all activation conditions satisfied.
15. Create one focused rollout commit containing the move, authority updates, link repairs, archive notice, migration record, and check evidence. Merge that commit into `main`, then advance the canonical checkout to it. The new authority order becomes active in each checkout only when its current state meets the activation rule in `## Decision`.

If any check fails before the rollout commit is created, abandon the isolated rollout worktree and keep the original checkout unchanged. If the rollout commit later needs reversal, revert that entire commit so root instructions, links, and both directory locations return together. Advance affected checkouts to the full revert before treating them as restored. Never restore or revert selected parts into a mixed authority state.

**Exceptions**:

An agent may read a legacy file only when the user asks for historical analysis or when a current authority links to it as evidence. The agent must identify it as legacy and must not derive new requirements from it. Archived bytes remain unchanged even when their relative links no longer resolve from the archive location.

## Consequences

**Positive**:

1. Future agents have one short and deterministic reading path.
2. Product scope, design direction, and durable decisions each have one named owner.
3. Historical plans remain searchable without competing with active guidance.

**Negative and tradeoffs**:

1. Old links must be updated as one coordinated documentation change.
2. Useful details from the old tree are no longer active by default.
3. The rollout requires a clean isolated Git worktree, content manifests, a rule disposition record, and one focused commit.
4. Archived links may no longer resolve from their new location because archive bytes remain unchanged.
5. Ratification and activation are separate. The current context system remains active until the verified rollout commit becomes the checked out repository state.

**Neutral**:

1. The application and database do not change.
2. Training and physical workload code remains in the repository even though current scope marks that capability as dropped.
3. Git history preserves the origin and prior edits of moved files.
4. Archiving the old operations files does not repeal safeguards already present in current code or `backend/AGENTS.md`. It leaves undecided providers and operating targets open.

## Follow-up

1. [ ] Apply and verify the documentation rollout as one focused change. Do not combine it with application cleanup. The new authority order is not active until this item satisfies the activation conditions in `## Decision`.
2. [ ] Keep the design drift explicit. `DESIGN.md` defines the intended dark stadium night system, while `frontend/src/index.css` and the current interface implement the older light system. The structured palette and prose inside `DESIGN.md` also use some different values. Resolve these issues later through their own decision and scoped implementation.
3. [ ] Keep the backend boundary drift explicit. `backend/AGENTS.md` prohibits direct EF Core work in API endpoints, while `MedicalEndpoints.cs` and `TrainingSessionEndpoints.cs` inject `AppDbContext`. Resolve that later through its own decision or focused refactor.
4. [ ] Keep excluded code intact. `docs/scope/deferred-and-excluded.md` remains the authority for the inactive training, physical workload, vendor integration, and video analysis direction until a later scope change.
5. [ ] Keep production choices open. The archived operations files record useful safeguards and blocked questions, but no hosting, storage, database, backup, email, monitoring, or instance topology becomes an active decision until an `Accepted` production specification records it.
6. [ ] Keep the URL state boundary explicit when future work touches it. Nuqs is a current dependency and existing Users filters use it, but retiring the old local state rule does not make every filter shareable by default. The active feature specification should name the owner of each new filter state.
7. [ ] Record progress in `docs/scope/`, durable decisions in `docs/specs/`, and local repository drift in the nearest applicable `AGENTS.md`. Do not recreate a general progress tracker.
