using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Migrations.IamPostgreSqlDB
{
    /// <inheritdoc />
    public partial class AddHrMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredModule", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10901"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Human resources and payroll processing.", 8, "Badge", true, false, "hr", null, "Sidebar", "core", null, new Guid("0194f700-0000-7000-8000-000000000001"), "HR & Payroll", null, null, "/hr" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c70101"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Run and manage payroll.", 10, "Payments", true, false, "hr-payroll", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10901"), "Hr", "core", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Payroll", null, null, "/hr/payroll" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c70102"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Configure payroll parameters.", 20, "Settings", true, false, "hr-payroll-settings", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10901"), "Hr", "core", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Payroll Settings", null, null, "/hr/payroll/settings" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c70101"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c70102"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10901"));
        }
    }
}
