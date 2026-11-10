using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.OrgSettings.Dtos;
using MediatR;
using System.Collections.Generic;

namespace LS.Application.Features.Shared.OrgSettings.QueryHandlers;

public record GetOrgSettingsQuery 
    : IRequest<AppResponse<IEnumerable<OrgSettingResponse>>>, ICachableRequest
{
    public string CacheGroup => "tenant-settings";
    public string Discriminator => "all";
    public string? CacheUserId => null;
    public bool IsVersioned => true;
}
