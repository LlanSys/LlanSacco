using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.Banking
{
    /// <inheritdoc />
    public partial class GuarantorReservationsAndRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GuarantorOperationId",
                schema: "banking",
                table: "SavingsTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GuarantorPledgeId",
                schema: "banking",
                table: "SavingsTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowsGuarantorReservations",
                schema: "banking",
                table: "SavingsProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CountsTowardLoanLimit",
                schema: "banking",
                table: "SavingsProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecurityPolicyEvidenceReference",
                schema: "banking",
                table: "SavingsProducts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GuarantorReservationReceipts",
                schema: "banking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OutcomeJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuarantorReservationReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuarantorReservations",
                schema: "banking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PledgeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SavingsAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PolicyVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    DecisionReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuarantorReservations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavingsTransactions_TenantId_GuarantorOperationId_GuarantorPledgeId",
                schema: "banking",
                table: "SavingsTransactions",
                columns: new[] { "TenantId", "GuarantorOperationId", "GuarantorPledgeId" });

            migrationBuilder.CreateIndex(
                name: "IX_GuarantorReservationReceipts_TenantId_OperationId",
                schema: "banking",
                table: "GuarantorReservationReceipts",
                columns: new[] { "TenantId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuarantorReservations_TenantId_LoanId",
                schema: "banking",
                table: "GuarantorReservations",
                columns: new[] { "TenantId", "LoanId" });

            migrationBuilder.CreateIndex(
                name: "IX_GuarantorReservations_TenantId_PledgeId",
                schema: "banking",
                table: "GuarantorReservations",
                columns: new[] { "TenantId", "PledgeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuarantorReservations_TenantId_SavingsAccountId",
                schema: "banking",
                table: "GuarantorReservations",
                columns: new[] { "TenantId", "SavingsAccountId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuarantorReservationReceipts",
                schema: "banking");

            migrationBuilder.DropTable(
                name: "GuarantorReservations",
                schema: "banking");

            migrationBuilder.DropIndex(
                name: "IX_SavingsTransactions_TenantId_GuarantorOperationId_GuarantorPledgeId",
                schema: "banking",
                table: "SavingsTransactions");

            migrationBuilder.DropColumn(
                name: "GuarantorOperationId",
                schema: "banking",
                table: "SavingsTransactions");

            migrationBuilder.DropColumn(
                name: "GuarantorPledgeId",
                schema: "banking",
                table: "SavingsTransactions");

            migrationBuilder.DropColumn(
                name: "AllowsGuarantorReservations",
                schema: "banking",
                table: "SavingsProducts");

            migrationBuilder.DropColumn(
                name: "CountsTowardLoanLimit",
                schema: "banking",
                table: "SavingsProducts");

            migrationBuilder.DropColumn(
                name: "SecurityPolicyEvidenceReference",
                schema: "banking",
                table: "SavingsProducts");
        }
    }
}
