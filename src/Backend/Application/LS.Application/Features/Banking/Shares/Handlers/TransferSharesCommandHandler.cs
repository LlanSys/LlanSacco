using LS.Application.Features.Banking.Shares.Commands;
using LS.Application.Features.Banking.Shares.IntegrationEvents;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Shares.Entities;
using LS.Domain.Features.Banking.Shares.Enums;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Banking.Shares.Handlers;

internal class TransferSharesCommandHandler(
    IBankingUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<TransferSharesCommandHandler> logger
) : IRequestHandler<TransferSharesCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(TransferSharesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.NumberOfShares <= 0)
            return AppResponses.Failure<Guid>("Number of shares to transfer must be greater than zero.");

        var product = await unitOfWork.ShareProducts.FirstOrDefaultAsync(x => x.Id == request.ShareProductId, cancellationToken)
            .ConfigureAwait(false);

        if (product == null)
            return AppResponses.Failure<Guid>("Share product not found.");

        decimal totalValue = request.NumberOfShares * product.PricePerShare;

        var fromAccount = await unitOfWork.ShareAccounts.FirstOrDefaultAsync(x => x.MemberId == request.FromMemberId && x.ShareProductId == request.ShareProductId, cancellationToken)
            .ConfigureAwait(false);

        if (fromAccount == null || fromAccount.TotalShares < request.NumberOfShares)
            return AppResponses.Failure<Guid>("Insufficient shares in the source account.");

        var toAccount = await unitOfWork.ShareAccounts.FirstOrDefaultAsync(x => x.MemberId == request.ToMemberId && x.ShareProductId == request.ShareProductId, cancellationToken)
            .ConfigureAwait(false);

        if (toAccount == null)
            toAccount = ShareAccount.Create(request.ToMemberId, request.ShareProductId, "System");

        fromAccount.RemoveShares(request.NumberOfShares, product.PricePerShare, "System");
        toAccount.AddShares(request.NumberOfShares, product.PricePerShare, "System");

        await unitOfWork.ShareAccounts.UpdateAsync(fromAccount, cancellationToken);

        if (toAccount.Id == Guid.Empty)
            await unitOfWork.ShareAccounts.CreateAsync(toAccount, cancellationToken);
        else
            await unitOfWork.ShareAccounts.UpdateAsync(toAccount, cancellationToken);

        var transferOutTransaction = ShareTransaction.CreateTransfer(
            fromAccount.Id,
            toAccount.Id,
            request.NumberOfShares,
            product.PricePerShare,
            ShareTransactionType.TransferOut,
            null,
            request.Notes ?? "Share transfer out",
            "System"
        );

        var transferInTransaction = ShareTransaction.CreateTransfer(
            fromAccount.Id,
            toAccount.Id,
            request.NumberOfShares,
            product.PricePerShare,
            ShareTransactionType.TransferIn,
            null,
            request.Notes ?? "Share transfer in",
            "System"
        );

        await unitOfWork.ShareTransactions.CreateAsync(transferOutTransaction, cancellationToken);
        await unitOfWork.ShareTransactions.CreateAsync(transferInTransaction, cancellationToken);

        await publisher.Publish(new ShareTransferredIntegrationEvent(fromAccount.TenantId, fromAccount.Id, toAccount.Id, request.NumberOfShares, product.PricePerShare, totalValue, null, "System"), cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Transferred {Shares} shares of Product {ProductId} from Member {FromMemberId} to Member {ToMemberId}", 
            request.NumberOfShares, request.ShareProductId, request.FromMemberId, request.ToMemberId);

        return AppResponses.Success("Shares transferred successfully.", transferOutTransaction.Id);
    }
}

