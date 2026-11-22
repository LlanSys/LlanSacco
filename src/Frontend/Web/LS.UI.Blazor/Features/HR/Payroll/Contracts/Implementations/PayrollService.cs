using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Payroll.Dtos;
using LS.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using LS.UI.Rcl.Features.HR.Payroll.Contracts.Interfaces;

namespace LS.UI.Blazor.Features.HR.Payroll.Contracts.Implementations;

internal sealed class PayrollService(IBackendApiClient apiClient) : IPayrollService
{
    private const string BaseUrl = "api/v1/hr/payroll";

    public Task<AppResponse<bool>> RunPayrollAsync(RunPayrollRequest request)
        => apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/run", request);

    public Task<AppResponse<bool>> ClosePayrollPeriodAsync(ClosePayrollPeriodRequest request)
        => apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/close-period", request);

    public Task<AppResponse<HrDashboardStatsResponse>> GetDashboardStatsAsync()
        => apiClient.SendAsync<HrDashboardStatsResponse>(HttpMethod.Get, "api/v1.0/hr/dashboard-stats");

    public Task<AppResponse<IEnumerable<PayrollPeriodResponse>>> GetPayrollPeriodsAsync(GetPayrollPeriodsRequest request)
        => apiClient.SendAsync<IEnumerable<PayrollPeriodResponse>>(HttpMethod.Get,
            $"{BaseUrl}/periods?Year={request.Year}&Month={request.Month}");

    public Task<AppResponse<PayrollStatutoryConfigurationResponse>> GetConfigurationAsync()
        => apiClient.SendAsync<PayrollStatutoryConfigurationResponse>(HttpMethod.Get, $"{BaseUrl}/configuration");

    public Task<AppResponse<Guid>> UpdateConfigurationAsync(UpdatePayrollConfigurationRequest request)
        => apiClient.SendAsync<Guid>(HttpMethod.Post, $"{BaseUrl}/configuration", request);

    public Task<AppResponse<IEnumerable<PayslipResponse>>> GetPayslipsAsync(Guid periodId)
        => apiClient.SendAsync<IEnumerable<PayslipResponse>>(HttpMethod.Get, $"{BaseUrl}/periods/{periodId}/payslips");
}
