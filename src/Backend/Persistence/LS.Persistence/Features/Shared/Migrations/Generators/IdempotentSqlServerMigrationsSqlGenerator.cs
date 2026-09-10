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
        builder.Append($"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = '{operation.Name}' AND object_id = OBJECT_ID('{operation.Table}'))").AppendLine();
        builder.AppendLine("BEGIN");
        using (builder.Indent())
        {
            base.Generate(operation, model, builder, false);
        }
        builder.AppendLine("END");
        if (terminate)
        {
            builder.AppendLine(";");
        }
    }
#pragma warning restore EF1001 // Internal EF Core API usage.
}
