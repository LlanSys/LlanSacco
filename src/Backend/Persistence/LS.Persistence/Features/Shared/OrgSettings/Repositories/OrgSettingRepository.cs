using LS.Domain.Features.Shared.OrgSettings.Contracts.Repositories;
using LS.Domain.Features.Shared.OrgSettings.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Shared.DataContext;
using Microsoft.EntityFrameworkCore;

namespace LS.Persistence.Features.Shared.OrgSettings.Repositories;

public class OrgSettingRepository(SharedDBContext dbContext) 
    : Repository<OrgSetting>(dbContext), IOrgSettingRepository
{
    private readonly SharedDBContext _dbContext = dbContext;
}
