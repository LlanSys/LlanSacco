using LS.Domain.Features.ControlPlane.Tenants.Entities;
using LS.Domain.Shared.Contracts.Repositories;

namespace LS.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;

public interface ITenantRepository : IRepository<Tenant>
{
}
