using Hangfire;
using LS.Domain.Features.CheckOff.Contracts;
using LS.Domain.Features.CheckOff.Enums;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.CheckOff.Jobs;

public class MasterCheckoffBatchJob(
    ICheckOffUnitOfWork unitOfWork,
    IBackgroundJobClient backgroundJobClient,
    ILogger<MasterCheckoffBatchJob> logger)
{
    public async Task ExecuteAsync(Guid batchId, CancellationToken cancellationToken)
    {
        logger.LogInformation("MasterCheckoffBatchJob started for BatchId: {BatchId}", batchId);

        var rowIds = await unitOfWork.CheckoffStagingRows.ListAsync(
            q => q.Where(r => r.CheckoffBatchId == batchId && r.Status == CheckoffRowStatus.Pending)
                  .Select(r => r.Id),
            cancellationToken);

        if (rowIds.Count == 0)
        {
            logger.LogInformation("No pending rows found for BatchId: {BatchId}", batchId);
            return;
        }

        int chunkSize = 100;
        for (int i = 0; i < rowIds.Count; i += chunkSize)
        {
            var chunk = rowIds.Skip(i).Take(chunkSize).ToList();
            backgroundJobClient.Enqueue<ChildCheckoffChunkJob>(j => j.ExecuteAsync(batchId, chunk, CancellationToken.None));
        }

        logger.LogInformation("MasterCheckoffBatchJob completed. Queued {ChunkCount} chunks for BatchId: {BatchId}", Math.Ceiling((double)rowIds.Count / chunkSize), batchId);
    }
}
