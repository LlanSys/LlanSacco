using System;

namespace LS.SharedKernel.Features.Membership.Dtos;

public record GuarantorRequestResponse(
    Guid Id,
    Guid MemberId,
    string MemberName,
    Guid NominatedGuarantorMemberId,
    string NominatedGuarantorName,
    decimal AmountToGuarantee,
    string Status,
    DateTimeOffset RequestDate,
    DateTimeOffset? ResponseDate,
    string? ResponseNote);



