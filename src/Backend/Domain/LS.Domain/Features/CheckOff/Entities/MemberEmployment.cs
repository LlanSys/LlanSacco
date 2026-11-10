using System;
using LS.Domain.Shared.Entities;
using LS.Domain.Features.Membership.Entities;

namespace LS.Domain.Features.CheckOff.Entities;

public class MemberEmployment : BaseEntity
{
    public Guid MemberId { get; set; }
    public Guid EmployerId { get; set; }
    public required string EmployeePayrollNumber { get; set; }
    public bool IsActive { get; set; }

    // Navigation
    public Member Member { get; set; } = null!;
    public Employer Employer { get; set; } = null!;

    public static MemberEmployment Create(Guid tenantId, Guid memberId, Guid employerId, string employeePayrollNumber, string createdBy)
    {
        return new MemberEmployment
        {
            TenantId = tenantId,
            MemberId = memberId,
            EmployerId = employerId,
            EmployeePayrollNumber = employeePayrollNumber,
            IsActive = true,
            CreatedBy = createdBy
        };
    }
}
