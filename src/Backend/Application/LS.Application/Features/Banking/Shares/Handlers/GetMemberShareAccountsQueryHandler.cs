using LS.Application.Features.Banking.Shares.Queries;
using LS.Domain.Features.Banking.Contracts;
using LS.SharedKernel.Dtos.Common;
using MediatR;

using LS.SharedKernel.Features.Banking.Shares.Dtos;

namespace LS.Application.Features.Banking.Shares.Handlers;

internal class GetMemberShareAccountsQueryHandler(
    IBankingUnitOfWork unitOfWork
) : IRequestHandler<GetMemberShareAccountsQuery, AppResponse<IEnumerable<ShareAccountResponse>>>
{
    public async Task<AppResponse<IEnumerable<ShareAccountResponse>>> Handle(GetMemberShareAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await unitOfWork.ShareAccounts.ListAsync(q => q
            .Where(x => x.MemberId == request.MemberId)
            .OrderBy(x => x.Product.Name)
            .Select(x => new ShareAccountResponse(
                x.Id,
                x.MemberId,
                x.ShareProductId,
                x.Product.Name,
                x.TotalShares,
                x.TotalValue,
                x.IsActive,
                x.OpenedAt
            )), cancellationToken).ConfigureAwait(false);

        return AppResponses.Success<IEnumerable<ShareAccountResponse>>(accounts);
    }
}

