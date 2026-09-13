# Architecture Remediation And Repository Governance Plan

Date: 2026-09-10
Status reviewed: 2026-09-13, including the password-reset, accounting-index and FOSA follow-up documented in phase-2-verification.md. Phases 0 and 1 complete; Phase 2 partially complete; Phases 3-7 pending. Checklists below distinguish completed work from outstanding phase gates.
Repository: https://github.com/LlanSys/LlanSacco

## 1. Objective And Authority

Restore the documented modular monolith, Clean Architecture, feature ownership, generic repository/Unit of Work, AppResponse, validation, and MediatR pipeline conventions. Establish a protected PR workflow with executable architecture checks. Preserve existing features and user changes; correct runtime defects before treating naming cleanup as complete.

AGENTS.md remains canonical. The user's September 10 instructions explicitly refine the contract: feature-specific repository interfaces and Repository property suffixes; separate validator files; operation-named files containing the command and handler by default. Phase 1 reconciled these rules with the canonical docs and regression tests; existing implementation debt remains assigned to the later phases. BaseTemplate is a reference, not authority over these instructions: its current checkout also contains exceptions and inherited weaknesses.

## 2. Verified Starting Point

- The local directory has no .git metadata. Do not infer original commits or change provenance.
- GitHub reports LlanSys/LlanSacco is private, size 0, default branch main, with admin access for the current account. Recheck branches and contents before bootstrap; size alone is not proof of no commits.
- Historical planning observation (superseded during Phase 0): the rulesets API returned HTTP 403: "Upgrade to GitHub Pro or make this repository public to enable this feature." Resolve private-repository plan eligibility with the owner; do not change visibility or purchase a plan automatically. Also inspect classic branch-protection availability. Local hooks and CI do not substitute for server-enforced merge protection.
- AppResponse.cs and AppResponses.cs match the current local BaseTemplate after namespace normalization. Preserve the transport structure while fixing usage. IRepository method signatures also match; a removed feature-specific using is not a contract change.
- Raw generic repository properties: Banking 14, CheckOff 6, Dividends 3, IAM 7, Loans 5, Membership 4 (39 total). Of these, 30 omit Repository. ControlPlane has three additional named-interface properties without the suffix. IAM's seven raw properties also exist in the current BaseTemplate.
- CheckOff and Dividends construct generic repositories in property getters. Membership already injects named repositories but exposes several as raw interfaces.
- Check-off validation and posting mutate entities returned by no-tracking methods without staging updates. Validation performs an employment lookup per row; posting queues work before the transaction commits.
- Payroll overwrites its effective-date-filtered configuration with an unfiltered latest configuration, materializes before filtering, and constructs error-less AppResponse failures.
- LedgerService throws for expected business validation failures. Several not-found outcomes use BusinessRule errors.
- Registered pipeline order is ExceptionHandlingBehavior -> LoggingBehaviour -> ValidationBehavior -> CachingBehavior -> CacheInvalidationBehavior -> handler. No transaction behavior appears in this registration; handlers use Unit of Work transaction methods.
- Direct cache/invalidation marker occurrence counts by context: IAM 15/21, HR 6/6, Shared 6/3, ControlPlane 5/8; Accounting, Banking, CheckOff, Dividends, Loans, Membership 0/0. These are source counts, not proof of complete runtime coverage or a requirement to cache every query.
- CacheInvalidationBehavior does not check IAppResponse.IsSuccess before invalidating. CachingBehavior caches via GetOrCreate then removes failure results; this is not strict prevention of failure caching. Version generation uses millisecond timestamps. Non-versioned key construction does not use CacheUserId.
- Internal validators exist, while assembly registrations do not explicitly request internal-type scanning. Verify actual discovery through DI; file extraction alone cannot establish execution.
- Existing architecture checks lack the required UoW property rules; the DBContext namespace allowlist omits Dividends. Existing public-type filename checks conflict with the newly mandated operation-file exception.

## 3. Standards To Encode

### Command, handler, and validator layout

Default layout within the owning feature:

```text
Features/{Context}/{Feature}/Commands/CreateOrder.cs
Features/{Context}/{Feature}/Validators/CreateOrderCommandValidator.cs
```

