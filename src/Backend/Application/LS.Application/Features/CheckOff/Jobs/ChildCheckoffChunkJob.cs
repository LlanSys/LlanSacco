using LS.Application.Features.Banking.Savings.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Jobs;

public class ChildCheckoffChunkJob(
    ICheckOffUnitOfWork checkOffUnitOfWork,
    IBankingUnitOfWork bankingUnitOfWork,
    ISender sender,
    ILogger<ChildCheckoffChunkJob> logger)
{
    public async Task ExecuteAsync(Guid batchId, List<Guid> rowIds, CancellationToken cancellationToken)
    {
        logger.LogInformation("ChildCheckoffChunkJob started for BatchId: {BatchId}, processing {RowCount} rows.", batchId, rowIds.Count);

        foreach (var rowId in rowIds)
        {
            await checkOffUnitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var row = await checkOffUnitOfWork.CheckoffStagingRows.FirstOrDefaultAsync(
                    r => r.Id == rowId, 
                    cancellationToken);

                if (row == null || row.Status != CheckoffRowStatus.Pending)
                    return false;

                var allocations = await checkOffUnitOfWork.CheckoffStagingLoanAllocations.ListAsync(
                    q => q.Where(a => a.CheckoffStagingRowId == rowId), 
                    cancellationToken);

                row.LoanAllocations = allocations;

                if (row.ResolvedMemberId == null)
                {
                    row.Status = CheckoffRowStatus.Failed;
                    row.ExceptionReason = "Member not resolved.";
                    await checkOffUnitOfWork.CheckoffStagingRows.UpdateAsync(row, cancellationToken);
                    return false;
                }

                try
                {
                    // 1. Savings
                    if (row.AmountForSavings > 0)
                    {
                        var defaultSavingsProduct = await bankingUnitOfWork.SavingsProducts
                            .FirstOrDefaultAsync(p => p.IsDefaultCheckoffTarget, cancellationToken);

                        if (defaultSavingsProduct != null)
                        {
                            var account = await bankingUnitOfWork.SavingsAccounts
                                .FirstOrDefaultAsync(a => a.MemberId == row.ResolvedMemberId && a.SavingsProductId == defaultSavingsProduct.Id, cancellationToken);

                            if (account != null)
                            {
                                await sender.Send(new DepositSavingsCommand(new LS.SharedKernel.Features.Banking.Savings.Dtos.DepositSavingsRequest(
                                    row.ResolvedMemberId.Value,
                                    account.SavingsProductId,
                                    row.AmountForSavings,
                                    $"Checkoff Deposit - Batch {batchId}",
                                    null
                                )), cancellationToken);
                            }
                        }
                    }

                    // 2. Shares
                    if (row.AmountForShares > 0)
                    {
                        // Logic for shares
                    }

                    // 3. Loans
                    if (row.AmountForLoans > 0 && row.LoanAllocations.Any())
                    {
                        foreach (var allocation in row.LoanAllocations)
                        {
                            if (allocation.Amount > 0)
                            {
                                await sender.Send(new LS.Application.Features.Loans.LoanRepayments.Commands.ProcessRepaymentCommand
                                {
                                    LoanApplicationId = allocation.LoanId,
                                    Amount = allocation.Amount,
                                    ReceiptNumber = $"CHK-{batchId.ToString()[..8]}"
                                }, cancellationToken);
                            }
                        }
                    }

                    row.Status = CheckoffRowStatus.Processed;
                    await checkOffUnitOfWork.CheckoffStagingRows.UpdateAsync(row, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing CheckoffStagingRow {RowId}", rowId);
                    row.Status = CheckoffRowStatus.Failed;
                    row.ExceptionReason = ex.Message;
                    await checkOffUnitOfWork.CheckoffStagingRows.UpdateAsync(row, cancellationToken);
                }

                return true;
            }, cancellationToken);
        }
        
        logger.LogInformation("ChildCheckoffChunkJob completed for BatchId: {BatchId}.", batchId);
    }
}
