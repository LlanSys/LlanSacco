using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.ReferenceData.Mappings;
using LS.Application.Utilities;
using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.IAM.Contracts;
using LS.SharedKernel.Features.IAM.ReferenceData.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.IAM.ReferenceData.Commands;


public sealed record UpdateReferenceCatalogItemCommand(string CatalogType, Guid Id, ReferenceCatalogItemRequest Request, string UserId)
    : IRequest<AppResponse<ReferenceCatalogItemResponse>>, ICacheInvalidatorRequest
{
    public IReadOnlyList<string> GroupVersionKeysToInvalidate => [CacheKeys.GroupVersion("iam-reference-data")];
}

