using LS.Persistence.Features.Loans.Collections.Repositories;
using LS.Domain.Features.Loans.Collections.Contracts.Repositories;
using LS.Persistence.Features.Shared.Migrations.Generators;
using LS.Persistence.Features.Loans.DataContext;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Contracts.Repositories;
using LS.Persistence.Features.Loans.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using LS.Persistence.Common.Interceptors;
using System;
using LS.Domain.Features.Loans.Guarantors.Contracts.Repositories;

namespace LS.Persistence.Features.Loans.Extensions;

public static class LoansPersistenceDI
{
    public static IServiceCollection AddLoansPersistence(this IServiceCollection services, IConfiguration configuration)
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
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory_Loans");
                })
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>();


            options.AddInterceptors(provider.GetRequiredService<TenantConnectionInterceptor>());
        }

        services.AddDbContext<LoansDBContext, LoansPostgreSqlDBContext>(ConfigureDbContextOptions);


        services.AddScoped<ILoanProductRepository, LoanProductRepository>();
        services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
        services.AddScoped<ILoanRepaymentRepository, LoanRepaymentRepository>();
        services.AddScoped<ILoanGuarantorRepository, LoanGuarantorRepository>();
        services.AddScoped<ILoanRepaymentScheduleRepository, LoanRepaymentScheduleRepository>();
        services.AddScoped<ICollectionCaseRepository, CollectionCaseRepository>();
        services.AddScoped<ICollectionActionRepository, CollectionActionRepository>();
        services.AddScoped<ICollectionPromiseRepository, CollectionPromiseRepository>();
        services.AddScoped<ILoansUnitOfWork, LoansUnitOfWork>();
        services.AddScoped<IGuarantorRecoveryRepository, GuarantorRecoveryRepository>();
        services.AddScoped<IGuarantorSecurityReviewRepository, GuarantorSecurityReviewRepository>();

        return services;
    }
}
