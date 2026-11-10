using System;
using System.Threading;
using System.Threading.Tasks;


namespace LS.Domain.Features.Loans.Contracts;

public class CreditAssessment
{
    public Guid MemberId { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal TotalSavings { get; set; }
    public decimal TotalShares { get; set; }
}

