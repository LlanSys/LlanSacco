# Phase 3 Repository and Unit of Work Verification

Phase 3 is complete for review on the branch based on `main` at `2751799`. Required / Remediation applies to the Phase 3 PR before merge.

## Repository decisions

- Reuse Membership's existing four feature repositories. Register named repositories for Loans, Banking, CheckOff, Dividends and IAM in their owning persistence extensions. Each resolves from the same scope as its Unit of Work and context.
- Normalize the suffix on the global ControlPlane catalog properties while retaining its existing save and disposal contract. Move the Accounting, CheckOff and Dividends Unit of Work implementations to their persistence context roots. Align the HR sequence repository with `IRepository<EmployeeNumberSequence>` and preserve its specialized transaction-owned allocation.
- Keep the generic repository, EF DbSets, query contracts and commit ownership in place. Switch the accounting mapping lookup and ordinary IAM token reads and inserts to inherited methods. Refresh-token ownership predicates and cancellation travel with those calls. Preserve the atomic token claim, lifecycle operations, aggregated account balances and runtime lookup routing.
- Apply the renamed properties to feature handlers, workers, tenant routing, consumers and test fixtures. Production registration is tested through the existing DI extensions for all ten contexts. Both configurations resolve the exact scoped repository instance exposed by the Unit of Work.

## Verification

| Check | Result |
| --- | --- |
| API build | Passed |
| Architecture guardrails | 84 passed |
| Exact debt comparison | 103 resolved entries removed, 476 entries in later phases, zero additions |
| Unit tests | 43 passed |
| PostgreSQL and SQL Server integration | 163 passed, one opt-in RabbitMQ test skipped in the default run |
| Live RabbitMQ on the existing LlanSacco Compose broker | Passed in isolation on port 5673 after an initial Windows temporary SQLite file cleanup error |

The provider suite uses disposable databases. No schema change was needed for Phase 3. The work was isolated from the original checkout's uncommitted changes. Phase 4 follows review of this phase.
