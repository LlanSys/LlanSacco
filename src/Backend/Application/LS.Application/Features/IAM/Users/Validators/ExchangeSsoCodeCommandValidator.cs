using FluentValidation;
using LS.Application.Features.IAM.Users.Commands;

namespace LS.Application.Features.IAM.Users.Validators;

internal sealed class ExchangeSsoCodeCommandValidator : AbstractValidator<ExchangeSsoCodeCommand>
{
    public ExchangeSsoCodeCommandValidator()
    {
        RuleFor(request => request.Code).NotEmpty();
    }
}
