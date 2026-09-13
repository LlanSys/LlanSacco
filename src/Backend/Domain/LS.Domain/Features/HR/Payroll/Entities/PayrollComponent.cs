using System;
using LS.Domain.Features.HR.Payroll.Enums;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.HR.Payroll.Entities;

public class PayrollComponent : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public PayrollComponentType Type { get; private set; }
    public bool IsTaxable { get; private set; }
    
    private PayrollComponent() { }

    public static PayrollComponent Create(string name, PayrollComponentType type, bool isTaxable, string createdBy)
    {
        return new PayrollComponent
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Type = type,
            IsTaxable = isTaxable,
            CreatedBy = createdBy
        };
    }

    public void Update(string name, PayrollComponentType type, bool isTaxable, string updatedBy)
    {
        Name = name;
        Type = type;
        IsTaxable = isTaxable;
        SetUpdatedInfo(updatedBy);
    }
}
