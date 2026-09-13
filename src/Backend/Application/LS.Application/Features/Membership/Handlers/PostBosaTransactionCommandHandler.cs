using LS.SharedKernel.Extensions;
using System.Threading.Tasks;
using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Features.Membership.Contracts;
using LS.Domain.Features.Membership.Entities;
using LS.Domain.Features.Membership.Enums;
using LS.SharedKernel.Dtos.Common;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.Application.Features.Membership.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using LS.Application.Features.Membership.Commands;
using System;
using System.Threading;

namespace LS.Application.Features.Membership.Handlers;

internal class PostBosaTransactionCommandHandler(
    IMembershipUnitOfWork _unitOfWork,
    IIntegrationEventPublisher _eventPublisher,
    ICurrentTenantProvider _tenantProvider,
    ILogger<PostBosaTransactionCommandHandler> _logger) : IRequestHandler<PostBosaTransactionCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(PostBosaTransactionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        if (tenantId == Guid.Empty)
        {
            _logger.LogError("TenantId not resolved for PostBosaTransactionCommand.");
            return AppResponses.Failure<Guid>(AppError.Unexpected());
        }

        // Fetch Member Account
        var account = await _unitOfWork.MemberAccountRepository.FindByIdAsync(request.Request.MemberAccountId, cancellationToken).ConfigureAwait(false);
        if (account == null)
        {
            return AppResponses.Failure<Guid>(AppError.NotFound("Member Account not found."));
        }

        var memberId = account.MemberId;
        var transactionType = request.Request.TransactionType.ToEnum<MemberTransactionType>();
        
        if (transactionType == MemberTransactionType.Deposit)
        {
            account.CurrentBalance += request.Request.Amount;
        }
        else if (transactionType == MemberTransactionType.Withdrawal)
        {
            if (account.CurrentBalance < request.Request.Amount)
            {
                return AppResponses.Failure<Guid>(AppError.BusinessRule("Insufficient balance."));
            }
            account.CurrentBalance -= request.Request.Amount;
        }

        // Create transaction
        var transaction = MemberTransaction.Create(
            tenantId,
            memberId,
            account.Id,
            request.Request.Amount,
            transactionType,
            request.Request.Reference,
            request.Request.Description,
            "System"
        );

        await _unitOfWork.MemberTransactions.CreateAsync(transaction, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.MemberAccountRepository.UpdateAsync(account, cancellationToken).ConfigureAwait(false);

        // Raise integration event
        var integrationEvent = new MemberTransactionPostedIntegrationEvent(
            tenantId,
            transaction.Id,
            account.Id,
            request.Request.TransactionType,
            request.Request.Amount,
            request.Request.Reference,
            request.Request.Description,
            request.Request.FeeComponent,
            request.Request.BranchId,
            request.Request.CostCenterId,
            request.Request.PaymentChannelGlAccountId
        );

        // Publish using MassTransit Outbox Publisher pattern
        await _eventPublisher.PublishAsync(integrationEvent, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        return AppResponses.Success<Guid>(transaction.Id);
    }
}



