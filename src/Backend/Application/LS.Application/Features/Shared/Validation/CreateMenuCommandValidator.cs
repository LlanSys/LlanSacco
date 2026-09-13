using LS.Application.Features.IAM.Menus.Commands;
using LS.SharedKernel.Validation.Features.IAM.Menus.Validators;
using FluentValidation;

namespace LS.Application.Features.Shared.Validation;

internal sealed class CreateMenuCommandValidator : AbstractValidator<CreateMenuCommand>
{
    public CreateMenuCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CreateMenuRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}

