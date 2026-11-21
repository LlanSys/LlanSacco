using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Features.CheckOff.DataContext.Migrations.CheckOffPostgreSqlDB
{
    /// <inheritdoc />
    public partial class AddBatchRefAndPeriodToCheckoffBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BatchReference",
                schema: "checkoff",
                table: "CheckoffBatches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessingPeriod",
                schema: "checkoff",
                table: "CheckoffBatches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "CheckoffStagingLoanAllocations",
                schema: "checkoff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckoffStagingRowId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckoffStagingLoanAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckoffStagingLoanAllocations_CheckoffStagingRows_Checkoff~",
                        column: x => x.CheckoffStagingRowId,
                        principalSchema: "checkoff",
                        principalTable: "CheckoffStagingRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckoffStagingLoanAllocations_CheckoffStagingRowId",
                schema: "checkoff",
                table: "CheckoffStagingLoanAllocations",
                column: "CheckoffStagingRowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckoffStagingLoanAllocations",
                schema: "checkoff");

            migrationBuilder.DropColumn(
                name: "BatchReference",
                schema: "checkoff",
                table: "CheckoffBatches");

            migrationBuilder.DropColumn(
                name: "ProcessingPeriod",
                schema: "checkoff",
                table: "CheckoffBatches");
        }
    }
}
