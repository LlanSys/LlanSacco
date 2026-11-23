using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.Domain.Features.Shared.Contracts;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System.Collections.Generic;
using System.Net;

namespace LS.Application.Features.Shared.OrgSettings.CommandHandlers;

public record DeleteOrgSettingCommand(Guid Id, string UserId)
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> GroupVersionKeysToInvalidate =>
    [
        CacheKeys.GroupVersion("tenant-settings")
    ];
}

internal sealed class DeleteOrgSettingCommandHandler(ISharedUnitOfWork unitOfWork)
    : IRequestHandler<DeleteOrgSettingCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(DeleteOrgSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await unitOfWork.OrgSettingRepository.FindByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        
        if (setting is null)
            return AppResponses.NotFound<bool>("Tenant setting not found.");

        await unitOfWork.OrgSettingRepository.DeleteAsync(setting.Id, cancellationToken).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        return AppResponses.Success("Tenant setting deleted successfully.", true);
    }
}
