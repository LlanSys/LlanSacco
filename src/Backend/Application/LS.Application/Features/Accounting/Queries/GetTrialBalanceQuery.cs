using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Accounting.Queries;

public record GetTrialBalanceQuery(DateTimeOffset AsOfDate) : IRequest<AppResponse<TrialBalanceResponse>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Trial balances must reflect all committed postings as of the requested date.";
}

