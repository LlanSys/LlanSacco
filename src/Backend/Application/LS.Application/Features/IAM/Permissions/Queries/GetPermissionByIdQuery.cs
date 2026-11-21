using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Permissions.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.IAM.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Permissions.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.IAM.Permissions.Queries;


public sealed record GetPermissionByIdQuery(Guid Id, string UserId) : IRequest<AppResponse<PermissionResponse>>, ICachableRequest
{
    public string CacheGroup => "permissions";

    public string Discriminator => CacheKeys.Entity("permissions", Id.ToString());

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

