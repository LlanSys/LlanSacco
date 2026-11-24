using FluentValidation;
using LS.Application.Features.Loans.Guarantors.Commands;

namespace LS.Application.Features.Loans.Guarantors.Validators;

internal sealed class ApproveGuarantorRecoveryCommandValidator : AbstractValidator<ApproveGuarantorRecoveryCommand>
{
    public ApproveGuarantorRecoveryCommandValidator() => RuleFor(x => x.RecoveryId).NotEmpty();
}
