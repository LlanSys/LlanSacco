namespace LS.Application.Features.IAM.Users.Contracts.Interfaces;

/// <summary>Checks, without consuming it, whether a set-password or recovery link can still set an account's password.</summary>
public interface IPasswordResetLinkInspector
{
    Task<bool> IsUsableAsync(string email, string token, CancellationToken cancellationToken);
}
