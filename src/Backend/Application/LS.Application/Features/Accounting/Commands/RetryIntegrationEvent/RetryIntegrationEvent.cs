using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Accounting.Contracts;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace LS.Application.Features.Accounting.Commands;

public record RetryIntegrationEventCommand(Guid ErrorId) : IRequest<AppResponse<bool>>;

internal partial class RetryIntegrationEventCommandHandler(
    IAccountingUnitOfWork unitOfWork,
    IIntegrationEventPublisher publishEndpoint,
    ILogger<RetryIntegrationEventCommandHandler> logger)
    : IRequestHandler<RetryIntegrationEventCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(RetryIntegrationEventCommand request, CancellationToken cancellationToken)
    {
        var error = await unitOfWork.AccountingIntegrationErrorRepository.FindByIdAsync(request.ErrorId, cancellationToken);
        if (error is null)
            return AppResponses.NotFound<bool>("Integration error not found.");

        if (error.Status != "PendingRetry")
            return AppResponses.Conflict<bool>("The integration event is not pending retry.");

        try
        {
            var type = Type.GetType($"LS.Application.Features.Loans.IntegrationEvents.{error.EventType}, LS.Application")
                ?? Type.GetType($"LS.Application.Features.Banking.IntegrationEvents.{error.EventType}, LS.Application")
                ?? Type.GetType($"LS.Domain.Shared.Events.{error.EventType}, LS.Domain");

            if (type is null)
                return AppResponses.Failure<bool>("The integration event type is unsupported.");

            var message = JsonSerializer.Deserialize(error.EventPayload, type);
            if (message is null)
                return AppResponses.Failure<bool>("The saved integration event could not be read.");

            await publishEndpoint.PublishAsync((dynamic)message, cancellationToken);
            error.Status = "Resolved";
            await unitOfWork.AccountingIntegrationErrorRepository.UpdateAsync(error, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            LogRetried(logger, error.EventId);
            return AppResponses.Success(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogRetryFailed(logger, error.EventId, ex);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Retried integration event {EventId}")]
    private static partial void LogRetried(ILogger logger, Guid eventId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to retry integration event {EventId}")]
    private static partial void LogRetryFailed(ILogger logger, Guid eventId, Exception exception);
}
