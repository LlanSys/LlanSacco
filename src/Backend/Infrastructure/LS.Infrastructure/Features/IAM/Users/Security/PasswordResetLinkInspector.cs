using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Domain.Features.IAM.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace LS.Infrastructure.Features.IAM.Users.Security;

internal sealed class PasswordResetLinkInspector(UserManager<AppUser> userManager) : IPasswordResetLinkInspector
{
    public async Task<bool> IsUsableAsync(string email, string token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        return user is not null && await PasswordResetLink.IsUsableAsync(userManager, user, token).ConfigureAwait(false);
    }
}
