using FluentValidation;
using LS.Application.Features.Shared.Payments.CommandHandlers.Mpesa;
using System.Text.Json;

namespace LS.Application.Features.Shared.Payments.Validators.Mpesa;

internal sealed class ProcessMpesaC2BValidationCommandValidator : AbstractValidator<ProcessMpesaC2BValidationCommand>
{
    public ProcessMpesaC2BValidationCommandValidator()
    {
        RuleFor(request => request.Payload).Must(payload => payload.ValueKind == JsonValueKind.Object).WithMessage("Provide a JSON object.");
    }
}
