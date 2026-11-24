using LS.Domain.Features.Loans.Contracts;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Dtos;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Queries;

internal sealed class GetGuarantorOperationsQueryHandler(ILoansUnitOfWork unitOfWork)
    : IRequestHandler<GetGuarantorOperationsQuery, AppResponse<List<GuarantorOperationResponse>>>
{
    public async Task<AppResponse<List<GuarantorOperationResponse>>> Handle(GetGuarantorOperationsQuery request, CancellationToken ct)
    {
        if (!await unitOfWork.LoanApplicationRepository.AnyAsync(x => x.Id == request.LoanId, ct).ConfigureAwait(false))
        {
            return AppResponses.NotFound<List<GuarantorOperationResponse>>("Loan not found.");
        }

        var recoveries = await unitOfWork.GuarantorRecoveryRepository.ListAsync(
            q => q.Where(x => x.LoanId == request.LoanId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(100)
                .Select(x => new GuarantorOperationResponse(
                    x.Id,
                    "RecoverPrincipal",
                    x.Status,
                        x.Amount,
                    x.CreatedAt,
                    x.CreatedBy,
                    x.ApprovedBy,
                    x.FailureReason,
                    x.EvidenceReference)),
            ct)
            .ConfigureAwait(false);

        var reviews = await unitOfWork.GuarantorSecurityReviewRepository.ListAsync(
            q => q.Where(x => x.LoanId == request.LoanId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(100)
                .Select(x => new GuarantorOperationResponse(
                    x.Id,
                    x.Kind,
                    x.Status,
                        x.SourceHeldAmount,
                    x.CreatedAt,
                    x.CreatedBy,
                    x.ApprovedBy,
                    x.FailureReason,
                    x.EvidenceReference,
                    x.PledgeId,
                    x.ReplacementMemberId)),
            ct)
            .ConfigureAwait(false);

        return AppResponses.Success(recoveries.Concat(reviews).OrderByDescending(x => x.CreatedAt).ToList());
    }
}
