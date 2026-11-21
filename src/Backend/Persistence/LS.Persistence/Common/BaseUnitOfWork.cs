using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Persistence.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LS.Persistence.Common;

public abstract class BaseUnitOfWork<TContext>(
    TContext context,
    IPublisher publisher,
    ILogger logger
) where TContext : DbContext
{
    protected TContext Context { get; } = context;
    private readonly IPublisher _publisher = publisher;
    private readonly ILogger _logger = logger;

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken)
    {
        var strategy = Context.Database.CreateExecutionStrategy();

        var result = await strategy.ExecuteAsync(async _ =>
        {
            var transaction = await Context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using var configuredTransaction = transaction.ConfigureAwait(false);
            try
            {
                var outcome = await operation().ConfigureAwait(false);
                await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return outcome;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                PersistenceLogDefinitions.LogTransactionConcurrencyRollback(_logger, ex);
                await RollbackAfterFailureAsync(transaction).ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                PersistenceLogDefinitions.LogTransactionErrorRollback(_logger, ex);
                await RollbackAfterFailureAsync(transaction).ConfigureAwait(false);
                throw;
            }
        }, cancellationToken).ConfigureAwait(false);
        await DispatchCommittedDomainEventsAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    private async Task RollbackAfterFailureAsync(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
    {
        DiscardAttempt();
        try
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception cleanupError)
        {
            // Preserve the original operation failure if the provider already aborted/disposed its transaction.
            PersistenceLogDefinitions.LogTransactionErrorRollback(_logger, cleanupError);
        }
    }
    public async Task<TResult> ExecuteInTransactionWithRetryAsync<TResult>(Func<Task<TResult>> operation,
        int maxRetries = 3, int baseDelayMs = 50, CancellationToken cancellationToken = default, Func<TResult, bool>? shouldCommit = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxRetries, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(baseDelayMs);
        cancellationToken.ThrowIfCancellationRequested();
        var strategy = Context.Database.CreateExecutionStrategy();

        var result = await strategy.ExecuteAsync(async _ =>
        {
            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await using (var transaction = await Context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
                {
                    try
                    {
                        var outcome = await operation().ConfigureAwait(false);
                        if (shouldCommit is not null && !shouldCommit(outcome))
                        {
                            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                            DiscardAttempt();
                            return outcome;
                        }
                        await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                        return outcome;
                    }
                    catch (DbUpdateConcurrencyException ex) when (attempt < maxRetries)
                    {
                        PersistenceLogDefinitions.LogTransactionConcurrencyRetry(_logger, attempt, maxRetries, ex);
                        await RollbackAfterFailureAsync(transaction).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        PersistenceLogDefinitions.LogRetryableTransactionErrorRollback(_logger, ex);
                        await RollbackAfterFailureAsync(transaction).ConfigureAwait(false);
                        throw;
                    }
                }
                // Release the transaction before waiting. Cancellation must also stop the backoff.
                await Task.Delay(TimeSpan.FromMilliseconds((double)baseDelayMs * attempt), cancellationToken).ConfigureAwait(false);
            }
            throw new InvalidOperationException("Max retry attempts exceeded due to concurrency conflicts.");
        }, cancellationToken).ConfigureAwait(false);
        await DispatchCommittedDomainEventsAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task<int> CompleteAsync(CancellationToken ct = default)
    {
        var count = await Context.SaveChangesAsync(ct).ConfigureAwait(false);
        // Embedded saves leave dispatch to the transaction owner.
        if (Context.Database.CurrentTransaction is null)
            await DispatchCommittedDomainEventsAsync(ct).ConfigureAwait(false);
        return count;
    }

    public IReadOnlyList<IDomainEvent> GetPendingDomainEvents()
        => DBContextHelper.CollectDomainEvents(Context.ChangeTracker).AsReadOnly();

    public void ClearDomainEvents()
        => DBContextHelper.ClearDomainEventsFromAggregates(Context.ChangeTracker);

    private void DiscardAttempt()
    {
        Context.ChangeTracker.Clear();
        if (Context is IDomainEventBuffer buffer)
            buffer.ClearCollectedDomainEvents();
    }

    private async Task DispatchCommittedDomainEventsAsync(CancellationToken cancellationToken)
    {
        if (Context is not IDomainEventBuffer buffer)
            return;

        var events = buffer.GetCollectedDomainEvents()?.ToArray() ?? [];
        buffer.ClearCollectedDomainEvents();

        // These are in-process notifications. Durable external work uses the context outbox.
        foreach (var domainEvent in events)
            await _publisher.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
    }
}
