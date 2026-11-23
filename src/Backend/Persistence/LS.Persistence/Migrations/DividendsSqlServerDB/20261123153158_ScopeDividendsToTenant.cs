using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.DividendsSqlServerDB
{
    /// <inheritdoc />
    public partial class ScopeDividendsToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DividendDistributionPreferences_MemberId",
                table: "DividendDistributionPreferences");

            migrationBuilder.DropIndex(
                name: "IX_DividendDeclarations_FinancialYear",
                table: "DividendDeclarations");

            migrationBuilder.DropIndex(
                name: "IX_DividendCalculations_DeclarationId_MemberId",
                table: "DividendCalculations");

            migrationBuilder.CreateIndex(
                name: "IX_DividendDistributionPreferences_TenantId_MemberId",
                table: "DividendDistributionPreferences",
                columns: new[] { "TenantId", "MemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DividendDeclarations_TenantId_FinancialYear",
                table: "DividendDeclarations",
                columns: new[] { "TenantId", "FinancialYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DividendCalculations_DeclarationId",
                table: "DividendCalculations",
                column: "DeclarationId");

            migrationBuilder.CreateIndex(
                name: "IX_DividendCalculations_TenantId_DeclarationId_MemberId",
                table: "DividendCalculations",
                columns: new[] { "TenantId", "DeclarationId", "MemberId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DividendDistributionPreferences_TenantId_MemberId",
                table: "DividendDistributionPreferences");

            migrationBuilder.DropIndex(
                name: "IX_DividendDeclarations_TenantId_FinancialYear",
                table: "DividendDeclarations");

            migrationBuilder.DropIndex(
                name: "IX_DividendCalculations_DeclarationId",
                table: "DividendCalculations");

            migrationBuilder.DropIndex(
                name: "IX_DividendCalculations_TenantId_DeclarationId_MemberId",
                table: "DividendCalculations");

            migrationBuilder.CreateIndex(
                name: "IX_DividendDistributionPreferences_MemberId",
                table: "DividendDistributionPreferences",
                column: "MemberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DividendDeclarations_FinancialYear",
                table: "DividendDeclarations",
                column: "FinancialYear",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DividendCalculations_DeclarationId_MemberId",
                table: "DividendCalculations",
                columns: new[] { "DeclarationId", "MemberId" },
                unique: true);
        }
    }
}
