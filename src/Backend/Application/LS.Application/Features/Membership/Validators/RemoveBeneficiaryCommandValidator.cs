using FluentValidation;
using LS.Application.Features.Membership.Commands;

namespace LS.Application.Features.Membership.Validators;

internal sealed class RemoveBeneficiaryCommandValidator : AbstractValidator<RemoveBeneficiaryCommand>
{
    public RemoveBeneficiaryCommandValidator()
    {
        RuleFor(request => request.MemberId).NotEmpty();
        RuleFor(request => request.BeneficiaryId).NotEmpty();
    }
}
