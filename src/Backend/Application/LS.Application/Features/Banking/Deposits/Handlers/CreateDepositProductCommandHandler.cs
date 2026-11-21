using LS.Application.Features.Banking.Deposits.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Deposits.Entities;
using LS.Domain.Features.Banking.Deposits.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Deposits.Dtos;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.Deposits.Handlers;

internal class CreateDepositProductCommandHandler(
    IBankingUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider) 
    : IRequestHandler<CreateDepositProductCommand, AppResponse<DepositProductResponse>>
{
    public async Task<AppResponse<DepositProductResponse>> Handle(CreateDepositProductCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        bool exists = await unitOfWork.DepositProducts.AnyAsync(x => x.Code == request.Code, cancellationToken);
            
        if (exists)
        {
            return AppResponses.Failure<DepositProductResponse>($"A deposit product with code '{request.Code}' already exists.");
        }

        var productType = Enum.Parse<DepositType>(request.Type, true);
        var penaltyStrategy = Enum.Parse<EarlyWithdrawalPenaltyStrategy>(request.PenaltyStrategy, true);

        var product = DepositProduct.Create(
            request.Name,
            request.Code,
            request.Description,
            productType,
            request.InterestRate,
            request.TermMonths,
            request.MinimumDeposit,
            penaltyStrategy,
            request.FlatPenaltyRate,
            request.InterestForfeiturePercentage,
            request.ProRataReducedInterestRate,
            actorProvider.ActorId.ToString()
        );

        await unitOfWork.DepositProducts.CreateAsync(product, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        var response = new DepositProductResponse(
            product.Id,
            product.Name,
            product.Code,
            product.Description,
            product.Type.ToString(),
            product.InterestRate,
            product.TermMonths,
            product.MinimumDeposit,
            product.PenaltyStrategy.ToString(),
            product.FlatPenaltyRate,
            product.InterestForfeiturePercentage,
            product.ProRataReducedInterestRate,
            product.IsActive
        );

        return AppResponses.Success("Deposit product created successfully.", response);
    }
}

