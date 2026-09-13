using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.LoansPostgreSqlDB
{
    /// <inheritdoc />
    public partial class Phase2CollectionCaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_CollectionCases_LoanApplicationId\";");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCases_LoanApplicationId",
                table: "CollectionCases",
                column: "LoanApplicationId",
                unique: true,
                filter: "\"Status\" = 'Open'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The previous predicate referenced a nonexistent column; retain the valid invariant on downgrade.
        }
    }
}