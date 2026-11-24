using LS.Persistence.Features.Shared.Migrations.Generators;
using LS.Domain.Features.Membership.Entities;
using LS.Persistence.Features.Membership.DataContext;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Contracts.Repositories;
using LS.Persistence.Features.Membership.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using LS.Persistence.Common.Interceptors;
using System;

namespace LS.Persistence.Features.Membership.Extensions;

public static class MembershipPersistenceDI
{
    public static IServiceCollection AddMembershipPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MembershipConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("MembershipConnection (or DefaultConnection) not found.");


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
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory_Membership");
                })
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>();


            options.AddInterceptors(provider.GetRequiredService<TenantConnectionInterceptor>());
        }

        services.AddDbContext<MembershipDBContext, MembershipPostgreSqlDBContext>(ConfigureDbContextOptions);


        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IMemberAccountRepository, MemberAccountRepository>();
        services.AddScoped<IMemberTransactionRepository, MemberTransactionRepository>();
        services.AddScoped<IMemberGuarantorRepository, MemberGuarantorRepository>();
        services.AddScoped<IGuarantorRequestRepository, GuarantorRequestRepository>();
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        services.AddScoped<IMembershipUnitOfWork, MembershipUnitOfWork>();

        return services;
    }
}
