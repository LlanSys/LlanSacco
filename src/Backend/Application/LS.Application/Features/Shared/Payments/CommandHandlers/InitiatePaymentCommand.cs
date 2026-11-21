using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Payments.Dtos;
using MediatR;

namespace LS.Application.Features.Shared.Payments.CommandHandlers;

public sealed record InitiatePaymentCommand(PaymentInitiationRequest Request) : IRequest<AppResponse<PaymentInitiationResponse>>;
