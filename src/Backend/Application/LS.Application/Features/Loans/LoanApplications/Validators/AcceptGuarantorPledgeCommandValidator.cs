using FluentValidation;
using LS.Application.Features.Loans.LoanApplications.Commands;

namespace LS.Application.Features.Loans.LoanApplications.Validators;

internal sealed class AcceptGuarantorPledgeCommandValidator : AbstractValidator<AcceptGuarantorPledgeCommand>
{
    public AcceptGuarantorPledgeCommandValidator()
    {
        RuleFor(request => request.LoanApplicationId).NotEmpty();
        RuleFor(request => request.GuarantorMemberId).NotEmpty();
        RuleFor(request => request.PledgeId).NotEmpty();
        RuleFor(request => request.AcceptedAmount).GreaterThan(0);
        RuleFor(request => request.AcceptedAmount).PrecisionScale(18, 2, true);
        RuleFor(request => request.SavingsAccountId).NotEmpty();
        RuleFor(request => request.TermsAccepted).Equal(true);
        RuleFor(request => request.TermsVersion).NotEmpty().MaximumLength(80);
    }
}
