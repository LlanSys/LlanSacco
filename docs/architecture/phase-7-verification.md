# Phase 7 Closure Verification

Phase 7 started from main at `101f4ea` after [PR #10](https://github.com/LlanSys/LlanSacco/pull/10) merged. The work uses the existing LlanSacco checkout.

## Implemented

- Make the nine remaining public EF entity configurations internal. Their mappings are unchanged.
- Add executable tenant isolation tests for Accounting, Banking, CheckOff, Dividends, Loans and Membership using the PostgreSQL and SQL Server fixtures. The architecture guard locates xUnit tests tagged with their context.
- Apply the standard tenant filters to Dividends. The first provider runs found that SQL Server exposed both tenants' test declarations and PostgreSQL could not insert a declaration because `RowVersion` lacked a default. Unique indexes now include the tenant for declaration years, distribution preferences and calculations.
- Repair beneficiary removal. The handler stages one audited soft delete and commits through the Membership Unit of Work. Both provider migrations preserve existing rows as active.
- Apply established permission keys to 35 actions in Accounting, Dividends, Payroll, Loans and Membership. Payroll dashboard access follows the existing payroll read policy. Dividend administration uses the Control Plane permission key already used by the catalog and UI.

SQL Server and PostgreSQL each have separate Membership and Dividends migrations. After tenants use overlapping dividend years or preference keys, reverting to the former global unique indexes requires resolving those overlaps before rollback.

## Verification

| Check | Result |
| --- | --- |
| API and Blazor Release builds | Passed |
| Architecture and migration model tests | 88 passed, including Membership and Dividends snapshot checks for both providers and the deliberate source violations |
| Unit tests | 47 passed |
| Release integration suite | 197 passed. The 14 new cases exercised both providers, including actual Dividends migrations and the Membership soft-delete command on migrated databases. |
| Opt-in RabbitMQ delivery | Passed against the existing `cg-llansacco` broker using a temporary queue |
| Source formatter, package policy, exact debt comparison and Git whitespace check | Passed |

The database tests used disposable containers. No application data migration or deployment was run against the existing Compose services. Signed-in browser verification and hosted Phase 7 checks remain open.

## Remains open

| Rule | Remaining | Work |
| --- | ---: | --- |
| AUTH001 | 44 | Finish the Banking, CheckOff, collections and accounting operations permission mapping. Verify that tenant role grants and UI access match the resulting API policies. Bind the guarantor and dividend self-service handlers to member ownership before recording any policy exceptions. |
| EF004 | 50 | Review each entity against its actual lifecycle. Verify ordinary soft deletes and preserve ledger, repayment and payroll history through an explicit retention decision where required. |
| EF002 | 0 | All entity configurations covered by this rule are internal. |
| TENANT001 | 0 | All nine tenant-filtered contexts have named isolation tests. |

The dividend preferences read currently lists tenant-wide preferences and the write assumes that the authenticated actor ID is the member ID. The guarantor response handler also lacks a verified link from the actor to the nominated member. Those routes remain in the register.

The cross-context review also identified per-allocation loan lookups in `ChildCheckoffChunkJob`. Batch the loan ownership check within each row transaction while preserving per-payment replay keys and the existing retry behavior.

Phase 7 also requires the remaining cross-context persistence review, targeted signed-in check-off, payroll, error and cache smoke tests, and protected PR checks. The phase stays open until the remaining work and evidence are recorded.
