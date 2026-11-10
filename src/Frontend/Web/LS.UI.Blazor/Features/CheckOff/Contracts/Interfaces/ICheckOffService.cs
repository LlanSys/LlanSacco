using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.CheckOff.Dtos;

namespace LS.UI.Blazor.Features.CheckOff.Contracts.Interfaces;

public interface ICheckOffService
{
    Task<AppResponse<List<EmployerResponse>>> GetEmployersAsync();
    Task<AppResponse<List<CheckoffBatchResponse>>> GetBatchesAsync();
    Task<AppResponse<CheckoffBatchDetailsResponse>> GetBatchDetailsAsync(Guid batchId);
    Task<AppResponse<Guid>> UploadBatchAsync(UploadCheckoffBatchRequest request);
    Task<AppResponse<bool>> ValidateBatchAsync(Guid batchId);
    Task<AppResponse<bool>> PostBatchAsync(Guid batchId);
}
