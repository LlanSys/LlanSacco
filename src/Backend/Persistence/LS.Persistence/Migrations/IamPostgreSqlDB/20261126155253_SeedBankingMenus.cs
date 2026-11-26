using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Migrations.IamPostgreSqlDB
{
    /// <inheritdoc />
    public partial class SeedBankingMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredModule", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c11001"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Savings and share products.", 35, "Savings", true, false, "banking", null, "Sidebar", "core", "banking-products.view", new Guid("0194f700-0000-7000-8000-000000000001"), "Banking", null, null, "/banking" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c11002"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Manage savings products.", 10, "Savings", true, false, "banking-savings-products", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c11001"), "Banking", "core", "banking-products.view", new Guid("0194f700-0000-7000-8000-000000000001"), "Savings Products", null, null, "/banking/savings-products" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c11003"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Manage share products.", 20, "PieChart", true, false, "banking-share-products", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c11001"), "Banking", "core", "banking-products.view", new Guid("0194f700-0000-7000-8000-000000000001"), "Share Products", null, null, "/banking/share-products" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c11002"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c11003"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c11001"));
        }
    }
}
