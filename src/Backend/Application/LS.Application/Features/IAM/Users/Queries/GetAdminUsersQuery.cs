using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Users.Queries;

public sealed record GetAdminUsersQuery(AdminUserSearchRequest SearchRequest)
    : IRequest<AppResponse<PagedResponse<AdminUserListResponse, string>>>, ICachableRequest
{
    public string CacheGroup => "iam-admin";
    public string Discriminator => CacheKeys.Discriminator(SearchRequest);
    public string? CacheUserId => null;
    public bool IsVersioned => true;
    public bool BypassCache => true;
}
