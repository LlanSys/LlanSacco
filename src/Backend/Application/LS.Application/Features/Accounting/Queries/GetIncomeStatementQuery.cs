using LS.Application.Contracts.Interfaces.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;
using System;

namespace LS.Application.Features.Accounting.Queries;

public record GetIncomeStatementQuery(DateTimeOffset StartDate, DateTimeOffset EndDate) : IRequest<AppResponse<IncomeStatementDto>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Financial statements reflect journals posted for the requested date range.";
}
