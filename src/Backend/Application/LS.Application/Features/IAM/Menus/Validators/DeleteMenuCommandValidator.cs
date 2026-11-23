using FluentValidation;
using LS.Application.Features.IAM.Menus.Commands;

namespace LS.Application.Features.IAM.Menus.Validators;

internal sealed class DeleteMenuCommandValidator : AbstractValidator<DeleteMenuCommand>
{
    public DeleteMenuCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Menu ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
