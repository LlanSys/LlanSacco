using LS.Persistence.Common.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LS.Persistence.Features.Loans.DataContext;

public class LoansPostgreSqlDBContextFactory : IDesignTimeDbContextFactory<LoansPostgreSqlDBContext>
{
    public LoansPostgreSqlDBContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfigurationFactory.Create();
        var optionsBuilder = new DbContextOptionsBuilder<LoansPostgreSqlDBContext>();
        var connectionString = DesignTimeConfigurationFactory.GetConnectionString(configuration, "BankingConnection", fallbackConnectionName: "DefaultPostgreSqlConnection");

        optionsBuilder.UseNpgsql(
            connectionString,
            sqlOptions => DesignTimeConfigurationFactory.ConfigurePostgreSql(sqlOptions, "__EFMigrationsHistory_Loans")).ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, LS.Persistence.Features.Shared.Migrations.Generators.IdempotentNpgsqlMigrationsSqlGenerator>();
        return new LoansPostgreSqlDBContext(optionsBuilder.Options);
    }
}
