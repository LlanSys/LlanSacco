using FluentValidation;
using LS.Application.Features.Loans.LoanApplications.Commands;

namespace LS.Application.Features.Loans.LoanApplications.Validators;

internal class ApplyForLoanCommandValidator : AbstractValidator<ApplyForLoanCommand>
{
    public ApplyForLoanCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.LoanProductId).NotEmpty();
        RuleFor(x => x.PrincipalAmount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.TermInMonths).InclusiveBetween(1, 600);
        RuleFor(x => x.Guarantors).Cascade(CascadeMode.Stop).NotNull().Must(x => x.Count <= 100 && x.Select(g => g.GuarantorMemberId).Distinct().Count() == x.Count);
        RuleFor(x => x.Guarantors).Must((request, guarantors) => guarantors.All(x => x.GuarantorMemberId != request.MemberId)).When(x => x.Guarantors is not null);
        RuleForEach(x => x.Guarantors).SetValidator(new LoanGuarantorInputValidator());
    }
}
