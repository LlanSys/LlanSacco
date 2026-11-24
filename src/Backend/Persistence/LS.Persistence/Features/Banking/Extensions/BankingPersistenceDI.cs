using LS.Persistence.Features.Banking.FOSA.Repositories;
using LS.Persistence.Features.Banking.Deposits.Repositories;
using LS.Persistence.Features.Banking.Savings.Repositories;
using LS.Persistence.Features.Banking.Shares.Repositories;
using LS.Domain.Features.Banking.FOSA.Contracts.Repositories;
using LS.Domain.Features.Banking.Deposits.Contracts.Repositories;
using LS.Domain.Features.Banking.Savings.Contracts.Repositories;
using LS.Domain.Features.Banking.Shares.Contracts.Repositories;
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
using LS.Domain.Features.Banking.GuarantorReservations.Contracts.Repositories;
using LS.Persistence.Features.Banking.GuarantorReservations.Repositories;

namespace LS.Persistence.Features.Banking.Extensions;

public static class BankingPersistenceDI
{
    public static IServiceCollection AddBankingPersistence(this IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        var connectionString = configuration.GetConnectionString("BankingConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("BankingConnection (or DefaultConnection) not found.");


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
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory_Banking");
                })
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>();


            options.AddInterceptors(provider.GetRequiredService<TenantConnectionInterceptor>());
        }

        services.AddDbContext<BankingDBContext, BankingPostgreSqlDBContext>(ConfigureDbContextOptions);


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
        services.AddScoped<IGuarantorReservationRepository, GuarantorReservationRepository>();
        services.AddScoped<IGuarantorReservationReceiptRepository, GuarantorReservationReceiptRepository>();

        return services;
    }
}
