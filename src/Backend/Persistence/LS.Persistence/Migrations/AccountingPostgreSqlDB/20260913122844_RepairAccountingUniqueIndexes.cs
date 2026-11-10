using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.AccountingPostgreSqlDB
{
    /// <inheritdoc />
    public partial class RepairAccountingUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rebuild indexes whose names already existed when the old generator omitted UNIQUE.
            // Existing duplicate financial data makes the migration fail and roll back; never delete it.
            migrationBuilder.DropIndex(name: "IX_Accounts_TenantId_AccountCode", schema: "accounting", table: "Accounts");
            migrationBuilder.DropIndex(name: "IX_Journals_TenantId_ReferenceNumber", schema: "accounting", table: "Journals");
            migrationBuilder.DropIndex(name: "IX_TransactionTypeGlMappings_TenantId_TransactionTypeCode", schema: "accounting", table: "TransactionTypeGlMappings");
            migrationBuilder.CreateIndex(name: "IX_Accounts_TenantId_AccountCode", schema: "accounting", table: "Accounts",
                columns: new[] { "TenantId", "AccountCode" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_Journals_TenantId_ReferenceNumber", schema: "accounting", table: "Journals",
                columns: new[] { "TenantId", "ReferenceNumber" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_TransactionTypeGlMappings_TenantId_TransactionTypeCode", schema: "accounting", table: "TransactionTypeGlMappings",
                columns: new[] { "TenantId", "TransactionTypeCode" }, unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Preserve the model's uniqueness invariants when downgrading.
        }
    }
}
