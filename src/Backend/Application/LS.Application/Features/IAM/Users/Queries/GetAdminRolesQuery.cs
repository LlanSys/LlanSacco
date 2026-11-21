using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Users.Queries;

public sealed record GetAdminRolesQuery : IRequest<AppResponse<IReadOnlyList<AdminRoleListResponse>>>, ICachableRequest
{
    public string CacheGroup => "iam-admin";
    public string Discriminator => "roles";
    public string? CacheUserId => null;
    public bool IsVersioned => true;
    public bool BypassCache => true;
}
