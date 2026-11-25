# Azure CI/CD Configuration

This guide configures the LlanSacco GitHub Actions deployment pipeline to use Microsoft Entra workload identity federation (OIDC). GitHub receives short-lived Azure credentials for each workflow run; no client secret or App Service publish profile is stored in GitHub.

## Identity Boundaries

LlanSacco deliberately uses two identities:

- **GitHub deployment identity:** runs EF migration bundles, manages the temporary Azure Database for PostgreSQL Flexible Server firewall rule, and deploys the API/UI packages.
- **App Service managed identity:** is used by the deployed application at runtime for Key Vault, Blob Storage, Redis, PostgreSQL, and other Azure services.

Do not assume the App Service managed identity is available on a GitHub-hosted runner. It is scoped to the App Service resource.

## 1. Create The GitHub Deployment Identity

In the Azure portal:

1. Open **Microsoft Entra ID**.
2. Open **App registrations** and select **New registration**.
3. Name it, for example, `github-llansacco-production`.
4. Leave the redirect URI empty and complete the registration.
5. Record the **Application (client) ID** and **Directory (tenant) ID**.
6. Open **Certificates & secrets** -> **Federated credentials** -> **Add credential**.
7. Choose **GitHub Actions deploying Azure resources**.
8. Select the repository owner and repository.
9. Set **Entity type** to `Environment`.
10. Set the environment name to `production`.

The environment name must match `environment: production` in `.github/workflows/deploy-azure.yml`.

## 2. Assign Azure RBAC Roles

Assign the deployment identity only the roles needed by the workflow:

- On each target App Service: `Website Contributor`.
- On the Azure Database for PostgreSQL Flexible Server: a role that allows managing its firewall rules (for example `Contributor` scoped to the flexible server, or a custom role with the `Microsoft.DBforPostgreSQL/flexibleServers/firewallRules/*` actions) so the workflow can create and remove its temporary firewall rule.
- If package deployment needs access to other deployment resources, grant the narrowest resource-scoped role that satisfies it.

Avoid subscription-wide `Contributor` unless it is a temporary bootstrap measure.

## 3. Create The PostgreSQL Migration Role

Connect to the target database on the Azure Database for PostgreSQL Flexible Server as its administrator and create a database role that the deployment workflow uses to run migrations. The role must be able to create and alter schema objects and read/write the EF migrations history tables in the target database (for example, the owner of the application schema objects, or a role granted equivalent privileges).

Review generated migrations before release and tighten permissions further if organizational policy requires a custom migration role.

## 4. Configure The GitHub Environment

In GitHub, open **Settings** -> **Environments** -> **production**.

Add these environment variables:

| Variable | Example | Purpose |
| --- | --- | --- |
| `AZURE_CLIENT_ID` | Application client ID | OIDC deployment identity |
| `AZURE_TENANT_ID` | Entra tenant ID | OIDC tenant |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID | Deployment subscription |
| `AZURE_RESOURCE_GROUP` | `rg-llansacco-prod` | Resource group containing the PostgreSQL Flexible Server |
| `AZURE_SQL_SERVER_NAME` | `bt-prod-postgres` | PostgreSQL Flexible Server name without `.postgres.database.azure.com` (the variable keeps its historical name) |
| `AZURE_SQL_NETWORK_MODE` | `PublicRunner` | `PublicRunner` or `PrivateRunner` migration networking strategy for the PostgreSQL Flexible Server (historical name) |
| `AZURE_API_APP_NAME` | `llan-sacco-api-dev` | Exact API App Service resource name (for app-service) |
| `AZURE_UI_APP_NAME` | `llan-sacco-web-dev` | Exact UI App Service resource name (for app-service) |
| `AZURE_DEPLOYMENT_TARGET` | Deprecated | Not read by `deploy-azure.yml`; select `app-service`, `aca-acr`, or `aca-ghcr` in the manual **Run workflow** dialog. Remove this GitHub environment variable to avoid confusion. |
| `AZURE_CONTAINER_REGISTRY_NAME` | `acrllansacco` | ACR Name for container deployments (for aca-acr) |
| `AZURE_ACA_API_NAME` | `ca-llan-sacco-api` | Exact API Container App resource name (for ACA flows) |
| `AZURE_ACA_UI_NAME` | `ca-llan-sacco-blazor` | Exact UI Container App resource name (for ACA flows) |
| `AZURE_SERVICE_BUS_NAMESPACE` | `bt-prod-servicebus` | Service Bus namespace for messaging |

