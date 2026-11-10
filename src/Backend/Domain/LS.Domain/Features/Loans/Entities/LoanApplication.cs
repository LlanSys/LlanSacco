using LS.Domain.Shared.Entities;
using LS.Domain.Features.Loans.Enums;
using System;
using System.Collections.Generic;

namespace LS.Domain.Features.Loans.Entities;

public class LoanApplication : BaseEntity
{
    public required string ApplicationNumber { get; set; }
    public Guid MemberId { get; set; }
    public Guid LoanProductId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public int TermInMonths { get; set; }
    public decimal InterestRate { get; set; }
    public InterestMethod InterestMethod { get; set; }
    public LoanStatus Status { get; set; }
    public DateTimeOffset ApplicationDate { get; set; }
    public DateTimeOffset? DisbursementDate { get; set; }
    public decimal OutstandingPrincipal { get; set; }
    public decimal OutstandingInterest { get; set; }

    public LoanProduct Product { get; set; } = null!;
    public ICollection<LoanGuarantor> Guarantors { get; set; } = new List<LoanGuarantor>();
    public ICollection<LoanRepaymentSchedule> Schedules { get; set; } = new List<LoanRepaymentSchedule>();
    public ICollection<LoanRepayment> Repayments { get; set; } = new List<LoanRepayment>();

    public static LoanApplication Create(
        Guid tenantId,
        string applicationNumber,
        Guid memberId,
        Guid loanProductId,
        decimal principalAmount,
        int termInMonths,
        decimal interestRate,
        InterestMethod interestMethod,
        string createdBy)
    {
        return new LoanApplication
        {
            TenantId = tenantId,
            ApplicationNumber = applicationNumber,
            MemberId = memberId,
            LoanProductId = loanProductId,
            PrincipalAmount = principalAmount,
            TermInMonths = termInMonths,
            InterestRate = interestRate,
            InterestMethod = interestMethod,
            Status = LoanStatus.Draft,
            ApplicationDate = DateTimeOffset.UtcNow,
            OutstandingPrincipal = principalAmount,
            OutstandingInterest = 0m,
            CreatedBy = createdBy
        };
    }
}
