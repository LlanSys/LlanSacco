using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.BankingPostgreSqlDB
{
    /// <inheritdoc />
    public partial class Phase2FosaPostingReplay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FosaTransactions_TenantId_Reference",
                table: "FosaTransactions",
                columns: new[] { "TenantId", "Reference" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FosaTransactions_TenantId_Reference",
                table: "FosaTransactions");
        }
    }
}
