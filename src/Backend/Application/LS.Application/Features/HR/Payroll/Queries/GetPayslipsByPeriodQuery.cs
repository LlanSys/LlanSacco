using LS.Application.Contracts.Interfaces.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LS.SharedKernel.Features.HR.Payroll.Dtos;
using LS.Domain.Features.HR.Contracts;
using LS.SharedKernel.Dtos.Common;
using MediatR;

namespace LS.Application.Features.HR.Payroll.Queries;

public record GetPayslipsByPeriodQuery(Guid PayrollPeriodId) : IRequest<AppResponse<IEnumerable<PayslipResponse>>>, IUncachedQuery
{
    string IUncachedQuery.CacheBypassReason => "Payslips contain sensitive compensation and must reflect the completed run.";
}

internal class GetPayslipsByPeriodQueryHandler(IHrUnitOfWork unitOfWork) : IRequestHandler<GetPayslipsByPeriodQuery, AppResponse<IEnumerable<PayslipResponse>>>
{
    public async Task<AppResponse<IEnumerable<PayslipResponse>>> Handle(GetPayslipsByPeriodQuery request, CancellationToken cancellationToken)
    {
        var period = await unitOfWork.PayrollPeriodRepository.FindByIdAsync(request.PayrollPeriodId, cancellationToken);
        if (period == null)
        {
            return AppResponses.NotFound<IEnumerable<PayslipResponse>>("Payroll period not found.");
        }

        var payslips = await unitOfWork.PayslipRepository.ListAsync(q => q.Where(p => p.PayrollPeriodId == request.PayrollPeriodId), cancellationToken);
        var employees = await unitOfWork.EmployeeRepository.ListAsync(null, cancellationToken);

        var payslipDtos = payslips.Select(p => 
        {
            var emp = employees.FirstOrDefault(e => e.Id == p.EmployeeId);
            return new PayslipResponse(
                p.Id,
                p.PayrollPeriodId,
                p.EmployeeId,
                emp != null ? $"{emp.FirstName} {emp.LastName}" : "Unknown Employee",
                p.BasicSalary,
                p.TotalAllowances,
                p.GrossPay,
                p.PayeAmount,
                p.NssfAmount,
                p.ShifAmount,
                p.HousingLevyAmount,
                p.TotalDeductions,
                p.NetPay
            );
        }).ToList();

        return new AppResponse<IEnumerable<PayslipResponse>>(payslipDtos);
    }
}



