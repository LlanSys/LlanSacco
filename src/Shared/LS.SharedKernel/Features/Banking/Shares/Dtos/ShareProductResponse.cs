using System;


namespace LS.SharedKernel.Features.Banking.Shares.Dtos;

public record ShareProductResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    int MinimumShares,
    decimal PricePerShare,
    bool IsActive
);

