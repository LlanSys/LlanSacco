using System;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.HR.Payroll.Entities;

public class PayeTaxBand : BaseEntity
{
    public Guid ConfigurationId { get; private set; }
    public decimal LowerLimit { get; private set; }
    public decimal? UpperLimit { get; private set; } // Null if it's the top band (e.g., above 800k)
    public decimal Rate { get; private set; } // Percentage rate, e.g., 10, 25, 30

    private PayeTaxBand() { }

    internal static PayeTaxBand Create(Guid configurationId, decimal lowerLimit, decimal? upperLimit, decimal rate, string createdBy)
    {
        return new PayeTaxBand
        {
            Id = Guid.CreateVersion7(),
            ConfigurationId = configurationId,
            LowerLimit = lowerLimit,
            UpperLimit = upperLimit,
            Rate = rate,
            CreatedBy = createdBy
        };
    }

    internal void Update(decimal? upperLimit, decimal rate)
    {
        UpperLimit = upperLimit;
        Rate = rate;
    }
}
