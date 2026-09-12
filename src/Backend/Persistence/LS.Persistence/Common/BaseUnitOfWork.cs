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

        return await strategy.ExecuteAsync(async _ =>
        {
            var transaction = await Context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using var configuredTransaction = transaction.ConfigureAwait(false);
            try
            {
                var result = await operation().ConfigureAwait(false);
                await DispatchDomainEventsAsync(cancellationToken).ConfigureAwait(false);
                await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                PersistenceLogDefinitions.LogTransactionConcurrencyRollback(_logger, ex);
                await RollbackAfterFailureAsync(transaction).ConfigureAwait(false);
                Context.ChangeTracker.Clear();
                throw;
            }
            catch (Exception ex)
            {
                PersistenceLogDefinitions.LogTransactionErrorRollback(_logger, ex);
                Context.ChangeTracker.Clear();
                await RollbackAfterFailureAsync(transaction).ConfigureAwait(false);
                throw;
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    private async Task RollbackAfterFailureAsync(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
    {
        Context.ChangeTracker.Clear();
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
        int maxRetries = 3, int baseDelayMs = 50, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxRetries, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(baseDelayMs);
        cancellationToken.ThrowIfCancellationRequested();
        var strategy = Context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async _ =>
        {
            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await using (var transaction = await Context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
                {
                    try
                    {
                        var result = await operation().ConfigureAwait(false);
                        await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                        return result;
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
    }

    public async Task<int> CompleteAsync(CancellationToken ct = default)
        => await Context.SaveChangesAsync(ct).ConfigureAwait(false);

    public IReadOnlyList<IDomainEvent> GetPendingDomainEvents()
    {
        var domainEntities = Context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        return domainEntities.SelectMany(x => x.DomainEvents).ToList().AsReadOnly();
    }

    public void ClearDomainEvents()
    {
        var domainEntities = Context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        domainEntities.ForEach(x => x.ClearDomainEvents());
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = Context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities.SelectMany(x => x.DomainEvents).ToList();
        domainEntities.ForEach(x => x.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await _publisher.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
    }
}
