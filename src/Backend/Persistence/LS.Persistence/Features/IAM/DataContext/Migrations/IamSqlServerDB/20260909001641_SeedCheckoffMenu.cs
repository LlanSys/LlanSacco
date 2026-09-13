using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Features.IAM.DataContext.Migrations.IamSqlServerDB
{
    /// <inheritdoc />
    public partial class SeedCheckoffMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredModule", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[] { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c70103"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Manage employer checkoff batches.", 30, "FactCheck", true, false, "hr-checkoff", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10901"), "Hr", "core", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Checkoff", null, null, "/checkoff/batches" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c70103"));
        }
    }
}
