using System;


namespace LS.SharedKernel.Features.Banking.Shares.Dtos;

public record ShareAccountResponse(
    Guid Id,
    Guid MemberId,
    Guid ShareProductId,
    string ProductName,
    int TotalShares,
    decimal TotalValue,
    bool IsActive,
    DateTimeOffset OpenedAt
);

