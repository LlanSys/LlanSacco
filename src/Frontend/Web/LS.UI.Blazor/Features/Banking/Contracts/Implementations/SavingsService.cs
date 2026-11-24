using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Features.Banking.Savings.Dtos;
using LS.UI.Blazor.Features.Banking.Contracts.Interfaces;
using LS.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LS.UI.Blazor.Features.Banking.Contracts.Implementations;

internal sealed class SavingsService(IBackendApiClient apiClient) : ISavingsService
{
    public Task<AppResponse<bool>> ConfigureSecurityAsync(Guid productId, ConfigureSavingsSecurityRequest request, CancellationToken ct = default) =>
        apiClient.SendAsync<bool>(HttpMethod.Put, $"api/banking/savings-products/{productId}/security-policy", request, cancellationToken: ct);

    public Task<AppResponse<IEnumerable<SavingsProductResponse>>> GetSavingsProductsAsync(bool includeInactive = false)
    {
        return apiClient.SendAsync<IEnumerable<SavingsProductResponse>>(
            HttpMethod.Get,
            $"api/banking/savings-products?onlyActive={!includeInactive}");
    }

    public Task<AppResponse<SavingsProductResponse>> CreateSavingsProductAsync(CreateSavingsProductRequest request)
    {
        return apiClient.SendAsync<SavingsProductResponse>(
            HttpMethod.Post,
            "api/banking/savings-products",
            request);
    }

    public Task<AppResponse<IEnumerable<SavingsAccountResponse>>> GetMemberSavingsAccountsAsync(Guid memberId)
    {
        return apiClient.SendAsync<IEnumerable<SavingsAccountResponse>>(
            HttpMethod.Get,
            $"api/banking/savings/members/{memberId}");
    }

    public Task<AppResponse<SavingsAccountResponse>> DepositSavingsAsync(DepositSavingsRequest request)
    {
        return apiClient.SendAsync<SavingsAccountResponse>(
            HttpMethod.Post,
            "api/banking/savings/deposit",
            request);
    }

    public Task<AppResponse<SavingsAccountResponse>> WithdrawSavingsAsync(WithdrawSavingsRequest request)
    {
        return apiClient.SendAsync<SavingsAccountResponse>(
            HttpMethod.Post,
            "api/banking/savings/withdraw",
            request);
    }
}
