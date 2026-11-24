# LlanSacco Reference SACCO Operating Profile

**Status:** Reference development baseline v0.1, 24 November 2026. The user authorized proceeding with the reference approach. For product design, synthetic data, development and acceptance preparation. No SACCO has approved it for production.

**Purpose:** Give the first SACCO a concrete operating proposal to review and amend. Use this with the [production readiness delivery plan](production-readiness-delivery-plan.md), [acceptance pack](reference-sacco-acceptance-pack.md) and [synthetic fixtures](reference-sacco-acceptance-fixtures.json). It completes the authorized Phase 0 development baseline. First-SACCO approval and implementation/production evidence are separate gates; none is inferred from the reference values.

## 1. How to use this profile

Each rule has one of three meanings:

- **Reference control:** Proposed LlanSacco behaviour that protects financial integrity, such as balanced journals, tenant isolation and duplicate-payment prevention.
- **Illustrative policy:** A complete sample business choice for development and testing. It is neither a statutory requirement nor a claim about what all Kenyan SACCOs do.
- **Confirmation required:** A real-world fact, legal applicability decision, approval or commercial arrangement that the SACCO must supply before activating the affected production workflow.

Copy the reference into a named tenant's review record. For each item, record Accepted, Amended or Not applicable, the replacement value where needed, the approving authority, supporting document and effective date. An unresolved mandatory item remains pending. A local sample value must never silently become production policy.

The approach supports all six workstreams in the delivery plan. It supplies working assumptions while business owners prepare the real evidence. Complete and review each implementation phase before starting its dependent phase.

## 2. Organization and product baseline

| Item | Proposed baseline | Production confirmation |
| --- | --- | --- |
| Jurisdiction, currency and business time | Kenya; KES; Africa/Nairobi | Legal identity, registrations and any additional jurisdictions/currencies |
| Financial year | 1 January to 31 December | Registered financial year and accounting policy |
| Initial membership | Adult individual members; active, verified membership needed for borrowing and guaranteeing | Common bond, admission rules, minimum age and any entity, diaspora or minor-member products |
| Product categories | Separate share capital, non-withdrawable deposits, withdrawable savings and loans | Approved products and permission to offer each; a software module does not establish regulatory authorization |
| Regulatory profile | Unclassified until evidenced | DT-SACCO, regulated specified NWDT-SACCO, or another applicable classification confirmed by Compliance |
| Initial channels | Staff operations and responsive member web portal; M-Pesa collections and approved payouts | Actual channel list and provider acceptance; native mobile and USSD need their own approval and security acceptance |
| Member identity | One authenticated IAM actor linked to the active membership in the current tenant | Identity-link remediation and rules for staff who are also members |
| Financial authorization | Independent maker and checker for disbursement, reversal, recovery, policy changes and payout approval | Named roles, delegated authorities, limits and treatment of related parties |
| Cutover | No sample date or infrastructure placement is assumed | Sponsor, cutoff instant, tenant/stamp, database provider, migration source and release owner |

FOSA and withdrawable products remain disabled for a tenant until its authorization and product scope are confirmed. Do not infer permission from the presence of Banking screens.

## 3. Reporting pack and definitions

### 3.1 Proposed operational and management reports

These are draft report specifications. Finance supplies approved layouts and sample control totals.

| Report | Minimum content | Tie-out and proposed operating frequency |
| --- | --- | --- |
| Trial balance | Account code/name, opening balance, period debits/credits, closing balance, period and posting status | Debits equal credits; daily and at close |
| Financial position | Assets, liabilities and equity with approved account mappings and comparatives | Assets equal liabilities plus equity; monthly |
| Income statement and cash flow | Income/expense detail; cash movement classifications; period and comparatives | Income agrees to the ledger; closing cash agrees to cash/bank controls; monthly |
| Bank and M-Pesa reconciliation | Opening/closing balances, receipts, fees, settlements, reversals, unmatched items and aging | Provider, bank, clearing account and member postings agree or have explained timing differences; daily |
| Member balances by product | Member/account, product, opening balance, posted movements, closing balance | Shares, deposits, savings and loan subledgers agree to their control accounts; daily/monthly |
| Loan book and arrears | Original and outstanding principal, interest separately, due dates, days past due, arrears, security and related-party flags | Loan control accounts and repayment schedules; daily/monthly |
| Credit risk and provisioning | Agreed classification buckets, exposure, security treatment, rate and calculated provision | Approved prudential/accounting rules and journal totals; monthly or required frequency |
| Guarantor exposure and recovery | Borrower, guarantor, pledge/reservation IDs, product, held amount, available coverage, age, release/recovery and exceptions | Reservation records agree to blocked balances; recoveries agree to member and ledger entries; daily |
| Dividend distribution | Approved calculation version, eligible basis, rate, gross allocation, tax, net, destination, payment status and unclaimed amount | Approved allocation equals paid/credited plus outstanding liability; per run |
| Member statements | Product-specific opening balance, dated transactions/references, reversals and closing balance; loan principal/interest/charges separately | Posted subledger and GL controls; on demand and agreed statement cycle |
| Control and exception pack | Suspense aging, unposted events, uncertain payments, manual corrections, closed-period attempts and policy changes | Named owner, reason, evidence and target resolution date; daily |
| Complaints and service pack | Case reference, category, dates, owner, resolution and overdue status | Case register and applicable reporting requirements; monthly |

