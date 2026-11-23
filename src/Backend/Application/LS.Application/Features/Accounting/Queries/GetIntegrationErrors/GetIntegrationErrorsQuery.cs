using LS.Application.Contracts.Interfaces.Common;

using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.Domain.Features.Accounting.Entities;
using MediatR;
using System;

namespace LS.Application.Features.Accounting.Queries.GetIntegrationErrors;

public record GetIntegrationErrorsQuery(string? Status = null) : IRequest<AppResponse<IEnumerable<AccountingIntegrationErrorResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Retry and consumer delivery can change error state between requests.";
}