Add this environment secret:

| Secret | Value |
| --- | --- |
| `AZURE_POSTGRES_CONNECTION_STRING` | PostgreSQL connection string for the migration role, shown below |

Example connection string:

```text
Host=<server>.postgres.database.azure.com;Port=5432;Database=<database>;Username=<migration-role>;Password=<password>;Ssl Mode=Require;
```

The former `AZURE_SQL_CONNECTION_STRING` secret is no longer used and can be removed.

Add required reviewers to the `production` environment if deployment approval is required.

## 5. PostgreSQL Flexible Server Networking

The workflow supports two explicit networking modes:

- `PublicRunner`: GitHub-hosted runner using a temporary `/32` PostgreSQL Flexible Server firewall rule (`az postgres flexible-server firewall-rule`).
- `PrivateRunner`: self-hosted runner with private network access; no firewall rule is created.

In `PublicRunner` mode the workflow:

1. Discovering the GitHub runner's public IP.
2. Creating a narrowly scoped temporary flexible-server firewall rule.
3. Running every migration bundle listed in `ops/migrations/contexts.txt`.
4. Removing the firewall rule with `if: always()`.

The deployment identity therefore needs permission to manage firewall rules on the flexible server.

For a private-endpoint-only production architecture, replace the GitHub-hosted migration job with a self-hosted runner inside the Azure virtual network. Do not enable broad public access merely to make CI pass.

## 6. Migration Bundle Behavior

The workflow:

- Builds one self-contained Linux PostgreSQL migration bundle per context in `ops/migrations/contexts.txt` through `scripts/migration-bundles.sh`.
- Passes the PostgreSQL connection string explicitly to each bundle.
- Exposes that same secret through the context-specific `ConnectionStrings__*` environment keys. EF constructs each design-time `DbContext` before processing the bundle's `--connection` override, so both forms are required.
- Retries bundle execution while the database is still starting or unavailable.
- Fails immediately when a bundle reports a deterministic missing-connection configuration error instead of treating it as a transient database delay.
- Runs one bundle per context listed in `ops/migrations/contexts.txt`, each with its own migrations history table (`__EFMigrationsHistory_<Context>`, for example `__EFMigrationsHistory_Banking`).

Migration retries are safe because EF migration bundles are idempotent with respect to their migrations history table. Generated migrations must still be reviewed for operations that are not transaction-safe.

## 7. Deployment Behavior

The API and UI deployment jobs authenticate independently with OIDC because each GitHub job runs on a fresh runner. `azure/webapps-deploy@v3` then deploys to the App Service names supplied by GitHub environment variables.

The old publish-profile secrets are no longer required:

- `AZURE_API_PUBLISH_PROFILE`
- `AZURE_UI_PUBLISH_PROFILE`

Remove them after the first successful OIDC deployment.

## Troubleshooting

- `DefaultAzureCredential failed`: confirm `azure/login@v2` succeeded and the federated credential subject targets the `production` GitHub environment.
- PostgreSQL authentication failure: confirm the role and password in `AZURE_POSTGRES_CONNECTION_STRING` exist on the target database and that the role has migration privileges.
- Database unavailable or starting: the workflow retries; inspect the flexible server status if all retries fail.
- Firewall denial: confirm public network access is enabled for this workflow model and that the deployment identity can manage firewall rules on the flexible server.
- `A valid connection string was not found`: confirm the migration step maps `AZURE_POSTGRES_CONNECTION_STRING` to `ConnectionStrings__IamConnection`, `ConnectionStrings__HrConnection`, `ConnectionStrings__SharedConnection`, `ConnectionStrings__BankingConnection`, `ConnectionStrings__ControlPlaneConnection`, and `ConnectionStrings__DefaultConnection`. The explicit `--connection` argument alone is not available early enough for design-time context construction.
- `Resource ... not found` during deployment: verify `AZURE_API_APP_NAME`, `AZURE_UI_APP_NAME`, subscription, and tenant values.
- OIDC subject mismatch: the federated credential must target environment `production`, not a branch subject, because the workflow jobs use a GitHub environment.
