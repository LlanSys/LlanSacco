using LS.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;
using LS.Domain.Features.ControlPlane.Tenants.Entities;
using LS.Persistence.Common;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.ControlPlane.DataContext;
using Microsoft.EntityFrameworkCore;

namespace LS.Persistence.Features.ControlPlane.Tenants.Repositories;

public class TenantRepository(ControlPlaneDBContext context) : Repository<Tenant>(context), ITenantRepository
{
}
