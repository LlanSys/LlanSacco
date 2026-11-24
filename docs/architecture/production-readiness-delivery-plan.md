# LlanSacco Production Readiness Delivery Plan

**Status** — Phases 0 and 1 are OPEN, 24 November 2026. Reference preparation is complete and Phase 1 core implementation has automated evidence. Neither phase has met its full completion checklist.

Architecture remediation established the platform and CI guardrails. Product release depends on evidence that a SACCO can operate the financial workflows, reconcile the books, recover from failure, and support its members. This plan covers guarantor reservations, finance acceptance, M-Pesa, automated dividends, statements, and self-service. Fixing every named workflow is in scope. The first SACCO's approved operating profile determines which gates must pass before its first live transaction.

## What the repository and reference material show

| Area | Present | Evidence still needed |
| --- | --- | --- |
| Guarantor-backed lending | Loans records consent and an owning-context outbox instruction; Banking owns retained reservations and durable outcomes. | Finish the [Phase 1 evidence record](production-readiness-phase-1-verification.md), staged browser acceptance, legacy reconciliation and first-SACCO sign-off. |
| Finance | Chart of accounts, journal posting, trial balance, balance sheet, and income statement provide the starting point. | Opening balances that reconcile to the source books, controlled reversals and close, and signed acceptance of reporting outputs. |
| M-Pesa | Provider adapters, callbacks, and a status reconciliation worker exist. The payment-completed event handler leaves business posting as a TODO. | Traceable member credit or repayment, settlement matching, an exception workflow, and verification with the first SACCO's provider setup. |
| Dividends | Declaration commands advance the lifecycle. Calculation and posting commands still have TODOs for the actual jobs. | Reproducible allocations, approval, posting, payment confirmation, and recovery of a partially completed batch. |
| Member access | IAM, membership links, and some member-owned endpoints exist. Infrastructure currently registers a simulated KYC orchestrator. | Production identity verification, complete member statements, and signed-in acceptance of the agreed portal journeys. |

- The financial production gap assessment calls for append-only accounting, controls for competing money movements, verified tenant isolation, and separate identity hardening for mobile or USSD channels. Use these as design and test inputs for the existing Banking, Accounting, and IAM contexts. [Financial production gaps](../reference/Critical%20Gaps%20to%20Address%20for%20Financial%20Production.docx)
- The requirements document adds guarantor exposure and recovery reports, dividend payment schedules and unclaimed dividends, statements by product, bank reconciliation, cash flow, an online member portal, migration, training, and disaster recovery. It uses two SACCO names and covers a wider ERP procurement scope. Confirm which reports, channels, and operating requirements apply to the first SACCO. [SACCO system requirements](../reference/SACCO%20system%20requirements.docx)
- Onboarding describes resolving an existing identity before linking roles and isolating external KYC providers behind adapters. The expert review gives architecture context. Use actual signed-in, data migration, provider, and recovery evidence for release acceptance. [Onboarding reference](../reference/OnBoarding.md) · [Expert review](../reference/expert-review-report.md)

## Delivery sequence

Phases 0 and 1 remain open against their full checklists. Reference preparation is complete. On 24 November 2026 the user authorized Phase 1 development using that reference; this allowed implementation to proceed without closing Phase 0's outstanding SACCO decisions or approvals. Complete and review each implementation phase before starting the next. Keep unverified evidence open and retain a PR and acceptance record for each phase. The plan uses the existing LlanSacco checkout and bounded contexts.

| Phase | Deliverable | Acceptance owner |
| --- | --- | --- |
| 0 | First SACCO release scope, operating rules, and evidence baseline | SACCO sponsor with Finance, Credit, Compliance, and Operations |
| 1 | Retry-safe guarantor reserve, confirm, release, and recovery | Credit and Banking operations |
| 2 | Reconciled opening balances, financial corrections, close, and reporting | Finance and Compliance |
| 3 | M-Pesa member posting, settlement reconciliation, and provider acceptance | Finance, Payments operations, and the provider account owner |
| 4 | Automated dividend calculation, approval, posting, and payout | Finance and the SACCO's dividend approval authority |
| 5 | Member statements and authorized exports | Finance and Member Services |
| 6 | Complete and verified member self-service | Member Services, Compliance, and IAM owner |
| 7 | Cutover rehearsal, support handover, and release approval | SACCO sponsor, Finance, and Operations |

