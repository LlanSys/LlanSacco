# Reference SACCO Acceptance Pack

**Status:** Development preparation baseline, 24 November 2026. The user authorized proceeding with the reference approach. Business approval and production acceptance remain pending. Phase 1 automated application scenarios are now being executed; see the [implementation evidence record](production-readiness-phase-1-verification.md). Staged browser acceptance and business sign-off remain open.

Use this pack with the [operating profile](reference-sacco-operating-profile.md), [machine-readable synthetic fixtures](reference-sacco-acceptance-fixtures.json) and [production readiness delivery plan](production-readiness-delivery-plan.md).

## 1. Development baseline and phase boundary

Use reference profile LLANSACCO-REFERENCE v0.1 for synthetic development and acceptance preparation. Its sample values are explicit design choices, not evidence of a real SACCO's policies. Production classification, filing templates, tax treatment, merchant/provider access, verification authority, deployment placement and cutover remain unresolved until supported by the first SACCO's evidence.

The user accepted the Phase 0 reference development baseline and explicitly authorized Phase 1. First-SACCO approvals remain production activation gates. Complete and review Phase 1 before starting Phase 2, retaining all unverified evidence as open. Preparation of later-phase scenarios does not start their implementation.

The JSON is a documentation fixture, not application configuration, a migration, a production tenant seed or an implemented policy API. Monetary values and ratios are decimal strings. Symbolic IDs identify synthetic records only. Reset each scenario family independently; do not mix the finance, credit, guarantor and dividend portfolios.

The first-SACCO review uses OP-01 through OP-10 in the operating profile. Each decision needs the actual tenant, proposed/replacement value, reviewer, authority, source document, effective date and Accepted/Amended/Not applicable decision. Chat authorization to develop does not fill those business approvals.

## 2. Reviewed source and first implementation gap

The following observations describe the source **before Phase 1 implementation** on 24 November 2026. They explain the original defects. Current behaviour and executed evidence are recorded in the [Phase 1 verification](production-readiness-phase-1-verification.md).

| Entry point | Observed behaviour | Phase 1 implication |
| --- | --- | --- |
| [AcceptGuarantorPledge](../../src/Backend/Application/LS.Application/Features/Loans/LoanApplications/Commands/AcceptGuarantorPledge.cs) | Checks linked member ownership, sets Accepted and LockedAmount, then publishes through MediatR before CompleteAsync | Consent and a requested amount must remain pending until an authoritative Banking outcome is durable; use the established context-owned outbox |
| [Banking guarantor consumer](../../src/Backend/Infrastructure/LS.Infrastructure/Messaging/Consumers/Banking/GuarantorAcceptedIntegrationEventHandler.cs) | Selects the first savings account, increments LockedFunds and returns when no account exists | Resolve the specific eligible account, persist one reservation and a durable accepted/rejected outcome; absence of an account cannot look like confirmed coverage |
| [SavingsAccount](../../src/Backend/Domain/LS.Domain/Features/Banking/Savings/Entities/SavingsAccount.cs) | Available balance is balance minus minimum balance minus LockedFunds | Every writer affecting availability must participate in the same concurrency rule; include withdrawal and account lifecycle paths |
| [DisburseLoan](../../src/Backend/Application/LS.Application/Features/Loans/LoanApplications/Commands/DisburseLoan.cs) | Requires Approved status, generates schedules and publishes a disbursement event; this handler contains no confirmed-reservation check | A guarantor-backed loan must not disburse on loan status alone; prove coverage and close the expiry/cancellation race |
| [LoanProduct](../../src/Backend/Domain/LS.Domain/Features/Loans/Entities/LoanProduct.cs) | Stores rate, interest method, maximum term and amount | Existing fields are not an implemented versioned operating profile; preserve prior contract terms when policy versioning is introduced |

Do not infer whole-system production readiness from these observations. Review current code again when implementation starts. Preserve existing member ownership checks, tenant activation, response contracts, migration history and user work.

## 3. Reference rules clarified for deterministic acceptance

The operating profile supplies the main policy. Use these additional sample decisions consistently:

