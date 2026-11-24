using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Features.Loans.Entities;
using LS.Domain.Features.Loans.Enums;

namespace LS.Application.Features.Loans.Services;

public sealed class StraightLineAmortizationStrategy : IAmortizationStrategy
{
    public InterestMethod SupportedMethod => InterestMethod.Flat;

    public List<LoanRepaymentSchedule> GenerateSchedule(LoanApplication loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        if (loan.TermInMonths is < 1 or > 600 || loan.InterestRate is < 0 or > 100 || loan.PrincipalAmount <= 0)
        {
            throw new InvalidOperationException("The loan terms are outside supported schedule limits.");
        }

        var disbursedAt = loan.DisbursementDate ?? throw new InvalidOperationException("A disbursement date is required.");
        var totalInterest = decimal.Round(loan.PrincipalAmount * loan.InterestRate / 1200m * loan.TermInMonths, 2, MidpointRounding.ToEven);
        var monthlyPrincipal = decimal.Round(loan.PrincipalAmount / loan.TermInMonths, 2, MidpointRounding.ToEven);
        var monthlyInterest = decimal.Round(totalInterest / loan.TermInMonths, 2, MidpointRounding.ToEven);
        var remainingPrincipal = loan.PrincipalAmount;
        var remainingInterest = totalInterest;
        var schedules = new List<LoanRepaymentSchedule>(loan.TermInMonths);

        for (var index = 1; index <= loan.TermInMonths; index++)
        {
            var principal = index == loan.TermInMonths ? remainingPrincipal : Math.Min(remainingPrincipal, monthlyPrincipal);
            var interest = index == loan.TermInMonths ? remainingInterest : Math.Min(remainingInterest, monthlyInterest);
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
            remainingPrincipal -= principal;
            remainingInterest -= interest;
        }
        return schedules;
    }
}
