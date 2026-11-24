using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Commands;

public sealed record CancelGuarantorSecurityReviewCommand(Guid ReviewId) : IRequest<AppResponse<bool>>;

internal sealed class CancelGuarantorSecurityReviewCommandHandler(ILoansUnitOfWork unitOfWork,
    ICurrentActorProvider actor, IContextEventPublisher<ILoansUnitOfWork> publisher, TimeProvider clock)
    : IRequestHandler<CancelGuarantorSecurityReviewCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(CancelGuarantorSecurityReviewCommand request, CancellationToken ct) =>
        unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var review = await unitOfWork.GuarantorSecurityReviewRepository.FindByIdAsync(request.ReviewId, ct).ConfigureAwait(false);

                if (review is null)
                {
                    return AppResponses.NotFound<bool>("Security review not found.");
                }

                if (review.Status == "Cancelled")
                {
                    return AppResponses.Success(true);
                }

                if (review.Status != "AwaitingApproval")
                {
                    return AppResponses.Conflict<bool>("A decision already sent to Banking must be reconciled before cancellation.");
                }

                var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(review.LoanId, ct).ConfigureAwait(false);

                if (loan is null)
                {
                    return AppResponses.NotFound<bool>("Loan not found.");
                }

                if (review.ReplacementPledgeId.HasValue)
                {
                    var replacement = await unitOfWork.LoanGuarantorRepository.FindByIdAsync(review.ReplacementPledgeId.Value, ct).ConfigureAwait(false);

                    if (replacement is not null)
                    {
                        if (!replacement.SavingsAccountId.HasValue)
                        {
                            replacement.Status = LS.Domain.Features.Loans.Enums.GuarantorStatus.Released;
                            await unitOfWork.LoanGuarantorRepository.UpdateAsync(replacement, ct).ConfigureAwait(false);
                        }
                        await GuarantorReservationMapping.StageReleaseAsync(
                        loan,
                        unitOfWork,
                        publisher,
                        actor.ActorId,
                            clock.GetUtcNow(),
                        ct,
                        [replacement])
                            .ConfigureAwait(false);
                    }
                }
                review.Status = "Cancelled";
                review.UpdatedBy = actor.ActorId;
                review.UpdatedAt = clock.GetUtcNow();
                loan.PendingSecurityReviewId = null;
                loan.UpdatedBy = actor.ActorId;
                loan.UpdatedAt = clock.GetUtcNow();
                await unitOfWork.GuarantorSecurityReviewRepository.UpdateAsync(review, ct).ConfigureAwait(false);
                await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, ct).ConfigureAwait(false);
                return AppResponses.Success(true);
            },
            cancellationToken: ct);
}
