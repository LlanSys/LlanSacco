using Hangfire;
using LS.Application.Features.Banking.Savings.Jobs;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Configuration;
using LS.Infrastructure.Contracts.Implementations.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LS.Infrastructure.Features.Banking.Savings.Jobs;

public sealed class TenantSavingsInterestJob(
    IServiceScopeFactory scopeFactory,
    IBackgroundJobClient jobs,
    IOptions<OrgSettings> options)
{
    public async Task ScheduleAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var catalog = scope.ServiceProvider.GetRequiredService<IControlPlaneUnitOfWork>();
        var hostTenant = await catalog.Tenants.FindByIdAsync(options.Value.DefaultTenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The host tenant is missing from the tenant catalog.");
        var tenantIds = await catalog.Tenants.ListAsync(q => q
            .Where(tenant => tenant.DeploymentStampId == hostTenant.DeploymentStampId && tenant.Status == TenantStatus.Active)
            .OrderBy(tenant => tenant.Id).Select(tenant => tenant.Id), cancellationToken).ConfigureAwait(false);

        foreach (var tenantId in tenantIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            jobs.Enqueue<TenantSavingsInterestJob>(job => job.RunAsync(tenantId, CancellationToken.None));
        }
    }

    public async Task RunAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var catalog = scope.ServiceProvider.GetRequiredService<IControlPlaneUnitOfWork>();
        var hostTenant = await catalog.Tenants.FindByIdAsync(options.Value.DefaultTenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The host tenant is missing from the tenant catalog.");
        var tenant = await catalog.Tenants.FindByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null || tenant.Status != TenantStatus.Active || tenant.DeploymentStampId != hostTenant.DeploymentStampId)
            return;

        scope.ServiceProvider.GetRequiredService<BackgroundExecutionContext>()
            .Initialize(tenantId, ICurrentActorProvider.SystemActor);
        await scope.ServiceProvider.GetRequiredService<CalculateSavingsInterestJob>()
            .ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}
