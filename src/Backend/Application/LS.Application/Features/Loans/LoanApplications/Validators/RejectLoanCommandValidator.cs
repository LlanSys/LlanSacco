using FluentValidation;
using LS.Application.Features.Loans.LoanApplications.Commands;

namespace LS.Application.Features.Loans.LoanApplications.Validators;

internal sealed class RejectLoanCommandValidator : AbstractValidator<RejectLoanCommand>
{
    public RejectLoanCommandValidator()
    {
        RuleFor(request => request.LoanApplicationId).NotEmpty();
        RuleFor(request => request.Reason).NotEmpty();
    }
}
