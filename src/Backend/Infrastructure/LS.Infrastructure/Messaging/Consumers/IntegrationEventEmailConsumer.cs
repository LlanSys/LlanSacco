using LS.SharedKernel.Features.Shared.Notifications.Dtos;
using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Application.Features.Shared.Notifications.Contracts.Interfaces;
using LS.SharedKernel.Extensions;
using LS.Application.Features.HR.Employees.IntegrationEvents;
using LS.Application.Features.IAM.Users.IntegrationEvents;
using LS.Application.Features.HR.Employees.Mappings;
using LS.Application.Features.IAM.Users.Mappings;
using LS.Application.Features.Shared.EmailTemplates.Mappings;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Entities;
using LS.Domain.Features.Shared.FailedMessages.Enums;

using LS.Infrastructure.Logging;
using LS.SharedKernel.Dtos.Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LS.Infrastructure.Messaging.Consumers;

public abstract class IntegrationEventEmailConsumer<TEvent>(
    IEmailComposer<TEvent> composer,
    ISharedUnitOfWork sharedUnitOfWork,
    ILogger<IntegrationEventEmailConsumer<TEvent>> logger) : IConsumer<TEvent> where TEvent : class, IIntegrationEvent

{
    public async Task Consume(ConsumeContext<TEvent> context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var evt = context.Message;
        var retryCount = context.GetRetryCount();
        var messageId = context.MessageId?.ToString() ?? Guid.CreateVersion7().ToString();

        try
        {
            var composed = await composer.ComposeAsync(evt, context.CancellationToken).ConfigureAwait(false);
            if (!composed.IsSuccess || composed.Data is null)
            {
                MessageBusLogDefinitions.LogEmailCompositionFailed(logger, messageId);
                return;
            }

            await context.Publish(new SendEmailRequest
            {
                To = composed.Data.RecipientEmail,
                Subject = composed.Data.Subject,
                Body = composed.Data.Body

            }, context.CancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (retryCount >= 4)
        {
            MessageBusLogDefinitions.LogPermanentEventFailure(logger, typeof(TEvent).Name, retryCount + 1, ex);

            var failed = FailedMessage.RecordPermanentFailure(
                messageId,
                typeof(TEvent).FullName!,
                JsonSerializer.Serialize(evt),
                ex.Message,
                retryCount + 1,
                GetType().Name,
                messageId,
                ex.StackTrace);

            await sharedUnitOfWork.FailedMessageRepository.CreateAsync(failed, context.CancellationToken).ConfigureAwait(false);
            await sharedUnitOfWork.CompleteAsync(context.CancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            MessageBusLogDefinitions.LogTransientEventFailure(logger, typeof(TEvent).Name, retryCount + 1, ex);
            throw; // let MassTransit retry
        }
    }
}

