using LS.Application.Features.Banking.Shares.Commands;
using LS.Application.Features.Banking.Shares.IntegrationEvents;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Shares.Entities;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Banking.Shares.Handlers;

internal class PurchaseSharesCommandHandler(
    IBankingUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<PurchaseSharesCommandHandler> logger
) : IRequestHandler<PurchaseSharesCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(PurchaseSharesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var product = await unitOfWork.ShareProducts.FirstOrDefaultAsync(x => x.Id == request.ShareProductId, cancellationToken)
            .ConfigureAwait(false);

        if (product == null)
            return AppResponses.Failure<Guid>("Share product not found.");

        int numberOfShares = (int)(request.Amount / product.PricePerShare);
        
        if (numberOfShares <= 0)
            return AppResponses.Failure<Guid>("Number of shares must be greater than zero.");

        decimal actualAmount = numberOfShares * product.PricePerShare;

        var account = await unitOfWork.ShareAccounts.FirstOrDefaultAsync(x => x.MemberId == request.MemberId && x.ShareProductId == request.ShareProductId, cancellationToken)
            .ConfigureAwait(false);

        if (account == null)
            account = ShareAccount.Create(request.MemberId, request.ShareProductId, "System");

        account.AddShares(numberOfShares, product.PricePerShare, "System");

        if (account.Id == Guid.Empty)
            await unitOfWork.ShareAccounts.CreateAsync(account, cancellationToken);
        else
            await unitOfWork.ShareAccounts.UpdateAsync(account, cancellationToken);

        var transaction = ShareTransaction.CreatePurchase(
            account.Id,
            numberOfShares,
            product.PricePerShare,
            null,
            request.Notes ?? "Share purchase",
            "System"
        );

        await unitOfWork.ShareTransactions.CreateAsync(transaction, cancellationToken);

        await publisher.Publish(new SharePurchasedIntegrationEvent(account.TenantId, account.Id, request.MemberId, numberOfShares, product.PricePerShare, actualAmount, null, "System"), cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Purchased {Shares} shares for Member {MemberId} in Product {ProductId}", numberOfShares, request.MemberId, request.ShareProductId);

        return AppResponses.Success("Shares purchased successfully.", transaction.Id);
    }
}

