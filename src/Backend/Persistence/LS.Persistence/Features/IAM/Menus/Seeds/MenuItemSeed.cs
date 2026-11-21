using LS.Domain.Features.IAM.Menus.Entities;

namespace LS.Persistence.Features.IAM.Menus.Seeds;

internal static class MenuItemSeed
{
    private static readonly DateTimeOffset SeedCreatedAt = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly Guid SeedTenantId = new("0194f700-0000-7000-8000-000000000001");

    private static readonly Guid DashboardId = Guid.Parse("018fd81d-2c94-7ad0-a4a3-f1edb9c10101");
    private static readonly Guid AdminId = Guid.Parse("018fd81d-2c94-7ad0-a4a3-f1edb9c10201");
    private static readonly Guid ControlPanelId = Guid.Parse("018fd81d-2c94-7ad0-a4a3-f1edb9c10501");
    private static readonly Guid AccountingId = Guid.Parse("018fd81d-2c94-7ad0-a4a3-f1edb9c10601");
    private static readonly Guid MembershipId = Guid.Parse("018fd81d-2c94-7ad0-a4a3-f1edb9c10701");
    private static readonly Guid LoansId = Guid.Parse("018fd81d-2c94-7ad0-a4a3-f1edb9c10801");
    private static readonly Guid HrId = Guid.Parse("018fd81d-2c94-7ad0-a4a3-f1edb9c10901");

    internal static IReadOnlyList<MenuItem> Items =>
    [
        Create(DashboardId, null, null, "dashboard", "Dashboard", "Operations dashboard.", "/dashboard", "Dashboard", "Sidebar", null, "Core", 1),
        
        Create(AccountingId, null, null, "accounting", "Accounting", "Financial operations and reports.", "/accounting", "AccountBalance", "Sidebar", null, "Core", 10),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c40104", AccountingId, null, "accounting-chart-of-accounts", "Chart of Accounts", "Manage Chart of Accounts.", "/accounting/chart-of-accounts", "AccountTree", "Accounting", null, "Core", 10),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c40105", AccountingId, null, "accounting-journal-entry", "Journal Entry", "Create new journal entries.", "/accounting/journal-entry", "EditNote", "Accounting", null, "Core", 20),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c40101", AccountingId, null, "accounting-trial-balance", "Trial Balance", "View Trial Balance.", "/accounting/trial-balance", "Receipt", "Accounting", null, "Core", 30),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c40102", AccountingId, null, "accounting-income-statement", "Income Statement", "View Income Statement.", "/accounting/income-statement", "TrendingUp", "Accounting", null, "Core", 40),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c40103", AccountingId, null, "accounting-balance-sheet", "Balance Sheet", "View Balance Sheet.", "/accounting/balance-sheet", "AccountBalanceWallet", "Accounting", null, "Core", 50),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c40106", AccountingId, null, "accounting-dlq", "Integration DLQ", "Integration Errors (DLQ).", "/accounting/dlq", "SyncProblem", "Accounting", null, "Core", 60),

        Create(HrId, null, null, "hr", "HR & Payroll", "Human resources and payroll processing.", "/hr", "Badge", "Sidebar", null, "Core", 20),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c70101", HrId, null, "hr-payroll", "Payroll", "Run and manage payroll.", "/hr/payroll", "Payments", "Hr", null, "Core", 10),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c70102", HrId, null, "hr-payroll-settings", "Payroll Settings", "Configure payroll parameters.", "/hr/payroll/settings", "Settings", "Hr", null, "Core", 20),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c70103", HrId, null, "hr-checkoff", "Checkoff", "Manage employer checkoff batches.", "/checkoff/batches", "FactCheck", "Hr", null, "Core", 30),

        Create(LoansId, null, null, "loans", "Loans", "Loan operations and management.", "/loans", "Money", "Sidebar", null, "Core", 30),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c60101", LoansId, null, "loans-products", "Loan Products", "Manage loan products.", "/loans/products", "Category", "Loans", "controlplane.manage", "Core", 10),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c60102", LoansId, null, "loans-applications", "Loan Applications", "Manage loan applications.", "/loans/applications", "Assignment", "Loans", "controlplane.manage", "Core", 20),

        Create(MembershipId, null, null, "membership", "Membership", "Member onboarding and CRM.", "/membership", "People", "Sidebar", null, "Core", 40),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c50101", MembershipId, null, "membership-onboarding", "Member Onboarding", "Onboard new members.", "/membership/onboarding", "PersonAdd", "Membership", null, "Core", 10),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c50102", MembershipId, null, "membership-pending-approvals", "Pending Approvals", "Review and approve pending member applications.", "/membership/pending-approvals", "HowToReg", "Membership", null, "Core", 20),

        Create(AdminId, null, null, "admin", "Admin", "Administrative workspace.", "/admin", "AdminPanelSettings", "Sidebar", null, "Core", 100),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20102", AdminId, null, "admin-departments", "Departments", "Department catalog and staff grouping.", "/admin/departments", "AccountTree", "AdminCenter", null, "Core", 20),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20103", AdminId, null, "admin-employees", "Employees", "Staff records and system access.", "/admin/employees", "Badge", "AdminCenter", null, "Core", 30),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20104", AdminId, null, "admin-menus", "Menus", "Navigation catalog and menu visibility.", "/admin/menus", "MenuOpen", "AdminCenter", null, "Core", 40),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20107", AdminId, null, "admin-org-settings", "Org Settings", "Tenant-specific configuration surface.", "/admin/org-settings", "Settings", "AdminCenter", null, "Core", 50),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20111", AdminId, null, "admin-iam", "Identity & Access", "Manage users, roles, permissions, and trusted devices.", "/admin/iam", "Group", "AdminCenter", null, "IAM", 60),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c20110", AdminId, null, "admin-access-catalog", "Access Catalog", "Source-of-truth permission and menu reference data.", "/admin/access-catalog", "LockPerson", "AdminCenter", null, "IAM", 70),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c10402", AdminId, null, "admin-payments", "Payments", "Test card and mobile-money payment flows.", "/features/payments", "CreditCard", "AdminCenter", "payments.view", "Showcase", 80),

        Create(ControlPanelId, null, null, "control-panel", "Control Panel", "Platform management.", "/control-panel", "Dns", "Sidebar", "controlplane.manage", null, 110),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c30101", ControlPanelId, null, "control-panel-tenants", "Tenants", "Manage SaaS tenants.", "/control-panel/tenants", "Business", "ControlPanel", "controlplane.manage", null, 10),
        Create("018fd81d-2c94-7ad0-a4a3-f1edb9c30102", ControlPanelId, null, "control-panel-stamps", "Stamps", "Deployment stamps.", "/control-panel/stamps", "Dns", "ControlPanel", "controlplane.manage", null, 20)
    ];

    private static MenuItem Create(string id, Guid? parentId, Guid? departmentId, string key, string title, string description, string url, string icon, string placement, string? requiredPermissionKey, string? requiredModule, int displayOrder)
        => Create(Guid.Parse(id), parentId, departmentId, key, title, description, url, icon, placement, requiredPermissionKey, requiredModule, displayOrder);

    private static MenuItem Create(Guid id, Guid? parentId, Guid? departmentId, string key, string title, string description, string url, string icon, string placement, string? requiredPermissionKey, string? requiredModule, int displayOrder)
    {
        var menu = MenuItem.Create(parentId, departmentId, key, title, description, url, icon, placement, requiredPermissionKey, requiredModule, displayOrder, "System");
        menu.Id = id;
        menu.TenantId = SeedTenantId;
        menu.CreatedAt = SeedCreatedAt;
        return menu;
    }
}
