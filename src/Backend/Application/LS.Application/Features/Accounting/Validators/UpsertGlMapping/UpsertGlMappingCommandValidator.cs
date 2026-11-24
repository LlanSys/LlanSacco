using FluentValidation;
using LS.Application.Features.Accounting.Commands;
using LS.Domain.Features.Accounting.Entities;

namespace LS.Application.Features.Accounting.Validators.UpsertGlMapping;

internal sealed class UpsertGlMappingCommandValidator : AbstractValidator<UpsertGlMappingCommand>
{
    public UpsertGlMappingCommandValidator()
    {
        RuleFor(x => x.TransactionTypeCode)
            .NotEmpty()
            .MaximumLength(64)
            .Matches("^[A-Z0-9_]+$")
            .WithMessage("Transaction type code must use uppercase letters, digits and underscores.");

        RuleFor(x => x.Request).NotNull();

        When(
            x => x.Request is not null,
            () =>
            {
                RuleFor(x => x.Request.Description).MaximumLength(250);

                AddSideRules("Debit", x => x.Request.DebitSideSource, x => x.Request.DebitExplicitGlAccountId);
                AddSideRules("Credit", x => x.Request.CreditSideSource, x => x.Request.CreditExplicitGlAccountId);
                AddSideRules("Fee", x => x.Request.FeeSideSource, x => x.Request.FeeExplicitGlAccountId);

                RuleFor(x => x.Request.CreditExplicitGlAccountId)
                    .Must((command, creditId) => creditId != command.Request.DebitExplicitGlAccountId)
                    .When(x => x.Request.DebitExplicitGlAccountId.HasValue && x.Request.CreditExplicitGlAccountId.HasValue)
                    .WithMessage("Debit and credit must use different ledger accounts.");

                RuleFor(x => x.Request)
                    .Must(request => request.DebitSideSource == GlMappingSources.ExplicitGl
                        && request.CreditSideSource == GlMappingSources.ExplicitGl)
                    .When(x => string.Equals(x.TransactionTypeCode, GlMappingSources.GuarantorRecoveryTransactionType, StringComparison.Ordinal))
                    .WithName("Request")
                    .WithMessage("Guarantor recovery must debit and credit explicit ledger accounts.");
            });
    }

    private void AddSideRules(
        string side,
        System.Linq.Expressions.Expression<Func<UpsertGlMappingCommand, string>> source,
        System.Linq.Expressions.Expression<Func<UpsertGlMappingCommand, Guid?>> explicitId)
    {
        var sourceOf = source.Compile();

        RuleFor(source)
            .NotEmpty()
            .Must(GlMappingSources.IsSupported)
            .WithName($"{side}SideSource")
            .WithMessage($"{side} side source must be one of {string.Join(", ", GlMappingSources.All)}.");

        RuleFor(explicitId)
            .NotNull()
            .When(x => sourceOf(x) == GlMappingSources.ExplicitGl)
            .WithName($"{side}ExplicitGlAccountId")
            .WithMessage($"{side} explicit ledger account is required when the source is {GlMappingSources.ExplicitGl}.");

        RuleFor(explicitId)
            .Must(id => id is null || id != Guid.Empty)
            .When(x => sourceOf(x) == GlMappingSources.ExplicitGl)
            .WithName($"{side}ExplicitGlAccountId")
            .WithMessage($"{side} explicit ledger account is required when the source is {GlMappingSources.ExplicitGl}.");

        RuleFor(explicitId)
            .Null()
            .When(x => sourceOf(x) != GlMappingSources.ExplicitGl)
            .WithName($"{side}ExplicitGlAccountId")
            .WithMessage($"{side} explicit ledger account must be empty unless the source is {GlMappingSources.ExplicitGl}.");
    }
}
