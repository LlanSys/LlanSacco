using System;
using LS.Domain.Features.HR.Payroll.Entities;
using LS.Domain.Features.HR.Payroll.Services;
using Xunit;

namespace LS.Tests.Unit.Domain.Features.HR.Payroll.Services;

public class KenyaTaxCalculatorServiceTests
{
    private readonly KenyaTaxCalculatorService _sut;
    private readonly PayrollStatutoryConfiguration _config;
    private readonly string _userId = "test-user";

    public KenyaTaxCalculatorServiceTests()
    {
        _sut = new KenyaTaxCalculatorService();

        // Standard KRA 2024 configurations
        _config = PayrollStatutoryConfiguration.Create(
            effectiveDate: DateTimeOffset.UtcNow.AddDays(-1),
            nssfTier1Limit: 7000,
            nssfTier2Limit: 36000,
            nssfRate: 6m,
            shifRate: 2.75m,
            housingLevyRate: 1.5m,
            personalReliefAmount: 2400,
            createdBy: _userId);

        _config.AddOrUpdatePayeTaxBand(0, 24000, 10);
        _config.AddOrUpdatePayeTaxBand(24001, 32333, 25);
        _config.AddOrUpdatePayeTaxBand(32334, 500000, 30);
        _config.AddOrUpdatePayeTaxBand(500001, 800000, 32.5m);
        _config.AddOrUpdatePayeTaxBand(800001, null, 35);
    }

    [Theory]
    // 50,000 gross
    // NSSF: (7000 * 6%) + (29000 * 6%) = 420 + 1740 = 2160
    // SHIF: 50000 * 2.75% = 1375
    // Housing Levy: 50000 * 1.5% = 750
    // Taxable Pay: 50000 - 2160 = 47840
    // PAYE: 
    //  24000 * 10% = 2400
    //  8333 * 25% = 2083.25
    //  15507 * 30% = 4652.1
    //  Total = 9135.35 - 2400 (Relief) = 6735.35
    // Net Pay = 50000 - (6735.35 + 2160 + 1375 + 750) = 38979.65
    [InlineData(50000, 6735.35, 2160, 1375, 750, 38979.65)]
    public void CalculateTaxes_ShouldReturnCorrectValues(
        decimal grossPay, 
        decimal expectedPaye, 
        decimal expectedNssf, 
        decimal expectedShif, 
        decimal expectedHousingLevy, 
        decimal expectedNetPay)
    {
        var (paye, nssf, shif, housingLevy, netPay) = _sut.CalculateTaxes(grossPay, _config);

        Assert.Equal(expectedNssf, nssf);
        Assert.Equal(expectedShif, shif);
        Assert.Equal(expectedHousingLevy, housingLevy);
        Assert.Equal(expectedPaye, paye);
        Assert.Equal(expectedNetPay, netPay);
    }
}
