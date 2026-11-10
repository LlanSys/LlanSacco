namespace LS.SharedKernel.Features.Banking.Deposits.Dtos;

public record OpenDepositAccountRequest(
    Guid MemberId,
    Guid DepositProductId
);
