using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Dtos.Loans.Collections;

public record CollectionCaseResponse(
    Guid Id,
    Guid LoanApplicationId,
    Guid? AssignedOfficerUserId,
    decimal TotalArrearsAmount,
    int DaysPastDue,
    string Status,
    DateTimeOffset OpenedAt
);

