using LS.Domain.Features.Accounting.Contracts;
using LS.Application.Features.Accounting.Contracts.Interfaces;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.Domain.Features.Accounting.Entities;
using LS.Application.Features.Loans.IntegrationEvents;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LS.Infrastructure.Messaging.Consumers.Accounting;

public class LoanDisbursedIntegrationEventHandler(
    IAccountingUnitOfWork unitOfWork,
    ILedgerService ledgerService,
    ILogger<LoanDisbursedIntegrationEventHandler> logger)
    : AccountingEventHandlerBase<LoanDisbursedIntegrationEvent>(unitOfWork, ledgerService, logger)
{
    protected override string GetTransactionTypeCode(LoanDisbursedIntegrationEvent evt) => "LOAN_DISBURSEMENT";
    protected override string GetReferenceNumber(LoanDisbursedIntegrationEvent evt) => evt.LoanNumber;
    protected override string GetDescription(LoanDisbursedIntegrationEvent evt) => $"Loan Disbursement for Application {evt.LoanApplicationId}";
    protected override Guid GetTenantId(LoanDisbursedIntegrationEvent evt) => evt.TenantId;

    protected override Task<IEnumerable<CreateJournalEntryDto>> CreateJournalEntriesAsync(LoanDisbursedIntegrationEvent evt, TransactionTypeGlMapping mapping)
    {
        var entries = new List<CreateJournalEntryDto>();

        var debitAccountId = ResolveAccountId(mapping.DebitSideSource, mapping.DebitExplicitGlAccountId, evt.PaymentChannelGlAccountId, null);
        entries.Add(new CreateJournalEntryDto { AccountId = debitAccountId, Debit = evt.Amount, Credit = 0, Description = $"Debit for Loan Disbursement", BranchId = evt.BranchId, CostCenterId = evt.CostCenterId });

        var creditAccountId = ResolveAccountId(mapping.CreditSideSource, mapping.CreditExplicitGlAccountId, evt.PaymentChannelGlAccountId, null);
        entries.Add(new CreateJournalEntryDto { AccountId = creditAccountId, Debit = 0, Credit = evt.Amount, Description = $"Credit for Loan Disbursement", BranchId = evt.BranchId, CostCenterId = evt.CostCenterId });

        if (evt.FeeComponent > 0)
        {
            var feeAccountId = ResolveAccountId(mapping.FeeSideSource, mapping.FeeExplicitGlAccountId, evt.PaymentChannelGlAccountId, null);
            // Assuming fees are deducted from disbursement or added to a separate income
            entries.Add(new CreateJournalEntryDto { AccountId = feeAccountId, Debit = 0, Credit = evt.FeeComponent, Description = $"Credit for Loan Fee", BranchId = evt.BranchId, CostCenterId = evt.CostCenterId });
        }

        return Task.FromResult<IEnumerable<CreateJournalEntryDto>>(entries);
    }
}
