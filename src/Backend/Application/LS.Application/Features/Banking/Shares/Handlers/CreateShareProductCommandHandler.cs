using LS.Application.Features.Banking.Shares.Commands;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Shares.Entities;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Banking.Shares.Handlers;

internal class CreateShareProductCommandHandler(
    IBankingUnitOfWork unitOfWork,
    ILogger<CreateShareProductCommandHandler> logger
) : IRequestHandler<CreateShareProductCommand, AppResponse<Guid>>
{
    public async Task<AppResponse<Guid>> Handle(CreateShareProductCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var product = ShareProduct.Create(
            request.Name,
            request.Code,
            request.PricePerShare,
            request.MinimumShares,
            "System"
        );

        await unitOfWork.ShareProducts.CreateAsync(product, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created ShareProduct {ProductId}", product.Id);

        return AppResponses.Success("Share product created successfully.", product.Id);
    }
}

