using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using System;
using System.Collections.Generic;

namespace LS.Application.Features.Loans.Services;

public class StraightLineAmortizationStrategy : IAmortizationStrategy
{
    public InterestMethod SupportedMethod => InterestMethod.Flat;

    public List<LoanRepaymentSchedule> GenerateSchedule(LoanApplication loan)
    {
        var schedules = new List<LoanRepaymentSchedule>();
        
        // Flat Interest = (Principal * Rate * Term) / (12 * 100) assuming Rate is annual
        // Or if Rate is monthly, (Principal * Rate * Term) / 100
        // Wait, typical flat rate is annual. Let's assume InterestRate is annual percentage.
        decimal annualRate = loan.InterestRate / 100m;
        decimal totalInterest = loan.PrincipalAmount * annualRate * (loan.TermInMonths / 12m);
        
        decimal principalPerMonth = loan.PrincipalAmount / loan.TermInMonths;
        decimal interestPerMonth = totalInterest / loan.TermInMonths;

        DateTimeOffset nextDueDate = (loan.DisbursementDate ?? DateTimeOffset.UtcNow).AddMonths(1);

        for (int i = 1; i <= loan.TermInMonths; i++)
        {
            schedules.Add(LoanRepaymentSchedule.Create(
                loan.TenantId,
                loan.Id,
                i,
                nextDueDate,
                principalPerMonth,
                interestPerMonth,
                "System-Amortization"
            ));

            nextDueDate = nextDueDate.AddMonths(1);
        }

        return schedules;
    }
}
