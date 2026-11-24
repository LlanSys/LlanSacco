using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using System;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Accounting.Contracts.Interfaces;

internal interface IFinanceService
{
    Task<AppResponse<TrialBalanceResponse>> GetTrialBalanceAsync(DateTimeOffset? asOfDate);
    Task<AppResponse<IncomeStatementResponse>> GetIncomeStatementAsync(DateTimeOffset? startDate, DateTimeOffset? endDate);
    Task<AppResponse<BalanceSheetResponse>> GetBalanceSheetAsync(DateTimeOffset? asOfDate);
    Task<AppResponse<AccountingDashboardStatsResponse>> GetDashboardStatsAsync();
    Task<AppResponse<System.Collections.Generic.IReadOnlyList<AccountResponse>>> GetAccountsAsync();
    Task<AppResponse<Guid>> PostJournalAsync(CreateJournalRequest request);
    Task<AppResponse<System.Collections.Generic.IEnumerable<AccountingIntegrationErrorResponse>>> GetIntegrationErrorsAsync(string? status = null);
    Task<AppResponse<bool>> RetryIntegrationEventAsync(Guid errorId);
    Task<AppResponse<System.Collections.Generic.IReadOnlyList<GlMappingResponse>>> GetGlMappingsAsync();
    Task<AppResponse<Guid>> UpsertGlMappingAsync(string transactionTypeCode, UpsertGlMappingRequest request);
}