### Phase 0 — Agree the first SACCO release

**OPEN:** The completed reference profile and synthetic acceptance pack are preparation. All four first-SACCO checklist items below remain outstanding; authorization to develop against the reference does not complete them.

Use the [reference SACCO operating profile](reference-sacco-operating-profile.md) as the proposed starting point for reports, credit and guarantor policy, dividends, payment routes and KYC. Its sample values are the authorized Phase 0 development baseline. The SACCO must accept or amend them and supply approvals and production evidence before production activation. The development baseline is complete; OP-01 through OP-10 remain business gates.

- [ ] Record the first SACCO, license category and applicable filing templates, tenant/stamp and database placement, products, supported channels, roles, approval limits, and cutover date and time zone. Obtain sample report layouts and control totals. Have Finance and Compliance sign the report list, definitions, and submission formats. [SASRA DTS returns](https://www.sasra.go.ke/dts-regulatory-return-forms/) · [SASRA NW-DTS returns](https://www.sasra.go.ke/nw-dts-regulatory-returns-forms/)
- [ ] Approve the credit policy for eligible guarantor balances, expiry, cancellation, adjustment, settlement, legacy holds, and default recovery. Approve dividend eligibility, rates, rounding, withholding as applicable, unclaimed balances, and payout destinations.
- [ ] Give Finance and Operations owners for the legacy source extracts, chart and product mappings, payment credentials and statements, complaint handling, and business acceptance. Use sanitized, realistic samples for dev and staging. Identify the production KYC route and required verification evidence.
- [ ] Record the first SACCO's day-one scope and acceptance scenarios. Keep every named workstream in the product plan. Any narrower rollout requires explicit sponsor agreement, approved operating controls, and disabled access to unfinished workflows. Include B2C or a bank payout rail in the critical path when the approved workflows require it. Track native mobile, USSD, additional bank integrations, and wider ERP requests in product scope when the sponsor confirms them.

Preparation is available in the [reference acceptance pack](reference-sacco-acceptance-pack.md) and its [synthetic fixtures](reference-sacco-acceptance-fixtures.json). They define reconciled sample books, expected results and the Phase 1 reservation lifecycle. The user authorized use of the reference approach for development on 24 November 2026. First-SACCO decisions OP-01 through OP-10 and all production phase gates remain open.

### Phase 1 — Make guarantor reservations replay-safe

**OPEN:** The first two implementation items below are checked. The remaining items combine completed lifecycle code and automated proofs with unfinished browser execution, representative tenant/database recovery verification, actual legacy-book reconciliation and review/sign-off. They remain unchecked until all their required evidence exists. This includes execution work, not only business signatures.

Implementation and automated evidence are tracked in the [Phase 1 verification and operator runbook](production-readiness-phase-1-verification.md). Keep this phase open for review and the unverified production gates listed there; Phase 2 has not started.

- [x] Let Loans own the pledge and Banking own the reservation. Record the pending pledge and its durable request in the same Loans commit. Store a Banking reservation with a stable tenant-and-pledge identity, eligible account, amount, currency, and outcome. Enforce uniqueness in the database. A matching replay returns the recorded outcome and a mismatched replay returns a typed conflict.
- [x] Atomically check available funds and commit the reservation and its durable outcome in Banking. All competing balance writers must respect the same availability rule. Confirm the pledge from the Banking outcome. Require confirmed coverage before the backed loan can progress past the agreed credit decision point.
- [ ] Implement release for the approved lifecycle, using a replay-safe transition for the same reservation. Define how an in-flight acceptance races with cancellation, how default recovery consumes held funds, and how an operator resolves failures. Reconcile legacy `LockedFunds` to active pledges and traceable adjustments before enabling the new flow. Escalate unmatched balances for Finance review.
- [ ] Prove the lifecycle with the real unit of work on PostgreSQL and SQL Server. Deliver the same message twice concurrently, fail after Banking commits and before acknowledgment, restart and replay, deliver release and reserve out of order, and race separate pledges against each other and withdrawals. Verify one durable hold, one release, no unauthorized overdraw, correct loan state, and correct tenant activation. Exercise the configured broker and recovery from a tenant database. Reuse the existing outbox recovery approach. [Phase 2 verification](phase-2-verification.md)

### Phase 2 — Reconcile the books and prove financial controls

- [ ] Reuse the Accounting ledger and Banking and Loans source transactions. Inventory all posting paths for first-SACCO products. Require balanced debit and credit entries with stable source references and auditable actors, and detect an outstanding posting across contexts. Agree account mappings for cash, bank, member deposits, shares, loans, and clearing accounts before imports or new payments are enabled.
- [ ] Implement a repeatable opening-balance import and dry-run with a cutoff, source row identifiers, versioned mappings, exception output, and authorized execution. Reconcile member-level shares, deposits and loan balances to control accounts, bank and cash totals to supporting schedules, and the imported trial balance to the approved source books. Require a balanced journal and no unexplained control difference. Document any authorized suspense items with owner, reason, and clearance date. Prove rerun and interrupted-batch recovery without duplicate journals.
- [ ] Post compensating journals linked to originals for approved corrections. Preserve the historical record and require applicable independent approval, reason, and linked subledger effects. Handle retries and concurrent approvals with a single authorized effect. Exercise teller, member and general-ledger reversal paths used at launch.
- [ ] Implement period controls for posting, close, and permitted reopen or next-period correction. Make posting and close safe under concurrency. Use the approved close checklist, including pending postings, suspense aging, and bank and member control reconciliations. Record actor, decision, reporting date, and as-of report totals.
- [ ] Have Finance sign agreed report outputs against sample books and opening/closing evidence. Cover trial balance, balance sheet, income statement, cash flow, bank reconciliation, and applicable returns. Include relevant credit reports such as arrears, provisioning, guarantor exposure and recoveries when in the signed report list. Validate ledger tie-outs, period filters, permissions, formats, and supported exports. Exercise actual close and reversal scenarios as Finance staff.

### Phase 3 — Post and reconcile M-Pesa payments

- [ ] Define the supported inbound routes and business intents for savings deposits and loan repayments. Use approved merchant and callback routing to resolve tenant and account. Validate the provider evidence, receipt, amount, currency, and expected reference before posting. Capture the callback or verified status durably and retain traceable evidence with restricted access.
- [ ] Record provider receipts and posting intent with uniqueness in the database. Use a commit-owned outbox and idempotent Banking and Loans consumers to post once, with a corresponding Accounting entry. Keep payment confirmation, member posting, and settlement as explicit states. Handle duplicate, late, missing, mismatched, failed and reversed payments. Send unmatched items to a controlled suspense and exception workflow with approval and audit.
- [ ] Reconcile provider receipts and statements to member postings, control accounts, and settlement bank entries, including fees and approved timing differences. Give Finance an exception queue, safe replay, and evidence of how an exception was resolved. Verify repeat imports, partial settlement, duplicate receipts and controlled reversals. Use provider status queries to recover uncertain transactions and settlement records to prove reconciliation.
- [ ] Exercise both database providers and the configured messaging transport. Verify sandbox and authorized controlled live scenarios for the routes used at launch. Obtain provider credentials through the approved configuration, verify callback setup and ownership, and reconcile the approved live transactions with Finance. For B2C, prove payout confirmation, retries, and exception handling whenever it is an approved payout rail. [Safaricom Daraja](https://developer.safaricom.co.ke/) · [Transaction Status API](https://developer.safaricom.co.ke/apis/TransactionStatus)

### Phase 4 — Complete automated dividends

- [ ] Implement a tenant-scoped, versioned calculation against approved as-of membership and historical share and savings balances. Capture calculation inputs, rules, eligible dates, rounding, withholding, and totals. Compare per-member allocations and aggregate liabilities with a Finance-approved worked example. Replay the calculation to produce identical results from the same inputs.
- [ ] Persist the calculation work request and status with durable dispatch. Advance to calculated only after verified allocations exist. Approve the exact result version and record the authority and time. Amend through a controlled new version with traceable changes.
- [ ] Post the dividend liability and each member distribution using stable identities and the approved destinations. Track internal account posting, external payout confirmation, failed and unclaimed items, and approved correction paths. Resume interrupted jobs and partial batches without paying the same member twice. Make completion depend on the confirmed state of all allocations, with outstanding liabilities visible.
- [ ] Prove deterministic results, competing requests, worker restart, partial failure, and balanced financial effects on both providers. Verify statement and GL tie-outs, operator recovery, and any external payout rail in the agreed scenario before live distribution.

### Phase 5 — Deliver member statements

- [ ] Generate savings and deposit, share capital, and loan statements from posted activity with an authoritative opening balance, signed movements, reversals, and closing balance. Document product-specific totals, dates, and outstanding loan amounts. Show pending activity clearly when offered alongside a statement. Reconcile representative periods back to the member account and Finance totals.
- [ ] Deliver member-authorized views and downloadable PDF and Excel with equivalent amounts and filters. Provide permitted staff access with audit. Verify date boundaries, paging and export totals, historical data and reversals, large accounts, private delivery, and access to the correct member and tenant.
- [ ] Run browser and export acceptance against staged data for each product, with Member Services and Finance sign-off.

### Phase 6 — Complete member self-service

- [ ] Complete enrollment, identity link, secure sign-in, MFA and recovery in accordance with the approved member policy. Verify KYC through the approved real provider or auditable manual workflow, including retry, rejection, and handoff. Prevent simulated KYC from approving production membership. Prove role coexistence for a staff member who is also a SACCO member.
- [ ] Deliver the responsive Blazor journeys for balances and activity, authorized profile updates and documents, PDF and Excel statements, loan application and progress, and guarantor requests and responses. Include payment and receipt tracking when members use the approved payment channel. Deliver approved notifications and a support path for failures. Exercise the API permissions independently of visible menus and routes.
- [ ] Run signed-in desktop and mobile-browser acceptance with actual tenant role assignments. Exercise member-to-member and cross-tenant isolation, signed-out routes, deactivated membership, permission and tenant switching, safe replay of writes, and recovery from provider errors. Verify that notification and download access follows current permission and identity.

### Phase 7 — Prove cutover and release readiness

- [ ] Rehearse the first SACCO cutover in a representative environment. Restore a backup, run migrations and opening imports, verify record counts and control totals, and test rollback or forward recovery with approved recovery objectives. Preserve posted transactions when recovering after live writes. Rehearse a pre-open hold and post-open intake suspension and recovery.
- [ ] Prove the production target's deployment configuration, managed credentials and access, limits, alert routing, and storage and database isolation. Exercise backlog replay, unavailable providers, reconciliation breaks, and support escalation. Protect member documents, credentials, identifiers and provider payloads under agreed retention and access controls. [ODPC data protection policy guidance](https://odpc.go.ke/wp-content/uploads/2026/07/ODPC-%E2%80%93-Guidance-Note-for-Data-Protection-Policies.pdf)
- [ ] Train staff on opening, posting, reversal, close, exceptions, and incident response. Deliver the operator and cutover runbooks, ownership and escalation details, approved starting balances, and signed acceptance evidence. Open live intake after the release owner and SACCO sign the required product and operational gates.

## Evidence required to close a phase

- Changes are available through the production entry points used by members and staff. UI, API, persistent state, background delivery, and operators agree about what succeeded, failed, and is pending.
- Apply the repository standards for feature ownership, typed outcomes, validators, permission checks, tenant-scoped jobs and caches, Unit of Work commits, and separate provider migrations. Reuse proven StarterTemplate platform patterns where applicable and preserve LlanSacco data and identities. [Repository standards](../../AGENTS.md)
- Record the implementation commit, required CI result, database-provider and tenant-placement test results where relevant, and any real broker or provider evidence. Prove migration from existing data as well as clean installs where schema changes are needed. Use staged browser and provider evidence for workflows that depend on them.
- Keep evidence of cancellation, competing requests, duplicate and delayed delivery, restart and recovery, durable audit and source-to-ledger reconciliation for the affected money paths. Record the applicable business sign-off and any blocking evidence still outstanding. Leave the affected gate open until the evidence exists.
