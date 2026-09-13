using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace LS.Persistence.Features.Shared.Migrations.Generators;

#pragma warning disable EF1001
public class IdempotentNpgsqlMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies,
    INpgsqlSingletonOptions npgsqlSingletonOptions)
    : Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.NpgsqlMigrationsSqlGenerator(dependencies, npgsqlSingletonOptions)
{
    protected override void Generate(CreateIndexOperation operation, IModel? model,
        MigrationCommandListBuilder builder, bool terminate = true)
    {
        // Preserve provider-generated uniqueness, predicates, included columns and index options.
        var generated = new MigrationCommandListBuilder(Dependencies);
        base.Generate(operation, model, generated, true);
        foreach (var command in generated.GetCommandList())
        {
            var sql = command.CommandText;
            var indexKeyword = sql.IndexOf("INDEX", StringComparison.Ordinal);
            if (indexKeyword < 0) throw new InvalidOperationException("Expected a PostgreSQL index statement.");
            var insertion = indexKeyword + "INDEX".Length;
            if (sql.AsSpan(insertion).StartsWith(" CONCURRENTLY", StringComparison.Ordinal))
                insertion += " CONCURRENTLY".Length;
            builder.Append(sql.Insert(insertion, " IF NOT EXISTS"));
            if (terminate) builder.EndCommand(command.TransactionSuppressed);
        }
    }
}
#pragma warning restore EF1001
