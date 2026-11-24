using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.Loans.IntegrationEvents;
using LS.Application.Features.Loans.LoanApplications.Commands;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Guarantors.Events;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Commands;

public sealed record ApplyGuarantorReservationOutcomeCommand(GuarantorReservationOutcome Outcome) : IRequest<AppResponse<bool>>;

internal sealed class ApplyGuarantorReservationOutcomeCommandHandler(ILoansUnitOfWork unitOfWork,
    ICurrentTenantProvider tenant, IContextEventPublisher<ILoansUnitOfWork> publisher,
    IEnumerable<IAmortizationStrategy> strategies, TimeProvider clock)
    : IRequestHandler<ApplyGuarantorReservationOutcomeCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(ApplyGuarantorReservationOutcomeCommand request, CancellationToken ct) =>
        unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var outcome = request.Outcome;

                if (tenant.TenantId == Guid.Empty || outcome.TenantId != tenant.TenantId)
                {
                    return AppResponses.Failure<bool>(AppError.Forbidden("The reservation outcome tenant is invalid."));
                }

                var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(outcome.LoanId, ct).ConfigureAwait(false);

                if (loan is null)
                {
                    return AppResponses.NotFound<bool>("Loan not found for the reservation outcome.");
                }

                var pledges = await unitOfWork.LoanGuarantorRepository.ListAsync(q => q.Where(x => x.LoanApplicationId == loan.Id), ct).ConfigureAwait(false);
                var byId = pledges.ToDictionary(x => x.Id);
                var terminalReleases = new List<LoanGuarantor>();

                foreach (var item in outcome.Items)
                {
                    if (!byId.TryGetValue(item.PledgeId, out var pledge) || pledge.SavingsAccountId != item.SavingsAccountId || pledge.AcceptedAmount != item.Amount)
                    {
                        return AppResponses.Conflict<bool>("The reservation outcome does not match the recorded consent.");
                    }
                }

                foreach (var item in outcome.Items)
                {
                    var pledge = byId[item.PledgeId];

                    if (item.Revision <= pledge.ReservationRevision)
                    {
                        continue;
                    }

                    pledge.ReservationRevision = item.Revision;
                    pledge.LockedAmount = item.RemainingAmount;
                    pledge.Status = item.State switch
                    {
                        "Held" => GuarantorStatus.ReservationHeld,
                        "Committed" => GuarantorStatus.Committed,
                        "Released" => GuarantorStatus.Released,
                        "Expired" => GuarantorStatus.Expired,
                        "Rejected" => GuarantorStatus.Rejected,
                        _ => throw new InvalidOperationException("Unknown reservation outcome state.")
                    };

                    pledge.ReservationFailure = outcome.Succeeded ? null : outcome.Reason;

                    if (loan.Status is LoanStatus.Cancelled or LoanStatus.Rejected or LoanStatus.Closed &&
                        pledge.Status is GuarantorStatus.ReservationHeld or GuarantorStatus.Committed)
                    {
                        pledge.Status = GuarantorStatus.ReleasePending;
                        terminalReleases.Add(pledge);
                    }
                    pledge.UpdatedBy = ICurrentActorProvider.SystemActor;
                    pledge.UpdatedAt = clock.GetUtcNow();
                    await unitOfWork.LoanGuarantorRepository.UpdateAsync(pledge, ct).ConfigureAwait(false);
                }

                if (outcome.Operation == "Commit" && loan.Status == LoanStatus.DisbursementPending && loan.DisbursementOperationId == outcome.OperationId)
                {
                    if (outcome.Succeeded)
                    {
                        if (outcome.Items.Count < 2 || outcome.Items.Any(x => x.State != "Committed") || outcome.Items.Sum(x => x.RemainingAmount) < loan.PrincipalAmount)
                        {
                            throw new InvalidOperationException("The committed coverage outcome is inconsistent.");
                        }

                        await DisburseLoanCommandHandler.StageDisbursementAsync(
                            loan,
                            unitOfWork,
                            strategies,
                            publisher,
                            loan.DisbursementRequestedBy ?? ICurrentActorProvider.SystemActor,
                            clock.GetUtcNow(),
                            ct)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        loan.Status = LoanStatus.Approved;
                        loan.SecurityFailure = outcome.Reason;
                    }
                }

                if (outcome.Operation is "Commit" or "AdoptLegacy")
                {
                    var review = await unitOfWork.GuarantorSecurityReviewRepository.FindByIdAsync(outcome.OperationId, ct).ConfigureAwait(false);

                    if (review is not null && review.Status == "Pending")
                    {
                        review.Status = outcome.Succeeded ? "Completed" : "Rejected";
                        review.FailureReason = outcome.Reason;
                        loan.PendingSecurityReviewId = null;

                        if (review.Kind == "Substitute" && outcome.Succeeded)
                        {
                            var original = byId[review.PledgeId];
                            original.Status = GuarantorStatus.ReleasePending;
                            await unitOfWork.LoanGuarantorRepository.UpdateAsync(original, ct).ConfigureAwait(false);
                            await publisher.PublishAsync(
                                new GuarantorReservationInstruction(
                                    loan.TenantId,
                                    Guid.CreateVersion7(),
                                    loan.Id,
                                        "Release",
                                    [GuarantorReservationMapping.ToItem(original)],
                                    review.CreatedBy,
                                        ReleaseCommitted: true,
                                    ApprovedBy: review.ApprovedBy,
                                    EvidenceReference: review.EvidenceReference),
                                ct)
                                .ConfigureAwait(false);
                        }

                        if (review.Kind == "AdoptLegacy" && !outcome.Succeeded)
                        {
                            var original = byId[review.PledgeId];
                            original.Status = GuarantorStatus.Accepted;
                            original.LockedAmount = review.SourceHeldAmount;
                            original.ReservationFailure = outcome.Reason;
                            await unitOfWork.LoanGuarantorRepository.UpdateAsync(original, ct).ConfigureAwait(false);
                        }
                        await unitOfWork.GuarantorSecurityReviewRepository.UpdateAsync(review, ct).ConfigureAwait(false);
                    }
                }

                if (outcome.Operation == "Recover")
                {
                    var recovery = await unitOfWork.GuarantorRecoveryRepository.FindByIdAsync(outcome.OperationId, ct).ConfigureAwait(false);

                    if (recovery is null || recovery.LoanId != loan.Id)
                    {
                        throw new InvalidOperationException("The recovery outcome has no authorized source instruction.");
                    }

                    if (recovery.Status == "Pending")
                    {
                        if (loan.PendingRecoveryPrincipal < recovery.Amount)
                        {
                            throw new InvalidOperationException("Pending recovery principal requires reconciliation.");
                        }

                        loan.PendingRecoveryPrincipal -= recovery.Amount;
                        recovery.Status = outcome.Succeeded ? "Completed" : "Rejected";
                        recovery.FailureReason = outcome.Reason;

                        if (outcome.Succeeded)
                        {
                            if (loan.OutstandingPrincipal < recovery.Amount)
                            {
                                throw new InvalidOperationException("Recovery would exceed outstanding principal.");
                            }

                            loan.OutstandingPrincipal -= recovery.Amount;
                            var repayment = LoanRepayment.Create(
                                loan.TenantId,
                                loan.Id,
                                "GR-" + recovery.Id.ToString("N"),
                                recovery.Amount,
                                recovery.Amount,
                                0,
                                0,
                                recovery.ApprovedBy!);

                            repayment.Id = recovery.Id;
                            repayment.PaymentDate = clock.GetUtcNow();
                            await unitOfWork.LoanRepaymentRepository.CreateAsync(repayment, ct).ConfigureAwait(false);
                            await publisher.PublishAsync(
                                new GuarantorRecoveryCompletedIntegrationEvent(
                                    loan.TenantId,
                                    loan.Id,
                                    repayment.Id,
                                        repayment.TotalAmount,
                                    repayment.ReceiptNumber,
                                    recovery.ApprovedBy!)
                                { EventId = repayment.Id, OccurredAt = repayment.PaymentDate },
                                ct)
                                .ConfigureAwait(false);

                            if (loan.OutstandingPrincipal == 0 && loan.OutstandingInterest == 0)
                            {
                                loan.Status = LoanStatus.Closed;
                                await GuarantorReservationMapping.StageReleaseAsync(
                                    loan,
                                    unitOfWork,
                                    publisher,
                                    ICurrentActorProvider.SystemActor,
                                    clock.GetUtcNow(),
                                    ct,
                                    pledges)
                                    .ConfigureAwait(false);
                            }
                        }
                        await unitOfWork.GuarantorRecoveryRepository.UpdateAsync(recovery, ct).ConfigureAwait(false);
                    }
                }

                if (terminalReleases.Count > 0)
                {
                    await GuarantorReservationMapping.StageReleaseAsync(
                        loan,
                        unitOfWork,
                        publisher,
                        ICurrentActorProvider.SystemActor,
                        clock.GetUtcNow(),
                        ct,
                        terminalReleases)
                        .ConfigureAwait(false);
                }

                loan.UpdatedAt = clock.GetUtcNow();
                loan.UpdatedBy = ICurrentActorProvider.SystemActor;
                await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, ct).ConfigureAwait(false);
                return AppResponses.Success(true);
            },
            cancellationToken: ct);
}
