using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using MediatR;
using System.Collections.Generic;

namespace LS.Application.Features.Accounting.Queries;

public record GetAccountsQuery : IRequest<AppResponse<IReadOnlyList<AccountResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Chart-of-accounts reads use the current posting configuration.";
}

