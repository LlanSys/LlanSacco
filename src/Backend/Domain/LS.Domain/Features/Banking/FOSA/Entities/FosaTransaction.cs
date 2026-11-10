using LS.Domain.Features.Banking.FOSA.Enums;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.FOSA.Entities;

public class FosaTransaction : BaseEntity
{
    public Guid? FosaAccountId { get; set; }
    public FosaAccount? FosaAccount { get; set; }
    
    public Guid? TellerTillId { get; set; }
    public TellerTill? TellerTill { get; set; }

    public OtcTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Reference { get; set; } = null!;
    
    public static FosaTransaction Create(Guid? fosaAccountId, Guid? tellerTillId, OtcTransactionType type, decimal amount, string reference, string createdBy)
    {
        return new FosaTransaction
        {
            FosaAccountId = fosaAccountId,
            TellerTillId = tellerTillId,
            Type = type,
            Amount = amount,
            Reference = reference,
            CreatedBy = createdBy
        };
    }
}
