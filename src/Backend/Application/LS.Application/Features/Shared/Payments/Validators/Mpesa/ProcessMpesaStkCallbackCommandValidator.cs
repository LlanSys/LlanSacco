using FluentValidation;
using LS.Application.Features.Shared.Payments.CommandHandlers.Mpesa;
using System.Text.Json;

namespace LS.Application.Features.Shared.Payments.Validators.Mpesa;

internal sealed class ProcessMpesaStkCallbackCommandValidator : AbstractValidator<ProcessMpesaStkCallbackCommand>
{
    public ProcessMpesaStkCallbackCommandValidator()
    {
        RuleFor(request => request.Payload).Must(payload => payload.ValueKind == JsonValueKind.Object).WithMessage("Provide a JSON object.");
    }
}
