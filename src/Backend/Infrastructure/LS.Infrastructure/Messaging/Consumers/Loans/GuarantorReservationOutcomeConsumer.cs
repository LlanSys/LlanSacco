using LS.Application.Features.Loans.Guarantors.Commands;
using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Contracts.Implementations.Common;
using LS.SharedKernel.Features.Loans.Guarantors.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Infrastructure.Messaging.Consumers.Loans;

public sealed partial class GuarantorReservationOutcomeConsumer(ISender sender,
    BackgroundExecutionContext executionContext, ILogger<GuarantorReservationOutcomeConsumer> logger)
    : IConsumer<GuarantorReservationOutcome>
{
    public async Task Consume(ConsumeContext<GuarantorReservationOutcome> context)
    {
        if (!executionContext.TenantId.HasValue)
        {
            executionContext.Initialize(context.Message.TenantId, ICurrentActorProvider.SystemActor);
        }
        else if (executionContext.TenantId != context.Message.TenantId)
        {
            throw new InvalidOperationException("A reservation retry cannot change tenant scope.");
        }

        var result = await sender.Send(new ApplyGuarantorReservationOutcomeCommand(context.Message), context.CancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            LogRejected(logger, context.Message.OperationId, result.Error?.Code);
            throw new InvalidOperationException("The reservation outcome requires source reconciliation before replay.");
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Reservation outcome {OperationId} rejected with {ErrorCode}")]
    private static partial void LogRejected(ILogger logger, Guid operationId, string? errorCode);
}
