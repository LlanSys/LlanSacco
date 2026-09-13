using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Migrations.Accounting
{
    /// <inheritdoc />
    public partial class SeedChartOfAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "accounting",
                table: "Accounts",
                columns: new[] { "Id", "AccountCode", "AccountName", "AccountType", "CreatedAt", "CreatedBy", "Description", "IsActive", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("0194f700-1000-7000-8000-000000001000"), "1000", "Cash in Hand", "Asset", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Physical cash at branches", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-1000-7000-8000-000000001010"), "1010", "Bank Account (Operating)", "Asset", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Main operating bank account", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-1000-7000-8000-000000001020"), "1020", "Mobile Money Account (M-Pesa)", "Asset", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "M-Pesa Paybill / Till", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-1000-7000-8000-000000001100"), "1100", "Normal Loans to Members", "Asset", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Outstanding principal for normal loans", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-1000-7000-8000-000000001110"), "1110", "Emergency Loans to Members", "Asset", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Outstanding principal for emergency loans", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-1000-7000-8000-000000001120"), "1120", "School Fee Loans to Members", "Asset", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Outstanding principal for school fee loans", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-1000-7000-8000-000000001130"), "1130", "Asset Finance Loans to Members", "Asset", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Outstanding principal for asset finance", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-1000-7000-8000-000000001200"), "1200", "Interest Receivable from Loans", "Asset", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Accrued interest not yet paid", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-1000-7000-8000-000000001300"), "1300", "Accounts Receivable", "Asset", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Other receivables", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-1000-7000-8000-000000001400"), "1400", "Property, Plant & Equipment", "Asset", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Fixed assets", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-1000-7000-8000-000000001410"), "1410", "Intangible Assets", "Asset", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Software licenses, etc.", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-2000-7000-8000-000000002000"), "2000", "Member Deposits (BOSA)", "Liability", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Non-withdrawable member deposits used as multiplier", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-2000-7000-8000-000000002010"), "2010", "Member Savings (FOSA)", "Liability", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Withdrawable ordinary savings", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-2000-7000-8000-000000002020"), "2020", "Fixed Deposits", "Liability", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Term deposits from members", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-2000-7000-8000-000000002100"), "2100", "Accounts Payable", "Liability", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Trade creditors", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-2000-7000-8000-000000002200"), "2200", "Unallocated Funds", "Liability", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Suspense account for unverified receipts", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-2000-7000-8000-000000002300"), "2300", "Statutory Deductions Payable", "Liability", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "PAYE, NSSF, SHIF liabilities", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-2000-7000-8000-000000002400"), "2400", "Dividends Payable", "Liability", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Declared but unpaid dividends", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-2000-7000-8000-000000002500"), "2500", "External Borrowings", "Liability", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Bank loans to the Sacco", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-3000-7000-8000-000000003000"), "3000", "Share Capital", "Equity", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Core capital contribution per member", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-3000-7000-8000-000000003100"), "3100", "Retained Earnings", "Equity", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Accumulated surpluses", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-3000-7000-8000-000000003200"), "3200", "Statutory Reserve Fund", "Equity", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "SASRA required 20% of surplus reserve", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-4000-7000-8000-000000004000"), "4000", "Interest Income from Loans", "Revenue", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Main interest revenue", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-4000-7000-8000-000000004010"), "4010", "Interest Income from Investments", "Revenue", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "T-Bills, interbank lending", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-4000-7000-8000-000000004100"), "4100", "Fee and Commission Income", "Revenue", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "General fees", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-4000-7000-8000-000000004110"), "4110", "Loan Application Fees", "Revenue", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Processing fees for loans", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-4000-7000-8000-000000004200"), "4200", "Penalty Income", "Revenue", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Late payment penalties", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-4000-7000-8000-000000004300"), "4300", "Other Operating Income", "Revenue", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Miscellaneous income", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-5000-7000-8000-000000005000"), "5000", "Personnel Expenses", "Expense", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Staff salaries, wages, benefits", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-5000-7000-8000-000000005010"), "5010", "Board & Committee Allowances", "Expense", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Governance costs", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-5000-7000-8000-000000005100"), "5100", "Administrative Expenses", "Expense", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Rent, utilities, licenses, software", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-5000-7000-8000-000000005200"), "5200", "Financial Expenses", "Expense", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Bank charges, interest on borrowing", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-5000-7000-8000-000000005300"), "5300", "Provision for Bad Debts", "Expense", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Expected credit loss allowance", true, new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });

            migrationBuilder.InsertData(
                schema: "accounting",
                table: "Journals",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "ReferenceNumber", "Status", "TenantId", "TransactionDate", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("0194f700-9000-7000-8000-000000009000"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Opening Balances Migration", "OB-001", "Posted", new Guid("0194f700-0000-7000-8000-000000000001"), new DateTimeOffset(new DateTime(2025, 12, 31, 23, 59, 59, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null });

            migrationBuilder.InsertData(
                schema: "accounting",
                table: "JournalLines",
                columns: new[] { "Id", "AccountId", "CreatedAt", "CreatedBy", "Credit", "Debit", "Description", "JournalId", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("0194f700-9000-7000-8000-000000009001"), new Guid("0194f700-1000-7000-8000-000000001010"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", 0m, 1000000m, "Opening Bank Balance", new Guid("0194f700-9000-7000-8000-000000009000"), new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-9000-7000-8000-000000009002"), new Guid("0194f700-3000-7000-8000-000000003000"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", 500000m, 0m, "Opening Share Capital", new Guid("0194f700-9000-7000-8000-000000009000"), new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("0194f700-9000-7000-8000-000000009003"), new Guid("0194f700-3000-7000-8000-000000003100"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", 500000m, 0m, "Opening Retained Earnings", new Guid("0194f700-9000-7000-8000-000000009000"), new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-1000-7000-8000-000000001000"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-1000-7000-8000-000000001020"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-1000-7000-8000-000000001100"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-1000-7000-8000-000000001110"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-1000-7000-8000-000000001120"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-1000-7000-8000-000000001130"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-1000-7000-8000-000000001200"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-1000-7000-8000-000000001300"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-1000-7000-8000-000000001400"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-1000-7000-8000-000000001410"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-2000-7000-8000-000000002000"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-2000-7000-8000-000000002010"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-2000-7000-8000-000000002020"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-2000-7000-8000-000000002100"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-2000-7000-8000-000000002200"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-2000-7000-8000-000000002300"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-2000-7000-8000-000000002400"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-2000-7000-8000-000000002500"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-3000-7000-8000-000000003200"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-4000-7000-8000-000000004000"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-4000-7000-8000-000000004010"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-4000-7000-8000-000000004100"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-4000-7000-8000-000000004110"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-4000-7000-8000-000000004200"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-4000-7000-8000-000000004300"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-5000-7000-8000-000000005000"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-5000-7000-8000-000000005010"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-5000-7000-8000-000000005100"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-5000-7000-8000-000000005200"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-5000-7000-8000-000000005300"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "JournalLines",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-9000-7000-8000-000000009001"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "JournalLines",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-9000-7000-8000-000000009002"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "JournalLines",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-9000-7000-8000-000000009003"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-1000-7000-8000-000000001010"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-3000-7000-8000-000000003000"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-3000-7000-8000-000000003100"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "Journals",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-9000-7000-8000-000000009000"));
        }
    }
}
