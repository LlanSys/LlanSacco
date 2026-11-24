using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Loans.Contracts.Interfaces;

internal interface ILoanService
{
    Task<AppResponse<MyGuarantorPledgesResponse>> GetMyGuarantorPledgesAsync(int page, CancellationToken ct = default);
    Task<AppResponse<bool>> AcceptGuarantorPledgeAsync(Guid loanId, AcceptGuarantorPledgeRequest request, CancellationToken ct = default);
    Task<AppResponse<LoanApplicationResponse>> GetGuarantorSummaryAsync(Guid loanId, CancellationToken ct = default);
    Task<AppResponse<List<GuarantorOperationResponse>>> GetGuarantorOperationsAsync(Guid loanId, CancellationToken ct = default);
    Task<AppResponse<List<GuarantorSavingsAccountResponse>>> GetReviewAccountsAsync(Guid loanId, Guid pledgeId, CancellationToken ct = default);
    Task<AppResponse<List<LS.SharedKernel.Features.Membership.Dtos.MemberOptionResponse>>> GetGuarantorMemberOptionsAsync(string search, CancellationToken ct = default);
    Task<AppResponse<List<GuarantorReconciliationResponse>>> GetGuarantorReconciliationAsync(int page, CancellationToken ct = default);
    Task<AppResponse<Guid>> RequestGuarantorRecoveryAsync(Guid loanId, GuarantorRecoveryRequest request, CancellationToken ct = default);
    Task<AppResponse<Guid>> RequestGuarantorSecurityReviewAsync(Guid loanId, GuarantorSecurityReviewRequest request, CancellationToken ct = default);
    Task<AppResponse<bool>> ApproveGuarantorOperationAsync(Guid id, bool recovery, CancellationToken ct = default);
    Task<AppResponse<bool>> CancelGuarantorSecurityReviewAsync(Guid id, CancellationToken ct = default);
    Task<AppResponse<bool>> CancelGuarantorRecoveryAsync(Guid id, CancellationToken ct = default);
    Task<AppResponse<bool>> DisburseLoanAsync(Guid loanId, CancellationToken ct = default);
    Task<AppResponse<bool>> CancelLoanAsync(Guid loanId, string reason, CancellationToken ct = default);
    Task<AppResponse<List<LoanProductResponse>>> GetLoanProductsAsync();
    Task<AppResponse<Guid>> CreateLoanProductAsync(CreateLoanProductRequest request);
    Task<AppResponse<bool>> UpdateLoanProductAsync(Guid id, UpdateLoanProductRequest request);

    Task<AppResponse<LoanDashboardStatsResponse>> GetDashboardStatsAsync();
    Task<AppResponse<List<LoanApplicationResponse>>> GetLoanApplicationsAsync(Guid? memberId = null);
    Task<AppResponse<Guid>> ApplyForLoanAsync(ApplyForLoanRequest request);
    Task<AppResponse<bool>> ApproveLoanAsync(Guid id);
    Task<AppResponse<bool>> RejectLoanAsync(Guid id, string reason);
    Task<AppResponse<Guid>> ProcessRepaymentAsync(Guid id, ProcessRepaymentRequest request);
}

