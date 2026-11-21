using LS.SharedKernel.Dtos.Common;
using LS.Domain.Features.Accounting.Contracts;
using LS.Application.Contracts.Interfaces.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Accounting.Commands;

internal class RetryIntegrationEventCommandHandler : IRequestHandler<RetryIntegrationEventCommand, AppResponse<bool>>
{
    private readonly IAccountingUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _publishEndpoint;
    private readonly ILogger<RetryIntegrationEventCommandHandler> _logger;

    public RetryIntegrationEventCommandHandler(
        IAccountingUnitOfWork unitOfWork,
        IIntegrationEventPublisher publishEndpoint,
        ILogger<RetryIntegrationEventCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<AppResponse<bool>> Handle(RetryIntegrationEventCommand request, CancellationToken cancellationToken)
    {
        var error = await _unitOfWork.AccountingIntegrationErrorRepository.FindByIdAsync(request.ErrorId, cancellationToken);
        if (error == null)
        {
            return AppResponses.NotFound<bool>("Integration error not found.");
        }

        if (error.Status != "PendingRetry")
        {
            return AppResponses.Failure<bool>($"Cannot retry error in status {error.Status}");
        }

        try
        {
            var type = Type.GetType($"LS.Application.Features.Loans.IntegrationEvents.{error.EventType}, LS.Application") 
                    ?? Type.GetType($"LS.Application.Features.Banking.IntegrationEvents.{error.EventType}, LS.Application")
                    ?? Type.GetType($"LS.Domain.Shared.Events.{error.EventType}, LS.Domain");

            if (type == null)
            {
                return AppResponses.Failure<bool>($"Could not resolve event type: {error.EventType}");
            }

            var message = JsonSerializer.Deserialize(error.EventPayload, type);
            if (message == null)
            {
                return AppResponses.Failure<bool>("Failed to deserialize event payload.");
            }

            // Using dynamic to bypass compile-time generic requirement for IIntegrationEventPublisher.PublishAsync<T>
            await _publishEndpoint.PublishAsync((dynamic)message, cancellationToken);

            error.Status = "Resolved";
            await _unitOfWork.AccountingIntegrationErrorRepository.UpdateAsync(error, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Successfully retried integration event {EventId}", error.EventId);

            return AppResponses.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retry integration event {EventId}", error.EventId);
            return AppResponses.Failure<bool>($"Failed to retry event: {ex.Message}");
        }
    }
}

