namespace LS.SharedKernel.Features.IAM.Users.Dtos;

public sealed record UpdateUserPermissionsRequest(IReadOnlyList<string> PermissionKeys);
