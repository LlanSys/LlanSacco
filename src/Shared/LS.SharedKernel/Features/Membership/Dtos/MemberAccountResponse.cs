using System;

namespace LS.SharedKernel.Features.Membership.Dtos;

public record MemberAccountResponse(
    Guid Id,
    Guid MemberId,
    string AccountNumber,
    string AccountType,
    decimal CurrentBalance,
    bool IsActive);



