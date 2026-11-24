# Production readiness Phase 1 — Guarantor-backed lending

**Phase status: OPEN**, 24 November 2026. Core implementation and automated verification are available for review. Browser execution, representative tenant/recovery verification, actual-book reconciliation and review/sign-off remain unfinished. Phase 2 has not started.

**Implementation commit:** `f1d1060` on `codex/production-readiness-phase-1`, based on `main` at `2bd4bd4`. The local verification below covers that source tree.

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

## Open acceptance gates

- Required CI and implementation review on the PR. All local checks above have passed; CI evidence is recorded separately.
- Signed-in browser acceptance on desktop/mobile with actual tenant roles, current terms, member ownership, KYC state changes and tenant switching. No browser acceptance is claimed by a successful build.
- First-SACCO credit/security policy and enforceable consent/notice evidence, eligible products and real KYC route. Simulated KYC remains a separate production blocker tracked in Phase 6.
- Actual legacy-book reconciliation, correct Finance-approved product/control-account mapping, and signed recovery/source-to-ledger tie-out.
- Representative deployed tenant/stamp database routing, backup/restore and incident recovery rehearsal. Local worker restart is evidence for durable processing, not proof of production recovery objectives.

The global local-endpoint audit reported existing port collisions across other project configurations, including a configured Mailpit overlap. Existing infrastructure was reused with `-SkipStart`; no application host or unrelated infrastructure was started or reconfigured. Resolve the relevant allocation before starting additional local hosts/services.

Builds currently report existing dependency vulnerability/analyzer warnings, including `System.Security.Cryptography.Pkcs` and `Microsoft.AspNetCore.WebUtilities`. This phase does not upgrade those dependencies. Record their remediation separately before release acceptance.
