using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Payments.Dtos;
using MediatR;

namespace LS.Application.Features.Shared.Payments.CommandHandlers;

public sealed record ProcessPaymentWebhookCommand(
    string Provider,
    string Payload,
    string SignatureHeader) : IRequest<AppResponse<PaymentWebhookVerificationResponse>>;
