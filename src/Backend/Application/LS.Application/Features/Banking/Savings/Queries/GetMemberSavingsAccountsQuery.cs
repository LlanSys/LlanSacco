using LS.Domain.Features.Banking.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.Savings.Queries;

public record GetMemberSavingsAccountsQuery(Guid MemberId) : IRequest<AppResponse<List<SavingsAccountResponse>>>;

internal class GetMemberSavingsAccountsQueryHandler(IBankingUnitOfWork unitOfWork) 
    : IRequestHandler<GetMemberSavingsAccountsQuery, AppResponse<List<SavingsAccountResponse>>>
{
    public async Task<AppResponse<List<SavingsAccountResponse>>> Handle(GetMemberSavingsAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await unitOfWork.SavingsAccounts.ListAsync(q => q
            .Where(x => x.MemberId == request.MemberId)
            .Select(a => new SavingsAccountResponse(
                a.Id,
                a.MemberId,
                a.SavingsProductId,
                a.Product.Name,
                a.Balance,
                a.LockedFunds,
                (a.Balance - a.LockedFunds - a.Product.MinimumBalance > 0) ? (a.Balance - a.LockedFunds - a.Product.MinimumBalance) : 0,
                a.IsActive,
                a.CreatedAt
            )), cancellationToken);

        return AppResponses.Success(accounts);
    }
}

