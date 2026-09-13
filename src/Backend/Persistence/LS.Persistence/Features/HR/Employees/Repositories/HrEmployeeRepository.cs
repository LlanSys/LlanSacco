using LS.Domain.Features.HR.Employees.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.HR.Employees.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.HR.DataContext;

namespace LS.Persistence.Features.HR.Employees.Repositories;

internal sealed class HrEmployeeRepository(HrDBContext context) : Repository<Employee>(context), IEmployeeRepository { }
