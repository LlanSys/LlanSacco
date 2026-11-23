using FluentValidation;
using LS.Application.Features.Loans.LoanApplications.Commands;

namespace LS.Application.Features.Loans.LoanApplications.Validators;

internal class LoanGuarantorInputValidator : AbstractValidator<LoanGuarantorInput>
{
    public LoanGuarantorInputValidator()
    {
        RuleFor(x => x.GuarantorMemberId).NotEmpty();
        RuleFor(x => x.GuaranteedAmount).GreaterThan(0);
    }
}
