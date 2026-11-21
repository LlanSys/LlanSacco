using System;

namespace LS.SharedKernel.Features.Membership.Dtos;

public record NominateGuarantorRequest(
    Guid MemberId,
    Guid NominatedGuarantorMemberId,
    decimal AmountToGuarantee);
