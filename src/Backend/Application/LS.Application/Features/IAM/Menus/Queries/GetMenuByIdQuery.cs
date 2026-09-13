using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Menus.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.IAM.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Menus.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.IAM.Menus.Queries;


public sealed record GetMenuByIdQuery(Guid Id, string UserId) : IRequest<AppResponse<MenuResponse>>, ICachableRequest
{
    public string CacheGroup => "menus";
    public string Discriminator => CacheKeys.Entity("menus", Id.ToString());
    public string? CacheUserId => null;
    public bool IsVersioned => true;
}

