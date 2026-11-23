using FluentValidation;
using LS.Application.Features.Shared.Payments.QueryHandlers;

namespace LS.Application.Features.Shared.Payments.Validators;

internal sealed class GetPaymentHistoryQueryValidator : AbstractValidator<GetPaymentHistoryQuery>
{
    public GetPaymentHistoryQueryValidator()
    {
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);
    }
}
