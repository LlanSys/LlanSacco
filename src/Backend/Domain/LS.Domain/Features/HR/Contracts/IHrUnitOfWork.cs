using LS.Domain.Shared.Contracts;
using LS.Domain.Features.HR.Departments.Contracts.Repositories;
using LS.Domain.Features.HR.Employees.Contracts.Repositories;
using LS.Domain.Features.HR.Payroll.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Domain.Shared.Contracts.Repositories;

namespace LS.Domain.Features.HR.Contracts;

public interface IHrUnitOfWork : ITransactionalUnitOfWork
{
    IDepartmentRepository DepartmentRepository { get; }
    IEmployeeRepository EmployeeRepository { get; }
    IEmployeeNumberSequenceRepository EmployeeNumberSequenceRepository { get; }

    // Payroll
    IPayrollStatutoryConfigurationRepository PayrollStatutoryConfigurationRepository { get; }
    IPayrollPeriodRepository PayrollPeriodRepository { get; }
    IEmployeeSalaryRepository EmployeeSalaryRepository { get; }
    IPayrollComponentRepository PayrollComponentRepository { get; }
    IEmployeePayrollComponentRepository EmployeePayrollComponentRepository { get; }
    IPayslipRepository PayslipRepository { get; }
}
