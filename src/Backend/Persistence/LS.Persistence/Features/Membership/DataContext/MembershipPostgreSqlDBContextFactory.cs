using LS.Persistence.Common.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LS.Persistence.Features.Membership.DataContext;

public class MembershipPostgreSqlDBContextFactory : IDesignTimeDbContextFactory<MembershipPostgreSqlDBContext>
{
    public MembershipPostgreSqlDBContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfigurationFactory.Create();
        var optionsBuilder = new DbContextOptionsBuilder<MembershipPostgreSqlDBContext>();
        var connectionString = DesignTimeConfigurationFactory.GetConnectionString(configuration, "MembershipConnection", fallbackConnectionName: "DefaultPostgreSqlConnection");

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => DesignTimeConfigurationFactory.ConfigurePostgreSql(npgsqlOptions, "__EFMigrationsHistory_Membership")).ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, LS.Persistence.Features.Shared.Migrations.Generators.IdempotentNpgsqlMigrationsSqlGenerator>();
        return new MembershipPostgreSqlDBContext(optionsBuilder.Options);
    }
}
