using LS.Domain.Shared.Entities;
using System;

namespace LS.Domain.Features.Membership.Entities;

public class MemberKycProfile : BaseEntity
{
    public Guid MemberId { get; set; }
    public bool IprsVerified { get; set; }
    public bool KraVerified { get; set; }
    public bool CrbChecked { get; set; }
    public bool AmlCleared { get; set; }
    public DateTimeOffset? LastVerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }

    public Member Member { get; set; } = null!;

    public static MemberKycProfile Create(Guid tenantId, Guid memberId, string createdBy)
    {
        return new MemberKycProfile
        {
            TenantId = tenantId,
            MemberId = memberId,
            IprsVerified = false,
            KraVerified = false,
            CrbChecked = false,
            AmlCleared = false,
            CreatedBy = createdBy
        };
    }

    public void MarkIprsVerified(string updatedBy)
    {
        IprsVerified = true;
        LastVerifiedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkKraVerified(string updatedBy)
    {
        KraVerified = true;
        LastVerifiedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkCrbChecked(string updatedBy)
    {
        CrbChecked = true;
        LastVerifiedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAmlCleared(string updatedBy)
    {
        AmlCleared = true;
        LastVerifiedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
