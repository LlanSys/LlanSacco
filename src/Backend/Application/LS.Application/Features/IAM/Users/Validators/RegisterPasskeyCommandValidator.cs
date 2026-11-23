using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;
using System.Text.Json;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class RegisterPasskeyCommandValidator : AbstractValidator<RegisterPasskeyCommand>
{
    public RegisterPasskeyCommandValidator()
    {
        RuleFor(request => request.AttestationResponse).Must(payload => payload.ValueKind == JsonValueKind.Object).WithMessage("Provide a JSON object.");
    }
}
