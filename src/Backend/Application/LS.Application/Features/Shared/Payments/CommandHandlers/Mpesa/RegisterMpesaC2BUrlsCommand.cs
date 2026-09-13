using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.Shared.Payments.CommandHandlers.Mpesa;

public sealed record RegisterMpesaC2BUrlsCommand() : IRequest<AppResponse<string>>;
