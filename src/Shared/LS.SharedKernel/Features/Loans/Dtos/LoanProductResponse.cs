using System;

namespace LS.SharedKernel.Features.Loans.Dtos;

public class LoanProductResponse
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal InterestRate { get; set; }
    public required string InterestMethod { get; set; }
    public int MaxTermInMonths { get; set; }
    public decimal MaxAmount { get; set; }
    public bool IsActive { get; set; }
}




