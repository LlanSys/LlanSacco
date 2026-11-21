using LS.Domain.Features.Banking.FOSA.ValueObjects;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.FOSA.Entities;

public class TillBalancingRecord : BaseEntity
{
    public Guid TellerTillId { get; set; }
    public TellerTill TellerTill { get; set; } = null!;
    
    public decimal SystemBalance { get; set; }
    public decimal PhysicalCount { get; set; }
    public decimal Variance => PhysicalCount - SystemBalance;
    
    public DenominationBreakdown Breakdown { get; set; } = null!;
    
    public string Remarks { get; set; } = string.Empty;
}
