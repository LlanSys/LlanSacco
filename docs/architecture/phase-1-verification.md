# Phase 1 Verification And Remaining Work

Date: 2026-09-11. Branch: refactor/phase-1-architecture-guardrails. Phase 0 parent: e859d0e. BaseTemplate reference: c17631e223bc2bd1d86c0d052668a0fee8ebbd31.

## Delivered

AGENTS.md, PLAN.md, PLAN_EXECUTION_STRATEGY.md, persistence standards and feature conventions agree on named repository interfaces, Repository property suffixes, commit ownership, separate validators and operation-named command/handler files. The ten bounded contexts have one machine-readable registry. AppResponse and IRepository contracts were preserved.

The initial exact register contains **582 findings**. These are occurrences across 18 rules, not 582 distinct business defects. They remain debt, not approved application behavior. Source and reflection tests reject new occurrences and stale allowances; PR comparison additionally forbids register growth and phase deferral. The bootstrap register itself is visible for review because no prior register exists on main. No blanket skipped tests or automatic baseline regeneration was added. Observed inventory JSON is uploaded with CI diagnostics.

| Rule | Count |
| --- | ---: |
| UOW001 raw repository exposure | 39 |
| UOW002 property suffix | 33 |
| UOW003 generic repository construction | 30 |
| UOW004 specialized inheritance review | 1 |
| DEP001 direct handler persistence dependencies | 0 |
| VAL001 validator file separation | 19 |
| LAY001 operation pairing | 131 |
| LAY002 public type filename | 1 |
| ERR001 error-less failure initializer | 7 |
| CACHE001 query cache policy | 33 |
| CACHE002 command cache impact | 93 |
| EF001 configuration suffix | 8 |
| EF002 configuration visibility | 29 |
| EF003 configuration namespace | 20 |
| EF004 soft-delete policy review | 51 |
| EF005 provider context namespace | 2 |
| AUTH001 missing permission requirement | 79 |
| TENANT001 missing isolation test file | 6 |

UOW004 identifies EmployeeNumberSequenceRepository, an atomic allocation contract. Phase 3 must record the specialized-contract decision rather than invent irrelevant entity CRUD. EF004 requires a retention-policy decision for financial records, not mechanical soft deletion. TENANT001 checks file coverage only; real database isolation remains unverified by that check. LAY002 records the MAUI Windows App.xaml.cs/MauiWinUIApp naming mismatch. Timestamped EF migrations are allowed only when the semantic base type and exact timestamp/type filename agree.

## Verification

- Required API build: passed locally with --no-restore -p:UseSharedCompilation=false.
- Architecture suite: 84 passed, 0 failed, 0 skipped. Negative controls cover aliased raw repositories, incorrect handlers, bundled validators/public types, migration filename spoofing, and added/stale debt.
- Unit suite: 41 passed, 0 failed, 0 skipped. The new dividend test executes handlers through MediatR with the real generic repository and EF InMemory; list/detail totals agree, declarations stay separate, empty totals are zero, and missing records return a typed failure. This is not PostgreSQL/SQL Server translation or tenant-isolation proof.
- PR debt-growth script: isolated Git fixture accepts unchanged/removal and rejects added/deferred debt.
- Blazor build: still fails on missing ApplyForLoanRequest in LoanService/ILoanService. Full integration/provider verification belongs to Phase 7; no claim that the entire required merge gate passes.
- Staged Gitleaks: no leaks found. git diff --check and git diff --cached --check passed. Existing analyzer warnings remain (425 in the final API rebuild).

## Compilation Repairs Needed To Run The Guards

The imported application could not compile. Dividends now calls inherited ListAsync/FindByIdAsync and maps the existing model: FinancialYear is an invariant string, totals sum NetPayout per declaration in database-composed queries, and Notes stays null because it has no persisted field. No schema or response-envelope change was made. Financial semantics and provider translation still require Phase 2 coverage.

Microsoft.OpenApi was aligned to the 2.12.0 range supported by ASP.NET OpenApi 10.0.12. Existing Quartz 4 and FIDO2 4 adapters were updated to their installed parameter/return APIs; cancellation is passed to job execution and FIDO operations, and returned JsonElements own cloned data. The API controller import and a missing Blazor interface brace were repaired. Stale accounting unit tests use the existing CreateJournalEntryDto; the Application assembly grants only LS.Tests.Unit access to internal validators. This does not fix or certify runtime validator scanning.

