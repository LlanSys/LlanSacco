using LS.Application.Features.Banking.Deposits.Queries;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Deposits.Dtos;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.Deposits.Handlers;

internal class GetDepositProductsQueryHandler(IBankingUnitOfWork unitOfWork) 
    : IRequestHandler<GetDepositProductsQuery, AppResponse<System.Collections.Generic.IEnumerable<DepositProductResponse>>>
{
    public async Task<AppResponse<System.Collections.Generic.IEnumerable<DepositProductResponse>>> Handle(GetDepositProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await unitOfWork.DepositProducts.ListAsync(ct: cancellationToken);

        var response = products.Select(p => new DepositProductResponse(
            p.Id,
            p.Name,
            p.Code,
            p.Description,
            p.Type.ToString(),
            p.InterestRate,
            p.TermMonths,
            p.MinimumDeposit,
            p.PenaltyStrategy.ToString(),
            p.FlatPenaltyRate,
            p.InterestForfeiturePercentage,
            p.ProRataReducedInterestRate,
            p.IsActive
        ));

        return AppResponses.Success("Deposit products retrieved successfully.", response);
    }
}

