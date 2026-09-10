using LS.Domain.Features.Dividends.Contracts;
using LS.Persistence.Common.Configuration;
using LS.Persistence.Common.Interceptors;
using LS.Persistence.Features.Dividends.Contracts;
using LS.Persistence.Features.Dividends.DataContext;
using LS.Persistence.Features.Dividends.Providers.PostgreSql;
using LS.Persistence.Features.Dividends.Providers.SqlServer;
using LS.Persistence.Features.Shared.Migrations.Generators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Reflection;

namespace LS.Persistence.Features.Dividends.Extensions;

public static class DividendsPersistenceDI
{
    public static IServiceCollection AddDividendsPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found.");

        services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName));
        var dbSettings = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>() ?? new DatabaseSettings();

        services.TryAddSingleton<TenantConnectionInterceptor>();

        void ConfigureDbContextOptions(IServiceProvider provider, DbContextOptionsBuilder options)
        {
            if (dbSettings.Provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString, pgOptions =>
                {
                    pgOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
                    pgOptions.CommandTimeout(30);
                    pgOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory_Dividends");
                }).ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>();
            }
            else
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                    sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Dividends");
                }).ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentSqlServerMigrationsSqlGenerator>();
            }
            
            options.AddInterceptors(provider.GetRequiredService<TenantConnectionInterceptor>());
        }

        if (dbSettings.Provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<DividendsDBContext, DividendsPostgreSqlDBContext>(ConfigureDbContextOptions);
        }
        else
        {
            services.AddDbContext<DividendsDBContext, DividendsSqlServerDBContext>(ConfigureDbContextOptions);
        }

        services.AddScoped<IDividendsUnitOfWork, DividendsUnitOfWork>();

        return services;
    }
}
