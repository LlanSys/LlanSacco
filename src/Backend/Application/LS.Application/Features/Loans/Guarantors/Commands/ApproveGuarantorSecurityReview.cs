using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Guarantors.Events;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Commands;

public sealed record ApproveGuarantorSecurityReviewCommand(Guid ReviewId) : IRequest<AppResponse<bool>>;

internal sealed class ApproveGuarantorSecurityReviewCommandHandler(ILoansUnitOfWork unitOfWork,
    ICurrentActorProvider actor, IContextEventPublisher<ILoansUnitOfWork> publisher, TimeProvider clock)
    : IRequestHandler<ApproveGuarantorSecurityReviewCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(ApproveGuarantorSecurityReviewCommand request, CancellationToken ct) =>
        unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var review = await unitOfWork.GuarantorSecurityReviewRepository.FindByIdAsync(request.ReviewId, ct).ConfigureAwait(false);

                if (review is null)
                {
                    return AppResponses.NotFound<bool>("Security review not found.");
                }

                if (string.Equals(review.CreatedBy, actor.ActorId, StringComparison.OrdinalIgnoreCase))
                {
                    return AppResponses.Failure<bool>(AppError.Forbidden("An independent security approver is required."));
                }

                if (review.Status is "Pending" or "Completed")
                {
                    return AppResponses.Success(true);
                }

                if (review.Status != "AwaitingApproval")
                {
                    return AppResponses.Conflict<bool>("This review has a terminal decision.");
                }

                var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(review.LoanId, ct).ConfigureAwait(false);
                var pledge = await unitOfWork.LoanGuarantorRepository.FindByIdAsync(review.PledgeId, ct).ConfigureAwait(false);

                if (loan is null || pledge is null || loan.PendingSecurityReviewId != review.Id || loan.PendingRecoveryPrincipal > 0)
                {
                    return AppResponses.Conflict<bool>("The loan security decision changed during review.");
                }

                GuarantorReservationInstruction instruction;

                if (review.Kind == "AdoptLegacy")
                {
                    if (pledge.Status != GuarantorStatus.Accepted || pledge.LockedAmount <= 0)
                    {
                        return AppResponses.Conflict<bool>("The identified legacy hold is no longer eligible for adoption.");
                    }

                    pledge.AcceptedAmount = review.SourceHeldAmount;
                    pledge.SavingsAccountId = review.SavingsAccountId;
                    pledge.ReservationExpiresAt = DateTimeOffset.MaxValue;
                    pledge.ReservationPolicyVersion = "legacy:" + pledge.Id.ToString("N");
                    pledge.Status = GuarantorStatus.PendingReservation;
                    await unitOfWork.LoanGuarantorRepository.UpdateAsync(pledge, ct).ConfigureAwait(false);
                    instruction = new(
                    loan.TenantId,
                    review.Id,
                    loan.Id,
                    "AdoptLegacy",
                    [GuarantorReservationMapping.ToItem(pledge)],
                        review.CreatedBy,
                    ReleaseCommitted: loan.Status == LoanStatus.Closed,
                    ApprovedBy: actor.ActorId,
                    EvidenceReference: review.EvidenceReference);
                }
                else
                {
                    var pledges = await unitOfWork.LoanGuarantorRepository.ListAsync(q => q.Where(x => x.LoanApplicationId == loan.Id), ct).ConfigureAwait(false);
                    var replacement = pledges.Single(x => x.Id == review.ReplacementPledgeId);

                    if (pledge.Status != GuarantorStatus.Committed || replacement.Status != GuarantorStatus.ReservationHeld || replacement.LockedAmount < pledge.LockedAmount)
                    {
                        return AppResponses.Failure<bool>(AppError.BusinessRule("The replacement needs confirmed coverage before substitution can be approved."));
                    }

                    var coverage = pledges.Where(x => x.Id != pledge.Id && (x.Status == GuarantorStatus.Committed || x.Id == replacement.Id)).ToArray();
                    instruction = new(
                    loan.TenantId,
                    review.Id,
                    loan.Id,
                    "Commit",
                    coverage.Select(x => GuarantorReservationMapping.ToItem(x)).ToArray(),
                        review.CreatedBy,
                    coverage.Sum(x => x.LockedAmount),
                    ApprovedBy: actor.ActorId,
                    EvidenceReference: review.EvidenceReference);
                }
                review.ApprovedBy = actor.ActorId;
                review.Status = "Pending";
                review.UpdatedBy = actor.ActorId;
                review.UpdatedAt = clock.GetUtcNow();
                await unitOfWork.GuarantorSecurityReviewRepository.UpdateAsync(review, ct).ConfigureAwait(false);
                await publisher.PublishAsync(instruction, ct).ConfigureAwait(false);
                return AppResponses.Success(true);
            },
            cancellationToken: ct);
}
