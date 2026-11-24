using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Domain.Features.IAM.Users.Enums;
using LS.Infrastructure.Configuration;
using LS.Infrastructure.Logging;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LS.Infrastructure.Features.IAM.Users.Contracts.Implementations.Services;

/// <summary>
/// Activates the IAM account linked to a member, mirroring employee grant-access:
/// temporary password, forced password change, confirmed email, member role and activation email.
/// </summary>
internal sealed class MemberPortalAccessService(
    UserManager<AppUser> userManager,
    IIamUnitOfWork iamUnitOfWork,
    IOptions<IamProvisioningSettings> provisioningOptions,
    IamAccessActivator accessActivator,
    ILogger<MemberPortalAccessService> logger) : IMemberPortalAccessService
{
    public async Task<AppResponse<bool>> GrantPortalAccessAsync(
        Guid appUserId,
        Guid memberId,
        Guid tenantId,
        string grantedBy,
        CancellationToken ct)
    {
        var memberKey = memberId.ToString();
        var userKey = appUserId.ToString();
        var memberRole = Roles.Customer.ToDisplayString();

        try
        {
            var temporaryPassword = provisioningOptions.Value.TemporaryPassword;

            if (string.IsNullOrWhiteSpace(temporaryPassword))
            {
                return AppResponses.Failure<bool>(AppError.DependencyUnavailable(
                    "IAM provisioning is not configured. Set IamProvisioning:TemporaryPassword via user secrets, environment variables, or Key Vault."));
            }

            AppUser? user = null;
            var activated = false;
            var emailConfirmedBeforeActivation = false;
            var roleAssigned = false;
            string[] rolesAdded = [];
            var write = await iamUnitOfWork.ExecuteInTransactionWithRetryAsync(
                async () =>
                {
                    activated = false;
                    roleAssigned = false;
                    rolesAdded = [];

                    user = await userManager.Users
                        .SingleOrDefaultAsync(u => u.Id == userKey, ct)
                        .ConfigureAwait(false);

                    if (user is null || user.IsDeleted || user.TenantId != tenantId)
                    {
                        return AppResponses.Failure<bool>(AppError.NotFound("The member's user account was not found."));
                    }

                    if (user.MemberId.HasValue && user.MemberId.Value != memberId)
                    {
                        return AppResponses.Failure<bool>(AppError.Conflict("The user account is linked to a different member."));
                    }

                    if (!user.MemberId.HasValue)
                    {
                        user.LinkToMember(memberId);
                    }

                    if (!user.IsActive)
                    {
                        var passwordResetResult = await accessActivator
                            .ResetTemporaryPasswordForReactivationAsync(user, temporaryPassword)
                            .ConfigureAwait(false);

                        if (!passwordResetResult.Succeeded)
                        {
                            return AppResponses.Failure<bool>(AppError.BusinessRule(passwordResetResult.Errors.First().Description));
                        }

                        emailConfirmedBeforeActivation = user.EmailConfirmed;
                        user.GrantAccess(grantedBy, [memberRole]);
                        user.EmailConfirmed = true;
                        activated = true;
                    }

                    var updateResult = await userManager.UpdateAsync(user).ConfigureAwait(false);

                    if (!updateResult.Succeeded)
                    {
                        return AppResponses.Failure<bool>(AppError.BusinessRule(updateResult.Errors.First().Description));
                    }

                    if (!await userManager.IsInRoleAsync(user, memberRole).ConfigureAwait(false))
                    {
                        var roleResult = await userManager.AddToRoleAsync(user, memberRole).ConfigureAwait(false);

                        if (!roleResult.Succeeded)
                        {
                            return AppResponses.Failure<bool>(AppError.BusinessRule(roleResult.Errors.First().Description));
                        }

                        roleAssigned = true;
                        rolesAdded = [memberRole];
                    }

                    return AppResponses.Success(true);
                },
                cancellationToken: ct,
                shouldCommit: response => response.IsSuccess)
                .ConfigureAwait(false);

            if (!write.IsSuccess)
            {
                return write;
            }

            if (!activated)
            {
                // Idempotent: an active account is never re-emailed; only a missing member role is repaired.
                return AppResponses.Success(
                    roleAssigned
                        ? "Portal access is already active. The member role was assigned."
                        : "Portal access is already active for this member.",
                    true);
            }

            var activationStamp = user!.ConcurrencyStamp;
            var emailResponse = await accessActivator
                .SendActivationEmailAsync(user, provisioningOptions.Value, createdNewUser: true, ct)
                .ConfigureAwait(false);

            if (!emailResponse.IsSuccess)
            {
                if (!await accessActivator.RollBackActivationAsync(user.Id, activationStamp, rolesAdded, grantedBy, emailConfirmedBeforeActivation).ConfigureAwait(false))
                {
                    ServiceLogDefinitions.LogGrantMemberPortalAccessError(
                        logger,
                        memberKey,
                        new InvalidOperationException("Activation compensation could not be applied."));
                }

                return AppResponses.Failure<bool>(AppError.DependencyUnavailable(
                    "Portal access was not granted because the activation email could not be sent. Please verify email settings and try again."));
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                ServiceLogDefinitions.LogMemberPortalAccessGranted(logger, memberKey, grantedBy);
            }

            return AppResponses.Success("Portal access granted, member role assigned, and activation email sent.", true);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogGrantMemberPortalAccessError(logger, memberKey, ex);
            throw;
        }
    }
}
