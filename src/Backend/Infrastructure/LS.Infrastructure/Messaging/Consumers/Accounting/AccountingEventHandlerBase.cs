using LS.Domain.Features.Accounting.Contracts;
using LS.Application.Features.Accounting.Contracts.Interfaces;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.Domain.Features.Accounting.Entities;
using LS.Domain.Shared.Contracts.Common;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace LS.Infrastructure.Messaging.Consumers.Accounting;

public abstract class AccountingEventHandlerBase<TEvent> : IConsumer<TEvent> where TEvent : class, IIntegrationEvent
{
    protected readonly IAccountingUnitOfWork _unitOfWork;
    protected readonly ILedgerService _ledgerService;
    protected readonly ILogger _logger;

    protected AccountingEventHandlerBase(
        IAccountingUnitOfWork unitOfWork,
        ILedgerService ledgerService,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _ledgerService = ledgerService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var evt = context.Message;
        
        try
        {
            var mapping = await _unitOfWork.TransactionTypeGlMappingRepository
                .GetByTransactionTypeCodeAsync(GetTransactionTypeCode(evt), context.CancellationToken)
                .ConfigureAwait(false);

            if (mapping == null)
            {
                throw new InvalidOperationException($"No GL mapping found for transaction type {GetTransactionTypeCode(evt)}");
            }

            var entries = await CreateJournalEntriesAsync(evt, mapping).ConfigureAwait(false);

            await _ledgerService.PostJournalAsync(
                referenceNumber: GetReferenceNumber(evt),
                description: GetDescription(evt),
                transactionDate: evt.OccurredAt,
                entries: entries,
                createdBy: "System-IntegrationEvent",
                cancellationToken: context.CancellationToken
            ).ConfigureAwait(false);
            
            _logger.LogInformation("Successfully posted GL for {EventType} {EventId}", typeof(TEvent).Name, context.MessageId);
        }
        catch (InvalidOperationException ex)
        {
            // Expected mapping errors -> DLQ
            _logger.LogError(ex, "Failed to process GL mapping for {EventType} {EventId}. Moving to DLQ.", typeof(TEvent).Name, context.MessageId);
            
            var error = new AccountingIntegrationError
            {
                TenantId = GetTenantId(evt),
                EventId = context.MessageId ?? Guid.Empty,
                EventType = typeof(TEvent).Name,
                EventPayload = JsonSerializer.Serialize(evt),
                ErrorMessage = ex.Message,
                Status = "Failed",
                OccurredAt = DateTimeOffset.UtcNow,
                CreatedBy = "System-IntegrationEvent"
            };
            
            await _unitOfWork.AccountingIntegrationErrorRepository.CreateAsync(error, context.CancellationToken).ConfigureAwait(false);
            await _unitOfWork.CompleteAsync(context.CancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing GL mapping for {EventType} {EventId}", typeof(TEvent).Name, context.MessageId);
            throw; // Let MassTransit retry other unexpected errors
        }
    }

    protected abstract string GetTransactionTypeCode(TEvent evt);
    protected abstract string GetReferenceNumber(TEvent evt);
    protected abstract string GetDescription(TEvent evt);
    protected abstract Guid GetTenantId(TEvent evt);
    protected abstract Task<IEnumerable<CreateJournalEntryDto>> CreateJournalEntriesAsync(TEvent evt, TransactionTypeGlMapping mapping);

    protected Guid ResolveAccountId(string source, Guid? explicitId, Guid? channelId, Guid? productId)
    {
        return source switch
        {
            "EXPLICIT_GL" => explicitId ?? throw new InvalidOperationException("Explicit GL Account ID is null"),
            "CHANNEL_ACCOUNT" => channelId ?? throw new InvalidOperationException("Channel Account ID is null"),
            "PRODUCT_ACCOUNT" => productId ?? throw new InvalidOperationException("Product Account ID is null"),
            _ => throw new InvalidOperationException($"Unknown GL source {source}")
        };
    }
}
