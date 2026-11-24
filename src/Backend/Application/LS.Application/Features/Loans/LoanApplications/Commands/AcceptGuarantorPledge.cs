using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.Membership.Contracts.Interfaces;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Guarantors.Events;
using MediatR;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

public sealed record AcceptGuarantorPledgeCommand(Guid LoanApplicationId, Guid GuarantorMemberId,
    decimal AcceptedAmount, Guid SavingsAccountId = default, bool TermsAccepted = false,
    string TermsVersion = "", Guid PledgeId = default) : IRequest<AppResponse<bool>>;

internal sealed class AcceptGuarantorPledgeCommandHandler(
    ILoansUnitOfWork unitOfWork, ICurrentActorProvider actor, ICurrentMemberResolver currentMember,
    IContextEventPublisher<ILoansUnitOfWork> publisher, TimeProvider clock)
    : IRequestHandler<AcceptGuarantorPledgeCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(AcceptGuarantorPledgeCommand request, CancellationToken ct)
    {
        if (await currentMember.FindActiveMemberIdAsync(ct).ConfigureAwait(false) != request.GuarantorMemberId)
        {
            return AppResponses.NotFound<bool>("Guarantor not found on this loan application.");
        }

        if (await currentMember.FindVerifiedActiveMemberIdAsync(ct).ConfigureAwait(false) != request.GuarantorMemberId)
        {
            return AppResponses.Failure<bool>(AppError.BusinessRule("Complete member identity verification before accepting a guarantee."));
        }

        return await unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanApplicationId, ct).ConfigureAwait(false);
                var guarantor = await unitOfWork.LoanGuarantorRepository.FirstOrDefaultAsync(
                x => x.Id == request.PledgeId &&
                    x.LoanApplicationId == request.LoanApplicationId && x.GuarantorMemberId == request.GuarantorMemberId,
                ct)
                    .ConfigureAwait(false);

                if (loan is null || guarantor is null)
                {
                    return AppResponses.NotFound<bool>("Guarantor not found on this loan application.");
                }

                if (loan.MemberId == request.GuarantorMemberId || request.AcceptedAmount <= 0 ||
                    request.AcceptedAmount > guarantor.GuaranteedAmount || request.SavingsAccountId == Guid.Empty)
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("Choose an eligible account and a positive amount within the nominated guarantee."));
                }

                if (loan.Status is not (LoanStatus.Submitted or LoanStatus.Approved or LoanStatus.UnderReview) &&
                    !(guarantor.ReplacesPledgeId.HasValue && loan.PendingSecurityReviewId.HasValue &&
                      loan.Status is LoanStatus.Disbursed or LoanStatus.Active or LoanStatus.Arrears or LoanStatus.Defaulted))
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("This loan is no longer accepting guarantor consent."));
                }

                if (guarantor.ReplacesPledgeId.HasValue && loan.PendingSecurityReviewId.HasValue)
                {
                    var review = await unitOfWork.GuarantorSecurityReviewRepository.FindByIdAsync(loan.PendingSecurityReviewId.Value, ct).ConfigureAwait(false);

                    if (review is null || review.ReplacementPledgeId != guarantor.Id || review.Status != "AwaitingApproval")
                    {
                        return AppResponses.Conflict<bool>("This replacement consent is no longer under review.");
                    }
                }

                if (guarantor.Status != GuarantorStatus.Nominated)
                {
                    return guarantor.AcceptedAmount == request.AcceptedAmount && guarantor.SavingsAccountId == request.SavingsAccountId &&
                        string.Equals(guarantor.ReservationPolicyVersion, request.TermsVersion, StringComparison.Ordinal) &&
                        guarantor.Status is GuarantorStatus.PendingReservation or GuarantorStatus.ReservationHeld or GuarantorStatus.Committed
                        ? AppResponses.Success("Consent is recorded; see the current reservation status.", true)
                        : AppResponses.Failure<bool>(AppError.Conflict("This pledge already has a different or terminal decision."));
                }

                var product = await unitOfWork.LoanProductRepository.FindByIdAsync(loan.LoanProductId, ct).ConfigureAwait(false);

                if (product is null || product.GuarantorReservationDays < 1 || product.GuarantorReservationDays > 365 ||
                    string.IsNullOrWhiteSpace(product.GuarantorPolicyVersion))
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("The guarantor policy requires configuration."));
                }

                var now = clock.GetUtcNow();

                if (!request.TermsAccepted || !string.Equals(request.TermsVersion, product.GuarantorPolicyVersion, StringComparison.Ordinal))
                {
                    return AppResponses.Conflict<bool>("Review and accept the current guarantor terms before submitting consent.");
                }

                if (product.GuarantorConsentDays < 1 || product.GuarantorConsentDays > 365 ||
                    guarantor.CreatedAt.AddDays(product.GuarantorConsentDays) <= now)
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("The guarantor consent request has expired."));
                }

                guarantor.Status = GuarantorStatus.PendingReservation;
                guarantor.SavingsAccountId = request.SavingsAccountId;
                guarantor.AcceptedAmount = request.AcceptedAmount;
                guarantor.LockedAmount = 0;
                guarantor.ReservationPolicyVersion = product.GuarantorPolicyVersion;
                guarantor.ConsentActorId = actor.ActorId;
                guarantor.ConsentAt = now;
                guarantor.ReservationExpiresAt = now.AddDays(product.GuarantorReservationDays);
                guarantor.UpdatedBy = actor.ActorId;
                guarantor.UpdatedAt = now;
                await unitOfWork.LoanGuarantorRepository.UpdateAsync(guarantor, ct).ConfigureAwait(false);
                // A concurrent cancellation must also conflict with this consent.
                loan.UpdatedAt = now;
                loan.UpdatedBy = actor.ActorId;
                await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, ct).ConfigureAwait(false);
                await publisher.PublishAsync(
                new GuarantorReservationInstruction(
                loan.TenantId,
                guarantor.Id,
                loan.Id,
                "Reserve",
                    [new(
                    guarantor.Id,
                    guarantor.GuarantorMemberId,
                    request.SavingsAccountId,
                    request.AcceptedAmount,
                        "KES",
                    guarantor.ReservationPolicyVersion,
                    guarantor.ReservationExpiresAt.Value)],
                actor.ActorId),
                ct)
                    .ConfigureAwait(false);

                return AppResponses.Success("Consent recorded. Funds reservation is pending Banking confirmation.", true);
            },
            cancellationToken: ct)
            .ConfigureAwait(false);
    }
}
