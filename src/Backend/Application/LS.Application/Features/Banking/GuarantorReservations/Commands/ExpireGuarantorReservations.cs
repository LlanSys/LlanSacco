using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.GuarantorReservations.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Loans.Guarantors.Events;
using MediatR;

namespace LS.Application.Features.Banking.GuarantorReservations.Commands;

public sealed record ExpireGuarantorReservationsCommand : IRequest<AppResponse<int>>;

internal sealed class ExpireGuarantorReservationsCommandHandler(IBankingUnitOfWork unitOfWork,
    ICurrentTenantProvider tenant, ISender sender, TimeProvider clock)
    : IRequestHandler<ExpireGuarantorReservationsCommand, AppResponse<int>>
{
    public async Task<AppResponse<int>> Handle(ExpireGuarantorReservationsCommand request, CancellationToken ct)
    {
        var now = clock.GetUtcNow();
        var processed = 0;

        while (true)
        {
            var batch = await unitOfWork.GuarantorReservationRepository.ListAsync(
                q => q.Where(x =>
                    x.State == GuarantorReservationState.Held && x.ExpiresAt <= now)
                    .OrderBy(x => x.Id)
                    .Take(100),
                ct)
                .ConfigureAwait(false);

            if (batch.Count == 0)
            {
                return AppResponses.Success(processed);
            }

            foreach (var item in batch)
            {
                var instruction = new GuarantorReservationInstruction(
                    tenant.TenantId,
                    Guid.CreateVersion7(),
                    item.LoanId,
                    "Expire",
                    [new(item.PledgeId, item.MemberId, item.SavingsAccountId, item.Amount, item.Currency, item.PolicyVersion, item.ExpiresAt)],
                    ICurrentActorProvider.SystemActor);

                var result = await sender.Send(new ProcessGuarantorReservationCommand(instruction), ct).ConfigureAwait(false);

                if (!result.IsSuccess)
                {
                    return AppResponses.Failure<int>(result.Error!);
                }

                processed++;
            }
        }
    }
}
