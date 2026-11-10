using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Shares.Dtos;
using LS.UI.Blazor.Features.Banking.Contracts.Interfaces;
using LS.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Banking.Contracts.Implementations;

internal sealed class SharesService(IBackendApiClient apiClient) : ISharesService
{
    public Task<AppResponse<IEnumerable<ShareProductResponse>>> GetShareProductsAsync(bool includeInactive = false)
    {
        return apiClient.SendAsync<IEnumerable<ShareProductResponse>>(
            HttpMethod.Get,
            $"api/v1/banking/share-products?includeInactive={includeInactive}");
    }

    public Task<AppResponse<Guid>> CreateShareProductAsync(CreateShareProductRequest request)
    {
        return apiClient.SendAsync<Guid>(
            HttpMethod.Post,
            "api/v1/banking/share-products",
            request);
    }

    public Task<AppResponse<IEnumerable<ShareAccountResponse>>> GetMemberShareAccountsAsync(Guid memberId)
    {
        return apiClient.SendAsync<IEnumerable<ShareAccountResponse>>(
            HttpMethod.Get,
            $"api/v1/banking/shares/members/{memberId}");
    }

    public Task<AppResponse<Guid>> PurchaseSharesAsync(PurchaseSharesRequest request)
    {
        return apiClient.SendAsync<Guid>(
            HttpMethod.Post,
            "api/v1/banking/shares/purchase",
            request);
    }

    public Task<AppResponse<Guid>> TransferSharesAsync(TransferSharesRequest request)
    {
        return apiClient.SendAsync<Guid>(
            HttpMethod.Post,
            "api/v1/banking/shares/transfer",
            request);
    }
}
