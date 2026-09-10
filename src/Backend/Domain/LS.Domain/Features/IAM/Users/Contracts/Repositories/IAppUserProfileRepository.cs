using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Domain.Features.IAM.Users.Contracts.Repositories;

public interface IAppUserProfileRepository : IRepository<AppUserProfile>
{
    Task<AppUserProfile?> GetByUserIdAsync(string userId);
    Task<AppUserProfile> CreateOrUpdateAsync(string userId, AppUserProfile profile, CancellationToken cancellationToken);
}
