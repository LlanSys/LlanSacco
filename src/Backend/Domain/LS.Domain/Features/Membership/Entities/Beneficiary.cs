using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Entities;
using System;

namespace LS.Domain.Features.Membership.Entities;

public class Beneficiary : BaseEntity, ISoftDeletable
{
    public Guid MemberId { get; set; }
    public required string FullName { get; set; }
    public required string Relationship { get; set; }
    public required string PhoneNumber { get; set; }
    public required string IdentificationNumber { get; set; }
    public decimal AllocationPercentage { get; set; }
    public bool IsNextOfKin { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Member Member { get; set; } = null!;

    public void MarkAsDeleted(string deletedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deletedBy);

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    public static Beneficiary Create(
        Guid tenantId,
        Guid memberId,
        string fullName,
        string relationship,
        string phoneNumber,
        string identificationNumber,
        decimal allocationPercentage,
        bool isNextOfKin,
        string createdBy)
    {
        return new Beneficiary
        {
            TenantId = tenantId,
            MemberId = memberId,
            FullName = fullName,
            Relationship = relationship,
            PhoneNumber = phoneNumber,
            IdentificationNumber = identificationNumber,
            AllocationPercentage = allocationPercentage,
            IsNextOfKin = isNextOfKin,
            CreatedBy = createdBy
        };
    }
}
