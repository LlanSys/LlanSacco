using LS.SharedKernel.Features.Membership.Dtos;
using LS.SharedKernel.Validation.Features.Membership;
using Xunit;

namespace LS.Tests.Unit.Shared.Validation;

/// <summary>
/// String fields converted with ToEnum must accept every member name. FluentValidation's IsInEnum()
/// rejects all strings, which previously blocked member onboarding and BOSA postings.
/// </summary>
public sealed class EnumStringValidationTests
{
    [Theory]
    [InlineData("Individual", true)]
    [InlineData("Group", true)]
    [InlineData("Corporate", true)]
    [InlineData("individual", false)]
    [InlineData("Cooperative", false)]
    [InlineData("", false)]
    public void Onboarding_accepts_exactly_the_membership_types_the_handler_can_convert(string membershipType, bool valid)
    {
        var request = new OnboardMemberRequest(
            "Amani", "Mwangi", "amani@jamii.example", "+254700900001", "90000001",
            new DateOnly(1985, 3, 14), membershipType);

        var result = new OnboardMemberRequestValidator().Validate(request);

        Assert.Equal(valid, !result.Errors.Any(e => e.PropertyName == nameof(OnboardMemberRequest.MembershipType)));
    }

    [Theory]
    [InlineData("Deposit", true)]
    [InlineData("RegistrationFee", true)]
    [InlineData("Transfer", false)]
    public void Bosa_posting_accepts_exactly_the_transaction_types_the_handler_can_convert(string transactionType, bool valid)
    {
        var request = new PostBosaTransactionRequest(Guid.CreateVersion7(), 100m, transactionType, "REF-1", null);

        var result = new PostBosaTransactionRequestValidator().Validate(request);

        Assert.Equal(valid, !result.Errors.Any(e => e.PropertyName == nameof(PostBosaTransactionRequest.TransactionType)));
    }
}
