using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Enums;
using LS.Infrastructure.Contracts.Implementations.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Text.Json;

namespace LS.Infrastructure.Jobs;

/// <summary>
/// Runs a queued MediatR request in the tenant and actor that queued it. A job without a verified tenant,
/// or whose tenant is no longer Active, is refused rather than run in a default workspace.
/// </summary>
[DisallowConcurrentExecution]
public class MediatorSerializedJob(IServiceProvider serviceProvider) : IJob
{
    public const string CommandDataKey = "CommandData";
    public const string CommandTypeKey = "CommandType";
    public const string TenantIdKey = "TenantId";
    public const string ActorIdKey = "ActorId";

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var data = context.MergedJobDataMap.GetString(CommandDataKey);
        var typeName = context.MergedJobDataMap.GetString(CommandTypeKey);

        if (string.IsNullOrWhiteSpace(data) || string.IsNullOrWhiteSpace(typeName)) return;

        var type = Type.GetType(typeName);
        if (type == null)
        {
            return;
        }

        if (JsonSerializer.Deserialize(data, type) is not IRequest request)
        {
            return;
        }

        if (!Guid.TryParse(context.MergedJobDataMap.GetString(TenantIdKey), out var tenantId) || tenantId == Guid.Empty)
        {
            throw new JobExecutionException("The queued operation has no verified workspace assignment.");
        }

        var actorId = context.MergedJobDataMap.GetString(ActorIdKey);
        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new JobExecutionException("The queued operation has no recorded actor.");
        }

        await using var scope = serviceProvider.CreateAsyncScope();
        var controlPlane = scope.ServiceProvider.GetRequiredService<IControlPlaneUnitOfWork>();
        if (!await controlPlane.TenantRepository.AnyAsync(
                tenant => tenant.Id == tenantId && tenant.Status == TenantStatus.Active, cancellationToken).ConfigureAwait(false))
        {
            throw new JobExecutionException("The queued workspace is unavailable.");
        }

        scope.ServiceProvider.GetRequiredService<BackgroundExecutionContext>().Initialize(tenantId, actorId);
        await scope.ServiceProvider.GetRequiredService<ISender>().Send(request, cancellationToken).ConfigureAwait(false);
    }
}
