using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.Accounting.Dtos;

public record BalanceSheetResponse
{
    public List<StatementRowResponse> Assets { get; init; } = [];
    public decimal TotalAssets { get; init; }

    public List<StatementRowResponse> Liabilities { get; init; } = [];
    public decimal TotalLiabilities { get; init; }

    public List<StatementRowResponse> Equities { get; init; } = [];
    public decimal TotalEquity { get; init; }

    public decimal TotalLiabilitiesAndEquity => TotalLiabilities + TotalEquity;
    public bool IsBalanced => TotalAssets == TotalLiabilitiesAndEquity;
}
