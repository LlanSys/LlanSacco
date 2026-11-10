using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.IamPostgreSqlDB
{
    /// <inheritdoc />
    public partial class SeedAccountingDlqMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredModule", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[] { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40106"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Integration Errors (DLQ).", 60, "SyncProblem", true, false, "accounting-dlq", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10601"), "Accounting", "core", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Integration DLQ", null, null, "/accounting/dlq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40106"));
        }
    }
}
