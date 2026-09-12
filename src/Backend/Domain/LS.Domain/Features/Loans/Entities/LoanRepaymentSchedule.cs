using LS.Domain.Shared.Entities;
using System;

namespace LS.Domain.Features.Loans.Entities;

public class LoanRepaymentSchedule : BaseEntity
{
    public Guid LoanApplicationId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public decimal PrincipalExpected { get; set; }
    public decimal InterestExpected { get; set; }
    public decimal TotalExpected => PrincipalExpected + InterestExpected;
    public decimal AmountPaid { get; set; }
    public bool IsPaid { get; set; }

    public LoanApplication Loan { get; set; } = null!;

    public static LoanRepaymentSchedule Create(
        Guid tenantId,
        Guid loanApplicationId,
        int installmentNumber,
        DateTimeOffset dueDate,
        decimal principalExpected,
        decimal interestExpected,
        string createdBy)
    {
        return new LoanRepaymentSchedule
        {
            TenantId = tenantId,
            LoanApplicationId = loanApplicationId,
            InstallmentNumber = installmentNumber,
            DueDate = dueDate,
            PrincipalExpected = principalExpected,
            InterestExpected = interestExpected,
            AmountPaid = 0m,
            IsPaid = false,
            CreatedBy = createdBy
        };
    }
}
