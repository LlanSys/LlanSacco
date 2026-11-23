using FluentValidation;
using LS.Application.Features.Membership.Commands;
using LS.SharedKernel.Validation.Features.Membership;

namespace LS.Application.Features.Membership.Validators;

internal sealed class RespondToGuarantorCommandValidator : AbstractValidator<RespondToGuarantorCommand>
{
    public RespondToGuarantorCommandValidator()
    {
        RuleFor(request => request.Request).NotNull().SetValidator(new RespondToGuarantorRequestValidator());
    }
}
