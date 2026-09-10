using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.Banking.Shares.Commands;

public record CreateShareProductCommand(
    string Name,
    string Code,
    string Description,
    decimal PricePerShare,
    int MinimumShares
) : IRequest<AppResponse<Guid>>;
