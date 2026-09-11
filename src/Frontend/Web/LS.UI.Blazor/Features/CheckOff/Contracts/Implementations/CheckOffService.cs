using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.CheckOff.Dtos;
using LS.UI.Blazor.Features.CheckOff.Contracts.Interfaces;
using System.Net.Http.Json;

namespace LS.UI.Blazor.Features.CheckOff.Contracts.Implementations;

public class CheckOffService(HttpClient httpClient) : ICheckOffService
{
    public async Task<AppResponse<List<EmployerResponse>>> GetEmployersAsync()
    {
        var response = await httpClient.GetAsync("api/checkoff/employers");
        var result = await response.Content.ReadFromJsonAsync<AppResponse<List<EmployerResponse>>>();
        return result ?? new AppResponse<List<EmployerResponse>> { IsSuccess = false, Error = AppError.Unexpected(), Message = "Failed to deserialize response." };
    }

    public async Task<AppResponse<List<CheckoffBatchResponse>>> GetBatchesAsync()
    {
        var response = await httpClient.GetAsync("api/checkoff/batches");
        var result = await response.Content.ReadFromJsonAsync<AppResponse<List<CheckoffBatchResponse>>>();
        return result ?? new AppResponse<List<CheckoffBatchResponse>> { IsSuccess = false, Error = AppError.Unexpected(), Message = "Failed to deserialize response." };
    }

    public async Task<AppResponse<CheckoffBatchDetailsResponse>> GetBatchDetailsAsync(Guid batchId)
    {
        var response = await httpClient.GetAsync($"api/checkoff/batches/{batchId}");
        var result = await response.Content.ReadFromJsonAsync<AppResponse<CheckoffBatchDetailsResponse>>();
        return result ?? new AppResponse<CheckoffBatchDetailsResponse> { IsSuccess = false, Error = AppError.Unexpected(), Message = "Failed to deserialize response." };
    }

    public async Task<AppResponse<Guid>> UploadBatchAsync(UploadCheckoffBatchRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("api/checkoff/batches/upload", request);
        var result = await response.Content.ReadFromJsonAsync<AppResponse<Guid>>();
        return result ?? new AppResponse<Guid> { IsSuccess = false, Error = AppError.Unexpected(), Message = "Failed to deserialize response." };
    }

    public async Task<AppResponse<bool>> ValidateBatchAsync(Guid batchId)
    {
        var response = await httpClient.PostAsync($"api/checkoff/batches/{batchId}/validate", null);
        var result = await response.Content.ReadFromJsonAsync<AppResponse<bool>>();
        return result ?? new AppResponse<bool> { IsSuccess = false, Error = AppError.Unexpected(), Message = "Failed to deserialize response." };
    }

    public async Task<AppResponse<bool>> PostBatchAsync(Guid batchId)
    {
        var response = await httpClient.PostAsync($"api/checkoff/batches/{batchId}/post", null);
        var result = await response.Content.ReadFromJsonAsync<AppResponse<bool>>();
        return result ?? new AppResponse<bool> { IsSuccess = false, Error = AppError.Unexpected(), Message = "Failed to deserialize response." };
    }
}
