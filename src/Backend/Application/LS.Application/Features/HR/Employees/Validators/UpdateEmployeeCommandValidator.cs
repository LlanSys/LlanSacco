using FluentValidation;
using LS.Application.Features.HR.Employees.CommandHandlers;
using LS.SharedKernel.Validation.Features.HR.Employees.Validators;

namespace LS.Application.Features.HR.Employees.Validators;

internal sealed class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Employee ID is required.");
        RuleFor(x => x.Request).SetValidator(new UpdateEmployeeRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
