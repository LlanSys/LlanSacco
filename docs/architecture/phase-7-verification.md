# Phase 7 Closure Verification

Phase 7 started from main at `101f4ea` after [PR #10](https://github.com/LlanSys/LlanSacco/pull/10) merged. The work uses the existing LlanSacco checkout.

## Authorization and tenant isolation

- Enforced operation permissions on 74 actions. The five member self-service actions require an authenticated actor and ownership derived from the actor's active membership in the current tenant. A caller-supplied member or request ID cannot establish ownership.
- Added provider-specific IAM catalog migrations for the new operations and the existing payroll policies. The migrations populate permission definitions and menu metadata for the default tenant. Tenant administrators control actual role assignments. Follow the [operation permission rollout](ui-to-backend-flow.md#operation-permissions) for existing users and additional tenants.
- Aligned the Check-off, Payroll and accounting integration error routes and action visibility with the API grants. An already authenticated user sees access denied when a grant is missing. Accounting integration responses retain their shape and replace private diagnostic detail with a safe summary.
- Added both-provider isolation tests for Accounting, Banking, CheckOff, Dividends, Loans and Membership. The Dividends tests detected missing tenant query filters and a PostgreSQL row-version default. The provider migrations scope dividend uniqueness to the tenant. Reverting to the former global unique indexes requires resolving any overlapping tenant years or keys first.
- Bound guarantor nominations and responses to the linked member, and use the request status for concurrency checking. Dividend preferences read only the linked member's election. Previously stored actor-keyed preferences remain accessible to their original actor and are corrected when that actor next updates the election.

## Persistence lifecycle

- Internalized the nine remaining public EF configurations.
- Made beneficiary and payroll tax-band removal audited soft deletes. Both providers have additive migrations that preserve existing rows. Payroll uses the active bands and stages changes on the tracked statutory configuration.
- Reviewed each remaining entity against its existing lifecycle in the [record register](record-lifecycles.json). Eleven financial history types also reject ordinary EF deletes at runtime. The source rule rejects typed removals for the reviewed records. The rule caught the existing PAYE deletion and rejects a deliberate invalid journal removal fixture.
- Batched Check-off loan ownership validation within each row transaction. Existing repayment references and replay boundaries remain in use.

## Verification

| Check | Result |
| --- | --- |
| API and Blazor Release builds | Passed |
| Architecture and provider model guards | 89 passed. All 145 Phase 7 findings are resolved and the exact debt register is empty. |
| Unit tests | 47 passed |
| Local integration before the final PAYE tracking correction | 205 passed. The two PAYE update cases surfaced a key-tracking defect and passed on both providers after correction. The full final run is tracked in the PR verification. |
| Opt-in RabbitMQ outbox delivery | Passed against the existing `cg-llansacco` broker using a temporary queue |
| Formatter, package policy, debt comparison and staged whitespace check | Passed |

The database verification used disposable databases. Application migrations were reviewed separately for SQL Server and PostgreSQL. The required hosted merge check is `Required / Remediation`.

## Follow-up evidence

- The signed-in route-policy smoke checks use authenticated test principals and the compiled Blazor route metadata. The Check-off and error response tests exercise the API-to-client contract. Cache tests cover tenant/user isolation, invalidation and rejection behavior. Browser acceptance against running application hosts remains unverified. The checkout had no API or Blazor listener on the configured development or Compose application ports. The existing infrastructure Compose group stayed running.
- The loan pledge route now checks the linked guarantor. The existing downstream funds-lock workflow still lacks verified transactional publication and idempotent balance reservation. Treat live financial acceptance of loan pledges as blocked until that workflow has an explicit reservation contract and a replay test.
- The default tenant receives catalog definitions through migrations. Before rolling out the grant requirements, tenant operators need the applicable catalog entries and role assignments. Validate them with a representative operator in each tenant.
