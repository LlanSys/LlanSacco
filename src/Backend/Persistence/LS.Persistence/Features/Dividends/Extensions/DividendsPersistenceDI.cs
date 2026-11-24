using LS.Persistence.Features.Dividends.Repositories;
using LS.Domain.Features.Dividends.Contracts.Repositories;
using LS.Domain.Features.Dividends.Contracts;
using LS.Persistence.Common.Interceptors;
using LS.Persistence.Features.Dividends.DataContext;
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


        services.TryAddScoped<TenantConnectionInterceptor>();

        void ConfigureDbContextOptions(IServiceProvider provider, DbContextOptionsBuilder options)
        {
            options.UseNpgsql(
                connectionString,
                pgOptions =>
                {
                    pgOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
                    pgOptions.CommandTimeout(30);
                    pgOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory_Dividends");
                })
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>();


            options.AddInterceptors(provider.GetRequiredService<TenantConnectionInterceptor>());
        }

        services.AddDbContext<DividendsDBContext, DividendsPostgreSqlDBContext>(ConfigureDbContextOptions);


        services.AddScoped<IDividendDeclarationRepository, DividendDeclarationRepository>();
        services.AddScoped<IDividendCalculationRepository, DividendCalculationRepository>();
        services.AddScoped<IDividendDistributionPreferenceRepository, DividendDistributionPreferenceRepository>();
        services.AddScoped<IDividendsUnitOfWork, DividendsUnitOfWork>();

        return services;
    }
}
