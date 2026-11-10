using LS.Domain.Shared.Contracts.Repositories;
using LS.Domain.Features.HR.Employees.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Domain.Features.HR.Employees.Contracts.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
}

