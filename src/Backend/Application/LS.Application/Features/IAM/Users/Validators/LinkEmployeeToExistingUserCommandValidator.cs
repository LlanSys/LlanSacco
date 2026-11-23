using LS.SharedKernel.Validation.Features.HR.Employees.Validators;
using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class LinkEmployeeToExistingUserCommandValidator : AbstractValidator<LinkEmployeeToExistingUserCommand>
{
    public LinkEmployeeToExistingUserCommandValidator()
    {
        RuleFor(request => request.NationalId).NotEmpty();
        RuleFor(request => request.EmployeeDetails).NotNull().SetValidator(new CreateEmployeeRequestValidator());
    }
}
