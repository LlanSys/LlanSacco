using FluentValidation;
using LS.Application.Features.Membership.Commands;
using LS.SharedKernel.Validation.Features.Membership;

namespace LS.Application.Features.Membership.Validators;

internal sealed class ApproveMemberCommandValidator : AbstractValidator<ApproveMemberCommand>
{
    public ApproveMemberCommandValidator()
    {
        RuleFor(request => request.Request).NotNull().SetValidator(new ApproveMemberRequestValidator());
    }
}
