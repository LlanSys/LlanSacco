using LS.Application.Features.Loans.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LS.Infrastructure.Messaging.Consumers.Banking;

/// <summary>Quarantines the former unkeyed message for operator reconciliation; it must never increment funds.</summary>
public sealed partial class GuarantorAcceptedIntegrationEventHandler(
    ILogger<GuarantorAcceptedIntegrationEventHandler> logger) : IConsumer<GuarantorAcceptedIntegrationEvent>
{
    public Task Consume(ConsumeContext<GuarantorAcceptedIntegrationEvent> context)
    {
        LogLegacyInstruction(logger, context.Message.TenantId, context.Message.LoanApplicationId, context.Message.EventId);
        throw new InvalidOperationException("A legacy unkeyed guarantee instruction requires reconciliation before adoption.");
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Legacy guarantor event {EventId} for tenant {TenantId}, loan {LoanId} requires reconciliation")]
    private static partial void LogLegacyInstruction(ILogger logger, Guid tenantId, Guid loanId, Guid eventId);
}
