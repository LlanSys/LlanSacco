using FluentValidation;
using LS.Application.Features.IAM.Menus.Commands;
using LS.SharedKernel.Validation.Features.IAM.Menus.Validators;

namespace LS.Application.Features.IAM.Menus.Validators;

internal sealed class CreateMenuCommandValidator : AbstractValidator<CreateMenuCommand>
{
    public CreateMenuCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CreateMenuRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
