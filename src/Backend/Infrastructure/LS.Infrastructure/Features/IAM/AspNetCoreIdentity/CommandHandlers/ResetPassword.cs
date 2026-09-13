using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Users.Commands;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Domain.Features.IAM.Users.Events;
using LS.Infrastructure.Logging;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LS.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class ResetPassword(
    UserManager<AppUser> userManager,
    IIamUnitOfWork iamUnitOfWork,
    ICacheService cacheService,
    IHttpContextAccessor httpContextAccessor,
    IPublisher publisher,
    ILogger<ResetPassword> logger) : IRequestHandler<ResetPasswordCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var request = command.Request;

        try
        {
            AppUser? committedUser = null;
            var result = await iamUnitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                ct.ThrowIfCancellationRequested();
                // Identity and the Unit of Work share the scoped IamDBContext. Identity's saves
                // therefore enlist in this transaction; reload the user on every retry.
                var user = await userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);
                if (user == null)
                {
                    ServiceLogDefinitions.LogInvalidToken(logger);
                    return AppResponses.Failure<bool>("Invalid reset request");
                }

                var password = request.Password ?? request.NewPassword ?? string.Empty;
                if (await userManager.CheckPasswordAsync(user, password).ConfigureAwait(false))
                    return AppResponses.Failure<bool>("New password must be different from your current password");

                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    ServiceLogDefinitions.LogInvalidToken(logger);
                    return AppResponses.Failure<bool>("The password reset request is invalid or expired.");
                }

                ct.ThrowIfCancellationRequested();
                var passwordResult = await userManager.ResetPasswordAsync(user, request.Token, password).ConfigureAwait(false);
                if (!passwordResult.Succeeded)
                {
                    var errors = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                    ServiceLogDefinitions.LogFailedToAddClaim(logger, "password", "reset", user.Id, errors);
                    return AppResponses.Failure<bool>("Password reset failed. Please ensure your password meets all requirements.");
                }

                user.CompletePasswordReset(user.Id);
                var updateResult = await userManager.UpdateAsync(user).ConfigureAwait(false);
                if (!updateResult.Succeeded)
                    return AppResponses.Failure<bool>(AppError.BusinessRule("Password reset could not be completed. Please try again."));

                ct.ThrowIfCancellationRequested();
                var refreshTokens = await iamUnitOfWork.TokenRepository.GetActiveTokensByUserIdAsync(user.Id, ct).ConfigureAwait(false);
                if (refreshTokens.Count != 0)
                {
                    var revokedByIp = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    await iamUnitOfWork.TokenRepository.RevokeTokensAsync(refreshTokens, "Password reset", revokedByIp, ct).ConfigureAwait(false);
                }
                committedUser = user;
                return AppResponses.Success("Password reset successfully", true);
            }, cancellationToken: ct, shouldCommit: response => response.IsSuccess).ConfigureAwait(false);

            if (!result.IsSuccess)
                return result;
            var user = committedUser!;
            await cacheService.RemoveAsync(CacheKeys.PasswordResetOtp(user.Id), ct).ConfigureAwait(false);
            await cacheService.RemoveAsync(CacheKeys.PasswordResetRateLimit(user.Id), ct).ConfigureAwait(false);

            await publisher.Publish(new PasswordResetSuccessEvent(
                user.Id,
                user.Email!,
                $"{user.FirstName} {user.LastName}"), ct).ConfigureAwait(false);

            ServiceLogDefinitions.LogEmailOtpSent(logger, user.Id, "PasswordReset");
            return AppResponses.Success("Password reset successfully", true);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogUnexpectedTokenValidationError(logger, ex);
            return AppResponses.Failure<bool>("Password reset failed. Please try again.");
        }
    }
}
