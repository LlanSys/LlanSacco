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

internal class GetBalanceSheetQueryHandler(IAccountingUnitOfWork unitOfWork, ILogger<GetBalanceSheetQueryHandler> logger) : IRequestHandler<GetBalanceSheetQuery, AppResponse<BalanceSheetDto>>
{
    private readonly IAccountingUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GetBalanceSheetQueryHandler> _logger = logger;

    public async Task<AppResponse<BalanceSheetDto>> Handle(GetBalanceSheetQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.AccountRepository.ListAsync(q => q.Where(a => 
            a.AccountType == AccountType.Asset || 
            a.AccountType == AccountType.Liability || 
            a.AccountType == AccountType.Equity ||
            a.AccountType == AccountType.Revenue ||
            a.AccountType == AccountType.Expense
        ), cancellationToken);

        var balances = await _unitOfWork.AccountRepository.GetAccountBalancesAsync(request.AsOfDate, cancellationToken);

        // Assets (Debit balances)
        var assets = accounts.Where(a => a.AccountType == AccountType.Asset).Select(a =>
        {
            var accountBalance = balances.TryGetValue(a.Id, out var bal) ? bal : 0m;
            return new StatementRowDto
            {
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                Balance = accountBalance
            };
        }).Where(r => r.Balance != 0).ToList();

        // Liabilities (Credit balances)
        var liabilities = accounts.Where(a => a.AccountType == AccountType.Liability).Select(a =>
        {
            var accountBalance = balances.TryGetValue(a.Id, out var bal) ? bal : 0m;
            return new StatementRowDto
            {
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                Balance = accountBalance < 0 ? Math.Abs(accountBalance) : -accountBalance
            };
        }).Where(r => r.Balance != 0).ToList();

        // Equities (Credit balances)
        var equities = accounts.Where(a => a.AccountType == AccountType.Equity).Select(a =>
        {
            var accountBalance = balances.TryGetValue(a.Id, out var bal) ? bal : 0m;
            return new StatementRowDto
            {
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                Balance = accountBalance < 0 ? Math.Abs(accountBalance) : -accountBalance
            };
        }).Where(r => r.Balance != 0).ToList();

        // Calculate Retained Earnings (Net Surplus up to AsOfDate)
        var revenueAccounts = accounts.Where(a => a.AccountType == AccountType.Revenue).Select(a => a.Id).ToList();
        var expenseAccounts = accounts.Where(a => a.AccountType == AccountType.Expense).Select(a => a.Id).ToList();
        
        var totalRevenue = revenueAccounts.Sum(id => balances.TryGetValue(id, out var b) ? b : 0m);
        var totalExpense = expenseAccounts.Sum(id => balances.TryGetValue(id, out var b) ? b : 0m);
        
        // Revenue is credit (negative), Expense is debit (positive)
        // Net Surplus = Revenue (abs) - Expense
        var netSurplus = (totalRevenue < 0 ? Math.Abs(totalRevenue) : -totalRevenue) - totalExpense;

        if (netSurplus != 0)
        {
            equities.Add(new StatementRowDto
            {
                AccountCode = "RETAINED-EARNINGS",
                AccountName = "Retained Earnings",
                Balance = netSurplus
            });
        }

        var dto = new BalanceSheetDto
        {
            Assets = assets,
            Liabilities = liabilities,
            Equities = equities,
            TotalAssets = assets.Sum(a => a.Balance),
            TotalLiabilities = liabilities.Sum(l => l.Balance),
            TotalEquity = equities.Sum(e => e.Balance)
        };

        return AppResponses.Success(dto);
    }
}

