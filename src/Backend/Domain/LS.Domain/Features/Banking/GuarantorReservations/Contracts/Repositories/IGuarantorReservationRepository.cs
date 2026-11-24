using LS.Domain.Features.Banking.GuarantorReservations.Entities;
using LS.Domain.Shared.Contracts.Repositories;

namespace LS.Domain.Features.Banking.GuarantorReservations.Contracts.Repositories;

public interface IGuarantorReservationRepository : IRepository<GuarantorReservation>
{
    Task LockTenantAsync(Guid tenantId, CancellationToken cancellationToken);
}
