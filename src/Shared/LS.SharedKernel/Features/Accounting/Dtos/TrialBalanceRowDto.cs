using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.Accounting.Dtos;

public record TrialBalanceRowDto
{
    public required string AccountCode { get; init; }
    public required string AccountName { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
}

