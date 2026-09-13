using LS.Domain.Features.HR.Payroll.Entities;

namespace LS.Domain.Features.HR.Payroll.Services;

public interface IKenyaTaxCalculatorService
{
    (decimal Paye, decimal Nssf, decimal Shif, decimal HousingLevy, decimal NetPay) CalculateTaxes(
        decimal grossPay, 
        PayrollStatutoryConfiguration config);
}
