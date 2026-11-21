using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Migrations.IAM
{
    /// <inheritdoc />
    public partial class AddMembershipMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "DisplayOrder", "Icon", "IsActive", "IsDeleted", "Key", "ParentId", "Placement", "RequiredModule", "RequiredPermissionKey", "TenantId", "Title", "UpdatedAt", "UpdatedBy", "Url" },
                values: new object[,]
                {
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10701"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Member onboarding and CRM.", 6, "People", true, false, "membership", null, "Sidebar", "core", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Membership", null, null, "/membership" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c50101"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Onboard new members.", 10, "PersonAdd", true, false, "membership-onboarding", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10701"), "Membership", "core", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Member Onboarding", null, null, "/membership/onboarding" },
                    { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c50102"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Review and approve pending member applications.", 20, "HowToReg", true, false, "membership-pending-approvals", new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10701"), "Membership", "core", null, new Guid("0194f700-0000-7000-8000-000000000001"), "Pending Approvals", null, null, "/membership/pending-approvals" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c50101"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c50102"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10701"));
        }
    }
}
