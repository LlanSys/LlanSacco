using System;
using System.Collections.Generic;


namespace LS.SharedKernel.Features.Accounting.Dtos;

public record IncomeStatementDto
{
    public List<StatementRowDto> Incomes { get; init; } = [];
    public decimal TotalIncome { get; init; }
    
    public List<StatementRowDto> Expenses { get; init; } = [];
    public decimal TotalExpense { get; init; }

    public decimal NetSurplus => TotalIncome - TotalExpense;
}

