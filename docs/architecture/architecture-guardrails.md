# Architecture Guardrails

AGENTS.md is canonical. This document describes enforcement, not alternative conventions.

## Rule Register

| Rule | Requirement | Remediation owner |
| --- | --- | --- |
| UOW001 | Expose feature-specific repository interfaces, not raw IRepository<T> | Phase 3 |
| UOW002 | Every UoW repository property ends in Repository | Phase 3 |
| UOW003 | Inject feature repositories; no new generic Repository<T> in UoW | Phase 3 |
| UOW004 | Named entity repository interfaces inherit IRepository; specialized contracts require explicit review | Phase 3 |
| DEP001 | Application handlers cannot receive raw repositories or EF contexts | Phase 3 |
| VAL001 | Every validator occupies its own type-named file, including internal validators | Phase 6 |
| LAY001 | CreateOrder.cs contains CreateOrderCommand and its internal matching handler only | Phase 6 |
| LAY002 | Other public types occupy matching files; unrelated public types cannot use the operation exception | Phase 6 |
| ERR001 | Failure initializers require a typed Error | Phase 4 |
| CACHE001 | Queries explicitly declare caching/bypass policy through ICachableRequest | Phase 5 |
| CACHE002 | Commands declare invalidation or an exact no-cached-dependents decision | Phase 5 |
| EF001 | Configuration type names end in Configuration | Phase 6 |
| EF002 | Configuration implementations are internal | Phase 7 |
| EF003 | Configuration namespace follows feature EntityConfigurations convention | Phase 6 |
| EF004 | DbSet soft-delete policy: review financial retention before changing entities | Phase 7 |
| EF005 | Provider contexts live in registered DataContext namespaces | Phase 6 |
| AUTH001 | Feature actions require permission policies | Phase 7 |
| TENANT001 | Each tenant-filtered context has a named isolation integration test | Phase 7 |

SourceArchitectureRules resolves C# symbols through Roslyn, including aliases and inherited interfaces. Reflection/assembly tests continue enforcing inward dependencies, permission boundaries, and persistence conventions. bounded-contexts.json must match actual Domain contexts and AGENTS.md; it drives persistence namespace checks, including Dividends. Application assembly references use a compile-time anchor rather than a bin-directory assumption.

CreateOrder.cs may contain the public CreateOrderCommand and internal CreateOrderCommandHandler. This does not waive validator separation, permit an unrelated public type, or rename the handler type to CreateOrder. Queries are not automatically forced into the command pairing rule. Generated .g.cs/.Designer.cs or auto-generated source is excluded from authored-source layout checks; entire Platforms or feature directories are not excluded. EF Migration subclasses may use a 14-digit timestamp followed by their exact type name; their authored bodies remain inspected.

## Exact Legacy Debt

architecture-debt.json is the reviewed occurrence register. Every entry identifies a rule, portable source path, symbol, line, concrete evidence, and owning remediation phase. The identity uses rule/path/symbol/evidence; line numbers are informational so moving an unchanged declaration does not hide or invent debt. The current locations are emitted on every run into artifacts/architecture-inventory.

CI compares each rule's actual occurrences to the register. New violations fail. Resolved entries also fail until removed from the register. Duplicate entries and wildcard paths are rejected. Never regenerate this file as a way to pass checks. The initial Phase 1 inventory is deliberate and visible in its PR; the PR check in scripts/check-architecture-debt.ps1 forbids adding identities or deferring owning phases relative to the base commit. Changing that policy requires an explicit architecture decision and a separately reviewed guardrail change. The register is not a permanent exception list or production approval.

Run dotnet test tests/LS.Tests.Architecture/LS.Tests.Architecture.csproj --no-restore -p:UseSharedCompilation=false. scripts/verify-baseline.ps1 runs the wider CI baseline. No test writes or updates the authoritative register; observed JSON artifacts are diagnostics only. Negative-control tests prove that aliased raw repositories, inline validators, and extra public types are rejected while the narrow operation pair is allowed.

## Cache Decisions

Use ICachableRequest for read queries, with explicit tenant/user/permission scope, filters/paging, expiration, and deliberate BypassCache for fresh/sensitive reads. Marker presence is only a structural check; Phase 5 verifies behavior, invalidation timing, and cache keys.

Commands affecting cached reads use ICacheInvalidatorRequest and real invalidation targets. A command with no cached dependents may instead have an exact Symbol and concrete Reason in cache-policy-decisions.json. Unused/duplicate decisions fail. Do not invent empty implementations merely to satisfy a marker test. Runtime behavior tests remain necessary: structural checks cannot establish cache correctness, validator discovery, transactional behavior, or absence of N+1 queries.

## Commit Ownership And Exceptions

Repositories stage ordinary mutations. CompleteAsync(ct) commits once, or ExecuteInTransactionAsync(operation, ct) owns save/commit. Do not double-save. Existing retry/domain-event inconsistencies remain Phase 2 debt. ControlPlane remains a global catalog with its existing distinct UoW contract; entity repositories still need named interfaces and Repository suffixes. Specialized IAM/non-entity repositories may have persistence-specific contracts.

## Review Policy

Changes to AGENTS.md, this document, the debt register, cache decisions, context registry, and architecture tests must explain the rule impact in the PR. Main requires a successful remediation gate and resolved review conversations. The current repository has no independent human approval requirement; this is not a claim that an author can independently approve their own change. Do not bypass the gate or expand baselines to hide a failure.