CreateOrder.cs contains the public CreateOrderCommand and its internal CreateOrderCommandHandler. The FILE is named CreateOrder.cs, not CreateOrderHandler.cs or CreateOrderCommandHandler.cs. Retain meaningful Command/Query/Handler TYPE suffixes for MediatR intent and existing type conventions. This is the interpretation of the user's filename example, consistent with the operation-named BaseTemplate file examined.

Validators always occupy their own type-named files, including internal validators. Shared transport validators stay in SharedKernel.Validation under their owning feature. Reuse transport validation with composition where suitable; application validation covers command and use-case concerns.

The operation-file arrangement is a narrow exception to public-type/filename equality, not permission to bundle unrelated public types. Every other public top-level type stays in its own matching file. Separate command/handler files require a documented exception with reason. Keep query conventions consistent where paired queries are used, but do not silently expand the command rule into unrelated packaging changes.

### Persistence and outcomes

- UoW properties use IXRepository XRepository; feature repository interfaces inherit IRepository<TEntity> where applicable. Explicitly document specialized security/non-entity repository exceptions.
- Inject feature repository implementations; no new Repository<T> in UoW getters. Place implementations at the context root and feature contracts under the correct owner.
- Use generic operations and ISpecification for reusable query rules. Add custom repository methods only for justified persistence-specific operations; audit existing methods for duplication.
- Use tracked loads for mutations or explicitly stage detached updates. Commit once under a clearly identified owner. Reconcile AGENTS.md's CompleteAsync rule with transaction helpers that already commit; do not introduce double saves just to satisfy a text rule.
- Use existing AppResponses/AppError factories. All failure envelopes have typed errors; expected outcomes do not become HTTP 500. Keep domain invariants rich while translating expected rejections at the proper boundary.
- Apply filters, security scope, ordering, projection, and paging before materialization. Batch lookups; preserve tenant, soft-delete, audit, and provider guarantees.

## 4. Delivery Phases And Gates

### Phase 0 - Safe Git Bootstrap And Baseline

**Complete: bootstrap and baseline capture.** Evidence: [Phase 0 baseline](../development/phase-0-baseline.md), import commit 1086b12 and PR #1. The recorded build failures are historical baseline evidence, not unfinished bootstrap tasks.

- [x] **0.1** Recheck remote contents, local hidden metadata, ignore rules, and any recoverable original history. Preserve local files; never force-push an unrelated baseline.
- [x] **0.2** Inventory workflows before the first push: inherited main-branch deployment/publishing must not execute against old infrastructure. Make deployment opt-in or require an explicitly enabled protected environment until LlanSacco configuration is verified.
- [x] **0.3** Exclude bin/obj, local environments, credentials, generated packages, and temporary artifacts. Scan the intended first commit for secrets without printing secret values.
- [x] **0.4** If the remote is empty, initialize a minimal governance baseline on main, then import application sources through an import/existing-application branch and PR. If it has content, integrate from the remote history without overwriting it. Record the unavoidable initial empty-repository bootstrap exception.
- [x] **0.5** Run API/UI builds and unit, architecture, and integration checks to record the actual baseline, including environment failures. Do not label an unexecuted test as passing.
- [x] **0.6** Resolve server-protection eligibility, then verify settings by API readback. If unavailable, mark governance incomplete explicitly; keep the repository private.

Gate: recoverable baseline, clean intended commit contents, no accidental deployment, actual CI results, and a recorded server-protection status. Known legacy failures must be explicit before source import; never hide them with continue-on-error.

Gate met. For 0.4, the remote already had a minimal main commit; the import preserved that history instead of applying the empty-repository branch of the plan. For 0.6, the repository was already public when inspected; visibility was not changed. The active ruleset and required check were read back. No independent human approval, merge or deployment is implied by this completion.

### Phase 1 - Canonical Standards And Regression Controls

**Complete: standards and regression controls.** Evidence: [Phase 1 verification](phase-1-verification.md), implementation f8a1561 and its follow-up commits. Completion means the rules and enforcement exist; it does not mean the legacy findings have all been fixed.

