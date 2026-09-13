using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Features.Accounting.Enums;
using System;
using System.Collections.Generic;

namespace LS.Persistence.Features.Accounting.Seeds;

internal static class ChartOfAccountsSeed
{
    private static readonly DateTimeOffset SeedCreatedAt = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly Guid SeedTenantId = new("0194f700-0000-7000-8000-000000000001");
    private const string SystemUser = "System";

    public static readonly Guid CashInHandId = Guid.Parse("0194f700-1000-7000-8000-000000001000");
    public static readonly Guid BankOperatingId = Guid.Parse("0194f700-1000-7000-8000-000000001010");
    public static readonly Guid MpesaId = Guid.Parse("0194f700-1000-7000-8000-000000001020");
    public static readonly Guid NormalLoansId = Guid.Parse("0194f700-1000-7000-8000-000000001100");
    public static readonly Guid EmergencyLoansId = Guid.Parse("0194f700-1000-7000-8000-000000001110");
    public static readonly Guid SchoolFeeLoansId = Guid.Parse("0194f700-1000-7000-8000-000000001120");
    public static readonly Guid AssetFinanceLoansId = Guid.Parse("0194f700-1000-7000-8000-000000001130");
    public static readonly Guid InterestReceivableId = Guid.Parse("0194f700-1000-7000-8000-000000001200");
    public static readonly Guid AccountsReceivableId = Guid.Parse("0194f700-1000-7000-8000-000000001300");
    public static readonly Guid PPEId = Guid.Parse("0194f700-1000-7000-8000-000000001400");
    public static readonly Guid IntangibleAssetsId = Guid.Parse("0194f700-1000-7000-8000-000000001410");

    public static readonly Guid MemberDepositsId = Guid.Parse("0194f700-2000-7000-8000-000000002000");
    public static readonly Guid MemberSavingsId = Guid.Parse("0194f700-2000-7000-8000-000000002010");
    public static readonly Guid FixedDepositsId = Guid.Parse("0194f700-2000-7000-8000-000000002020");
    public static readonly Guid AccountsPayableId = Guid.Parse("0194f700-2000-7000-8000-000000002100");
    public static readonly Guid UnallocatedFundsId = Guid.Parse("0194f700-2000-7000-8000-000000002200");
    public static readonly Guid StatutoryDeductionsPayableId = Guid.Parse("0194f700-2000-7000-8000-000000002300");
    public static readonly Guid DividendsPayableId = Guid.Parse("0194f700-2000-7000-8000-000000002400");
    public static readonly Guid ExternalBorrowingsId = Guid.Parse("0194f700-2000-7000-8000-000000002500");

    public static readonly Guid ShareCapitalId = Guid.Parse("0194f700-3000-7000-8000-000000003000");
    public static readonly Guid RetainedEarningsId = Guid.Parse("0194f700-3000-7000-8000-000000003100");
    public static readonly Guid StatutoryReserveId = Guid.Parse("0194f700-3000-7000-8000-000000003200");

    public static readonly Guid InterestIncomeLoansId = Guid.Parse("0194f700-4000-7000-8000-000000004000");
    public static readonly Guid InterestIncomeInvestmentsId = Guid.Parse("0194f700-4000-7000-8000-000000004010");
    public static readonly Guid FeeIncomeId = Guid.Parse("0194f700-4000-7000-8000-000000004100");
    public static readonly Guid LoanAppFeesId = Guid.Parse("0194f700-4000-7000-8000-000000004110");
    public static readonly Guid PenaltyIncomeId = Guid.Parse("0194f700-4000-7000-8000-000000004200");
    public static readonly Guid OtherOperatingIncomeId = Guid.Parse("0194f700-4000-7000-8000-000000004300");

    public static readonly Guid PersonnelExpensesId = Guid.Parse("0194f700-5000-7000-8000-000000005000");
    public static readonly Guid BoardAllowancesId = Guid.Parse("0194f700-5000-7000-8000-000000005010");
    public static readonly Guid AdminExpensesId = Guid.Parse("0194f700-5000-7000-8000-000000005100");
    public static readonly Guid FinancialExpensesId = Guid.Parse("0194f700-5000-7000-8000-000000005200");
    public static readonly Guid ProvisionBadDebtsId = Guid.Parse("0194f700-5000-7000-8000-000000005300");

    public static readonly Guid OpeningBalanceJournalId = Guid.Parse("0194f700-9000-7000-8000-000000009000");

