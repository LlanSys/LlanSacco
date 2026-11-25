using LS.Application.Features.IAM.Users.Queries;
using LS.SharedKernel.Validation.Features.IAM.Users.Validators;
using FluentValidation;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class ValidatePasswordResetLinkQueryValidator : AbstractValidator<ValidatePasswordResetLinkQuery>
{
    public ValidatePasswordResetLinkQueryValidator()
    {
        RuleFor(query => query.Request)
            .NotNull()
            .SetValidator(new ValidatePasswordResetLinkRequestValidator());
    }
}
