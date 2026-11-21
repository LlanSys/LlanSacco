using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.Deposits.Entities;

public class DepositAccount : BaseEntity
{
    public Guid MemberId { get; set; }
    
    public Guid DepositProductId { get; set; }
    public DepositProduct Product { get; set; } = null!;
    
    public decimal Balance { get; set; }
    public DateOnly? LastInterestAccruedOn { get; set; }
    public decimal AccruedInterest { get; set; }
    
    public DateTimeOffset MaturityDate { get; set; }
    public bool IsActive { get; set; } = true;

    public static DepositAccount Create(Guid memberId, Guid depositProductId, DateTimeOffset maturityDate, string createdBy)
    {
        return new DepositAccount
        {
            Id = Guid.CreateVersion7(),
            MemberId = memberId,
            DepositProductId = depositProductId,
            Balance = 0,
            AccruedInterest = 0,
            MaturityDate = maturityDate,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
