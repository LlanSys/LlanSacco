using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class CreateAppUserCommandValidator : AbstractValidator<CreateAppUserCommand>
{
    public CreateAppUserCommandValidator()
    {
        RuleFor(request => request.Request).NotNull();
        RuleFor(request => request.Request.Username).NotEmpty().When(request => request.Request is not null);
        RuleFor(request => request.Request.Email).NotEmpty().EmailAddress().When(request => request.Request is not null);
        RuleFor(request => request.Request.Password).NotEmpty().When(request => request.Request is not null);
        RuleFor(request => request.Request.FirstName).NotEmpty().When(request => request.Request is not null);
        RuleFor(request => request.Request.LastName).NotEmpty().When(request => request.Request is not null);
    }
}
