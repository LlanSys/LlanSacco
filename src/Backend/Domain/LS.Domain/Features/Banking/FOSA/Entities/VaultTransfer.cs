using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.FOSA.Entities;

public sealed class VaultTransfer : BaseEntity
{
    public Guid TellerTillId { get; init; }
    public Guid VaultId { get; init; }
    public decimal Amount { get; init; }
    public required string Denominations { get; init; }
    public required string Remarks { get; init; }
}
