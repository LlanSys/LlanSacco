using LS.Application.Features.Shared.Payments.Contracts.Interfaces;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Payments.Dtos;
using MediatR;

namespace LS.Application.Features.Shared.Payments.QueryHandlers;

internal sealed class GetPaymentStatusHandler(IPaymentGateway paymentGateway)
    : IRequestHandler<GetPaymentStatusQuery, AppResponse<PaymentStatusResponse>>
{
    public Task<AppResponse<PaymentStatusResponse>> Handle(
        GetPaymentStatusQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return paymentGateway.GetStatusAsync(request.PaymentReference, request.Provider, cancellationToken);
    }
}
