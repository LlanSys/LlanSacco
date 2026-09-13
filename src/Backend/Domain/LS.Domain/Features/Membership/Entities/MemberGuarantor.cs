using LS.Domain.Shared.Entities;
using System;

namespace LS.Domain.Features.Membership.Entities;

public class MemberGuarantor : BaseEntity
{
    public Guid MemberId { get; set; }
    public Guid GuarantorMemberId { get; set; }
    public Guid? LoanApplicationId { get; set; }
    public decimal GuaranteedAmount { get; set; }

    public Member Member { get; set; } = null!;
    public Member Guarantor { get; set; } = null!;

    public static MemberGuarantor Create(
        Guid tenantId,
        Guid memberId,
        Guid guarantorMemberId,
        Guid? loanApplicationId,
        decimal guaranteedAmount,
        string createdBy)
    {
        return new MemberGuarantor
        {
            TenantId = tenantId,
            MemberId = memberId,
            GuarantorMemberId = guarantorMemberId,
            LoanApplicationId = loanApplicationId,
            GuaranteedAmount = guaranteedAmount,
            CreatedBy = createdBy
        };
    }
}
