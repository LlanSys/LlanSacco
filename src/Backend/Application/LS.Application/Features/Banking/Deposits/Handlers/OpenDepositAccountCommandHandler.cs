using LS.Application.Features.Banking.Deposits.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Deposits.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Deposits.Dtos;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.Deposits.Handlers;

internal class OpenDepositAccountCommandHandler(
    IBankingUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider) 
    : IRequestHandler<OpenDepositAccountCommand, AppResponse<DepositAccountResponse>>
{
    public async Task<AppResponse<DepositAccountResponse>> Handle(OpenDepositAccountCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var product = await unitOfWork.DepositProducts.FindByIdAsync(request.DepositProductId, cancellationToken);
        if (product == null)
        {
            return AppResponses.NotFound<DepositAccountResponse>($"Deposit product with ID {request.DepositProductId} not found.");
        }

        if (!product.IsActive)
        {
            return AppResponses.Failure<DepositAccountResponse>("Cannot open an account for an inactive product.");
        }

        // Add MaturityDate based on TermMonths
        var maturityDate = DateTimeOffset.UtcNow.AddMonths(product.TermMonths);

        var account = DepositAccount.Create(
            request.MemberId,
            request.DepositProductId,
            maturityDate,
            actorProvider.ActorId.ToString()
        );

        await unitOfWork.DepositAccounts.CreateAsync(account, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        var response = new DepositAccountResponse(
            account.Id,
            account.MemberId,
            account.DepositProductId,
            account.Balance,
            account.AccruedInterest,
            account.MaturityDate,
            account.IsActive
        );

        return AppResponses.Success("Deposit account opened successfully.", response);
    }
}

