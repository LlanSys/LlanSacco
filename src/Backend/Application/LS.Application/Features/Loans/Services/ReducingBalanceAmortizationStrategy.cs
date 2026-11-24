using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;

namespace LS.Application.Features.Loans.Services;

public sealed class ReducingBalanceAmortizationStrategy : IAmortizationStrategy
{
    public InterestMethod SupportedMethod => InterestMethod.ReducingBalance;

    public List<LoanRepaymentSchedule> GenerateSchedule(LoanApplication loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        if (loan.TermInMonths is < 1 or > 600 || loan.InterestRate is < 0 or > 100 || loan.PrincipalAmount <= 0)
        {
            throw new InvalidOperationException("The loan terms are outside supported schedule limits.");
        }

        var disbursedAt = loan.DisbursementDate ?? throw new InvalidOperationException("A disbursement date is required.");
        var monthlyRate = loan.InterestRate / 1200m;
        var factor = Power(1 + monthlyRate, loan.TermInMonths);
        var installment = decimal.Round(
            monthlyRate == 0 ? loan.PrincipalAmount / loan.TermInMonths :
                loan.PrincipalAmount * monthlyRate / (1 - 1 / factor),
            2,
            MidpointRounding.ToEven);

        var remaining = loan.PrincipalAmount;
        var schedules = new List<LoanRepaymentSchedule>(loan.TermInMonths);

        for (var index = 1; index <= loan.TermInMonths; index++)
        {
            var interest = decimal.Round(remaining * monthlyRate, 2, MidpointRounding.ToEven);
            var principal = index == loan.TermInMonths ? remaining : Math.Min(remaining, installment - interest);
            var due = disbursedAt.AddMonths(index);

            if (disbursedAt.Day == DateTime.DaysInMonth(disbursedAt.Year, disbursedAt.Month))
            {
                due = due.AddDays(DateTime.DaysInMonth(due.Year, due.Month) - due.Day);
            }

            var schedule = LoanRepaymentSchedule.Create(
                loan.TenantId,
                loan.Id,
                index,
                due,
                principal,
                interest,
                loan.UpdatedBy ?? loan.CreatedBy);

            schedule.CreatedAt = disbursedAt;
            schedules.Add(schedule);
            remaining -= principal;
        }
        return schedules;
    }

    private static decimal Power(decimal value, int exponent)
    {
        var result = 1m;

        while (exponent > 0)
        {
            if ((exponent & 1) != 0)
            {
                result *= value;
            }

            exponent >>= 1;

            if (exponent > 0)
            {
                value *= value;
            }
        }
        return result;
    }
}
