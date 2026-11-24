using LS.Application.Features.Banking.GuarantorReservations.Commands;
using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Contracts.Implementations.Common;
using LS.SharedKernel.Features.Loans.Guarantors.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Infrastructure.Messaging.Consumers.Banking;

public sealed partial class GuarantorReservationInstructionConsumer(
    ISender sender, BackgroundExecutionContext executionContext,
    ILogger<GuarantorReservationInstructionConsumer> logger) : IConsumer<GuarantorReservationInstruction>
{
    public async Task Consume(ConsumeContext<GuarantorReservationInstruction> context)
    {
        if (!executionContext.TenantId.HasValue)
        {
            executionContext.Initialize(context.Message.TenantId, ICurrentActorProvider.SystemActor);
        }
        else if (executionContext.TenantId != context.Message.TenantId)
        {
            throw new InvalidOperationException("A reservation retry cannot change tenant scope.");
        }

        var result = await sender.Send(new ProcessGuarantorReservationCommand(context.Message), context.CancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            LogRejected(logger, context.Message.OperationId, result.Error?.Code);
            throw new InvalidOperationException("The reservation instruction could not be processed. Inspect its durable source before replay.");
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Reservation instruction {OperationId} rejected with {ErrorCode}")]
    private static partial void LogRejected(ILogger logger, Guid operationId, string? errorCode);
}
