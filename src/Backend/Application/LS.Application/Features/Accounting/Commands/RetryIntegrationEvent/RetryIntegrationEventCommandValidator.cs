using FluentValidation;

namespace LS.Application.Features.Accounting.Commands;

internal class RetryIntegrationEventCommandValidator : AbstractValidator<RetryIntegrationEventCommand>
{
    public RetryIntegrationEventCommandValidator()
    {
        RuleFor(x => x.ErrorId).NotEmpty();
    }
}

