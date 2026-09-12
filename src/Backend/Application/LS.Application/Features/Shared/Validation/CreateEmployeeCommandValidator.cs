using LS.Application.Features.HR.Employees.CommandHandlers;
using LS.SharedKernel.Validation.Features.HR.Employees.Validators;
using FluentValidation;

namespace LS.Application.Features.Shared.Validation;

internal sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CreateEmployeeRequestValidator());
        RuleFor(x => x.User).NotEmpty().WithMessage("Current user is required.");
    }
}

