using LS.Domain.Features.ControlPlane.Auditing.Contracts.Repositories;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;
using LS.Persistence.Features.ControlPlane.DataContext;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Persistence.Features.ControlPlane;

public class ControlPlaneUnitOfWork(
    ControlPlaneDBContext context,
    ITenantRepository tenants,
    IDeploymentStampRepository deploymentStamps,
    IImpersonationRecordRepository impersonationRecords) : IControlPlaneUnitOfWork
{
    private readonly ControlPlaneDBContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public ITenantRepository Tenants { get; } = tenants ?? throw new ArgumentNullException(nameof(tenants));
    public IDeploymentStampRepository DeploymentStamps { get; } = deploymentStamps ?? throw new ArgumentNullException(nameof(deploymentStamps));
    public IImpersonationRecordRepository ImpersonationRecords { get; } = impersonationRecords ?? throw new ArgumentNullException(nameof(impersonationRecords));

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
