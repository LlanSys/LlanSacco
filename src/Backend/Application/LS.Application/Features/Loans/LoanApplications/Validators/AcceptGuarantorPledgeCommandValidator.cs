using FluentValidation;
using LS.Application.Features.Loans.LoanApplications.Commands;

namespace LS.Application.Features.Loans.LoanApplications.Validators;

internal sealed class AcceptGuarantorPledgeCommandValidator : AbstractValidator<AcceptGuarantorPledgeCommand>
{
    public AcceptGuarantorPledgeCommandValidator()
    {
        RuleFor(request => request.LoanApplicationId).NotEmpty();
        RuleFor(request => request.GuarantorMemberId).NotEmpty();
        RuleFor(request => request.AcceptedAmount).GreaterThan(0);
    }
}
