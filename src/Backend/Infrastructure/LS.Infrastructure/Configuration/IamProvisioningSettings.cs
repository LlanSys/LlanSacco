using System.ComponentModel.DataAnnotations;

namespace LS.Infrastructure.Configuration;

/// <summary>
/// Settings for staff-initiated account activation (employee system access and member portal access).
/// Activated accounts never receive a shared password: they get a one-time set-password link that lands on
/// the password-reset page (<see cref="EmailSettings.ClientBaseUrl"/> + <see cref="PasswordRecoverySettings.ResetPath"/>).
/// </summary>
public sealed class IamProvisioningSettings
{
    public const string SectionName = "IamProvisioning";

    /// <summary>Sign-in page mentioned in the activation email for use after the password has been set.</summary>
    [Required]
    public string SignInUrl { get; init; } = "https://localhost:7049/iam/sign-in";

    /// <summary>How long an activation set-password link stays valid. Single use: setting the password rotates the security stamp.</summary>
    [Range(1, 168)]
    public int ActivationLinkLifetimeHours { get; init; } = 72;
}
