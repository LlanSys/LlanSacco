using System;


namespace LS.SharedKernel.Features.Banking.Shares.Dtos;

public record PurchaseSharesRequest(
    Guid MemberId,
    Guid ShareProductId,
    decimal Amount,
    string? Notes
);

