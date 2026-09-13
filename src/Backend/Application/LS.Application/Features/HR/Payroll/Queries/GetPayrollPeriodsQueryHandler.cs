using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Features.HR.Payroll.Dtos;
using LS.Domain.Features.HR.Contracts;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.HR.Payroll.Queries;

internal sealed class GetPayrollPeriodsQueryHandler(
    IHrUnitOfWork unitOfWork) : IRequestHandler<GetPayrollPeriodsQuery, AppResponse<IEnumerable<PayrollPeriodResponse>>>
{
    public async Task<AppResponse<IEnumerable<PayrollPeriodResponse>>> Handle(GetPayrollPeriodsQuery request, CancellationToken cancellationToken)
    {
        var periods = await unitOfWork.PayrollPeriodRepository.ListAsync(null, cancellationToken);
        periods = periods.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToList();

        var dtos = periods.Select(p => new PayrollPeriodResponse(
            p.Id,
            p.Year,
            p.Month,
            p.Status.ToString(),
            p.ProcessedAt,
            p.ProcessedBy));

        return new AppResponse<IEnumerable<PayrollPeriodResponse>>(dtos);
    }
}


