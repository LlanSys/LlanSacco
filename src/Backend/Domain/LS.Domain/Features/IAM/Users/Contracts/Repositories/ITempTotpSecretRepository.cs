using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Domain.Features.IAM.Users.Contracts.Repositories;

public interface ITempTotpSecretRepository : IRepository<TempTotpSecret>
{
    Task<TempTotpSecret?> GetValidTempSecretByUserIdAsync(string userId);
    Task<bool> DeleteExpiredSecretsAsync(CancellationToken cancellationToken);
    Task<bool> DeleteUserTempSecretsAsync(string userId, CancellationToken cancellationToken);
}
