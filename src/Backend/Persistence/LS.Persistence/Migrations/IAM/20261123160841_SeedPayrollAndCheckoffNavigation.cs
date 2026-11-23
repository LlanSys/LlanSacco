using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Migrations.IAM
{
    /// <inheritdoc />
    public partial class SeedPayrollAndCheckoffNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c70101"),
                column: "RequiredPermissionKey",
                value: "hr.payroll.view");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c70102"),
                column: "RequiredPermissionKey",
                value: "hr.payroll.settings");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c70103"),
                column: "RequiredPermissionKey",
                value: "checkoff.view");

            migrationBuilder.InsertData(
                table: "PermissionActions",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("01a20929-f9ac-73e4-a2a9-9b58a43faa56"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Run for the selected operation.", true, false, "run", "Run", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20929-f9ad-76eb-b38b-2c628ec05f75"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Close for the selected operation.", true, false, "close", "Close", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20929-f9ad-7e24-b7e5-875dbc5e193c"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Settings for the selected operation.", true, false, "settings", "Settings", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });

            migrationBuilder.InsertData(
                table: "PermissionResources",
                columns: new[] { "Id", "ContextKey", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("01a20929-f9ad-7254-9d94-0fb71645df4a"), "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Payroll operations.", true, false, "hr.payroll", "Payroll", new Guid("0194f700-0000-7000-8000-000000000001"), null, null });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Context", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "IsActive", "IsDeleted", "Key", "Resource", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("01a20929-f9a7-7156-a0dd-0d98c3d5a34a"), "view", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View payroll periods, summaries, and payslips.", true, false, "hr.payroll.view", "hr.payroll", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20929-f9ab-781c-8205-f7e45e20ff03"), "run", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Run payroll for a period.", true, false, "hr.payroll.run", "hr.payroll", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20929-f9ab-7a9e-a2ab-2dc3066cc88b"), "settings", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Read and maintain payroll configuration.", true, false, "hr.payroll.settings", "hr.payroll", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20929-f9ab-7b37-8add-a1468ed88e5e"), "close", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Close a processed payroll period.", true, false, "hr.payroll.close", "hr.payroll", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20929-f9ac-73e4-a2a9-9b58a43faa56"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20929-f9ad-76eb-b38b-2c628ec05f75"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20929-f9ad-7e24-b7e5-875dbc5e193c"));

            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a20929-f9ad-7254-9d94-0fb71645df4a"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20929-f9a7-7156-a0dd-0d98c3d5a34a"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20929-f9ab-781c-8205-f7e45e20ff03"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20929-f9ab-7a9e-a2ab-2dc3066cc88b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20929-f9ab-7b37-8add-a1468ed88e5e"));

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c70101"),
                column: "RequiredPermissionKey",
                value: null);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c70102"),
                column: "RequiredPermissionKey",
                value: null);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c70103"),
                column: "RequiredPermissionKey",
                value: null);
        }
    }
}