Quartz now uses its System.Text.Json serializer. Existing persisted scheduler payloads need compatibility/migration testing before any deployment; this change has not been exercised against a persistent scheduler. Passkey ownership/uniqueness callbacks still return true in the inherited implementation and must be fixed and security-tested before release. Compilation does not certify authentication correctness.

The API's DumpEnv target was removed. Its existing local ef_env.txt is preserved on disk, removed from Git tracking and ignored; builds must not serialize process environments into versioned artifacts. No history rewrite or deployment was performed.

## Runtime Review Register

These findings need behavioral tests; the source register cannot prove them resolved.

| ID | Source and symbol | Evidence / required verification | Owner |
| --- | --- | --- | --- |
| RUNTIME001 | LS.Application/Features/CheckOff/Commands/Batches/ValidateCheckoffBatchCommand.cs, ValidateCheckoffBatchCommandHandler | No-tracking row/batch mutations and per-row employment lookup; prove committed results from a fresh context and batched access | Phase 2 |
| RUNTIME002 | LS.Application/Features/CheckOff/Commands/Batches/PostCheckoffBatchCommand.cs, PostCheckoffBatchCommandHandler | Job enqueue occurs inside the transaction before durable commit; prove rollback/retry/outbox semantics | Phase 2 |
| RUNTIME003 | LS.Application/Features/HR/Payroll/Commands/RunPayrollCommandHandler.cs, RunPayrollCommandHandler | Effective-date selection is replaced by unfiltered latest configuration; filtering follows materialization | Phase 2 |
| RUNTIME004 | LS.Application/Behaviours/CacheInvalidationBehavior.cs, Handle | Invalidation lacks successful-result gate; timestamp generations can collide | Phase 5 |
| RUNTIME005 | LS.Application/Behaviours/CachingBehavior.cs, Handle | Failed result enters GetOrCreate before removal; verify nonversioned user scope and in-flight invalidation | Phase 5 |
| RUNTIME006 | LS.Application/Extensions/DependencyInjection.cs, AddApplication | AddValidatorsFromAssembly does not explicitly include internal validators; prove actual DI discovery and pipeline ordering | Phase 6 |
| RUNTIME007 | LS.Infrastructure/Features/IAM/Users/Contracts/Implementations/Services/PasskeyService.cs, MakeNewCredentialAsync/MakeAssertionAsync callbacks | Credential uniqueness and user-handle ownership callbacks unconditionally accept; replace with persisted ownership checks and negative authentication tests | Phase 7, release blocker |
| RUNTIME008 | LS.Infrastructure/Extensions/DependencyInjection.cs, Quartz registration | Test persisted job serialization compatibility and job cancellation under Quartz 4 | Phase 7, deployment blocker |
| BUILD001 | LS.UI.Blazor/Features/Loans/Contracts/Implementations/LoanService.cs, ApplyForLoanAsync | Missing ApplyForLoanRequest prevents UI compilation; reconcile with API transport contract | Phase 6 |

Backend paths above are relative to src/Backend/{layer}; UI paths are relative to src/Frontend/Web. The existing remediation plan also requires a full cross-context mutation/query audit, provider integration checks and permission coverage. This register does not claim static analysis can exhaustively identify runtime defects.

## Git Review Boundary

Changes continue through PRs to main, with Required / Remediation enforced. Phase 0's import remains unmerged; therefore a Phase 1 PR to main necessarily includes the import ancestry. Review Phase 1 specifically against e859d0e. The full gate must pass before merging the combined candidate. Deployments, provisioning and image publishing remain manually gated and disabled. No force push or bypass was used.

## Remote Evidence

Draft [PR #2](https://github.com/LlanSys/LlanSacco/pull/2) publishes implementation commit f8a15611f6e5f2c38773ef0f208d62f101d4c320. [Remediation run 34586115114](https://github.com/LlanSys/LlanSacco/actions/runs/34586115114) completed on Linux: API, Architecture, Unit, Integration and Secrets passed. Integration reported 16 passed and one skipped: RabbitMqOutboxTransportTests.Ef_outbox_should_deliver_message_to_real_rabbitmq_consumer. This does not establish complete outbox or both-provider coverage.

Blazor failed on the same missing ApplyForLoanRequest found locally. Required / Remediation therefore failed, confirming that successful Phase 1 checks cannot override an application build failure. Container Publish was skipped. The PR remains a blocked draft; main was not merged or deployed. A documentation-only follow-up records these results; the evidence above identifies the exact implementation commit tested.