- [x] **1.1** Update AGENTS.md, persistence-standards.md, feature-folder-convention.md, PLAN.md, and PLAN_EXECUTION_STRATEGY.md together. Correct repository identity, LS namespace, current context registry, stale template/downstream language, and commit ownership rules. Preserve DataProtection application identity.
- [x] **1.2** Encode the operation-file exception, separate validators, repository naming/types, outcome factories, and cache decision policy.
- [x] **1.3** Add Roslyn/semantic or reflection guardrails as appropriate. Avoid regex-only structural tests and vacuous suffix checks.
- [x] **1.4** Produce an exhaustive violations register with rule ID, symbol, file, owning phase, and verification. For incremental remediation, use an exact reviewed legacy baseline with no-new-violations enforcement; each subsequent PR removes entries. No wildcard exclusions, silent baseline regeneration, or blanket skipped tests. Any initially failing baseline exception must be visible in the bootstrap PR.

Gate: new deviations fail CI, diagnostics identify symbols/files, and tests explicitly allow only the agreed command/handler pairing. Final closure requires removing resolved legacy allowances.

Gate met: 84 architecture tests, negative controls and exact debt-growth/stale-entry checks passed. The initial register contained 582 findings across 18 rules; Phase 2 removed three resolved payroll entries without expanding the register. The old Blazor blocker in the Phase 1 report was subsequently fixed in 4b43a47. GEMINI.md was committed in e68ac27 with canonical AGENTS.md references. Validator extraction, request cache coverage and repository normalization remain later-phase implementation work.

### Phase 2 - Persistence And Financial Correctness

**In progress.** Evidence: [Phase 2 verification](phase-2-verification.md), including the password-reset, accounting-index and FOSA follow-up. Items 2.1, 2.2 and 2.4 are complete. Items 2.3 and 2.5 have substantial completed work, but their full scope and gate remain open.

- [x] **2.1** Stage check-off batch/row mutations correctly and prove committed state using a fresh DBContext.
- [x] **2.2** Replace per-row employment lookups with a filtered batch query and deterministic in-memory mapping. Define duplicate payroll-number handling and validate it.
- [ ] **2.3** Make job dispatch durable relative to commit, preferably through the established outbox/dispatcher mechanism. Prove rollback does not release work, retries do not double-post, and recovery covers commit-to-dispatch failures.
- [x] **2.4** Fix payroll effective configuration selection using a database-side effective-date predicate and deterministic ordering. Push period/status filters into queries, batch related data, and review payroll deletion/re-run semantics rather than preserving destructive behavior blindly.
- [ ] **2.5** Audit every mutation through generic no-tracking methods, transaction entry point, and background job for the same defects. Review transaction failure results, cancellation, domain-event dispatch, retries, and cross-context writes. Do not wrap all requests in a new transaction behavior indiscriminately.

Completion notes for 2.1, 2.2 and 2.4: fresh-context provider tests prove saved CheckOff diagnostics, bounded employment lookups and deterministic rejection of duplicate/ambiguous payroll numbers. Payroll tests prove effective-date cutoff, batched inputs, atomic persistence, preservation of earlier payslips and rejection of competing/rerun processing. Main corrections began in 50bdc46/6476ac8; later provider and retry coverage is recorded in the phase report.

**2.3 - Dispatch and recovery detail**

- [x] Implement provider-aware, context-owned Banking, CheckOff and Loans outboxes; keep Posting state and the CheckOff dispatch event under the same commit owner (4b43a47).
- [x] Correct workers to consume Validated rows, check downstream results, process shares and mark the batch Posted only after successful row processing; provider tests prove rejected rows remain retryable (4b43a47).
- [x] Use stable payment references and verify matching/conflicting savings, share, loan and journal replays on both providers (4b43a47).
- [x] Prove EF outbox rollback, successive commits in one scope and delivery by a new host after the writer is disposed (4b43a47). This uses in-memory transport; it does not certify RabbitMQ connectivity.
- [ ] Prove combined CheckOff dispatch-to-payment recovery across contexts, including competing deliveries. Worker-state and receiver-replay tests currently cover these parts separately.
- [ ] Complete tenant/database selection for recurring interest jobs and outbox delivery in dedicated tenant databases.

**2.5 - Mutation and transaction audit detail**