- Deposit-multiple capacity includes outstanding principal plus approved undisbursed commitments. Otherwise two simultaneous approvals could each consume the same headroom. A pending commitment must have a durable lifecycle and release when validly cancelled.
- The sample loan uses monthly reducing interest, midpoint-to-even rounding to two decimals, unchanged installments after a principal prepayment and a shortened term. There is no sample prepayment fee. Reconcile the last installment to the remaining principal and interest.
- Security must be confirmed before disbursement. Pre-disbursement expiry and disbursement authorization must resolve to one outcome. After Banking commits security for an authorized disbursement, an elapsed pre-disbursement deadline cannot release it.
- Recovery examples cover principal only. Charges and future interest are not silently added to the guaranteed amount. Approved recovery reduces held funds and member balance together and reduces the loan by the corresponding principal amount.
- The sample retains remaining original coverage after partial repayment/recovery until approved settlement, substitution or release. Excess coverage is reported, not silently freed.
- Dividend allocations round each member's share and deposit component separately, then sum the rounded components. A different rounding order requires a new policy version and refreshed examples.
- Supplied dividend withholding amounts in the fixture are arbitrary test inputs. They exercise netting and liability separation without asserting a legal rate.

These are reference choices to review with the SACCO; they do not override existing contracts.

## 4. Synthetic finance book

Use the JSON journals as the source for these expected outputs. The period is 1 December 2026 inclusive to 1 January 2027 exclusive, Africa/Nairobi. The book is shown before any income/expense closing-transfer journal. Account labels are reference mappings, not the approved production chart of accounts.

### 4.1 Opening balances

| Debit account | Amount KES | Credit account | Amount KES |
| --- | ---: | --- | ---: |
| Bank | 500,000.00 | Member savings | 250,000.00 |
| Cash | 20,000.00 | Member non-withdrawable deposits | 400,000.00 |
| Loan principal | 300,000.00 | Share capital | 100,000.00 |
| | | Retained surplus | 70,000.00 |
| **Total** | **820,000.00** | **Total** | **820,000.00** |

The member schedule in the fixture reconciles shares, savings, deposits and loan principal to these control balances.

### 4.2 Posted movements

| Reference | Event | Expected financial effect |
| --- | --- | --- |
| SYN-R01 | M01 pays KES 10,000 to savings | Debit M-Pesa clearing; credit member savings |
| SYN-S01 | Settle SYN-R01 with KES 50 fee | Debit bank 9,950 and fees 50; credit clearing 10,000 |
| SYN-R02 | M01 pays KES 12,000 on the loan | Debit clearing 12,000; credit principal 10,000 and interest income 2,000 |
| SYN-S02 | Settle SYN-R02 without a fee | Debit bank 12,000; credit clearing 12,000 |
| SYN-ERR01 | Erroneous KES 5,000 cash deposit to M02 | Debit cash; credit member savings; preserve the original |
| SYN-REV01 | Approved full reversal of SYN-ERR01 | Debit member savings; credit cash; link to the original |

SYN-R02 assumes a KES 2,000 due-interest component and KES 10,000 principal allocation supplied by this synthetic loan schedule. It is separate from the new-loan amortization example. Production accrued-interest and fee recognition require Finance-approved mappings.

### 4.3 Expected closing outputs

| Output | Expected KES |
| --- | ---: |
| Bank | 521,950.00 |
| Cash | 20,000.00 |
| Loan principal | 290,000.00 |
| M-Pesa clearing | 0.00 |
| Member savings | 260,000.00 |
| Member deposits | 400,000.00 |
| Share capital | 100,000.00 |
| Retained surplus before closing transfer | 70,000.00 |
| Interest income | 2,000.00 |
| Payment fee expense | 50.00 |
| Period surplus | 1,950.00 |
| Trial balance debits and credits, each | 832,000.00 |
| Assets and liabilities-plus-equity, each | 831,950.00 |
| Net cash movement | 21,950.00 |

Seven journals, including opening, have total debits and credits of KES 874,000 each. The closing trial balance includes expense and income accounts; the balance sheet incorporates the KES 1,950 period result. These are different presentations of the same balanced book.

SYN-R01 has a KES 10,000 clearing asset before settlement, even though the member is already credited. Do not report it as bank-settled on 2 December. By the final cutoff, provider gross receipts of KES 22,000 reconcile to KES 21,950 in bank and KES 50 in fees.

