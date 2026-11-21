using LS.Domain.Features.Shared.Payments.Entities;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Payments.Dtos;

namespace LS.Application.Features.Shared.Payments.Contracts.Interfaces;

public interface IPaymentGateway
{
    Task<AppResponse<PaymentInitiationResponse>> InitiateAsync(
        PaymentRecord record,
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default);

    Task<AppResponse<PaymentStatusResponse>> GetStatusAsync(
        string paymentReference,
        string? provider = null,
        CancellationToken cancellationToken = default);
}
