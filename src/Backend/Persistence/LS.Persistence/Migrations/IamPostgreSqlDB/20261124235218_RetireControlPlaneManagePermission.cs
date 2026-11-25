using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Migrations.IamPostgreSqlDB
{
    /// <inheritdoc />
    public partial class RetireControlPlaneManagePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10801"));

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10501"),
                column: "RequiredPermissionKey",
                value: "control_plane.view_tenants");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c30101"),
                column: "RequiredPermissionKey",
                value: "control_plane.view_tenants");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c30102"),
                column: "RequiredPermissionKey",
                value: "control_plane.view_stamps");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c60101"),
                column: "RequiredPermissionKey",
                value: "products.view");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c60102"),
                column: "RequiredPermissionKey",
                value: "applications.view");

            migrationBuilder.InsertData(
                table: "PermissionActions",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("01a20ff5-7ec4-70fe-a798-acf2360f2db6"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "View tenants and their status.", true, false, "view_tenants", "View Tenants", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-72f9-bf56-fbf18492a9b3"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Start and end support impersonation sessions.", true, false, "manage_support", "Manage Support", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-7709-8eed-3bcafe603de4"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Move tenants between deployment stamps.", true, false, "migrate_tenants", "Migrate Tenants", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-77af-910b-1b80bc330cfe"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Approve tenant KYC.", true, false, "approve_kyc", "Approve Kyc", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-7847-893c-a4a0fca0de49"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Create tenants.", true, false, "create_tenants", "Create Tenants", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-7894-b42a-86a02c70229e"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Activate, suspend and complete provisioning of tenants.", true, false, "manage_tenant_lifecycle", "Manage Tenant Lifecycle", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-79dd-858f-e4f1b4bb973b"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Enable or disable tenant modules.", true, false, "manage_tenant_modules", "Manage Tenant Modules", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-7a6b-941d-dbcdc2b3bada"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "View deployment stamps.", true, false, "view_stamps", "View Stamps", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-7a77-9b10-5402fd51d4ba"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Update tenant details.", true, false, "edit_tenants", "Edit Tenants", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-7ac6-9c6f-dcc4f20e1ffb"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "View platform support audit records.", true, false, "view_audit", "View Audit", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-7d7f-a190-54bb9c870d49"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Approve the applicable operation.", true, false, "approve", "Approve", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-7e4e-93bd-93909b3eefa1"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Create and update deployment stamps.", true, false, "manage_stamps", "Manage Stamps", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });

            migrationBuilder.InsertData(
                table: "PermissionContexts",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("01a20ff5-7ec4-7ab9-83de-48bbc150f20c"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Platform tenant, stamp and support operations.", true, false, "ControlPlane", "Control plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-7b14-bdc8-8fbee734a628"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Dividend declaration and distribution.", true, false, "Dividends", "Dividends", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });

            migrationBuilder.InsertData(
                table: "PermissionResources",
                columns: new[] { "Id", "ContextKey", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("01a20ff5-7ec4-737e-aa41-28f32524ab62"), "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Platform tenants, stamps and support.", true, false, "control_plane", "Control plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-75a4-ab0e-90dcfbe6ecb7"), "Dividends", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Dividend declarations and distributions.", true, false, "dividends", "Dividends", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Context", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "IsActive", "IsDeleted", "Key", "Resource", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("01a20ff5-7ebe-75cc-a91f-a8411f89e7da"), "view_tenants", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View tenants and their status.", true, false, "control_plane.view_tenants", "control_plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec3-70c7-b7a2-e3ca69276903"), "approve_kyc", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Approve tenant KYC.", true, false, "control_plane.approve_kyc", "control_plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec3-7966-9733-057851b1f9a2"), "edit_tenants", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Update tenant details.", true, false, "control_plane.edit_tenants", "control_plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec3-79dc-91b8-0c3e7d5bf784"), "manage_tenant_lifecycle", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Activate, suspend and complete provisioning of tenants.", true, false, "control_plane.manage_tenant_lifecycle", "control_plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec3-7a96-9805-bfaf39d16722"), "view_stamps", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View deployment stamps.", true, false, "control_plane.view_stamps", "control_plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec3-7b6b-b994-9b7f7b67e508"), "manage_tenant_modules", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Enable or disable tenant modules.", true, false, "control_plane.manage_tenant_modules", "control_plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec3-7c2b-9594-ee3ad40a83c3"), "migrate_tenants", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Move tenants between deployment stamps.", true, false, "control_plane.migrate_tenants", "control_plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec3-7e3e-b946-56071d9d37a4"), "create_tenants", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Create tenants.", true, false, "control_plane.create_tenants", "control_plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-7113-a3ae-3c9c907935a7"), "manage", "Dividends", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Declare dividends and run calculations.", true, false, "dividends.manage", "dividends", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-73eb-a7f3-04663f7e39a7"), "manage_stamps", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Create and update deployment stamps.", true, false, "control_plane.manage_stamps", "control_plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-769c-aadc-e19c55133c99"), "view_audit", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View platform support audit records.", true, false, "control_plane.view_audit", "control_plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-77ac-bce3-a53b71c84f75"), "approve", "Dividends", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Approve declarations and post distributions.", true, false, "dividends.approve", "dividends", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-7b7f-be38-f75447f401e0"), "manage_support", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Start and end support impersonation sessions.", true, false, "control_plane.manage_support", "control_plane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20ff5-7ec4-7cdb-8cc8-0cd7f7ecb4f3"), "view", "Dividends", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View dividend declarations.", true, false, "dividends.view", "dividends", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });
            // Platform permissions are granted directly to host-tenant System Administrators (never to the shared
            // roles), replacing the retired coarse control-plane permission in both of its historical spellings.
            migrationBuilder.Sql(""""
                INSERT INTO "UserClaims" ("UserId", "ClaimType", "ClaimValue")
                SELECT u."Id", 'permission', p.key
                FROM "Users" u
                JOIN "UserRoles" ur ON ur."UserId" = u."Id"
                JOIN "Roles" r ON r."Id" = ur."RoleId" AND r."Name" = 'System Administrator'
                CROSS JOIN unnest(ARRAY['control_plane.view_tenants','control_plane.create_tenants','control_plane.edit_tenants','control_plane.manage_tenant_lifecycle','control_plane.manage_tenant_modules','control_plane.migrate_tenants','control_plane.approve_kyc','control_plane.view_stamps','control_plane.manage_stamps','control_plane.view_audit','control_plane.manage_support']) AS p(key)
                WHERE u."TenantId" = '0194f700-0000-7000-8000-000000000001'
                  AND NOT EXISTS (SELECT 1 FROM "UserClaims" c WHERE c."UserId" = u."Id" AND c."ClaimType" = 'permission' AND c."ClaimValue" = p.key);
                DELETE FROM "UserClaims" WHERE "ClaimType" = 'permission' AND "ClaimValue" IN ('controlplane.manage', 'control_plane.manage');
                DELETE FROM "RoleClaims" WHERE "ClaimType" = 'permission' AND "ClaimValue" IN ('controlplane.manage', 'control_plane.manage');
                """");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(""""
                DELETE FROM "UserClaims" WHERE "ClaimType" = 'permission' AND "ClaimValue" IN ('control_plane.view_tenants','control_plane.create_tenants','control_plane.edit_tenants','control_plane.manage_tenant_lifecycle','control_plane.manage_tenant_modules','control_plane.migrate_tenants','control_plane.approve_kyc','control_plane.view_stamps','control_plane.manage_stamps','control_plane.view_audit','control_plane.manage_support');
                """");
            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-70fe-a798-acf2360f2db6"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-72f9-bf56-fbf18492a9b3"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7709-8eed-3bcafe603de4"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-77af-910b-1b80bc330cfe"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7847-893c-a4a0fca0de49"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7894-b42a-86a02c70229e"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-79dd-858f-e4f1b4bb973b"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7a6b-941d-dbcdc2b3bada"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7a77-9b10-5402fd51d4ba"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7ac6-9c6f-dcc4f20e1ffb"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7d7f-a190-54bb9c870d49"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7e4e-93bd-93909b3eefa1"));

            migrationBuilder.DeleteData(
                table: "PermissionContexts",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7ab9-83de-48bbc150f20c"));

            migrationBuilder.DeleteData(
                table: "PermissionContexts",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7b14-bdc8-8fbee734a628"));

            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-737e-aa41-28f32524ab62"));

            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-75a4-ab0e-90dcfbe6ecb7"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ebe-75cc-a91f-a8411f89e7da"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec3-70c7-b7a2-e3ca69276903"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec3-7966-9733-057851b1f9a2"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec3-79dc-91b8-0c3e7d5bf784"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec3-7a96-9805-bfaf39d16722"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec3-7b6b-b994-9b7f7b67e508"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec3-7c2b-9594-ee3ad40a83c3"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec3-7e3e-b946-56071d9d37a4"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7113-a3ae-3c9c907935a7"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-73eb-a7f3-04663f7e39a7"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-769c-aadc-e19c55133c99"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-77ac-bce3-a53b71c84f75"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7b7f-be38-f75447f401e0"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20ff5-7ec4-7cdb-8cc8-0cd7f7ecb4f3"));

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c10501"),
                column: "RequiredPermissionKey",
                value: "controlplane.manage");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c30101"),
                column: "RequiredPermissionKey",
                value: "controlplane.manage");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c30102"),
                column: "RequiredPermissionKey",
                value: "controlplane.manage");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c60101"),
                column: "RequiredPermissionKey",
                value: "controlplane.manage");

            migrationBuilder.UpdateData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9c60102"),
                column: "RequiredPermissionKey",
                value: "controlplane.manage");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Context", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "IsActive", "IsDeleted", "Key", "Resource", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("018fd81d-2c94-7ad0-a4a3-f1edb9b10801"), "manage", "ControlPlane", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Manage Control Plane Tenants and Stamps.", true, false, "controlplane.manage", "controlplane", new Guid("0194f700-0000-7000-8000-000000000001"), null, null });
        }
    }
}
