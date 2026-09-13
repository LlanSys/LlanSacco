using LS.Domain.Features.Banking.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.Savings.Queries;

public record GetSavingsProductsQuery(bool OnlyActive = true) : IRequest<AppResponse<List<SavingsProductResponse>>>;

internal class GetSavingsProductsQueryHandler(IBankingUnitOfWork unitOfWork) 
    : IRequestHandler<GetSavingsProductsQuery, AppResponse<List<SavingsProductResponse>>>
{
    public async Task<AppResponse<List<SavingsProductResponse>>> Handle(GetSavingsProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await unitOfWork.SavingsProducts.ListAsync(q => 
        {
            var query = q;
            if (request.OnlyActive)
            {
                query = query.Where(x => x.IsActive);
            }
            return query
                .OrderBy(x => x.Name)
                .Select(p => new SavingsProductResponse(
                    p.Id,
                    p.Name,
                    p.Code,
                    p.Description,
                    p.InterestRate,
                    p.MinimumBalance,
                    p.AllowsWithdrawals,
                    p.WithdrawalFee,
                    p.DailyWithdrawalLimit,
                    p.MonthlyWithdrawalLimit,
                    p.IsActive
                ));
        }, cancellationToken);

        return AppResponses.Success(products);
    }
}

