using LS.Application.Features.Banking.Shares.Queries;
using LS.Domain.Features.Banking.Contracts;
using LS.SharedKernel.Dtos.Common;
using MediatR;

using LS.SharedKernel.Features.Banking.Shares.Dtos;

namespace LS.Application.Features.Banking.Shares.Handlers;

internal class GetShareProductsQueryHandler(
    IBankingUnitOfWork unitOfWork
) : IRequestHandler<GetShareProductsQuery, AppResponse<IEnumerable<ShareProductResponse>>>
{
    public async Task<AppResponse<IEnumerable<ShareProductResponse>>> Handle(GetShareProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await unitOfWork.ShareProducts.ListAsync(q =>
        {
            if (!request.IncludeInactive)
                q = q.Where(x => x.IsActive);
                
            return q.OrderBy(x => x.Name).Select(x => new ShareProductResponse(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.MinimumShares,
                x.PricePerShare,
                x.IsActive
            ));
        }, cancellationToken).ConfigureAwait(false);

        return AppResponses.Success<IEnumerable<ShareProductResponse>>(products);
    }
}

