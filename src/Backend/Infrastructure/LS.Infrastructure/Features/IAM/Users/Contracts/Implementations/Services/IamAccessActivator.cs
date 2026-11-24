using LS.Application.Features.Shared.Notifications.Contracts.Interfaces;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Infrastructure.Configuration;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Notifications.Dtos;
using Microsoft.AspNetCore.Identity;

namespace LS.Infrastructure.Features.IAM.Users.Contracts.Implementations.Services;

/// <summary>
/// Shared IAM access-activation steps used when staff grant system or portal access:
/// temporary-password reset, activation email delivery and compensation when delivery fails.
/// </summary>
internal sealed class IamAccessActivator(
    UserManager<AppUser> userManager,
    IIamUnitOfWork iamUnitOfWork,
    IEmailService emailService)
{
    public async Task<IdentityResult> ResetTemporaryPasswordForReactivationAsync(
        AppUser user,
        string temporaryPassword)
    {
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
        var resetResult = await userManager.ResetPasswordAsync(user, resetToken, temporaryPassword).ConfigureAwait(false);

        if (!resetResult.Succeeded)
        {
            return resetResult;
        }

        user.RequirePasswordChange = true;
        user.PasswordLastChanged = null;
        user.ResetFailedLoginAttempts();

        var failedCountResult = await userManager.ResetAccessFailedCountAsync(user).ConfigureAwait(false);

        if (!failedCountResult.Succeeded)
        {
            return failedCountResult;
        }

        return await userManager.SetLockoutEndDateAsync(user, null).ConfigureAwait(false);
    }

    public Task<AppResponse<SendEmailResponse>> SendActivationEmailAsync(
        AppUser user,
        IamProvisioningSettings settings,
        bool createdNewUser,
        CancellationToken ct)
    {
        var displayName = $"{user.FirstName} {user.LastName}".Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = user.UserName ?? user.Email ?? "there";
        }

        var subject = createdNewUser
            ? "Your LlanSacco account is ready"
            : "Your LlanSacco access has been reactivated";

        var body = $"""
            <p>Hello {System.Net.WebUtility.HtmlEncode(displayName)},</p>
            <p>Your system access has been enabled.</p>
            <p><strong>Username:</strong> {System.Net.WebUtility.HtmlEncode(user.UserName ?? user.Email ?? string.Empty)}</p>
            <p><strong>Temporary password:</strong> {System.Net.WebUtility.HtmlEncode(settings.TemporaryPassword)}</p>
            <p>Please sign in at <a href="{System.Net.WebUtility.HtmlEncode(settings.SignInUrl)}">{System.Net.WebUtility.HtmlEncode(settings.SignInUrl)}</a> and change your password immediately.</p>
            <p>If you did not expect this access, contact your system administrator.</p>
            """;

        return emailService.SendEmailAsync(
            new SendEmailRequest
            {
                To = user.Email,
                Subject = subject,
                Body = body
            },
            ct);
    }

    /// <summary>
    /// Reverts an activation whose email could not be delivered. Returns false when compensation could not be applied.
    /// Pass <paramref name="restoreEmailConfirmed"/> when the activation also changed the confirmation flag.
    /// </summary>
    public async Task<bool> RollBackActivationAsync(
        string userId, string? activationStamp, string[] rolesAdded, string changedBy, bool? restoreEmailConfirmed = null)
    {
        // Complete compensation even if the HTTP request was cancelled after the provider rejected delivery.
        return await iamUnitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var current = await userManager.FindByIdAsync(userId).ConfigureAwait(false);

                if (current is null || !current.IsActive)
                {
                    return true;
                }

                if (!string.Equals(current.ConcurrencyStamp, activationStamp, StringComparison.Ordinal))
                {
                    return false;
                }

                if (rolesAdded.Length > 0
                    && !(await userManager.RemoveFromRolesAsync(current, rolesAdded).ConfigureAwait(false)).Succeeded)
                {
                    return false;
                }

                current.RevokeAccess(changedBy, "Activation email could not be sent.");
                if (restoreEmailConfirmed.HasValue)
                {
                    current.EmailConfirmed = restoreEmailConfirmed.Value;
                }

                return (await userManager.UpdateAsync(current).ConfigureAwait(false)).Succeeded;
            },
            cancellationToken: CancellationToken.None,
            shouldCommit: success => success)
            .ConfigureAwait(false);
    }
}
