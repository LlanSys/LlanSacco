using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Utilities;
using LS.Domain.Features.HR.Contracts;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Shared.Contracts;
using LS.Domain.Shared.Contracts.Common;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Shared.Dashboard.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using LS.SharedKernel.Extensions;

namespace LS.Application.Features.Shared.Dashboard.QueryHandlers;


public record GetDashboardSummaryQuery(string UserId, string? RoleScope = null) : IRequest<AppResponse<DashboardSummaryResponse>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Dashboard data depends on live financial and operational state across contexts.";
}

