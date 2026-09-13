using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.Accounting
{
    /// <inheritdoc />
    public partial class AddTransactionTypeGlMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransactionTypeGlMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionTypeCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DebitAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTypeGlMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionTypeGlMappings_Accounts_CreditAccountId",
                        column: x => x.CreditAccountId,
                        principalSchema: "accounting",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionTypeGlMappings_Accounts_DebitAccountId",
                        column: x => x.DebitAccountId,
                        principalSchema: "accounting",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeGlMappings_CreditAccountId",
                table: "TransactionTypeGlMappings",
                column: "CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeGlMappings_DebitAccountId",
                table: "TransactionTypeGlMappings",
                column: "DebitAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeGlMappings_TenantId_TransactionTypeCode",
                table: "TransactionTypeGlMappings",
                columns: new[] { "TenantId", "TransactionTypeCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionTypeGlMappings");
        }
    }
}
