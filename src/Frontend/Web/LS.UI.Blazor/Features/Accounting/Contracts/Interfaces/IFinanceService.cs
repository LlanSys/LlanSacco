using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using System;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Accounting.Contracts.Interfaces;

internal interface IFinanceService
{
    Task<AppResponse<TrialBalanceResponse>> GetTrialBalanceAsync(DateTimeOffset? asOfDate);
    Task<AppResponse<IncomeStatementDto>> GetIncomeStatementAsync(DateTimeOffset? startDate, DateTimeOffset? endDate);
    Task<AppResponse<BalanceSheetDto>> GetBalanceSheetAsync(DateTimeOffset? asOfDate);
    Task<AppResponse<AccountingDashboardStatsResponse>> GetDashboardStatsAsync();
    Task<AppResponse<System.Collections.Generic.IReadOnlyList<AccountResponse>>> GetAccountsAsync();
    Task<AppResponse<Guid>> PostJournalAsync(CreateJournalRequest request);
    Task<AppResponse<System.Collections.Generic.IEnumerable<AccountingIntegrationErrorDto>>> GetIntegrationErrorsAsync(string? status = null);
    Task<AppResponse<bool>> RetryIntegrationEventAsync(Guid errorId);

