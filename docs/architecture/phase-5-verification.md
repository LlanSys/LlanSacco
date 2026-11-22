# Phase 5 Behaviour and Cache Coverage

## Request and writer inventory

The source audit found 65 MediatR query policies. Of these, 54 explicitly bypass the query cache and 11 use tenant-scoped caching. The exact command register contains 116 reviewed decisions for requests without cached dependents. The remaining 15 commands invalidate active cache groups. The audit removes all 126 existing CACHE001 and CACHE002 entries.

| Context | Reads | Writers and invalidation |
| --- | --- | --- |
| Accounting | Ledgers, statements, operational totals and integration error status read committed data. | Journal commands, accounting consumers and retries write data read directly. |
| Banking | FOSA, shares, deposits, savings, product terms and financial totals use fresh reads. | Transaction handlers, interest jobs, maturity processing and accounting events change uncached data. |
| CheckOff | Batches, rejected rows, employers and instructions read current state. | Upload, validation, posting, employer maintenance and background processing affect fresh reads. |
| Control Plane | Tenant and stamp administration, support access, hostname resolution and module reads use current state. Database routing uses one snapshot per request scope. | Provisioning callbacks, tenant and stamp commands, role-sensitive support access and module changes affect fresh reads. |
| Dividends | Declarations, preferences and payout state read current financial data. | Declaration, calculation, approval and payout handlers affect fresh reads. |
| HR | Department list, detail and search are cached per tenant for 30 minutes. Search includes the full filter and page request. Department detail includes its ID. Employee and payroll reads bypass caching. | Department create, update and delete advance the department generation. Employee and payroll changes, including IAM updates to employee links, affect fresh reads. |
| IAM | Menu and permission definition search and detail, and the two reference catalogue views, are cached per tenant for 30 minutes. Search and reference requests use their full filters or catalogue selector. Navigation, user grants, role membership, devices, credentials and private profile reads bypass caching. | Menu, permission definition and reference catalogue writers advance their corresponding generations. Session and credential stores own token and challenge expiry. |
| Loans | Application, product, balance and collection queries use fresh financial and officer-scoped data. | Application, repayment, collections and reminder handlers affect fresh reads. |
| Membership | Member details, beneficiaries, review queues and operational totals use fresh state. | Membership, guarantor and financial handlers affect fresh reads. |
| Shared | Administrative settings list and detail are cached per tenant for 30 minutes, with masked response values for sensitive keys. The detail includes the setting key. Payment reads, operational dashboard and runtime settings are fresh. | Settings commands advance the administrative settings generation. Runtime settings read tenant-scoped committed values directly. Payment callbacks update fresh financial data. Background email reads the current tenant template from its repository. |

EF model seed data for departments and IAM reference definitions is applied through provider migrations. Runtime writes to retained groups go through the command policies above. External SQL maintenance can leave previously cached administrative metadata visible for the remainder of its 30-minute TTL.

## Behaviour decisions

- Run exception logging, request logging and validation before any query cache lookup. Validate I/O sequentially so validators sharing a scoped DbContext finish one at a time. HTTP authorization runs before controller-mediated requests. Credential and permission-sensitive reads bypass caching.
- Store unique generations in the shared cache. Payloads use the request type, tenant, user when applicable, filter and paging discriminator, and relevant group and entity generations. Every instance reads the shared generation even when a payload is already in its local cache. Group versions invalidate every user scope within a tenant, and user versions can invalidate one scope. An empty tenant bypasses the query cache. Reading a shared generation also invalidates payloads still held in memory by other instances. [HybridCache documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-10.0)
- Reuse StarterTemplate's cache factory lifetime and execution-context handling, and keep HybridCache's same-instance miss coalescing. A typed failure leaves the factory before HybridCache writes, and the calling request still receives the typed AppResponse. A failed version lookup reads from the source.
- Hostname and module lookups read directly from the control plane. Database routing keeps one snapshot within the request scope so existing connections use a consistent route while new requests see updated configuration.
- Writes retain their handler-owned Unit of Work. Successful outcomes advance their generations after the handler returns. Rejection does not advance a generation. After commit, invalidation has a three-second budget even if the client disconnects. If the shared cache rejects the update, the server logs the failure and preserves the durable result, with stale administrative entries expiring within their configured TTL. Financial and security reads bypass this stale window.

## Local verification

| Check | Result |
| --- | --- |
| API and Blazor builds | Passed |
| Architecture checks | 84 passed. The exact register has 346 entries in later phases and zero Phase 5 entries. |
| Unit tests | 47 passed |
| Release integration suite | 180 passed across the two provider fixtures and other integration checks. The opt-in RabbitMQ case is excluded by default. |
| Phase 5 regressions | Seven passed. The five MediatR and HybridCache cases also ran against the running LlanSacco Redis service. The other two cover fresh hostname resolution and connection routing across scopes. |
| Opt-in RabbitMQ transport | Passed on retry against the running LlanSacco broker. Windows locked the temporary SQLite file during cleanup on the first attempt. |
| Package policy and scoped formatting | Passed |
| Hosted remediation check | Pending the Phase 5 PR |

Phase 5 is based on main at 515ed6c, the merged [Phase 4 PR](https://github.com/LlanSys/LlanSacco/pull/8). Its baseline tree matches the one used for local Phase 5 verification.
