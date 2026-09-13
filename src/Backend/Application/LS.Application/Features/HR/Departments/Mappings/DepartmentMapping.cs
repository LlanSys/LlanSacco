using LS.Domain.Features.HR.Departments.Entities;
using LS.SharedKernel.Features.HR.Departments.Dtos;

namespace LS.Application.Features.HR.Departments.Mappings;

public static class DepartmentMapping
{
    public static DepartmentResponse ToDepartmentResponse(this Department department)
    {
        ArgumentNullException.ThrowIfNull(department);

        return new DepartmentResponse(
            department.Id,
            department.Code,
            department.Name,
            department.Description,
            department.IsActive);
    }
}
