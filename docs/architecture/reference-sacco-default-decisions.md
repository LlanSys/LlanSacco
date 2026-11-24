# Reference SACCO Default Decisions (OP-01 to OP-10)

**Status:** Reference defaults v0.1, 24 November 2026. These decisions are recorded for **Jamii Reference SACCO**, a fictional SACCO. It exists only to certify LlanSacco features end to end. It is not a real organization, it has no registration, and nothing here is legal, tax or regulatory advice.

Use this record with the [operating profile](reference-sacco-operating-profile.md), the [acceptance pack](reference-sacco-acceptance-pack.md) and the [delivery plan](production-readiness-delivery-plan.md).

## How these defaults are used

- **They close Phase 0 for the reference tenant only.** Development, automated tests, local and staging browser acceptance, and demos use these values as decided.
- **They are the platform default and placeholder for every new tenant.** A new tenant starts from this record, and each item is shown as "Reference default" until that tenant records Accepted, Amended or Not applicable with its own approver, document and effective date.
- **They never authorize production.** A real tenant cannot go live on a reference default. Production activation still requires that tenant's own OP-01 to OP-10 decisions and the production evidence listed in the delivery plan.
- The defaults follow practice common among Kenyan deposit-taking and BOSA SACCOs. Where a value depends on law or tax, it is a marked placeholder that the tenant's advisors must confirm.

## Register

