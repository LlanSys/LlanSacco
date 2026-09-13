using LS.Application.Features.Accounting.Queries;
using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Accounting.Contracts.Repositories;
using LS.Domain.Features.Accounting.Entities;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Accounting.Handlers;

internal class GetTrialBalanceQueryHandler(IAccountingUnitOfWork unitOfWork, ILogger<GetTrialBalanceQueryHandler> logger) : IRequestHandler<GetTrialBalanceQuery, AppResponse<TrialBalanceResponse>>
{
    private readonly IAccountingUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GetTrialBalanceQueryHandler> _logger = logger;

    public async Task<AppResponse<TrialBalanceResponse>> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.AccountRepository.ListAsync(q => q, cancellationToken);
        var balances = await _unitOfWork.AccountRepository.GetAccountBalancesAsync(request.AsOfDate, cancellationToken);

        var TrialBalanceResponse = new TrialBalanceResponse
        {
            Rows = accounts.Select(a =>
            {
                var accountBalance = balances.TryGetValue(a.Id, out var bal) ? bal : 0m;

                return new TrialBalanceRowDto
                {
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    Debit = accountBalance > 0 ? accountBalance : 0,
                    Credit = accountBalance < 0 ? System.Math.Abs(accountBalance) : 0
                };
            })
            .Where(r => r.Debit != 0 || r.Credit != 0)
            .OrderBy(r => r.AccountCode)
            .ToList()
        };

        return AppResponses.Success(TrialBalanceResponse with
        {
            TotalDebit = TrialBalanceResponse.Rows.Sum(r => r.Debit),
            TotalCredit = TrialBalanceResponse.Rows.Sum(r => r.Credit)
        });
    }
}


