using System;
using System.Collections.Generic;
using System.Linq;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.HR.Payroll.Entities;

public class PayrollStatutoryConfiguration : BaseEntity
{
    public DateTimeOffset EffectiveDate { get; private set; }
    
    // NSSF
    public decimal NssfTier1Limit { get; private set; }
    public decimal NssfTier2Limit { get; private set; }
    public decimal NssfRate { get; private set; }
    
    // SHIF / NHIF
    public decimal ShifRate { get; private set; }
    
    // Housing Levy
    public decimal HousingLevyRate { get; private set; }
    
    // PAYE Personal Relief
    public decimal PersonalReliefAmount { get; private set; }

    private readonly List<PayeTaxBand> _payeTaxBands = [];
    public IReadOnlyCollection<PayeTaxBand> PayeTaxBands => _payeTaxBands.AsReadOnly();

    private PayrollStatutoryConfiguration() { }

    public static PayrollStatutoryConfiguration Create(
        DateTimeOffset effectiveDate,
        decimal nssfTier1Limit,
        decimal nssfTier2Limit,
        decimal nssfRate,
        decimal shifRate,
        decimal housingLevyRate,
        decimal personalReliefAmount,
        string createdBy)
    {
        return new PayrollStatutoryConfiguration
        {
            Id = Guid.CreateVersion7(),
            EffectiveDate = effectiveDate,
            NssfTier1Limit = nssfTier1Limit,
            NssfTier2Limit = nssfTier2Limit,
            NssfRate = nssfRate,
            ShifRate = shifRate,
            HousingLevyRate = housingLevyRate,
            PersonalReliefAmount = personalReliefAmount,
            CreatedBy = createdBy
        };
    }

    public void Update(
        DateTimeOffset effectiveDate,
        decimal nssfTier1Limit,
        decimal nssfTier2Limit,
        decimal nssfRate,
        decimal shifRate,
        decimal housingLevyRate,
        decimal personalReliefAmount,
        string updatedBy)
    {
        EffectiveDate = effectiveDate;
        NssfTier1Limit = nssfTier1Limit;
        NssfTier2Limit = nssfTier2Limit;
        NssfRate = nssfRate;
        ShifRate = shifRate;
        HousingLevyRate = housingLevyRate;
        PersonalReliefAmount = personalReliefAmount;
        SetUpdatedInfo(updatedBy);
    }

    public void AddOrUpdatePayeTaxBand(decimal lowerLimit, decimal? upperLimit, decimal rate)
    {
        var existing = _payeTaxBands.FirstOrDefault(x => x.LowerLimit == lowerLimit);
        if (existing != null)
        {
            existing.Update(upperLimit, rate);
        }
        else
        {
            _payeTaxBands.Add(PayeTaxBand.Create(Id, lowerLimit, upperLimit, rate, CreatedBy));
        }
    }

    public void RemovePayeTaxBand(Guid bandId)
    {
        var existing = _payeTaxBands.FirstOrDefault(x => x.Id == bandId);
        if (existing != null)
        {
            _payeTaxBands.Remove(existing);
        }
    }
}
