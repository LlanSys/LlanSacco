using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Features.IAM.DataContext.Migrations.IamSqlServerDB
{
    /// <inheritdoc />
    public partial class CleanMenuSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuRoutes",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10603"));

            migrationBuilder.DeleteData(
                table: "MenuRoutes",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10614"));

            migrationBuilder.UpdateData(
                table: "MenuRoutes",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10602"),
                columns: new[] { "Key", "Label" },
                values: new object[] { "admin", "Admin" });

            migrationBuilder.UpdateData(
                table: "MenuRoutes",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10615"),
                columns: new[] { "Key", "PlacementKey", "Url" },
                values: new object[] { "admin-payments", "AdminCenter", "/admin/payments" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MenuRoutes",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10602"),
                columns: new[] { "Key", "Label" },
                values: new object[] { "admin-center", "Admin Center" });

            migrationBuilder.UpdateData(
                table: "MenuRoutes",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10615"),
                columns: new[] { "Key", "PlacementKey", "Url" },
                values: new object[] { "features-payments", "Sidebar", "/features/payments" });

            migrationBuilder.InsertData(
                table: "MenuRoutes",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "PlacementKey", "TenantId", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10603"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Approved application route.", true, false, "solution-overview", "Solution Overview", "Sidebar", new Guid("0194f700-0000-7000-8000-000000000001"), null, null, "/overview" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9d10614"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Approved application route.", true, false, "features", "Features", "Sidebar", new Guid("0194f700-0000-7000-8000-000000000001"), null, null, "/features" }
                });
        }
    }
}
