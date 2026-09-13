using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Enums;
using LS.Domain.Shared.Contracts.Common;

namespace LS.Application.Features.CheckOff.Jobs;

public class MasterCheckoffBatchJob(ICheckOffUnitOfWork unitOfWork,
    ChildCheckoffChunkJob childJob, ICurrentTenantProvider tenantProvider)
{
    public async Task ExecuteAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;
        if (tenantId == Guid.Empty) throw new LS.Domain.Shared.Exceptions.TenantNotResolvedException();
        var batch = await unitOfWork.CheckoffBatches.FirstOrDefaultAsync(b => b.Id == batchId && b.TenantId == tenantId, cancellationToken)
            ?? throw new InvalidOperationException("Checkoff batch was not found.");
        if (batch.Status == CheckoffBatchStatus.Posted) return;
        if (batch.Status != CheckoffBatchStatus.Posting) throw new InvalidOperationException("Checkoff batch is not ready for posting.");
        while (true)
        {
            var ids = await unitOfWork.CheckoffStagingRows.ListAsync(q => q
                .Where(r => r.TenantId == tenantId && r.CheckoffBatchId == batchId && r.Status == CheckoffRowStatus.Validated)
                .OrderBy(r => r.Id).Take(100).Select(r => r.Id), cancellationToken);
            if (ids.Count == 0) break;
            await childJob.ExecuteAsync(batchId, ids.ToList(), cancellationToken);
        }
        var incomplete = await unitOfWork.CheckoffStagingRows.ListAsync(q => q
            .Where(r => r.TenantId == tenantId && r.CheckoffBatchId == batchId && r.Status != CheckoffRowStatus.Processed)
            .Take(1).Select(r => r.Id), cancellationToken);
        if (incomplete.Count != 0) throw new InvalidOperationException("Checkoff batch contains unprocessed rows.");
        batch.Status = CheckoffBatchStatus.Posted;
        await unitOfWork.CheckoffBatches.UpdateAsync(batch, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
    }
}
