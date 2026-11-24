using LS.Domain.Features.Banking.GuarantorReservations.Contracts.Repositories;
using LS.Domain.Features.Banking.GuarantorReservations.Entities;
using LS.Persistence.Common;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.Banking.DataContext;

namespace LS.Persistence.Features.Banking.GuarantorReservations.Repositories;

internal sealed class GuarantorReservationRepository(BankingDBContext context)
    : Repository<GuarantorReservation>(context), IGuarantorReservationRepository
{
    public Task LockTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        if (tenantId == Guid.Empty || context.CurrentTenantId != tenantId)
        {
            throw new InvalidOperationException("Reservation serialization requires the owning tenant.");
        }
        // One short transaction per tenant also serializes missing rows and multi-pledge decisions.
        // Other tenants remain independent; balance writers retain optimistic row-version checks.
        return TransactionLock.AcquireAsync(context, $"banking-guarantors:{tenantId:D}", cancellationToken);
    }
}
