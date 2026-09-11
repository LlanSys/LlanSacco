using LS.Domain.Features.Accounting.Contracts;
using LS.Application.Features.Accounting.Contracts.Interfaces;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.Domain.Features.Accounting.Entities;
using LS.Application.Features.Membership.IntegrationEvents;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.Infrastructure.Messaging.Consumers.Accounting;

public class MemberTransactionPostedIntegrationEventHandler(
    IAccountingUnitOfWork unitOfWork,
    ILedgerService ledgerService,
    ILogger<MemberTransactionPostedIntegrationEventHandler> logger)
    : AccountingEventHandlerBase<MemberTransactionPostedIntegrationEvent>(unitOfWork, ledgerService, logger)
{
    protected override string GetTransactionTypeCode(MemberTransactionPostedIntegrationEvent evt) => evt.TransactionType.ToString();
    protected override string GetReferenceNumber(MemberTransactionPostedIntegrationEvent evt) => evt.Reference;
    protected override string GetDescription(MemberTransactionPostedIntegrationEvent evt) => evt.Notes ?? $"Auto-generated for Member Transaction {evt.MemberTransactionId}";
    protected override Guid GetTenantId(MemberTransactionPostedIntegrationEvent evt) => evt.TenantId;

    protected override Task<IEnumerable<CreateJournalEntryDto>> CreateJournalEntriesAsync(MemberTransactionPostedIntegrationEvent evt, TransactionTypeGlMapping mapping)
    {
        var entries = new List<CreateJournalEntryDto>();

        var debitAccountId = ResolveAccountId(mapping.DebitSideSource, mapping.DebitExplicitGlAccountId, evt.PaymentChannelGlAccountId, null);
        entries.Add(new CreateJournalEntryDto { AccountId = debitAccountId, Debit = evt.Amount, Credit = 0, Description = $"Debit for {evt.TransactionType}", BranchId = evt.BranchId, CostCenterId = evt.CostCenterId });

        var creditAccountId = ResolveAccountId(mapping.CreditSideSource, mapping.CreditExplicitGlAccountId, evt.PaymentChannelGlAccountId, null);
        entries.Add(new CreateJournalEntryDto { AccountId = creditAccountId, Debit = 0, Credit = evt.Amount, Description = $"Credit for {evt.TransactionType}", BranchId = evt.BranchId, CostCenterId = evt.CostCenterId });

        if (evt.FeeComponent > 0)
        {
            var feeAccountId = ResolveAccountId(mapping.FeeSideSource, mapping.FeeExplicitGlAccountId, evt.PaymentChannelGlAccountId, null);
            entries.Add(new CreateJournalEntryDto { AccountId = feeAccountId, Debit = 0, Credit = evt.FeeComponent, Description = $"Credit for Transaction Fee", BranchId = evt.BranchId, CostCenterId = evt.CostCenterId });
        }

        return Task.FromResult<IEnumerable<CreateJournalEntryDto>>(entries);
    }
}
