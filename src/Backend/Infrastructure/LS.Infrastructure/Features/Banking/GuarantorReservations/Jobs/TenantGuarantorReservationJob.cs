using Hangfire;
using LS.Application.Features.Banking.GuarantorReservations.Commands;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Configuration;
using LS.Infrastructure.Contracts.Implementations.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LS.Infrastructure.Features.Banking.GuarantorReservations.Jobs;

public sealed class TenantGuarantorReservationJob(IServiceScopeFactory scopeFactory,
    IBackgroundJobClient jobs, IOptions<OrgSettings> options)
{
    public async Task ScheduleAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var catalog = scope.ServiceProvider.GetRequiredService<IControlPlaneUnitOfWork>();
        var host = await catalog.TenantRepository.FindByIdAsync(options.Value.DefaultTenantId, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The host tenant is missing from the catalog.");

        var ids = await catalog.TenantRepository.ListAsync(
            q => q.Where(x => x.Status == TenantStatus.Active &&
                x.DeploymentStampId == host.DeploymentStampId)
                .OrderBy(x => x.Id)
                .Select(x => x.Id),
            ct)
            .ConfigureAwait(false);

        foreach (var id in ids)
        {
            ct.ThrowIfCancellationRequested();
            jobs.Enqueue<TenantGuarantorReservationJob>(job => job.RunAsync(id, CancellationToken.None));
        }
    }

    public async Task RunAsync(Guid tenantId, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var catalog = scope.ServiceProvider.GetRequiredService<IControlPlaneUnitOfWork>();
        var host = await catalog.TenantRepository.FindByIdAsync(options.Value.DefaultTenantId, ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The host tenant is missing from the catalog.");

        var tenant = await catalog.TenantRepository.FindByIdAsync(tenantId, ct).ConfigureAwait(false);

        if (tenant is null || tenant.Status != TenantStatus.Active || tenant.DeploymentStampId != host.DeploymentStampId)
        {
            return;
        }

        scope.ServiceProvider.GetRequiredService<BackgroundExecutionContext>().Initialize(tenantId, ICurrentActorProvider.SystemActor);
        var result = await scope.ServiceProvider.GetRequiredService<ISender>().Send(new ExpireGuarantorReservationsCommand(), ct).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException("Guarantor expiry requires reconciliation before retry.");
        }
    }
}
