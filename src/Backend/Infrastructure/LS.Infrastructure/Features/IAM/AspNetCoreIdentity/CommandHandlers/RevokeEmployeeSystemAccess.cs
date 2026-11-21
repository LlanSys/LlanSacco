using LS.Application.Features.IAM.Users.Commands;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Infrastructure.Logging;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;

internal sealed class RevokeEmployeeSystemAccess(
    UserManager<AppUser> userManager,
    IIamUnitOfWork iamUnitOfWork,
    ILogger<RevokeEmployeeSystemAccess> logger)
    : IRequestHandler<RevokeEmployeeSystemAccessCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(RevokeEmployeeSystemAccessCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await iamUnitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
            {
                var user = await userManager.Users
                    .SingleOrDefaultAsync(item => item.EmployeeId == command.EmployeeId, cancellationToken)
                    .ConfigureAwait(false);

                if (user is null)
                {
                    return AppResponses.Failure<bool>("This employee is not linked to an IAM user account.");
                }

                if (user.IsActive)
            {
                var reason = string.IsNullOrWhiteSpace(command.Request.Reason)
                    ? "Revoked by administrator"
                    : command.Request.Reason.Trim();
                user.RevokeAccess(command.RevokedBy, reason);
                var updateResult = await userManager.UpdateAsync(user).ConfigureAwait(false);
                if (!updateResult.Succeeded)
                    return AppResponses.Failure<bool>("Could not revoke system access. Please try again.");
            }

            var sessions = await iamUnitOfWork.SessionRepository.GetActiveSessionsByUserIdAsync(user.Id, cancellationToken)
                .ConfigureAwait(false);
            foreach (var session in sessions)
                session.Revoke("Employee system access revoked.");
            await iamUnitOfWork.SessionRepository.UpdateRangeAsync(
                new System.Collections.ObjectModel.Collection<AppUserSession>(sessions), cancellationToken).ConfigureAwait(false);
                await iamUnitOfWork.TokenRepository
                    .RevokeAllUserTokensAsync(user.Id, "Employee system access revoked", cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                return AppResponses.Success("Employee system access revoked. Active sessions and refresh tokens have been terminated.", true);
            }, cancellationToken: cancellationToken, shouldCommit: response => response.IsSuccess).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogDeactivateUserError(logger, command.EmployeeId.ToString(), ex);
            throw;
        }
    }
}
