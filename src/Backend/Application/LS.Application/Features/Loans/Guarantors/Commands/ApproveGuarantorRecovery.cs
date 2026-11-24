using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Enums;
using LS.Domain.Features.Loans.Guarantors.Services;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Guarantors.Events;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Commands;

public sealed record ApproveGuarantorRecoveryCommand(Guid RecoveryId) : IRequest<AppResponse<bool>>;

internal sealed class ApproveGuarantorRecoveryCommandHandler(ILoansUnitOfWork unitOfWork,
    ICurrentActorProvider actor, IContextEventPublisher<ILoansUnitOfWork> publisher, TimeProvider clock)
    : IRequestHandler<ApproveGuarantorRecoveryCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(ApproveGuarantorRecoveryCommand request, CancellationToken ct) =>
        unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var recovery = await unitOfWork.GuarantorRecoveryRepository.FindByIdAsync(request.RecoveryId, ct).ConfigureAwait(false);

                if (recovery is null)
                {
                    return AppResponses.NotFound<bool>("Recovery request not found.");
                }

                if (string.Equals(recovery.CreatedBy, actor.ActorId, StringComparison.OrdinalIgnoreCase))
                {
                    return AppResponses.Failure<bool>(AppError.Forbidden("An independent recovery approver is required."));
                }

                if (recovery.Status is "Pending" or "Completed")
                {
                    return AppResponses.Success(true);
                }

                if (recovery.Status != "AwaitingApproval")
                {
                    return AppResponses.Conflict<bool>("This recovery request has a terminal decision.");
                }

                var loan = await unitOfWork.LoanApplicationRepository.FindByIdAsync(recovery.LoanId, ct).ConfigureAwait(false);

                if (loan is null || loan.Status != LoanStatus.Defaulted || loan.PendingSecurityReviewId.HasValue ||
                    recovery.Amount > loan.OutstandingPrincipal - loan.PendingRecoveryPrincipal)
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("The loan no longer has sufficient recoverable principal."));
                }

                var pledges = await unitOfWork.LoanGuarantorRepository.ListAsync(
                    q => q.Where(x => x.LoanApplicationId == loan.Id &&
                        x.Status == GuarantorStatus.Committed && x.LockedAmount > 0)
                        .OrderBy(x => x.Id),
                    ct)
                    .ConfigureAwait(false);

                var coverage = pledges.Sum(x => x.LockedAmount);

                if (coverage < recovery.Amount)
                {
                    return AppResponses.Failure<bool>(AppError.BusinessRule("Committed guarantor coverage is insufficient."));
                }

                var allocation = GuarantorRecoveryAllocator.Allocate(recovery.Amount, pledges.ToDictionary(x => x.Id, x => x.LockedAmount));
                var items = pledges.Where(x => allocation[x.Id] > 0)
                    .Select(x => GuarantorReservationMapping.ToItem(x, allocation[x.Id]))
                    .ToArray();

                recovery.ApprovedBy = actor.ActorId;
                recovery.Status = "Pending";
                recovery.UpdatedAt = clock.GetUtcNow();
                recovery.UpdatedBy = actor.ActorId;
                loan.PendingRecoveryPrincipal += recovery.Amount;
                loan.UpdatedAt = clock.GetUtcNow();
                loan.UpdatedBy = actor.ActorId;
                await unitOfWork.LoanApplicationRepository.UpdateAsync(loan, ct).ConfigureAwait(false);
                await unitOfWork.GuarantorRecoveryRepository.UpdateAsync(recovery, ct).ConfigureAwait(false);
                await publisher.PublishAsync(
                    new GuarantorReservationInstruction(
                        loan.TenantId,
                        recovery.Id,
                        loan.Id,
                            "Recover",
                        items,
                        recovery.CreatedBy,
                        ApprovedBy: actor.ActorId,
                        EvidenceReference: recovery.EvidenceReference),
                    ct)
                    .ConfigureAwait(false);

                return AppResponses.Success("Approved recovery is pending Banking confirmation.", true);
            },
            cancellationToken: ct);
}
