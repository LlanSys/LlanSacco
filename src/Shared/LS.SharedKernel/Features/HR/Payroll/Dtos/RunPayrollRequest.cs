using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.HR.Payroll.Dtos;

public record RunPayrollRequest(Guid PayrollPeriodId, string RunBy);
