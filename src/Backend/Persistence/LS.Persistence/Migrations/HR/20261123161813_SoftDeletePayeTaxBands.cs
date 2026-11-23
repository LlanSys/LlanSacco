using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.HR
{
    /// <inheritdoc />
    public partial class SoftDeletePayeTaxBands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Hr_PayeTaxBands",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Hr_PayeTaxBands",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Hr_PayeTaxBands",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Hr_PayeTaxBands");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Hr_PayeTaxBands");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Hr_PayeTaxBands");
        }
    }
}
