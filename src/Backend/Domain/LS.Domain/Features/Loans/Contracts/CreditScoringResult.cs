using System;
using System.Threading;
using System.Threading.Tasks;


namespace LS.Domain.Features.Loans.Contracts;

public class CreditScoringResult
{
    public bool IsApproved { get; set; }
    public string? Reason { get; set; }
    public decimal MaxAllowedAmount { get; set; }
}

