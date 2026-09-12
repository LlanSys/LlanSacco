using System;

namespace LS.SharedKernel.Dtos.Dividends;

public record DividendDeclarationDto
{
    public Guid Id { get; init; }
    public string FinancialYear { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal ShareDividendRate { get; init; }
    public decimal DepositInterestRate { get; init; }
    public decimal TotalCalculatedAmount { get; init; }
    public string? Notes { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
