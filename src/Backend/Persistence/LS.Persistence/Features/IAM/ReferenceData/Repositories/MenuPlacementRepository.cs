using LS.Domain.Features.IAM.ReferenceData.Contracts.Repositories;
using LS.Domain.Features.IAM.ReferenceData.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.IAM.DataContext;

namespace LS.Persistence.Features.IAM.ReferenceData.Repositories;

internal sealed class MenuPlacementRepository(IamDBContext context)
    : Repository<MenuPlacement>(context), IMenuPlacementRepository;
