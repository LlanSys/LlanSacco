using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Payroll.Dtos;

namespace LS.UI.Rcl.Features.HR.Payroll.Contracts.Interfaces;

public interface IPayrollService
{
    Task<AppResponse<bool>> RunPayrollAsync(RunPayrollRequest request);
    Task<AppResponse<bool>> ClosePayrollPeriodAsync(ClosePayrollPeriodRequest request);
    Task<AppResponse<IEnumerable<PayrollPeriodResponse>>> GetPayrollPeriodsAsync(GetPayrollPeriodsRequest request);
    Task<AppResponse<HrDashboardStatsResponse>> GetDashboardStatsAsync();
    
    Task<AppResponse<PayrollStatutoryConfigurationResponse>> GetConfigurationAsync();
    Task<AppResponse<Guid>> UpdateConfigurationAsync(UpdatePayrollConfigurationRequest request);
    Task<AppResponse<IEnumerable<PayslipResponse>>> GetPayslipsAsync(Guid periodId);
}

