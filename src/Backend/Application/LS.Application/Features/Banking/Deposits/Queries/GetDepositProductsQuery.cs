using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Deposits.Dtos;
using MediatR;
using System.Collections.Generic;

namespace LS.Application.Features.Banking.Deposits.Queries;

public record GetDepositProductsQuery : IRequest<AppResponse<IEnumerable<DepositProductResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Deposit terms reflect current product configuration before an account is opened.";
}
