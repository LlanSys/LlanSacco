namespace LS.SharedKernel.Features.IAM.Users.Dtos;

public sealed record ValidatePasswordResetLinkRequest(string Email, string Token);