- [x] Stage detached savings balances, eliminate the separate first-deposit save and reject withdrawal penalties before staging mutations; verify saved/unchanged state with fresh provider contexts (0ad2d29).
- [x] Propagate retry cancellation, clean up failed attempts, dispose transactions before backoff and remove redundant saves from existing retry callbacks (d9c4005). Intentional failure-result writes remain supported and tested.
- [x] Persist daily interest markers and use atomic, bounded accrual/maturity batches with provider concurrency and rerun tests (d9c4005).
- [x] Correct CheckOff provider DI, establish explicit background tenant/actor scope and preserve that scope in tenant connection routing; verify isolation (d9c4005, 4b43a47).
- [x] Stage loan repayments with the named ILoanRepaymentRepository; fix new share-account creation and self-transfer balance inflation; verify provider replay and balance conservation (4b43a47).
- [x] Repair index-generator semantics and collection-case predicates; apply full Banking and Loans migration chains in fresh databases for both providers (4b43a47). No deployed database was migrated.
- [x] Claim refresh tokens atomically inside rotation transactions and re-read expired tokens during retries; provider tests prove one competing winner, tenant isolation and rollback recovery (06b9edf).
- [x] Make password reset and refresh-token revocation atomic, including Identity failure results, retries and cancellation. The explicit commit predicate preserves intentional diagnostic writes by default; both provider tests verify rollback after embedded saves.
- [x] Reject invalid FOSA cash amounts and denomination counts before mutation; use the stable actor identifier. Provider tests verify unchanged rejected state, cash/vault balance conservation and stale-update rejection.
- [ ] Finish the wider IAM and cross-context mutation audit, including user creation/role/profile boundaries, login/session issuance and remaining FOSA/background writer recovery. These targeted fixes do not close the whole audit.
- [ ] Settle and test generic/Shared domain-event commit semantics. Financial outboxes do not settle every domain-event helper.
- [x] Add separate provider migrations to rebuild the three accounting unique indexes. Upgrade tests reproduce existing non-unique indexes, prove successful repair and confirm duplicate journals cause rollback without data loss. No deployed database was migrated.

Gate: focused unit plus PostgreSQL and SQL Server integration tests prove saved state, rollback, idempotency, future-configuration exclusion, and bounded query counts. If an environment is unavailable, retain an explicit verification gap.

**Gate remains open** because the unchecked recovery/audit items above remain. Verified checkpoint 4b43a47 passed API/Blazor builds, 84 architecture tests, 43 unit tests and 69 integration tests; the external RabbitMQ test was skipped. Its GitHub Required / Remediation check also passed. Follow-up 06b9edf passed API, 43 unit and 84 architecture checks plus two additional provider claim tests. This records exact test evidence rather than assuming one combined 71-test run. The later follow-up passed 89 integration tests with the same one RabbitMQ skip, including actual accounting upgrades and Identity failure-result rollback. PR #3 remains draft; no merge or deployment is implied.

### Phase 3 - Repository And Unit Of Work Alignment

1. Normalize Membership and Loans, then Banking, CheckOff, Dividends, IAM, and ControlPlane; verify HR, Accounting, and Shared as well.
2. Replace all 39 raw properties with named repository contracts; fix all 33 missing suffixes. Reuse existing named implementations before creating thin ones.
3. Update constructor injection, registrations, handlers/services/jobs, mocks, and integration fixtures in each context-sized PR. Move misplaced UoW implementations.
4. Inventory custom repository methods; retain justified persistence behavior and replace generic CRUD duplication with inherited methods/specifications. Preserve accepted IAM lifecycle and runtime lookup routing exceptions.
5. Check references, context ownership, cancellation, filters, and commits across all consumers, including non-MediatR writers.

Gate: zero raw generic UoW properties or unsuffixed repository properties outside individually documented legitimate exceptions; no generic repository construction in UoW; DI resolves all contexts; provider regression tests pass.

### Phase 4 - AppResponse And Business Failure Alignment

