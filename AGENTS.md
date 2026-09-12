# LlanSacco Agent Instructions

This file is the canonical source of truth and working contract for AI coding tools in this repository. If another tool-specific file conflicts with this one, this file wins and the other file should be updated.

## Start Every Coding Or Debugging Task

1. Read this `AGENTS.md` first.
2. Read `PLAN.md` and `PLAN_EXECUTION_STRATEGY.md` before phase or architecture work.
3. Read the closest relevant docs before changing code:
   - `docs/architecture/feature-folder-convention.md`
   - `docs/architecture/persistence-standards.md`
   - `docs/architecture/saas-multitenancy-strategy.md`
   - `docs/architecture/ui-to-backend-flow.md`
   - `docs/development/environment-configuration-checklist.md`
   - `docs/development/local-platform.md`
   - `docs/development/azure-storage-configuration.md`
   - `docs/development/non-azure-deployment.md`
   - `docs/development/configuration-code-conventions.md`
4. Inspect the existing code before proposing or writing changes.
5. If a convention changes, update this file and the relevant docs in the same PR.

## Project Identity

- Repo: `LlanSys/LlanSacco`
- Product: LlanSacco
- Root namespace prefix: `LS`
- LlanSacco is the SACCO product, derived from BaseTemplate. Do not rerun template renaming on this established application.
- After protected data has been issued, `DataProtection:ApplicationName` is immutable without an explicit migration or security reset.
- Target framework: `.NET 10`
- Architecture: modular monolith with Clean Architecture boundaries and bounded-context feature folders.
- Current bounded contexts: `Accounting`, `Banking`, `CheckOff`, `ControlPlane`, `Dividends`, `HR`, `IAM`, `Loans`, `Membership`, and `Shared`. The machine-readable registry is `docs/architecture/bounded-contexts.json`.

## Architecture Rules

- Dependency direction is inward: `Domain <- Application <- Infrastructure/Persistence <- Api/UI`.
- `Domain` must not reference Application, Infrastructure, Persistence, API, or UI.
- `Application` must not reference Infrastructure or Persistence.
- `SharedKernel` must not reference Domain, Infrastructure, or Persistence.
- Infrastructure and Persistence implement contracts; they do not define business policy.
- Keep feature-owned code inside `Features/{Context}/{Feature}` in every layer where that code exists.
- Keep cross-cutting concerns outside feature folders: logging, middleware, generic caching, generic repositories, JSON helpers, base entities, and shared abstractions.

## Backend Conventions

- Use MediatR request/handler flows already established in the repo.
- Commands/queries and handlers should remain feature-owned.
- Each public top-level type normally occupies its own type-named file. The narrow default exception is a command slice: `CreateOrder.cs` contains `CreateOrderCommand` and its internal `CreateOrderCommandHandler`, and no unrelated types. Keep Command/Handler type suffixes; omit them from the paired file name. Separate command/handler files require a reviewed, exact exception. Validators always occupy separate type-named files, including internal validators: `Validators/CreateOrderCommandValidator.cs`. Do not bundle unrelated DTOs, settings, entities, or helpers. Partial generated companions retain their existing type-based naming.
- Use C# 12 Primary Constructors where applicable for classes, records, and struct declarations (e.g. controllers, handlers, services) instead of traditional constructor boilerplate.
- Use `Request` and `Response` suffixes for transport models instead of `Dto`. Do not mirror Domain enums to SharedKernel; use `string` properties in transport records and map them using `ToEnum()` at the boundaries.
- Use `AppResponse<T>` and existing `AppResponses`/`AppError` factories for expected business outcomes. Failure envelopes must contain a typed Error; Message alone is insufficient. Preserve the response contract and use the correct NotFound/Forbidden/Validation/BusinessRule semantics.
- Throw exceptions only for unexpected or exceptional failures.
- API responses must go through the established response/problem-details pattern.
- Do not leak exception type names, stack traces, provider errors, connection strings, or internal IDs into user-facing messages.
- All user-facing errors from the UI must be sanitized through the shared messaging pattern.
- Provider-specific integration DTOs belong beside the provider adapter. Shared contracts should stay provider-neutral and should not expose Stripe, M-Pesa, Azure, or other provider wire payloads.
- When a capability supports multiple runtime providers per operation, use a router/factory over isolated provider adapters. Do not make one provider adapter understand another provider's DTOs, credentials, or callbacks.
- Application-layer command/query handlers must use bounded-context Unit of Work interfaces (`ISharedUnitOfWork`, `IBankingUnitOfWork`, etc.) for persistence orchestration, not raw `IRepository<T>`. Stage writes and commit once with a `CancellationToken`: use `CompleteAsync(ct)` for the ordinary path, or let `ExecuteInTransactionAsync(operation, ct)` own its save/commit. Do not add a second save merely to satisfy this rule. Pass the request token to retry helpers, re-read mutable state inside every retry callback, and keep irreversible external side effects outside retries. Define domain-event semantics before selecting another transaction helper.
- Pass `CancellationToken` to all async I/O calls (`ReadAsStringAsync`, `CompleteAsync`, `SaveChangesAsync`, etc.) where the parameter is available.
- Use `StringComparison.OrdinalIgnoreCase` for case-insensitive comparisons. Reserve `ToUpperInvariant()`/`ToLowerInvariant()` for value normalization (stored keys, wire formats, display strings) — never for equality checks. C# `switch` on normalized strings is acceptable when `StringComparison` is not supported by the expression.
- Always use `Guid.CreateVersion7()` instead of `Guid.NewGuid()` to ensure time-ordered identifiers.

