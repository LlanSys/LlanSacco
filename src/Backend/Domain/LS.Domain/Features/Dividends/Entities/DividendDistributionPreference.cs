using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.Dividends.Entities;

public class DividendDistributionPreference : BaseEntity
{
    public Guid MemberId { get; private set; }
    
    // Percentages should add up to 100
    public decimal CapitalizePercentage { get; private set; }
    public decimal FosaPercentage { get; private set; }
    public decimal ExternalBankPercentage { get; private set; }

    private DividendDistributionPreference() { } // EF Core

    public static DividendDistributionPreference Create(
        Guid tenantId,
        Guid memberId,
        decimal capitalizePercentage,
        decimal fosaPercentage,
        decimal externalBankPercentage,
        string createdBy)
    {
        var pref = new DividendDistributionPreference
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            CreatedBy = createdBy,
            MemberId = memberId
        };
        
        pref.UpdatePreferences(capitalizePercentage, fosaPercentage, externalBankPercentage);
        return pref;
    }

    public void UpdatePreferences(decimal capitalizePercentage, decimal fosaPercentage, decimal externalBankPercentage)
    {
        if (capitalizePercentage < 0 || fosaPercentage < 0 || externalBankPercentage < 0)
            throw new ArgumentException("Percentages cannot be negative.");

        if (capitalizePercentage + fosaPercentage + externalBankPercentage != 100m)
            throw new ArgumentException("Total distribution percentages must equal 100.");

        CapitalizePercentage = capitalizePercentage;
        FosaPercentage = fosaPercentage;
        ExternalBankPercentage = externalBankPercentage;
    }
}
