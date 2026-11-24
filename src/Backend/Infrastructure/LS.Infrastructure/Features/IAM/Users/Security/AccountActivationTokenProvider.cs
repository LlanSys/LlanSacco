using LS.Domain.Features.IAM.Users.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LS.Infrastructure.Features.IAM.Users.Security;

/// <summary>
/// Long-lived, single-use token provider for activation set-password links.
/// The default password-reset provider issues short email codes (minutes) that suit self-service recovery,
/// not an invitation that may sit in an inbox. Tokens embed the security stamp, so they stop working as soon as
/// the password is set (ResetPasswordAsync rotates the stamp).
/// </summary>
public sealed class AccountActivationTokenProvider(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<AccountActivationTokenProviderOptions> options,
    ILogger<DataProtectorTokenProvider<AppUser>> logger)
    : DataProtectorTokenProvider<AppUser>(dataProtectionProvider, options, logger)
{
    public const string ProviderName = "AccountActivation";
}
