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

internal class GetMemberDepositAccountsQueryHandler(IBankingUnitOfWork unitOfWork) 
    : IRequestHandler<GetMemberDepositAccountsQuery, AppResponse<System.Collections.Generic.IEnumerable<DepositAccountResponse>>>
{
    public async Task<AppResponse<System.Collections.Generic.IEnumerable<DepositAccountResponse>>> Handle(GetMemberDepositAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await unitOfWork.DepositAccounts.ListAsync(
            q => q.Where(x => x.MemberId == request.MemberId), 
            cancellationToken);

        var response = accounts.Select(a => new DepositAccountResponse(
            a.Id,
            a.MemberId,
            a.DepositProductId,
            a.Balance,
            a.AccruedInterest,
            a.MaturityDate,
            a.IsActive
        ));

        return AppResponses.Success("Deposit accounts retrieved successfully.", response);
    }
}

