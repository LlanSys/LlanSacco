using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.IAM.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.IAM.ReferenceData.Commands;


public sealed record DeleteReferenceCatalogItemCommand(string CatalogType, Guid Id, string UserId)
    : IRequest<AppResponse<bool>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("iam-reference-data")];
}

