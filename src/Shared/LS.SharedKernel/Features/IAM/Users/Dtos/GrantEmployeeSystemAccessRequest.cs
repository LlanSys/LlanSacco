namespace LS.SharedKernel.Features.IAM.Users.Dtos;

public sealed record GrantEmployeeSystemAccessRequest(IReadOnlyList<string> Roles);
