using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.MembershipPostgreSqlDB
{
    /// <inheritdoc />
    public partial class SoftDeleteBeneficiaries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Beneficiaries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Beneficiaries",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Beneficiaries",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Beneficiaries");
        }
    }
}
