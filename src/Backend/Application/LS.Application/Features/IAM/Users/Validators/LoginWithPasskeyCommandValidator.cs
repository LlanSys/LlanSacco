using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;
using System.Text.Json;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class LoginWithPasskeyCommandValidator : AbstractValidator<LoginWithPasskeyCommand>
{
    public LoginWithPasskeyCommandValidator()
    {
        RuleFor(request => request.CorrelationId).NotEmpty();
        RuleFor(request => request.AssertionResponse).Must(payload => payload.ValueKind == JsonValueKind.Object).WithMessage("Provide a JSON object.");
    }
}
