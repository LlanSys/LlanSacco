using LS.Domain.Features.Banking.Deposits.Enums;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.Deposits.Entities;

public class DepositTransaction : BaseEntity
{
    public Guid DepositAccountId { get; set; }
    public DepositAccount Account { get; set; } = null!;
    
    public DepositTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    
    public string? Reference { get; set; }

    public static DepositTransaction Create(
        Guid depositAccountId, 
        DepositTransactionType type, 
        decimal amount, 
        decimal balanceAfter, 
        string? reference,
        string createdBy)
    {
        return new DepositTransaction
        {
            Id = Guid.CreateVersion7(),
            DepositAccountId = depositAccountId,
            Type = type,
            Amount = amount,
            BalanceAfter = balanceAfter,
            Reference = reference,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
