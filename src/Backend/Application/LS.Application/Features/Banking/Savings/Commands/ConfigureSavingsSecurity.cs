using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using MediatR;

namespace LS.Application.Features.Banking.Savings.Commands;

public sealed record ConfigureSavingsSecurityCommand(Guid ProductId, ConfigureSavingsSecurityRequest Request) : IRequest<AppResponse<bool>>;

internal sealed class ConfigureSavingsSecurityCommandHandler(IBankingUnitOfWork unitOfWork,
    ICurrentActorProvider actor, TimeProvider clock) : IRequestHandler<ConfigureSavingsSecurityCommand, AppResponse<bool>>
{
    public async Task<AppResponse<bool>> Handle(ConfigureSavingsSecurityCommand request, CancellationToken ct)
    {
        var product = await unitOfWork.SavingsProductRepository.FindByIdAsync(request.ProductId, ct).ConfigureAwait(false);

        if (product is null)
        {
            return AppResponses.NotFound<bool>("Savings product not found.");
        }

        if (request.Request.CountsTowardLoanLimit && product.AllowsWithdrawals)
        {
            return AppResponses.Failure<bool>(AppError.BusinessRule("Only a non-withdrawable deposit product can count toward this reference borrowing limit."));
        }

        product.AllowsGuarantorReservations = request.Request.AllowsGuarantorReservations;
        product.CountsTowardLoanLimit = request.Request.CountsTowardLoanLimit;
        product.SecurityPolicyEvidenceReference = request.Request.EvidenceReference;
        product.UpdatedBy = actor.ActorId;
        product.UpdatedAt = clock.GetUtcNow();
        await unitOfWork.SavingsProductRepository.UpdateAsync(product, ct).ConfigureAwait(false);
        await unitOfWork.CompleteAsync(ct).ConfigureAwait(false);
        return AppResponses.Success(true);
    }
}
