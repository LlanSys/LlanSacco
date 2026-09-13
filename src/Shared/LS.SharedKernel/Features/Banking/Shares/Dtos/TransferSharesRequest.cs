using System;


namespace LS.SharedKernel.Features.Banking.Shares.Dtos;

public record TransferSharesRequest(
    Guid FromMemberId,
    Guid ToMemberId,
    Guid ShareProductId,
    int NumberOfShares,
    string? Notes
);
