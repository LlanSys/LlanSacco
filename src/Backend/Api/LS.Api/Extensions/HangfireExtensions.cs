using Hangfire;
using Hangfire.PostgreSql;
using LS.Infrastructure.Features.Banking.Savings.Jobs;
using LS.Infrastructure.Features.Banking.GuarantorReservations.Jobs;
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

        var connectionString = configuration.GetConnectionString("SharedConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("SharedConnection (or DefaultConnection) not found for Hangfire.");

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

            config.UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(connectionString));

        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
        });

        services.AddTransient<TenantSavingsInterestJob>();
        services.AddTransient<TenantGuarantorReservationJob>();
        services.AddTransient<LS.Application.Features.Banking.Savings.Jobs.CalculateSavingsInterestJob>();

        return services;
    }

    public static IApplicationBuilder UseHangfireDashboard(this IApplicationBuilder app)
    {
        // Require authorization in a real setup.
        // For now, making it accessible on /hangfire for local development/control plane
        app.UseHangfireDashboard(
            "/hangfire",
            new DashboardOptions
            {
                // Authorization = new[] { new HangfireAuthorizationFilter() }
            });

        // Schedule Background Jobs
        RecurringJob.AddOrUpdate<TenantGuarantorReservationJob>(
            "expire-guarantor-reservations", job => job.ScheduleAsync(CancellationToken.None), "*/5 * * * *");

        RecurringJob.AddOrUpdate<TenantSavingsInterestJob>(
            "calculate-savings-interest",
            job => job.ScheduleAsync(CancellationToken.None),
            Cron.Daily(23, 0) // Run daily at 11:00 PM
        );

        return app;
    }
}
