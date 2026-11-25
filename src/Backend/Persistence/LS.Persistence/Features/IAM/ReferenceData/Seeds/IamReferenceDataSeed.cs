using LS.Domain.Features.IAM.ReferenceData.Entities;

namespace LS.Persistence.Features.IAM.ReferenceData.Seeds;

internal static class IamReferenceDataSeed
{
    private static readonly DateTimeOffset SeedCreatedAt = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly Guid SeedTenantId = new("0194f700-0000-7000-8000-000000000001");

    internal static IReadOnlyList<PermissionContext> PermissionContexts =>
    [
        CreateContext("018fd81d-2c94-7ad0-a4a3-f1edb9d10101", "Banking", "Banking", "Customer, accounts, loans, and financial operations."),
        CreateContext("018fd81d-2c94-7ad0-a4a3-f1edb9d10102", "HR", "Human Resources", "Departments, employees, and staff operations."),
        CreateContext("018fd81d-2c94-7ad0-a4a3-f1edb9d10103", "IAM", "Identity and Access", "Users, roles, permissions, sessions, and devices."),
        CreateContext("018fd81d-2c94-7ad0-a4a3-f1edb9d10104", "Platform", "Platform", "Cross-cutting platform configuration and navigation."),
        CreateContext("018fd81d-2c94-7ad0-a4a3-f1edb9d10105", "Shared", "Shared", "Cross-cutting platform services and reusable capability showcases."),
        CreateContext("01a20921-9d64-734c-a76d-af07344ad421", "Accounting", "Accounting", "Accounting operations."),
        CreateContext("01a20921-9d64-74cb-924a-c2b7fc80dcfc", "CheckOff", "CheckOff", "CheckOff operations."),
        CreateContext("01a20921-9d64-70bd-8750-72c87b5a85b1", "Loans", "Loans", "Loans operations."),
        CreateContext("01a20ff5-7ec4-7ab9-83de-48bbc150f20c", "ControlPlane", "Control plane", "Platform tenant, stamp and support operations."),
        CreateContext("01a20ff5-7ec4-7b14-bdc8-8fbee734a628", "Dividends", "Dividends", "Dividend declaration and distribution.")
    ];

    internal static IReadOnlyList<PermissionResource> PermissionResources =>
    [
        CreateResource("018fd81d-2c94-7ad0-a4a3-f1edb9d10201", "customers", "Customers", "Banking", "Customer records and onboarding."),
        CreateResource("018fd81d-2c94-7ad0-a4a3-f1edb9d10202", "departments", "Departments", "HR", "Department catalog and staff grouping."),
        CreateResource("018fd81d-2c94-7ad0-a4a3-f1edb9d10203", "employees", "Employees", "HR", "Employee records and IAM linkage."),
        CreateResource("018fd81d-2c94-7ad0-a4a3-f1edb9d10204", "menus", "Menus", "Platform", "Navigation registry and menu visibility."),
        CreateResource("018fd81d-2c94-7ad0-a4a3-f1edb9d10205", "permissions", "Permissions", "IAM", "Permission catalog and assignment surface."),
        CreateResource("018fd81d-2c94-7ad0-a4a3-f1edb9d10206", "roles", "Roles", "IAM", "Role catalog and permission bundles."),
        CreateResource("018fd81d-2c94-7ad0-a4a3-f1edb9d10207", "users", "Users", "IAM", "Application user accounts."),
        CreateResource("018fd81d-2c94-7ad0-a4a3-f1edb9d10208", "payments", "Payments", "Shared", "Payment checkout, status, and provider administration." ),
        CreateResource("01a20921-9d64-7286-ad6a-aa845861e1e1", "accounting-integrations", "Accounting integrations", "Accounting", "Accounting integrations operations."),
        CreateResource("01a2bd10-4c1e-7a31-9f52-6d0b3e8a1c03", "gl-mappings", "GL mappings", "Accounting", "Transaction-type GL mapping configuration."),
        CreateResource("01a20921-9d64-739f-97e8-58248c469132", "banking-products", "Banking products", "Banking", "Banking products operations."),
        CreateResource("01a20921-9d64-7614-bc45-2101b553c789", "banking-accounts", "Banking accounts", "Banking", "Banking accounts operations."),
        CreateResource("01a20921-9d64-78c5-a609-825ab678a8bd", "banking-transactions", "Banking transactions", "Banking", "Banking transactions operations."),
        CreateResource("01a20921-9d64-7238-a409-21e5dd782ad3", "teller-tills", "Teller tills", "Banking", "Teller tills operations."),
        CreateResource("01a20921-9d64-707f-bd5e-99454f749c7b", "vault-transfers", "Vault transfers", "Banking", "Vault transfers operations."),
        CreateResource("01a20921-9d64-7316-9f44-d15fd2a67243", "checkoff", "Check-off", "CheckOff", "Check-off operations."),
        CreateResource("01a20921-9d64-7ad6-9330-b48828498b6c", "collections", "Collections", "Loans", "Collections operations."),
        CreateResource("01a20921-9d64-7afa-abd0-86f52a7c5564", "applications", "Loan applications", "Loans", "Loan applications operations."),
        CreateResource("01a20929-f9ad-7254-9d94-0fb71645df4a", "hr.payroll", "Payroll", "HR", "Payroll operations."),
        CreateResource("01a20ff5-7ec4-737e-aa41-28f32524ab62", "control_plane", "Control plane", "ControlPlane", "Platform tenants, stamps and support."),
        CreateResource("01a20ff5-7ec4-75a4-ab0e-90dcfbe6ecb7", "dividends", "Dividends", "Dividends", "Dividend declarations and distributions.")
    ];

