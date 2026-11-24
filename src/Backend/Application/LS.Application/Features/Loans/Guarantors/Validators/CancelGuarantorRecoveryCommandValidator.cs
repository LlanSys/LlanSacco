using FluentValidation;
using LS.Application.Features.Loans.Guarantors.Commands;

namespace LS.Application.Features.Loans.Guarantors.Validators;

internal sealed class CancelGuarantorRecoveryCommandValidator : AbstractValidator<CancelGuarantorRecoveryCommand>
{
    public CancelGuarantorRecoveryCommandValidator() => RuleFor(x => x.RecoveryId).NotEmpty();
}
