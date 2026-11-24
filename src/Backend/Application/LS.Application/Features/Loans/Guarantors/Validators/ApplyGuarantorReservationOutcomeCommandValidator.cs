using FluentValidation;
using LS.Application.Features.Loans.Guarantors.Commands;

namespace LS.Application.Features.Loans.Guarantors.Validators;

internal sealed class ApplyGuarantorReservationOutcomeCommandValidator : AbstractValidator<ApplyGuarantorReservationOutcomeCommand>
{
    public ApplyGuarantorReservationOutcomeCommandValidator()
    {
        RuleFor(x => x.Outcome).NotNull();
        When(
            x => x.Outcome is not null,
            () =>
            {
                RuleFor(x => x.Outcome.TenantId).NotEmpty();
                RuleFor(x => x.Outcome.OperationId).NotEmpty();
                RuleFor(x => x.Outcome.LoanId).NotEmpty();
            });
    }
}
