namespace LS.Infrastructure.Features.IAM.Users.Security;

/// <summary>Custom JWT claim types issued by IAM.</summary>
public static class IamCustomClaimTypes
{
    /// <summary>
    /// Present (value <c>true</c>) while <c>AppUser.RequirePasswordChange</c> is set. The API blocks every request except the
    /// password-change allowlist while it is present; a token refreshed after the change no longer carries it.
    /// </summary>
    public const string PasswordChangeRequired = "pwd_change_required";
}
