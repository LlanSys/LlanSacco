using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.CheckOff.Dtos;

public class CheckoffRowRequest
{
    public string EmployeePayrollNumber { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}
