using LS.Domain.Features.Banking.Deposits.Enums;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.Deposits.Entities;

public class DepositProduct : BaseEntity
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string? Description { get; set; }
    
    public DepositType Type { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public decimal MinimumDeposit { get; set; }
    public bool IsActive { get; set; } = true;
    
    public EarlyWithdrawalPenaltyStrategy PenaltyStrategy { get; set; }
    public decimal? FlatPenaltyRate { get; set; }
    public decimal? InterestForfeiturePercentage { get; set; }
    public decimal? ProRataReducedInterestRate { get; set; }
    
    public static DepositProduct Create(
        string name, 
        string code, 
        string? description,
        DepositType type,
        decimal interestRate, 
        int termMonths,
        decimal minimumDeposit,
        EarlyWithdrawalPenaltyStrategy penaltyStrategy,
        decimal? flatPenaltyRate,
        decimal? interestForfeiturePercentage,
        decimal? proRataReducedInterestRate,
        string createdBy)
    {
        return new DepositProduct
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Code = code,
            Description = description,
            Type = type,
            InterestRate = interestRate,
            TermMonths = termMonths,
            MinimumDeposit = minimumDeposit,
            PenaltyStrategy = penaltyStrategy,
            FlatPenaltyRate = flatPenaltyRate,
            InterestForfeiturePercentage = interestForfeiturePercentage,
            ProRataReducedInterestRate = proRataReducedInterestRate,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
