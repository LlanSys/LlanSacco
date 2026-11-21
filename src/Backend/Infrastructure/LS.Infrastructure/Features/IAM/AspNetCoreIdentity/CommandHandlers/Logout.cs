using LS.Application.Features.IAM.Users.Commands;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Domain.Features.IAM.Users.Events;
using LS.Infrastructure.Logging;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LS.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class Logout(
    SignInManager<AppUser> signInManager,
    IHttpContextAccessor httpContextAccessor,
    IIamUnitOfWork iamUnitOfWork,
    IPublisher publisher,
    ILogger<Logout> logger) : IRequestHandler<LogoutCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionId = httpContextAccessor.HttpContext?.Request.Headers["X-Session-Id"].FirstOrDefault()
                ?? httpContextAccessor.HttpContext?.User?.FindFirstValue("session_id");

            string? revokedSessionId = null;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await iamUnitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
                {
                    if (Guid.TryParse(sessionId, out var sessionGuid))
                    {
                        var session = await iamUnitOfWork.SessionRepository.FirstOrDefaultAsync(
                            s => s.Id == sessionGuid && s.AppUserId == userId, cancellationToken).ConfigureAwait(false);
                        revokedSessionId = session?.Id.ToString();
                        if (session is not null && !session.IsRevoked)
                        {
                            session.Revoke("User signed out");
                            await iamUnitOfWork.SessionRepository.UpdateAsync(session, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    await iamUnitOfWork.TokenRepository
                        .RevokeAllUserTokensAsync(userId, "User signed out", httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(), cancellationToken)
                        .ConfigureAwait(false);
                    return true;
                }, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            await signInManager.SignOutAsync().ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(userId))
                await publisher.Publish(new UserLogoutEvent(userId, revokedSessionId), cancellationToken).ConfigureAwait(false);
            ServiceLogDefinitions.LogUserSignedOut(logger, userId ?? string.Empty);
            return AppResponses.Success("Signed out successfully", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogUnexpectedTokenValidationError(logger, ex);
            throw;
        }
    }
}
