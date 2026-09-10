using FluentValidation;

namespace LS.Application.Features.Shared.Payments.QueryHandlers;

internal sealed class GetPaymentHistoryQueryValidator : AbstractValidator<GetPaymentHistoryQuery>
{
    public GetPaymentHistoryQueryValidator()
    {
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);
    }
}

