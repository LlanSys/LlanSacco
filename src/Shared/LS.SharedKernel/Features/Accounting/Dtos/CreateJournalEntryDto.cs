using System;

namespace LS.SharedKernel.Features.Accounting.Dtos;

public record CreateJournalEntryDto
{
    public Guid AccountId { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public string? Description { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? CostCenterId { get; init; }
}