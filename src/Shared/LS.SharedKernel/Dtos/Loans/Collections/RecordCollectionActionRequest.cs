using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Dtos.Loans.Collections;

public record RecordCollectionActionRequest(
    string ActionType,
    string Notes,
    DateTimeOffset? ActionDate = null
);

