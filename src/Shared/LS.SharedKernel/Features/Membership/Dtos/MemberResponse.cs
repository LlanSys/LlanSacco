using System;

namespace LS.SharedKernel.Features.Membership.Dtos;

public record MemberResponse(
    Guid Id,
    string MemberNumber,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string IdentificationNumber,
    DateOnly DateOfBirth,
    string Status,
    string KycStatus,
    string MembershipType,
    DateTimeOffset JoinedAt);



