using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.Accounting.Dtos;

public record BalanceSheetDto
{
    public List<StatementRowDto> Assets { get; init; } = [];
    public decimal TotalAssets { get; init; }

    public List<StatementRowDto> Liabilities { get; init; } = [];
    public decimal TotalLiabilities { get; init; }

    public List<StatementRowDto> Equities { get; init; } = [];
    public decimal TotalEquity { get; init; }

    public decimal TotalLiabilitiesAndEquity => TotalLiabilities + TotalEquity;
    public bool IsBalanced => TotalAssets == TotalLiabilitiesAndEquity;
}
