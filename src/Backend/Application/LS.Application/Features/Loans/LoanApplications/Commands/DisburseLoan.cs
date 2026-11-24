using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.Loans.IntegrationEvents;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Guarantors.Events;
using MediatR;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

public sealed record DisburseLoanCommand(Guid LoanApplicationId, Guid? BranchId = null,
    Guid? CostCenterId = null, Guid? PaymentChannelGlAccountId = null) : IRequest<AppResponse<bool>>;

internal sealed class DisburseLoanCommandHandler(
    ILoansUnitOfWork unitOfWork, IEnumerable<IAmortizationStrategy> strategies,
    ICurrentActorProvider actor, IContextEventPublisher<ILoansUnitOfWork> publisher, TimeProvider clock)
    : IRequestHandler<DisburseLoanCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(DisburseLoanCommand request, CancellationToken ct) =>
        unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(request.LoanApplicationId, ct).ConfigureAwait(false);

                if (loan is null)
                {
                    return AppResponses.NotFound<bool>("Loan application not found.");
                }

                if (loan.Status is LoanStatus.Disbursed or LoanStatus.Active or LoanStatus.DisbursementPending)
                {
                    return AppResponses.Success("The disbursement is already recorded or awaiting security confirmation.", true);
                }

                if (loan.Status != LoanStatus.Approved)
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("Only an approved loan can be disbursed."));
                }

                if (!strategies.Any(x => x.SupportedMethod == loan.InterestMethod))
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("The loan interest method is not configured."));
                }

                var guarantors = await unitOfWork.LoanGuarantorRepository.ListAsync(q => q.Where(x => x.LoanApplicationId == loan.Id), ct).ConfigureAwait(false);
                var confirmed = guarantors.Where(x => x.Status == GuarantorStatus.ReservationHeld &&
                    x.ReservationExpiresAt > clock.GetUtcNow() && x.SavingsAccountId.HasValue)
                    .ToArray();

                if (confirmed.Length < 2 || confirmed.Select(x => x.GuarantorMemberId).Distinct().Count() < 2 ||
                    confirmed.Any(x => x.GuarantorMemberId == loan.MemberId) || confirmed.Sum(x => x.LockedAmount) < loan.PrincipalAmount)
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("At least two eligible guarantors and full confirmed principal coverage are required."));
                }

                loan.DisbursementBranchId = request.BranchId;
                loan.DisbursementRequestedBy = actor.ActorId;
                loan.DisbursementCostCenterId = request.CostCenterId;
                loan.DisbursementPaymentChannelGlAccountId = request.PaymentChannelGlAccountId;
                loan.Status = LoanStatus.DisbursementPending;
                loan.DisbursementOperationId = Guid.CreateVersion7();
                loan.SecurityFailure = null;
                loan.UpdatedAt = clock.GetUtcNow();
                loan.UpdatedBy = actor.ActorId;
                await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, ct).ConfigureAwait(false);
                await publisher.PublishAsync(
                    new GuarantorReservationInstruction(
                        loan.TenantId,
                        loan.DisbursementOperationId.Value,
                            loan.Id,
                        "Commit",
                        confirmed.Select(x => new GuarantorReservationItem(
                            x.Id,
                            x.GuarantorMemberId,
                                x.SavingsAccountId!.Value,
                            x.AcceptedAmount,
                            "KES",
                            x.ReservationPolicyVersion,
                                x.ReservationExpiresAt!.Value))
                            .ToArray(),
                        actor.ActorId,
                        loan.PrincipalAmount),
                    ct)
                    .ConfigureAwait(false);

                return AppResponses.Success("Disbursement is pending authoritative security confirmation.", true);
            },
            cancellationToken: ct);

    internal static async Task StageDisbursementAsync(
        LoanApplication loan,
        ILoansUnitOfWork unitOfWork,
        IEnumerable<IAmortizationStrategy> strategies,
        IContextEventPublisher<ILoansUnitOfWork> publisher,
        string actor,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var strategy = strategies.Single(x => x.SupportedMethod == loan.InterestMethod);
        loan.Status = LoanStatus.Disbursed;
        loan.DisbursementDate = now;
        loan.UpdatedBy = actor;
        loan.UpdatedAt = now;

        foreach (var schedule in strategy.GenerateSchedule(loan))
        {
            await unitOfWork.LoanRepaymentScheduleRepository.CreateAsync(schedule, ct).ConfigureAwait(false);
        }

        await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, ct).ConfigureAwait(false);
        await publisher.PublishAsync(
            new LoanDisbursedIntegrationEvent(
                loan.TenantId,
                loan.Id,
                loan.MemberId,
                loan.PrincipalAmount,
                0,
                loan.ApplicationNumber,
                actor,
                loan.DisbursementBranchId,
                loan.DisbursementCostCenterId,
                loan.DisbursementPaymentChannelGlAccountId)
            { EventId = loan.Id, OccurredAt = now },
            ct)
            .ConfigureAwait(false);
    }
}
