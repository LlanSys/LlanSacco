using Hangfire;
using Hangfire.PostgreSql;
using LS.Persistence.Common.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace LS.Api.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        var dbSettings = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>() ?? new DatabaseSettings();

        var connectionString = configuration.GetConnectionString("SharedConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("SharedConnection (or DefaultConnection) not found for Hangfire.");

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings();

            if (dbSettings.Provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
            {
                config.UsePostgreSqlStorage(options =>
                    options.UseNpgsqlConnection(connectionString));
            }
            else
            {
                config.UseSqlServerStorage(connectionString, new Hangfire.SqlServer.SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });
            }
        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
        });

        // Register Jobs
        services.AddTransient<LS.Application.Features.Banking.Savings.Jobs.CalculateSavingsInterestJob>();

        return services;
    }

    public static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder app)
    {
        // Require authorization in a real setup.
        // For now, making it accessible on /hangfire for local development/control plane
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            // Authorization = new[] { new HangfireAuthorizationFilter() }
        });

        // Schedule Background Jobs
        RecurringJob.AddOrUpdate<LS.Application.Features.Banking.Savings.Jobs.CalculateSavingsInterestJob>(
            "calculate-savings-interest",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Daily(23, 0) // Run daily at 11:00 PM
        );

        return app;
    }
}
