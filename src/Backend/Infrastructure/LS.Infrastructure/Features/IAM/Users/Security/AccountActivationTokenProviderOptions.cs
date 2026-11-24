using Microsoft.AspNetCore.Identity;

namespace LS.Infrastructure.Features.IAM.Users.Security;

/// <summary>
/// Options for <see cref="AccountActivationTokenProvider"/>. The lifespan is bound from
/// <c>IamProvisioning:ActivationLinkLifetimeHours</c>; the protector name isolates activation tokens from other Identity tokens.
/// </summary>
public sealed class AccountActivationTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public AccountActivationTokenProviderOptions()
    {
        Name = AccountActivationTokenProvider.ProviderName;
        TokenLifespan = TimeSpan.FromHours(72);
    }
}
