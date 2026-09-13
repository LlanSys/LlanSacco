using LS.Domain.Shared.Entities;
using LS.Domain.Features.Membership.Enums;
using System;

namespace LS.Domain.Features.Membership.Entities;

public class MemberTransaction : BaseEntity
{
    public Guid MemberId { get; set; }
    public Guid MemberAccountId { get; set; }
    public decimal Amount { get; set; }
    public MemberTransactionType TransactionType { get; set; }
    public string Reference { get; set; } = null!;
    public DateTimeOffset TransactionDate { get; set; }
    public string? Description { get; set; }

    public Member Member { get; set; } = null!;
    public MemberAccount Account { get; set; } = null!;

    public static MemberTransaction Create(
        Guid tenantId,
        Guid memberId,
        Guid memberAccountId,
        decimal amount,
        MemberTransactionType transactionType,
        string reference,
        string? description,
        string createdBy)
    {
        return new MemberTransaction
        {
            TenantId = tenantId,
            MemberId = memberId,
            MemberAccountId = memberAccountId,
            Amount = amount,
            TransactionType = transactionType,
            Reference = reference,
            Description = description,
            TransactionDate = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
