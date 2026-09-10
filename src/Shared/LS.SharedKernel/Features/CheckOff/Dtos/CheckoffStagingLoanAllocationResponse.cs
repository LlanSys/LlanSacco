using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.CheckOff.Dtos;

public record CheckoffStagingLoanAllocationResponse(
    Guid Id,
    Guid LoanId,
    decimal Amount
);