    internal static IReadOnlyList<Account> Accounts =>
    [
        CreateAccount(CashInHandId, "1000", "Cash in Hand", AccountType.Asset, "Physical cash at branches"),
        CreateAccount(BankOperatingId, "1010", "Bank Account (Operating)", AccountType.Asset, "Main operating bank account"),
        CreateAccount(MpesaId, "1020", "Mobile Money Account (M-Pesa)", AccountType.Asset, "M-Pesa Paybill / Till"),
        CreateAccount(NormalLoansId, "1100", "Normal Loans to Members", AccountType.Asset, "Outstanding principal for normal loans"),
        CreateAccount(EmergencyLoansId, "1110", "Emergency Loans to Members", AccountType.Asset, "Outstanding principal for emergency loans"),
        CreateAccount(SchoolFeeLoansId, "1120", "School Fee Loans to Members", AccountType.Asset, "Outstanding principal for school fee loans"),
        CreateAccount(AssetFinanceLoansId, "1130", "Asset Finance Loans to Members", AccountType.Asset, "Outstanding principal for asset finance"),
        CreateAccount(InterestReceivableId, "1200", "Interest Receivable from Loans", AccountType.Asset, "Accrued interest not yet paid"),
        CreateAccount(AccountsReceivableId, "1300", "Accounts Receivable", AccountType.Asset, "Other receivables"),
        CreateAccount(PPEId, "1400", "Property, Plant & Equipment", AccountType.Asset, "Fixed assets"),
        CreateAccount(IntangibleAssetsId, "1410", "Intangible Assets", AccountType.Asset, "Software licenses, etc."),

        CreateAccount(MemberDepositsId, "2000", "Member Deposits (BOSA)", AccountType.Liability, "Non-withdrawable member deposits used as multiplier"),
        CreateAccount(MemberSavingsId, "2010", "Member Savings (FOSA)", AccountType.Liability, "Withdrawable ordinary savings"),
        CreateAccount(FixedDepositsId, "2020", "Fixed Deposits", AccountType.Liability, "Term deposits from members"),
        CreateAccount(AccountsPayableId, "2100", "Accounts Payable", AccountType.Liability, "Trade creditors"),
        CreateAccount(UnallocatedFundsId, "2200", "Unallocated Funds", AccountType.Liability, "Suspense account for unverified receipts"),
        CreateAccount(StatutoryDeductionsPayableId, "2300", "Statutory Deductions Payable", AccountType.Liability, "PAYE, NSSF, SHIF liabilities"),
        CreateAccount(DividendsPayableId, "2400", "Dividends Payable", AccountType.Liability, "Declared but unpaid dividends"),
        CreateAccount(ExternalBorrowingsId, "2500", "External Borrowings", AccountType.Liability, "Bank loans to the Sacco"),

        CreateAccount(ShareCapitalId, "3000", "Share Capital", AccountType.Equity, "Core capital contribution per member"),
        CreateAccount(RetainedEarningsId, "3100", "Retained Earnings", AccountType.Equity, "Accumulated surpluses"),
        CreateAccount(StatutoryReserveId, "3200", "Statutory Reserve Fund", AccountType.Equity, "SASRA required 20% of surplus reserve"),

        CreateAccount(InterestIncomeLoansId, "4000", "Interest Income from Loans", AccountType.Revenue, "Main interest revenue"),
        CreateAccount(InterestIncomeInvestmentsId, "4010", "Interest Income from Investments", AccountType.Revenue, "T-Bills, interbank lending"),
        CreateAccount(FeeIncomeId, "4100", "Fee and Commission Income", AccountType.Revenue, "General fees"),
        CreateAccount(LoanAppFeesId, "4110", "Loan Application Fees", AccountType.Revenue, "Processing fees for loans"),
        CreateAccount(PenaltyIncomeId, "4200", "Penalty Income", AccountType.Revenue, "Late payment penalties"),
        CreateAccount(OtherOperatingIncomeId, "4300", "Other Operating Income", AccountType.Revenue, "Miscellaneous income"),

        CreateAccount(PersonnelExpensesId, "5000", "Personnel Expenses", AccountType.Expense, "Staff salaries, wages, benefits"),
        CreateAccount(BoardAllowancesId, "5010", "Board & Committee Allowances", AccountType.Expense, "Governance costs"),
        CreateAccount(AdminExpensesId, "5100", "Administrative Expenses", AccountType.Expense, "Rent, utilities, licenses, software"),
        CreateAccount(FinancialExpensesId, "5200", "Financial Expenses", AccountType.Expense, "Bank charges, interest on borrowing"),
        CreateAccount(ProvisionBadDebtsId, "5300", "Provision for Bad Debts", AccountType.Expense, "Expected credit loss allowance")
    ];

