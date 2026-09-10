using System;

namespace LS.SharedKernel.Features.Membership.Dtos;

public record BeneficiaryResponse(
    Guid Id,
    Guid MemberId,
    string FullName,
    string Relationship,
    string PhoneNumber,
    string IdentificationNumber,
    decimal AllocationPercentage,
    bool IsNextOfKin
);
