using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.Accounting.Dtos;

public record StatementRowDto
{
    public required string AccountCode { get; init; }
    public required string AccountName { get; init; }
    public decimal Balance { get; init; }
}

