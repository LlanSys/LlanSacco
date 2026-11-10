using System;


namespace LS.SharedKernel.Features.Banking.Shares.Dtos;

public record CreateShareProductRequest(
    string Name,
    string Code,
    string? Description,
    decimal PricePerShare,
    int MinimumShares
);

