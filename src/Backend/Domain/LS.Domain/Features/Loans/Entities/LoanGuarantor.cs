using LS.Domain.Shared.Entities;
using LS.Domain.Features.Loans.Enums;
using System;

namespace LS.Domain.Features.Loans.Entities;

public class LoanGuarantor : BaseEntity
{
    public Guid LoanApplicationId { get; set; }
    public Guid GuarantorMemberId { get; set; }
    public decimal GuaranteedAmount { get; set; }
    public GuarantorStatus Status { get; set; }
    public decimal LockedAmount { get; set; }

    public LoanApplication Loan { get; set; } = null!;

    public static LoanGuarantor Create(
        Guid tenantId,
        Guid loanApplicationId,
        Guid guarantorMemberId,
        decimal guaranteedAmount,
        string createdBy)
    {
        return new LoanGuarantor
        {
            TenantId = tenantId,
            LoanApplicationId = loanApplicationId,
            GuarantorMemberId = guarantorMemberId,
            GuaranteedAmount = guaranteedAmount,
            Status = GuarantorStatus.Nominated,
            LockedAmount = 0m,
            CreatedBy = createdBy
        };
    }
}
