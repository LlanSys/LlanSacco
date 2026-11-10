using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.OrgSettings.Dtos;
using MediatR;

namespace LS.Application.Features.Shared.OrgSettings.QueryHandlers;

public record GetOrgSettingByKeyQuery(string Key) 
    : IRequest<AppResponse<OrgSettingResponse>>, ICachableRequest
{
    public string CacheGroup => "tenant-settings";
    public string Discriminator => Key;
    public string? CacheUserId => null;
    public bool IsVersioned => true;
}
