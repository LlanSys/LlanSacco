using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.Banking
{
    /// <inheritdoc />
    public partial class Phase2BankingAccrualConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultCheckoffTarget",
                schema: "banking",
                table: "SavingsProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastInterestAccruedOn",
                schema: "banking",
                table: "SavingsAccounts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastInterestAccruedOn",
                schema: "banking",
                table: "DepositAccounts",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefaultCheckoffTarget",
                schema: "banking",
                table: "SavingsProducts");

            migrationBuilder.DropColumn(
                name: "LastInterestAccruedOn",
                schema: "banking",
                table: "SavingsAccounts");

            migrationBuilder.DropColumn(
                name: "LastInterestAccruedOn",
                schema: "banking",
                table: "DepositAccounts");
        }
    }
}
