using LS.Persistence.Features.Shared.Migrations.Generators;
using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Accounting.Contracts.Repositories;
using LS.Persistence.Features.Accounting.DataContext;
using LS.Persistence.Features.Accounting.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using LS.Persistence.Common.Interceptors;

namespace LS.Persistence.Features.Accounting.Extensions;

public static class AccountingPersistenceDI
{
    public static IServiceCollection AddAccountingPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AccountingConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("AccountingConnection (or DefaultConnection) not found.");

        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));


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
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory_Accounting");
                })
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>();


            options.AddInterceptors(provider.GetRequiredService<TenantConnectionInterceptor>());
        }

        services.AddDbContext<AccountingDBContext, AccountingPostgreSqlDBContext>(ConfigureDbContextOptions);


        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IJournalRepository, JournalRepository>();
        services.AddScoped<ITransactionTypeGlMappingRepository, TransactionTypeGlMappingRepository>();
        services.AddScoped<IAccountingIntegrationErrorRepository, AccountingIntegrationErrorRepository>();
        services.AddScoped<IAccountingUnitOfWork, AccountingUnitOfWork>();

        return services;
    }
}
