using LS.Application.Features.Accounting.Queries;
using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Accounting.Contracts.Repositories;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Features.Accounting.Enums;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace LS.Application.Features.Accounting.Handlers;

internal class GetIncomeStatementQueryHandler(IAccountingUnitOfWork unitOfWork, ILogger<GetIncomeStatementQueryHandler> logger) : IRequestHandler<GetIncomeStatementQuery, AppResponse<IncomeStatementDto>>
{
    private readonly IAccountingUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GetIncomeStatementQueryHandler> _logger = logger;

    public async Task<AppResponse<IncomeStatementDto>> Handle(GetIncomeStatementQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.AccountRepository.ListAsync(q => q.Where(a => a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense), cancellationToken);
        var balances = await _unitOfWork.AccountRepository.GetAccountBalancesAsync(request.StartDate, request.EndDate, cancellationToken);

        var incomes = accounts.Where(a => a.AccountType == AccountType.Revenue).Select(a =>
        {
            var accountBalance = balances.TryGetValue(a.Id, out var bal) ? bal : 0m;
            // Revenue is normally credit (negative)
            return new StatementRowDto
            {
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                Balance = accountBalance < 0 ? Math.Abs(accountBalance) : -accountBalance
            };
        }).Where(r => r.Balance != 0).ToList();

        var expenses = accounts.Where(a => a.AccountType == AccountType.Expense).Select(a =>
        {
            var accountBalance = balances.TryGetValue(a.Id, out var bal) ? bal : 0m;
            // Expense is normally debit (positive)
            return new StatementRowDto
            {
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                Balance = accountBalance
            };
        }).Where(r => r.Balance != 0).ToList();

        var dto = new IncomeStatementDto
        {
            Incomes = incomes,
            Expenses = expenses,
            TotalIncome = incomes.Sum(i => i.Balance),
            TotalExpense = expenses.Sum(e => e.Balance)
        };

        return AppResponses.Success(dto);
    }
}

