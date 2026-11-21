using LS.Domain.Features.ControlPlane.Auditing.Contracts.Repositories;
using LS.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Domain.Features.ControlPlane.Contracts;

public interface IControlPlaneUnitOfWork : IDisposable, IAsyncDisposable
{
    ITenantRepository TenantRepository { get; }
    IDeploymentStampRepository DeploymentStampRepository { get; }
    IImpersonationRecordRepository ImpersonationRecordRepository { get; }
    
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}
