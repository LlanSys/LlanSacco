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

public class LoanRepaymentIntegrationEventHandler(
    IAccountingUnitOfWork unitOfWork,
    ILedgerService ledgerService,
    ILogger<LoanRepaymentIntegrationEventHandler> logger)
    : AccountingEventHandlerBase<LoanRepaymentIntegrationEvent>(unitOfWork, ledgerService, logger)
{
    protected override string GetTransactionTypeCode(LoanRepaymentIntegrationEvent evt) => "LOAN_REPAYMENT";
    protected override string GetReferenceNumber(LoanRepaymentIntegrationEvent evt) => evt.ReceiptNumber;
    protected override string GetDescription(LoanRepaymentIntegrationEvent evt) => $"Loan Repayment for Application {evt.LoanApplicationId}";
    protected override Guid GetTenantId(LoanRepaymentIntegrationEvent evt) => evt.TenantId;

    protected override Task<IEnumerable<CreateJournalEntryDto>> CreateJournalEntriesAsync(LoanRepaymentIntegrationEvent evt, TransactionTypeGlMapping mapping)
    {
        var entries = new List<CreateJournalEntryDto>();

        var debitAccountId = ResolveAccountId(mapping.DebitSideSource, mapping.DebitExplicitGlAccountId, evt.PaymentChannelGlAccountId, null);
        entries.Add(new CreateJournalEntryDto { AccountId = debitAccountId, Debit = evt.Amount, Credit = 0, Description = $"Debit for Loan Repayment", BranchId = evt.BranchId, CostCenterId = evt.CostCenterId });

        var creditAccountId = ResolveAccountId(mapping.CreditSideSource, mapping.CreditExplicitGlAccountId, evt.PaymentChannelGlAccountId, null); // Product GL will come from product once integrated, for now null fallback will throw if PRODUCT_ACCOUNT is used and not provided.
        entries.Add(new CreateJournalEntryDto { AccountId = creditAccountId, Debit = 0, Credit = evt.PrincipalComponent + evt.InterestComponent, Description = $"Credit for Loan Repayment Principal/Interest", BranchId = evt.BranchId, CostCenterId = evt.CostCenterId });

        if (evt.PenaltyComponent > 0)
        {
            var penaltyAccountId = ResolveAccountId(mapping.FeeSideSource, mapping.FeeExplicitGlAccountId, evt.PaymentChannelGlAccountId, null);
            entries.Add(new CreateJournalEntryDto { AccountId = penaltyAccountId, Debit = 0, Credit = evt.PenaltyComponent, Description = $"Credit for Loan Penalty", BranchId = evt.BranchId, CostCenterId = evt.CostCenterId });
        }

        return Task.FromResult<IEnumerable<CreateJournalEntryDto>>(entries);
    }
}
