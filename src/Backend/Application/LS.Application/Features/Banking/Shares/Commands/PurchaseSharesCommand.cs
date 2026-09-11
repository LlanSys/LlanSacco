using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.Banking.Shares.Commands;

public record PurchaseSharesCommand(
    Guid MemberId,
    Guid ShareProductId,
    decimal Amount,
    string? Notes
) : IRequest<AppResponse<Guid>>;
