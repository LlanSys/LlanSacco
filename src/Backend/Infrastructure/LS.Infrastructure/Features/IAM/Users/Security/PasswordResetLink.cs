using LS.Domain.Features.IAM.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace LS.Infrastructure.Features.IAM.Users.Security;

/// <summary>
/// Decides whether a recovery token or an activation set-password token can still set this account's password.
/// Revoked (inactive) accounts never qualify, so revoking access also retires every outstanding link.
/// </summary>
internal static class PasswordResetLink
{
    public const string UnusableMessage = LS.Application.Features.IAM.Users.Queries.ValidatePasswordResetLinkQuery.UnusableMessage;

    public static async Task<bool> IsUsableAsync(UserManager<AppUser> userManager, AppUser user, string? token)
    {
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(user);

        if (!user.IsActive || string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        return await IsActivationTokenAsync(userManager, user, token).ConfigureAwait(false)
            || await userManager.VerifyUserTokenAsync(
                user,
                userManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<AppUser>.ResetPasswordTokenPurpose,
                token).ConfigureAwait(false);
    }

    public static async Task<bool> IsActivationTokenAsync(UserManager<AppUser> userManager, AppUser user, string token)
    {
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(user);

        return user.IsActive
            && user.RequirePasswordChange
            && await userManager.VerifyUserTokenAsync(
                user,
                AccountActivationTokenProvider.ProviderName,
                UserManager<AppUser>.ResetPasswordTokenPurpose,
                token).ConfigureAwait(false);
    }
}
