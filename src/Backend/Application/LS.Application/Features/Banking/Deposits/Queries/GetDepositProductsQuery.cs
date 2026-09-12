using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Deposits.Dtos;
using MediatR;
using System.Collections.Generic;

namespace LS.Application.Features.Banking.Deposits.Queries;

public record GetDepositProductsQuery : IRequest<AppResponse<IEnumerable<DepositProductResponse>>>;
