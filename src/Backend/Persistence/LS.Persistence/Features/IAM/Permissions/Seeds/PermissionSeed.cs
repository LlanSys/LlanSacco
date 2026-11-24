using LS.Domain.Features.IAM.Permissions.Entities;

namespace LS.Persistence.Features.IAM.Permissions.Seeds;

internal static class PermissionSeed
{
    private static readonly DateTimeOffset SeedCreatedAt = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly Guid SeedTenantId = new("0194f700-0000-7000-8000-000000000001");

    internal static IReadOnlyList<Permission> Items =>
    [
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10101", "IAM", "users", "view", "View application users."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10102", "IAM", "users", "create", "Create application users."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10103", "IAM", "users", "edit", "Update application users."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10104", "IAM", "users", "deactivate", "Deactivate application users."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10105", "IAM", "users", "manage_roles", "Manage user role assignments."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10106", "IAM", "users", "manage_permissions", "Manage direct user permissions."),

        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10201", "IAM", "roles", "view", "View platform roles."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10202", "IAM", "roles", "create", "Create platform roles."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10203", "IAM", "roles", "edit", "Update platform roles."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10204", "IAM", "roles", "delete", "Delete platform roles."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10205", "IAM", "roles", "manage_permissions", "Manage role permission assignments."),

        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10301", "HR", "departments", "view", "View departments."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10302", "HR", "departments", "create", "Create departments."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10303", "HR", "departments", "edit", "Update departments."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10304", "HR", "departments", "delete", "Delete departments."),

        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10401", "HR", "employees", "view", "View employees."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10402", "HR", "employees", "create", "Create employees."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10403", "HR", "employees", "edit", "Update employees."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10404", "HR", "employees", "delete", "Delete employees."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10405", "HR", "payroll", "view", "View payroll runs."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10406", "HR", "payroll", "manage", "Run and close payroll periods."),

        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10601", "Platform", "menus", "view", "View menu catalog."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10602", "Platform", "menus", "create", "Create menu items."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10603", "Platform", "menus", "edit", "Update menu items."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10604", "Platform", "menus", "delete", "Delete menu items."),

        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10701", "Shared", "payments", "view", "View payment history and provider readiness."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10702", "Shared", "payments", "create", "Initiate payment checkout flows."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10703", "Shared", "payments", "admin", "Manage M-Pesa payment administration actions."),

        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10801", "ControlPlane", "controlplane", "manage", "Manage Control Plane Tenants and Stamps."),

        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10901", "Membership", "members", "view", "View members."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10902", "Membership", "members", "create", "Create members."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10903", "Membership", "members", "edit", "Update members."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b10904", "Membership", "members", "approve", "Approve member onboarding requests."),

        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b11001", "Loans", "products", "view", "View loan products."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b11002", "Loans", "products", "manage", "Manage loan products."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b11003", "Loans", "applications", "view", "View loan applications."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b11004", "Loans", "applications", "create", "Apply for loans."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b11005", "Loans", "applications", "approve", "Approve loan applications."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b11006", "Loans", "repayments", "create", "Process loan repayments."),

        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b11101", "Accounting", "transactions", "view", "View accounting transactions."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b11102", "Accounting", "transactions", "create", "Post transactions."),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9b11103", "Accounting", "transactions", "approve", "Approve pending transactions."),
        Create("01a20921-9d5f-7902-9430-1ce4ec5562c9", "Accounting", "accounting-integrations", "view", "View integration failure summaries."),
        Create("01a20921-9d63-7c8d-91b5-e505e23f0d68", "Accounting", "accounting-integrations", "retry", "Replay failed accounting events."),
        Create("01a2bd10-4c1e-7a31-9f52-6d0b3e8a1c01", "Accounting", "gl-mappings", "view", "View transaction-type GL mappings."),
        Create("01a2bd10-4c1e-7a31-9f52-6d0b3e8a1c02", "Accounting", "gl-mappings", "manage", "Configure transaction-type GL mappings."),
        Create("01a20921-9d63-74bd-aa17-eeb1ba2ffbbc", "Banking", "banking-products", "view", "View banking products."),
        Create("01a20921-9d63-7d98-a492-2aee5ad38552", "Banking", "banking-products", "manage", "Maintain deposit, savings and share products."),
        Create("01a20921-9d63-7191-a608-ae2202a6708a", "Banking", "banking-accounts", "view", "View member banking accounts."),
        Create("01a20921-9d63-7580-9ac2-4f9a9298c180", "Banking", "banking-accounts", "open", "Open member banking accounts."),
        Create("01a20921-9d63-71ea-bb31-1a70f75367d8", "Banking", "banking-transactions", "post", "Post banking transactions."),
        Create("01a20921-9d63-78ec-90da-6e691c73eac8", "Banking", "teller-tills", "manage", "Open and close teller tills."),
        Create("01a20921-9d63-7d8e-939f-6c2960120723", "Banking", "vault-transfers", "create", "Transfer cash from a teller till to the vault."),
        Create("01a20921-9d64-7157-9414-0262f9f8d530", "CheckOff", "checkoff", "view", "View check-off batches and employers."),
        Create("01a20921-9d64-7b09-913d-2e2aeb429c9f", "CheckOff", "checkoff", "prepare", "Prepare and validate check-off batches."),
        Create("01a20921-9d64-79bf-bdb1-7f4b0dd7ff85", "CheckOff", "checkoff", "post", "Post validated check-off deductions."),
        Create("01a20921-9d64-7425-a488-6ed6904e1fdb", "Loans", "collections", "view", "View collection cases and promises."),
        Create("01a20921-9d64-7399-8bc5-b0fb72d44f60", "Loans", "collections", "manage", "Maintain collection cases, actions and promises."),
        Create("01a20921-9d64-778c-b95e-c79716ba6413", "Loans", "applications", "disburse", "Disburse approved loan applications."),
        Create("01a20929-f9a7-7156-a0dd-0d98c3d5a34a", "HR", "hr.payroll", "view", "View payroll periods, summaries, and payslips."),
        Create("01a20929-f9ab-781c-8205-f7e45e20ff03", "HR", "hr.payroll", "run", "Run payroll for a period."),
        Create("01a20929-f9ab-7b37-8add-a1468ed88e5e", "HR", "hr.payroll", "close", "Close a processed payroll period."),
        Create("01a20929-f9ab-7a9e-a2ab-2dc3066cc88b", "HR", "hr.payroll", "settings", "Read and maintain payroll configuration.")
    ];

    private static Permission Create(string id, string context, string resource, string action, string description)
    {
        var permission = Permission.Create(null, context, resource, action, description, "System");
        permission.Id = Guid.Parse(id);
        permission.TenantId = SeedTenantId;
        permission.CreatedAt = SeedCreatedAt;
        return permission;
    }
}
