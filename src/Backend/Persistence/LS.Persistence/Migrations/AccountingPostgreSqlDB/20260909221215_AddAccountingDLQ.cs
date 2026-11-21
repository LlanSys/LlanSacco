using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Migrations.AccountingPostgreSqlDB
{
    /// <inheritdoc />
    public partial class AddAccountingDLQ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionTypeGlMappings_Accounts_CreditAccountId",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionTypeGlMappings_Accounts_DebitAccountId",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropIndex(
                name: "IX_TransactionTypeGlMappings_CreditAccountId",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropIndex(
                name: "IX_TransactionTypeGlMappings_DebitAccountId",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropColumn(
                name: "CreditAccountId",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropColumn(
                name: "DebitAccountId",
                table: "TransactionTypeGlMappings");

            migrationBuilder.RenameTable(
                name: "TransactionTypeGlMappings",
                newName: "TransactionTypeGlMappings",
                newSchema: "accounting");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "accounting",
                table: "JournalLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                schema: "accounting",
                table: "JournalLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreditExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditSideSource",
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "DebitExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DebitSideSource",
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "FeeExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeeSideSource",
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AccountingIntegrationErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventPayload = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValue: new byte[0])
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingIntegrationErrors", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "accounting",
                table: "JournalLines",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-9000-7000-8000-000000009001"),
                columns: new[] { "BranchId", "CostCenterId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                schema: "accounting",
                table: "JournalLines",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-9000-7000-8000-000000009002"),
                columns: new[] { "BranchId", "CostCenterId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                schema: "accounting",
                table: "JournalLines",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-9000-7000-8000-000000009003"),
                columns: new[] { "BranchId", "CostCenterId" },
                values: new object[] { null, null });

            migrationBuilder.InsertData(
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "CreditExplicitGlAccountId", "CreditSideSource", "DebitExplicitGlAccountId", "DebitSideSource", "Description", "FeeExplicitGlAccountId", "FeeSideSource", "TenantId", "TransactionTypeCode", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("0194f700-6000-7000-8000-000000006001"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, "CHANNEL_ACCOUNT", null, "PRODUCT_ACCOUNT", "Loan Disbursement Mapping", new Guid("0194f700-4000-7000-8000-000000004110"), "EXPLICIT_GL", new Guid("0194f700-0000-7000-8000-000000000001"), "LOAN_DISBURSEMENT", null, null },
                    { new Guid("0194f700-6000-7000-8000-000000006002"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, "PRODUCT_ACCOUNT", null, "CHANNEL_ACCOUNT", "Loan Repayment Mapping", new Guid("0194f700-4000-7000-8000-000000004200"), "EXPLICIT_GL", new Guid("0194f700-0000-7000-8000-000000000001"), "LOAN_REPAYMENT", null, null },
                    { new Guid("0194f700-6000-7000-8000-000000006003"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", new Guid("0194f700-3000-7000-8000-000000003000"), "EXPLICIT_GL", null, "CHANNEL_ACCOUNT", "Share Purchase Mapping", new Guid("0194f700-4000-7000-8000-000000004100"), "EXPLICIT_GL", new Guid("0194f700-0000-7000-8000-000000000001"), "SHARE_PURCHASE", null, null },
                    { new Guid("0194f700-6000-7000-8000-000000006004"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, "PRODUCT_ACCOUNT", null, "CHANNEL_ACCOUNT", "Member Deposit Mapping", new Guid("0194f700-4000-7000-8000-000000004100"), "EXPLICIT_GL", new Guid("0194f700-0000-7000-8000-000000000001"), "MEMBER_DEPOSIT", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeGlMappings_CreditExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                column: "CreditExplicitGlAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeGlMappings_DebitExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                column: "DebitExplicitGlAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeGlMappings_FeeExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                column: "FeeExplicitGlAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionTypeGlMappings_Accounts_CreditExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                column: "CreditExplicitGlAccountId",
                principalSchema: "accounting",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionTypeGlMappings_Accounts_DebitExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                column: "DebitExplicitGlAccountId",
                principalSchema: "accounting",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionTypeGlMappings_Accounts_FeeExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                column: "FeeExplicitGlAccountId",
                principalSchema: "accounting",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionTypeGlMappings_Accounts_CreditExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionTypeGlMappings_Accounts_DebitExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionTypeGlMappings_Accounts_FeeExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropTable(
                name: "AccountingIntegrationErrors");

            migrationBuilder.DropIndex(
                name: "IX_TransactionTypeGlMappings_CreditExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropIndex(
                name: "IX_TransactionTypeGlMappings_DebitExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropIndex(
                name: "IX_TransactionTypeGlMappings_FeeExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-6000-7000-8000-000000006001"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-6000-7000-8000-000000006002"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-6000-7000-8000-000000006003"));

            migrationBuilder.DeleteData(
                schema: "accounting",
                table: "TransactionTypeGlMappings",
                keyColumn: "Id",
                keyValue: new Guid("0194f700-6000-7000-8000-000000006004"));

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "accounting",
                table: "JournalLines");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                schema: "accounting",
                table: "JournalLines");

            migrationBuilder.DropColumn(
                name: "CreditExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropColumn(
                name: "CreditSideSource",
                schema: "accounting",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropColumn(
                name: "DebitExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropColumn(
                name: "DebitSideSource",
                schema: "accounting",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropColumn(
                name: "FeeExplicitGlAccountId",
                schema: "accounting",
                table: "TransactionTypeGlMappings");

            migrationBuilder.DropColumn(
                name: "FeeSideSource",
                schema: "accounting",
                table: "TransactionTypeGlMappings");

            migrationBuilder.RenameTable(
                name: "TransactionTypeGlMappings",
                schema: "accounting",
                newName: "TransactionTypeGlMappings");

            migrationBuilder.AddColumn<Guid>(
                name: "CreditAccountId",
                table: "TransactionTypeGlMappings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DebitAccountId",
                table: "TransactionTypeGlMappings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeGlMappings_CreditAccountId",
                table: "TransactionTypeGlMappings",
                column: "CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeGlMappings_DebitAccountId",
                table: "TransactionTypeGlMappings",
                column: "DebitAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionTypeGlMappings_Accounts_CreditAccountId",
                table: "TransactionTypeGlMappings",
                column: "CreditAccountId",
                principalSchema: "accounting",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionTypeGlMappings_Accounts_DebitAccountId",
                table: "TransactionTypeGlMappings",
                column: "DebitAccountId",
                principalSchema: "accounting",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