## Persistence Rules

- Every declared `DbSet<T>` entity must have explicit EF configuration.
- Custom persistence context-related types use the project `DB` acronym, for example `IamDBContext`, `DBContextHelper`, and `ITenantFilteredDBContext`; keep Microsoft framework API names unchanged.
- Respect tenant isolation, soft delete, and audit actor conventions.
- `CreatedBy`, `UpdatedBy`, `ActivatedBy`, `DeactivatedBy`, and `DeletedBy` must store stable actor identifiers, not display names or labels such as `DevelopmentSeed`.
- Every Unit of Work repository property uses a feature-specific interface and the `Repository` suffix, for example `IMemberTransactionRepository MemberTransactionRepository`. Entity repository interfaces inherit `IRepository<TEntity>`; never expose raw `IRepository<TEntity>` from a Unit of Work. Inject implementations; never construct generic repositories in property getters. Specialized non-entity/security contracts require a documented exception to inheritance, not naming.
- Prefer inherited generic repository methods unless a concrete repository method is persistence-specific. Use specifications for reusable query rules; do not invent aliases such as GetAllAsync/GetByIdAsync when ListAsync/FindByIdAsync already exist.
- Read-only generic methods are no-tracking. Mutations must use tracked retrieval or explicitly stage changes with UpdateAsync/UpdateRangeAsync before the Unit of Work commits.
- Compose queries before materialization. Do not call `ToListAsync` before filters, tenant scope, paging, projection, or security scope are applied.
- Use `Any` for existence checks, not `Count > 0`.
- Use `CountAsync` only when the count is returned, logged, or used for a decision.
- Avoid N+1 query patterns. Batch identifiers and map results in memory.
- Cursor pagination must fetch `pageSize + 1` before trimming.
- **LlanSacco supports multiple database providers (PostgreSQL and SQL Server).** When adding EF Core migrations, you MUST generate separate migrations for each provider's DbContext and specify the correct output directory. For example, for the IAM bounded context: `dotnet ef migrations add <Name> -c IamSqlServerDBContext -o Migrations/IAM` and `dotnet ef migrations add <Name> -c IamPostgreSqlDBContext -o Migrations/IamPostgreSqlDB`. Do not rely on default output directories.
- **Database Context Factories:** When creating `IDesignTimeDbContextFactory<T>` implementations, always specify a provider-specific fallback connection string using `DesignTimeConfigurationFactory.GetConnectionString(..., fallbackConnectionName: "DefaultPostgreSqlConnection")` (or `"DefaultSqlConnection"`) to prevent parsing crashes from incompatible connection string parameters.
- **Idempotent Migrations:** EF Core index creations are globally intercepted by custom SQL generators (`IdempotentSqlServerMigrationsSqlGenerator` and `IdempotentNpgsqlMigrationsSqlGenerator`) to inject `IF NOT EXISTS` logic. Do not write manual raw SQL for idempotency on index creation—the generator handles this automatically.

## Validation, Logging, And Caching

- Add FluentValidation validators for write requests that can accept user input. Keep each validator in a separate type-named file under its feature and prove it is discovered by DI and runs through MediatR. Internal validators require explicit scanning/registration; file placement alone does not prove execution.
- Use source-generated `LoggerMessage` methods; do not add free-form logging in new code paths.
- Every `catch` block must log or deliberately translate the error into an expected result.
- Every query needs an explicit cache policy through `ICachableRequest`; sensitive/fresh reads use BypassCache deliberately. Every command needs a cache-impact decision: use `ICacheInvalidatorRequest` for affected cached reads or a reviewed exact no-cached-dependents decision. Do not add empty marker implementations just to pass checks. Invalidation follows a successful committed outcome; failed results must not be cached. Validate behavior ordering, tenant/user/permission scope, and non-MediatR writers.
- Keep cache keys tenant-aware where data is tenant-scoped.

