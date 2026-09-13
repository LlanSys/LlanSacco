using System.Linq;
using LS.Domain.Features.HR.Payroll.Entities;

namespace LS.Domain.Features.HR.Payroll.Services;

public class KenyaTaxCalculatorService : IKenyaTaxCalculatorService
{
    public (decimal Paye, decimal Nssf, decimal Shif, decimal HousingLevy, decimal NetPay) CalculateTaxes(
        decimal grossPay, 
        PayrollStatutoryConfiguration config)
    {
        // 1. Calculate NSSF (Tier I and Tier II limits)
        decimal nssf = 0m;
        if (grossPay > 0)
        {
            decimal tier1Amount = decimal.Min(grossPay, config.NssfTier1Limit);
            decimal tier2Amount = decimal.Max(0, decimal.Min(grossPay - config.NssfTier1Limit, config.NssfTier2Limit - config.NssfTier1Limit));
            nssf = (tier1Amount + tier2Amount) * (config.NssfRate / 100m);
        }

        // 2. Calculate SHIF / NHIF (2.75% of Gross)
        decimal shif = grossPay * (config.ShifRate / 100m);

        // 3. Calculate Housing Levy (1.5% of Gross)
        decimal housingLevy = grossPay * (config.HousingLevyRate / 100m);

        // 4. Calculate Taxable Pay (Gross - NSSF - SHIF allowable deductions, etc.)
        // Under current KRA rules, NSSF is an allowable deduction before PAYE. 
        // Housing Levy is NOT an allowable deduction. 
        // Note: SHIF might become an allowable deduction depending on exact implementation, but standard is just NSSF.
        decimal taxablePay = decimal.Max(0, grossPay - nssf);

        // 5. Calculate PAYE based on DB-driven bands
        decimal paye = 0m;
        decimal remainingTaxable = taxablePay;
        decimal previousUpperLimit = 0m;

        var bands = config.PayeTaxBands.OrderBy(b => b.LowerLimit).ToList();

        foreach (var band in bands)
        {
            if (remainingTaxable <= 0) break;

            decimal bandWidth = band.UpperLimit.HasValue 
                ? (band.UpperLimit.Value - previousUpperLimit) 
                : remainingTaxable; // If no upper limit, it taxes the rest

            decimal amountToTaxInBand = decimal.Min(remainingTaxable, bandWidth);
            
            paye += amountToTaxInBand * (band.Rate / 100m);
            remainingTaxable -= amountToTaxInBand;
            previousUpperLimit = band.UpperLimit ?? 0;
        }

        // Apply personal relief
        decimal finalPaye = decimal.Max(0, paye - config.PersonalReliefAmount);

        // 6. Calculate Net Pay
        decimal totalDeductions = finalPaye + nssf + shif + housingLevy;
        decimal netPay = decimal.Max(0, grossPay - totalDeductions);

        return (finalPaye, nssf, shif, housingLevy, netPay);
    }
}