1. Replace IsSuccess=false object initializers lacking Error with typed factories. Normalize NotFound, Forbidden, Validation, Conflict, and BusinessRule semantics.
2. Correct expected LedgerService failures and preserve domain invariants. Audit API overrides and UI clients so ProblemDetails and safe messages survive the full flow.
3. Define boolean payload semantics explicitly: AppResponse<bool>(false) is still a successful envelope. For check-off validation, decide whether rejected rows are a successful validation report or a failed operation, then use a clear response contract and tests.
4. Preserve existing response structure unless a separately justified, compatibility-tested contract change is required. Move user-facing internal identifiers out of errors and use source-generated diagnostic logging.

Gate: HTTP contract tests prove expected status/error codes and sanitization; no expected missing-record/business rejection becomes an accidental 500.

### Phase 5 - Behaviour And Cache Coverage

1. Inventory every MediatR request and every writer by context. Record query policy as cached or explicitly bypassed/uncached with reason, TTL, tenant/user/permission scope, filter/page discriminator, and affected invalidators. Financial freshness and sensitive user-specific reads require deliberate decisions.
2. Retain ICachableRequest and ICacheInvalidatorRequest names. Do not add empty marker implementations solely to satisfy tests. Commands affecting cached reads declare actual invalidation targets; other writers use the same policy through appropriate services/events.
3. Add an IsSuccess gate to invalidation; invalidate only after a successful commit. Define behavior for cache failure after durable commit so clients are not misled into retrying a completed financial mutation.
4. Prevent failed results from entering shared caches, rather than inserting then evicting them. Preserve stampede protection and test concurrent misses.
5. Centralize key construction. Verify tenant/user scope for entity and list keys, empty-tenant behavior, permission-sensitive data, direct invalidation, user-scoped groups, and all filter/paging inputs.
6. Replace collision-prone version tokens with a concurrency-safe generation strategy. Test multi-node HybridCache behavior, local/distributed invalidation, in-flight reads, rapid successive writes, and version expiry.
7. Verify pipeline order through IMediator and actual DI: exceptions/logging outside validation; validation and applicable authorization before cache hits; invalidation after successful commit. Evaluate a transaction behavior only if a scoped marker and UoW ownership solve a demonstrated problem without nested commits.
8. Verify CancellationToken propagation and LoggerMessage usage throughout behaviors; review validators that perform async I/O so parallel validation does not share a DbContext unsafely.

Gate: DI-level tests prove participation and ordering; integration tests prove stale data is invalidated, tenants/users cannot share unintended entries, failures are not cached, rejected commands do not invalidate, and cache faults cannot trigger duplicate writes.

### Phase 6 - Validator Extraction And Slice Layout

1. Extract every inline validator to its own type-named file under the owning feature. Inventory user-input writes and reuse shared transport validators where appropriate.
2. Explicitly register intended internal validators or adopt public standalone validators consistently. Assert IValidator<TCommand> resolution and prove invalid requests never enter handlers through IMediator/API tests.
3. Co-locate each command and internal handler in its operation-named file, for example CreateOrder.cs. Preserve Command and Handler type suffixes and update tests to the narrow filename exception.
4. Apply feature ownership and Request/Response transport naming cleanup with serialization/consumer checks. Replace free-form logs in affected paths with LoggerMessage definitions.

Gate: all validators are standalone, all relevant input writes validate at runtime, operation files follow the default or carry reviewed exceptions, and public-type guardrails still reject unrelated bundling.

### Phase 7 - Full Closure

Run API and Blazor builds, unit/architecture tests, both-provider integration suites, formatting checks, migration/model guards, and targeted check-off/payroll/error/cache UI smoke tests. Do not generate migrations for naming-only refactors; real schema changes require separate provider migrations and correct factory fallbacks.

Re-audit all registered contexts for inward dependencies, cross-context persistence, entity configuration, tenant/soft-delete filters, audit actors, N+1 access, and remaining custom-method duplication. Remove temporary violation allowances, update the phase register with evidence, and verify a deliberately violating test fixture is rejected by guardrails.

Gate: all remediation items closed or explicitly blocked with evidence; protected PR checks verified on the remote; no claim of production readiness based only on compilation.

## 5. Git Workflow And Remote Guardrails

