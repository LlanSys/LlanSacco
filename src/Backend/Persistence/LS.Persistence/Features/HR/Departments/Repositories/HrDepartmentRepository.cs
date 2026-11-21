using LS.Domain.Features.HR.Departments.Contracts.Repositories;
using LS.Domain.Features.HR.Departments.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.HR.DataContext;

namespace LS.Persistence.Features.HR.Departments.Repositories;

internal sealed class HrDepartmentRepository(HrDBContext context) : Repository<Department>(context), IDepartmentRepository
{
}
