using LS.Application.Contracts.Interfaces.Common;
using System.Collections.Generic;
using LS.SharedKernel.Features.HR.Payroll.Dtos;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.HR.Payroll.Queries;

public record GetPayrollPeriodsQuery() : IRequest<AppResponse<IEnumerable<PayrollPeriodResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Payroll periods reflect committed generation and closure state.";
}


