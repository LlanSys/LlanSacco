using FluentValidation;
using LS.SharedKernel.Features.Membership.Dtos;

namespace LS.SharedKernel.Validation.Features.Membership;

public class OnboardMemberRequestValidator : AbstractValidator<OnboardMemberRequest>
{
    public OnboardMemberRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");

        RuleFor(x => x.IdentificationNumber)
            .NotEmpty().WithMessage("Identification number is required.")
            .MaximumLength(50).WithMessage("Identification number cannot exceed 50 characters.");

        RuleFor(x => x.MembershipType)
            .IsInEnum().WithMessage("A valid membership type must be provided.");
    }
}
