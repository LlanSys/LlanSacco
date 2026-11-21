using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;

namespace LS.Persistence.Features.Shared.Migrations.Generators;

public class IdempotentSqlServerMigrationsSqlGenerator(
    MigrationsSqlGeneratorDependencies dependencies,
    ICommandBatchPreparer commandBatchPreparer) : Microsoft.EntityFrameworkCore.Migrations.SqlServerMigrationsSqlGenerator(dependencies, commandBatchPreparer)
{

#pragma warning disable EF1001 // Internal EF Core API usage.

    protected override void Generate(
        CreateIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        var indexName = operation.Name.Replace("'", "''", StringComparison.Ordinal);
        var tableName = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema)
            .Replace("'", "''", StringComparison.Ordinal);
        builder.Append($"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'{indexName}' AND object_id = OBJECT_ID(N'{tableName}'))").AppendLine();
        builder.AppendLine("BEGIN");
        using (builder.Indent())
        {
            base.Generate(operation, model, builder, false);
        }
        builder.AppendLine();
        builder.AppendLine("END");
        if (terminate)
        {
            builder.AppendLine(";");
            builder.EndCommand();
        }
    }
#pragma warning restore EF1001 // Internal EF Core API usage.
}
