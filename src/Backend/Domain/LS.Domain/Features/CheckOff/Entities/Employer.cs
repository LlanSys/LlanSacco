using System;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.CheckOff.Entities;

public class Employer : BaseEntity
{
    public required string Name { get; set; }
    public required string ContactPerson { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public bool IsActive { get; set; }

    public static Employer Create(Guid tenantId, string name, string contactPerson, string email, string phoneNumber, string createdBy)
    {
        return new Employer
        {
            TenantId = tenantId,
            Name = name,
            ContactPerson = contactPerson,
            Email = email,
            PhoneNumber = phoneNumber,
            IsActive = true,
            CreatedBy = createdBy
        };
    }
}
