using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Deposits.Dtos;
using MediatR;
using System;

namespace LS.Application.Features.Banking.Deposits.Commands;

public record DepositToAccountCommand(Guid AccountId, DepositTransactionRequest Request) : IRequest<AppResponse<DepositTransactionResponse>>;
