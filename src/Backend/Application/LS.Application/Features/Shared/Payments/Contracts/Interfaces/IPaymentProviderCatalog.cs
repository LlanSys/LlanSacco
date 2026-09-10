using LS.SharedKernel.Features.Shared.Payments.Dtos;

namespace LS.Application.Features.Shared.Payments.Contracts.Interfaces;

public interface IPaymentProviderCatalog
{
    IReadOnlyCollection<PaymentProviderCapabilityResponse> GetCapabilities();
}
