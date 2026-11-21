using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.Domain.Features.IAM.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.IAM.ReferenceData.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.IAM.ReferenceData.Queries;


public sealed record GetIamReferenceDataQuery : IRequest<AppResponse<IamReferenceDataResponse>>, ICachableRequest
{
    public string CacheGroup => "iam-reference-data";
    public string Discriminator => CacheKeys.Discriminator("all");
    public string? CacheUserId => null;
    public bool IsVersioned => true;
}

