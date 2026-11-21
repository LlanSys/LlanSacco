using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.FOSA.Entities;

public class FosaAccount : BaseEntity
{
    public Guid MemberId { get; set; }
    public string AccountNumber { get; set; } = null!;
    public decimal Balance { get; set; }
    public decimal OverdraftLimit { get; set; }
    public bool IsActive { get; set; } = true;
    
    public static FosaAccount Create(Guid memberId, string accountNumber, string createdBy)
    {
        return new FosaAccount
        {
            MemberId = memberId,
            AccountNumber = accountNumber,
            Balance = 0,
            OverdraftLimit = 0,
            IsActive = true,
            CreatedBy = createdBy
        };
    }
}
