using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.HR.Employees.IntegrationEvents;
using LS.Application.Features.IAM.Users.IntegrationEvents;
using LS.Application.Utilities;
using LS.Domain.Features.IAM.Users.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LS.Application.Features.IAM.Users.EventHandlers;

internal sealed class AppUserCreatedEventHandler(IIntegrationEventPublisher integrationEventPublisher, ILogger<AppUserCreatedEventHandler> logger)
    : INotificationHandler<AppUserCreatedEvent>
{
    public async Task Handle(AppUserCreatedEvent evt, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(evt);

            var integrationEvent = new AppUserCreatedIntegrationEvent(
                evt.UserId,
                evt.TenantId,
                evt.EmployeeId,
                evt.UserName,
                evt.FullName,
                evt.Email);

            await integrationEventPublisher.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);

            LogDefinitions.LogAppUserCreatedIntegrationPublished(logger, evt.UserId);
                
        }
        catch (Exception ex)
        {
            LogDefinitions.LogAppUserCreatedIntegrationPublishFailed(logger, evt?.UserId ?? string.Empty, ex);
            throw;
        }
    }
}
