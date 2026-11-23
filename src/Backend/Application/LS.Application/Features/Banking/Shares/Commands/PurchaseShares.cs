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

public record PurchaseSharesCommand(
    Guid MemberId,
    Guid ShareProductId,
    decimal Amount,
    string? Notes,
    string? ReferenceNumber = null
) : IRequest<AppResponse<Guid>>;

internal partial class PurchaseSharesCommandHandler(
    IBankingUnitOfWork unitOfWork,
    IContextEventPublisher<IBankingUnitOfWork> publisher,
    ILogger<PurchaseSharesCommandHandler> logger,
    ICurrentTenantProvider tenantProvider,
    ICurrentActorProvider actorProvider) : IRequestHandler<PurchaseSharesCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(PurchaseSharesCommand request, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;
        var actorId = actorProvider.ActorId;
        if (tenantId == Guid.Empty) return AppResponses.Failure<Guid>(AppError.Forbidden("A tenant is required."));
        if (request.Amount <= 0 || request.ReferenceNumber?.Length > 100)
            return AppResponses.Failure<Guid>("A positive amount and valid reference are required.");
        if (!string.IsNullOrWhiteSpace(request.ReferenceNumber))
        {
            var prior = await unitOfWork.ShareTransactionRepository.FirstOrDefaultAsync(t => t.TenantId == tenantId
                && t.ReferenceNumber == request.ReferenceNumber && t.TransactionType == ShareTransactionType.Purchase, cancellationToken);
            if (prior is not null)
            {
                var priorAccount = await unitOfWork.ShareAccountRepository.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Id == prior.ShareAccountId, cancellationToken);
                return priorAccount is not null && priorAccount.MemberId == request.MemberId && priorAccount.ShareProductId == request.ShareProductId && prior.TotalAmount == request.Amount
                    ? AppResponses.Success(prior.Id)
                    : AppResponses.Conflict<Guid>("This reference is already associated with different share purchase details.");
            }
        }
        var product = await unitOfWork.ShareProductRepository.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.ShareProductId, cancellationToken);
        if (product is null || !product.IsActive || product.PricePerShare <= 0)
            return AppResponses.Failure<Guid>("An active share product with a positive price is required.");
        var shareCount = request.Amount / product.PricePerShare;
        if (shareCount != decimal.Truncate(shareCount) || shareCount > int.MaxValue)
            return AppResponses.Failure<Guid>("The amount must purchase a whole number of shares.");
        var numberOfShares = (int)shareCount;
        var account = await unitOfWork.ShareAccountRepository.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.MemberId == request.MemberId && a.ShareProductId == request.ShareProductId, cancellationToken);
        var isNew = account is null;
        account ??= ShareAccount.Create(request.MemberId, request.ShareProductId, actorId);
        if (!account.IsActive || numberOfShares < product.MinimumShares && isNew
            || (long)account.TotalShares + numberOfShares > (product.MaximumShares ?? int.MaxValue))
            return AppResponses.Failure<Guid>("This purchase does not satisfy the account status or share limits.");
        account.TenantId = tenantId;
        account.AddShares(numberOfShares, product.PricePerShare, actorId);
        if (isNew) await unitOfWork.ShareAccountRepository.CreateAsync(account, cancellationToken);
        else await unitOfWork.ShareAccountRepository.UpdateAsync(account, cancellationToken);
        var transaction = ShareTransaction.CreatePurchase(account.Id, numberOfShares, product.PricePerShare,
            request.ReferenceNumber, request.Notes ?? "Share purchase", actorId);
        await unitOfWork.ShareTransactionRepository.CreateAsync(transaction, cancellationToken);
        await publisher.PublishAsync(new SharePurchasedIntegrationEvent(tenantId, account.Id, request.MemberId,
            numberOfShares, product.PricePerShare, request.Amount, request.ReferenceNumber, actorId)
            { EventId = transaction.Id, OccurredAt = transaction.TransactionDate }, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        Purchased(logger, numberOfShares, request.MemberId);
        return AppResponses.Success("Shares purchased successfully.", transaction.Id);
    }

    [LoggerMessage(EventId = 6101, Level = LogLevel.Information, Message = "Purchased {Shares} shares for member {MemberId}")]
    private static partial void Purchased(ILogger logger, int shares, Guid memberId);
}
