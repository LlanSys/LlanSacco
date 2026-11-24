using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Migrations.IamPostgreSqlDB
{
    /// <inheritdoc />
    public partial class SeedGlMappingPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PermissionResources",
                columns: new[] { "Id", "ContextKey", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("01a2bd10-4c1e-7a31-9f52-6d0b3e8a1c03"), "Accounting", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Transaction-type GL mapping configuration.", true, false, "gl-mappings", "GL mappings", new Guid("0194f700-0000-7000-8000-000000000001"), null, null });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Context", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "IsActive", "IsDeleted", "Key", "Resource", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("01a2bd10-4c1e-7a31-9f52-6d0b3e8a1c01"), "view", "Accounting", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View transaction-type GL mappings.", true, false, "gl-mappings.view", "gl-mappings", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a2bd10-4c1e-7a31-9f52-6d0b3e8a1c02"), "manage", "Accounting", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Configure transaction-type GL mappings.", true, false, "gl-mappings.manage", "gl-mappings", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a2bd10-4c1e-7a31-9f52-6d0b3e8a1c03"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a2bd10-4c1e-7a31-9f52-6d0b3e8a1c01"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a2bd10-4c1e-7a31-9f52-6d0b3e8a1c02"));
        }
    }
}
