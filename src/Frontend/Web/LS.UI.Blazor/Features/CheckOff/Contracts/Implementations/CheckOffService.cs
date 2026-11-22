using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.CheckOff.Dtos;
using LS.UI.Blazor.Features.CheckOff.Contracts.Interfaces;
using LS.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;

namespace LS.UI.Blazor.Features.CheckOff.Contracts.Implementations;

internal sealed class CheckOffService(IBackendApiClient apiClient) : ICheckOffService
{
    private const string BaseUrl = "api/checkoff";

    public Task<AppResponse<List<EmployerResponse>>> GetEmployersAsync()
        => apiClient.SendAsync<List<EmployerResponse>>(HttpMethod.Get, $"{BaseUrl}/employers");

    public Task<AppResponse<List<CheckoffBatchResponse>>> GetBatchesAsync()
        => apiClient.SendAsync<List<CheckoffBatchResponse>>(HttpMethod.Get, $"{BaseUrl}/batches");

    public Task<AppResponse<CheckoffBatchDetailsResponse>> GetBatchDetailsAsync(Guid batchId)
        => apiClient.SendAsync<CheckoffBatchDetailsResponse>(HttpMethod.Get, $"{BaseUrl}/batches/{batchId}");

    public Task<AppResponse<Guid>> UploadBatchAsync(UploadCheckoffBatchRequest request)
        => apiClient.SendAsync<Guid>(HttpMethod.Post, $"{BaseUrl}/batches/upload", request);

    public Task<AppResponse<bool>> ValidateBatchAsync(Guid batchId)
        => apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/batches/{batchId}/validate");

    public Task<AppResponse<bool>> PostBatchAsync(Guid batchId)
        => apiClient.SendAsync<bool>(HttpMethod.Post, $"{BaseUrl}/batches/{batchId}/post");
}
