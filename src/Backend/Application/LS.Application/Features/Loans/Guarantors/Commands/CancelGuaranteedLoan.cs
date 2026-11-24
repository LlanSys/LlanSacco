using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Commands;

public sealed record CancelGuaranteedLoanCommand(Guid LoanId, string Reason) : IRequest<AppResponse<bool>>;

internal sealed class CancelGuaranteedLoanCommandHandler(ILoansUnitOfWork unitOfWork,
    ICurrentActorProvider actor, IContextEventPublisher<ILoansUnitOfWork> publisher, TimeProvider clock)
    : IRequestHandler<CancelGuaranteedLoanCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(CancelGuaranteedLoanCommand request, CancellationToken ct) =>
        unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanId, ct).ConfigureAwait(false);

                if (loan is null)
                {
                    return AppResponses.NotFound<bool>("Loan not found.");
                }

                if (loan.Status == LoanStatus.Cancelled)
                {
                    return AppResponses.Success(true);
                }

                if (loan.Status is not (LoanStatus.Draft or LoanStatus.Submitted or LoanStatus.UnderReview or LoanStatus.Approved or LoanStatus.Rejected))
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("A disbursement in progress or an issued loan cannot be cancelled."));
                }

                loan.Status = LoanStatus.Cancelled;
                loan.SecurityFailure = request.Reason;
                loan.UpdatedAt = clock.GetUtcNow();
                loan.UpdatedBy = actor.ActorId;
                await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, ct).ConfigureAwait(false);
                await GuarantorReservationMapping.StageReleaseAsync(loan, unitOfWork, publisher, actor.ActorId, clock.GetUtcNow(), ct).ConfigureAwait(false);
                return AppResponses.Success("Cancellation recorded. Any pending funds release remains visible until Banking confirms it.", true);
            },
            cancellationToken: ct);
}
