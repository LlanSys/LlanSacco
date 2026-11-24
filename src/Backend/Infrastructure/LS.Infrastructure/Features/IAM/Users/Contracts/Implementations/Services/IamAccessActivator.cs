using LS.Application.Features.Shared.Notifications.Contracts.Interfaces;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Users.Entities;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using LS.Infrastructure.Configuration;
using LS.Infrastructure.Features.IAM.Users.Security;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Notifications.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LS.Infrastructure.Features.IAM.Users.Contracts.Implementations.Services;

/// <summary>
/// Shared IAM access-activation steps used when staff grant system or portal access:
/// locking the account behind an unknown random password, emailing a one-time set-password link,
/// and compensation when delivery fails. No password is ever chosen for, or disclosed to, the user.
/// </summary>
internal sealed class IamAccessActivator(
    UserManager<AppUser> userManager,
    IIamUnitOfWork iamUnitOfWork,
    IEmailService emailService,
    IOptions<IamProvisioningSettings> provisioningOptions,
    IOptions<EmailSettings> emailOptions,
    IOptions<PasswordRecoverySettings> recoveryOptions)
{
    private const int RandomPasswordBytes = 48;

    /// <summary>True when activation links can be built (EmailSettings:ClientBaseUrl is an absolute http(s) URL).</summary>
    public bool CanBuildActivationLinks => PasswordResetLinkBuilder.TryGetBaseUri(emailOptions.Value, out _);

    /// <summary>
    /// Creates a password nobody knows: cryptographically random, used once to set the hash and then discarded.
    /// The suffix guarantees the configured complexity rules are met.
    /// </summary>
    public static string CreateUnusablePassword()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(RandomPasswordBytes)) + "aA1!";

    /// <summary>
    /// Replaces the password of an account being (re)activated with an unknown random password, rotating the security stamp
    /// (which also invalidates any older reset or activation links), and forces a password change.
    /// </summary>
    public async Task<IdentityResult> LockPasswordForActivationAsync(AppUser user)
    {
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
        var resetResult = await userManager.ResetPasswordAsync(user, resetToken, CreateUnusablePassword()).ConfigureAwait(false);

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

    /// <summary>
    /// Builds the one-time set-password link. Call only after the activation has committed: the token embeds the
    /// current security stamp, and setting the password through the reset page rotates it, so the link works once.
    /// </summary>
    public async Task<Uri> CreateSetPasswordLinkAsync(AppUser user)
    {
        var token = await userManager.GenerateUserTokenAsync(
            user,
            AccountActivationTokenProvider.ProviderName,
            UserManager<AppUser>.ResetPasswordTokenPurpose)
            .ConfigureAwait(false);

        return PasswordResetLinkBuilder.Build(emailOptions.Value, recoveryOptions.Value, user.Email!, token);
    }

    public async Task<AppResponse<SendEmailResponse>> SendActivationEmailAsync(
        AppUser user,
        bool createdNewUser,
        CancellationToken ct)
    {
        var settings = provisioningOptions.Value;
        var displayName = $"{user.FirstName} {user.LastName}".Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = user.UserName ?? user.Email ?? "there";
        }

        var subject = createdNewUser
            ? "Your LlanSacco account is ready"
            : "Your LlanSacco access has been reactivated";

        Uri setPasswordLink;

        try
        {
            setPasswordLink = await CreateSetPasswordLinkAsync(user).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            // Translated into a failed delivery so the caller compensates the activation.
            return AppResponses.Failure<SendEmailResponse>(AppError.DependencyUnavailable("The activation link could not be created."));
        }

        var link = WebUtility.HtmlEncode(setPasswordLink.ToString());
        var signInUrl = WebUtility.HtmlEncode(settings.SignInUrl);
        var lifetime = settings.ActivationLinkLifetimeHours.ToString(CultureInfo.InvariantCulture);

        var body = $"""
            <p>Hello {WebUtility.HtmlEncode(displayName)},</p>
            <p>Your system access has been enabled.</p>
            <p><strong>Username:</strong> {WebUtility.HtmlEncode(user.UserName ?? user.Email ?? string.Empty)}</p>
            <p>Before you can sign in, choose your own password using this secure link:</p>
            <p><a href="{link}">Set your password</a></p>
            <p>The link can be used once and expires in {lifetime} hours. If it has expired, use "Forgot password?" on the sign-in page or ask your system administrator to resend access.</p>
            <p>After setting your password, sign in at <a href="{signInUrl}">{signInUrl}</a>.</p>
            <p>If you did not expect this email, do not use the link and contact your system administrator.</p>
            """;

        return await emailService.SendEmailAsync(
            new SendEmailRequest
            {
                To = user.Email,
                Subject = subject,
                Body = body
            },
            ct)
            .ConfigureAwait(false);
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

                // Invalidate any set-password link generated for this activation, in case delivery partially succeeded.
                if (!(await userManager.UpdateSecurityStampAsync(current).ConfigureAwait(false)).Succeeded)
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
