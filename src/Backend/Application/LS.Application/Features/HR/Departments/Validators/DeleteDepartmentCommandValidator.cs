using FluentValidation;
using LS.Application.Features.HR.Departments.CommandHandlers;

namespace LS.Application.Features.HR.Departments.Validators;

internal sealed class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Department ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
