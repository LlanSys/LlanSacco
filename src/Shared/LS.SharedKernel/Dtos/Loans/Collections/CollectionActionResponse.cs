using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Dtos.Loans.Collections;

public record CollectionActionResponse(
    Guid Id,
    string ActionType,
    DateTimeOffset ActionDate,
    string Notes
);

