using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.OrgSettings.Dtos;
using MediatR;
using System.Collections.Generic;

namespace LS.Application.Features.Shared.OrgSettings.CommandHandlers;

public record CreateOrgSettingCommand(CreateOrgSettingRequest Request, string UserId) 
    : IRequest<AppResponse<OrgSettingResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> GroupVersionKeysToInvalidate =>
    [
        CacheKeys.GroupVersion("tenant-settings")
    ];
}
