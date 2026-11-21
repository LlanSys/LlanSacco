using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.BankingPostgreSqlDB
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
                type: "boolean",
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

            migrationBuilder.CreateTable(
                name: "FosaAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    OverdraftLimit = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false, defaultValue: new byte[0])
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FosaAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TellerTills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedTellerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TillName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    MaxLimit = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false, defaultValue: new byte[0])
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TellerTills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vaults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false, defaultValue: new byte[0])
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vaults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FosaTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FosaAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    TellerTillId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false, defaultValue: new byte[0])
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FosaTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FosaTransactions_FosaAccounts_FosaAccountId",
                        column: x => x.FosaAccountId,
                        principalTable: "FosaAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FosaTransactions_TellerTills_TellerTillId",
                        column: x => x.TellerTillId,
                        principalTable: "TellerTills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TillBalancingRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TellerTillId = table.Column<Guid>(type: "uuid", nullable: false),
                    SystemBalance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PhysicalCount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Note1000Count = table.Column<int>(type: "integer", nullable: false),
                    Note500Count = table.Column<int>(type: "integer", nullable: false),
                    Note200Count = table.Column<int>(type: "integer", nullable: false),
                    Note100Count = table.Column<int>(type: "integer", nullable: false),
                    Note50Count = table.Column<int>(type: "integer", nullable: false),
                    Coin20Count = table.Column<int>(type: "integer", nullable: false),
                    Coin10Count = table.Column<int>(type: "integer", nullable: false),
                    Coin5Count = table.Column<int>(type: "integer", nullable: false),
                    Coin1Count = table.Column<int>(type: "integer", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: false, defaultValue: new byte[0])
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TillBalancingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TillBalancingRecords_TellerTills_TellerTillId",
                        column: x => x.TellerTillId,
                        principalTable: "TellerTills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FosaAccounts_AccountNumber",
                table: "FosaAccounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FosaAccounts_MemberId",
                table: "FosaAccounts",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_FosaTransactions_FosaAccountId",
                table: "FosaTransactions",
                column: "FosaAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FosaTransactions_Reference",
                table: "FosaTransactions",
                column: "Reference");

            migrationBuilder.CreateIndex(
                name: "IX_FosaTransactions_TellerTillId",
                table: "FosaTransactions",
                column: "TellerTillId");

            migrationBuilder.CreateIndex(
                name: "IX_TellerTills_TillName",
                table: "TellerTills",
                column: "TillName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TillBalancingRecords_TellerTillId",
                table: "TillBalancingRecords",
                column: "TellerTillId");

            migrationBuilder.CreateIndex(
                name: "IX_Vaults_Name",
                table: "Vaults",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FosaTransactions");

            migrationBuilder.DropTable(
                name: "TillBalancingRecords");

            migrationBuilder.DropTable(
                name: "Vaults");

            migrationBuilder.DropTable(
                name: "FosaAccounts");

            migrationBuilder.DropTable(
                name: "TellerTills");

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
