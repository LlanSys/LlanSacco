using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Domain.Features.IAM.Users.Contracts.Repositories;

public interface IAppUserTotpSecretRepository : IRepository<AppUserTotpSecret>
{
    Task<AppUserTotpSecret?> GetByUserIdAsync(string userId);
    Task<AppUserTotpSecret?> GetActiveSecretByUserIdAsync(string userId);
    Task<bool> DeactivateUserSecretsAsync(string userId);
}
