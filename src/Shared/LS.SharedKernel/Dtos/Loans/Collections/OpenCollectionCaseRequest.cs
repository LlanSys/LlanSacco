using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Dtos.Loans.Collections;

public record OpenCollectionCaseRequest(
    Guid LoanApplicationId,
    decimal TotalArrearsAmount,
    int DaysPastDue,
    string TriggeredBy
);

