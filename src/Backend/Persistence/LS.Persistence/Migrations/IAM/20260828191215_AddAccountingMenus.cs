using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Migrations.IAM
{
    /// <inheritdoc />
    public partial class AddAccountingMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredModule", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40104"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Manage Chart of Accounts.", 5, "AccountTree", true, false, "accounting-chart-of-accounts", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10601"), "Accounting", "core", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Chart of Accounts", null, null, "/accounting/chart-of-accounts" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40105"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Create new journal entries.", 6, "EditNote", true, false, "accounting-journal-entry", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10601"), "Accounting", "core", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Journal Entry", null, null, "/accounting/journal-entry" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40104"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40105"));
        }
    }
}
