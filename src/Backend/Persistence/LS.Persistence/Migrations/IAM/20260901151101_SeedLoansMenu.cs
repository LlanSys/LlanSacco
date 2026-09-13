using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Migrations.IAM
{
    /// <inheritdoc />
    public partial class SeedLoansMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredModule", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10801"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Loan operations and management.", 7, "Money", true, false, "loans", null, "Sidebar", "core", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Loans", null, null, "/loans" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c60101"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Manage loan products.", 10, "Category", true, false, "loans-products", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10801"), "Loans", "core", "controlplane.manage", new Guid("0194f700-0000-7000-8000-000000000001"), "Loan Products", null, null, "/loans/products" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c60102"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Manage loan applications.", 20, "Assignment", true, false, "loans-applications", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10801"), "Loans", "core", "controlplane.manage", new Guid("0194f700-0000-7000-8000-000000000001"), "Loan Applications", null, null, "/loans/applications" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c60101"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c60102"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10801"));
        }
    }
}
