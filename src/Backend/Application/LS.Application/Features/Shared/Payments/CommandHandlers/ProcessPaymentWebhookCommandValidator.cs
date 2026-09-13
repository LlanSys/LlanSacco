using FluentValidation;

namespace LS.Application.Features.Shared.Payments.CommandHandlers;

internal sealed class ProcessPaymentWebhookCommandValidator : AbstractValidator<ProcessPaymentWebhookCommand>
{
    public ProcessPaymentWebhookCommandValidator()
    {
        RuleFor(command => command.Provider)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.Payload)
            .NotEmpty();

        RuleFor(command => command.SignatureHeader)
            .NotEmpty();
    }
}

