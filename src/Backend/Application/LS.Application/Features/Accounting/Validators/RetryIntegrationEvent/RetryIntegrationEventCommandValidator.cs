using FluentValidation;
using LS.Application.Features.Accounting.Commands;

namespace LS.Application.Features.Accounting.Validators.RetryIntegrationEvent;

internal class RetryIntegrationEventCommandValidator : AbstractValidator<RetryIntegrationEventCommand>
{
    public RetryIntegrationEventCommandValidator()
    {
        RuleFor(x => x.ErrorId).NotEmpty();
    }
}
