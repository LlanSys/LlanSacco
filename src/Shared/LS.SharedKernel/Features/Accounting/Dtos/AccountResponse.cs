using System;
using System.Collections.Generic;

namespace LS.SharedKernel.Features.Accounting.Dtos;

public record AccountResponse
{
    public Guid Id { get; init; }
    public required string AccountCode { get; init; }
    public required string AccountName { get; init; }
    public required string AccountType { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

