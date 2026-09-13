using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Domain.Features.ControlPlane.Tenants.Events;
using LS.Domain.Features.IAM.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LS.Infrastructure.Logging;

namespace LS.Infrastructure.Features.IAM.Users.EventHandlers;

internal sealed class TenantInfrastructureChangedEventHandlers(
    IIamUnitOfWork iamUnitOfWork,
    ISessionService sessionService,
    ILogger<TenantInfrastructureChangedEventHandlers> logger) : 
    INotificationHandler<TenantStampChangedDomainEvent>,
    INotificationHandler<TenantModuleRevokedDomainEvent>
{
    private readonly IIamUnitOfWork _iamUnitOfWork = iamUnitOfWork;
    private readonly ISessionService _sessionService = sessionService;
    private readonly ILogger<TenantInfrastructureChangedEventHandlers> _logger = logger;

    public async Task Handle(TenantStampChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        ControlPlaneLogDefinitions.LogTenantStampChangedRevokingSessions(_logger, notification.TenantId);
        await RevokeAllTenantSessionsAsync(notification.TenantId, cancellationToken).ConfigureAwait(false);
    }

    public async Task Handle(TenantModuleRevokedDomainEvent notification, CancellationToken cancellationToken)
    {
        ControlPlaneLogDefinitions.LogTenantModuleRevokedRevokingSessions(_logger, notification.TenantId, notification.ModuleKey);
        await RevokeAllTenantSessionsAsync(notification.TenantId, cancellationToken).ConfigureAwait(false);
    }

    private async Task RevokeAllTenantSessionsAsync(System.Guid tenantId, CancellationToken cancellationToken)
    {
        var users = await _iamUnitOfWork.UserRepository.FindAll()
            .Where(u => u.TenantId == tenantId && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var userId in users)
        {
            await _sessionService.RevokeAllUserSessionsAsync(userId, null, cancellationToken).ConfigureAwait(false);
        }
    }
}
