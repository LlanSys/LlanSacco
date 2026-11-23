using FluentValidation;
using LS.Application.Features.HR.Employees.CommandHandlers;

namespace LS.Application.Features.HR.Employees.Validators;

internal sealed class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Employee ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
