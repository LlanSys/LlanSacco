using FluentValidation;
using LS.Application.Features.Shared.Payments.QueryHandlers;

namespace LS.Application.Features.Shared.Payments.Validators;

internal sealed class GetPaymentStatusQueryValidator : AbstractValidator<GetPaymentStatusQuery>
{
    public GetPaymentStatusQueryValidator()
    {
        RuleFor(query => query.Provider)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(query => query.PaymentReference)
            .NotEmpty()
            .MaximumLength(200);
    }
}
