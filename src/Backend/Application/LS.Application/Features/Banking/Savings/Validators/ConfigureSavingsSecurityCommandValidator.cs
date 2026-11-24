using FluentValidation;
using LS.Application.Features.Banking.Savings.Commands;

namespace LS.Application.Features.Banking.Savings.Validators;

internal sealed class ConfigureSavingsSecurityCommandValidator : AbstractValidator<ConfigureSavingsSecurityCommand>
{
    public ConfigureSavingsSecurityCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () => RuleFor(x => x.Request.EvidenceReference).NotEmpty().MaximumLength(200));
    }
}
