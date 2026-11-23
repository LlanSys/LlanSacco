using FluentValidation;
using LS.Application.Features.Membership.Commands;
using LS.SharedKernel.Validation.Features.Membership;

namespace LS.Application.Features.Membership.Validators;

internal sealed class PostBosaTransactionCommandValidator : AbstractValidator<PostBosaTransactionCommand>
{
    public PostBosaTransactionCommandValidator()
    {
        RuleFor(request => request.Request).NotNull().SetValidator(new PostBosaTransactionRequestValidator());
    }
}
