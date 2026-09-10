using System;

namespace LS.SharedKernel.Dtos.Dividends;

public record DividendPreferenceDto
{
    public Guid MemberId { get; init; }
    public decimal CapitalizePercentage { get; init; }
    public decimal FosaPercentage { get; init; }
    public decimal ExternalBankPercentage { get; init; }
}
