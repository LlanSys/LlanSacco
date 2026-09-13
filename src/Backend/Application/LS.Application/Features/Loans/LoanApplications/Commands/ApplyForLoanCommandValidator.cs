using FluentValidation;

namespace LS.Application.Features.Loans.LoanApplications.Commands;

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

internal class LoanGuarantorInputValidator : AbstractValidator<LoanGuarantorInput>
{
    public LoanGuarantorInputValidator()
    {
        RuleFor(x => x.GuarantorMemberId).NotEmpty();
        RuleFor(x => x.GuaranteedAmount).GreaterThan(0);
    }
}

