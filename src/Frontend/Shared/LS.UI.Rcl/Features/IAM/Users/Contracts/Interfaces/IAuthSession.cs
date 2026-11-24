using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Users.Dtos;

namespace LS.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;

public interface IAuthSession
{
    bool IsInitialized { get; }
    bool IsAuthenticated { get; }
    CurrentUserResponse? CurrentUser { get; }
    string? ProfilePictureDataUri { get; }
    string? PendingTwoFactorUserId { get; }
    string? LastError { get; }
    bool HasFullAccess { get; }
    bool MfaEnrollmentRequired { get; }
    bool PasswordChangeRequired { get; }

    Task InitializeAsync();
    Task RefreshAsync();

    /// <summary>Obtains a fresh access token (dropping claims such as the password-change gate) and reloads the current user.</summary>
    Task<bool> RenewSessionAsync();
    Task<AppResponse<LoginResponse>> SignInAsync(LoginRequest request);
    Task<AppResponse<LoginResponse>> SignInWithPasskeyAsync(LoginWithPasskeyRequest request);
    Task<AppResponse<LoginResponse>> ExchangeSsoCodeAsync(string code);
    Task<AppResponse<VerifyOtpResponse>> CompleteTwoFactorAsync(VerifyOtpRequest request);
    Task<AppResponse<bool>> SignOutAsync();
    bool HasPermission(string permission);
    bool HasAnyPermission(params string[] permissions);
    bool IsInRole(string role);
}
