using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Menus.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.IAM.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Menus.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.IAM.Menus.Queries;


public sealed record GetNavigationMenusQuery(string Placement, IReadOnlyList<string> PermissionKeys, string UserId, bool HasFullAccess = false)
    : IRequest<AppResponse<IReadOnlyList<MenuResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Navigation reflects current grants, menu assignments and access policy.";
}

