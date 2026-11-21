using LS.Domain.Features.HR.Payroll.Contracts.Repositories;
using LS.Domain.Features.HR.Payroll.Entities;
using LS.Persistence.Common.Repositories;
using LS.Persistence.Features.HR.DataContext;

namespace LS.Persistence.Features.HR.Payroll;

internal sealed class PayrollStatutoryConfigurationRepository(HrDBContext context) : Repository<PayrollStatutoryConfiguration>(context), IPayrollStatutoryConfigurationRepository { }
internal sealed class PayrollPeriodRepository(HrDBContext context) : Repository<PayrollPeriod>(context), IPayrollPeriodRepository { }
internal sealed class EmployeeSalaryRepository(HrDBContext context) : Repository<EmployeeSalary>(context), IEmployeeSalaryRepository { }
internal sealed class PayrollComponentRepository(HrDBContext context) : Repository<PayrollComponent>(context), IPayrollComponentRepository { }
internal sealed class EmployeePayrollComponentRepository(HrDBContext context) : Repository<EmployeePayrollComponent>(context), IEmployeePayrollComponentRepository { }
internal sealed class PayslipRepository(HrDBContext context) : Repository<Payslip>(context), IPayslipRepository { }
