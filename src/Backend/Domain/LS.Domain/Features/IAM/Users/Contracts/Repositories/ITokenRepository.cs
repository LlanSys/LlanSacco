using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Domain.Features.IAM.Users.Contracts.Repositories;

public interface ITokenRepository : IRepository<RefreshToken>
{
    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetUserTokensAsync(string userId, int limit = 10);
    Task<List<RefreshToken>> GetExpiredTokensAsync(int daysOld = 30);
    Task MarkTokenAsUsedAsync(RefreshToken refreshToken);
    Task<bool> TryUseRefreshTokenAsync(string token, string userId, CancellationToken cancellationToken = default);
    Task<bool> IsTokenActiveAsync(string token);
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string reason);
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string reason, string? revokedByIp = null);
    Task RevokeTokensAsync(List<RefreshToken> tokens, string reason, string? revokedByIp = null, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(string userId, string reason, string? revokedByIp = null, CancellationToken cancellationToken = default);
    Task CleanupExpiredTokensAsync(string? userId = null, CancellationToken cancellationToken = default);
}
