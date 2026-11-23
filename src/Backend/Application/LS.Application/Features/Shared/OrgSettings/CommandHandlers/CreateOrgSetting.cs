using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.Shared.OrgSettings.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.Shared.Contracts;
using LS.Domain.Features.Shared.OrgSettings.Entities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.OrgSettings.Dtos;
using MediatR;
using System.Collections.Generic;
using System.Net;

namespace LS.Application.Features.Shared.OrgSettings.CommandHandlers;

public record CreateOrgSettingCommand(CreateOrgSettingRequest Request, string UserId)
    : IRequest<AppResponse<OrgSettingResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> GroupVersionKeysToInvalidate =>
    [
        CacheKeys.GroupVersion("tenant-settings")
    ];
}

internal sealed class CreateOrgSettingCommandHandler(
    ISharedUnitOfWork unitOfWork,
    IEncryptionService encryptionService)
    : IRequestHandler<CreateOrgSettingCommand, AppResponse<OrgSettingResponse>>
{
    public async Task<AppResponse<OrgSettingResponse>> Handle(CreateOrgSettingCommand request, CancellationToken cancellationToken)
    {
        var existingSetting = await unitOfWork.OrgSettingRepository.FirstOrDefaultAsync(x => x.Key == request.Request.Key, cancellationToken).ConfigureAwait(false);
        
        if (existingSetting != null)
        {
            return AppResponses.Conflict<OrgSettingResponse>($"A setting with key '{request.Request.Key}' already exists.");
        }

        var setting = new OrgSetting(
            request.Request.Key,
            OrgSettingMapping.IsSensitiveKey(request.Request.Key)
                ? encryptionService.Encrypt(request.Request.Value)
                : request.Request.Value,
            request.UserId,
            request.Request.Description);

        await unitOfWork.OrgSettingRepository.CreateAsync(setting, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        var response = setting.ToResponse();
        return AppResponses.Success("Tenant setting created successfully.", response);
    }
}
