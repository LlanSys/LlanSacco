using FluentValidation;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Savings.Entities;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace LS.Application.Features.Banking.Savings.Commands;

public record CreateSavingsProductCommand(CreateSavingsProductRequest Request) : IRequest<AppResponse<SavingsProductResponse>>;

internal class CreateSavingsProductCommandValidator : AbstractValidator<CreateSavingsProductCommand>
{
    public CreateSavingsProductCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.InterestRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.MinimumBalance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.WithdrawalFee).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.DailyWithdrawalLimit).GreaterThanOrEqualTo(0).When(x => x.Request.DailyWithdrawalLimit.HasValue);
        RuleFor(x => x.Request.MonthlyWithdrawalLimit).GreaterThanOrEqualTo(0).When(x => x.Request.MonthlyWithdrawalLimit.HasValue);
    }
}

internal class CreateSavingsProductCommandHandler(
    IBankingUnitOfWork unitOfWork,
    ICurrentActorProvider actorProvider) 
    : IRequestHandler<CreateSavingsProductCommand, AppResponse<SavingsProductResponse>>
{
    public async Task<AppResponse<SavingsProductResponse>> Handle(CreateSavingsProductCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        bool exists = await unitOfWork.SavingsProducts.AnyAsync(x => x.Code == request.Code, cancellationToken);
            
        if (exists)
        {
            return AppResponses.Failure<SavingsProductResponse>($"A savings product with code '{request.Code}' already exists.");
        }

        var product = SavingsProduct.Create(
            request.Name,
            request.Code,
            request.InterestRate,
            request.MinimumBalance,
            request.AllowsWithdrawals,
            request.WithdrawalFee,
            request.DailyWithdrawalLimit,
            request.MonthlyWithdrawalLimit,
            false, // isDefaultCheckoffTarget
            actorProvider.ActorId.ToString()
        );
        
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            product.Description = request.Description;
        }

        await unitOfWork.SavingsProducts.CreateAsync(product, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        var response = new SavingsProductResponse(
            product.Id,
            product.Name,
            product.Code,
            product.Description,
            product.InterestRate,
            product.MinimumBalance,
            product.AllowsWithdrawals,
            product.WithdrawalFee,
            product.DailyWithdrawalLimit,
            product.MonthlyWithdrawalLimit,
            product.IsActive
        );

        return AppResponses.Success("Savings product created successfully.", response);
    }
}

