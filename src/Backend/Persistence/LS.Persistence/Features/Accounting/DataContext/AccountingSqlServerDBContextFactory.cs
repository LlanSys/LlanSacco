using LS.Persistence.Common.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LS.Persistence.Features.Accounting.DataContext;

public class AccountingSqlServerDBContextFactory : IDesignTimeDbContextFactory<AccountingSqlServerDBContext>
{
    public AccountingSqlServerDBContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfigurationFactory.Create();
        var optionsBuilder = new DbContextOptionsBuilder<AccountingSqlServerDBContext>();
        var connectionString = DesignTimeConfigurationFactory.GetConnectionString(configuration, "AccountingConnection", "DefaultSqlConnection");

        optionsBuilder.UseSqlServer(
            connectionString,
            sqlOptions => DesignTimeConfigurationFactory.ConfigureSqlServer(sqlOptions, "__EFMigrationsHistory_Accounting"))
            .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, LS.Persistence.Features.Shared.Migrations.Generators.IdempotentSqlServerMigrationsSqlGenerator>();
        return new AccountingSqlServerDBContext(optionsBuilder.Options);
    }
}
