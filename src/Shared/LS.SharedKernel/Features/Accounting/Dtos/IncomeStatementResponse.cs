using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.Accounting.Dtos;

public record IncomeStatementResponse
{
    public List<StatementRowResponse> Incomes { get; init; } = [];
    public decimal TotalIncome { get; init; }
    
    public List<StatementRowResponse> Expenses { get; init; } = [];
    public decimal TotalExpense { get; init; }

    public decimal NetSurplus => TotalIncome - TotalExpense;
}

