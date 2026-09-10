using FluentValidation;
using LS.SharedKernel.Features.Membership.Dtos;

namespace LS.SharedKernel.Validation.Features.Membership;

public class RespondToGuarantorRequestValidator : AbstractValidator<RespondToGuarantorRequest>
{
    public RespondToGuarantorRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Request ID is required.");

        RuleFor(x => x.ResponseNote)
            .MaximumLength(500).WithMessage("Response note cannot exceed 500 characters.");
    }
}
