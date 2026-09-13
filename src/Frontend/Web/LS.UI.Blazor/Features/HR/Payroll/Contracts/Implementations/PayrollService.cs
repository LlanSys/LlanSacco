using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.HR.Payroll.Dtos;
using LS.UI.Rcl.Features.HR.Payroll.Contracts.Interfaces;

namespace LS.UI.Blazor.Features.HR.Payroll.Contracts.Implementations;

internal sealed class PayrollService(HttpClient httpClient) : IPayrollService
{
    private const string BaseUrl = "api/v1/hr/payroll";

    public async Task<AppResponse<bool>> RunPayrollAsync(RunPayrollRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseUrl}/run", request).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<AppResponse<bool>>().ConfigureAwait(false)
            ?? new AppResponse<bool> { IsSuccess = false, Error = AppError.Unexpected() };
    }

    public async Task<AppResponse<bool>> ClosePayrollPeriodAsync(ClosePayrollPeriodRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseUrl}/close-period", request).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<AppResponse<bool>>().ConfigureAwait(false)
            ?? new AppResponse<bool> { IsSuccess = false, Error = AppError.Unexpected() };
    }

    public async Task<AppResponse<HrDashboardStatsResponse>> GetDashboardStatsAsync()
    {
        var response = await httpClient.GetAsync("api/v1.0/hr/dashboard-stats").ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<AppResponse<HrDashboardStatsResponse>>().ConfigureAwait(false)
            ?? new AppResponse<HrDashboardStatsResponse> { IsSuccess = false, Error = AppError.Unexpected() };
    }

    public async Task<AppResponse<IEnumerable<PayrollPeriodResponse>>> GetPayrollPeriodsAsync(GetPayrollPeriodsRequest request)
    {
        var queryString = $"?Year={request.Year}&Month={request.Month}";
        var response = await httpClient.GetAsync($"{BaseUrl}/periods{queryString}").ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<AppResponse<IEnumerable<PayrollPeriodResponse>>>().ConfigureAwait(false)
            ?? new AppResponse<IEnumerable<PayrollPeriodResponse>> { IsSuccess = false, Error = AppError.Unexpected() };
    }

    public async Task<AppResponse<PayrollStatutoryConfigurationResponse>> GetConfigurationAsync()
    {
        var response = await httpClient.GetAsync("$BaseUrl/configuration").ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<AppResponse<PayrollStatutoryConfigurationResponse>>().ConfigureAwait(false)
            ?? new AppResponse<PayrollStatutoryConfigurationResponse> { IsSuccess = false, Error = AppError.Unexpected() };
    }

    public async Task<AppResponse<Guid>> UpdateConfigurationAsync(UpdatePayrollConfigurationRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("$BaseUrl/configuration", request).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<AppResponse<Guid>>().ConfigureAwait(false)
            ?? new AppResponse<Guid> { IsSuccess = false, Error = AppError.Unexpected() };
    }

    public async Task<AppResponse<IEnumerable<PayslipResponse>>> GetPayslipsAsync(Guid periodId)
    {
        var response = await httpClient.GetAsync("$BaseUrl/periods/$periodId/payslips").ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<AppResponse<IEnumerable<PayslipResponse>>>().ConfigureAwait(false)
            ?? new AppResponse<IEnumerable<PayslipResponse>> { IsSuccess = false, Error = AppError.Unexpected() };
    }
}




