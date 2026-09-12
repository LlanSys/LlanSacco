namespace LS.SharedKernel.Features.Membership.Dtos;

public record AddBeneficiaryRequest(
    string FirstName,
    string LastName,
    string Relationship,
    string PhoneNumber,
    string Email,
    string NationalId,
    decimal AllocationPercentage,
    bool IsNextOfKin
);
