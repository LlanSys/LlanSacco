using System;

namespace LS.SharedKernel.Features.Loans.Dtos;

public class UpdateLoanProductRequest
{
    public Guid Id { get; set; }
    public required string ProductName { get; set; }
    public string? Description { get; set; }
    public decimal InterestRate { get; set; }
    public required string InterestMethod { get; set; }
    public int MaxTermInMonths { get; set; }
    public decimal MaxAmount { get; set; }
    public bool IsActive { get; set; }
}



