namespace LS.SharedKernel.Features.IAM.Users.Dtos;
public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
