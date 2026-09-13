using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;

namespace LS.Application.Features.IAM.Users.Commands;

public sealed record RevokeEmployeeSystemAccessCommand(
    Guid EmployeeId,
    DeactivateUserRequest Request,
    string RevokedBy)
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> DirectInvalidationKeys => [];

    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("iam-admin")];
}
