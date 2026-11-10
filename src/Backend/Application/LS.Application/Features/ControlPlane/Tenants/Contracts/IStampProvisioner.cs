using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.ControlPlane.Tenants.Contracts;

public interface IStampProvisioner
{
    Task ProvisionIsolatedStampAsync(string tenantId, string stampId, string resourceGroup, string databaseProvider, CancellationToken cancellationToken = default);
}
