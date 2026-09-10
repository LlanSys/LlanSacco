using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.Savings.Entities;

public class SavingsAccount : BaseEntity
{
    public Guid MemberId { get; set; }
    
    public Guid SavingsProductId { get; set; }
    public SavingsProduct Product { get; set; } = null!;
    
    public decimal Balance { get; set; }
    public decimal LockedFunds { get; set; }
    
    public bool IsActive { get; set; } = true;

    public static SavingsAccount Create(Guid memberId, Guid savingsProductId, string createdBy)
    {
        return new SavingsAccount
        {
            Id = Guid.CreateVersion7(),
            MemberId = memberId,
            SavingsProductId = savingsProductId,
            Balance = 0,
            LockedFunds = 0,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public decimal GetAvailableBalance(decimal minimumBalance)
    {
        return Balance - minimumBalance - LockedFunds;
    }
}
