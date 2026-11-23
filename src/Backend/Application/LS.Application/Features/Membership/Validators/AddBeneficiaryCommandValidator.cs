using FluentValidation;
using LS.Application.Features.Membership.Commands;

namespace LS.Application.Features.Membership.Validators;

internal sealed class AddBeneficiaryCommandValidator : AbstractValidator<AddBeneficiaryCommand>
{
    public AddBeneficiaryCommandValidator()
    {
        RuleFor(request => request.MemberId).NotEmpty();
        RuleFor(request => request.Request).NotNull();
        RuleFor(request => request.Request.FirstName).NotEmpty().When(request => request.Request is not null);
        RuleFor(request => request.Request.LastName).NotEmpty().When(request => request.Request is not null);
        RuleFor(request => request.Request.Relationship).NotEmpty().When(request => request.Request is not null);
    }
}
