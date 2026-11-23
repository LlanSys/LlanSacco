using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.Banking.Shares.Handlers;
using LS.Application.Features.Banking.Shares.IntegrationEvents;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Shares.Entities;
using LS.Domain.Features.Banking.Shares.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Banking.Shares.Commands;

public record TransferSharesCommand(
    Guid FromMemberId,
    Guid ToMemberId,
    Guid ShareProductId,
    int NumberOfShares,
    string? Notes
) : IRequest<AppResponse<Guid>>;

internal partial class TransferSharesCommandHandler(
    IBankingUnitOfWork unitOfWork,
    IContextEventPublisher<IBankingUnitOfWork> publisher,
    ICurrentActorProvider actorProvider,
    ILogger<TransferSharesCommandHandler> logger
) : IRequestHandler<TransferSharesCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(TransferSharesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.FromMemberId == request.ToMemberId)
            return AppResponses.Failure<Guid>("Source and destination members must be different.");

        if (request.NumberOfShares <= 0)
            return AppResponses.Failure<Guid>("Number of shares to transfer must be greater than zero.");

        var product = await unitOfWork.ShareProductRepository.FirstOrDefaultAsync(x => x.Id == request.ShareProductId, cancellationToken)
            .ConfigureAwait(false);

        if (product == null)
            return AppResponses.NotFound<Guid>("Share product not found.");

        decimal totalValue = request.NumberOfShares * product.PricePerShare;

        var fromAccount = await unitOfWork.ShareAccountRepository.FirstOrDefaultAsync(x => x.MemberId == request.FromMemberId && x.ShareProductId == request.ShareProductId, cancellationToken)
            .ConfigureAwait(false);

        if (fromAccount == null || fromAccount.TotalShares < request.NumberOfShares)
            return AppResponses.Failure<Guid>("Insufficient shares in the source account.");

        var toAccount = await unitOfWork.ShareAccountRepository.FirstOrDefaultAsync(x => x.MemberId == request.ToMemberId && x.ShareProductId == request.ShareProductId, cancellationToken)
            .ConfigureAwait(false);

        var isNewAccount = toAccount is null;
        if (toAccount == null)
            toAccount = ShareAccount.Create(request.ToMemberId, request.ShareProductId, actorProvider.ActorId);

        fromAccount.RemoveShares(request.NumberOfShares, product.PricePerShare, actorProvider.ActorId);
        toAccount.AddShares(request.NumberOfShares, product.PricePerShare, actorProvider.ActorId);

        await unitOfWork.ShareAccountRepository.UpdateAsync(fromAccount, cancellationToken);

        if (isNewAccount)
            await unitOfWork.ShareAccountRepository.CreateAsync(toAccount, cancellationToken);
        else
            await unitOfWork.ShareAccountRepository.UpdateAsync(toAccount, cancellationToken);

        var transferOutTransaction = ShareTransaction.CreateTransfer(
            fromAccount.Id,
            toAccount.Id,
            request.NumberOfShares,
            product.PricePerShare,
            ShareTransactionType.TransferOut,
            null,
            request.Notes ?? "Share transfer out",
            actorProvider.ActorId
        );

        var transferInTransaction = ShareTransaction.CreateTransfer(
            fromAccount.Id,
            toAccount.Id,
            request.NumberOfShares,
            product.PricePerShare,
            ShareTransactionType.TransferIn,
            null,
            request.Notes ?? "Share transfer in",
            actorProvider.ActorId
        );

        await unitOfWork.ShareTransactionRepository.CreateAsync(transferOutTransaction, cancellationToken);
        await unitOfWork.ShareTransactionRepository.CreateAsync(transferInTransaction, cancellationToken);

        await publisher.PublishAsync(new ShareTransferredIntegrationEvent(fromAccount.TenantId, fromAccount.Id, toAccount.Id, request.NumberOfShares, product.PricePerShare, totalValue, null, actorProvider.ActorId) { EventId = transferOutTransaction.Id, OccurredAt = transferOutTransaction.TransactionDate }, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        LogTransferred(logger,
            request.NumberOfShares, request.ShareProductId, request.FromMemberId, request.ToMemberId);

        return AppResponses.Success("Shares transferred successfully.", transferOutTransaction.Id);
    }
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Transferred {Shares} shares of Product {ProductId} from Member {FromMemberId} to Member {ToMemberId}")]
    private static partial void LogTransferred(ILogger logger, int shares, Guid productId, Guid fromMemberId, Guid toMemberId);}
