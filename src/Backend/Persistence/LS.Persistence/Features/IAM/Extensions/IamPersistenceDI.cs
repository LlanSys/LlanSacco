using LS.Persistence.Features.IAM.ReferenceData.Repositories;
using LS.Domain.Features.IAM.ReferenceData.Contracts.Repositories;
using LS.Persistence.Features.Shared.Migrations.Generators;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.HR.Employees.Contracts.Repositories;
using LS.Domain.Features.IAM.Menus.Contracts.Repositories;
using LS.Domain.Features.IAM.Permissions.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Persistence.Features.IAM.DataContext;
using LS.Persistence.Features.IAM.Menus.Repositories;
using LS.Persistence.Features.IAM.Permissions.Repositories;
using LS.Persistence.Features.IAM.Users.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using LS.Persistence.Common.Interceptors;

namespace LS.Persistence.Features.IAM.Extensions;

public static class IamPersistenceDI
{
    public static IServiceCollection AddIamPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("IamConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("IamConnection (or DefaultConnection) not found.");


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
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory_IAM");
                })
                .ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>();


            // Enable sensitive data logging only in non-production environments for diagnostics
            if (environment?.IsDevelopment() == true || environment?.IsStaging() == true)
            {
                options.EnableSensitiveDataLogging();
            }

            options.AddInterceptors(provider.GetRequiredService<TenantConnectionInterceptor>());
        }

        services.AddDbContext<IamDBContext, IamPostgreSqlDBContext>(ConfigureDbContextOptions);


        services.AddScoped<IUserRepository, IamUserRepository>();
        services.AddScoped<ISessionRepository, IamSessionRepository>();
        services.AddScoped<ITokenRepository, IamTokenRepository>();
        services.AddScoped<IAppUserProfileRepository, IamAppUserProfileRepository>();
        services.AddScoped<IAppUserTotpSecretRepository, IamAppUserTotpSecretRepository>();
        services.AddScoped<ITempTotpSecretRepository, IamTempTotpSecretRepository>();
        services.AddScoped<IPermissionRepository, IamPermissionRepository>();
        services.AddScoped<IMenuRepository, IamMenuRepository>();
        services.AddScoped<IPermissionContextRepository, PermissionContextRepository>();
        services.AddScoped<IPermissionResourceRepository, PermissionResourceRepository>();
        services.AddScoped<IPermissionActionRepository, PermissionActionRepository>();
        services.AddScoped<IMenuPlacementRepository, MenuPlacementRepository>();
        services.AddScoped<IMenuIconRepository, MenuIconRepository>();
        services.AddScoped<IMenuRouteRepository, MenuRouteRepository>();
        services.AddScoped<IFido2CredentialRepository, Fido2CredentialRepository>();
        services.AddScoped<IIamUnitOfWork, IamUnitOfWork>();

        return services;
    }
}


