using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class UpdateAdminUserCommandValidator : AbstractValidator<UpdateAdminUserCommand>
{
    public UpdateAdminUserCommandValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
        RuleFor(request => request.Request).NotNull();
        RuleFor(request => request.Request.UserName).NotEmpty().When(request => request.Request is not null);
        RuleFor(request => request.Request.Email).NotEmpty().EmailAddress().When(request => request.Request is not null);
        RuleFor(request => request.Request.FirstName).NotEmpty().When(request => request.Request is not null);
        RuleFor(request => request.Request.LastName).NotEmpty().When(request => request.Request is not null);
        RuleFor(request => request.Request.Roles).NotNull().When(request => request.Request is not null);
    }
}
