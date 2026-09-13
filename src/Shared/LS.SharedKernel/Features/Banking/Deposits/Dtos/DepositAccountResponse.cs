namespace LS.SharedKernel.Features.Banking.Deposits.Dtos;

public record DepositAccountResponse(
    Guid Id,
    Guid MemberId,
    Guid DepositProductId,
    decimal Balance,
    decimal AccruedInterest,
    DateTimeOffset MaturityDate,
    bool IsActive
);
