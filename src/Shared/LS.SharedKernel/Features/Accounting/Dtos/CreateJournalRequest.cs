using System;
using System.Collections.Generic;

namespace LS.SharedKernel.Features.Accounting.Dtos;

public record CreateJournalRequest
{
    public required string ReferenceNumber { get; init; }
    public required string Description { get; init; }
    public DateTimeOffset TransactionDate { get; init; } = DateTimeOffset.UtcNow;
    public List<CreateJournalEntryDto> Entries { get; init; } = new();
}

