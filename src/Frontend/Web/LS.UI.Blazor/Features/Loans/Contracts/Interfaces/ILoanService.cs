using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Loans.Contracts.Interfaces;

internal interface ILoanService
{
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

