using FluentValidation;
using LS.Application.Features.Loans.LoanApplications.Commands;

namespace LS.Application.Features.Loans.LoanApplications.Validators;

internal class ApplyForLoanCommandValidator : AbstractValidator<ApplyForLoanCommand>
{
    public ApplyForLoanCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.LoanProductId).NotEmpty();
        RuleFor(x => x.PrincipalAmount).GreaterThan(0);
        RuleFor(x => x.TermInMonths).GreaterThan(0);
        RuleForEach(x => x.Guarantors).SetValidator(new LoanGuarantorInputValidator());
    }
}