Use a protected-main PR workflow with short-lived feat/*, fix/*, refactor/*, and chore/* branches. BaseTemplate workflows target main; a permanent develop branch adds no demonstrated benefit here. Use release/* only when maintaining a separate supported release, and tag releases vX.Y.Z. Hotfixes follow the same checks. Squash merge focused PRs and remove merged branches.

Protect main against direct pushes, force pushes, and deletion; require current successful checks and resolved review conversations. Set least-privilege GitHub Actions permissions. Configure CODEOWNERS for actual eligible maintainers of architecture, domain, persistence, IAM, and .github paths; do not invent accounts or require an impossible self-approval. Require independent approval when a second eligible reviewer is available; record any temporary solo-maintainer review policy explicitly while retaining PR/check requirements. Narrow, auditable emergency bypass only.

Adapt BaseTemplate's architecture/backend/frontend/full-solution workflows and PR template. Add an always-running, uniquely named aggregate merge gate for every PR to main. It must fail on failed/cancelled required jobs and permit only justified non-applicable skips; required workflows must not disappear due to path filters. Verify exact emitted check names before requiring them.

Required gate coverage: architecture and source conventions; API and Blazor build; unit tests; PostgreSQL/SQL Server persistence tests for relevant changes; formatting/analyzer checks; migration consistency; secret/dependency checks appropriate to available repository capabilities. Full/nightly jobs supplement rather than replace merge-time correctness checks. Upload diagnostics/test reports on failure. No continue-on-error for required checks.

Update .gitignore, .editorconfig, PR template, contribution guide, CODEOWNERS, and a single local validation script aligned with CI. PR descriptions include problem, behavior, contract impact, evidence, and remaining risks. Capture the BaseTemplate comparison commit when available. Pin/review workflow dependencies and deployment targets. Use OIDC for Azure and environment-controlled release promotion; do not activate inherited deployment credentials or paths.

GitHub references:
- https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches
- https://docs.github.com/en/pull-requests/how-tos/merge-and-close-pull-requests/troubleshooting-required-status-checks

## 6. Enforcement Matrix

| Rule | Enforcement |
| --- | --- |
| Named UoW repository types and suffix | Reflection/semantic architecture tests |
| No raw repositories/DbContexts in handlers | Constructor and dependency checks with exact exceptions |
| Thin repositories; no premature saves | Semantic checks for known forbidden calls plus behavioral integration tests/review |
| Separate validators and operation file pairing | Roslyn source structure tests excluding generated sources only |
| Validators actually execute | DI and IMediator/API tests |
| Typed failure outcomes | Source checks plus HTTP contract tests |
| Cache policy coverage and invalidation | Request inventory checks plus pipeline/integration tests |
| No tracking-loss/N+1/partial commits | Fresh-context, query-count, rollback and idempotency tests |
| Inward dependencies and bounded contexts | Assembly/namespace architecture tests and explicit context registry |
| Provider and tenant correctness | Dual-provider integration, migration/model, filter/audit tests |
| Remote merge policy | Rules/protection readback and required-check verification |

Structural tests cannot prove all business semantics or absence of N+1 queries. Keep behavioral tests and review evidence for those properties.

## 7. Execution Tracking

- [x] Phase 0: Git bootstrap and baseline (PR #1; failing source baseline captured, merge gate verified)
- [x] Phase 1: Standards and regression controls (all four items complete; see [Phase 1 evidence](phase-1-verification.md). Legacy remediation remains assigned to later phases.)
- [ ] Phase 2: Persistence/financial correctness (2.1, 2.2 and 2.4 complete; 2.3 and 2.5 partially complete. See the detailed checked/unchecked items above.)
- [ ] Phase 3: Repository/UoW alignment
- [ ] Phase 4: AppResponse alignment
- [ ] Phase 5: Behaviours and caching
- [ ] Phase 6: Validators and slice layout
- [ ] Phase 7: Full closure

Checked items mean implemented and supported by the cited evidence; they do not mean merged or deployed. Unchecked items with completed substeps remain open until their remaining scope is verified. Phases 3-7 have not been marked complete merely because Phase 2 made isolated improvements in those areas.

Use context-sized PRs with their own tests; do not combine the entire cleanup into one merge. Phase 2 takes priority over broad mechanical renames. Update this checklist with actual commits/PRs and test evidence, not estimates of completion.
