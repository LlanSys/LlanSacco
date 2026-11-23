using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class ProcessSsoLoginCommandValidator : AbstractValidator<ProcessSsoLoginCommand>
{
    public ProcessSsoLoginCommandValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.Provider).NotEmpty();
        RuleFor(request => request.ProviderKey).NotEmpty();
    }
}
