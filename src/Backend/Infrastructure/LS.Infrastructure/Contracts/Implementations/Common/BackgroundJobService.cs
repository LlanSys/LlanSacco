using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Jobs;
using LS.Infrastructure.Logging;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Text.Json;
using TriggerBuilder = Quartz.TriggerBuilder;

namespace LS.Infrastructure.Contracts.Implementations.Common;

/// <summary>
/// Queues MediatR requests on Quartz. Each job records the tenant and actor that queued it so it runs in that
/// workspace (see <see cref="MediatorSerializedJob"/>), never in a default one.
/// </summary>
internal sealed class BackgroundJobService(
    ISchedulerFactory schedulerFactory,
    ILogger<BackgroundJobService> logger,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider) : IBackgroundJobService
{
    public void Enqueue(IRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var job = BuildJob(request);
            var trigger = TriggerBuilder.Create().StartNow().Build();

            _ = Task.Run(async () =>
            {
                var scheduler = await schedulerFactory.GetScheduler().ConfigureAwait(false);
                await scheduler.ScheduleJob(job, trigger).ConfigureAwait(false);
            });
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogBackgroundJobEnqueueError(logger, request.GetType().Name, ex);
            throw;
        }
    }

    public async Task EnqueueAsync(IRequest? request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var job = BuildJob(request);
        var trigger = TriggerBuilder.Create().StartNow().Build();
        var scheduler = await schedulerFactory.GetScheduler(ct).ConfigureAwait(false);
        await scheduler.ScheduleJob(job, trigger, cancellationToken: ct).ConfigureAwait(false);
    }

    internal IJobDetail BuildJob(IRequest request)
    {
        var type = request.GetType();

        // Read the scope now: the caller's request (and its tenant) is gone by the time the job runs.
        return JobBuilder.Create<MediatorSerializedJob>()
            .WithIdentity($"{type.Name}_{Guid.CreateVersion7()}", "MediatRGroup")
            .UsingJobData(MediatorSerializedJob.CommandDataKey, JsonSerializer.Serialize(request, type))
            .UsingJobData(MediatorSerializedJob.CommandTypeKey, type.AssemblyQualifiedName!)
            .UsingJobData(MediatorSerializedJob.TenantIdKey, tenantProvider.TenantId.ToString())
            .UsingJobData(MediatorSerializedJob.ActorIdKey, actorProvider.ActorId)
            .Build();
    }
}
