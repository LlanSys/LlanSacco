using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LS.Persistence.Migrations.IAM
{
    /// <inheritdoc />
    public partial class SeedSaccoOperationPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PermissionActions",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("01a20921-9d68-73d1-b02a-1e46b43bbc6b"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Prepare the applicable operation.", true, false, "prepare", "Prepare", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d68-7409-96fc-ef9be0737d63"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Open the applicable operation.", true, false, "open", "Open", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d68-75dc-b278-e86c93cf4bb6"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Disburse the applicable operation.", true, false, "disburse", "Disburse", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d68-793f-97cd-1f28fc8e1a3e"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Post the applicable operation.", true, false, "post", "Post", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d68-7968-ae28-c53da13177c8"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Retry the applicable operation.", true, false, "retry", "Retry", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d68-7fd6-8137-a19db0ee0d30"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Manage the applicable operation.", true, false, "manage", "Manage", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });

            migrationBuilder.InsertData(
                table: "PermissionContexts",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("01a20921-9d64-70bd-8750-72c87b5a85b1"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Loans operations.", true, false, "Loans", "Loans", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-734c-a76d-af07344ad421"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Accounting operations.", true, false, "Accounting", "Accounting", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-74cb-924a-c2b7fc80dcfc"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "CheckOff operations.", true, false, "CheckOff", "CheckOff", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });

            migrationBuilder.InsertData(
                table: "PermissionResources",
                columns: new[] { "Id", "ContextKey", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "Key", "Label", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("01a20921-9d64-707f-bd5e-99454f749c7b"), "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Vault transfers operations.", true, false, "vault-transfers", "Vault transfers", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-7238-a409-21e5dd782ad3"), "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Teller tills operations.", true, false, "teller-tills", "Teller tills", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-7286-ad6a-aa845861e1e1"), "Accounting", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Accounting integrations operations.", true, false, "accounting-integrations", "Accounting integrations", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-7316-9f44-d15fd2a67243"), "CheckOff", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Check-off operations.", true, false, "checkoff", "Check-off", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-739f-97e8-58248c469132"), "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Banking products operations.", true, false, "banking-products", "Banking products", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-7614-bc45-2101b553c789"), "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Banking accounts operations.", true, false, "banking-accounts", "Banking accounts", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-78c5-a609-825ab678a8bd"), "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Banking transactions operations.", true, false, "banking-transactions", "Banking transactions", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-7ad6-9330-b48828498b6c"), "Loans", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Collections operations.", true, false, "collections", "Collections", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-7afa-abd0-86f52a7c5564"), "Loans", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, "Loan applications operations.", true, false, "applications", "Loan applications", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Context", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "IsActive", "IsDeleted", "Key", "Resource", "TenantId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("01a20921-9d5f-7902-9430-1ce4ec5562c9"), "view", "Accounting", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View integration failure summaries.", true, false, "accounting-integrations.view", "accounting-integrations", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d63-7191-a608-ae2202a6708a"), "view", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View member banking accounts.", true, false, "banking-accounts.view", "banking-accounts", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d63-71ea-bb31-1a70f75367d8"), "post", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Post banking transactions.", true, false, "banking-transactions.post", "banking-transactions", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d63-74bd-aa17-eeb1ba2ffbbc"), "view", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View banking products.", true, false, "banking-products.view", "banking-products", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d63-7580-9ac2-4f9a9298c180"), "open", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Open member banking accounts.", true, false, "banking-accounts.open", "banking-accounts", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d63-78ec-90da-6e691c73eac8"), "manage", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Open and close teller tills.", true, false, "teller-tills.manage", "teller-tills", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d63-7c8d-91b5-e505e23f0d68"), "retry", "Accounting", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Replay failed accounting events.", true, false, "accounting-integrations.retry", "accounting-integrations", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d63-7d8e-939f-6c2960120723"), "create", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Transfer cash from a teller till to the vault.", true, false, "vault-transfers.create", "vault-transfers", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d63-7d98-a492-2aee5ad38552"), "manage", "Banking", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Maintain deposit, savings and share products.", true, false, "banking-products.manage", "banking-products", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-7157-9414-0262f9f8d530"), "view", "CheckOff", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View check-off batches and employers.", true, false, "checkoff.view", "checkoff", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-7399-8bc5-b0fb72d44f60"), "manage", "Loans", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Maintain collection cases, actions and promises.", true, false, "collections.manage", "collections", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-7425-a488-6ed6904e1fdb"), "view", "Loans", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "View collection cases and promises.", true, false, "collections.view", "collections", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-778c-b95e-c79716ba6413"), "disburse", "Loans", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Disburse approved loan applications.", true, false, "applications.disburse", "applications", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-79bf-bdb1-7f4b0dd7ff85"), "post", "CheckOff", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Post validated check-off deductions.", true, false, "checkoff.post", "checkoff", new Guid("0194f700-0000-7000-8000-000000000001"), null, null },
                    { new Guid("01a20921-9d64-7b09-913d-2e2aeb429c9f"), "prepare", "CheckOff", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", null, null, null, "Prepare and validate check-off batches.", true, false, "checkoff.prepare", "checkoff", new Guid("0194f700-0000-7000-8000-000000000001"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d68-73d1-b02a-1e46b43bbc6b"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d68-7409-96fc-ef9be0737d63"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d68-75dc-b278-e86c93cf4bb6"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d68-793f-97cd-1f28fc8e1a3e"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d68-7968-ae28-c53da13177c8"));

            migrationBuilder.DeleteData(
                table: "PermissionActions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d68-7fd6-8137-a19db0ee0d30"));

            migrationBuilder.DeleteData(
                table: "PermissionContexts",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-70bd-8750-72c87b5a85b1"));

            migrationBuilder.DeleteData(
                table: "PermissionContexts",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-734c-a76d-af07344ad421"));

            migrationBuilder.DeleteData(
                table: "PermissionContexts",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-74cb-924a-c2b7fc80dcfc"));

            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-707f-bd5e-99454f749c7b"));

            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-7238-a409-21e5dd782ad3"));

            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-7286-ad6a-aa845861e1e1"));

            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-7316-9f44-d15fd2a67243"));

            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-739f-97e8-58248c469132"));

            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-7614-bc45-2101b553c789"));

            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-78c5-a609-825ab678a8bd"));

            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-7ad6-9330-b48828498b6c"));

            migrationBuilder.DeleteData(
                table: "PermissionResources",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-7afa-abd0-86f52a7c5564"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d5f-7902-9430-1ce4ec5562c9"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d63-7191-a608-ae2202a6708a"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d63-71ea-bb31-1a70f75367d8"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d63-74bd-aa17-eeb1ba2ffbbc"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d63-7580-9ac2-4f9a9298c180"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d63-78ec-90da-6e691c73eac8"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d63-7c8d-91b5-e505e23f0d68"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d63-7d8e-939f-6c2960120723"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d63-7d98-a492-2aee5ad38552"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-7157-9414-0262f9f8d530"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-7399-8bc5-b0fb72d44f60"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-7425-a488-6ed6904e1fdb"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-778c-b95e-c79716ba6413"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-79bf-bdb1-7f4b0dd7ff85"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("01a20921-9d64-7b09-913d-2e2aeb429c9f"));
        }
    }
}
