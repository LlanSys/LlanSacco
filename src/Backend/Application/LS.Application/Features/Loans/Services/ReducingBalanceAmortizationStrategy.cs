using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;
using System;
using System.Collections.Generic;

namespace LS.Application.Features.Loans.Services;

public class ReducingBalanceAmortizationStrategy : IAmortizationStrategy
{
    public InterestMethod SupportedMethod => InterestMethod.ReducingBalance;

    public List<LoanRepaymentSchedule> GenerateSchedule(LoanApplication loan)
    {
        var schedules = new List<LoanRepaymentSchedule>();
        
        decimal monthlyRate = (loan.InterestRate / 100m) / 12m;
        int n = loan.TermInMonths;
        decimal p = loan.PrincipalAmount;

        decimal emi;
        if (monthlyRate == 0)
        {
            emi = p / n;
        }
        else
        {
            double r = (double)monthlyRate;
            double mathFactor = Math.Pow(1 + r, n);
            emi = p * (decimal)(r * mathFactor / (mathFactor - 1));
        }

        DateTimeOffset nextDueDate = (loan.DisbursementDate ?? DateTimeOffset.UtcNow).AddMonths(1);
        decimal remainingPrincipal = p;

        for (int i = 1; i <= n; i++)
        {
            decimal interestForMonth = remainingPrincipal * monthlyRate;
            decimal principalForMonth = emi - interestForMonth;

            // Handle rounding differences on the last installment
            if (i == n)
            {
                principalForMonth = remainingPrincipal;
            }

            schedules.Add(LoanRepaymentSchedule.Create(
                loan.TenantId,
                loan.Id,
                i,
                nextDueDate,
                principalForMonth,
                interestForMonth,
                "System-Amortization"
            ));

            remainingPrincipal -= principalForMonth;
            nextDueDate = nextDueDate.AddMonths(1);
        }

        return schedules;
    }
}
