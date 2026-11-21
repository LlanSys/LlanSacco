namespace LS.SharedKernel.Features.IAM.Users.Dtos;


public record UnlockRequest(string Password, string? Email = null, string? EmployeeNumber = null);