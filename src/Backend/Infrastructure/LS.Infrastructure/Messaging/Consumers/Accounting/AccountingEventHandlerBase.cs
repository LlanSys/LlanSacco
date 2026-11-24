using LS.Application.Features.Accounting.Contracts.Interfaces;
using LS.Domain.Features.Accounting.Contracts;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Contracts.Implementations.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LS.Infrastructure.Messaging.Consumers.Accounting;

public abstract partial class AccountingEventHandlerBase<TEvent> : IConsumer<TEvent> where TEvent : class, IIntegrationEvent
{
    protected readonly IAccountingUnitOfWork _unitOfWork;
    protected readonly ILedgerService _ledgerService;
    protected readonly ILogger _logger;
    private readonly BackgroundExecutionContext _executionContext;

    protected AccountingEventHandlerBase(
        IAccountingUnitOfWork unitOfWork,
        ILedgerService ledgerService,
        ILogger logger,
        BackgroundExecutionContext executionContext)
    {
        _unitOfWork = unitOfWork;
        _ledgerService = ledgerService;
        _logger = logger;
        _executionContext = executionContext;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var evt = context.Message;
        _executionContext.Initialize(GetTenantId(evt), ICurrentActorProvider.SystemActor);

        try
        {
            var transactionTypeCode = GetTransactionTypeCode(evt);
            var mapping = await _unitOfWork.TransactionTypeGlMappingRepository
                .FirstOrDefaultAsync(m => m.TransactionTypeCode == transactionTypeCode, context.CancellationToken)
                .ConfigureAwait(false);

            if (mapping is null)
            {
                await RecordFailureAsync(context, "No ledger mapping is configured for the transaction type.").ConfigureAwait(false);
                return;
            }

            var rejection = await ValidateMappingAsync(evt, mapping, context.CancellationToken).ConfigureAwait(false);

            if (rejection is not null)
            {
                await RecordFailureAsync(context, rejection).ConfigureAwait(false);
                return;
            }
            var entries = await CreateJournalEntriesAsync(evt, mapping).ConfigureAwait(false);
            var result = await _ledgerService.PostJournalAsync(
                referenceNumber: GetReferenceNumber(evt),
                description: GetDescription(evt),
                transactionDate: evt.OccurredAt,
                entries: entries,
                createdBy: ICurrentActorProvider.SystemActor,
                cancellationToken: context.CancellationToken)
                .ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                await RecordFailureAsync(context, result.Error?.Message ?? "The journal could not be posted.").ConfigureAwait(false);
                return;
            }

            LogPosted(_logger, typeof(TEvent).Name, context.MessageId);
        }
        catch (LedgerMappingException ex)
        {
            await RecordFailureAsync(context, ex.Message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUnexpectedFailure(_logger, typeof(TEvent).Name, context.MessageId, ex);
            throw;
        }
    }

    private async Task RecordFailureAsync(ConsumeContext<TEvent> context, string message)
    {
        var evt = context.Message;
        var eventType = typeof(TEvent).Name;
        LogRejected(_logger, eventType, context.MessageId, message);
        var error = new AccountingIntegrationError
        {
            TenantId = GetTenantId(evt),
            EventId = context.MessageId ?? Guid.Empty,
            EventType = eventType,
            EventPayload = JsonSerializer.Serialize(evt),
            ErrorMessage = message,
            Status = "Failed",
            OccurredAt = DateTimeOffset.UtcNow,
            CreatedBy = ICurrentActorProvider.SystemActor
        };

        await _unitOfWork.AccountingIntegrationErrorRepository.CreateAsync(error, context.CancellationToken).ConfigureAwait(false);
        await _unitOfWork.CompleteAsync(context.CancellationToken).ConfigureAwait(false);
    }

    protected abstract string GetTransactionTypeCode(TEvent evt);

    protected virtual Task<string?> ValidateMappingAsync(TEvent evt, TransactionTypeGlMapping mapping, CancellationToken cancellationToken)
        => Task.FromResult<string?>(null);

    protected abstract string GetReferenceNumber(TEvent evt);
    protected abstract string GetDescription(TEvent evt);
    protected abstract Guid GetTenantId(TEvent evt);
    protected abstract Task<IEnumerable<CreateJournalEntryRequest>> CreateJournalEntriesAsync(TEvent evt, TransactionTypeGlMapping mapping);

    protected Guid ResolveAccountId(string source, Guid? explicitId, Guid? channelId, Guid? productId)
        => source switch
        {
            "EXPLICIT_GL" => explicitId ?? throw new LedgerMappingException("The explicit ledger account is missing."),
            "CHANNEL_ACCOUNT" => channelId ?? throw new LedgerMappingException("The payment channel ledger account is missing."),
            "PRODUCT_ACCOUNT" => productId ?? throw new LedgerMappingException("The product ledger account is missing."),
            _ => throw new LedgerMappingException("The ledger account source is unsupported.")
        };

    [LoggerMessage(Level = LogLevel.Information, Message = "Posted ledger event {EventType} {EventId}")]
    private static partial void LogPosted(ILogger logger, string eventType, Guid? eventId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ledger event {EventType} {EventId} rejected: {Reason}")]
    private static partial void LogRejected(ILogger logger, string eventType, Guid? eventId, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unexpected failure posting ledger event {EventType} {EventId}")]
    private static partial void LogUnexpectedFailure(ILogger logger, string eventType, Guid? eventId, Exception exception);

    private sealed class LedgerMappingException(string message) : Exception(message);
}
