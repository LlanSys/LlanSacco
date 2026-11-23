using System.Collections.Generic;

namespace LS.SharedKernel.Features.Accounting.Dtos;

public record TrialBalanceResponse
{
    public List<TrialBalanceRowResponse> Rows { get; init; } = new();
    public decimal TotalDebit { get; init; }
    public decimal TotalCredit { get; init; }
}
