using LS.Application.Features.Accounting.Contracts.Interfaces;
using LS.Application.Features.Loans.IntegrationEvents;
using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Features.Accounting.Enums;
using LS.Infrastructure.Contracts.Implementations.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using Microsoft.Extensions.Logging;

namespace LS.Infrastructure.Messaging.Consumers.Accounting;

public sealed class GuarantorRecoveryCompletedIntegrationEventHandler(IAccountingUnitOfWork unitOfWork,
    ILedgerService ledger, ILogger<GuarantorRecoveryCompletedIntegrationEventHandler> logger, BackgroundExecutionContext executionContext)
    : AccountingEventHandlerBase<GuarantorRecoveryCompletedIntegrationEvent>(unitOfWork, ledger, logger, executionContext)
{
    protected override string GetTransactionTypeCode(GuarantorRecoveryCompletedIntegrationEvent evt) => "GUARANTOR_RECOVERY";
    protected override string GetReferenceNumber(GuarantorRecoveryCompletedIntegrationEvent evt) => evt.ReceiptNumber;
    protected override string GetDescription(GuarantorRecoveryCompletedIntegrationEvent evt) => "Approved guarantor principal recovery";
    protected override Guid GetTenantId(GuarantorRecoveryCompletedIntegrationEvent evt) => evt.TenantId;

    protected override async Task<string?> ValidateMappingAsync(
        GuarantorRecoveryCompletedIntegrationEvent evt,
        TransactionTypeGlMapping mapping,
        CancellationToken cancellationToken)
    {
        if (evt.Amount <= 0 || mapping.DebitSideSource != "EXPLICIT_GL" || mapping.CreditSideSource != "EXPLICIT_GL" ||
            !mapping.DebitExplicitGlAccountId.HasValue || !mapping.CreditExplicitGlAccountId.HasValue ||
            mapping.DebitExplicitGlAccountId == mapping.CreditExplicitGlAccountId)
        {
            return "Guarantor recovery requires its own approved deposit-liability and loan-principal mapping.";
        }

        var debit = mapping.DebitExplicitGlAccountId.Value;
        var credit = mapping.CreditExplicitGlAccountId.Value;
        var accounts = await _unitOfWork.AccountRepository.ListAsync(q => q.Where(x => x.Id == debit || x.Id == credit), cancellationToken).ConfigureAwait(false);

        if (accounts.Count != 2 || !accounts.Any(x => x.Id == debit && x.IsActive && x.AccountType == AccountType.Liability) ||
            !accounts.Any(x => x.Id == credit && x.IsActive && x.AccountType == AccountType.Asset))
        {
            return "Guarantor recovery must debit an active deposit liability and credit an active loan principal asset.";
        }

        return null;
    }

    protected override Task<IEnumerable<CreateJournalEntryRequest>> CreateJournalEntriesAsync(
        GuarantorRecoveryCompletedIntegrationEvent evt, TransactionTypeGlMapping mapping)
    {
        // This is an internal offset, not a cash receipt. Finance must approve its own mapping.
        var savingsLiability = ResolveAccountId(mapping.DebitSideSource, mapping.DebitExplicitGlAccountId, null, null);
        var loanPrincipal = ResolveAccountId(mapping.CreditSideSource, mapping.CreditExplicitGlAccountId, null, null);
        IEnumerable<CreateJournalEntryRequest> entries =
        [
            new() { AccountId = savingsLiability, Debit = evt.Amount, Credit = 0, Description = "Reduce guarantor deposit liability" },
            new() { AccountId = loanPrincipal, Debit = 0, Credit = evt.Amount, Description = "Reduce borrower principal receivable" }
        ];

        return Task.FromResult(entries);
    }
}
