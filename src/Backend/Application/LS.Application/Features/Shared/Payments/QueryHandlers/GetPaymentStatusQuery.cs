using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Payments.Dtos;
using MediatR;

namespace LS.Application.Features.Shared.Payments.QueryHandlers;

public sealed record GetPaymentStatusQuery(string Provider, string PaymentReference)
    : IRequest<AppResponse<PaymentStatusResponse>>, ICachableRequest
{
    public string CacheGroup => "payments";

    public string Discriminator => CacheKeys.Entity(Provider, PaymentReference);

    public string? CacheUserId => null;

    public bool IsVersioned => false;

    public bool BypassCache => true;
}
