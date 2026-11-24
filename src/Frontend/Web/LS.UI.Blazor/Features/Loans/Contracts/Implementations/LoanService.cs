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

    public Task<AppResponse<MyGuarantorPledgesResponse>> GetMyGuarantorPledgesAsync(int page, CancellationToken ct = default) =>
        _apiClient.SendAsync<MyGuarantorPledgesResponse>(HttpMethod.Get, $"{BaseUrl}/applications/my-guarantor-pledges?page={page}", cancellationToken: ct);

    public Task<AppResponse<bool>> AcceptGuarantorPledgeAsync(Guid loanId, AcceptGuarantorPledgeRequest request, CancellationToken ct = default) =>
        _apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/applications/{loanId}/guarantors/accept", request, cancellationToken: ct);

    public Task<AppResponse<LoanApplicationResponse>> GetGuarantorSummaryAsync(Guid loanId, CancellationToken ct = default) =>
        _apiClient.SendAsync<LoanApplicationResponse>(HttpMethod.Get, $"{BaseUrl}/applications/{loanId}/guarantor-summary", cancellationToken: ct);

    public Task<AppResponse<List<GuarantorOperationResponse>>> GetGuarantorOperationsAsync(Guid loanId, CancellationToken ct = default) =>
        _apiClient.SendAsync<List<GuarantorOperationResponse>>(HttpMethod.Get, $"{BaseUrl}/applications/{loanId}/guarantor-operations", cancellationToken: ct);

    public Task<AppResponse<List<GuarantorSavingsAccountResponse>>> GetReviewAccountsAsync(Guid loanId, Guid pledgeId, CancellationToken ct = default) =>
        _apiClient.SendAsync<List<GuarantorSavingsAccountResponse>>(HttpMethod.Get, $"{BaseUrl}/applications/{loanId}/guarantor-review-accounts/{pledgeId}", cancellationToken: ct);

    public Task<AppResponse<List<LS.SharedKernel.Features.Membership.Dtos.MemberOptionResponse>>> GetGuarantorMemberOptionsAsync(string search, CancellationToken ct = default) =>
        _apiClient.SendAsync<List<LS.SharedKernel.Features.Membership.Dtos.MemberOptionResponse>>(
            HttpMethod.Get,
            $"{BaseUrl}/applications/guarantor-member-options?search={Uri.EscapeDataString(search)}",
            cancellationToken: ct);

    public Task<AppResponse<List<GuarantorReconciliationResponse>>> GetGuarantorReconciliationAsync(int page, CancellationToken ct = default) =>
        _apiClient.SendAsync<List<GuarantorReconciliationResponse>>(HttpMethod.Get, $"{BaseUrl}/applications/guarantor-reconciliation?page={page}", cancellationToken: ct);

    public Task<AppResponse<Guid>> RequestGuarantorRecoveryAsync(Guid loanId, GuarantorRecoveryRequest request, CancellationToken ct = default) =>
        _apiClient.SendAsync<Guid>(HttpMethod.Post, $"{BaseUrl}/applications/{loanId}/guarantor-recoveries", request, cancellationToken: ct);

    public Task<AppResponse<Guid>> RequestGuarantorSecurityReviewAsync(Guid loanId, GuarantorSecurityReviewRequest request, CancellationToken ct = default) =>
        _apiClient.SendAsync<Guid>(HttpMethod.Post, $"{BaseUrl}/applications/{loanId}/guarantor-security-reviews", request, cancellationToken: ct);

    public Task<AppResponse<bool>> ApproveGuarantorOperationAsync(Guid id, bool recovery, CancellationToken ct = default) =>
        _apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/applications/{(recovery ? "guarantor-recoveries" : "guarantor-security-reviews")}/{id}/approve", new { }, cancellationToken: ct);

    public Task<AppResponse<bool>> CancelGuarantorRecoveryAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/applications/guarantor-recoveries/{id}/cancel", new { }, cancellationToken: ct);

    public Task<AppResponse<bool>> CancelGuarantorSecurityReviewAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/applications/guarantor-security-reviews/{id}/cancel", new { }, cancellationToken: ct);

    public Task<AppResponse<bool>> DisburseLoanAsync(Guid loanId, CancellationToken ct = default) =>
        _apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/applications/{loanId}/disburse", new { }, cancellationToken: ct);

    public Task<AppResponse<bool>> CancelLoanAsync(Guid loanId, string reason, CancellationToken ct = default) =>
        _apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/applications/{loanId}/cancel", reason, cancellationToken: ct);

    public async Task<AppResponse<List<LoanProductResponse>>> GetLoanProductsAsync() => await _apiClient.SendAsync<List<LoanProductResponse>>(HttpMethod.Get, $"{BaseUrl}/products").ConfigureAwait(false);

    public async Task<AppResponse<Guid>> CreateLoanProductAsync(CreateLoanProductRequest request) => await _apiClient.SendAsync<Guid>(HttpMethod.Post, $"{BaseUrl}/products", request).ConfigureAwait(false);

    public async Task<AppResponse<bool>> UpdateLoanProductAsync(Guid id, UpdateLoanProductRequest request) => await _apiClient.SendAsync<bool>(HttpMethod.Put, $"{BaseUrl}/products/{id}", request).ConfigureAwait(false);

    public async Task<AppResponse<LoanDashboardStatsResponse>> GetDashboardStatsAsync() => await _apiClient.SendAsync<LoanDashboardStatsResponse>(HttpMethod.Get, $"{BaseUrl}/dashboard-stats").ConfigureAwait(false);

    public async Task<AppResponse<List<LoanApplicationResponse>>> GetLoanApplicationsAsync(Guid? memberId = null)
    {
        var url = $"{BaseUrl}/applications";

        if (memberId.HasValue)
        {
            url += $"?memberId={memberId.Value}";
        }
        return await _apiClient.SendAsync<List<LoanApplicationResponse>>(HttpMethod.Get, url).ConfigureAwait(false);
    }

    public async Task<AppResponse<Guid>> ApplyForLoanAsync(ApplyForLoanRequest request) => await _apiClient.SendAsync<Guid>(HttpMethod.Post, $"{BaseUrl}/applications", request).ConfigureAwait(false);

    public async Task<AppResponse<bool>> ApproveLoanAsync(Guid id) => await _apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/applications/{id}/approve", new { }).ConfigureAwait(false);

    public async Task<AppResponse<bool>> RejectLoanAsync(Guid id, string reason) => await _apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/applications/{id}/reject", reason).ConfigureAwait(false);

    public async Task<AppResponse<Guid>> ProcessRepaymentAsync(Guid id, ProcessRepaymentRequest request) => await _apiClient.SendAsync<Guid>(HttpMethod.Post, $"{BaseUrl}/applications/{id}/repayments", request).ConfigureAwait(false);
}




