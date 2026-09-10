using System;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.CheckOff.Entities;

public class CheckoffInstruction : BaseEntity
{
    public Guid MemberEmploymentId { get; set; }
    public decimal ExpectedSavingsAmount { get; set; }
    public decimal ExpectedSharesAmount { get; set; }
    public bool IsActive { get; set; }

    // Navigation
    public MemberEmployment MemberEmployment { get; set; } = null!;

    public static CheckoffInstruction Create(Guid tenantId, Guid memberEmploymentId, decimal expectedSavingsAmount, decimal expectedSharesAmount, string createdBy)
    {
        return new CheckoffInstruction
        {
            TenantId = tenantId,
            MemberEmploymentId = memberEmploymentId,
            ExpectedSavingsAmount = expectedSavingsAmount,
            ExpectedSharesAmount = expectedSharesAmount,
            IsActive = true,
            CreatedBy = createdBy
        };
    }
}