### 4.4 Expected member statements

| Member | Savings opening | Posted savings movements | Savings closing | Loan principal opening | Principal repayment | Principal closing |
| --- | ---: | --- | ---: | ---: | ---: | ---: |
| SYN-M01 | 100,000.00 | +10,000.00 | 110,000.00 | 100,000.00 | 10,000.00 | 90,000.00 |
| SYN-M02 | 90,000.00 | +5,000.00 and linked -5,000.00 reversal | 90,000.00 | 120,000.00 | 0.00 | 120,000.00 |
| SYN-M03 | 60,000.00 | None | 60,000.00 | 80,000.00 | 0.00 | 80,000.00 |
| **Total** | **250,000.00** | **Net +10,000.00** | **260,000.00** | **300,000.00** | **10,000.00** | **290,000.00** |

M01's loan statement separately shows KES 2,000 interest paid. Its total payment is KES 12,000, not KES 10,000. Share and deposit statements retain their opening/closing balances with no movements in this book.

## 5. Phase 1 guarantor acceptance specification

### 5.1 Proposed authoritative lifecycle

Loans owns consent and the loan/pledge lifecycle. Banking owns the reservation and eligible balance.

1. A linked guarantor records consent against the specific pledge, account/product scope, amount, currency and policy version. Loans stages its pending request in the same transaction as the durable dispatch intent.
2. Banking atomically validates eligibility/capacity and records Held or Rejected with a stable tenant-and-pledge identity and durable outcome. Loans reflects that outcome rather than declaring success before Banking confirms it.
3. A matching repeated request returns the recorded result. A changed account, amount, currency or owning loan under the same identity is a conflict. Reject without modifying existing funds.
4. Before disbursement, Banking commits the relevant holds for disbursement through an authoritative transition that is serialized against expiry/release. Loans must durably receive the required coverage confirmation before authorizing its money movement.
5. Once committed for disbursement, uncertainty or a failed acknowledgment goes to reconciliation. A timer must not release security while a disbursement might complete. An aborted disbursement releases only after authoritative cancellation evidence.
6. Rejection, cancellation and expiry before commitment release once. Store a terminal cancellation/release decision even when the reserve request has not yet arrived, so delayed delivery cannot resurrect it.
7. After disbursement, held funds remain until the approved lifecycle permits settlement release, substitution or recovery. A repeated release/recovery instruction cannot repeat its financial effect.

Use stable instruction identities for individual recovery/adjustment operations as well as the reservation identity. A reservation can have several authorized partial recoveries; each instruction executes once. Audit all state changes and keep messages recoverable through the owning outbox.

An implementation may use different internal state names. The visible outcomes and financial invariants above must remain testable. Do not implement a distributed database transaction between contexts.

### 5.2 Required scenarios

Automated execution uses real Units of Work, separate connections/scopes, both providers and local RabbitMQ. See the [scenario evidence and remaining acceptance](production-readiness-phase-1-verification.md) for precise coverage. Automated proofs do not establish browser usability, first-SACCO approval or production deployment.

