using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Shared.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.OrgSettings.Dtos;
using LS.Application.Features.Shared.OrgSettings.Mappings;
using MediatR;
using System.Net;

namespace LS.Application.Features.Shared.OrgSettings.QueryHandlers;

internal sealed class GetOrgSettingByKeyHandler(
    ISharedUnitOfWork unitOfWork,
    IEncryptionService encryptionService)
    : IRequestHandler<GetOrgSettingByKeyQuery, AppResponse<OrgSettingResponse>>
{
    public async Task<AppResponse<OrgSettingResponse>> Handle(GetOrgSettingByKeyQuery request, CancellationToken cancellationToken)
    {
        var setting = await unitOfWork.OrgSettingRepository.FirstOrDefaultAsync(x => x.Key == request.Key, cancellationToken).ConfigureAwait(false);
        
        if (setting == null)
        {
            return AppResponses.Failure<OrgSettingResponse>("Tenant setting not found.");
        }

        var response = setting.ToResponse();
        
        if (response.Value != "***")
        {
            try
            {
                response = response with { Value = encryptionService.Decrypt(response.Value) };
            }
            catch
            {
                // Fallback to original value on decryption failure
            }
        }

        return AppResponses.Success("Tenant setting retrieved successfully.", response);
    }
}
