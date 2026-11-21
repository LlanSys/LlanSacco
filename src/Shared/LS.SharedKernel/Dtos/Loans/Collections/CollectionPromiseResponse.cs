using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Dtos.Loans.Collections;

public record CollectionPromiseResponse(
    Guid Id,
    DateTimeOffset PromiseDate,
    decimal PromiseAmount,
    string Status
);

