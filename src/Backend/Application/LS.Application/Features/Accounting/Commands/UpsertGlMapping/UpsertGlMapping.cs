using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Features.Accounting.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using MediatR;

namespace LS.Application.Features.Accounting.Commands;

public sealed record UpsertGlMappingCommand(string TransactionTypeCode, UpsertGlMappingRequest Request)
    : IRequest<AppResponse<Guid>>;

internal sealed class UpsertGlMappingCommandHandler(
    IAccountingUnitOfWork unitOfWork,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider)
    : IRequestHandler<UpsertGlMappingCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(UpsertGlMappingCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = tenantProvider.TenantId;

        var accountIds = new[] { request.DebitExplicitGlAccountId, request.CreditExplicitGlAccountId, request.FeeExplicitGlAccountId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        var accounts = accountIds.Length == 0
            ? []
            : await unitOfWork.AccountRepository
                .ListAsync(q => q.Where(a => a.TenantId == tenantId && accountIds.Contains(a.Id)), cancellationToken)
                .ConfigureAwait(false);

        var accountRejection = ValidateAccounts(command, accounts);

        if (accountRejection is not null)
        {
            return AppResponses.Failure<Guid>(accountRejection);
        }

        var mapping = await unitOfWork.TransactionTypeGlMappingRepository
            .FirstOrDefaultAsync(m => m.TenantId == tenantId && m.TransactionTypeCode == command.TransactionTypeCode, cancellationToken)
            .ConfigureAwait(false);

        var actorId = actorProvider.ActorId;

        if (mapping is null)
        {
            mapping = new TransactionTypeGlMapping
            {
                TenantId = tenantId,
                TransactionTypeCode = command.TransactionTypeCode,
                DebitSideSource = request.DebitSideSource,
                CreditSideSource = request.CreditSideSource,
                FeeSideSource = request.FeeSideSource,
                CreatedBy = actorId
            };

            Apply(mapping, request);
            await unitOfWork.TransactionTypeGlMappingRepository.CreateAsync(mapping, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            Apply(mapping, request);
            mapping.UpdatedAt = DateTimeOffset.UtcNow;
            mapping.UpdatedBy = actorId;
            await unitOfWork.TransactionTypeGlMappingRepository.UpdateAsync(mapping, cancellationToken).ConfigureAwait(false);
        }

        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);
        return AppResponses.Success("GL mapping saved.", mapping.Id);
    }

    private static AppError? ValidateAccounts(UpsertGlMappingCommand command, IReadOnlyCollection<Account> accounts)
    {
        var request = command.Request;

        foreach (var (side, id) in new[]
                 {
                     ("Debit", request.DebitExplicitGlAccountId),
                     ("Credit", request.CreditExplicitGlAccountId),
                     ("Fee", request.FeeExplicitGlAccountId)
                 })
        {
            if (!id.HasValue)
            {
                continue;
            }

            var account = accounts.FirstOrDefault(a => a.Id == id.Value);

            if (account is null)
            {
                return AppError.NotFound($"{side} ledger account was not found.");
            }

            if (!account.IsActive)
            {
                return AppError.BusinessRule($"{side} ledger account is inactive.");
            }
        }

        if (!string.Equals(command.TransactionTypeCode, GlMappingSources.GuarantorRecoveryTransactionType, StringComparison.Ordinal))
        {
            return null;
        }

        // Same rule GuarantorRecoveryCompletedIntegrationEventHandler enforces at posting time.
        var debit = accounts.FirstOrDefault(a => a.Id == request.DebitExplicitGlAccountId);
        var credit = accounts.FirstOrDefault(a => a.Id == request.CreditExplicitGlAccountId);

        if (request.DebitSideSource != GlMappingSources.ExplicitGl || request.CreditSideSource != GlMappingSources.ExplicitGl ||
            debit is null || credit is null || debit.Id == credit.Id ||
            debit.AccountType != AccountType.Liability || credit.AccountType != AccountType.Asset)
        {
            return AppError.BusinessRule(
                "Guarantor recovery must debit an active deposit liability and credit an active loan principal asset.");
        }

        return null;
    }

    private static void Apply(TransactionTypeGlMapping mapping, UpsertGlMappingRequest request)
    {
        mapping.DebitSideSource = request.DebitSideSource;
        mapping.DebitExplicitGlAccountId = request.DebitExplicitGlAccountId;
        mapping.CreditSideSource = request.CreditSideSource;
        mapping.CreditExplicitGlAccountId = request.CreditExplicitGlAccountId;
        mapping.FeeSideSource = request.FeeSideSource;
        mapping.FeeExplicitGlAccountId = request.FeeExplicitGlAccountId;
        mapping.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
    }
}
