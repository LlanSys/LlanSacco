using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace LS.Persistence.Features.Shared.Migrations.Generators;

public class IdempotentNpgsqlMigrationsSqlGenerator(
    MigrationsSqlGeneratorDependencies dependencies,
    INpgsqlSingletonOptions npgsqlSingletonOptions) : Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.NpgsqlMigrationsSqlGenerator(dependencies, npgsqlSingletonOptions)
{

#pragma warning disable EF1001 // Internal EF Core API usage.

    protected override void Generate(
        CreateIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        builder.Append($"CREATE INDEX IF NOT EXISTS ");
        
        var indexName = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name);
        builder.Append(indexName).Append(" ON ");
        
        if (operation.Schema != null)
        {
            builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Schema)).Append(".");
        }
        
        builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table)).Append(" (");
        
        for (int i = 0; i < operation.Columns.Length; i++)
        {
            builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Columns[i]));
            if (i < operation.Columns.Length - 1)
            {
                builder.Append(", ");
            }
        }
        
        builder.Append(")");
        
        if (terminate)
        {
            builder.AppendLine(";");
        }
    }
#pragma warning restore EF1001 // Internal EF Core API usage.
}
