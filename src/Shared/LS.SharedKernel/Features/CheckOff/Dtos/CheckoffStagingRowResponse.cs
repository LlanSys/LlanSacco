using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.CheckOff.Dtos;

public record CheckoffStagingRowResponse(
    Guid Id,
    string EmployeePayrollNumber,
    string MemberName,
    decimal AmountForSavings,
    decimal AmountForShares,
    decimal AmountForLoans,
    Guid? ResolvedMemberId,
    string Status,
    string? ExceptionReason,
    List<CheckoffStagingLoanAllocationResponse> LoanAllocations
);

