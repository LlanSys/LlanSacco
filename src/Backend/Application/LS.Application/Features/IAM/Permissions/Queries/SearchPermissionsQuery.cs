using System.Collections.ObjectModel;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Permissions.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Permissions.Entities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Permissions.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.IAM.Permissions.Queries;


public sealed record SearchPermissionsQuery(PermissionSearchRequest SearchRequest, string UserId)
    : IRequest<AppResponse<PagedResponse<PermissionResponse, Guid>>>, ICachableRequest
{
    public string CacheGroup => "permissions";

    public string Discriminator => CacheKeys.Discriminator(SearchRequest);

    public string? CacheUserId => null;

    public bool IsVersioned => true;
}

