using System;

namespace LS.SharedKernel.Features.Membership.Dtos;

public record OnboardMemberRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string IdentificationNumber,
    DateOnly DateOfBirth,
    string MembershipType);


