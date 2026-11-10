using System;

namespace LS.SharedKernel.Features.HR.Payroll.Dtos;

public record PayslipResponse(
    Guid Id,
    Guid PayrollPeriodId,
    Guid EmployeeId,
    string EmployeeName,
    decimal BasicSalary,
    decimal TotalAllowances,
    decimal GrossPay,
    decimal PayeAmount,
    decimal NssfAmount,
    decimal ShifAmount,
    decimal HousingLevyAmount,
    decimal TotalDeductions,
    decimal NetPay);

