using LS.Domain.Shared.Entities;
using System;

namespace LS.Domain.Features.Loans.Entities;

public class LoanRepayment : BaseEntity
{
    public Guid LoanApplicationId { get; set; }
    public required string ReceiptNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PrincipalComponent { get; set; }
    public decimal InterestComponent { get; set; }
    public decimal PenaltyComponent { get; set; }
    public DateTimeOffset PaymentDate { get; set; }

    public LoanApplication Loan { get; set; } = null!;

    public static LoanRepayment Create(
        Guid tenantId,
        Guid loanApplicationId,
        string receiptNumber,
        decimal totalAmount,
        decimal principalComponent,
        decimal interestComponent,
        decimal penaltyComponent,
        string createdBy)
    {
        return new LoanRepayment
        {
            TenantId = tenantId,
            LoanApplicationId = loanApplicationId,
            ReceiptNumber = receiptNumber,
            TotalAmount = totalAmount,
            PrincipalComponent = principalComponent,
            InterestComponent = interestComponent,
            PenaltyComponent = penaltyComponent,
            PaymentDate = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
