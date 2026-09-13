using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.HR.Payroll.Entities;
using LS.Domain.Features.HR.Payroll.Enums;
using LS.Domain.Features.HR.Payroll.Services;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.HR.Payroll.Commands;

internal sealed class RunPayrollCommandHandler(
    IHrUnitOfWork unitOfWork,
    IKenyaTaxCalculatorService taxCalculator,
    ICurrentActorProvider currentActorProvider,
    ICurrentTenantProvider tenantProvider) : IRequestHandler<RunPayrollCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(RunPayrollCommand request, CancellationToken cancellationToken)
    {
        if (tenantProvider.TenantId == Guid.Empty) return AppResponses.Failure<bool>(AppError.Forbidden("A tenant context is required."));
        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var period = await unitOfWork.PayrollPeriodRepository.FindByIdAsync(request.PayrollPeriodId, cancellationToken);

            if (period == null || period.TenantId != tenantProvider.TenantId)
                return AppResponses.NotFound<bool>("Payroll period not found.");

            if (period.Status == PayrollPeriodStatus.Closed)
                return AppResponses.Failure<bool>("Cannot run payroll for a closed period.");

            if (period.ProcessedAt is not null || await unitOfWork.PayslipRepository.AnyAsync(p => p.PayrollPeriodId == period.Id, cancellationToken))
                return AppResponses.Failure<bool>("Payroll has already been generated for this period. Existing payslips require an explicit correction workflow.");

            var monthEnd = new DateTimeOffset(period.Year, period.Month, 1, 0, 0, 0, TimeSpan.FromHours(3)).AddMonths(1).AddTicks(-1);
            var now = DateTimeOffset.UtcNow;
            var effectiveAt = (monthEnd < now ? monthEnd : now).ToUniversalTime();
            var activeConfig = await unitOfWork.PayrollStatutoryConfigurationRepository.FirstOrDefaultAsync(q => q
                .Where(c => c.EffectiveDate <= effectiveAt)
                .OrderByDescending(c => c.EffectiveDate).ThenByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id), cancellationToken);
            if (activeConfig == null)
                return AppResponses.Failure<bool>("No statutory configuration is effective for this payroll period.");
            if (!activeConfig.PayeTaxBands.Any())
                return AppResponses.Failure<bool>("The statutory configuration has no PAYE tax bands.");

            var employees = await unitOfWork.EmployeeRepository.ListAsync(q => q.Where(e => !e.IsDeleted), cancellationToken);
            if (employees.Count == 0) return AppResponses.Failure<bool>("There are no eligible employees for payroll.");
            var employeeIds = employees.Select(e => e.Id).ToArray();
            var salaries = await unitOfWork.EmployeeSalaryRepository.ListAsync(q => q.Where(s => employeeIds.Contains(s.EmployeeId)), cancellationToken);
            if (salaries.GroupBy(s => s.EmployeeId).Any(g => g.Skip(1).Any()) || salaries.Any(s => s.BasicSalary < 0))
                return AppResponses.Failure<bool>("Employee salary configuration is ambiguous or invalid.");
            var employeeSalaries = salaries.ToDictionary(s => s.EmployeeId);
            if (employees.Any(e => !employeeSalaries.ContainsKey(e.Id)))
                return AppResponses.Failure<bool>("Every eligible employee must have a salary configuration before payroll can run.");
            var employeeComponents = await unitOfWork.EmployeePayrollComponentRepository.ListAsync(q => q
                .Where(ec => ec.IsActive && employeeIds.Contains(ec.EmployeeId)), cancellationToken);
            var componentIds = employeeComponents.Select(ec => ec.PayrollComponentId).Distinct().ToArray();
            var comps = await unitOfWork.PayrollComponentRepository.ListAsync(q => q.Where(c => componentIds.Contains(c.Id)), cancellationToken);
            var components = comps.ToDictionary(c => c.Id);
            if (employeeComponents.Any(ec => !components.ContainsKey(ec.PayrollComponentId) || ec.Amount < 0))
                return AppResponses.Failure<bool>("Payroll components are missing or invalid.");
            var componentsByEmployee = employeeComponents.ToLookup(ec => ec.EmployeeId);
            var currentUser = currentActorProvider.ActorId;
            foreach (var employee in employees)
            {
                var salary = employeeSalaries[employee.Id];
                cancellationToken.ThrowIfCancellationRequested();
                var basicSalary = salary.BasicSalary;

                // Gather components
                var employeeComps = componentsByEmployee[employee.Id].ToArray();

                decimal totalAllowances = 0m;
                decimal taxableAllowances = 0m;
                decimal nonStatutoryDeductions = 0m;

                foreach (var ec in employeeComps)
                {
                    if (components.TryGetValue(ec.PayrollComponentId, out var comp))
                    {
                        if (comp.Type == PayrollComponentType.Allowance)
                        {
                            totalAllowances += ec.Amount;
                            if (comp.IsTaxable) taxableAllowances += ec.Amount;
                        }
                        else if (comp.Type == PayrollComponentType.Deduction)
                        {
                            nonStatutoryDeductions += ec.Amount;
                        }
                    }
                }

                decimal grossPay = basicSalary + totalAllowances;
                decimal taxableGross = basicSalary + taxableAllowances; // Simplified, assuming basic is taxable.

                // Calculate Statutory Taxes
                var taxes = taxCalculator.CalculateTaxes(taxableGross, activeConfig);

                decimal totalStatutory = taxes.Paye + taxes.Nssf + taxes.Shif + taxes.HousingLevy;
                decimal netPay = grossPay - totalStatutory - nonStatutoryDeductions;

                var payslip = Payslip.Create(
                    period.Id,
                    employee.Id,
                    basicSalary,
                    totalAllowances,
                    grossPay,
                    taxes.Paye,
                    taxes.Nssf,
                    taxes.Shif,
                    taxes.HousingLevy,
                    totalStatutory + nonStatutoryDeductions,
                    netPay,
                    currentUser);

                // Add details
                payslip.AddDetail("Basic Salary", basicSalary, PayslipDetailType.GrossPay); // Actually just a detail of Gross

                if (taxes.Paye > 0) payslip.AddDetail("PAYE", taxes.Paye, PayslipDetailType.StatutoryDeduction);
                if (taxes.Nssf > 0) payslip.AddDetail("NSSF", taxes.Nssf, PayslipDetailType.StatutoryDeduction);
                if (taxes.Shif > 0) payslip.AddDetail("SHIF", taxes.Shif, PayslipDetailType.StatutoryDeduction);
                if (taxes.HousingLevy > 0) payslip.AddDetail("Housing Levy", taxes.HousingLevy, PayslipDetailType.StatutoryDeduction);

                foreach (var ec in employeeComps)
                {
                    if (components.TryGetValue(ec.PayrollComponentId, out var comp))
                    {
                        var type = comp.Type == PayrollComponentType.Allowance ? PayslipDetailType.Earning : PayslipDetailType.Deduction;
                        payslip.AddDetail(comp.Name, ec.Amount, type);
                    }
                }

                await unitOfWork.PayslipRepository.CreateAsync(payslip, cancellationToken);
            }

            period.RecordPayrollRun(currentUser);
            await unitOfWork.PayrollPeriodRepository.UpdateAsync(period, cancellationToken);

            return new AppResponse<bool>(true, "Payroll processed successfully.");
        }, cancellationToken);
    }
}
