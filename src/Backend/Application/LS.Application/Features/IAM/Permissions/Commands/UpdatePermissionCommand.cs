using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Permissions.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Permissions.Entities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Permissions.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.IAM.Permissions.Commands;


public sealed record UpdatePermissionCommand(Guid Id, UpdatePermissionRequest Request, string UserId)
    : IRequest<AppResponse<PermissionResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [CacheKeys.Entity("permissions", Id.ToString())];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("permissions")];
}