For arrears, distinguish overdue principal from total unpaid installments. Define portfolio-at-risk numerator, denominator, thresholds and treatment of rescheduling before presenting a percentage. Regulatory provisioning must use the applicable approved rules rather than a guessed set of bands.

Every report/export should identify tenant, business period, as-of instant and time zone, currency, filters, generation time, source/rule version and responsible reviewer where applicable. Avoid unnecessary member identifiers in broadly distributed management reports.

### 3.2 Regulatory reporting profiles

SASRA publishes separate [DTS returns](https://www.sasra.go.ke/dts-regulatory-return-forms/) and [NW-DTS returns](https://www.sasra.go.ke/nw-dts-regulatory-returns-forms/). Its DTS catalogue includes capital adequacy, liquidity, risk classification/provisioning, investments, financial position and comprehensive income. These are starting sources for template discovery, not proof that every form applies to a tenant.

Maintain separate reporting profiles for DT, regulated specified NWDT and any other confirmed category. Do not select one by default in production or combine their statutory forms into a generic export.

For each applicable return, the review record must identify:

- Official template and version, authority, applicable legal basis and reporting entity.
- Reporting period, frequency, deadline, submission route and authorized submitter.
- Field definitions, source queries, account/product mappings and rounding/unit conventions.
- Reconciliation checks, approved sample output, reviewer and submission acknowledgment.

Keep calculation definitions separate from the presentation template so a revised form can reuse reviewed financial data. A PDF or spreadsheet is ready for filing only after the SACCO approves its applicability and contents.

## 4. Illustrative credit policy

These values make the reference testable. They must be replaced or explicitly approved before a live loan product is enabled.

| Rule | Illustrative choice |
| --- | --- |
| Membership seasoning | Six completed months of active membership |
| Contribution history | Required scheduled contributions up to date; no unexplained arrears |
| Deposit multiple | Maximum principal exposure after the new loan: 3 times eligible non-withdrawable deposits, including outstanding principal and approved undisbursed commitments |
| Absolute product limit | KES 1,000,000; the lower eligibility/affordability/security limit still applies |
| Interest | 12% nominal annually on monthly reducing principal; monthly rate 1%; equal monthly installments |
| Term | Up to 36 monthly installments; first due date one calendar month after disbursement |
| Schedule details | Preserve end-of-month dates, no holiday adjustment in the sample, first-period interest one monthly period; round posted amounts to KES 0.01 and reconcile the final installment |
| Fees and penalties | Zero in the reference example; production must explicitly approve any charges and their disclosure |
| Affordability | Total monthly contractual debt service at most 40% of verified net monthly income; external obligations included without double counting |
| Security | Full principal coverage before disbursement using eligible own-deposit liens and/or accepted guarantor reservations |
| Guarantors | At least two distinct eligible guarantors where any guarantor-backed amount is required; no self-guaranteeing |
| Guarantor capacity | At most 1:1 coverage against eligible balances after existing liens, reservations and other encumbrances |
| Approval | Credit prepares; independent authorized checker approves; no self-approval or approval of one's own related-party transaction |
| Repayment allocation | Oldest due installment first: contractual interest then principal; surplus reduces principal, retains the installment and shortens the term; no prepayment fee in the reference |
| Pending application | Unaccepted requests expire after 14 calendar days; accepted pre-disbursement reservations after 30 calendar days unless an approved extension is recorded |
| Post-disbursement coverage | Keep the agreed principal reservation until full settlement, approved substitution or authorized recovery; no automatic time expiry |
| Rescheduling and write-off | Separate approved actions with original history preserved; never silently restart arrears aging |

The 40%, 3-times multiple, rates, limits and dates above are sample choices, not legal thresholds. Include employer deduction limits, other credit obligations, product-specific exceptions, death/exit treatment and related-party rules in the SACCO review. Do not enable a product with unresolved charge, interest or repayment-allocation rules.

**Worked eligibility example:** A borrower has KES 200,000 eligible deposits and KES 100,000 existing outstanding principal. The sample multiple allows at most KES 500,000 additional principal: 3 × 200,000 − 100,000. A KES 300,000 request can pass that check but still needs affordability and security approval. If no own deposits are pledged, two guarantors reserving KES 150,000 each provide the required KES 300,000 coverage. Each guarantor must independently have that capacity.

### 4.1 Guarantor reservation and recovery controls

- A request is not a hold. The guarantor must accept a specific borrower, loan, amount, currency, liability terms and expiry. Retain consent evidence.
- Banking authoritatively reserves eligible funds once, using a stable tenant-and-pledge identity. Duplicate delivery returns the same outcome. Withdrawals, other guarantees, transfers and account exit must respect the same encumbrance rules.
- A reservation records an encumbrance; it does not itself pay the borrower, debit the guarantor or recognize income. Keep total eligible balances, blocked amounts and withdrawable amounts distinct.
- Rejection, pre-disbursement cancellation and expiry release only the applicable reservation. Once disbursed, cancellation cannot free the security. Resolve reserve/release races and uncertain outcomes before reporting funds as available.
- The sample retains original coverage through the loan's life. Proportional release can be an explicitly approved alternative, with recalculation and consent where required; it must not change existing guarantees retroactively.
- Default recovery requires the approved notice process, enforceable guarantee terms and independent authorization. Record evidence, the amount recovered, corresponding loan/member postings, and remaining liability. Do not treat arrears alone as authority to debit a guarantor.
- For multiple guarantors, propose proportional recovery against their remaining committed amounts, capped by legally recoverable balances and total recoverable debt. Credit/Compliance must approve the method and any charges covered.
- Reconcile existing holds before migration. Every difference has an owner and resolution; never invent a pledge or erase a balance to force agreement.

**Acceptance example:** A guarantor has KES 200,000 eligible funds and KES 50,000 existing encumbrances. A KES 100,000 reservation succeeds once and leaves KES 50,000 eligible unencumbered capacity. A competing KES 80,000 eligible withdrawal cannot also succeed. A duplicate message does not increase the hold. For a non-withdrawable product, ordinary withdrawal remains prohibited independently of the guarantee.

## 5. Finance and close policy

Propose daily cash/provider reconciliations and monthly accounting close. Finance approves the chart of accounts, product mappings, accounting basis, interest recognition, impairment, fees, tax and suspense treatment before posting real balances.

- Import opening balances at an agreed cutoff using stable source references. Reconcile member-level balances and accrued components to source books and control accounts. Do not count imported balances again as current-period receipts.
- Posted financial history is preserved. Correct through an approved linked reversal and replacement where appropriate, including all affected subledgers.
- A maker cannot approve their own reversal, opening import or close. Record actors, reason and evidence.
- Close requires balanced books, completed control reconciliations and disposition of outstanding exceptions. Authorized timing differences remain visible with owners; an unexplained difference cannot be labelled reconciled.
- Posting into a closed period is rejected. Reopen requires specific authority and an audit record; use a next-open-period adjustment where the approved policy requires it.
- Approve reports against a fixed sample book, including original transactions, reversals and opening/closing movements. UI, PDF and Excel must agree.

## 6. Dividend and deposit-return policy

Separate distributions on share capital from returns on eligible deposits. Finance must confirm the legal/accounting treatment, distributable surplus, reserves, prudential constraints, authority and applicable tax rules. A sample rate does not establish an entitlement or authorize payment.

| Item | Illustrative choice |
| --- | --- |
| Distribution period | Approved financial year |
| Eligibility | Historical eligible share/deposit holdings during the period; membership exit/death, delinquency and account restrictions require explicit rules |
| Calculation basis | Daily closing eligible balances, summed and divided by actual calendar days in the financial year |
| Rates | Separate approved annual rates for share capital and each eligible deposit product |
| Sample rates | 10% on share capital and 8% on eligible deposits, solely for worked examples |
| Precision | High-precision decimal calculation; round each member share/deposit component to KES 0.01 using midpoint-to-even, then sum the rounded components |
| Rounding difference | Report the difference between rounded member totals and the rounded aggregate calculation; Finance approves its accounting treatment |
| Approval | Freeze inputs and calculations; checker approves the exact version under the SACCO's confirmed governance/AGM authority |
| Destination | Proposed default: eligible member deposit account; external bank/M-Pesa only through an approved, verified destination |
| Tax | Explicit approved treatment by payment type and recipient; no assumed zero tax and no universal sample tax rate |
| Failed or unclaimed allocations | Remain traceable outstanding liabilities with reason, age and follow-up; no automatic write-back to income |
| Offset against a loan | Disabled unless the approved terms and applicable law permit it, with required authority/consent and separate postings |

**Worked allocation:** A member holds KES 100,000 eligible shares and KES 200,000 eligible deposits unchanged for the full year. At the sample rates, gross share distribution is KES 10,000 and gross deposit return is KES 16,000. Gross total is KES 26,000. Net payable is gross less the applicable approved deductions; do not infer a tax rate from this example.

The year includes 365 or 366 days as appropriate. A new holding participates only for its eligible days. Eligibility basis, rate, source balances and rule version must reproduce the same result. Do not accidentally include a previously credited distribution in its own calculation base.

A rate or input change creates a new calculation version and requires renewed approval. Interrupted payouts resume by stable allocation identity. An uncertain external payout remains pending investigation/status verification rather than being sent again as a new payment. Review statutory unclaimed-assets handling and its deadlines with Compliance before defining transfers or write-backs.

## 7. Payment and reconciliation routes

### 7.1 Proposed routes

| Purpose | Reference route | Before live use |
| --- | --- | --- |
| Member savings/deposit receipt | SACCO-controlled Paybill collection and STK initiation through an approved Daraja arrangement | Merchant/shortcode ownership, enabled products, contracts, callbacks and actual production acceptance |
| Loan repayment | Same approved collection rail, with a specific loan/payment intent or controlled allocation rule | How members select the loan and how ambiguous references are resolved |
| Loan disbursement/dividend payout | Verified internal account credit, or approved bank/M-Pesa B2C rail | Product authority, destination verification, funding, limits, fees and payout approvals |
| Refund/reversal | Controlled workflow linked to the original receipt | Provider procedure, accounting effects, authority and final confirmation |
| Provider outage | Pending/exception queue and safe reconciliation recovery | Support contacts, escalation and reconciliation ownership |

Safaricom's [Daraja portal](https://developer.safaricom.co.ke/) is the official integration starting point. Commercial terms and enabled services must be confirmed for the actual account; see also [Safaricom's M-PESA services terms](https://www.safaricom.co.ke/images/images/Downloads/M_PRESA_Services.pdf). This reference selects an integration direction, not a provider contract or guarantee of API access.

### 7.2 Financial controls

- Resolve tenant from the approved merchant/account configuration, and member/product from the payment intent or validated reference. Phone number alone is insufficient to establish member ownership.
- Distinguish initiated, provider-confirmed, member-posted and bank-settled states. A submitted STK request is not proof of payment.
- Duplicate callbacks, imports and retries cannot create extra member credits, repayments or journals. A reversal is a separate linked financial event.
- Receipt posting ordinarily debits the approved M-Pesa clearing/control asset and credits the appropriate member liability or loan/interest balances. Finance approves the exact entries, including suspense and fee treatment.
- Settlement moves the clearing amount to bank and records provider fees separately under the approved arrangement. Match gross/net differences explicitly.
- Unmatched receipts go to an approved suspense workflow. Do not guess a member, silently discard money or mark it reconciled because the callback was received.
- Reconcile provider statements, internal receipt records, member postings, ledger control accounts and bank settlement daily. Retain timing differences, approvals and evidence.
- Changes to payout destinations need verified ownership and independent review where required. Neither a member phone change nor a provider callback alone authorizes a new payout destination.

## 8. KYC and member onboarding route

### 8.1 Proposed initial model

Use a documented staff-assisted verification workflow as a candidate initial route, subject to Compliance approving its evidence and legality for the selected products. Add an authorized external verification adapter where the SACCO has a valid service agreement. Provider automation must preserve review and exception handling.

Manual review does not mean accepting an uploaded ID as independently verified. The reviewer must use approved evidence and authorized verification sources, record the checks actually performed and escalate any check that cannot be completed. Do not claim access to government identity services or credit bureaus without authorization.

### 8.2 Proposed information and checks

| Area | Proposed scope |
| --- | --- |
| Identity | Legal name, accepted identification type/number, date of birth and verification evidence; handle document discrepancies |
| Contact | Verified phone/email as applicable and address; contact verification is distinct from identity verification |
| Membership | Common-bond eligibility, application, declarations and admission decision |
| Financial purpose | Occupation/business, intended account use and source of funds, proportionate to risk |
| Tax | KRA PIN or other tax details only where required for the approved product/obligations |
| AML/CFT | Risk assessment, applicable sanctions/PEP screening, escalation and enhanced due diligence |
| Beneficiary/next of kin | Details required by the membership policy, with appropriate purpose, access and lawful processing |
| Non-individual members | Separate scope for legal existence, beneficial ownership and authorized signatories before enabling them |
| Evidence governance | Lawful basis, privacy notice, access limits, retention/deletion schedule, provider arrangements and legal holds |

FRC publishes [AML/CFT legislation and guidance](https://www.frc.go.ke/?page_id=231) and [risk-assessment guidance](https://www.frc.go.ke/wp-content/uploads/2025/03/TF-Risk-Assessment-Guidance-Feb.-2025.pdf). Compliance must translate applicable obligations into the tenant's screening, monitoring and reporting procedures. Identity matching alone does not complete due diligence.

Follow the [ODPC data-protection policy guidance](https://odpc.go.ke/wp-content/uploads/2026/07/ODPC-%E2%80%93-Guidance-Note-for-Data-Protection-Policies.pdf) when documenting how personal data is processed. Do not treat consent as the universal lawful basis, collect biometrics by default, or keep documents indefinitely.

### 8.3 Review and activation controls

Use explicit Draft, Submitted, Under review, More information required, Approved, Rejected and Suspended/Review due outcomes, with verification status separate from membership status and login access.

A failed or unavailable verification route leaves the application pending/referred. No simulator or timeout can approve production KYC. Approval records identify reviewer, evidence, risk decision, policy version and time. A materially changed identity or risk profile triggers review under the approved policy.

For the sample, propose annual review for higher-risk members and every three years for standard-risk members, plus event-driven reviews. These intervals are illustrative and must be replaced by Compliance's approved risk-based schedule. Restrict financial activation until required CDD is complete. Any legal reporting/restriction process must be handled by authorized Compliance staff without inappropriate disclosure to members.

## 9. Versioning and implementation decisions

This is a target design for the six delivery workstreams, not a statement that these capabilities are implemented.

- Keep the operating profile tenant-scoped and product rules versioned with effective dates. Record who proposed and approved a change.
- Preserve policy snapshots on loan offers, guarantees, dividend runs and relevant financial approvals. A new policy affects new activity unless a separately approved migration explicitly changes existing contracts.
- Keep business policy in the owning bounded context: Loans for credit, Banking for eligible balances/reservations, Accounting for mappings/close, Dividends for allocation rules, and Membership/IAM for onboarding and ownership. Reuse their existing contracts and Unit of Work flows.
- Keep provider credentials and deployment options in approved host configuration/secrets; keep financial policy changes in auditable business workflows. Do not store credentials in this profile.
- Start with typed supported choices and reviewed calculations. Avoid an arbitrary formula language or general-purpose rules engine until actual SACCO variations justify one.
- Make critical integrity controls non-optional: balanced postings, tenant isolation, independent authorization where required, stable transaction identities and preservation of posted history.
- Prove UI/API enforcement and recovery, including both supported database providers where applicable. A configuration screen or passing unit test alone does not establish production readiness.

## 10. First-SACCO amendment and approval register

Every item below is open. Add actual names, decisions, document references and dates to the SACCO's signed record.

| ID | Decision/evidence | Accountable owner | Status |
| --- | --- | --- | --- |
| OP-01 | Legal identity, regulatory classification, authorization and permitted products/channels | Sponsor and Compliance | Pending |
| OP-02 | Report catalogue, current official templates, definitions, deadlines, mappings and sample totals | Finance and Compliance | Pending |
| OP-03 | Credit pricing, eligibility, affordability, fees, schedules, limits and related-party policy | Credit authority | Pending |
| OP-04 | Eligible security, consent, hold/release, expiry, substitution, notices and guarantor recovery | Credit, Banking and Compliance | Pending |
| OP-05 | Accounting policy, opening books, reversal, period close and exception approval | Finance | Pending |
| OP-06 | Distribution authority, eligible basis, rates, reserves, tax, offsets and unclaimed-assets treatment | Finance and governing authority | Pending |
| OP-07 | Merchant/provider contracts, credentials, callbacks, settlement statements, payout routes and limits | Payment account owner and Finance | Pending |
| OP-08 | KYC sources/providers, manual evidence, AML/CFT procedures, privacy and retention | Compliance and data-protection owner | Pending |
| OP-09 | Statements, member journeys, complaints and authorized communications | Member Services and Finance | Pending |
| OP-10 | Cutover, recovery objectives, restore rehearsal, training, support and release approval | Sponsor and Operations | Pending |

Development can use this profile immediately with synthetic records. Production activation requires the applicable decisions, implemented controls and actual acceptance evidence from the delivery plan. Keep each affected phase open until those exist.
