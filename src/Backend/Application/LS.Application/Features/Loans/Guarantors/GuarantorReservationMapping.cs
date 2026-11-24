using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using LS.SharedKernel.Features.Loans.Guarantors.Events;

namespace LS.Application.Features.Loans.Guarantors;

internal static class GuarantorReservationMapping
{
    internal static GuarantorReservationItem ToItem(LoanGuarantor pledge, decimal recoveryAmount = 0) =>
        new(
            pledge.Id,
            pledge.GuarantorMemberId,
            pledge.SavingsAccountId!.Value,
            pledge.AcceptedAmount,
            "KES",
            pledge.ReservationPolicyVersion,
            pledge.ReservationExpiresAt!.Value,
            recoveryAmount);

    internal static async Task StageReleaseAsync(
        LoanApplication loan,
        ILoansUnitOfWork unitOfWork,
        IContextEventPublisher<ILoansUnitOfWork> publisher,
        string actor,
        DateTimeOffset now,
        CancellationToken ct,
        IReadOnlyCollection<LoanGuarantor>? currentPledges = null)
    {
        var pledges = currentPledges is null
            ? await unitOfWork.LoanGuarantorRepository.ListAsync(
                q => q.Where(x => x.LoanApplicationId == loan.Id &&
                    x.SavingsAccountId != null && x.ReservationExpiresAt != null && x.AcceptedAmount > 0 &&
                    x.Status != GuarantorStatus.Released && x.Status != GuarantorStatus.Expired && x.Status != GuarantorStatus.Rejected),
                ct)
                .ConfigureAwait(false)
            : currentPledges.Where(x => x.SavingsAccountId.HasValue && x.ReservationExpiresAt.HasValue && x.AcceptedAmount > 0 &&
                x.Status is not (GuarantorStatus.Released or GuarantorStatus.Expired or GuarantorStatus.Rejected))
                .ToList();

        if (pledges.Count == 0)
        {
            return;
        }

        foreach (var pledge in pledges)
        {
            pledge.Status = GuarantorStatus.ReleasePending;
            pledge.UpdatedBy = actor;
            pledge.UpdatedAt = now;
            await unitOfWork.LoanGuarantorRepository.UpdateAsync(pledge, ct).ConfigureAwait(false);
        }
        await publisher.PublishAsync(
            new GuarantorReservationInstruction(
                loan.TenantId,
                Guid.CreateVersion7(),
                loan.Id,
                "Release",
                pledges.Select(x => ToItem(x)).ToArray(),
                actor,
                ReleaseCommitted: loan.Status == LoanStatus.Closed),
            ct)
            .ConfigureAwait(false);
    }
}
