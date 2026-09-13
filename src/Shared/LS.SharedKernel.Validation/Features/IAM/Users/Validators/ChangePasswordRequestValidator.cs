using LS.SharedKernel.Features.IAM.Users.Dtos;
using LS.SharedKernel.Validation.Validators.Common;
using FluentValidation;

namespace LS.SharedKernel.Validation.Features.IAM.Users.Validators;

public sealed class ChangePasswordRequestValidator : Validator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters.")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password.");
    }
}
