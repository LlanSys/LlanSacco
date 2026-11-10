using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.Savings.Entities;

public class SavingsProduct : BaseEntity
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string? Description { get; set; }
    
    public decimal InterestRate { get; set; }
    public decimal MinimumBalance { get; set; }
    
    public bool AllowsWithdrawals { get; set; } = true;
    public decimal WithdrawalFee { get; set; }
    
    public decimal? DailyWithdrawalLimit { get; set; }
    public decimal? MonthlyWithdrawalLimit { get; set; }
    
    public bool IsActive { get; set; }
    public bool IsDefaultCheckoffTarget { get; set; } = true;

    public static SavingsProduct Create(
        string name, 
        string code, 
        decimal interestRate, 
        decimal minimumBalance, 
        bool allowsWithdrawals,
        decimal withdrawalFee,
        decimal? dailyWithdrawalLimit,
        decimal? monthlyWithdrawalLimit,
        bool isDefaultCheckoffTarget,
        string createdBy)
    {
        return new SavingsProduct
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Code = code,
            InterestRate = interestRate,
            MinimumBalance = minimumBalance,
            AllowsWithdrawals = allowsWithdrawals,
            WithdrawalFee = withdrawalFee,
            DailyWithdrawalLimit = dailyWithdrawalLimit,
            MonthlyWithdrawalLimit = monthlyWithdrawalLimit,
            IsDefaultCheckoffTarget = isDefaultCheckoffTarget,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
