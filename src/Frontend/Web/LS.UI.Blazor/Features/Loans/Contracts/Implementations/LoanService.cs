using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using LS.UI.Blazor.Features.Loans.Contracts.Interfaces;
using LS.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Loans.Contracts.Implementations;

internal sealed class LoanService(IBackendApiClient _apiClient) : ILoanService
{
    private const string BaseUrl = "api/v1.0/loans";

    public async Task<AppResponse<List<LoanProductResponse>>> GetLoanProductsAsync()
    {
        return await _apiClient.SendAsync<List<LoanProductResponse>>(HttpMethod.Get, $"{BaseUrl}/products").ConfigureAwait(false);
    }

    public async Task<AppResponse<Guid>> CreateLoanProductAsync(CreateLoanProductRequest request)
    {
        return await _apiClient.SendAsync<Guid>(HttpMethod.Post, $"{BaseUrl}/products", request).ConfigureAwait(false);
    }

    public async Task<AppResponse<bool>> UpdateLoanProductAsync(Guid id, UpdateLoanProductRequest request)
    {
        return await _apiClient.SendAsync<bool>(HttpMethod.Put, $"{BaseUrl}/products/{id}", request).ConfigureAwait(false);
    }

    public async Task<AppResponse<LoanDashboardStatsResponse>> GetDashboardStatsAsync()
    {
        return await _apiClient.SendAsync<LoanDashboardStatsResponse>(HttpMethod.Get, $"{BaseUrl}/dashboard-stats").ConfigureAwait(false);
    }

    public async Task<AppResponse<List<LoanApplicationResponse>>> GetLoanApplicationsAsync(Guid? memberId = null)
    {
        var url = $"{BaseUrl}/applications";
        if (memberId.HasValue)
        {
            url += $"?memberId={memberId.Value}";
        }
        return await _apiClient.SendAsync<List<LoanApplicationResponse>>(HttpMethod.Get, url).ConfigureAwait(false);
    }

    public async Task<AppResponse<Guid>> ApplyForLoanAsync(ApplyForLoanRequest request)
    {
        return await _apiClient.SendAsync<Guid>(HttpMethod.Post, $"{BaseUrl}/applications", request).ConfigureAwait(false);
    }

    public async Task<AppResponse<bool>> ApproveLoanAsync(Guid id)
    {
        return await _apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/applications/{id}/approve", new { }).ConfigureAwait(false);
    }

    public async Task<AppResponse<bool>> RejectLoanAsync(Guid id, string reason)
    {
        return await _apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/applications/{id}/reject", reason).ConfigureAwait(false);
    }

    public async Task<AppResponse<Guid>> ProcessRepaymentAsync(Guid id, ProcessRepaymentRequest request)
    {
        return await _apiClient.SendAsync<Guid>(HttpMethod.Post, $"{BaseUrl}/applications/{id}/repayments", request).ConfigureAwait(false);
    }
}




