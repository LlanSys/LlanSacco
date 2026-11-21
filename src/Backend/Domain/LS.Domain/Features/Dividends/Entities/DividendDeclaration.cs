using LS.Domain.Features.Dividends.Enums;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Dividends.Entities;

public class DividendDeclaration : BaseEntity
{
    public int FinancialYear { get; private set; }
    public decimal ShareDividendRate { get; private set; }
    public decimal DepositInterestRate { get; private set; }
    public decimal ShareWhtRate { get; private set; }
    public decimal DepositWhtRate { get; private set; }
    public DividendStatus Status { get; private set; }

    // Navigation property
    private readonly List<DividendCalculation> _calculations = new();
    public IReadOnlyCollection<DividendCalculation> Calculations => _calculations.AsReadOnly();

    private DividendDeclaration() { } // EF Core

    public static DividendDeclaration Create(
        Guid tenantId,
        int financialYear,
        decimal shareDividendRate,
        decimal depositInterestRate,
        decimal shareWhtRate = 0.05m,
        decimal depositWhtRate = 0.15m,
        string createdBy = "System")
    {
        return new DividendDeclaration
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            CreatedBy = createdBy,
            FinancialYear = financialYear,
            ShareDividendRate = shareDividendRate,
            DepositInterestRate = depositInterestRate,
            ShareWhtRate = shareWhtRate,
            DepositWhtRate = depositWhtRate,
            Status = DividendStatus.Draft
        };
    }

    public void UpdateRates(decimal shareDividendRate, decimal depositInterestRate, decimal shareWhtRate, decimal depositWhtRate)
    {
        if (Status != DividendStatus.Draft)
            throw new InvalidOperationException("Can only update rates when declaration is in Draft status.");

        ShareDividendRate = shareDividendRate;
        DepositInterestRate = depositInterestRate;
        ShareWhtRate = shareWhtRate;
        DepositWhtRate = depositWhtRate;
    }

    public void MarkAsCalculated()
    {
        if (Status != DividendStatus.Draft)
            throw new InvalidOperationException("Can only move to Calculated from Draft.");
        Status = DividendStatus.Calculated;
    }

    public void MarkAsApproved()
    {
        if (Status != DividendStatus.Calculated)
            throw new InvalidOperationException("Can only approve a calculated declaration.");
        Status = DividendStatus.Approved;
    }

    public void MarkAsCompleted()
    {
        if (Status != DividendStatus.Processing)
            throw new InvalidOperationException("Can only complete a processing declaration.");
        Status = DividendStatus.Completed;
    }
    
    public void StartProcessing()
    {
        if (Status != DividendStatus.Approved)
            throw new InvalidOperationException("Can only process an approved declaration.");
        Status = DividendStatus.Processing;
    }
}
