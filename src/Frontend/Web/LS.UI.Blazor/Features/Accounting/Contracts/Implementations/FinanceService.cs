using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.UI.Blazor.Features.Accounting.Contracts.Interfaces;
using LS.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using LS.UI.Blazor.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Accounting.Contracts.Implementations;

internal sealed class FinanceService(IBackendApiClient apiClient, IOptions<BackendApiSettings> apiSettings) : IFinanceService
{
    public Task<AppResponse<TrialBalanceResponse>> GetTrialBalanceAsync(DateTimeOffset? asOfDate)
    {
        var url = "api/v1.0/finance/trial-balance";
        if (asOfDate.HasValue)
        {
            url += $"?asOfDate={Uri.EscapeDataString(asOfDate.Value.ToString("o"))}";
        }
        
        return apiClient.SendAsync<TrialBalanceResponse>(
            HttpMethod.Get,
            url
        );
    }

    public Task<AppResponse<IncomeStatementDto>> GetIncomeStatementAsync(DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        var url = "api/v1.0/finance/income-statement";
        
        var queryParams = new System.Collections.Generic.List<string>();
        if (startDate.HasValue) queryParams.Add($"startDate={Uri.EscapeDataString(startDate.Value.ToString("o"))}");
        if (endDate.HasValue) queryParams.Add($"endDate={Uri.EscapeDataString(endDate.Value.ToString("o"))}");
        
        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        return apiClient.SendAsync<IncomeStatementDto>(
            HttpMethod.Get,
            url
        );
    }

    public Task<AppResponse<BalanceSheetDto>> GetBalanceSheetAsync(DateTimeOffset? asOfDate)
    {
        var url = "api/v1.0/finance/balance-sheet";
        if (asOfDate.HasValue)
        {
            url += $"?asOfDate={Uri.EscapeDataString(asOfDate.Value.ToString("o"))}";
        }

        return apiClient.SendAsync<BalanceSheetDto>(
            HttpMethod.Get,
            url
        );
    }

    public Task<AppResponse<AccountingDashboardStatsResponse>> GetDashboardStatsAsync()
    {
        return apiClient.SendAsync<AccountingDashboardStatsResponse>(
            HttpMethod.Get,
            "api/v1.0/finance/dashboard-stats"
        );
    }

    public Task<AppResponse<System.Collections.Generic.IReadOnlyList<AccountResponse>>> GetAccountsAsync()
    {
        var url = "api/v1.0/finance/accounts";
        return apiClient.SendAsync<System.Collections.Generic.IReadOnlyList<AccountResponse>>(
            HttpMethod.Get,
            url
        );
    }

    public Task<AppResponse<Guid>> PostJournalAsync(CreateJournalRequest request)
    {
        var url = "api/v1.0/finance/journals";
        return apiClient.SendAsync<Guid>(
            HttpMethod.Post,
            url,
            request
        );
    }

    public Task<AppResponse<System.Collections.Generic.IEnumerable<AccountingIntegrationErrorDto>>> GetIntegrationErrorsAsync(string? status = null)
    {
        var url = "api/v1.0/finance/integration-errors";
        if (!string.IsNullOrEmpty(status))
        {
            url += $"?status={Uri.EscapeDataString(status)}";
        }
        return apiClient.SendAsync<System.Collections.Generic.IEnumerable<AccountingIntegrationErrorDto>>(
            HttpMethod.Get,
            url
        );
    }

    public Task<AppResponse<bool>> RetryIntegrationEventAsync(Guid errorId)
    {
        var url = $"api/v1.0/finance/integration-errors/{errorId}/retry";
        return apiClient.SendAsync<bool>(
            HttpMethod.Post,
            url,
            null // Empty payload for POST to this endpoint
        );
    }
}

