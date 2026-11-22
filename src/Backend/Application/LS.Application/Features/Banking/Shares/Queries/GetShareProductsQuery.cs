using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;

using LS.SharedKernel.Features.Banking.Shares.Dtos;

namespace LS.Application.Features.Banking.Shares.Queries;

public record GetShareProductsQuery(bool IncludeInactive = false) : IRequest<AppResponse<IEnumerable<ShareProductResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Share availability and terms reflect current product configuration.";
}
