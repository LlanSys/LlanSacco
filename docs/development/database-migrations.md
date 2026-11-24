# Database Migrations Workflow

LlanSacco uses PostgreSQL as its only database provider within a modular monolith architecture. Each bounded context (IAM, Banking, HR, Shared, etc.) has its own single migration chain, generated with that context's `*PostgreSqlDBContext`. SQL Server support and its migrations have been removed.

## Core Principles

1. **Never use the base `DbContext` for migrations.**
   The base `DbContext` classes (e.g., `SharedDBContext`, `IamDBContext`) contain the `DbSet` properties and business logic, but they are NOT used to generate migrations.
2. **Always use the PostgreSQL derived `DbContext`.**
   Migrations MUST be generated using the PostgreSQL contexts (e.g., `SharedPostgreSqlDBContext`, `BankingPostgreSqlDBContext`).
3. **Always output to the context's existing PostgreSQL migration folder.**
   Find it from the location of the context's `*PostgreSqlDBContextModelSnapshot.cs`. Examples:
   - `src/Backend/Persistence/LS.Persistence/Migrations/BankingPostgreSqlDB`
   - `src/Backend/Persistence/LS.Persistence/Migrations/LoansPostgreSqlDB`
   - `src/Backend/Persistence/LS.Persistence/Features/Shared/DataContext/Migrations/SharedPostgreSqlDB`
   - `src/Backend/Persistence/LS.Persistence/Features/ControlPlane/DataContext/Migrations/ControlPlanePostgreSqlDB`
   - `src/Backend/Persistence/LS.Persistence/Features/IAM/DataContext/Migrations/IamPostgreSqlDB`

## How to generate a Migration

Whenever you add a new property, column, class, or make any schema changes, run one migration for the affected context from the repository root:

```powershell
dotnet ef migrations add <MigrationName> `
  --project src/Backend/Persistence/LS.Persistence/LS.Persistence.csproj `
  --startup-project src/Backend/Api/LS.Api/LS.Api.csproj `
  --context <ContextName>PostgreSqlDBContext `
  --output-dir <that context's existing PostgreSQL migration folder>
```

**Example:** Adding a new property to a Shared feature:
```powershell
dotnet ef migrations add AddNewProperty `
  --project src/Backend/Persistence/LS.Persistence/LS.Persistence.csproj `
  --startup-project src/Backend/Api/LS.Api/LS.Api.csproj `
  --context SharedPostgreSqlDBContext `
  --output-dir Features/Shared/DataContext/Migrations/SharedPostgreSqlDB
```

Index creation is made idempotent automatically by `IdempotentNpgsqlMigrationsSqlGenerator`; do not hand-write `IF NOT EXISTS` SQL for indexes.

## Applying Migrations

`ops/migrations/contexts.txt` lists the 10 migration contexts. `scripts/migration-bundles.sh build <output-dir>` builds one PostgreSQL bundle per context, and `scripts/migration-bundles.sh run <dir> <connection-string>` runs them against a PostgreSQL database.

The GitHub Actions workflow `deploy-azure.yml` runs these bundles using the `AZURE_POSTGRES_CONNECTION_STRING` secret; `non-azure-deploy.yml` uses `POSTGRES_CONNECTION_STRING`.

If a migration fails in the pipeline because an object cannot be found, it usually means:
- The migration was accidentally generated against the base `DbContext` rather than the `*PostgreSqlDBContext`.
- An earlier migration was deleted or skipped.
- The snapshot is out of sync.
Always verify that your migration exists in the context's PostgreSQL folder next to its `*PostgreSqlDBContextModelSnapshot.cs`.
