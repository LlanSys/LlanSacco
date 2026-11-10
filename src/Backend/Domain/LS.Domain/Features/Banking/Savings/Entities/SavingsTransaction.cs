using LS.Domain.Features.Banking.Savings.Enums;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Banking.Savings.Entities;

public class SavingsTransaction : BaseEntity
{
    public Guid SavingsAccountId { get; set; }
    
    public SavingsTransactionType Type { get; set; }
    
    public decimal Amount { get; set; }
    
    public string? Notes { get; set; }
    
    public string? ExternalReferenceId { get; set; }
    
    public static SavingsTransaction Create(
        Guid savingsAccountId, 
        SavingsTransactionType type, 
        decimal amount, 
        string? notes, 
        string? externalReferenceId,
        string createdBy)
    {
        return new SavingsTransaction
        {
            Id = Guid.CreateVersion7(),
            SavingsAccountId = savingsAccountId,
            Type = type,
            Amount = amount,
            Notes = notes,
            ExternalReferenceId = externalReferenceId,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
