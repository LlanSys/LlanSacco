using LS.Domain.Features.IAM.Permissions.Contracts.Repositories;
using LS.Domain.Features.IAM.Permissions.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.IAM.DataContext;

namespace LS.Persistence.Features.IAM.Permissions.Repositories;

public sealed class IamPermissionRepository(IamDBContext context)
    : Repository<Permission>(context), IPermissionRepository;
