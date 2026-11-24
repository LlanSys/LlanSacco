using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.LoansPostgreSqlDB
{
    /// <inheritdoc />
    public partial class ConfirmedGuarantorSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EligibleDepositMultiple",
                table: "LoanProducts",
                type: "numeric(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 3m);

            migrationBuilder.AddColumn<int>(
                name: "GuarantorConsentDays",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 14);

            migrationBuilder.AddColumn<string>(
                name: "GuarantorPolicyVersion",
                table: "LoanProducts",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "LLANSACCO-REFERENCE/0.1");

            migrationBuilder.AddColumn<int>(
                name: "GuarantorReservationDays",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<decimal>(
                name: "AcceptedAmount",
                table: "LoanGuarantors",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ConsentActorId",
                table: "LoanGuarantors",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConsentAt",
                table: "LoanGuarantors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplacesPledgeId",
                table: "LoanGuarantors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReservationExpiresAt",
                table: "LoanGuarantors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReservationFailure",
                table: "LoanGuarantors",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReservationPolicyVersion",
                table: "LoanGuarantors",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "ReservationRevision",
                table: "LoanGuarantors",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SavingsAccountId",
                table: "LoanGuarantors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedDepositMultiple",
                table: "LoanApplications",
                type: "numeric(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedEligibleDeposits",
                table: "LoanApplications",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedExposureLimit",
                table: "LoanApplications",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreditApprovedAt",
                table: "LoanApplications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditApprovedBy",
                table: "LoanApplications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditPolicyVersion",
                table: "LoanApplications",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DisbursementBranchId",
                table: "LoanApplications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DisbursementCostCenterId",
                table: "LoanApplications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DisbursementOperationId",
                table: "LoanApplications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DisbursementPaymentChannelGlAccountId",
                table: "LoanApplications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisbursementRequestedBy",
                table: "LoanApplications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PendingRecoveryPrincipal",
                table: "LoanApplications",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PendingSecurityReviewId",
                table: "LoanApplications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityFailure",
                table: "LoanApplications",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GuarantorRecoveries",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EvidenceReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false, defaultValue: new byte[0])
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuarantorRecoveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuarantorSecurityReviews",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PledgeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplacementPledgeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReplacementMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    SavingsAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    Kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceHeldAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EvidenceReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false, defaultValue: new byte[0])
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuarantorSecurityReviews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanGuarantors_TenantId_LoanApplicationId_GuarantorMemberId",
                table: "LoanGuarantors",
                columns: new[] { "TenantId", "LoanApplicationId", "GuarantorMemberId" });

            migrationBuilder.CreateIndex(
                name: "IX_GuarantorRecoveries_TenantId_LoanId",
                schema: "loans",
                table: "GuarantorRecoveries",
                columns: new[] { "TenantId", "LoanId" });

            migrationBuilder.CreateIndex(
                name: "IX_GuarantorSecurityReviews_TenantId_LoanId",
                schema: "loans",
                table: "GuarantorSecurityReviews",
                columns: new[] { "TenantId", "LoanId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuarantorRecoveries",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "GuarantorSecurityReviews",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanGuarantors_TenantId_LoanApplicationId_GuarantorMemberId",
                table: "LoanGuarantors");

            migrationBuilder.DropColumn(
                name: "EligibleDepositMultiple",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "GuarantorConsentDays",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "GuarantorPolicyVersion",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "GuarantorReservationDays",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "AcceptedAmount",
                table: "LoanGuarantors");

            migrationBuilder.DropColumn(
                name: "ConsentActorId",
                table: "LoanGuarantors");

            migrationBuilder.DropColumn(
                name: "ConsentAt",
                table: "LoanGuarantors");

            migrationBuilder.DropColumn(
                name: "ReplacesPledgeId",
                table: "LoanGuarantors");

            migrationBuilder.DropColumn(
                name: "ReservationExpiresAt",
                table: "LoanGuarantors");

            migrationBuilder.DropColumn(
                name: "ReservationFailure",
                table: "LoanGuarantors");

            migrationBuilder.DropColumn(
                name: "ReservationPolicyVersion",
                table: "LoanGuarantors");

            migrationBuilder.DropColumn(
                name: "ReservationRevision",
                table: "LoanGuarantors");

            migrationBuilder.DropColumn(
                name: "SavingsAccountId",
                table: "LoanGuarantors");

            migrationBuilder.DropColumn(
                name: "ApprovedDepositMultiple",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "ApprovedEligibleDeposits",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "ApprovedExposureLimit",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "CreditApprovedAt",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "CreditApprovedBy",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "CreditPolicyVersion",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "DisbursementBranchId",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "DisbursementCostCenterId",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "DisbursementOperationId",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "DisbursementPaymentChannelGlAccountId",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "DisbursementRequestedBy",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "PendingRecoveryPrincipal",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "PendingSecurityReviewId",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "SecurityFailure",
                table: "LoanApplications");
        }
    }
}
