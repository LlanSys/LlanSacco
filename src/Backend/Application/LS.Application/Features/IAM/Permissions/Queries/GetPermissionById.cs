using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Permissions.Mappings;
using LS.Application.Utilities;
using LS.Domain.Features.IAM.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Permissions.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.IAM.Permissions.Queries;



internal sealed class GetPermissionByIdQueryHandler(IIamUnitOfWork unitOfWork, ILogger<GetPermissionByIdQueryHandler> logger)
    : IRequestHandler<GetPermissionByIdQuery, AppResponse<PermissionResponse>>
{
    public async Task<AppResponse<PermissionResponse>> Handle(GetPermissionByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var permission = await unitOfWork.PermissionRepository.FindByIdAsync(query.Id, cancellationToken).ConfigureAwait(false);
            return permission is null
                ? AppResponses.Failure<PermissionResponse>($"Permission {query.Id} not found.")
                : AppResponses.Success(permission.ToPermissionResponse());
        }
        catch (Exception ex)
        {
            LogDefinitions.LogPipelineException(logger, nameof(GetPermissionByIdQueryHandler), ex);
            throw;
        }
    }
}
