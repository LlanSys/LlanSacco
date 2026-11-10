using LS.Application.Features.Shared.Payments.Contracts.Interfaces;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Payments.Dtos;

using MediatR;

namespace LS.Application.Features.Shared.Payments.QueryHandlers;

internal sealed class GetPaymentCapabilitiesHandler(IPaymentProviderCatalog providerCatalog)
    : IRequestHandler<GetPaymentCapabilitiesQuery, AppResponse<IReadOnlyCollection<PaymentProviderCapabilityResponse>>>
{
    public Task<AppResponse<IReadOnlyCollection<PaymentProviderCapabilityResponse>>> Handle(
        GetPaymentCapabilitiesQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(AppResponses.Success(providerCatalog.GetCapabilities()));
    }
}
