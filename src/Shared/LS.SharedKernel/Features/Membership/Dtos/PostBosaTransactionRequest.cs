using System;

namespace LS.SharedKernel.Features.Membership.Dtos;

public record PostBosaTransactionRequest(
    Guid MemberAccountId,
    decimal Amount,
    string TransactionType,
    string Reference,
    string? Description,
    decimal FeeComponent = 0,
    Guid? BranchId = null,
    Guid? CostCenterId = null,
    Guid? PaymentChannelGlAccountId = null);


