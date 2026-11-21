using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Enums;
using LS.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LS.Infrastructure.Messaging;

internal sealed partial class TenantOutboxRouting(
    IServiceScopeFactory scopeFactory, IOptions<OrgSettings> settings, ILogger<TenantOutboxRouting> logger)
    : BackgroundService
{
    private Guid[] _tenantIds = [];

    public Guid? NextTenant(ref int cursor)
    {
        var tenants = Volatile.Read(ref _tenantIds);
        return tenants.Length == 0 ? null : tenants[(uint)Interlocked.Increment(ref cursor) % tenants.Length];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var catalog = scope.ServiceProvider.GetRequiredService<IControlPlaneUnitOfWork>();
                var hostTenant = await catalog.TenantRepository.FindByIdAsync(settings.Value.DefaultTenantId, stoppingToken).ConfigureAwait(false);
                var tenants = hostTenant is null ? [] : await catalog.TenantRepository.ListAsync(q => q
                    .Where(t => t.DeploymentStampId == hostTenant.DeploymentStampId
                        && (t.Status == TenantStatus.Active || t.Status == TenantStatus.Suspended))
                    .OrderBy(t => t.Id).Select(t => t.Id), stoppingToken).ConfigureAwait(false);
                Volatile.Write(ref _tenantIds, tenants.ToArray());
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                // Stop using any stale routes until the catalog is reachable again.
                Volatile.Write(ref _tenantIds, []);
                LogCatalogUnavailable(logger, ex);
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }

    [LoggerMessage(EventId = 3313, Level = LogLevel.Warning, Message = "Could not refresh tenant outbox database routes")]
    private static partial void LogCatalogUnavailable(ILogger logger, Exception exception);
}
