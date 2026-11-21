using LS.Domain.Shared.Entities;
using LS.Domain.Features.Membership.Enums;
using System;

namespace LS.Domain.Features.Membership.Entities;

public class GuarantorRequest : BaseEntity
{
    public Guid MemberId { get; set; }
    public Guid NominatedGuarantorMemberId { get; set; }
    public decimal AmountToGuarantee { get; set; }
    public GuarantorRequestStatus Status { get; set; }
    public DateTimeOffset RequestDate { get; set; }
    public DateTimeOffset? ResponseDate { get; set; }
    public string? ResponseNote { get; set; }

    public Member Member { get; set; } = null!;
    public Member NominatedGuarantor { get; set; } = null!;

    public static GuarantorRequest Create(
        Guid tenantId,
        Guid memberId,
        Guid nominatedGuarantorMemberId,
        decimal amountToGuarantee,
        string createdBy)
    {
        return new GuarantorRequest
        {
            TenantId = tenantId,
            MemberId = memberId,
            NominatedGuarantorMemberId = nominatedGuarantorMemberId,
            AmountToGuarantee = amountToGuarantee,
            Status = GuarantorRequestStatus.Pending,
            RequestDate = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void Accept(string responseNote, string updatedBy)
    {
        Status = GuarantorRequestStatus.Accepted;
        ResponseNote = responseNote;
        ResponseDate = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(string responseNote, string updatedBy)
    {
        Status = GuarantorRequestStatus.Rejected;
        ResponseNote = responseNote;
        ResponseDate = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
