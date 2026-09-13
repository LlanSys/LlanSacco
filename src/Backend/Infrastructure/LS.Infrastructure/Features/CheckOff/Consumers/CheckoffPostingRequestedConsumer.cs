using LS.Application.Features.CheckOff.IntegrationEvents;
using LS.Application.Features.CheckOff.Jobs;
using LS.Infrastructure.Contracts.Implementations.Common;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace LS.Infrastructure.Features.CheckOff.Consumers;

public sealed class CheckoffPostingRequestedConsumer(IServiceScopeFactory scopeFactory) : IConsumer<CheckoffPostingRequested>
{
    public async Task Consume(ConsumeContext<CheckoffPostingRequested> context)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<BackgroundExecutionContext>()
            .Initialize(context.Message.TenantId, context.Message.ActorId);
        var job = ActivatorUtilities.CreateInstance<MasterCheckoffBatchJob>(scope.ServiceProvider);
        await job.ExecuteAsync(context.Message.BatchId, context.CancellationToken);
    }
}
