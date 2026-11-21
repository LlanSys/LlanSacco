using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Payments.Dtos;

namespace LS.Application.Features.Shared.Payments.Contracts.Interfaces;

public interface IPaymentWebhookVerifier
{
    Task<AppResponse<PaymentWebhookVerificationResponse>> VerifyAsync(
        string provider,
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken = default);
}
