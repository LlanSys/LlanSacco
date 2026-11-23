using FluentValidation;
using LS.Application.Features.HR.Departments.CommandHandlers;
using LS.SharedKernel.Validation.Features.HR.Departments.Validators;

namespace LS.Application.Features.HR.Departments.Validators;

internal sealed class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CreateDepartmentRequestValidator());
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Current user is required.");
    }
}
