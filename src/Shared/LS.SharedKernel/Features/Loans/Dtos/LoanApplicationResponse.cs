using System;
using System.Collections.Generic;

namespace LS.SharedKernel.Features.Loans.Dtos;

public record LoanApplicationResponse
{
    public Guid Id { get; init; }
    public string ApplicationNumber { get; init; } = string.Empty;
    public Guid MemberId { get; init; }
    public Guid LoanProductId { get; init; }
    public decimal PrincipalAmount { get; init; }
    public int TermInMonths { get; init; }
    public decimal InterestRate { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset ApplicationDate { get; init; }
    public decimal OutstandingPrincipal { get; init; }
    public decimal OutstandingInterest { get; init; }
    public List<LoanGuarantorDto> Guarantors { get; init; } = new();
}


