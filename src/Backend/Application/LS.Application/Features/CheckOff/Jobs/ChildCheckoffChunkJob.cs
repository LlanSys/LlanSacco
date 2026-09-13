using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.Banking.Savings.Commands;
using LS.Application.Features.Banking.Shares.Commands;
using LS.Application.Features.Loans.LoanRepayments.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Enums;
using LS.Domain.Features.Loans.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Features.Banking.Savings.Dtos;

namespace LS.Application.Features.CheckOff.Jobs;

public class ChildCheckoffChunkJob(ICheckOffUnitOfWork checkOffUnitOfWork,
    IBankingUnitOfWork bankingUnitOfWork, ILoansUnitOfWork loansUnitOfWork,
    IBackgroundRequestSender sender, ICurrentTenantProvider tenantProvider)
{
    public async Task ExecuteAsync(Guid batchId, List<Guid> rowIds, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;
        if (tenantId == Guid.Empty) throw new LS.Domain.Shared.Exceptions.TenantNotResolvedException();
        foreach (var rowId in rowIds)
        {
            await checkOffUnitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var row = await checkOffUnitOfWork.CheckoffStagingRows.FirstOrDefaultAsync(
                    r => r.Id == rowId && r.CheckoffBatchId == batchId && r.TenantId == tenantId, cancellationToken)
                    ?? throw new InvalidOperationException("Checkoff row was not found.");
                if (row.Status == CheckoffRowStatus.Processed) return true;
                if (row.Status != CheckoffRowStatus.Validated || row.ResolvedMemberId is not Guid memberId || memberId == Guid.Empty)
                    throw new InvalidOperationException("Checkoff row is not validated.");
                if (row.AmountForSavings < 0 || row.AmountForShares < 0 || row.AmountForLoans < 0 || row.TotalDeducted <= 0 ||
                    row.TotalDeducted != row.AmountForSavings + row.AmountForShares + row.AmountForLoans)
                    throw new InvalidOperationException("Checkoff row amounts are inconsistent.");
                var allocations = await checkOffUnitOfWork.CheckoffStagingLoanAllocations.ListAsync(
                    q => q.Where(a => a.TenantId == tenantId && a.CheckoffStagingRowId == rowId), cancellationToken);
                if (allocations.Any(a => a.Amount <= 0) || allocations.Sum(a => a.Amount) != row.AmountForLoans)
                    throw new InvalidOperationException("Checkoff loan allocations do not match the deduction.");
                foreach (var allocation in allocations)
                {
                    var loan = await loansUnitOfWork.LoanApplicationRepository.FirstOrDefaultAsync(
                        l => l.Id == allocation.LoanId && l.TenantId == tenantId && l.MemberId == memberId, cancellationToken);
                    if (loan is null) throw new InvalidOperationException("Checkoff loan does not belong to the resolved member.");
                }
                if (row.AmountForSavings > 0)
                {
                    var reference = $"CHK:{row.Id:N}:S";
                    var prior = await bankingUnitOfWork.SavingsTransactions.FirstOrDefaultAsync(
                        t => t.TenantId == tenantId && t.ExternalReferenceId == reference, cancellationToken);
                    Guid productId;
                    if (prior is not null)
                    {
                        var account = await bankingUnitOfWork.SavingsAccounts.FirstOrDefaultAsync(
                            a => a.TenantId == tenantId && a.Id == prior.SavingsAccountId, cancellationToken)
                            ?? throw new InvalidOperationException("Original savings account was not found.");
                        productId = account.SavingsProductId;
                    }
                    else
                    {
                        var products = await bankingUnitOfWork.SavingsProducts.ListAsync(q => q
                            .Where(p => p.TenantId == tenantId && p.IsActive && p.IsDefaultCheckoffTarget).Take(2), cancellationToken);
                        if (products.Count != 1) throw new InvalidOperationException("Exactly one active checkoff savings product is required.");
                        productId = products.Single().Id;
                    }
                    var result = await sender.SendAsync(new DepositSavingsCommand(new DepositSavingsRequest(
                        memberId, productId, row.AmountForSavings, "Checkoff savings payment", reference)), cancellationToken);
                    if (!result.IsSuccess) throw new InvalidOperationException("Checkoff savings payment was rejected.");
                }
                if (row.AmountForShares > 0)
                {
                    var reference = $"CHK:{row.Id:N}:H";
                    var prior = await bankingUnitOfWork.ShareTransactions.FirstOrDefaultAsync(
                        t => t.TenantId == tenantId && t.ReferenceNumber == reference, cancellationToken);
                    Guid productId;
                    if (prior is not null)
                    {
                        var account = await bankingUnitOfWork.ShareAccounts.FirstOrDefaultAsync(
                            a => a.TenantId == tenantId && a.Id == prior.ShareAccountId, cancellationToken)
                            ?? throw new InvalidOperationException("Original share account was not found.");
                        productId = account.ShareProductId;
                    }
                    else
                    {
                        var products = await bankingUnitOfWork.ShareProducts.ListAsync(q => q
                            .Where(p => p.TenantId == tenantId && p.IsActive).Take(2), cancellationToken);
                        if (products.Count != 1) throw new InvalidOperationException("Exactly one active checkoff share product is required.");
                        productId = products.Single().Id;
                    }
                    var result = await sender.SendAsync(new PurchaseSharesCommand(memberId, productId,
                        row.AmountForShares, "Checkoff share payment", reference), cancellationToken);
                    if (!result.IsSuccess) throw new InvalidOperationException("Checkoff share payment was rejected.");
                }
                foreach (var allocation in allocations)
                {
                    var result = await sender.SendAsync(new ProcessRepaymentCommand
                    {
                        LoanApplicationId = allocation.LoanId, Amount = allocation.Amount,
                        ReceiptNumber = $"CHK:{allocation.Id:N}"
                    }, cancellationToken);
                    if (!result.IsSuccess) throw new InvalidOperationException("Checkoff loan payment was rejected.");
                }
                row.Status = CheckoffRowStatus.Processed;
                row.ExceptionReason = null;
                await checkOffUnitOfWork.CheckoffStagingRows.UpdateAsync(row, cancellationToken);
                return true;
            }, cancellationToken);
        }
    }
}
