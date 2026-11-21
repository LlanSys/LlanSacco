using LS.Domain.Features.IAM.Permissions.Entities;
using LS.Domain.Shared.Contracts.Repositories;

namespace LS.Domain.Features.IAM.Permissions.Contracts.Repositories;

public interface IPermissionRepository : IRepository<Permission>
{
}
