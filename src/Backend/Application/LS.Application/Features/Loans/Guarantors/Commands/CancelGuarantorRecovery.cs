using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.Loans.Guarantors.Commands;

public sealed record CancelGuarantorRecoveryCommand(Guid RecoveryId) : IRequest<AppResponse<bool>>;

internal sealed class CancelGuarantorRecoveryCommandHandler(ILoansUnitOfWork unitOfWork,
    ICurrentActorProvider actor, TimeProvider clock) : IRequestHandler<CancelGuarantorRecoveryCommand, AppResponse<bool>>
{
    public Task<AppResponse<bool>> Handle(CancelGuarantorRecoveryCommand request, CancellationToken ct) =>
        unitOfWork.ExecuteInTransactionWithRetryAsync(
            async () =>
            {
                var recovery = await unitOfWork.GuarantorRecoveryRepository.FindByIdAsync(request.RecoveryId, ct).ConfigureAwait(false);

                if (recovery is null)
                {
                    return AppResponses.NotFound<bool>("Recovery request not found.");
                }

                if (recovery.Status == "Cancelled")
                {
                    return AppResponses.Success(true);
                }

                if (recovery.Status != "AwaitingApproval")
                {
                    return AppResponses.Conflict<bool>("A recovery sent to Banking must be reconciled before further action.");
                }

                recovery.Status = "Cancelled";
                recovery.UpdatedBy = actor.ActorId;
                recovery.UpdatedAt = clock.GetUtcNow();
                await unitOfWork.GuarantorRecoveryRepository.UpdateAsync(recovery, ct).ConfigureAwait(false);
                return AppResponses.Success(true);
            },
            cancellationToken: ct);
}
