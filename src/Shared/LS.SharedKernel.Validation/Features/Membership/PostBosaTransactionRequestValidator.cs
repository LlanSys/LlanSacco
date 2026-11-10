using FluentValidation;
using LS.SharedKernel.Features.Membership.Dtos;

namespace LS.SharedKernel.Validation.Features.Membership;

public class PostBosaTransactionRequestValidator : AbstractValidator<PostBosaTransactionRequest>
{
    public PostBosaTransactionRequestValidator()
    {
        RuleFor(x => x.MemberAccountId)
            .NotEmpty().WithMessage("Member account ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transaction amount must be greater than zero.");

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("A valid transaction type must be provided.");

        RuleFor(x => x.Reference)
            .NotEmpty().WithMessage("Transaction reference is required.")
            .MaximumLength(100).WithMessage("Reference cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
