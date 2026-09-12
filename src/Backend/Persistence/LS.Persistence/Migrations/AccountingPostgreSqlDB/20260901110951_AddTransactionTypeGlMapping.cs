using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.AccountingPostgreSqlDB
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionTypeCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DebitAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValue: new byte[0])
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
