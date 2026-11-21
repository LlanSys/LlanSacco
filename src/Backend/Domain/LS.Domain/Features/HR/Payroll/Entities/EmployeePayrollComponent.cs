using System;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.HR.Payroll.Entities;

public class EmployeePayrollComponent : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid PayrollComponentId { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsActive { get; private set; }

    private EmployeePayrollComponent() { }

    public static EmployeePayrollComponent Create(Guid employeeId, Guid payrollComponentId, decimal amount, string createdBy)
    {
        return new EmployeePayrollComponent
        {
            Id = Guid.CreateVersion7(),
            EmployeeId = employeeId,
            PayrollComponentId = payrollComponentId,
            Amount = amount,
            IsActive = true,
            CreatedBy = createdBy
        };
    }

    public void UpdateAmount(decimal amount, string updatedBy)
    {
        Amount = amount;
        SetUpdatedInfo(updatedBy);
    }

    public void Deactivate(string updatedBy)
    {
        IsActive = false;
        SetUpdatedInfo(updatedBy);
    }
}
