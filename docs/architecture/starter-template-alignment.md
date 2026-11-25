# StarterTemplate Alignment: A/B Comparison And Porting Order

Date: 2026-11-25 (updated after PRs #21–#25). Base: StarterTemplate (`E:\Repos\StarterTemplate`, prefix ST). Target: LlanSacco (prefix LS).

**Rule:** ST is the base. Every shared area first matches ST, then LS re-applies only SACCO-specific changes. LS-only SACCO domains (Loans, Membership, Accounting, Banking, CheckOff, HR/Payroll, Dividends) are kept as they are and adapted to ST's conventions.

Status key: **same**, **diverged** (LS changed ST's version), **missing** (in ST, not in LS), **LS-only**.

## Why the system administrator only sees "Control Panel"

This blocked the Phase 1 grant/revoke smoke test.

1. LS's `BackendApiClient` never sends the tenant header. LS also has no `WorkspaceSelection`, `FrontendTenantContext` or `WorkspaceSwitcher`, so every call from a platform admin runs in platform scope, and the server returns only platform menus.
2. ST works differently:
   - platform staff enter a tenant workspace through the switcher, a support grant or the demo workspace;
   - the API client then attaches the tenant header;
   - the server returns that tenant's menus;
   - "Return To Platform" leaves the tenant again.

   Each tenant also gets its own administrator through ST's invitation and setup flow.
3. Fixed by PR #23: the menu seed no longer gates **Loan Products** and **Loan Applications** behind a platform permission (they now use `products.view` and `applications.view`).

## Comparison

### Frontend (Blazor)

| Area | Status | Difference | Decision |
| --- | --- | --- | --- |
| App.razor / Routes.razor | diverged → aligned in PR #21 | Session gate, loading screen and host-error screen are ported. Workspace resolve is still deferred. | Finish with the workspace flow |
| Workspace: `FrontendTenantContext`, `WorkspaceSelection`, `WorkspaceSwitcher`, tenant header in `BackendApiClient` | missing | Root cause above | **Port as-is** |
| NavMenu + `TemplateNavigation` | diverged | ST groups sections, hides `/control-panel/*` children, filters to the current workspace and refreshes policy every 15 s. LS only splits menus by admin key. | Port, keep SACCO menu items |
| MainLayout, UserProfileMenu, MudProviderHost | diverged | ST has the workspace-aware user menu and uses code-behind files | Port |
| Theme / app.css | diverged | ST: `--st-*` variables, primary `#334E68`, dynamic `OrganizationBranding`. LS: `--bt-*`, primary `#112250`, hard-coded "LlanSacco", Google Fonts CDN, Bootstrap leftovers. | Port ST's theme, then rebrand through org settings |
| Common: AuthBrand, ControlPanelBackLink, MenuIconPicker, UiLabels | missing | | Port |
| IAM pages: AcceptInvitation, AdministratorInvitation, CompleteAdministratorSetup, ExternalProviderConnection | missing | | Port (follows backend invitations) |
| SignIn, Profile, Dashboard, Admin, AdminIam | diverged | ST has code-behind, log definitions and workspace awareness | Port |
| Control plane: Tenant KYC, WorkspaceSetup, TenantFrontendAddressDialog, PlatformStaff, Platform/Tenant Support, Modules catalogue, Demo, WorkTasks | missing | | Port in later waves |
| OrgSettings | diverged | ST has presentation preferences | Port |
| AdminEmployees / AdminDepartments / EmployeeGrantAccessDialog | LS-only | | Keep, reach through the tenant workspace |
| Shared RCL (`LS.UI.Rcl`), MAUI (`LS.UI.Maui`) | LS-only | Placeholder projects (template `Component1`, default MAUI pages) | Decide: remove or keep as deliberate scaffolds |

### Backend and shared

| Area | Status | Difference | Decision |
| --- | --- | --- | --- |
| Workspace resolve (`PublicWorkspaceController`, `ResolveWorkspaceQuery`, `WorkspaceInfoResponse`) | missing | | **Port as-is** (the frontend depends on it) |
| Tenant isolation: `TenantClaimBoundary`, `LiveAuthority` middleware, `IdentityTenantWriteBoundary` | aligned (PR #24) | `TenantUserStore` not ported | Check that `TenantUserStore` is still needed |
| Demo, SupportSession, ModuleAvailability middleware | missing | | Port with their features |
| Pipeline order | diverged | LS rate-limits **before** authentication, so limits cannot be per-user. LS shows the developer exception page in Development. | Match ST |
| Post-auth chain | diverged | ST: AdministratorSetup. LS: PasswordChangeEnforcement. | Keep both |
| Permissions: `PlatformPermissions` catalog | aligned (PR #23) | `controlplane.manage` retired; each control-plane endpoint uses a specific platform permission | Done |
| Permissions: `PlatformAccess`, `AccountKind`, `IamDelegation`, `RoleAccessRules`, `AdministrationPageAccess`, `CurrentUserPermissions` | missing | | Port in wave 2 |
| Menu seed | diverged | Loans gating fixed in PR #23; the remaining differences are SACCO menu items | Keep SACCO items |
| Navigation: `EffectiveNavigation`, `NavigationOverride`, module catalogue, `ModulePageBindings` | missing | | Port |
| Invitations: `AccountInvitation`, `InviteAdminUser`, `TenantAdministratorInvitation` + worker, `SingleUseCredential` | missing | LS uses `PasswordResetLinkBuilder` and `AccountActivationTokenProvider` | Port, then reconcile with PR #19 activation links |
| Grant/revoke employee access, member portal access, `PasswordChangeEnforcementMiddleware` | LS-only | | Keep |
| SSO / passkeys | diverged | LS has no SSO code store, passkey challenge store or external-identity linking | Port (passkey ownership is a release blocker, RUNTIME007) |
| KYC, blob storage + ClamAV, provisioning workers, support sessions | missing | | Later waves |
| OrgSettings provider | diverged | LS is uncached and has no change audit | Port |
| SignalR hub | diverged | LS doesn't set `CloseOnAuthenticationExpiration` | Port |
| Health checks | diverged | LS has one Postgres check (Postgres-only by decision) | Keep, align naming |
| TrustedProxySettings | missing | | Port |
| GraphQL (HotChocolate) | missing | | Decide: port or explicitly drop |
| Database providers | diverged | LS is Postgres-only by decision (PR #16) | Keep |
| Tenant outbox, Hangfire, `IDomainEventBuffer`, `TransactionLock` | LS-only | | Keep |
| Validator and handler file layout | diverged | Organisation only | Follow AGENTS.md (no churn) |

### Tests, tooling, CI, ops

| Area | Status | Difference | Decision |
| --- | --- | --- | --- |
| `.husky/pre-commit` | diverged, broken | Sources a missing `_/husky.sh` and runs a non-existent `tests/BT.Tests.Architecture` | **Fix now** (ST's hook is also broken: fix both) |
| `core.hooksPath` | LS-only | Points at the main checkout's `.husky`, so `.githooks/pre-push` never runs | Fix with the hook |
| Architecture tests | missing | `ContextDependencyTests`, `ForeignRepositoryProbe` | Port |
| Unit/integration coverage | missing | About 25 ST unit files (IAM, Passkey, Menus/Modules, OrgSettings, ApiExceptionHandler, TenantRoleIsolation) and ST ControlPlane/ApiTests integration suites | Port with each feature |
| Package versions | diverged | LS on 10.0.14 vs ST 10.0.15 (EF/ASP.NET), MediatR 14 vs 15, others behind | Align to ST |
| Dependency policy | missing | No `dependency-policy.yml`, no NU warnings-as-errors, audit level `moderate`, no `dependabot.yml`, CI path triggers miss `Directory.*.props` | Port |
| Directory.Build.props / .editorconfig | diverged | Roslynator formatting analyzers dropped | Port |
| Local setup | missing | `ops/local/.env.example`, `clamd.conf`, `update-local-database`, `Get-LocalQaLogin`, package-management doc | Port |
| LS guardrails engine, debt ledger, remediation CI, gitleaks, CODEOWNERS | LS-only | | Keep |

## Porting order

Each wave is one PR (or a small set), ending with build, tests and a browser check.

0. **Hygiene (independent):** fix the husky hook and hooks path, and align package versions and the dependency policy.
1. **Workspace foundation:** backend workspace resolve plus tenant claim boundary; frontend `FrontendTenantContext`, `WorkspaceSelection`, the tenant header and `WorkspaceSwitcher`. *Unblocks grant/revoke.* Coordinate with the tenancy session.
2. **Permissions and navigation:** ST's platform permission catalog, page access, `EffectiveNavigation`, the menu seed fix and ST's NavMenu. Coordinate with the permissions session.
3. **Shell look:** ST's theme, CSS variables, layouts, common components and `OrganizationBranding`; LlanSacco branding through org settings.
4. **IAM completeness:** invitations, administrator setup, SSO/passkey stores and external identity linking.
5. **Control plane:** platform staff, support sessions, modules catalogue, KYC and provisioning, demo, work tasks.
6. **Re-run the Phase 1 smoke test**, including grant/revoke and session timeout.

## Phase 1 smoke status (2026-11-25, local Docker stack)

| Step | Result |
| --- | --- |
| Sign-in + TOTP, reach dashboard | Pass |
| Refresh on an authorized page | Pass |
| Admin MFA enforcement redirect to `/iam/security`; MFA disable/re-enable | Pass |
| Logout | Pass |
| Signed-out `/iam/sign-in`, `/iam/forgot-password`, `/iam/reset-password` stay put | Pass |
| Grant access email / revoke access | **Blocked**: unreachable from platform scope (wave 1) |
| Session timeout | Skipped by choice |
