using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.GuarantorReservations.Entities;

public sealed class GuarantorReservationReceipt : BaseEntity, IRetainedFinancialRecord
{
    public Guid OperationId { get; init; }
    public Guid LoanId { get; init; }
    public required string Fingerprint { get; init; }
    public required string OutcomeJson { get; init; }
}
