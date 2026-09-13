using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Payments.Dtos;

using MediatR;

namespace LS.Application.Features.Shared.Payments.QueryHandlers;

public sealed record GetPaymentCapabilitiesQuery
    : IRequest<AppResponse<IReadOnlyCollection<PaymentProviderCapabilityResponse>>>, ICachableRequest
{
    public string CacheGroup => "payments";
    public string Discriminator => "capabilities";
    public string? CacheUserId => null;
    public bool IsVersioned => false;
    public bool BypassCache => true;
}
