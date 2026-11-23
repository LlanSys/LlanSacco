using LS.Application.Features.Banking.Shares.Handlers;
using LS.Domain.Features.Banking.Contracts;
using LS.Domain.Features.Banking.Shares.Entities;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LS.Application.Features.Banking.Shares.Commands;

public record CreateShareProductCommand(
    string Name,
    string Code,
    string Description,
    decimal PricePerShare,
    int MinimumShares
) : IRequest<AppResponse<Guid>>;

internal partial class CreateShareProductCommandHandler(
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

        await unitOfWork.ShareProductRepository.CreateAsync(product, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken).ConfigureAwait(false);

        LogProductCreated(logger, product.Id);

        return AppResponses.Success("Share product created successfully.", product.Id);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Created ShareProduct {ProductId}")]
    private static partial void LogProductCreated(ILogger logger, Guid productId);
}