    internal static IReadOnlyList<Journal> Journals =>
    [
        CreateJournal(OpeningBalanceJournalId, "OB-001", "Opening Balances Migration", new DateTimeOffset(new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc)))
    ];

    internal static IReadOnlyList<JournalLine> JournalLines =>
    [
        CreateJournalLine(Guid.Parse("0194f700-9000-7000-8000-000000009001"), OpeningBalanceJournalId, BankOperatingId, "Opening Bank Balance", 1000000, 0),
        CreateJournalLine(Guid.Parse("0194f700-9000-7000-8000-000000009002"), OpeningBalanceJournalId, ShareCapitalId, "Opening Share Capital", 0, 500000),
        CreateJournalLine(Guid.Parse("0194f700-9000-7000-8000-000000009003"), OpeningBalanceJournalId, RetainedEarningsId, "Opening Retained Earnings", 0, 500000)
    ];

    private static Account CreateAccount(Guid id, string code, string name, AccountType type, string description)
    {
        var account = Account.Create(SeedTenantId, code, name, type, description, SystemUser);
        account.Id = id;
        account.CreatedAt = SeedCreatedAt;
        return account;
    }

    private static Journal CreateJournal(Guid id, string reference, string description, DateTimeOffset date)
    {
        var journal = Journal.Create(SeedTenantId, reference, description, date, SystemUser);
        journal.Id = id;
        journal.CreatedAt = SeedCreatedAt;
        journal.Status = JournalStatus.Posted;
        return journal;
    }

    private static JournalLine CreateJournalLine(Guid id, Guid journalId, Guid accountId, string description, decimal debit, decimal credit)
    {
        var line = JournalLine.Create(SeedTenantId, journalId, accountId, description, debit, credit, SystemUser);
        line.Id = id;
        line.CreatedAt = SeedCreatedAt;
        return line;
    }

    internal static IReadOnlyList<TransactionTypeGlMapping> TransactionTypeGlMappings =>
    [
        CreateMapping(Guid.Parse("0194f700-6000-7000-8000-000000006001"), "LOAN_DISBURSEMENT", "PRODUCT_ACCOUNT", null, "CHANNEL_ACCOUNT", null, "EXPLICIT_GL", LoanAppFeesId, "Loan Disbursement Mapping"),
        CreateMapping(Guid.Parse("0194f700-6000-7000-8000-000000006002"), "LOAN_REPAYMENT", "CHANNEL_ACCOUNT", null, "PRODUCT_ACCOUNT", null, "EXPLICIT_GL", PenaltyIncomeId, "Loan Repayment Mapping"),
        CreateMapping(Guid.Parse("0194f700-6000-7000-8000-000000006003"), "SHARE_PURCHASE", "CHANNEL_ACCOUNT", null, "EXPLICIT_GL", ShareCapitalId, "EXPLICIT_GL", FeeIncomeId, "Share Purchase Mapping"),
        CreateMapping(Guid.Parse("0194f700-6000-7000-8000-000000006004"), "MEMBER_DEPOSIT", "CHANNEL_ACCOUNT", null, "PRODUCT_ACCOUNT", null, "EXPLICIT_GL", FeeIncomeId, "Member Deposit Mapping")
    ];

    private static TransactionTypeGlMapping CreateMapping(Guid id, string typeCode, string debitSource, Guid? debitId, string creditSource, Guid? creditId, string feeSource, Guid? feeId, string description)
    {
        var mapping = new TransactionTypeGlMapping
        {
            TenantId = SeedTenantId,
            TransactionTypeCode = typeCode,
            DebitSideSource = debitSource,
            DebitExplicitGlAccountId = debitId,
            CreditSideSource = creditSource,
            CreditExplicitGlAccountId = creditId,
            FeeSideSource = feeSource,
            FeeExplicitGlAccountId = feeId,
            Description = description,
            CreatedBy = SystemUser,
            CreatedAt = SeedCreatedAt
        };
        mapping.Id = id;
        return mapping;
    }
}
