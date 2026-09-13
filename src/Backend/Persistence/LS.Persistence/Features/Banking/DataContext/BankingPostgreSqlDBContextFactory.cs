using LS.Persistence.Common.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LS.Persistence.Features.Banking.DataContext;

public class BankingPostgreSqlDBContextFactory : IDesignTimeDbContextFactory<BankingPostgreSqlDBContext>
{
    public BankingPostgreSqlDBContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfigurationFactory.Create();
        var optionsBuilder = new DbContextOptionsBuilder<BankingPostgreSqlDBContext>();
        var connectionString = DesignTimeConfigurationFactory.GetConnectionString(configuration, "BankingConnection", fallbackConnectionName: "DefaultPostgreSqlConnection");

        optionsBuilder.UseNpgsql(
            connectionString,
            sqlOptions => DesignTimeConfigurationFactory.ConfigurePostgreSql(sqlOptions, "__EFMigrationsHistory_Banking")).ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, LS.Persistence.Features.Shared.Migrations.Generators.IdempotentNpgsqlMigrationsSqlGenerator>();
        return new BankingPostgreSqlDBContext(optionsBuilder.Options);
    }
}
