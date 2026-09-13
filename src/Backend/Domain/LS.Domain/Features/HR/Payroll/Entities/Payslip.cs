using System;
using System.Collections.Generic;
using System.Linq;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.HR.Payroll.Entities;

public class Payslip : BaseEntity
{
    public Guid PayrollPeriodId { get; private set; }
    public Guid EmployeeId { get; private set; }
    
    public decimal BasicSalary { get; private set; }
    public decimal TotalAllowances { get; private set; }
    public decimal GrossPay { get; private set; }
    
    public decimal PayeAmount { get; private set; }
    public decimal NssfAmount { get; private set; }
    public decimal ShifAmount { get; private set; }
    public decimal HousingLevyAmount { get; private set; }
    public decimal TotalDeductions { get; private set; }
    
    public decimal NetPay { get; private set; }

    private readonly List<PayslipDetail> _details = [];
    public IReadOnlyCollection<PayslipDetail> Details => _details.AsReadOnly();

    private Payslip() { }

    public static Payslip Create(
        Guid payrollPeriodId,
        Guid employeeId,
        decimal basicSalary,
        decimal totalAllowances,
        decimal grossPay,
        decimal payeAmount,
        decimal nssfAmount,
        decimal shifAmount,
        decimal housingLevyAmount,
        decimal totalDeductions,
        decimal netPay,
        string createdBy)
    {
        return new Payslip
        {
            Id = Guid.CreateVersion7(),
            PayrollPeriodId = payrollPeriodId,
            EmployeeId = employeeId,
            BasicSalary = basicSalary,
            TotalAllowances = totalAllowances,
            GrossPay = grossPay,
            PayeAmount = payeAmount,
            NssfAmount = nssfAmount,
            ShifAmount = shifAmount,
            HousingLevyAmount = housingLevyAmount,
            TotalDeductions = totalDeductions,
            NetPay = netPay,
            CreatedBy = createdBy
        };
    }

    public void AddDetail(string description, decimal amount, Enums.PayslipDetailType type)
    {
        _details.Add(PayslipDetail.Create(Id, description, amount, type, CreatedBy));
    }
}