## IAM And Security

- IAM is a core platform capability, not an application-specific feature.
- Administrative endpoints require permission-based authorization, not only `[Authorize]`.
- Admin MFA enforcement is intentional; non-admin MFA can be policy-driven.
- Session lifecycle must expire cleanly, warn before timeout, and avoid stale UI state.
- Blazor tokens use protected browser storage with a circuit-scoped in-memory fallback; do not mirror bearer or refresh tokens into Redis.
- Profile images are private assets. Use API-mediated access, not direct public blob URLs.
- Local secrets belong in `dotnet user-secrets`; production secrets belong in Azure Key Vault/App Service configuration.
- Never commit secrets, connection strings, app passwords, tokens, private keys, or local temp files.

## Frontend Conventions

- Blazor UI talks to the backend API boundary for app flows.
- Use MudBlazor components where practical and keep UI compact, responsive, and consistent.
- Use `wwwroot/css` for app CSS.
- Avoid JavaScript unless Blazor cannot access the browser capability directly.
- Tables should support compact layout, horizontal overflow, pagination/search/filter patterns, and consistent actions.
- Listing pages follow the Customer hierarchy: create actions occupy a separate top-right page action row, shown-count badges occupy the list heading's right edge, and search/filter controls occupy a full-width row below the heading.
- New route guards must use native Blazor authorization: `@attribute [Authorize]`, `@layout` with a policy-protected layout, or `<AuthorizeView>`. Do not add new ad-hoc `HasPermission()` checks for route-level gating; leave existing `HasPermission()` calls for inline UI visibility unchanged.
- Control Plane pages must use `ControlPlaneLayout` (`@layout ControlPlaneLayout`), which enforces the `ControlPlane.Manage` policy at the layout level before `OnInitializedAsync` fires.

## Azure And Configuration

- Every settings POCO must have a matching JSON/config section.
- Parse configurable modes, providers, transports, and strategies once into typed values, then use `switch` branches with fail-fast unsupported-value errors.
- Keep technical configuration enums in the owning layer; reserve Domain enums for business language and invariants.
- Prefer Azure managed identity in deployed Azure environments.
- GitHub Actions authenticates to Azure through OIDC workload identity federation; do not add publish profiles or long-lived Azure client secrets to deployment workflows.
- Use connection strings only for local development or controlled non-managed-identity scenarios.
- Key Vault secret names use double hyphen mapping, for example `Section--Setting`.
- Environment variables use double underscore mapping, for example `Section__Setting`.
- Production email delivery must use approved provider API adapters. Personal mailbox SMTP is not a production path; Mailpit SMTP is local-only capture infrastructure.
- Keep Azure setup notes in `docs/development/azure-storage-configuration.md` and the checklist in `docs/development/environment-configuration-checklist.md`.
- Run application hosts from Visual Studio and local infrastructure through `ops/local/docker-compose.yml`; do not replace or remove Azure production provider configuration when adding local providers.

## Workflow Rules

- Preserve user changes. Do not revert unrelated work.
- Architecture debt is recorded exactly in `docs/architecture/architecture-debt.json`, with rule, file, symbol, evidence, and owning remediation phase. CI rejects new violations and stale entries. Never regenerate or expand the register to hide a regression; remove resolved entries in the fixing PR. See `docs/architecture/architecture-guardrails.md`.
- Prefer small, focused commits with clear messages.
- When adding or modifying constructor dependencies for core abstractions (like `DbContext`), ensure the dependency injection configurations in the integration tests are also updated to provide those services.
- Before opening or updating a PR, run:
  - `dotnet build src\Backend\Api\LS.Api\LS.Api.csproj --no-restore -p:UseSharedCompilation=false`
  - `dotnet test tests\LS.Tests.Architecture\LS.Tests.Architecture.csproj --no-restore -p:UseSharedCompilation=false`
- During remediation, deployment and provisioning are manual-only and require `LLANSACCO_DEPLOYMENT_ENABLED=true`. Publishing images requires manual dispatch plus `LLANSACCO_PUBLISH_ENABLED=true`. Keep these disabled until LlanSacco targets and credentials are verified. Import and remediation changes use PRs to `main`; `Required / Remediation` must pass before merge.
- PRs should explain what changed, why, impact, and checks.
- If a review comment is valid and small, update the same PR branch.
- If a review comment changes scope materially, discuss before expanding the PR.

## Tool-Specific Entry Files

The following files exist only to point other AI tools back to this canonical file:

- `.github/copilot-instructions.md`
- `CLAUDE.md`
- `GEMINI.md`
- `.cursor/rules/llan-sacco.mdc`
- `.windsurfrules`

Keep these files short and aligned with this file. Do not let them become independent rulebooks.
