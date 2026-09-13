using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Dtos.Loans.Collections;

public record RecordPromiseToPayRequest(
    DateTimeOffset PromiseDate,
    decimal PromiseAmount
);