| ID | Action | Expected result/evidence |
| --- | --- | --- |
| G01 | Reserve KES 100,000 from KES 200,000 eligible balance with KES 50,000 prior encumbrance | One new active hold; total blocked 150,000; remaining capacity 50,000; no cash/GL movement |
| G02 | Deliver G01 twice concurrently, then replay after success | One reservation and one funds effect; matching durable outcome |
| G03 | Crash after Banking commits but before acknowledgment; restart/replay | Hold stays 100,000; Loans eventually receives the same authoritative outcome |
| G04 | Submit different amount/account/currency under G01's identity | Typed conflict; original reservation unchanged |
| G05 | Race G01 with an otherwise permitted KES 80,000 withdrawal | If hold wins, withdrawal fails; if withdrawal wins, hold fails with capacity 70,000; both cannot succeed |
| G06 | Race two different KES 100,000 pledges for the original KES 150,000 capacity | At most one succeeds; no negative capacity |
| G07 | No eligible account, inactive member/account or insufficient capacity | Durable rejected outcome; Loans does not treat the pledge as confirmed |
| G08 | Cancel before reserve delivery; then deliver reserve and release repeatedly | Terminal released/cancelled decision; no resurrected hold |
| G09 | Expire at the deadline while disbursement is being authorized | One serialized outcome: blocked disbursement or committed security; never disbursed with expired security |
| G10 | Lose the disbursement-commit acknowledgment | Security remains encumbered while unresolved; reconcile and resume without double disbursement |
| G11 | Settle loan and deliver release repeatedly/out of order | Each active reservation releases once; stale reserve cannot recreate it |
| G12 | Recover KES 100,000 principal against two KES 150,000 commitments | Authorized KES 50,000 debit each; remaining holds 100,000 each; remaining loan principal 100,000; balanced traceable postings |
| G13 | Retry G12 and race two recoveries against the same debt | Each authorized instruction executes once; total recovery cannot exceed approved recoverable debt or held/eligible funds |
| G14 | Tenant B reuses Tenant A's symbolic pledge/member labels; member A accesses member B | Independent tenant records; unauthorized access cannot read or change another member's pledge |
| G15 | Import legacy LockedFunds that do not match identified active pledges | Exception report and blocked activation for unresolved differences; no silent balance correction |
| G16 | Simultaneous approvals consume the borrower's deposit-multiple headroom | Existing principal plus approved undisbursed commitments stay within the approved limit |
| G17 | Change credit/guarantor policy after consent/approval | Existing transaction retains its approved terms; a new policy cannot silently lower its security or change liability |
| G18 | Read API, member UI and operator recovery view during delayed delivery | All show pending/confirmed/rejected/released consistently; pending is never labelled confirmed |

### 5.3 Implementation order and evidence

Implement within Phase 1 in this order, with one integrated acceptance review:

1. Agree the reviewed lifecycle and contract identities; inventory every availability writer and existing-hold source.
2. Add the retained reservation model/configuration/repository through the Banking Unit of Work, exact uniqueness/concurrency rules and separate provider migrations. Prove upgrades as well as clean databases.
3. Use the established owning-context outbox for requests and outcomes; preserve tenant activation and durable replay.
4. Integrate Loans consent, confirmed coverage, disbursement commitment and rejection. Enforce member ownership through the existing API/Application boundary.
5. Complete release, expiry, substitution, authorized recovery and legacy reconciliation with audit and operational repair.
6. Deliver staff/member pending and failure states, guarantor exposure/recovery outputs and authorized recovery controls.
7. Execute G01-G18 on both providers; record broker recovery, browser evidence, Finance/Credit acceptance and CI results.

Changes to settings, validators, cache policy, permissions and entity lifecycle registers must follow the existing repository standards. Do not hide debt by expanding architecture exceptions. Keep the phase open until its required evidence exists.

## 6. Acceptance scenarios for subsequent phases

These scenarios are preparation. They do not bypass the delivery sequence or establish implemented behaviour.