    internal static IReadOnlyList<PermissionAction> PermissionActions =>
    [
        CreateAction("018fd81d-2c94-7ad0-a4a3-f1edb9d10301", "view", "View", "Read and list records."),
        CreateAction("018fd81d-2c94-7ad0-a4a3-f1edb9d10302", "create", "Create", "Create new records."),
        CreateAction("018fd81d-2c94-7ad0-a4a3-f1edb9d10303", "edit", "Edit", "Update existing records."),
        CreateAction("018fd81d-2c94-7ad0-a4a3-f1edb9d10304", "delete", "Delete", "Soft-delete or remove records."),
        CreateAction("018fd81d-2c94-7ad0-a4a3-f1edb9d10305", "deactivate", "Deactivate", "Disable active records or accounts."),
        CreateAction("018fd81d-2c94-7ad0-a4a3-f1edb9d10306", "manage_permissions", "Manage permissions", "Assign or revoke permissions."),
        CreateAction("018fd81d-2c94-7ad0-a4a3-f1edb9d10307", "manage_roles", "Manage roles", "Assign or revoke roles."),
        CreateAction("018fd81d-2c94-7ad0-a4a3-f1edb9d10308", "admin", "Administer", "Perform restricted provider administration actions."),
        CreateAction("01a20921-9d68-7fd6-8137-a19db0ee0d30", "manage", "Manage", "Manage the applicable operation."),
        CreateAction("01a20921-9d68-7409-96fc-ef9be0737d63", "open", "Open", "Open the applicable operation."),
        CreateAction("01a20921-9d68-793f-97cd-1f28fc8e1a3e", "post", "Post", "Post the applicable operation."),
        CreateAction("01a20921-9d68-73d1-b02a-1e46b43bbc6b", "prepare", "Prepare", "Prepare the applicable operation."),
        CreateAction("01a20921-9d68-7968-ae28-c53da13177c8", "retry", "Retry", "Retry the applicable operation."),
        CreateAction("01a20921-9d68-75dc-b278-e86c93cf4bb6", "disburse", "Disburse", "Disburse the applicable operation."),
        CreateAction("01a20929-f9ac-73e4-a2a9-9b58a43faa56", "run", "Run", "Run for the selected operation."),
        CreateAction("01a20929-f9ad-76eb-b38b-2c628ec05f75", "close", "Close", "Close for the selected operation."),
        CreateAction("01a20929-f9ad-7e24-b7e5-875dbc5e193c", "settings", "Settings", "Settings for the selected operation."),
        CreateAction("01a20ff5-7ec4-70fe-a798-acf2360f2db6", "view_tenants", "View Tenants", "View tenants and their status."),
        CreateAction("01a20ff5-7ec4-7847-893c-a4a0fca0de49", "create_tenants", "Create Tenants", "Create tenants."),
        CreateAction("01a20ff5-7ec4-7a77-9b10-5402fd51d4ba", "edit_tenants", "Edit Tenants", "Update tenant details."),
        CreateAction("01a20ff5-7ec4-7894-b42a-86a02c70229e", "manage_tenant_lifecycle", "Manage Tenant Lifecycle", "Activate, suspend and complete provisioning of tenants."),
        CreateAction("01a20ff5-7ec4-79dd-858f-e4f1b4bb973b", "manage_tenant_modules", "Manage Tenant Modules", "Enable or disable tenant modules."),
        CreateAction("01a20ff5-7ec4-7709-8eed-3bcafe603de4", "migrate_tenants", "Migrate Tenants", "Move tenants between deployment stamps."),
        CreateAction("01a20ff5-7ec4-77af-910b-1b80bc330cfe", "approve_kyc", "Approve Kyc", "Approve tenant KYC."),
        CreateAction("01a20ff5-7ec4-7a6b-941d-dbcdc2b3bada", "view_stamps", "View Stamps", "View deployment stamps."),
        CreateAction("01a20ff5-7ec4-7e4e-93bd-93909b3eefa1", "manage_stamps", "Manage Stamps", "Create and update deployment stamps."),
        CreateAction("01a20ff5-7ec4-7ac6-9c6f-dcc4f20e1ffb", "view_audit", "View Audit", "View platform support audit records."),
        CreateAction("01a20ff5-7ec4-72f9-bf56-fbf18492a9b3", "manage_support", "Manage Support", "Start and end support impersonation sessions."),
        CreateAction("01a20ff5-7ec4-7d7f-a190-54bb9c870d49", "approve", "Approve", "Approve the applicable operation.")
    ];