| ID | Reference default for Jamii Reference SACCO | Status (reference tenant) | Status (real tenants) |
| --- | --- | --- | --- |
| OP-01 | See [OP-01](#op-01-identity-classification-products-and-channels) | Decided (fictional) | Tenant decision required |
| OP-02 | See [OP-02](#op-02-reports) | Decided (fictional) | Tenant decision required |
| OP-03 | See [OP-03](#op-03-credit-policy) | Decided (fictional) | Tenant decision required |
| OP-04 | See [OP-04](#op-04-guarantor-security) | Decided (fictional) | Tenant decision required |
| OP-05 | See [OP-05](#op-05-accounting-and-close) | Decided (fictional) | Tenant decision required |
| OP-06 | See [OP-06](#op-06-dividends-and-interest-on-deposits) | Decided (fictional) | Tenant decision required |
| OP-07 | See [OP-07](#op-07-payments) | Decided (fictional) | Tenant decision required |
| OP-08 | See [OP-08](#op-08-kyc-amlcft-and-privacy) | Decided (fictional) | Tenant decision required |
| OP-09 | See [OP-09](#op-09-member-services) | Decided (fictional) | Tenant decision required |
| OP-10 | See [OP-10](#op-10-cutover-recovery-and-release) | Decided (fictional) | Tenant decision required |

Approver for every reference item: LlanSacco product owner, acting as the fictional sponsor. Source: this document.

## OP-01 Identity, classification, products and channels

- Fictional society: Jamii Reference SACCO Society Ltd, tenant code `JAMII-REF`, currency KES, time zone Africa/Nairobi, financial year January to December.
- Classification: non-withdrawable deposit-taking (BOSA) SACCO, so the reference uses the SASRA NW-DTS reporting profile. FOSA and withdrawable products stay disabled.
- Products:
  - Share capital: minimum KES 5,000, non-refundable except by transfer on exit.
  - Non-withdrawable member deposits: minimum monthly contribution KES 1,000.
  - Development loan: guarantor-backed.
  - Emergency loan: same rules, KES 100,000 cap, 12 months.
- Channels: staff web, responsive member web portal, and M-Pesa Paybill collections. Native mobile, USSD and B2C payouts are out of the reference scope.
- Roles: Credit officer (maker), Credit manager (checker), Teller, Finance officer, Finance manager, Compliance officer, Member. No one approves their own transaction.

## OP-02 Reports

- Catalogue: every report in section 3.1 of the operating profile.
- Regulatory profile: SASRA NW-DTS returns, generated from the reference synthetic book. The reference outputs are test artefacts, not filings.
- Frequency: daily trial balance and M-Pesa reconciliation; monthly financial statements, loan book, arrears and guarantor exposure; quarterly regulatory pack.
- Sample control totals: the finance book in the acceptance pack fixtures.

## OP-03 Credit policy

- Section 4 of the operating profile is adopted as written: six months' membership, 3x deposits, KES 1,000,000 product cap, 12% a year reducing balance, 36 months, 40% affordability.
- Fees: zero.
- Arrears classification follows common SACCO practice:
  - Performing: 0 days past due.
  - Watch: 1–30 days.
  - Substandard: 31–180 days.
  - Doubtful: 181–360 days.
  - Loss: over 360 days.

  These bands are placeholders until the tenant's prudential rules are confirmed.
- Related parties: board members, staff and their close relations need approval from the credit committee, excluding the conflicted person, and the related-party flag is recorded.

## OP-04 Guarantor security

- Section 4.1 of the operating profile is adopted as written, matching what Phase 1 implemented:
  - At least two guarantors and full principal coverage.
  - Only non-withdrawable deposits are eligible.
  - 1:1 capacity.
  - 14-day consent window and 30-day pre-disbursement hold.
  - Hold retained until settlement.
- Default recovery:
  - The loan is classified defaulted at 90 days past due.
  - Guarantors get written notice at least 30 days before recovery.
  - Recovery is proportional to remaining committed amounts and covers principal only.
  - It needs a maker and an independent checker.
- Substitution: the replacement guarantor must consent and be held before the original is released.

## OP-05 Accounting and close

- Basis: accrual accounting, IFRS-style presentation. The chart of accounts and product mappings are the reference mappings in the acceptance pack fixtures.
- `GUARANTOR_RECOVERY` mapping: debit member non-withdrawable deposits (liability), credit loan principal (asset).
- Opening balances: imported at month-end cutoff 23:59 Africa/Nairobi from the reference book, with a dry run first.
- Corrections: linked reversals only, maker-checker.
- Close: monthly, by the 10th business day, with the checklist in section 5 of the operating profile. Suspense items older than 30 days are escalated to the Finance manager.

## OP-06 Dividends and interest on deposits

- Authority: AGM resolution on the board's recommendation.
- Rates for worked examples: 10% on share capital, 8% interest on deposits. Calculation basis and rounding as in section 6 of the operating profile.
- Withholding tax placeholder: 5% on both components, held as a separate liability. The tenant's tax advisor must confirm the actual treatment.
- Destination: credited to the member's deposit account. There are no external payouts in the reference.
- Unclaimed amounts stay as an aged liability. Transfer under unclaimed-assets law is a tenant compliance decision.
- No offset against loans.

## OP-07 Payments

- Collections: M-Pesa Paybill with STK Push, using the Daraja sandbox only. Account reference format is `<member number>` for deposits and `<member number>-L<loan number>` for repayments.
- Unmatched receipts go to the M-Pesa suspense account and are reviewed by Finance within 2 business days.
- Settlement: daily sweep from the M-Pesa clearing account to bank, with fees recorded separately.
- B2C and bank payouts are disabled. No production credentials exist for the reference tenant.

## OP-08 KYC, AML/CFT and privacy

- Route: staff-assisted manual verification with an approved checklist. A reviewer verifies:
  - National ID or passport.
  - KRA PIN.
  - Phone and email.
  - Physical address.
  - Next of kin.
  - Occupation and source of funds.
- Simulated KYC may run in development only. It can never approve a member in a production environment.
- Risk tiers: standard and high. High risk covers PEPs, unusual source of funds and adverse screening. Reviews happen every 3 years for standard risk and every year for high risk.
- Retention placeholder: KYC records are kept 7 years after membership ends, then deleted unless under legal hold. Access is restricted to Compliance and authorized staff.

## OP-09 Member services

- Statements: on demand in the portal and monthly, as PDF and Excel, one per product.
- Complaints: acknowledged within 2 business days and resolved within 14 days, with escalation to the Compliance officer after that.
- Notifications: email and in-app, for guarantor requests, loan status, payments received and statements.

## OP-10 Cutover, recovery and release

- Cutover: month-end 23:59 Africa/Nairobi, with intake closed from 18:00.
- Recovery objectives: RPO 15 minutes and RTO 4 hours, both placeholders.
- A full backup restore rehearsal must pass before go-live.
- Training: two sessions per role group, plus the runbooks.
- Release approval: product owner plus the tenant sponsor. The reference tenant itself never goes live.
