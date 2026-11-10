using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Dividends.Entities;

public class DividendCalculation : BaseEntity
{
    public Guid DeclarationId { get; private set; }
    public Guid MemberId { get; private set; }
    
    // Phase 1: Share Dividend
    public decimal WeightedShareBalance { get; private set; }
    public decimal GrossShareDividend { get; private set; }
    
    // Phase 2: Deposit Interest
    public decimal WeightedDepositBalance { get; private set; }
    public decimal GrossDepositInterest { get; private set; }
    
    // Phase 3: Taxation
    public decimal ShareWithholdingTax { get; private set; }
    public decimal DepositWithholdingTax { get; private set; }
    public decimal TotalWithholdingTax { get; private set; }
    
    // Phase 4: Net Payout
    public decimal NetPayout { get; private set; }
    
    // Distribution State
    public bool IsDistributed { get; private set; }
    public DateTimeOffset? DistributedAt { get; private set; }

    // Navigation property
    public DividendDeclaration Declaration { get; private set; } = null!;

    private DividendCalculation() { } // EF Core

    public static DividendCalculation Create(
        Guid tenantId,
        Guid declarationId,
        Guid memberId,
        decimal weightedShareBalance,
        decimal grossShareDividend,
        decimal weightedDepositBalance,
        decimal grossDepositInterest,
        decimal shareWithholdingTax,
        decimal depositWithholdingTax,
        string createdBy = "System")
    {
        return new DividendCalculation
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            CreatedBy = createdBy,
            DeclarationId = declarationId,
            MemberId = memberId,
            WeightedShareBalance = weightedShareBalance,
            GrossShareDividend = grossShareDividend,
            WeightedDepositBalance = weightedDepositBalance,
            GrossDepositInterest = grossDepositInterest,
            ShareWithholdingTax = shareWithholdingTax,
            DepositWithholdingTax = depositWithholdingTax,
            TotalWithholdingTax = shareWithholdingTax + depositWithholdingTax,
            NetPayout = (grossShareDividend + grossDepositInterest) - (shareWithholdingTax + depositWithholdingTax),
            IsDistributed = false
        };
    }

    public void MarkAsDistributed(DateTimeOffset distributedAt)
    {
        if (IsDistributed)
            throw new InvalidOperationException("This calculation has already been distributed.");

        IsDistributed = true;
        DistributedAt = distributedAt;
    }
}
