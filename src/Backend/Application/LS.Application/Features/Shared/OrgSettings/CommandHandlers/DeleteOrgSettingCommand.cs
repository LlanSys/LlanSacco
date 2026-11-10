using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Collections.Generic;

namespace LS.Application.Features.Shared.OrgSettings.CommandHandlers;

public record DeleteOrgSettingCommand(Guid Id, string UserId) 
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> GroupVersionKeysToInvalidate =>
    [
        CacheKeys.GroupVersion("tenant-settings")
    ];
}
