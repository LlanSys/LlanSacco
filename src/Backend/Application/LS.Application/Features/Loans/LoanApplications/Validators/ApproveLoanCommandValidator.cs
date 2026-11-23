using FluentValidation;
using LS.Application.Features.Loans.LoanApplications.Commands;

namespace LS.Application.Features.Loans.LoanApplications.Validators;

internal sealed class ApproveLoanCommandValidator : AbstractValidator<ApproveLoanCommand>
{
    public ApproveLoanCommandValidator()
    {
        RuleFor(request => request.LoanApplicationId).NotEmpty();
    }
}
