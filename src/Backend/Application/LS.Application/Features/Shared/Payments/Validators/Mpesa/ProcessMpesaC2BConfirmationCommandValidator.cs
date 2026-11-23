using FluentValidation;
using LS.Application.Features.Shared.Payments.CommandHandlers.Mpesa;
using System.Text.Json;

namespace LS.Application.Features.Shared.Payments.Validators.Mpesa;

internal sealed class ProcessMpesaC2BConfirmationCommandValidator : AbstractValidator<ProcessMpesaC2BConfirmationCommand>
{
    public ProcessMpesaC2BConfirmationCommandValidator()
    {
        RuleFor(request => request.Payload).Must(payload => payload.ValueKind == JsonValueKind.Object).WithMessage("Provide a JSON object.");
    }
}
