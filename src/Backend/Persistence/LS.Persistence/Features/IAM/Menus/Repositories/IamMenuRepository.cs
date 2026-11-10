using LS.Domain.Features.IAM.Menus.Contracts.Repositories;
using LS.Domain.Features.IAM.Menus.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.IAM.DataContext;

namespace LS.Persistence.Features.IAM.Menus.Repositories;

public sealed class IamMenuRepository(IamDBContext context)
    : Repository<MenuItem>(context), IMenuRepository;
