using System;

namespace LS.SharedKernel.Features.Dividends.Dtos;

public record DividendPreferenceResponse
{
    public Guid MemberId { get; init; }
    public decimal CapitalizePercentage { get; init; }
    public decimal FosaPercentage { get; init; }
    public decimal ExternalBankPercentage { get; init; }
}
