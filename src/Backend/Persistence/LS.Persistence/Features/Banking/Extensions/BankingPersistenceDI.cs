using LS.Persistence.Features.Banking.FOSA.Repositories;
using LS.Persistence.Features.Banking.Deposits.Repositories;
using LS.Persistence.Features.Banking.Savings.Repositories;
using LS.Persistence.Features.Banking.Shares.Repositories;
using LS.Domain.Features.Banking.FOSA.Contracts.Repositories;
using LS.Domain.Features.Banking.Deposits.Contracts.Repositories;
using LS.Domain.Features.Banking.Savings.Contracts.Repositories;
using LS.Domain.Features.Banking.Shares.Contracts.Repositories;
using LS.Persistence.Common.Configuration;
using LS.Persistence.Features.Shared.Migrations.Generators;
using LS.Persistence.Features.Banking.DataContext;
using LS.Domain.Features.Banking.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using LS.Persistence.Common.Interceptors;
using System;

namespace LS.Persistence.Features.Banking.Extensions;

public static class BankingPersistenceDI
{
    public static IServiceCollection AddBankingPersistence(this IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        var connectionString = configuration.GetConnectionString("BankingConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("BankingConnection (or DefaultConnection) not found.");

        services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName));
        var dbSettings = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>() ?? new DatabaseSettings();

        services.TryAddScoped<TenantConnectionInterceptor>();

        void ConfigureDbContextOptions(IServiceProvider provider, DbContextOptionsBuilder options)
        {
            if (dbSettings.Provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString, pgOptions =>
                {
                    pgOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
                    pgOptions.CommandTimeout(30);
                    pgOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory_Banking");
                }).ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>();
            }
            else
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                    sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_Banking");
                }).ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentSqlServerMigrationsSqlGenerator>();
            }
            
            options.AddInterceptors(provider.GetRequiredService<TenantConnectionInterceptor>());
        }

        if (dbSettings.Provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<BankingDBContext, BankingPostgreSqlDBContext>(ConfigureDbContextOptions);
        }
        else
        {
            services.AddDbContext<BankingDBContext, BankingSqlServerDBContext>(ConfigureDbContextOptions);
        }

        services.AddScoped<IVaultTransferRepository, VaultTransferRepository>();
        services.AddScoped<IShareProductRepository, ShareProductRepository>();
        services.AddScoped<IShareAccountRepository, ShareAccountRepository>();
        services.AddScoped<IShareTransactionRepository, ShareTransactionRepository>();
        services.AddScoped<ISavingsProductRepository, SavingsProductRepository>();
        services.AddScoped<ISavingsAccountRepository, SavingsAccountRepository>();
        services.AddScoped<ISavingsTransactionRepository, SavingsTransactionRepository>();
        services.AddScoped<IDepositProductRepository, DepositProductRepository>();
        services.AddScoped<IDepositAccountRepository, DepositAccountRepository>();
        services.AddScoped<IDepositTransactionRepository, DepositTransactionRepository>();
        services.AddScoped<IFosaAccountRepository, FosaAccountRepository>();
        services.AddScoped<IFosaTransactionRepository, FosaTransactionRepository>();
        services.AddScoped<ITellerTillRepository, TellerTillRepository>();
        services.AddScoped<ITillBalancingRecordRepository, TillBalancingRecordRepository>();
        services.AddScoped<IVaultRepository, VaultRepository>();
        services.AddScoped<IBankingUnitOfWork, BankingUnitOfWork>();

        return services;
    }
}
