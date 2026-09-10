using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Menus.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Menus.Entities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Menus.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.IAM.Menus.Commands;


public sealed record CreateMenuCommand(CreateMenuRequest Request, string UserId)
    : IRequest<AppResponse<MenuResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [];
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("menus")];
}

