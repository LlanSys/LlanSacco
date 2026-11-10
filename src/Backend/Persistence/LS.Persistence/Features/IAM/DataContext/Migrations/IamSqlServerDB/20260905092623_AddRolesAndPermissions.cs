using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Features.IAM.DataContext.Migrations.IamSqlServerDB
{
    /// <inheritdoc />
    public partial class AddRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10301"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10402"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10401"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10501"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10502"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10503"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10504"));

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10101"),
                column: "DisplayOrder",
                value: 1);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"),
                columns: new[] { "DisplayOrder", "Key", "Title" },
                values: new object[] { 100, "admin", "Admin" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10402"),
                columns: new[] { "DisplayOrder", "Key", "ParentId", "Placement" },
                values: new object[] { 80, "admin-payments", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"), "AdminCenter" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10501"),
                column: "DisplayOrder",
                value: 110);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10601"),
                column: "DisplayOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10701"),
                column: "DisplayOrder",
                value: 40);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10801"),
                column: "DisplayOrder",
                value: 30);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10901"),
                column: "DisplayOrder",
                value: 20);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40101"),
                column: "DisplayOrder",
                value: 30);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40102"),
                column: "DisplayOrder",
                value: 40);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40103"),
                column: "DisplayOrder",
                value: 50);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40104"),
                column: "DisplayOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40105"),
                column: "DisplayOrder",
                value: 20);

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Context", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "IsActive", "IsDeleted", "Key", "Resource", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10405"), "view", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View payroll runs.", true, false, "payroll.view", "payroll", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10406"), "manage", "HR", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Run and close payroll periods.", true, false, "payroll.manage", "payroll", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10901"), "view", "Membership", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View members.", true, false, "members.view", "members", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10902"), "create", "Membership", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Create members.", true, false, "members.create", "members", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10903"), "edit", "Membership", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Update members.", true, false, "members.edit", "members", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10904"), "approve", "Membership", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Approve member onboarding requests.", true, false, "members.approve", "members", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11001"), "view", "Loans", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View loan products.", true, false, "products.view", "products", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11002"), "manage", "Loans", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Manage loan products.", true, false, "products.manage", "products", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11003"), "view", "Loans", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View loan applications.", true, false, "applications.view", "applications", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11004"), "create", "Loans", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Apply for loans.", true, false, "applications.create", "applications", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11005"), "approve", "Loans", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Approve loan applications.", true, false, "applications.approve", "applications", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11006"), "create", "Loans", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Process loan repayments.", true, false, "repayments.create", "repayments", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11101"), "view", "Accounting", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View accounting transactions.", true, false, "transactions.view", "transactions", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11102"), "create", "Accounting", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Post transactions.", true, false, "transactions.create", "transactions", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11103"), "approve", "Accounting", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Approve pending transactions.", true, false, "transactions.approve", "transactions", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10405"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10406"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10901"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10902"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10903"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10904"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11001"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11002"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11003"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11004"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11005"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11006"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11101"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11102"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b11103"));

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10101"),
                column: "DisplayOrder",
                value: 2);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10201"),
                columns: new[] { "DisplayOrder", "Key", "Title" },
                values: new object[] { 3, "admin-center", "Admin Center" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10402"),
                columns: new[] { "DisplayOrder", "Key", "ParentId", "Placement" },
                values: new object[] { 10, "features-payments", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10401"), "Features" });

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10501"),
                column: "DisplayOrder",
                value: 4);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10601"),
                column: "DisplayOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10701"),
                column: "DisplayOrder",
                value: 6);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10801"),
                column: "DisplayOrder",
                value: 7);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10901"),
                column: "DisplayOrder",
                value: 8);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40101"),
                column: "DisplayOrder",
                value: 10);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40102"),
                column: "DisplayOrder",
                value: 20);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40103"),
                column: "DisplayOrder",
                value: 30);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40104"),
                column: "DisplayOrder",
                value: 5);

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c40105"),
                column: "DisplayOrder",
                value: 6);

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredModule", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10301"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Architecture and solution overview.", 1, "AutoStories", true, false, "solution-overview", null, "Sidebar", "showcase", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Solution Overview", null, null, "/overview" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10401"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Reusable platform capability showcases.", 4, "MenuOpen", true, false, "features", null, "Sidebar", "showcase", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Features", null, null, "/features" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Context", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "IsActive", "IsDeleted", "Key", "Resource", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10501"), "view", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View customers.", true, false, "customers.view", "customers", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10502"), "create", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Create customers.", true, false, "customers.create", "customers", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10503"), "edit", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Update customers.", true, false, "customers.edit", "customers", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10504"), "delete", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Delete customers.", true, false, "customers.delete", "customers", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });
        }
    }
}
