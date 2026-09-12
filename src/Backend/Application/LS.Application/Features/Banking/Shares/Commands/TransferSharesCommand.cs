using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.Banking.Shares.Commands;

public record TransferSharesCommand(
    Guid FromMemberId,
    Guid ToMemberId,
    Guid ShareProductId,
    int NumberOfShares,
    string? Notes
) : IRequest<AppResponse<Guid>>;
