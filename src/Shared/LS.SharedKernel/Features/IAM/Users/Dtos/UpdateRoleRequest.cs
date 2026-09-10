namespace LS.SharedKernel.Features.IAM.Users.Dtos;

public sealed record UpdateRoleRequest(string Name, Guid? DepartmentId = null);
