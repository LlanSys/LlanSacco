using LS.Domain.Shared.Entities;
using LS.Domain.Features.Membership.Enums;
using System;

namespace LS.Domain.Features.Membership.Entities;

public class MemberAccount : BaseEntity
{
    public Guid MemberId { get; set; }
    public required string AccountNumber { get; set; }
    public MemberAccountType AccountType { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }

    public Member Member { get; set; } = null!;

    public static MemberAccount Create(
        Guid tenantId,
        Guid memberId,
        string accountNumber,
        MemberAccountType accountType,
        string createdBy)
    {
        return new MemberAccount
        {
            TenantId = tenantId,
            MemberId = memberId,
            AccountNumber = accountNumber,
            AccountType = accountType,
            CurrentBalance = 0m,
            IsActive = true,
            CreatedBy = createdBy
        };
    }
}
