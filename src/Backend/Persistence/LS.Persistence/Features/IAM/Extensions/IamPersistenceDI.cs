using LS.Persistence.Common.Configuration;
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
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory_IAM");
                }).ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentNpgsqlMigrationsSqlGenerator>();
            }
            else
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                    sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory_IAM");
                }).ReplaceService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsSqlGenerator, IdempotentSqlServerMigrationsSqlGenerator>();
            }

            // Enable sensitive data logging only in non-production environments for diagnostics
            if (environment?.IsDevelopment() == true || environment?.IsStaging() == true)
            {
                options.EnableSensitiveDataLogging();
            }

            options.AddInterceptors(provider.GetRequiredService<TenantConnectionInterceptor>());
        }

        if (dbSettings.Provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<IamDBContext, IamPostgreSqlDBContext>(ConfigureDbContextOptions);
        }
        else
        {
            services.AddDbContext<IamDBContext, IamSqlServerDBContext>(ConfigureDbContextOptions);
        }

        services.AddScoped<IUserRepository, IamUserRepository>();
        services.AddScoped<ISessionRepository, IamSessionRepository>();
        services.AddScoped<ITokenRepository, IamTokenRepository>();
        services.AddScoped<IAppUserProfileRepository, IamAppUserProfileRepository>();
        services.AddScoped<IAppUserTotpSecretRepository, IamAppUserTotpSecretRepository>();
        services.AddScoped<ITempTotpSecretRepository, IamTempTotpSecretRepository>();
        services.AddScoped<IPermissionRepository, IamPermissionRepository>();
        services.AddScoped<IMenuRepository, IamMenuRepository>();
        services.AddScoped<IIamUnitOfWork, IamUnitOfWork>();

        return services;
    }
}


