using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class GetOtpStatusCommandValidator : AbstractValidator<GetOtpStatusCommand>
{
    public GetOtpStatusCommandValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
    }
}
