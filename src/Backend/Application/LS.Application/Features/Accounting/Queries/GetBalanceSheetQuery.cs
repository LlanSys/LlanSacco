using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Accounting.Queries;

public record GetBalanceSheetQuery(DateTimeOffset AsOfDate) : IRequest<AppResponse<BalanceSheetResponse>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "The as-of balance sheet must include corrections posted since the last read.";
}
