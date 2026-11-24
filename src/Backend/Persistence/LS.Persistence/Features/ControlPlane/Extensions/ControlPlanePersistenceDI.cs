using LS.Persistence.Features.Shared.Migrations.Generators;
using LS.Domain.Features.ControlPlane.Auditing.Contracts.Repositories;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;
using LS.Persistence.Features.ControlPlane.Auditing.Repositories;
using LS.Persistence.Features.ControlPlane.DataContext;
using LS.Persistence.Features.ControlPlane.Tenants.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace LS.Persistence.Features.ControlPlane.Extensions;

public static class ControlPlanePersistenceDI
{
    public static IServiceCollection AddControlPlanePersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("ControlPlaneConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ControlPlaneConnection (or DefaultConnection) not found.");


        void ConfigureDbContextOptions(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(
                connectionString,
                pgOptions =>
                {
                    pgOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
                    pgOptions.CommandTimeout(30);
                    pgOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory_ControlPlane");
                })
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>();


            if (environment?.IsDevelopment() == true || environment?.IsStaging() == true)
            {
                options.EnableSensitiveDataLogging();
            }
        }

        services.AddDbContext<ControlPlaneDBContext, ControlPlanePostgreSqlDBContext>(ConfigureDbContextOptions);


        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IDeploymentStampRepository, DeploymentStampRepository>();
        services.AddScoped<IImpersonationRecordRepository, ImpersonationRecordRepository>();
        services.AddScoped<IControlPlaneUnitOfWork, ControlPlaneUnitOfWork>();

        return services;
    }
}


