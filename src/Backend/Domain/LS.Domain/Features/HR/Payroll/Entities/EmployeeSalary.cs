using System;
using LS.Domain.Shared.Entities;

namespace LS.Domain.Features.HR.Payroll.Entities;

public class EmployeeSalary : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public decimal BasicSalary { get; private set; }
    public string Currency { get; private set; } = "KES";

    private EmployeeSalary() { }

    public static EmployeeSalary Create(Guid employeeId, decimal basicSalary, string createdBy)
    {
        return new EmployeeSalary
        {
            Id = Guid.CreateVersion7(),
            EmployeeId = employeeId,
            BasicSalary = basicSalary,
            CreatedBy = createdBy
        };
    }

    public void UpdateBasicSalary(decimal basicSalary, string updatedBy)
    {
        BasicSalary = basicSalary;
        SetUpdatedInfo(updatedBy);
    }
}
