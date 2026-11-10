using LS.Domain.Shared.Entities;
using LS.Domain.Features.Membership.Enums;
using System;
using System.Collections.Generic;

namespace LS.Domain.Features.Membership.Entities;

public class Member : BaseEntity
{
    public required string MemberNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string IdentificationNumber { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public MemberStatus Status { get; set; }
    public KycStatus KycStatus { get; set; } = KycStatus.Pending;
    public MembershipType MembershipType { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public Guid? AppUserId { get; set; }

    // Navigation properties
    public MemberKycProfile KycProfile { get; set; } = null!;
    public ICollection<MemberAccount> Accounts { get; set; } = new List<MemberAccount>();
    public ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();

    public static Member Create(
        Guid tenantId,
        string memberNumber,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string identificationNumber,
        DateOnly dateOfBirth,
        MembershipType type,
        string createdBy)
    {
        return new Member
        {
            TenantId = tenantId,
            MemberNumber = memberNumber,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            IdentificationNumber = identificationNumber,
            DateOfBirth = dateOfBirth,
            Status = MemberStatus.PendingApproval,
            KycStatus = KycStatus.Pending,
            MembershipType = type,
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void Suspend(string actorId)
    {
        if (Status == MemberStatus.Closed || Status == MemberStatus.Rejected)
            throw new InvalidOperationException($"Cannot suspend member from status {Status}");
            
        Status = MemberStatus.Suspended;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Close(string actorId)
    {
        if (Status == MemberStatus.Closed || Status == MemberStatus.Rejected)
            throw new InvalidOperationException($"Cannot close member from status {Status}");
            
        Status = MemberStatus.Closed;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
