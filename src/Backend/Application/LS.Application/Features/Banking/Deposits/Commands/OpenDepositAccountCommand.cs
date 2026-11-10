using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Deposits.Dtos;
using MediatR;

namespace LS.Application.Features.Banking.Deposits.Commands;

public record OpenDepositAccountCommand(OpenDepositAccountRequest Request) : IRequest<AppResponse<DepositAccountResponse>>;
