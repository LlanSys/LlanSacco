using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.HR.Payroll.Dtos;

public record GetPayrollPeriodsRequest(int Year, int Month);
