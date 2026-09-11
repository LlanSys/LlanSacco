using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.DividendsPostgreSqlDB
{
    /// <inheritdoc />
    public partial class InitialDividends : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DividendDeclarations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialYear = table.Column<int>(type: "integer", nullable: false),
                    ShareDividendRate = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    DepositInterestRate = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    ShareWhtRate = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    DepositWhtRate = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DividendDeclarations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DividendDistributionPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CapitalizePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    FosaPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ExternalBankPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DividendDistributionPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DividendCalculations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeclarationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeightedShareBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GrossShareDividend = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    WeightedDepositBalance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GrossDepositInterest = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ShareWithholdingTax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DepositWithholdingTax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalWithholdingTax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetPayout = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsDistributed = table.Column<bool>(type: "boolean", nullable: false),
                    DistributedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DividendCalculations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DividendCalculations_DividendDeclarations_DeclarationId",
                        column: x => x.DeclarationId,
                        principalTable: "DividendDeclarations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DividendCalculations_DeclarationId_MemberId",
                table: "DividendCalculations",
                columns: new[] { "DeclarationId", "MemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DividendDeclarations_FinancialYear",
                table: "DividendDeclarations",
                column: "FinancialYear",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DividendDistributionPreferences_MemberId",
                table: "DividendDistributionPreferences",
                column: "MemberId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DividendCalculations");

            migrationBuilder.DropTable(
                name: "DividendDistributionPreferences");

            migrationBuilder.DropTable(
                name: "DividendDeclarations");
        }
    }
}
