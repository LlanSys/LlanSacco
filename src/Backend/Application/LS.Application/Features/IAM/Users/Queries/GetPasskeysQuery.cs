using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using MediatR;
using System.Collections.Generic;

namespace LS.Application.Features.IAM.Users.Queries;

public record GetPasskeysQuery : IRequest<AppResponse<IReadOnlyList<PasskeyResponse>>>, ICachableRequest
{
    public string CacheGroup => "passkeys";
    public string Discriminator => string.Empty;
    public string? CacheUserId => null;
    public bool IsVersioned => false;
    public bool BypassCache => true;
}