| Phase / IDs | Scenarios | Expected result |
| --- | --- | --- |
| Finance F01-F03 | Import the opening book; repeat/import after interruption; compare member controls and trial balance | One opening effect per source row; KES 820,000 opening debits/credits; matching subledgers |
| Finance F04-F06 | Post the six movements; reverse the error twice; race post with period close | Seven journals total; closing totals above; one reversal effect; no posting sneaks into the closed period |
| Finance F07-F08 | Produce financial reports; attempt self-approved correction/reopen | Reports reconcile; unauthorized approval refused; authorized corrections retain history |
| M-Pesa P01-P03 | Receive SYN-R01/R02 through approved test routes; duplicate/reorder callbacks; interrupt member posting | Provider receipt, member posting and GL trace once; recover pending posting without crediting twice |
| M-Pesa P04-P06 | Import settlements repeatedly; inspect pre-settlement cutoff; process unknown/mismatched receipt | 22,000 gross = 21,950 bank + 50 fees; interim clearing remains visible; unmatched receipt has controlled suspense ownership |
| M-Pesa P07-P08 | Lose payout callback; receive a provider reversal after member funds are spent | Query/reconcile uncertain payment before another payout; authorized reversal/receivable workflow preserves the original and balanced books |
| Dividends D01-D03 | Full-year balances, mid-year change and leap-year holding | Fixture component totals reproduce exactly; the 2024 leap case produces KES 8,387.98 |
| Dividends D04-D05 | Change source data/rate after approval; repeat calculation | Changed inputs require a new version and approval; identical snapshot/rules produce identical allocations |
| Dividends D06-D08 | Pay first allocation; interrupt second; resume/replay and review outstanding items | First net 25,268.75 paid once; second net 11,328.93 pending until confirmed; separate tax liability 955.75; no automatic write-back |
| Statements S01-S03 | View and export each product; apply business-period boundaries; inspect reversal | UI/PDF/Excel reconcile; both error and reversal remain visible; loan principal and interest stay separate |
| Statements S04-S05 | Request another member/tenant's export; page a large account then export | Ownership enforced at API/download time; full export totals agree with authoritative period totals |
| Member access M01-M03 | Submit identity evidence; request more information; provider unavailable/rejected | Recorded review outcome; financial activation stays pending until required checks are approved |
| Member access M04-M06 | Use simulator in production; deactivate membership with active session; staff/member role coexistence | Simulator cannot approve production KYC; current membership/permissions enforced; staff privileges do not establish member ownership |
| Cutover C01-C03 | Restore, migrate and import; replay pending work; reconcile opening and closing books | Counts and control totals agree; no duplicate financial effects; recovery objectives evidenced |
| Cutover C04-C05 | Suspend intake after live writes; recover provider/broker outage | Preserve accepted financial history and reconcile external outcomes before resumption; trained operators use the runbook |

For each grouped ID range, record separate execution results for each named action. Sandbox evidence does not replace controlled production-provider evidence. Failure, cancellation, delayed delivery and independent-tenant execution are part of acceptance.

## 7. Dividend worked outputs

Use daily closing eligible balances and actual days in the financial year.

| Member / case | Share gross | Deposit gross | Total gross | Supplied test withholding | Net |
| --- | ---: | ---: | ---: | ---: | ---: |
| SYN-D01, unchanged full-year holdings | 10,000.00 | 16,000.00 | 26,000.00 | 731.25 | 25,268.75 |
| SYN-D02, holdings change on 1 July 2025 | 7,520.55 | 4,032.88 | 11,553.43 | 224.50 | 11,328.93 |
| **Total** | **17,520.55** | **20,032.88** | **37,553.43** | **955.75** | **36,597.68** |

SYN-D02 has 181 days at KES 50,000 shares and 184 days at KES 100,000 shares. Its deposit balance is zero for the first 181 days and KES 100,000 for the remaining 184. The independent leap case has KES 100,000 eligible shares from 29 February 2024 through year-end: 307 of 366 days, producing KES 8,387.98 at the sample share rate.

Withholding amounts above are supplied arithmetic inputs only. They intentionally do not define a tax rate. After withholding is reclassified to tax payable and the first net allocation is paid, the outstanding dividend liability is KES 11,328.93 and the separate tax payable is KES 955.75. Approval, credit/payment and remittance of tax are distinct events.

## 8. Evidence record and present status

For each executed scenario record scenario ID, profile/rule version, implementation commit, environment/tenant placement, database provider, actor roles, inputs, expected/actual result, trace/source references, test or browser artifacts, reviewer, date and unresolved findings. Keep private member/provider evidence in approved restricted storage, not in this repository.

| Item | Present status |
| --- | --- |
| Reference operating rules and worked acceptance data | Prepared; authorized for reference development use |
| Arithmetic, internal fixture consistency and documentation links | Passed locally on 24 November 2026 using decimal arithmetic: balanced journals and account/member controls, settlement, dividend/calendar totals, credit examples, race arithmetic, statement boundary and local links |
| Application scenario execution G/F/P/D/S/M/C | Not run by this documentation change |
| Runtime policy versioning and baseline configuration | Not implemented by this pack |
| First-SACCO OP-01 through OP-10 | Pending business evidence and approval |
| Production phases 0-7 | Open; no phase is closed by the reference preparation |

No real provider credentials, member records, merchant numbers or identity documents belong in the synthetic fixtures.
