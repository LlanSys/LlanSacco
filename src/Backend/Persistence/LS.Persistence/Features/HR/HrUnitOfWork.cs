using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Features.HR.Departments.Contracts.Repositories;
using LS.Domain.Features.HR.Employees.Contracts.Repositories;
using LS.Domain.Features.HR.Payroll.Contracts.Repositories;
using LS.Domain.Features.IAM.Users.Contracts.Repositories;
using LS.Domain.Shared.Contracts.Repositories;
using LS.Persistence.Common;
using LS.Persistence.Features.HR.DataContext;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Features.HR;

public sealed class HrUnitOfWork(
    HrDBContext context,
    IDepartmentRepository departmentRepository,
    IEmployeeRepository employeeRepository,
    IEmployeeNumberSequenceRepository employeeNumberSequenceRepository,
    IPayrollStatutoryConfigurationRepository payrollStatutoryConfigurationRepository,
    IPayrollPeriodRepository payrollPeriodRepository,
    IEmployeeSalaryRepository employeeSalaryRepository,
    IPayrollComponentRepository payrollComponentRepository,
    IEmployeePayrollComponentRepository employeePayrollComponentRepository,
    IPayslipRepository payslipRepository,
    IPublisher publisher,
    ILogger<HrUnitOfWork> logger
) : BaseUnitOfWork<HrDBContext>(context, publisher, logger), IHrUnitOfWork
{
    public IDepartmentRepository DepartmentRepository { get; } = departmentRepository;
    public IEmployeeRepository EmployeeRepository { get; } = employeeRepository;
    public IEmployeeNumberSequenceRepository EmployeeNumberSequenceRepository { get; } = employeeNumberSequenceRepository;

    public IPayrollStatutoryConfigurationRepository PayrollStatutoryConfigurationRepository { get; } = payrollStatutoryConfigurationRepository;
    public IPayrollPeriodRepository PayrollPeriodRepository { get; } = payrollPeriodRepository;
    public IEmployeeSalaryRepository EmployeeSalaryRepository { get; } = employeeSalaryRepository;
    public IPayrollComponentRepository PayrollComponentRepository { get; } = payrollComponentRepository;
    public IEmployeePayrollComponentRepository EmployeePayrollComponentRepository { get; } = employeePayrollComponentRepository;
    public IPayslipRepository PayslipRepository { get; } = payslipRepository;
}