    internal static IReadOnlyList<MenuPlacement> MenuPlacements =>
    [
        CreatePlacement("018fd81d-2c94-7ad0-a4a3-f1edb9d10401", "Sidebar", "Sidebar", "Main application navigation."),
        CreatePlacement("018fd81d-2c94-7ad0-a4a3-f1edb9d10402", "AdminCenter", "Admin Center", "Administration landing tiles."),
        CreatePlacement("018fd81d-2c94-7ad0-a4a3-f1edb9d10403", "ControlPanel", "Control Panel", "Platform control plane tiles.")
    ];

    internal static IReadOnlyList<MenuIcon> MenuIcons =>
    [
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10501", "AccountTree", "Account tree"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10502", "AdminPanelSettings", "Admin panel"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10503", "AutoStories", "Story/book"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10504", "Badge", "Badge"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10505", "Business", "Business"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10506", "Dashboard", "Dashboard"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10507", "Devices", "Devices"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10508", "Group", "Group"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10509", "LockPerson", "Security lock"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10510", "Menu", "Generic menu"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10511", "MenuOpen", "Menu"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10512", "Settings", "Settings"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10513", "CreditCard", "Credit card"),
        CreateIcon("018fd81d-2c94-7ad0-a4a3-f1edb9d10514", "Dns", "DNS/platform")
    ];

    internal static IReadOnlyList<MenuRoute> MenuRoutes =>
    [
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10601", "dashboard", "Dashboard", "/dashboard", "Sidebar"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10602", "admin", "Admin", "/admin", "Sidebar"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10604", "admin-customers", "Customers", "/admin/customers", "AdminCenter"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10605", "admin-departments", "Departments", "/admin/departments", "AdminCenter"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10606", "admin-employees", "Employees", "/admin/employees", "AdminCenter"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10607", "admin-menus", "Menus", "/admin/menus", "AdminCenter"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10608", "admin-permissions", "Permissions", "/admin/permissions", "AdminCenter"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10609", "admin-roles", "Roles", "/admin/roles", "AdminCenter"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10610", "admin-settings", "Settings", "/admin/settings", "AdminCenter"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10611", "admin-user-devices", "User Devices", "/admin/user-devices", "AdminCenter"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10612", "admin-users", "Users", "/admin/users", "AdminCenter"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10613", "admin-access-catalog", "Access Catalog", "/admin/access-catalog", "AdminCenter"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10615", "admin-payments", "Payments", "/admin/payments", "AdminCenter"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10616", "control-panel", "Control Panel", "/control-panel", "Sidebar"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10617", "control-panel-tenants", "Tenants", "/control-panel/tenants", "ControlPanel"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10618", "control-panel-stamps", "Stamps", "/control-panel/stamps", "ControlPanel"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10619", "admin-org-settings", "Org Settings", "/admin/org-settings", "AdminCenter"),
        CreateRoute("018fd81d-2c94-7ad0-a4a3-f1edb9d10620", "admin-iam", "Identity & Access", "/admin/iam", "AdminCenter")
    ];

    private static PermissionContext CreateContext(string id, string key, string label, string description)
    {
        var item = PermissionContext.Create(key, label, description, "System");
        item.Id = Guid.Parse(id);
        item.TenantId = SeedTenantId;
        item.CreatedAt = SeedCreatedAt;
        return item;
    }

    private static PermissionResource CreateResource(string id, string key, string label, string contextKey, string description)
    {
        var item = PermissionResource.Create(key, label, contextKey, description, "System");
        item.Id = Guid.Parse(id);
        item.TenantId = SeedTenantId;
        item.CreatedAt = SeedCreatedAt;
        return item;
    }

    private static PermissionAction CreateAction(string id, string key, string label, string description)
    {
        var item = PermissionAction.Create(key, label, description, "System");
        item.Id = Guid.Parse(id);
        item.TenantId = SeedTenantId;
        item.CreatedAt = SeedCreatedAt;
        return item;
    }

    private static MenuPlacement CreatePlacement(string id, string key, string label, string description)
    {
        var item = MenuPlacement.Create(key, label, description, "System");
        item.Id = Guid.Parse(id);
        item.TenantId = SeedTenantId;
        item.CreatedAt = SeedCreatedAt;
        return item;
    }

    private static MenuIcon CreateIcon(string id, string key, string label)
    {
        var item = MenuIcon.Create(key, label, "Approved MudBlazor icon key.", "System");
        item.Id = Guid.Parse(id);
        item.TenantId = SeedTenantId;
        item.CreatedAt = SeedCreatedAt;
        return item;
    }

    private static MenuRoute CreateRoute(string id, string key, string label, string url, string placementKey)
    {
        var item = MenuRoute.Create(key, label, url, placementKey, "Approved application route.", "System");
        item.Id = Guid.Parse(id);
        item.TenantId = SeedTenantId;
        item.CreatedAt = SeedCreatedAt;
        return item;
    }
}
