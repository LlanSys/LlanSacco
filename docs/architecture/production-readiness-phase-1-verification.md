# Production readiness Phase 1 — Guarantor-backed lending

**Phase status: OPEN**, 24 November 2026. Core implementation, automated verification and a staged local acceptance (browser consent, isolation, approval, disbursement, legacy reconciliation rehearsal and restore rehearsal) are complete. Remaining: browser cancellation/settlement/substitution/recovery, a recovery GL-mapping path and tie-out, first-SACCO reconciliation, representative deployed recovery, and review/sign-off. Phase 2 has not started.

**Implementation commit:** `f1d1060` on `codex/production-readiness-phase-1`, based on `main` at `2bd4bd4`, merged to `main` as `fabaa0a` through [LlanSys/LlanSacco#13](https://github.com/LlanSys/LlanSacco/pull/13) after all required CI checks passed. The local verification below covers that source tree.

This is the production-readiness delivery phase, distinct from the earlier architecture remediation phases. The user accepted LLANSACCO-REFERENCE v0.1 as the development baseline and authorized this implementation. That authorization does not supply a real SACCO's policies, KYC-provider approval, Finance mapping approval or release sign-off.

## Delivered behaviour

- Loans retains the precise pledge identity, member consent, selected savings account, accepted amount, policy version and expiry. It commits pending consent and a context-owned outbox instruction together. Pending consent is not confirmed security.
- Banking owns the retained reservation and operation receipt. Unique tenant/pledge and tenant/operation indexes prevent duplicate identities. Matching deliveries replay the recorded outcome; mismatched operation details return a typed conflict. Money is compared at two decimal places and expiry at UTC microsecond precision across providers.
- Banking serializes short reservation decisions within each tenant using transaction-owned SQL Server application locks or PostgreSQL advisory locks. Other tenants proceed independently. Savings row versions and fresh reads inside retries protect competing withdrawals. No external side effect runs inside a database retry.
- Disbursement requires at least two distinct, non-borrower guarantors and full confirmed principal coverage. It records a pending commitment, then creates schedules and one durable disbursement event only after Banking confirms non-expiring committed security. An empty guarantor list cannot bypass this gate. Own-deposit liens and unsecured-product policies are not implemented by this change; the reference release uses the guarantor-backed route.
- Cancellation, rejection, deadline expiry and settlement use retained, replay-safe transitions. A release arriving before reserve creates a terminal tombstone. Committed security does not expire with the original pre-disbursement deadline. Late outcomes cannot resurrect a released pledge. Legacy adoption completing after loan closure queues the necessary release.
- Credit approval requires an independent actor and records eligible deposit balances, the configured multiple, the policy version and the approval time. Borrower-scoped transaction locks make simultaneous approvals include outstanding principal and approved undisbursed commitments. Eligible deposit products must be explicitly configured and non-withdrawable.
- Recovery requires a defaulted loan, a source-linked request, notice/authority evidence and a second actor. Approval earmarks principal while Banking is pending. Proportional allocation uses exact integer cents, assigns residual cents to the largest fractional remainders and resolves ties by stable pledge identity. This avoids intermediate decimal overflow and prevents the last guarantor absorbing more than its remaining capacity. Banking debits each approved guarantor balance and hold once, retaining both operation and pledge links. Loans applies confirmed principal reduction once. Unapproved requests can be cancelled; in-flight requests require reconciliation.
- Recovery publishes a dedicated `GUARANTOR_RECOVERY` accounting event. Its approved mapping must debit an active deposit liability and credit an active loan-principal asset. A cash-receipt mapping is rejected. Missing/invalid mappings produce the existing durable accounting exception, rather than a fabricated successful journal. Finance still needs to approve the actual account identities.
- Legacy holds are never silently added again. A source-linked review with independent approval explains the identified existing locked amount, without increasing it. Closed-loan adoption releases it. Unexplained differences remain visible and prevent new reservations against the affected account.
- Substitution retains the original committed hold until the replacement member gives consent, Banking confirms the replacement hold, and an independent approver commits the new coverage. Only then is release of the original security queued.
- A five-minute tenant-scoped Hangfire job expires eligible pre-disbursement holds. It rechecks active tenant/stamp placement before executing.

The withdrawal race also exposed a scoped-outbox retry defect. EF clears tracking after rollback, while the scoped publisher can retain its old `OutboxState` reference. The context publisher now restores only the required outbox state envelope before staging fresh messages. It does not restore failed business writes or discarded messages. The forced same-snapshot race exercises the actual EF outbox on both providers.

## Entry points and permissions

| User | Entry point | Required access and behaviour |
| --- | --- | --- |
| Member | `/loans/my-guarantees`, linked from membership guarantor requests | Signed in; API uses `MemberSelfService`; ownership is resolved from the current tenant's active IAM-linked membership. Acceptance additionally requires verified membership and the exact displayed policy version. |
| Credit reader | `/loans/applications/{loanId}/guarantees` | `applications.view`; current pledge states, consent reference, held amounts, pending recovery and latest operation history. |
| Credit operator/checker | The loan guarantee operations page | `applications.approve` for prepare/approve/cancel; recovery and security adoption/substitution enforce independent approval in Application. `applications.disburse` for disbursement. |
| Banking/Credit reconciler | `/loans/guarantor-reconciliation` | Both `applications.view` and `banking-accounts.view`; paged member/product balances, explained active holds and differences. |
| Product operator | Savings product security configuration | `banking-products.manage`; explicit eligibility switches and an audit evidence reference. Existing Control Plane page guards remain in place. |

Existing loan application and product pages retain their existing `ControlPlane.Manage` guards/layout. The dedicated Phase 1 pages use native Blazor authorization and the existing API permission boundary. Grant the required tenant roles through normal administration; do not broaden route access as a deployment shortcut.

The operation list intentionally shows the latest 100 recoveries and 100 security reviews for a focused loan. The retained records remain in the database. These operational views do not establish Finance acceptance of portfolio reports or statutory return layouts.

## Automated evidence

Tests use separate database connections/scopes and the real bounded-context Units of Work. Direct-delivery tests read actual durable outbox envelopes; the broker test uses the production consumers and newly built worker hosts.

| Check | Recorded result |
| --- | --- |
| Real local RabbitMQ (`scripts/test-local-messaging.ps1 -SkipStart`) | 3 passed, zero skipped: the existing transport certification and new guarantor worker-restart tests for SQL Server and PostgreSQL. |
| Migration/lifecycle and platform integration regressions | 218 passed, zero skipped or failed. Includes both providers' clean installation/prior-schema upgrades, closed-loan adoption race, settlement after substitution, ownership/verification gates, competing recovery approvals, validator DI, UoW DI and native route/API permissions. |
| Final recovery and deterministic rollback regressions | 14 passed on both providers after the allocation correction. The withdrawal is deliberately committed first after both writers read the same snapshot; Banking must retry, durably reject the unaffordable new hold and publish only the fresh outcome. |
| Unit tests | 50 passed, zero skipped or failed, including exact-cent residual allocation, stable ties, partial/final reconciliation and large-value overflow prevention. |
| Required API build | Passed after the final financial correction: zero errors; analyzer warnings remain. |
| Architecture guardrails | 93 passed after the final source changes, including the four Banking/Loans provider model-snapshot checks. |
| Blazor build | Passed with zero errors; member and operator pages compile. |
| Repository formatter | Full changed-source and subsequent incremental passes completed; zero files need formatting in the final incremental pass. |

Reproduce the checks from the repository root. The focused run repeats affected cases after the exact-cent allocation correction; its count is not additional distinct scenario coverage.

```powershell
dotnet build src/Backend/Api/LS.Api/LS.Api.csproj --no-restore -p:UseSharedCompilation=false
dotnet test tests/LS.Tests.Architecture/LS.Tests.Architecture.csproj --no-restore -p:UseSharedCompilation=false
dotnet test tests/LS.Tests.Unit/LS.Tests.Unit.csproj --no-restore -p:UseSharedCompilation=false -p:RunAnalyzers=false
dotnet test tests/LS.Tests.Integration/LS.Tests.Integration.csproj --no-restore --filter '(FullyQualifiedName~GuarantorReservationTests&Category!=ExternalRabbitMq)|FullyQualifiedName~Phase2PersistenceTests|FullyQualifiedName~Phase6ValidationTests|FullyQualifiedName~SaccoOperationAuthorizationTests|FullyQualifiedName~UnitOfWorkRegistrationTests' -p:UseSharedCompilation=false -p:RunAnalyzers=false
dotnet test tests/LS.Tests.Integration/LS.Tests.Integration.csproj --no-restore --filter 'FullyQualifiedName~GuarantorReservationTests&(FullyQualifiedName~recovery|FullyQualifiedName~Forced_same_snapshot)' -p:UseSharedCompilation=false -p:RunAnalyzers=false
./scripts/test-local-messaging.ps1 -SkipStart
```

| Acceptance scenarios | Automated coverage and limits |
| --- | --- |
| G01–G06 | Confirmed capacity, concurrent duplicate delivery, replay after committed Banking state, changed identity details, competing pledges and forced same-snapshot withdrawal. Separate rollback injection proves no partial hold/receipt/outbox. |
| G07 | Durable rejection for missing/foreign-member accounts, insufficient capacity and unexplained locks; Application consent checks active ownership and verified membership. Staged inactive-member and account journeys remain acceptance work. |
| G08–G11 | Cancel before reserve delivery, terminal release, both expiry/commit orderings, delayed commit outcome, replay, settlement release and worker restart. This is controlled fault simulation, not a production host crash rehearsal. |
| G12–G13 | Exact per-pledge recovery, replay, debt earmarking and competing approval checks; dedicated balanced internal-offset journal and missing/wrong mapping exceptions. Finance must reconcile a real recovery to its source books. |
| G14 | Same pledge/operation identities under a foreign tenant cannot mutate the original account; member ownership and endpoint/route policies are tested. Real role assignments and cross-tenant database placement require staged acceptance. |
| G15 | Unexplained funds block new reservations; approved legacy adoption explains existing locks without increment, including closed-loan release. No real legacy extract has been imported or signed off. |
| G16–G17 | Competing credit approvals include undisbursed commitments; approved deposit policy is captured; changed displayed terms are rejected and old consent snapshots remain unchanged. |
| G18 | Durable pending/confirmed/rejected/released states are covered in Application and represented in compiled member/operator views. Signed-in browser usability and displayed-state acceptance remain open. |

## Migrations and activation runbook

Four separate migrations preserve existing history:

| Context | Migration |
| --- | --- |
| Banking SQL Server | `Migrations/Banking/20261123235728_GuarantorReservationsAndRecovery` |
| Banking PostgreSQL | `Migrations/BankingPostgreSqlDB/20261123235834_GuarantorReservationsAndRecovery` |
| Loans SQL Server | `Migrations/Loans/20261123235911_ConfirmedGuarantorSecurity` |
| Loans PostgreSQL | `Migrations/LoansPostgreSqlDB/20261123235942_ConfirmedGuarantorSecurity` |

The new savings eligibility flags default to false. Schema installation does not approve a production product or infer the meaning of existing `LockedFunds`. Existing loan product rows receive reference policy defaults for development; Credit must explicitly review those values before activation. No existing pledge is silently promoted to confirmed security.

Deploy the updated API and member UI together. Consent now requires the exact pledge ID, selected account, explicit terms acceptance and displayed version. Older callers that omit these fields receive validation failure and must refresh/adopt the new contract. Existing stored enum values are preserved; new states are appended and historical `Accepted` means legacy review is required.

1. Keep live intake closed. Take and verify the approved backup; identify the tenant's stamp, provider and Banking/Loans targets. Stop the old unkeyed guarantee writer before deploying the new consumer path. Apply the appropriate migration for each owning context through the normal manual migration process.
2. Inventory each existing `LockedFunds` amount against source pledges and the old message backlog. The former `GuarantorAcceptedIntegrationEvent` consumer now quarantines unkeyed instructions in the broker error path. Do not blindly replay them or manually increment balances.
3. Use hold reconciliation to identify differences. For each source-supported legacy pledge, prepare an adoption review against its actual member account, attach an evidence reference and obtain independent approval. For closed loans, verify the resulting release. Leave unmatched amounts blocked and owned by Finance/Credit investigation.
4. Credit and Banking approve the participating savings products, the credit multiple, consent window, hold window and policy version. Configure savings security eligibility with an evidence reference. Confirm actual member verification, role assignments and liability terms before new consent.
5. Finance configures and signs `GUARANTOR_RECOVERY`: `EXPLICIT_GL` debit to the approved member-deposit liability and `EXPLICIT_GL` credit to the approved loan-principal asset. Test a recovery and tie the retained savings transactions, loan repayment reference and balanced journal together. A valid account type alone does not prove the correct product/control account.
6. Verify outbox workers, broker queues and the expiry job on the intended stamp. For pending operations, reconcile the source operation ID, Banking receipt and current reservation revision before replay. Reuse the same operation identity and payload. Never assign a fresh identity merely to bypass a conflict.
7. Rehearse member consent, delayed/rejected confirmation, independent approval, disbursement, cancellation, settlement, substitution and default recovery with actual staff/member roles in a representative browser session. Record screens, audit actors, expected balances and exception handling.
8. Obtain Credit, Banking, Finance and release-owner approval before live intake. Keep unverified differences and accounting exceptions visible. Use forward correction and approved recovery for financial data; do not run `Down` migrations against issued reservations or recovery history.

## Staged acceptance on the local stack (24 November 2026)

Executed against the containerised API and Blazor app on PostgreSQL 17 with RabbitMQ, Redis and Quartz, using the fictional Jamii Reference SACCO (`scripts/seed-reference-acceptance.ps1`). Signed-in staff and members used their own accounts and roles; the admin enrolled in TOTP as required for System Administrators. Results are rehearsal evidence on a local stack, not production acceptance.

| Scenario | How it was exercised | Result |
| --- | --- | --- |
| G18 consent → confirmed hold | Guarantor signed in, reviewed terms `LLANSACCO-REFERENCE/0.1`, chose the eligible account and consented in the browser | Pending (0 held) → "Funds reserved" (100,000 held) after Banking confirmed through the outbox and RabbitMQ. One reservation and the matching `LockedFunds` in the database |
| G14 member-to-member isolation | Unrelated member signed in; then called the consent API on another guarantor's pledge three ways (posing as that guarantor with their account, with her own account, and as herself) | No pledges visible; all three refused (404 "Guarantor not found"); no reservation or lock created |
| Approval and disbursement | Credit checker approved on `/loans/applications` and requested disbursement on the guarantees page | `DisbursementPending` until Banking committed both holds, then `Disbursed`, both pledges `Committed`, 24 schedule rows, every outbox drained. The expiry job only selects `Held` reservations, so committed security cannot expire |
| G15 legacy reconciliation (rehearsal) | Simulated an import: a disbursed loan with a 30,000 legacy hold, a closed loan with a 20,000 legacy hold, and an unexplained 15,000 lock with no pledge | Reconciliation listed exactly 30,000 / 20,000 / 15,000. A new guarantee on the unexplained account was durably rejected with no lock change. Maker self-approval of adoption was refused ("An independent security approver is required"); checker approval adopted the open-loan hold without incrementing (130,000 held = 130,000 explained) and released the closed-loan hold (20,000 → 0). The unexplained 15,000 remains visible and blocking |
| Backup, restore and worker recovery | Consent recorded, API stopped with the reserve instruction staged in the Loans outbox; `pg_dump`, database dropped and recreated, `pg_restore`; API restarted, then restarted again | The staged message survived the restore and was delivered once: one `Held` reservation, `LockedFunds` +15,000 exactly once, outboxes drained, no change after the second restart |

Defects found and fixed during this acceptance:

- Every `[Authorize]` Blazor page (31) returned HTTP 500 on a direct link or refresh. The components endpoint now allows the anonymous shell request and `AuthorizeRouteView` authorizes inside the circuit, as in StarterTemplate.
- Credit staff could not approve loans: `/loans/applications` required the platform permission `ControlPlane.Manage`. It now requires `applications.view`, shows Approve/Reject only for `applications.approve`, uses the SACCO layout, and is covered by the route/API permission test.
- Member onboarding and BOSA postings rejected every request: string fields were validated with `IsInEnum()`, which fails for all strings. They now accept exactly what the handler's `ToEnum` converts (unit tested).
- Local Azurite had no `profile-images` container, so storage readiness failed until the first upload. The local setup now creates it, mirroring `ops/azure/modules/storage.bicep`.

Findings recorded for follow-up (not fixed here):

- Users created through `POST api/v1/iam/admin/users` stay inactive and unconfirmed, and no API activates a non-employee user, so members cannot reach the portal. The rehearsal activated the fictional `@jamii.example` users directly in the local database.
- There is no API or UI to configure `TransactionTypeGlMapping`, and no `GUARANTOR_RECOVERY` mapping is seeded, so default recovery cannot post its journal. The recovery and source-to-ledger tie-out were therefore not executed.
- Banking's rejection reason for an account with unexplained locks is generic ("ineligible, insufficient available funds, or expired") and does not name the unexplained difference.
- Members land on the staff dashboard; the backing-account selector shows an empty identifier before selection; statuses and identifiers appear as raw codes and GUIDs on staff pages.
- Signing the same user in elsewhere ends the existing browser session.
- `GetLookupRequestValidator` validates a request no handler consumes.
- Deployed environments do not yet provision `MediatR--LicenseKey` in Key Vault.

## Open acceptance gates

- Signed-in browser acceptance of cancellation before disbursement, settlement release, substitution and default recovery; desktop and mobile viewports; KYC state changes and tenant switching. Consent, member isolation, approval, disbursement, legacy adoption and recovery after restore passed on the local stack (above).
- First-SACCO credit/security policy and enforceable consent/notice evidence, eligible products and real KYC route. Simulated KYC remains a separate production blocker tracked in Phase 6.
- Actual legacy-book reconciliation with the first SACCO's records (the reference rehearsal passed), a way to configure and approve the `GUARANTOR_RECOVERY` mapping, and a signed recovery/source-to-ledger tie-out.
- Backup/restore and incident recovery on a representative deployed tenant/stamp with agreed recovery objectives. The local restore rehearsal passed; it does not prove production recovery objectives.

The global local-endpoint audit reported existing port collisions across other project configurations, including a configured Mailpit overlap. Existing infrastructure was reused with `-SkipStart`; no application host or unrelated infrastructure was started or reconfigured. Resolve the relevant allocation before starting additional local hosts/services.

The required CI checks and implementation review closed with the merge of PR #13.

The Phase 1 builds reported known vulnerable transitive packages. A follow-up change pins the patched versions directly: `System.Security.Cryptography.Pkcs` 10.0.15 (via `Microsoft.Data.SqlClient`), `Microsoft.AspNetCore.WebUtilities` 2.3.14 (via `Microsoft.AspNetCore.Diagnostics`) and `System.Formats.Cbor` 10.0.15 (via `Fido2`). `dotnet list LlanSacco.slnx package --vulnerable --include-transitive` then reports no vulnerable packages. Analyzer warnings remain.
