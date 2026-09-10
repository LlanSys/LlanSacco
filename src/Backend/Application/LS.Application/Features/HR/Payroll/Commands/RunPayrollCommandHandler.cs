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
    ILogger<RunPayrollCommandHandler> logger) : IRequestHandler<RunPayrollCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(RunPayrollCommand request, CancellationToken cancellationToken)
    {
        var period = await unitOfWork.PayrollPeriodRepository.FindByIdAsync(request.PayrollPeriodId, cancellationToken);

        if (period == null)
            return new AppResponse<bool> { IsSuccess = false, Message = "Payroll period not found." };

        if (period.Status == PayrollPeriodStatus.Closed)
            return new AppResponse<bool> { IsSuccess = false, Message = "Cannot run payroll for a closed period." };

        var activeConfig = await unitOfWork.PayrollStatutoryConfigurationRepository
            .FirstOrDefaultAsync(c => c.EffectiveDate <= DateTimeOffset.UtcNow, cancellationToken); // Simplified since we can't do Include/OrderBy without ISpecification easily here. Or we could use a specification. Let's just use FirstOrDefaultAsync. Actually wait.
            // Oh, FirstOrDefaultAsync takes an expression but we need the latest. Let's just use ListAsync and filter in memory since configurations are few.
        var configs = await unitOfWork.PayrollStatutoryConfigurationRepository.ListAsync(null, cancellationToken);
        var config = configs.OrderByDescending(c => c.EffectiveDate).FirstOrDefault();
        activeConfig = config;

        if (activeConfig == null)
            return new AppResponse<bool> { IsSuccess = false, Message = "No active statutory configuration found." };

        var employees = await unitOfWork.EmployeeRepository.ListAsync(null, cancellationToken);
        employees = employees.Where(e => !e.IsDeleted).ToList();

        var existingPayslips = await unitOfWork.PayslipRepository.ListAsync(null, cancellationToken);
        existingPayslips = existingPayslips.Where(p => p.PayrollPeriodId == period.Id).ToList();

        var salaries = await unitOfWork.EmployeeSalaryRepository.ListAsync(null, cancellationToken);
        var employeeSalaries = salaries.ToDictionary(s => s.EmployeeId, s => s);

        var empComps = await unitOfWork.EmployeePayrollComponentRepository.ListAsync(null, cancellationToken);
        var employeeComponents = empComps.Where(ec => ec.IsActive).ToList();

        var comps = await unitOfWork.PayrollComponentRepository.ListAsync(null, cancellationToken);
        var components = comps.ToDictionary(c => c.Id, c => c);

        var currentUser = currentActorProvider.ActorId;

        // Delete existing draft payslips for this period
        foreach (var p in existingPayslips)
        {
            await unitOfWork.PayslipRepository.DeleteAsync(p.Id, cancellationToken);
        }

        foreach (var employee in employees)
        {
            if (!employeeSalaries.TryGetValue(employee.Id, out var salary))
            {
                logger.LogWarning("Employee {EmployeeId} has no salary configuration. Skipping.", employee.Id);
                continue;
            }

            var basicSalary = salary.BasicSalary;
            
            // Gather components
            var employeeComps = employeeComponents.Where(ec => ec.EmployeeId == employee.Id).ToList();
            
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

        await unitOfWork.CompleteAsync(cancellationToken);

        return new AppResponse<bool>(true, "Payroll processed successfully.");
    }
}
